using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace QuickVR
{
    
    public class QuickVRNodeEye : QuickVRNode
    {

        #region PUBLIC ATTRIBUTES

        protected bool _isLeft
        {
            get
            {
                return _role == QuickHumanBodyBones.LeftEye;
            }
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _blinkFactor = 0;

        #endregion

        #region GET AND SET

        public virtual float GetBlinkFactor()
        {
            return _blinkFactor;
        }

        #endregion

        #region UPDATE

        //protected override TrackedDevice CheckDevice()
        //{
        //    return InputSystem.GetDevice<XRHMD>();
        //}

        //protected override void UpdateTracking()
        //{
        //    base.UpdateTracking();

        //    //Compute eyeball rotation
        //    //if (_inputDevice.TryGetFeatureValue(QuickVRUsages.combineEyeVector, out Vector3 vEye))
        //    //{
        //    //    Vector3 r = transform.TransformDirection(vEye);
        //    //    Vector3 rotAxis = Vector3.Cross(transform.forward, r);
        //    //    float rotAngle = Vector3.Angle(transform.forward, r);
        //    //    transform.Rotate(rotAxis, rotAngle, Space.World);
        //    //}

        //    ////Compute blinkFactor
        //    //if (_inputDevice.TryGetFeatureValue(_isLeft ? QuickVRUsages.leftEyeOpenness : QuickVRUsages.rightEyeOpenness, out float eOpen))
        //    //{
        //    //    _blinkFactor = 1.0f - eOpen;
        //    //}
        //}
        
        #endregion

    }

}


