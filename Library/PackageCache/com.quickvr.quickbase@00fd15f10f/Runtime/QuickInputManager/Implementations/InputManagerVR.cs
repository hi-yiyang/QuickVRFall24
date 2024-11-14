using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Controls;

namespace QuickVR
{

    public static class InputManagerVRUtils
    {

        public static bool GetXRButtonValue(this XRController controller, string xrButtonPath)
        {
            bool result = false;

            ButtonControl bControl = controller.TryGetChildControl<ButtonControl>(xrButtonPath);
            if (bControl != null)
            {
                result = bControl.isPressed;
            }

            return result;
        }

        public static float GetXRAxisValue(this XRController controller, string xrAxisPath)
        {
            float result = 0;

            AxisControl aControl = controller.TryGetChildControl<AxisControl>(xrAxisPath);
            if (aControl != null)
            {
                result = aControl.ReadValue();
            }

            return result;
        }

        public static string GetThumbAxisName(this XRController controller)
        {
            if (controller.name.ToLower().Contains("htc")) return "trackpad";

            return "thumbstick";
        }

    }

    public class InputManagerVR : InputManagerGeneric<XRController, InputManagerVR.AxisCode, InputManagerVR.ButtonCodes>
    {

        #region PUBLIC ATTRIBUTES

        public enum AxisCode
        {
            //Left Controller
            LeftStick_Horizontal,
            LeftStick_Vertical,

            LeftTrigger,

            LeftGrip,

            //Right Controller
            RightStick_Horizontal,
            RightStick_Vertical,

            RightTrigger,

            RightGrip,
        }

        public enum ButtonCodes
        {
            //Left Controller
            LeftPrimaryPress,
            LeftPrimaryTouch,

            LeftSecondaryPress,
            LeftSecondaryTouch,

            LeftStickPress,
            LeftStickTouch,

            LeftTriggerPress,
            LeftTriggerTouch,

            LeftGripPress,
            LeftGripTouch,

            //Right Controller
            RightPrimaryPress,
            RightPrimaryTouch,

            RightSecondaryPress,
            RightSecondaryTouch,

            RightStickPress,
            RightStickTouch,

            RightTriggerPress,
            RightTriggerTouch,

            RightGripPress,
            RightGripTouch,
        }

        public float _deadZoneThumbStick = 0.2f;
        public float _deadZoneIndexTrigger = 0.1f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected static List<ButtonCodes> _buttons = new List<ButtonCodes>();
        protected static Dictionary<ButtonCodes, InputManager.VirtualButtonState> _buttonState = new Dictionary<ButtonCodes, InputManager.VirtualButtonState>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            _buttons = QuickUtils.GetEnumValues<ButtonCodes>();
            foreach (ButtonCodes bCode in _buttons)
            {
                _buttonState[bCode] = InputManager.VirtualButtonState.Idle;
            }

            base.Awake();
        }

        //protected virtual IEnumerator Start()
        //{
        //    while (XRController.leftHand == null)
        //    {
        //        yield return null;
        //    }

        //    XRController controller = XRController.leftHand;
        //    ReadOnlyArray<InputControl> controls = controller.allControls;
        //    foreach (InputControl c in controls)
        //    {
        //        Debug.Log(c.path);
        //    }
        //}

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, AxisCode.RightStick_Horizontal.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, AxisCode.LeftStick_Vertical.ToString());

            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCodes.RightTriggerPress.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, ButtonCodes.LeftSecondaryPress.ToString());
        }

        #endregion

        #region GET AND SET

        protected override XRController GetInputDevice()
        {
            return null;
        }

        protected override void SetInputDevice(AxisCode axis)
        {
            _inputDevice = (axis <= AxisCode.LeftGrip) ? XRController.leftHand : XRController.rightHand;
        }

        protected override void SetInputDevice(ButtonCodes button)
        {
            _inputDevice = (button <= ButtonCodes.LeftGripTouch) ? XRController.leftHand : XRController.rightHand;
        }

        protected override float ImpGetAxis(AxisCode axis)
        {
            return GetAxis(axis);
        }

        protected override bool ImpGetButton(ButtonCodes button)
        {
            return GetKey(button);
        }

        private static XRController GetXRController(AxisCode axis)
        {
            return axis <= AxisCode.LeftGrip ? XRController.leftHand : XRController.rightHand;
        }

        private static XRController GetXRController(ButtonCodes key)
        {
            return key <= ButtonCodes.LeftGripTouch ? XRController.leftHand : XRController.rightHand;
        }

        public static float GetAxis(AxisCode axis)
        {
            XRController controller = GetXRController(axis);

            float result = 0;

            if (controller != null && controller.IsTracked())
            {
                if (axis == AxisCode.LeftStick_Horizontal || axis == AxisCode.RightStick_Horizontal)
                {
                    result = controller.GetXRAxisValue(controller.GetThumbAxisName() + "/x");
                }
                else if (axis == AxisCode.LeftStick_Vertical || axis == AxisCode.RightStick_Vertical)
                {
                    result = controller.GetXRAxisValue(controller.GetThumbAxisName() + "/y");
                }
                else if (axis == AxisCode.LeftTrigger || axis == AxisCode.RightTrigger)
                {
                    result = controller.GetXRAxisValue("trigger");
                }
                else if (axis == AxisCode.LeftGrip || axis == AxisCode.RightGrip)
                {
                    result = controller.GetXRAxisValue("grip");
                }
            }

            return result;
        }

        public static bool GetKeyDown(ButtonCodes key)
        {
            return _buttonState[key] == InputManager.VirtualButtonState.Triggered;
        }

        public static bool GetKey(ButtonCodes button)
        {
            bool result = false;
            XRController controller = GetXRController(button);

            if (controller != null)
            {
                if (button == ButtonCodes.LeftPrimaryPress || button == ButtonCodes.RightPrimaryPress)
                {
                    result = controller.GetXRButtonValue("primarybutton");
                }
                else if (button == ButtonCodes.LeftPrimaryTouch || button == ButtonCodes.RightPrimaryTouch)
                {
                    result = controller.GetXRButtonValue("primarytouched");
                }
                else if (button == ButtonCodes.LeftSecondaryPress || button == ButtonCodes.RightSecondaryPress)
                {
                    result = controller.GetXRButtonValue("secondarybutton");
                }
                else if (button == ButtonCodes.LeftSecondaryTouch || button == ButtonCodes.RightSecondaryTouch)
                {
                    result = controller.GetXRButtonValue("secondarytouched");
                }
                else if (button == ButtonCodes.LeftStickPress || button == ButtonCodes.RightStickPress)
                {
                    result = controller.GetXRButtonValue("thumbstickclicked");
                }
                else if (button == ButtonCodes.LeftStickTouch || button == ButtonCodes.RightStickTouch)
                {
                    result = controller.GetXRButtonValue("thumbsticktouched");
                }
                else if (button == ButtonCodes.LeftTriggerPress || button == ButtonCodes.RightTriggerPress)
                {
                    result = controller.GetXRButtonValue("triggerpressed");
                }
                else if (button == ButtonCodes.LeftTriggerTouch || button == ButtonCodes.RightTriggerTouch)
                {
                    result = controller.GetXRButtonValue("triggertouched");
                }
                else if (button == ButtonCodes.LeftGripPress || button == ButtonCodes.RightGripPress)
                {
                    result = controller.GetXRButtonValue("grippressed");
                }
                else if (button == ButtonCodes.LeftGripTouch || button == ButtonCodes.RightGripTouch)
                {

                }
            }

            return result;
        }

        public static bool GetKeyUp(ButtonCodes key)
        {
            return _buttonState[key] == InputManager.VirtualButtonState.Released;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            foreach (ButtonCodes bCode in _buttons)
            {
                _buttonState[bCode] = InputManager.GetNextState(_buttonState[bCode], GetKey(bCode));
            }
        }

        #endregion

    }

}

