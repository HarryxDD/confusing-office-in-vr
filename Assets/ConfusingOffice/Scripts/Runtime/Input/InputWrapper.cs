using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && IS_INSTALL_INPUTSYSTEM_PKG
using UnityEngine.InputSystem.UI;
#endif

namespace UnityJapanOffice
{
    public class InputWrapper 
    {


        // InputSytem
#if ENABLE_INPUT_SYSTEM && IS_INSTALL_INPUTSYSTEM_PKG

        public static void InitializeEventSystem(GameObject eventSystemGameObject)
        {
            InputSystemUIInputModule inputModuele = eventSystemGameObject.GetComponent<InputSystemUIInputModule>();
            StandaloneInputModule legacyInputModule = eventSystemGameObject.GetComponent<StandaloneInputModule>();
            // InputSystem
            if (inputModuele == null)
            {
                inputModuele = eventSystemGameObject.AddComponent<InputSystemUIInputModule>();
            }
            if (legacyInputModule)
            {
                Object.Destroy(legacyInputModule);
            }
            // remove pad control from input module
            // want to handle events in this script
            inputModuele.move = null;
            inputModuele.submit = null;
            inputModuele.cancel = null;
        }

        public static Vector2 GetLookDelta()
        {
            return InputSystemProxy.Instance.GetLookDelta();
        }

        public static Vector2 GetMoveVector()
        {
            return InputSystemProxy.Instance.GetMoveVector();
        }


        public static bool IsTriggerToggleSprint()
        {
            return InputSystemProxy.Instance.IsTriggerToggleSprint();
        }
        public static bool IsSprint()
        {
            return InputSystemProxy.Instance.IsSprint();
        }


        public static bool IsTriggerToggleMenuButton()
        {
            return InputSystemProxy.Instance.IsTriggerToggleMenuButton();
        }

        public static bool IsTriggerToggleMenuPointerButton()
        {
            return InputSystemProxy.Instance.IsTriggerToggleMenuPointerButton();
        }

        public static bool IsTriggerCloseGuideUI()
        {
            return InputSystemProxy.Instance.IsTriggerCloseGuideUI();
        }

        public static Vector3 GetMousePosition()
        {
            return InputSystemProxy.Instance.GetMousePosition();
        }



        public static bool IsPressSelectDown()
        {
            return InputSystemProxy.Instance.IsPressSelectDown();
        }
        public static bool IsPressSelectUp()
        {
            return InputSystemProxy.Instance.IsPressSelectUp();
        }

        public static bool IsPressSelectLeft()
        {
            return InputSystemProxy.Instance.IsPressSelectLeft();
        }
        public static bool IsPressSelectRight()
        {
            return InputSystemProxy.Instance.IsPressSelectRight();
        }

        public static bool IsPressCancel()
        {
            return InputSystemProxy.Instance.IsPressCancel();
        }
        public static bool IsPressSubmit()
        {
            return InputSystemProxy.Instance.IsPressSubmit();
        }

#else // Legacy InputManager

        public static void InitializeEventSystem(GameObject eventSystemGameObject)
        {
            StandaloneInputModule legacyInputModule = eventSystemGameObject.GetComponent<StandaloneInputModule>();
            if (!legacyInputModule)
            {
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
            }
        }

        public static Vector2 GetLookDelta()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        public static Vector2 GetMoveVector()
        {
            return new Vector2( Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }


        public static bool IsTriggerToggleSprint()
        {
            return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        }

        public static bool IsSprint()
        {
            return false;
        }

        public static bool IsTriggerToggleMenuButton()
        {
            return false;
        }

        public static bool IsTriggerToggleMenuPointerButton()
        {
            return Input.GetMouseButtonDown(0);
        }

        public static bool IsTriggerCloseGuideUI()
        {
            return Input.GetKeyDown(KeyCode.Return) | Input.GetMouseButtonDown(0);
        }

        public static Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }
        
        public static bool IsPressSelectDown()
        {
            return false;
        }
        public static bool IsPressSelectUp()
        {
            return false;
        }
        
        public static bool IsPressSelectLeft()
        {
            return false;
        }
        public static bool IsPressSelectRight()
        {
            return false;
        }


        public static bool IsPressCancel()
        {
            return false;
        }
        public static bool IsPressSubmit()
        {
            return false;
        }
#endif
    }
}