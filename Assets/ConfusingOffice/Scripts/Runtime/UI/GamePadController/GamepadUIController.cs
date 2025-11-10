using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityJapanOffice
{

    public class GamepadUIController : MonoBehaviour
    {
        [System.Serializable]
        class MenuItem
        {
            [SerializeField]
            public GameObject item;
            [SerializeField]
            public List<GameObject> childItems;

            public bool hasChildMenu()
            {
                return (childItems != null && childItems.Count > 0);
            }
        }

        [SerializeField]
        private List<MenuItem> selectObjects;

        private List<Toggle> togglesWithChildMenu;

        //EventData
        private BaseEventData m_BaseEventData;
        private PointerEventData m_PointerEventData;
        private AxisEventData m_axisEventData;

        private int idx = -1;
        private int childIdx = -1;
        private Vector3 m_prevMousePosition;

        public static GamepadUIController current {  get; private set; }

        public GameObject selectedGameObject
        {
            get
            {
                if (idx < 0 | idx >= selectObjects.Count)
                {
                    return null;
                }
                if(childIdx < 0)
                {
                    return selectObjects[idx].item;
                }
                if(childIdx <= selectObjects[idx].childItems.Count-1 ){
                    return selectObjects[idx].childItems[childIdx];
                }

                return null;
            }
        }

        public void OnStartMenuFromController()
        {
            if(idx < 0)
            {
                this.OnControllerSelectStart();
            }
        }


        private void Awake()
        {
            current = this;
            InputWrapper.InitializeEventSystem(EventSystem.current.gameObject);

            // eventData
            m_BaseEventData = new BaseEventData(EventSystem.current);
            m_PointerEventData = new PointerEventData(EventSystem.current);
            m_axisEventData = new AxisEventData(EventSystem.current);

            // Create frameUI( Indicate pad select)
            CreateSelectFrameObject();

            // collect toggle items
            togglesWithChildMenu = new List<Toggle>();
            foreach(var target in  selectObjects)
            {
                if(target.childItems == null || target.childItems.Count==0)
                {
                    continue;
                }
                var toggle = target.item.GetComponent<Toggle>();
                if (toggle)
                {
                    togglesWithChildMenu.Add(toggle);
                }
            }
        }

        // Create frameUI( Indicate pad select)
        private void CreateSelectFrameObject()
        {
            var guide = new GameObject("SelectFrame");
            guide.transform.parent = selectObjects[0].item.transform.parent;
            guide.AddComponent<RectTransform>();
            guide.AddComponent<ControllerSelectFrameImage>();
            guide.AddComponent<ControllerSelectUI>();
            guide.AddComponent<Image>();
        }

        private void OnDestroy()
        {
            current = null;
        }

        // Update is called once per frame
        private void Update()
        {
            if (this.IsSelectingChildMenu())
            {
                this.UpdateChildMenu();
            }
            else
            {
                this.UpdateMenu();
            }

            // Reset if mouse move
            var mousePosition = InputWrapper.GetMousePosition();
            if ((mousePosition - m_prevMousePosition).sqrMagnitude >= 9.0f)
            {
                ResetSelect();
                m_prevMousePosition = mousePosition;
            }
        }

        // Update of the first-level menu 
        private void UpdateMenu() { 
            if (InputWrapper.IsPressSelectDown())
            {
                if (idx < selectObjects.Count - 1)
                {
                    ++idx;
                    if(idx == 0)
                    {
                        OnControllerSelectStart();
                    }
                }
                EventSystem.current.SetSelectedGameObject(selectObjects[idx].item);
            }
            if (InputWrapper.IsPressSelectUp())
            {
                if (idx > 0)
                {
                    --idx;
                }else if(idx < 0)
                {
                    idx = 0;
                    OnControllerSelectStart();
                }
                EventSystem.current.SetSelectedGameObject(selectObjects[idx].item);
            }
            if (InputWrapper.IsPressSubmit())
            {
                if (idx >= 0) {

                    m_BaseEventData.Reset();
                    EventSystem.current.SetSelectedGameObject(selectObjects[idx].item);
                    ExecuteEvents.Execute(selectObjects[idx].item, m_BaseEventData, ExecuteEvents.submitHandler);

                    if (selectObjects[idx].hasChildMenu())
                    {
                        childIdx = 0;
                        SelectChildMenu(idx, childIdx, -1);
                    }
                }
            }
            //Debug.Log("idx * " + idx + " child:" + childIdx);
        }

        // Update of the second-level menu
        private void UpdateChildMenu()
        {
            var currentChildMenus = selectObjects[this.idx].childItems;
            int oldChildIdx = childIdx;

            //Debug.Log("UpdateChildMenu:" +childIdx +"/"+ currentChildMenus.Count +"::" + currentChildMenus[childIdx].name);

            if (InputWrapper.IsPressSelectDown())
            {                
                if (childIdx < currentChildMenus.Count - 1)
                {
                    ++childIdx;
                }
                SelectChildMenu(idx,childIdx, oldChildIdx);
            }
            if (InputWrapper.IsPressSelectUp())
            {
                if (childIdx > 0)
                {
                    --childIdx;
                }
                else if (childIdx < 0)
                {
                    childIdx = 0;
                }
                SelectChildMenu(idx,childIdx, oldChildIdx);
            }
            if (InputWrapper.IsPressCancel())
            {
                EventSystem.current.SetSelectedGameObject(selectObjects[idx].item);
                m_BaseEventData.Reset();
                ExecuteEvents.Execute(selectObjects[idx].item, m_BaseEventData, ExecuteEvents.submitHandler);
                childIdx = -1;
            }

            if (InputWrapper.IsPressSubmit())
            {
                EventSystem.current.SetSelectedGameObject(selectObjects[idx].childItems[childIdx]);
                m_BaseEventData.Reset();
                ExecuteEvents.Execute(selectObjects[idx].childItems[childIdx], m_BaseEventData, ExecuteEvents.submitHandler);
            }

            bool pressLeft = InputWrapper.IsPressSelectLeft();
            bool pressRight = InputWrapper.IsPressSelectRight();
            if (pressLeft || pressRight)
            {
                m_axisEventData.Reset();
                if (pressRight)
                {
                    m_axisEventData.moveDir = MoveDirection.Right;
                }
                if (pressLeft) 
                {
                    m_axisEventData.moveDir = MoveDirection.Left;
                }
                ExecuteEvents.Execute(selectObjects[idx].childItems[childIdx], m_axisEventData, ExecuteEvents.moveHandler);
            }
        }

        private void SelectChildMenu(int currentIdx,int currentChildIdx,int oldChildIdx)
        {
            if(oldChildIdx >= 0)
            {
                m_BaseEventData.Reset();
                ExecuteEvents.Execute(selectObjects[currentIdx].childItems[oldChildIdx], m_PointerEventData, ExecuteEvents.pointerExitHandler);
            }
            m_PointerEventData.Reset();
            ExecuteEvents.Execute(selectObjects[currentIdx].childItems[currentChildIdx], m_PointerEventData, ExecuteEvents.pointerEnterHandler);

            EventSystem.current.SetSelectedGameObject(selectObjects[currentIdx].childItems[currentChildIdx]);

        }

        private bool IsSelectingChildMenu()
        {
            return (this.childIdx >= 0);
        }

        private void OnControllerSelectStart()
        {
            this.idx = 0;
            this.childIdx = -1;
            foreach(var toggle in togglesWithChildMenu)
            {
                if(toggle.isOn ){
                    toggle.isOn = false;
                }
            }
        }

        private void ResetSelect()
        {
            this.idx = -1;
            this.childIdx = -1;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}