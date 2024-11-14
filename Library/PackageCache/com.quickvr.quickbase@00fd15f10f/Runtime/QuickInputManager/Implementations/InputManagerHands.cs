using UnityEngine;

namespace QuickVR
{
    public class InputManagerHands : BaseInputManager<BaseInputManager.DefaultCode, InputManagerHands.ButtonCode>
    {

        #region PUBLIC ATTRIBUTES

        public enum ButtonCode
        {
            //LeftHand
            LeftIndexPinch,
            LeftMiddlePinch,
            LeftRingPinch,
            LeftLittlePinch,
            LeftThumbUp,
            LeftThumbDown,

            //RightHand
            RightIndexPinch,
            RightMiddlePinch,
            RightRingPinch,
            RightLittlePinch,
            RightThumbUp,
            RightThumbDown,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickUnityVR _unityVR = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCode.RightThumbUp.ToString(), ButtonCode.LeftThumbUp.ToString());
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnSourceAnimatorSet += ActionSourceAnimatorSet;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnSourceAnimatorSet -= ActionSourceAnimatorSet;
        }

        #endregion

        #region GET AND SET

        protected virtual void ActionSourceAnimatorSet(Animator animator)
        {
            _unityVR = animator.GetComponent<QuickUnityVR>();
        }

        protected static QuickVRHand GetVRHand(ButtonCode bCode)
        {
            return QuickVRHandTrackingManager.GetVRHand(bCode.ToString().Contains("Left"));
        }

        protected override float ImpGetAxis(string axis)
        {
            return 0.0f;
        }

        protected override bool ImpGetButton(ButtonCode button)
        {
            return GetKey(button);
        }

        public static bool GetKey(ButtonCode button)
        {
            if (QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Hands)
            {
                QuickVRHand h = GetVRHand(button);
                if (h != null && h._isTracked)
                {
                    //Pinching gestures
                    if (button == ButtonCode.LeftIndexPinch || button == ButtonCode.RightIndexPinch) return h.IsGesturePinchIndex();
                    if (button == ButtonCode.LeftMiddlePinch || button == ButtonCode.RightMiddlePinch) return h.IsGesturePinchMiddle();
                    if (button == ButtonCode.LeftRingPinch || button == ButtonCode.RightRingPinch) return h.IsGesturePinchRing();
                    if (button == ButtonCode.LeftLittlePinch || button == ButtonCode.RightLittlePinch) return h.IsGesturePinchLittle();

                    //Thumb gestures
                    if (button == ButtonCode.LeftThumbUp || button == ButtonCode.RightThumbUp) return GetVRHand(button).IsGestureThumbUp();
                    if (button == ButtonCode.LeftThumbDown || button == ButtonCode.RightThumbDown) return GetVRHand(button).IsGestureThumbDown();
                }
            }

            return false;
        }

        protected override float ImpGetAxis(BaseInputManager.DefaultCode axis)
        {
            return 0;
        }

        #endregion

    }

}
