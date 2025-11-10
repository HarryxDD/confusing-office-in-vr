#if ENABLE_INPUT_SYSTEM && IS_INSTALL_INPUTSYSTEM_PKG
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityJapanOffice
{
    public class InputSystemProxy : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset asset;

        private DefaultInputActions defaultActions;
        private InputAction mouseDeltaAction;
        private InputAction moveAction;
        private InputAction sprintToggleAction;
        private InputAction sprintHoldAction;
        // UI
        private InputAction uiPointAction;
        private InputAction uiClickAction;
        // OfficeUI
        private InputAction officeUISubmit;
        private InputAction officeUICancel;
        private InputAction officeUIDown;
        private InputAction officeUIUp;
        private InputAction officeUILeft;
        private InputAction officeUIRight;
        private InputAction officeUIToggleMenu;

        public static InputSystemProxy Instance { get;private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            if (InputSystemProxy.Instance)
            {
                return;
            }
            var gmo = new GameObject("InputSystemProxy", typeof(InputSystemProxy));
            GameObject.DontDestroyOnLoad(gmo);
        }

        private static void UpdateInputActions(InputActionAsset inputActionAsset)
        {
            var playerActionMap = inputActionAsset.FindActionMap("Player");
            // Pointer scale
            var lookAction = playerActionMap.FindAction("Look");
            var lookActionBindings = lookAction.bindings;
            for (int i = 0; i < lookActionBindings.Count; i++)
            {
                var bind = lookActionBindings[i];
                if(bind.effectivePath == "<Pointer>/delta")
                {
                    bind.overrideProcessors = "ScaleVector2(x=0.05,y=0.05)";

                    lookAction.ApplyBindingOverride(bind);
                }
            }
            //Sprint Toggle
            InputAction sprintToggle = playerActionMap.AddAction("SprintToggle", InputActionType.Button);
            var leftShiftKeyBind = new InputBinding("<Keyboard>/leftShift", null, ";Keyboard&Mouse");
            var rightShiftKeyBind = new InputBinding("<Keyboard>/rightShift", null, ";Keyboard&Mouse");
            var leftSholderBind = new InputBinding("<Gamepad>/leftShoulder", null, ";Gamepad");
            sprintToggle.AddBinding(leftShiftKeyBind);
            sprintToggle.AddBinding(rightShiftKeyBind);
            sprintToggle.AddBinding(leftSholderBind);

            // Sprint
            InputAction sprint = playerActionMap.AddAction("Sprint", InputActionType.Button);
            var leftStickButton = new InputBinding("<Gamepad>/leftStickPress",null, ";Gamepad");
            sprint.AddBinding(leftStickButton);

        }

        public static void AddOfficeUIActionMap(InputActionAsset inputActionAsset)
        {
            const string actionMapName = "OfficeUI";
            if(inputActionAsset.FindActionMap(actionMapName) != null){
                return;
            }
            var officeUIMap = inputActionAsset.AddActionMap(actionMapName);

            var downAction = officeUIMap.AddAction("SelectDown");
            var downDpad = new InputBinding("<Gamepad>/dpad/down",null,";Gamepad");
            var downLeftStick = new InputBinding("<Gamepad>/leftStick/down",null,";Gamepad", "AxisDeadzone(min=0.6,max=1)");
            downAction.AddBinding(downDpad);
            downAction.AddBinding(downLeftStick);

            var upAction = officeUIMap.AddAction("SelectUp");
            var upDpad = new InputBinding("<Gamepad>/dpad/up", null, ";Gamepad");
            var upLeftStick = new InputBinding("<Gamepad>/leftStick/up", null, ";Gamepad", "AxisDeadzone(min=0.6,max=1)");
            upAction.AddBinding(upDpad);
            upAction.AddBinding(upLeftStick);

            
            var leftAction = officeUIMap.AddAction("SelectLeft");
            var leftDpad = new InputBinding("<Gamepad>/dpad/left",null,";Gamepad");
            var leftLeftStick = new InputBinding("<Gamepad>/leftStick/left",null,";Gamepad", "AxisDeadzone(min=0.6,max=1)");
            leftAction.AddBinding(leftDpad);
            leftAction.AddBinding(leftLeftStick);

            var rightAction = officeUIMap.AddAction("SelectRight");
            var rightDpad = new InputBinding("<Gamepad>/dpad/right", null, ";Gamepad");
            var rightLeftStick = new InputBinding("<Gamepad>/leftStick/right", null, ";Gamepad", "AxisDeadzone(min=0.6,max=1)");
            rightAction.AddBinding(rightDpad);
            rightAction.AddBinding(rightLeftStick);

            var submitAction = officeUIMap.AddAction("Submit");
            var padSouthButon = new InputBinding("<Gamepad>/buttonSouth", null, ";Gamepad");
            submitAction.AddBinding(padSouthButon);

            var cancelAction = officeUIMap.AddAction("Cancel");
            var padEastButon = new InputBinding("<Gamepad>/buttonEast", null, ";Gamepad");
            cancelAction.AddBinding(padEastButon);
            var closeMenuAction = officeUIMap.AddAction("ToggleMenu");
            var padStartButon = new InputBinding("<Gamepad>/start", null, ";Gamepad");
            closeMenuAction.AddBinding(padStartButon);
        }

        private void Awake()
        {
            if (Instance)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }
            Instance = this;
            if (!asset) {
                defaultActions = new DefaultInputActions();
                asset = defaultActions.asset;
                UpdateInputActions(asset);
                AddOfficeUIActionMap(asset);
            }
            asset.Enable();

            this.mouseDeltaAction = asset.FindAction("Player/Look");
            this.moveAction = asset.FindAction("Player/Move");
            this.sprintToggleAction = asset.FindAction("Player/SprintToggle");
            this.sprintHoldAction = asset.FindAction("Player/Sprint");
            this.uiPointAction = asset.FindAction("UI/Point");
            this.uiClickAction = asset.FindAction("UI/Click");

            this.officeUISubmit = asset.FindAction("OfficeUI/Submit");
            this.officeUICancel = asset.FindAction("OfficeUI/Cancel");
            this.officeUIDown = asset.FindAction("OfficeUI/SelectDown");
            this.officeUIUp = asset.FindAction("OfficeUI/SelectUp");
            this.officeUILeft = asset.FindAction("OfficeUI/SelectLeft");
            this.officeUIRight = asset.FindAction("OfficeUI/SelectRight");

            this.officeUIToggleMenu = asset.FindAction("OfficeUI/ToggleMenu");

        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public Vector2 GetLookDelta()
        {
            var val = this.mouseDeltaAction.ReadValue<Vector2>();
            return val;
        }

        public Vector2 GetMoveVector()
        {
            var val = this.moveAction.ReadValue<Vector2>();
            return val;
        }

        public bool IsTriggerToggleSprint()
        {
            return this.sprintToggleAction.WasPressedThisFrame();
        }

        public bool IsSprint()
        {
            return (this.sprintHoldAction.ReadValue<float>() > 0.1f);
        }

        // todo
        public bool IsTriggerToggleMenuButton()
        {
            return this.officeUIToggleMenu.WasPressedThisFrame();
        }

        public bool IsTriggerToggleMenuPointerButton()
        {
            return this.uiClickAction.WasPressedThisFrame();
        }

        public bool IsTriggerCloseGuideUI()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.isPressed) {
                return true;
            }

            if(Gamepad.current != null &&
                (Gamepad.current.startButton.isPressed ||
                Gamepad.current.leftShoulder.isPressed ||
                Gamepad.current.buttonSouth.isPressed ||
                Gamepad.current.buttonEast.isPressed ) )                
            {
                return true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                return true;
            }
            return false;
        }

        public Vector3 GetMousePosition()
        {
            var position = this.uiPointAction.ReadValue<Vector2>();

            return new Vector3(position.x, position.y, 0);
        }

        public bool IsPressSelectLeft()
        {
            return this.officeUILeft.WasPressedThisFrame();
        }
        public bool IsPressSelectRight()
        {
            return this.officeUIRight.WasPressedThisFrame();
        }

        public bool IsPressSelectDown()
        {
            return this.officeUIDown.WasPressedThisFrame();
        }
        public bool IsPressSelectUp()
        {
            return this.officeUIUp.WasPressedThisFrame();
        }
        public bool IsPressCancel()
        {
            return this.officeUICancel.WasPressedThisFrame();
        }
        public bool IsPressSubmit()
        {
            return this.officeUISubmit.WasPressedThisFrame();
        }
    }
}
#endif