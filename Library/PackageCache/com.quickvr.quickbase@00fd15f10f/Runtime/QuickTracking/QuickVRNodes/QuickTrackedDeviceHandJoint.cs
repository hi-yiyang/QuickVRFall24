using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

namespace QuickVR
{

    public abstract class QuickTrackedDeviceHandJointBase<T, U> : QuickTrackedDevice
    {

        #region PROTECTED ATTRIBUTES

        protected U _handJointID = default;

        protected bool _isLeft = false;

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickTrackedDeviceHandJointBase(U handJointID, bool isLeft)
        {
            _handJointID = handJointID;
            _isLeft = isLeft;
        }

        #endregion

        #region GET AND SET

        protected abstract bool TryGetJointPose(out Pose pose);
        
        public override Vector3 GetTrackedPosition()
        {
            TryGetJointPose(out Pose pose);
            return pose.position;
        }

        public override Quaternion GetTrackedRotation()
        {
            TryGetJointPose(out Pose pose);
            return pose.rotation;
        }

        #endregion

    }

    public class QuickTrackedDeviceHandJoint : QuickTrackedDeviceHandJointBase<QuickVRHand, XRHandJointID>
    {
        
        #region PROTECTED ATTRIBUTES

        protected QuickVRHand _hand
        {
            get
            {
                return QuickVRHandTrackingManager.GetVRHand(_isLeft);
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            QuickXRRig xrRig = QuickXRRig._instance;

            //LEFT HAND             
            xrRig.GetVRNode(QuickHumanBodyBones.LeftHand).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.Wrist, true));

            xrRig.GetVRNode(QuickHumanBodyBones.LeftThumbProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbMetacarpal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftThumbIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbProximal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftThumbDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbDistal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftThumbTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbTip, true));

            xrRig.GetVRNode(QuickHumanBodyBones.LeftIndexProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexProximal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftIndexIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexIntermediate, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftIndexDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexDistal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftIndexTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexTip, true));

            xrRig.GetVRNode(QuickHumanBodyBones.LeftMiddleProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleProximal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftMiddleIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleIntermediate, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftMiddleDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleDistal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftMiddleTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleTip, true));

            xrRig.GetVRNode(QuickHumanBodyBones.LeftRingProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingProximal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftRingIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingIntermediate, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftRingDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingDistal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftRingTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingTip, true));

            xrRig.GetVRNode(QuickHumanBodyBones.LeftLittleProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleProximal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftLittleIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleIntermediate, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftLittleDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleDistal, true));
            xrRig.GetVRNode(QuickHumanBodyBones.LeftLittleTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleTip, true));

            //RIGHT HAND
            xrRig.GetVRNode(QuickHumanBodyBones.RightHand).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.Wrist, false));

            xrRig.GetVRNode(QuickHumanBodyBones.RightThumbProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbMetacarpal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightThumbIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbProximal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightThumbDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbDistal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightThumbTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.ThumbTip, false));

            xrRig.GetVRNode(QuickHumanBodyBones.RightIndexProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexProximal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightIndexIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexIntermediate, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightIndexDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexDistal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightIndexTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.IndexTip, false));

            xrRig.GetVRNode(QuickHumanBodyBones.RightMiddleProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleProximal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightMiddleIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleIntermediate, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightMiddleDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleDistal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightMiddleTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.MiddleTip, false));

            xrRig.GetVRNode(QuickHumanBodyBones.RightRingProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingProximal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightRingIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingIntermediate, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightRingDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingDistal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightRingTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.RingTip, false));

            xrRig.GetVRNode(QuickHumanBodyBones.RightLittleProximal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleProximal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightLittleIntermediate).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleIntermediate, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightLittleDistal).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleDistal, false));
            xrRig.GetVRNode(QuickHumanBodyBones.RightLittleTip).AddTrackedDevice(new QuickTrackedDeviceHandJoint(XRHandJointID.LittleTip, false));
        }

        public QuickTrackedDeviceHandJoint(XRHandJointID handJointID, bool isLeft) : base(handJointID, isLeft)
        {

        }

        #endregion

        #region GET AND SET

        public override bool IsTracking()
        {
            return _hand != null && _hand._isTracked;
        }

        protected override bool TryGetJointPose(out Pose pose) 
        {
            XRHandJoint joint = _hand._xrHand.GetJoint(_handJointID);
            return joint.TryGetPose(out pose);
        }

        #endregion

        #region UPDATE

        public override void UpdateTracking()
        {
            base.UpdateTracking();

            QuickTrackedObject tObject = _vrNode.GetTrackedObject();
            tObject.transform.localPosition = Vector3.zero;
            tObject.transform.localRotation = Quaternion.identity;
        }

        #endregion

    }

}