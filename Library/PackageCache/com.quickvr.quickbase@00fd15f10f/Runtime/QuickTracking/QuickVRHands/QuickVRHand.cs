using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Hands;

namespace QuickVR
{

    public class QuickVRHand
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeft
        {
            get; protected set;
        }

        public bool _isTracked
        {
            get
            {
                return _handSubsystem.running && _xrHand.isTracked;
            }
            
        }
        
        public XRHand _xrHand
        {
            get
            {
                return _isLeft ? _handSubsystem.leftHand : _handSubsystem.rightHand;
            }
        }

        public MetaAimHand _metaHand
        {
            get
            {
                return _isLeft ? MetaAimHand.left : MetaAimHand.right;
            }
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected XRHandSubsystem _handSubsystem
        {
            get
            {
                return QuickVRHandTrackingManager._handSubsystem;
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickVRHand(bool isLeft)
        {
            _isLeft = isLeft;
        }

        #endregion

        #region GET AND SET

        public virtual bool IsGesturePinchIndex()
        {
            return _metaHand != null && _metaHand.indexPressed.isPressed;
        }

        public virtual bool IsGesturePinchMiddle()
        {
            return _metaHand != null && _metaHand.middlePressed.isPressed;
        }
        public virtual bool IsGesturePinchRing()
        {
            return _metaHand != null && _metaHand.ringPressed.isPressed;
        }

        public virtual bool IsGesturePinchLittle()
        {
            return _metaHand != null && _metaHand.littlePressed.isPressed;
        }

        public virtual bool IsGesturePoke()
        {
            bool result = false;

            if (_handSubsystem.running)
            {
                //bool wasPoking = _isGesturePoke;
                result =
                    IsIndexExtended() &&
                    IsFingerGrabbing(XRHandFingerID.Middle) &&
                    IsFingerGrabbing(XRHandFingerID.Ring) &&
                    IsFingerGrabbing(XRHandFingerID.Little);

                //if (_isGesturePoke && !wasPoking)
                //{
                //    StartPokeGesture();
                //}
                //else if (!_isGesturePoke && wasPoking)
                //{
                //    EndPokeGesture();
                //}
            }

            return result;
        }

        public virtual bool IsGestureThumbUp()
        {
            return _handSubsystem.running? IsGestureThumb(true) : false;
        }

        public virtual bool IsGestureThumbDown()
        {
            return _handSubsystem.running? IsGestureThumb(false) : false;
        }

        protected virtual bool IsGestureThumb(bool isUp)
        {
            bool result = false;

            //Check if the index, middle, ring and little fingers are closed. 
            if (
                IsFingerGrabbing(XRHandFingerID.Index) &&
                IsFingerGrabbing(XRHandFingerID.Middle) && 
                IsFingerGrabbing(XRHandFingerID.Ring) &&
                IsFingerGrabbing(XRHandFingerID.Little))
            {
                _xrHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose);
                _xrHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose tipPose);
                _xrHand.GetJoint(XRHandJointID.ThumbProximal).TryGetPose(out Pose proxPose);

                //Check the angle between the thumb and the forward of the hand
                Vector3 localUp = (_isLeft? 1 : -1) * wristPose.up;
                Vector3 v = Vector3.ProjectOnPlane(tipPose.position - proxPose.position, localUp);
                Vector3 w = Vector3.ProjectOnPlane(wristPose.forward, localUp);
                float angleFwd = Vector3.SignedAngle(w, v, localUp);

                if (angleFwd > 30)
                {
                    //Check the angle of the thumb with the up vector
                    Vector3 globalUp = isUp ? Vector3.up : Vector3.down;

                    float d = Vector3.Dot(v, globalUp);

                    result = (d > 0) && (Vector3.Angle(v, globalUp) < 45.0f);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the given hand's index finger tip is farther from the wrist than the index intermediate joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's index finger tip is farther from the wrist than the index intermediate joint, false otherwise.</returns>
        protected virtual bool IsIndexExtended()
        {
            if (!(_xrHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  _xrHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var tipPose) &&
                  _xrHand.GetJoint(XRHandJointID.IndexIntermediate).TryGetPose(out var intermediatePose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToIntermediate = intermediatePose.position - wristPose.position;
            return wristToTip.sqrMagnitude > wristToIntermediate.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's finger tip is closer to the wrist than the proximal joint.
        /// </summary>
        /// <param name="fingerID">The finger to check for the required pose.</param>
        /// <returns>True if the given hand's middle finger tip is closer to the wrist than the middle proximal joint, false otherwise.</returns>
        protected virtual bool IsFingerGrabbing(XRHandFingerID fingerID)
        {
            XRHandJointID proxID;
            if (fingerID == XRHandFingerID.Thumb)
            {
                proxID = XRHandJointID.ThumbProximal;
            }
            else if (fingerID == XRHandFingerID.Index)
            {
                proxID = XRHandJointID.IndexProximal;
            }
            else if (fingerID == XRHandFingerID.Middle)
            {
                proxID = XRHandJointID.MiddleProximal;
            }
            else if (fingerID == XRHandFingerID.Ring)
            {
                proxID = XRHandJointID.RingProximal;
            }
            else
            {
                proxID = XRHandJointID.LittleProximal;
            }

            XRHandJointID tipID = proxID + (fingerID == XRHandFingerID.Thumb? 2 : 3);

            if (!(_xrHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose) &&
                  _xrHand.GetJoint(tipID).TryGetPose(out Pose tipPose) &&
                  _xrHand.GetJoint(proxID).TryGetPose(out Pose proximalPose)))
            {
                return false;
            }

            Vector3 wristToTip = tipPose.position - wristPose.position;
            Vector3 wristToProximal = proximalPose.position - wristPose.position;

            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        #endregion

        #region DEBUG

        public virtual void DebugHand()
        {
            if (_xrHand.isTracked && _metaHand != null && _metaHand.IsTracked())
            {
                _xrHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose);
                DebugExtension.DrawCoordinatesSystem(wristPose.position, wristPose.right, wristPose.up, wristPose.forward);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_metaHand.devicePosition.ReadValue(), 0.01f);
            }

            //if (_xrHand.handedness != Handedness.Invalid)
            //{
            //    _xrHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose);
            //    //DebugExtension.DrawCoordinatesSystem(wristPose.position, wristPose.right, wristPose.up, wristPose.forward);

            //    _xrHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose tipPose);
            //    _xrHand.GetJoint(XRHandJointID.ThumbProximal).TryGetPose(out Pose proxPose);

            //    if (_xrHand.handedness == Handedness.Right)
            //    {
            //        //Debug.DrawLine(proxPose.position, tipPose.position, Color.yellow);
            //        Vector3 v = Vector3.ProjectOnPlane(tipPose.position - proxPose.position, wristPose.up);
            //        Vector3 w = Vector3.ProjectOnPlane(wristPose.forward, wristPose.up);
            //        float angle = Vector3.SignedAngle(w, v, wristPose.up);

            //        DebugExtension.DrawCoordinatesSystem(wristPose.position, v, wristPose.up, w);
            //    }
            //}
        }

        #endregion

    }

}
