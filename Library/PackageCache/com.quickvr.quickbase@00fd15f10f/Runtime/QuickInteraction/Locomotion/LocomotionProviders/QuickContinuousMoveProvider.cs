using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System;

namespace QuickVR.Interaction.Locomotion
{

    public class QuickContinuousMoveProvider : ContinuousMoveProviderBase, IQuickLocomotionProvider
    {

        #region PROTECTED ATTRIBUTES

        protected List<Tuple<Vector2, float>> _inputData = new List<Tuple<Vector2, float>>();

        protected Vector2 _currentMoveDir = Vector2.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            forwardSource = QuickSingletonManager.GetInstance<QuickXRRig>().transform;
        }

        #endregion

        #region GET AND SET

        public virtual void AddInputData(Vector2 moveDir, float speed)
        {
            _inputData.Add(new Tuple<Vector2, float>(moveDir, speed));
        }

        protected override Vector2 ReadInput()
        {
            return _currentMoveDir;
            
            //Vector2 result = Vector2.zero;
            //if (_hasCustomInput)
            //{
            //    result = _customMoveDir;
            //}
            //else
            //{
            //    if (QuickVRInteractionManager._instance._locomotionContinuousMoveEnabled)
            //    {
            //        float dX = InputManagerVR.GetAxis(InputManagerVR.AxisCode.LeftStick_Horizontal);
            //        float dY = InputManagerVR.GetAxis(InputManagerVR.AxisCode.LeftStick_Vertical);

            //        result = new Vector2(dX, dY);
            //    }
            //}

            //return result;
        }

        #endregion

        #region UPDATE

        public void UpdateLocomotion()
        {
            foreach (var inputData in  _inputData)
            {
                _currentMoveDir = inputData.Item1;
                moveSpeed = inputData.Item2;
                Update();
            }

            _inputData.Clear();
        }

        #endregion

    }

}


