using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using UnityEngine.InputSystem;

namespace QuickVR
{

    public class QuickVRNodeHead : QuickVRNode
    {

        #region CONSTANTS

        //Rotation limits for CameraMono
        const float MAX_HORIZONTAL_ANGLE = 80;
        const float MAX_VERTICAL_ANGLE = 45;

        #endregion

        #region PROTECTED ATTRIBUTES

        //Rotation attributes for CameraMono
        protected static float _speedH = 2.0f;
        protected static float _speedV = 2.0f;
        protected static float _offsetH = 0;
        protected static float _offsetV = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            QuickVRManager.OnPreCalibrate += ActionOnPreCalibrate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            QuickVRManager.OnPreCalibrate -= ActionOnPreCalibrate;
        }

        private void ActionOnPreCalibrate()
        {
            _offsetH = 0;
            _offsetV = 0;

            Animator animatorSrc = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource();
            if (animatorSrc != null ) 
            {
                Transform tBone = animatorSrc.GetBoneTransformNormalized(HumanBodyBones.Head);
                Vector3 headOffset = tBone.position - animatorSrc.GetEyeCenterVR().position;
                GetTrackedObject().transform.localPosition = tBone.InverseTransformVector(headOffset);
            }
        }

        #endregion

        #region UPDATE

        public override void UpdateState()
        {
            base.UpdateState();

            if (!_isTracked && QuickVRManager._instance.IsCalibrated())
            {
                if (!InputManagerKeyboard.GetKey(Key.LeftCtrl))
                {
                    float x = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
                    float y = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
                    _offsetH += _speedH * x;
                    _offsetV -= _speedV * y;

                    _offsetH = Mathf.Clamp(_offsetH, -MAX_HORIZONTAL_ANGLE, MAX_HORIZONTAL_ANGLE);
                    _offsetV = Mathf.Clamp(_offsetV, -MAX_VERTICAL_ANGLE, MAX_VERTICAL_ANGLE);

                    //UpdateTrackedRotation(Quaternion.Euler(_offsetV, _offsetH, 0));
                    //transform.localRotation = Quaternion.identity;
                }

                Vector3 pivotPos = _trackedObject.transform.position;
                transform.RotateAround(pivotPos, QuickXRRig._instance.transform.up, _offsetH);
                transform.RotateAround(pivotPos, transform.right, _offsetV);

                _state = State.Inferred;
            }
        }

        #endregion

    }

}
