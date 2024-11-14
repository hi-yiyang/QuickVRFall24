using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR.Interaction.Locomotion
{

    public class QuickContinuousTurnProvider : ContinuousTurnProviderBase, IQuickLocomotionProvider
    {
        #region PROTECTED ATTRIBUTES

        protected float _customTurnAngle { get; set; }
        protected bool _hasCustomInput = false;

        #endregion

        #region UPDATE

        protected override Vector2 ReadInput()
        {
            Vector2 result = Vector2.zero;

            if (_hasCustomInput)
            {
                result = new Vector2(_customTurnAngle, 0);
            }
            else
            {
                if (QuickVRInteractionManager._instance._locomotionContinuousTurnEnabled)
                {
                    result = new Vector2(InputManagerVR.GetAxis(InputManagerVR.AxisCode.RightStick_Horizontal), 0);
                }
            }
            
            return result;
        }

        public virtual void UpdateLocomotion(float turnAngle)
        {
            _customTurnAngle = turnAngle;
            _hasCustomInput = true;

            UpdateLocomotion();

            _hasCustomInput = false;
        }

        public void UpdateLocomotion()
        {
            Update();
        }

        #endregion

    }

}


