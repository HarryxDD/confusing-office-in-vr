using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;


namespace UnityJapanOffice
{
    [RequireComponent(typeof(ControllerSelectFrameImage))]
    public class ControllerSelectUI : MonoBehaviour
    {
        private ControllerSelectFrameImage frameImageCache;
        private RectTransform rectTransformCache;

        public GameObject targetGameObject;
        private void Awake()
        {
            this.frameImageCache = GetComponent<ControllerSelectFrameImage>();
            this.rectTransformCache = GetComponent<RectTransform>();

        }
        // Update is called once per frame
        void Update()
        {
            targetGameObject = GamepadUIController.current.selectedGameObject;
            if (targetGameObject == null)
            {
                frameImageCache.Visible = false;
                return;
            }
            var target = targetGameObject.GetComponent<RectTransform>();
            if(target == null)
            {
                frameImageCache.Visible = false;
                return;
            }
            frameImageCache.Visible = true;
            rectTransformCache.SetParent( target.parent );
            rectTransformCache.pivot = target.pivot;
            rectTransformCache.anchorMin = target.anchorMin;
            rectTransformCache.anchorMax = target.anchorMax;

            rectTransformCache.position = target.position;
            rectTransformCache.sizeDelta = target.sizeDelta;
            rectTransformCache.localScale = target.localScale;
        }
    }
}