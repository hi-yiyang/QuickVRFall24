using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickAnimator : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES
        
        public float _headHeight { get; protected set; }

        public Transform _eyeVRLeft {get; protected set;}
        public Transform _eyeVRRight { get; protected set; }
        public Transform _eyeVRCenter { get; protected set; }

        public Transform _tipThumbLeft { get; protected set; }
        public Transform _tipIndexLeft { get; protected set; }
        public Transform _tipMiddleLeft { get; protected set; }
        public Transform _tipRingLeft { get; protected set; }
        public Transform _tipLittleLeft { get; protected set; }

        public Transform _tipThumbRight { get; protected set; }
        public Transform _tipIndexRight { get; protected set; }
        public Transform _tipMiddleRight { get; protected set; }
        public Transform _tipRingRight { get; protected set; }
        public Transform _tipLittleRight { get; protected set; }

        public Transform _vrHandAimLeft { get; protected set; }
        public Transform _vrHandAimRight { get; protected set; }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Dictionary<QuickHumanBodyBones, Transform> _normalizedTransforms = null;

        protected Animator _animator
        {
            get
            {
                if (!m_Animator)
                {
                    m_Animator = GetComponent<Animator>();
                }

                return m_Animator;
            }
        }
        protected Animator m_Animator = null;

        #endregion

        #region CONSTANTS

        protected const string NORMALIZED_TRANSFORM_NAME = "__NormalizedTransform__";

        public const string VR_EYE_LEFT = "__VREyeLeft__";
        public const string VR_EYE_RIGHT = "__VREyeRight__";
        public const string VR_EYE_CENTER = "__VREyeCenter__";

        public const string VR_HAND_AIM_LEFT = "__VRHandAimLeft__";
        public const string VR_HAND_AIM_RIGHT = "__VRHandAimRight__";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();
        }

        public virtual void Reset()
        {
            //Save the current human pose
            HumanPose tmpPose = new HumanPose();
            QuickHumanPoseHandler.GetHumanPose(_animator, ref tmpPose);

            _animator.EnforceTPose();
            Vector3 hPos = _animator.GetBoneTransform(HumanBodyBones.Head).position;
            Vector3 p = Vector3.ProjectOnPlane(hPos, transform.up);
            _headHeight = Vector3.Distance(hPos, p);

            CreateMissingBones();
            CreateNormalizedTransforms();

            //Restore the Human Pose
            QuickHumanPoseHandler.SetHumanPose(_animator, ref tmpPose);
        }

        protected virtual void CreateMissingBones()
        {
            CreateEyesVR();
            CreateFingerTips();
            CreateHandAims();
        }

        protected virtual void CreateEyesVR()
        {
            _eyeVRLeft = CreateEyeVR(true);
            _eyeVRRight = CreateEyeVR(false);
            _eyeVRCenter = CreateEyeCenterVR();
        }

        protected virtual Transform CreateEyeCenterVR()
        {
            Transform tHead = _animator.GetBoneTransform(HumanBodyBones.Head);
            Transform tResult = tHead.CreateChild(VR_EYE_CENTER);

            tResult.rotation = _animator.transform.rotation;
            tResult.position = Vector3.Lerp(_eyeVRLeft.position, _eyeVRRight.position, 0.5f);

            return tResult;
        }

        //private static void CreateEyeVR(this Animator animator, bool eyeLeft)
        //{
        //    Transform tHead = animator.GetBoneTransform(HumanBodyBones.Head);
        //    Transform tEye = animator.GetBoneTransform(eyeLeft ? HumanBodyBones.LeftEye : HumanBodyBones.RightEye);
        //    if (!tEye)
        //    {
        //        tEye = tHead.CreateChild(eyeLeft ? VR_EYE_LEFT : VR_EYE_RIGHT);

        //        //The eye center position
        //        tEye.position = tHead.position + animator.transform.forward * 0.15f + animator.transform.up * 0.13f;

        //        //Account for the Eye Separation
        //        float sign = eyeLeft ? -1.0f : 1.0f;
        //        tEye.position += sign * animator.transform.right * 0.032f;
        //    }
        //}

        protected virtual Transform CreateEyeVR(bool eyeLeft)
        {
            Transform tHead = _animator.GetBoneTransform(HumanBodyBones.Head);
            Transform tEye = tHead.CreateChild(eyeLeft ? VR_EYE_LEFT : VR_EYE_RIGHT);

            //The eye center position
            tEye.position = tHead.position + _animator.transform.forward * 0.15f + _animator.transform.up * 0.13f;

            //Account for the Eye Separation
            float sign = eyeLeft ? -1.0f : 1.0f;
            tEye.position += sign * _animator.transform.right * 0.032f;

            return tEye;
        }

        protected virtual void CreateFingerTips()
        {
            //Left hand tip bones
            _tipThumbLeft = CreateFingerTip(HumanBodyBones.LeftThumbDistal);
            _tipIndexLeft = CreateFingerTip(HumanBodyBones.LeftIndexDistal);
            _tipMiddleLeft = CreateFingerTip(HumanBodyBones.LeftMiddleDistal);
            _tipRingLeft = CreateFingerTip(HumanBodyBones.LeftRingDistal);
            _tipLittleLeft = CreateFingerTip(HumanBodyBones.LeftLittleDistal);

            //Right hand tip bones
            _tipThumbRight = CreateFingerTip(HumanBodyBones.RightThumbDistal);
            _tipIndexRight = CreateFingerTip(HumanBodyBones.RightIndexDistal);
            _tipMiddleRight = CreateFingerTip(HumanBodyBones.RightMiddleDistal);
            _tipRingRight = CreateFingerTip(HumanBodyBones.RightRingDistal);
            _tipLittleRight = CreateFingerTip(HumanBodyBones.RightLittleDistal);
        }

        protected virtual Transform CreateFingerTip(HumanBodyBones boneDistalID)
        {
            Transform tBoneTip = null;

            Transform tBoneDistal = _animator.GetBoneTransform(boneDistalID);
            if (tBoneDistal)
            {
                Transform tBoneIntermediate = _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(boneDistalID));
                Vector3 v = tBoneDistal.position - tBoneIntermediate.position;

                tBoneTip = tBoneDistal.CreateChild("__FingerTip__");
                tBoneTip.position = tBoneDistal.position + v;
            }

            return tBoneTip;
        }

        protected virtual void CreateHandAims()
        {
            _vrHandAimLeft = _animator.transform.CreateChild(VR_HAND_AIM_LEFT);
            _vrHandAimRight = _animator.transform.CreateChild(VR_HAND_AIM_RIGHT);
        }

        protected virtual void CreateNormalizedTransforms()
        {
            _normalizedTransforms = new Dictionary<QuickHumanBodyBones, Transform>();
            for (QuickHumanBodyBones boneID = 0; boneID != QuickHumanBodyBones.LastBone; boneID++)
            {
                CreateNormalizedTransform(boneID);
            }
        }

        protected virtual void CreateNormalizedTransform(QuickHumanBodyBones boneID)
        {
            Transform tResult = null;
            Transform tBone = _animator.GetBoneTransform(boneID);
            string boneName = boneID.ToString();
            bool isLeft = boneName.Contains("Left");

            if (tBone)
            {
                //By default, set tResult to be at bone's position and make it to look the same direction that the Animator is looking at. 
                tResult = tBone.CreateChild(NORMALIZED_TRANSFORM_NAME);
                tResult.position = tBone.position;
                tResult.rotation = transform.rotation;
                
                //Compute the normalized rotation for each bone
                if (boneID == QuickHumanBodyBones.LeftHand || boneID == QuickHumanBodyBones.RightHand)
                {
                    int sign = isLeft ? -1 : 1;
                    Transform tFingerMid = _animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);
                    Transform tFingerRing = _animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftRingProximal : HumanBodyBones.RightRingProximal);
                    Vector3 v = Vector3.ProjectOnPlane(sign * (tFingerRing.position - tFingerMid.position), transform.right);

                    tResult.LookAt(tResult.position + sign * transform.right, transform.up);
                    tResult.Rotate(tResult.forward, sign * Vector3.Angle(tResult.right, v), Space.World);
                }
                else if (boneID == QuickHumanBodyBones.Spine)
                {
                    Transform tBoneNext = GetFirstValidBone(new HumanBodyBones[]{ HumanBodyBones.Chest, HumanBodyBones.UpperChest, HumanBodyBones.Neck, HumanBodyBones.Head });
                    if (tBoneNext)
                    {
                        Vector3 v = tBoneNext.position - tResult.position;
                        Vector3 rotAxis = Vector3.Cross(tResult.up, v).normalized;
                        float rotAngle = Vector3.Angle(tResult.up, v);

                        tResult.Rotate(rotAxis, rotAngle, Space.World);
                    }
                }
                else if (boneName.Contains("Proximal") || boneName.Contains("Intermediate"))
                {
                    tResult.up = GetBoneTransformNormalized(isLeft ? QuickHumanBodyBones.LeftHand : QuickHumanBodyBones.RightHand).up;
                    tResult.forward = _animator.GetBoneTransform(boneID + 1).position - _animator.GetBoneTransform(boneID).position;
                    if (boneName.Contains("Thumb"))
                    {
                        float sign = boneName.Contains("Left") ? -1 : 1;
                        tResult.Rotate(tResult.forward, sign * 90, Space.World);
                    }
                }
                else if (boneName.Contains("Distal"))
                {
                    tResult.up = GetBoneTransformNormalized(isLeft ? QuickHumanBodyBones.LeftHand : QuickHumanBodyBones.RightHand).up;
                    //ikTarget.forward = _animator.GetBoneTransform(boneID).position - _animator.GetBoneTransform(boneID - 1).position;
                    tResult.forward = _animator.GetBoneTransformFingerTip((HumanBodyBones)boneID).position - _animator.GetBoneTransform(boneID).position;
                    if (boneName.Contains("Thumb"))
                    {
                        float sign = boneName.Contains("Left") ? -1 : 1;
                        tResult.Rotate(tResult.forward, sign * 90, Space.World);
                    }
                }
                else if (boneName.Contains("Tip"))
                {
                    tResult.rotation = tResult.parent.parent.Find(NORMALIZED_TRANSFORM_NAME).rotation;
                }
            }

            _normalizedTransforms[boneID] = tResult;
        }

        //[ButtonMethod]
        //public virtual void Test()
        //{
        //    CreateNormalizedTransforms();
        //}

        #endregion

        #region GET AND SET

        public virtual Transform GetBoneTransformFingerTip(QuickHumanBodyBones fingerBoneTipID)
        {
            Transform result = null;

            if (fingerBoneTipID >= QuickHumanBodyBones.LeftThumbTip && fingerBoneTipID <= QuickHumanBodyBones.RightLittleTip)
            {
                if (fingerBoneTipID == QuickHumanBodyBones.LeftThumbTip) result = _tipThumbLeft;
                else if (fingerBoneTipID == QuickHumanBodyBones.LeftIndexTip) result = _tipIndexLeft;
                else if (fingerBoneTipID == QuickHumanBodyBones.LeftMiddleTip) result = _tipMiddleLeft;
                else if (fingerBoneTipID == QuickHumanBodyBones.LeftRingTip) result = _tipRingLeft;
                else if (fingerBoneTipID == QuickHumanBodyBones.LeftLittleTip) result = _tipLittleLeft;

                else if (fingerBoneTipID == QuickHumanBodyBones.RightThumbTip) result = _tipThumbRight;
                else if (fingerBoneTipID == QuickHumanBodyBones.RightIndexTip) result = _tipIndexRight;
                else if (fingerBoneTipID == QuickHumanBodyBones.RightMiddleTip) result = _tipMiddleRight;
                else if (fingerBoneTipID == QuickHumanBodyBones.RightRingTip) result = _tipRingRight;
                else if (fingerBoneTipID == QuickHumanBodyBones.RightLittleTip) result = _tipLittleRight;
            }

            return result;
        }

        protected Transform GetFirstValidBone(HumanBodyBones[] bones)
        {
            Transform tResult = null;

            for (int i = 0; i < bones.Length && !tResult; i++)
            {
                tResult = _animator.GetBoneTransform(bones[i]);
            }

            return tResult;
        }

        public Transform GetBoneTransformNormalized(QuickHumanBodyBones boneID)
        {
            return _normalizedTransforms[boneID];
        }

        //public static Transform GetEyeVR(this Animator animator, bool eyeLeft)
        //{
        //    HumanBodyBones eyeBoneID = eyeLeft ? HumanBodyBones.LeftEye : HumanBodyBones.RightEye;
        //    Transform eye = animator.GetBoneTransform(eyeBoneID);
        //    if (!eye)
        //    {
        //        eye = animator.GetBoneTransform(HumanBodyBones.Head).Find(eyeLeft ? VR_EYE_LEFT : VR_EYE_RIGHT);
        //    }

        //    return eye;
        //}

        #endregion

    }

}


