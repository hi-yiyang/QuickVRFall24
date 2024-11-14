using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace QuickVR
{
    
    public class QuickVRNodeHand : QuickVRNode
    {

        #region PUBLIC ATTRIBUTES

        protected bool _isLeft
        {
            get
            {
                return _role == QuickHumanBodyBones.LeftHand;
            }
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Dictionary<XRHandJointID, QuickHumanBodyBones> _toQuickHumanBodyBones
        {
            get
            {
                if (m_ToQuickHumanBodyBones == null || m_ToQuickHumanBodyBones.Count == 0)
                {
                    m_ToQuickHumanBodyBones = new Dictionary<XRHandJointID, QuickHumanBodyBones>();

                    m_ToQuickHumanBodyBones[XRHandJointID.ThumbMetacarpal] = _isLeft? QuickHumanBodyBones.LeftThumbProximal : QuickHumanBodyBones.RightThumbProximal;
                    m_ToQuickHumanBodyBones[XRHandJointID.ThumbProximal] = _isLeft ? QuickHumanBodyBones.LeftThumbIntermediate : QuickHumanBodyBones.RightThumbIntermediate;
                    m_ToQuickHumanBodyBones[XRHandJointID.ThumbDistal] = _isLeft ? QuickHumanBodyBones.LeftThumbDistal : QuickHumanBodyBones.RightThumbDistal;
                    m_ToQuickHumanBodyBones[XRHandJointID.ThumbTip] = _isLeft ? QuickHumanBodyBones.LeftThumbTip : QuickHumanBodyBones.RightThumbTip;

                    m_ToQuickHumanBodyBones[XRHandJointID.IndexProximal] = _isLeft ? QuickHumanBodyBones.LeftIndexProximal : QuickHumanBodyBones.RightIndexProximal;
                    m_ToQuickHumanBodyBones[XRHandJointID.IndexIntermediate] = _isLeft ? QuickHumanBodyBones.LeftIndexIntermediate : QuickHumanBodyBones.RightIndexIntermediate;
                    m_ToQuickHumanBodyBones[XRHandJointID.IndexDistal] = _isLeft ? QuickHumanBodyBones.LeftIndexDistal : QuickHumanBodyBones.RightIndexDistal;
                    m_ToQuickHumanBodyBones[XRHandJointID.IndexTip] = _isLeft ? QuickHumanBodyBones.LeftIndexTip : QuickHumanBodyBones.RightIndexTip;

                    m_ToQuickHumanBodyBones[XRHandJointID.MiddleProximal] = _isLeft ? QuickHumanBodyBones.LeftMiddleProximal : QuickHumanBodyBones.RightMiddleProximal;
                    m_ToQuickHumanBodyBones[XRHandJointID.MiddleIntermediate] = _isLeft ? QuickHumanBodyBones.LeftMiddleIntermediate : QuickHumanBodyBones.RightMiddleIntermediate;
                    m_ToQuickHumanBodyBones[XRHandJointID.MiddleDistal] = _isLeft ? QuickHumanBodyBones.LeftMiddleDistal : QuickHumanBodyBones.RightMiddleDistal;
                    m_ToQuickHumanBodyBones[XRHandJointID.MiddleTip] = _isLeft ? QuickHumanBodyBones.LeftMiddleTip : QuickHumanBodyBones.RightMiddleTip;

                    m_ToQuickHumanBodyBones[XRHandJointID.RingProximal] = _isLeft ? QuickHumanBodyBones.LeftRingProximal : QuickHumanBodyBones.RightRingProximal;
                    m_ToQuickHumanBodyBones[XRHandJointID.RingIntermediate] = _isLeft ? QuickHumanBodyBones.LeftRingIntermediate : QuickHumanBodyBones.RightRingIntermediate;
                    m_ToQuickHumanBodyBones[XRHandJointID.RingDistal] = _isLeft ? QuickHumanBodyBones.LeftRingDistal : QuickHumanBodyBones.RightRingDistal;
                    m_ToQuickHumanBodyBones[XRHandJointID.RingTip] = _isLeft ? QuickHumanBodyBones.LeftRingTip : QuickHumanBodyBones.RightRingTip;

                    m_ToQuickHumanBodyBones[XRHandJointID.LittleProximal] = _isLeft ? QuickHumanBodyBones.LeftLittleProximal : QuickHumanBodyBones.RightLittleProximal;
                    m_ToQuickHumanBodyBones[XRHandJointID.LittleIntermediate] = _isLeft ? QuickHumanBodyBones.LeftLittleIntermediate : QuickHumanBodyBones.RightLittleIntermediate;
                    m_ToQuickHumanBodyBones[XRHandJointID.LittleDistal] = _isLeft ? QuickHumanBodyBones.LeftLittleDistal : QuickHumanBodyBones.RightLittleDistal;
                    m_ToQuickHumanBodyBones[XRHandJointID.LittleTip] = _isLeft ? QuickHumanBodyBones.LeftLittleTip : QuickHumanBodyBones.RightLittleTip;
                }

                return m_ToQuickHumanBodyBones;
            }
        }
        protected Dictionary<XRHandJointID, QuickHumanBodyBones> m_ToQuickHumanBodyBones = null;

        protected QuickVRHandAnimatorBase _handAnimator = null;
        
        protected const int NUM_BONES_PER_FINGER = 4;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            QuickVRManager.OnSourceAnimatorSet += ActionOnSourceAnimatorSet;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            QuickVRManager.OnSourceAnimatorSet -= ActionOnSourceAnimatorSet;
        }

        #endregion

        #region GET AND SET

        private void ActionOnSourceAnimatorSet(Animator animator)
        {
            if (_handAnimator.GetType() == typeof(QuickVRHandAnimatorHumanoid))
            {
                ((QuickVRHandAnimatorHumanoid)_handAnimator)._animator = animator;
            }
        }

        public override void SetRole(QuickHumanBodyBones role)
        {
            base.SetRole(role);

            _handAnimator = Instantiate(Resources.Load<QuickVRHandAnimatorBase>("Prefabs/" + (role == QuickHumanBodyBones.LeftHand ? "pf_HandLeft" : "pf_HandRight")), transform);
            _handAnimator.transform.localPosition = Vector3.zero;

            foreach (SkinnedMeshRenderer r in _handAnimator.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMesh = null;
            }
        }

        public virtual QuickVRHandAnimatorBase GetHandAnimator()
        {
            return _handAnimator;
        }

        //protected override bool GetDevicePosition(out Vector3 pos)
        //{
        //    return _inputDevice.TryGetFeatureValue(QuickVRUsages.pointerPosition, out pos) || base.GetDevicePosition(out pos);
        //}

        //protected override bool GetDeviceRotation(out Quaternion rot)
        //{
        //    return _inputDevice.TryGetFeatureValue(QuickVRUsages.pointerRotation, out rot) || base.GetDeviceRotation(out rot);
        //}

        #endregion

        #region UPDATE

        protected override void UpdateTrackedPosition(Vector3 localPos)
        {
            base.UpdateTrackedPosition(localPos);

            if (QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Controllers)
            {
                _trackedObject.transform.position = _handAnimator.GetComponent<QuickVRHandOffset>()._handOrigin.position;
            }
            else
            {
                _trackedObject.transform.localPosition = Vector3.zero;
            }
        }

        protected override void UpdateTrackedRotation(Quaternion localRot)
        {
            base.UpdateTrackedRotation(localRot);

            if (QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Controllers)
            {
                _trackedObject.transform.rotation = _handAnimator.GetComponent<QuickVRHandOffset>()._handOrigin.rotation;
            }
            else
            {
                _trackedObject.transform.localRotation = Quaternion.identity;
            }
        }

        public override void UpdateState()
        {
            QuickVRHand h = QuickVRHandTrackingManager.GetVRHand(_isLeft);
            if (h != null && h._isTracked)
            {
                XRHand xrHand = h._xrHand;
                XRHandJoint joint = xrHand.GetJoint(XRHandJointID.Wrist);
                if (joint.TryGetPose(out var pose))
                {
                    UpdateTrackedPosition(pose.position);
                    UpdateTrackedRotation(pose.rotation);
                }

                UpdateFingersXRHand(xrHand);

                _xrRig.GetVRNode(_isLeft ? QuickHumanBodyBones.LeftHandAim : QuickHumanBodyBones.RightHandAim).UpdateState();

                _state = State.Tracked;
            }
            else
            {
                base.UpdateState();

                UpdateFingersController();
            }
        }

        protected virtual void UpdateFingersXRHand(XRHand hand)
        {
            for (XRHandJointID jointID = XRHandJointID.Wrist; jointID != XRHandJointID.EndMarker; jointID++)
            {
                if (_toQuickHumanBodyBones.TryGetValue(jointID, out QuickHumanBodyBones boneID))
                {
                    XRHandJoint joint = hand.GetJoint(jointID);
                    QuickVRNode vrNode = QuickXRRig._instance.GetVRNode(boneID);
                    if (joint.TryGetPose(out Pose pose))
                    {
                        vrNode.transform.localPosition = pose.position;
                        vrNode.transform.localRotation = pose.rotation;
                        vrNode._state = State.Tracked;
                    }
                    else
                    {
                        vrNode._state = State.NotTracked;
                    }
                }
            }

            //QuickVRNode vrNodeThumbDistal = _playArea.GetVRNode(_isLeft ? QuickHumanBodyBones.LeftThumbDistal : QuickHumanBodyBones.RightThumbDistal);
            //QuickVRNode vrNodeThumbTip = _playArea.GetVRNode(_isLeft ? QuickHumanBodyBones.LeftThumbTip : QuickHumanBodyBones.RightThumbTip);
            //vrNodeThumbTip.transform.localPosition = vrNodeThumbTip.transform.localPosition;
            //vrNodeThumbTip.transform.localRotation = vrNodeThumbTip.transform.localRotation;
        }

        protected virtual void UpdateFingersController()
        {
            if (_isTracked && QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Controllers)
            {
                //Update the nodes of the fingers
                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, _isLeft);
                    for (int i = 0; i < QuickHumanTrait.NUM_BONES_PER_FINGER; i++)
                    {
                        QuickVRNode nFinger = QuickXRRig._instance.GetVRNode(fingerBones[i]);

                        //The finger is tracked.
                        Transform t = _handAnimator[(int)f][i]; // .GetBoneFingerTransform(f, i);
                        nFinger.transform.position = t.position;
                        nFinger.transform.rotation = t.rotation;

                        //Correct the rotation
                        //if (IsLeft())
                        //{
                        //    nFinger.transform.Rotate(Vector3.right, 180, Space.Self);
                        //    nFinger.transform.Rotate(Vector3.up, -90, Space.Self);
                        //}
                        //else
                        //{
                        //    nFinger.transform.Rotate(Vector3.up, 90, Space.Self);
                        //}

                        nFinger._state = State.Inferred;
                    }
                }
            }
        }

        #endregion

        #region DEBUG

        protected override void OnDrawGizmos()
        {
            if (!_model && _showModel && _isTrackedOrInferred)
            {
                LoadVRModel();
            }

            base.OnDrawGizmos();
        }

        #endregion

    }

}


