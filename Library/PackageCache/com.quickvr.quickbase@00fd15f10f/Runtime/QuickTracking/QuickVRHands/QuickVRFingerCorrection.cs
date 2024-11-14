using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public class QuickVRFingerCorrection : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        [Header("Thumb Finger Correction")]
        public float _separationOffsetThumb = 0.0f;

        [Header("Index Finger Correction")]
        //[Range(-1.0f, 1.0f)]
        public float _closeOffsetIndex = 0.0f;
        public float _separationOffsetIndex = 0.0f;

        [Header("Middle Finger Correction")]
        //[Range(-1.0f, 1.0f)]
        public float _closeOffsetMiddle = 0.0f;
        public float _separationOffsetMiddle = 0.0f;

        [Header("Ring Finger Correction")]
        //[Range(-1.0f, 1.0f)]
        public float _closeOffsetRing = 0.0f;
        public float _separationOffsetRing = 0.0f;

        [Header("Little Finger Correction")]
        //[Range(-1.0f, 1.0f)]
        public float _closeOffsetLittle = 0.0f;
        public float _separationOffsetLittle = 0.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected HumanPose _humanPose = new HumanPose();
        protected Animator _animator
        {
            get
            {
                return GetComponent<Animator>();
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCopyPose += ActionOnPostCopyPose;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCopyPose -= ActionOnPostCopyPose;
        }

        #endregion

        #region UPDATE

        protected virtual void ActionOnPostCopyPose()
        {
            //Debug.Log(QuickHumanTrait.GetMuscleName(QuickHumanTrait.GetMuscleFromBone(HumanBodyBones.RightIndexProximal, 0)));
            //Debug.Log(QuickHumanTrait.GetMuscleName(QuickHumanTrait.GetMuscleFromBone(HumanBodyBones.RightIndexProximal, 1)));
            //Debug.Log(QuickHumanTrait.GetMuscleName(QuickHumanTrait.GetMuscleFromBone(HumanBodyBones.RightIndexProximal, 2)));

            QuickHumanPoseHandler.GetHumanPose(_animator, ref _humanPose);

            //Correct left hand
            CorrectFingerSeparationFactor(HumanBodyBones.LeftThumbProximal, _separationOffsetThumb, ref _humanPose);

            CorrectFingerCloseFactor(HumanBodyBones.LeftIndexProximal, _closeOffsetIndex, ref _humanPose);
            CorrectFingerCloseFactor(HumanBodyBones.LeftMiddleProximal, _closeOffsetMiddle, ref _humanPose);
            CorrectFingerCloseFactor(HumanBodyBones.LeftRingProximal, _closeOffsetRing, ref _humanPose);
            CorrectFingerCloseFactor(HumanBodyBones.LeftLittleProximal, _closeOffsetLittle, ref _humanPose);

            CorrectFingerSeparationFactor(HumanBodyBones.LeftLittleProximal, _separationOffsetLittle, ref _humanPose);

            //Correct right hand
            CorrectFingerSeparationFactor(HumanBodyBones.RightThumbProximal, _separationOffsetThumb, ref _humanPose);

            CorrectFingerCloseFactor(HumanBodyBones.RightIndexProximal, _closeOffsetIndex, ref _humanPose);
            CorrectFingerCloseFactor(HumanBodyBones.RightMiddleProximal, _closeOffsetMiddle, ref _humanPose);
            CorrectFingerCloseFactor(HumanBodyBones.RightRingProximal, _closeOffsetRing, ref _humanPose);
            CorrectFingerCloseFactor(HumanBodyBones.RightLittleProximal, _closeOffsetLittle, ref _humanPose);

            QuickHumanPoseHandler.SetHumanPose(_animator, ref _humanPose);
            //Debug.Log(_humanPose.muscles[muscleID].ToString("f3"));
        }

        protected virtual void CorrectFingerCloseFactor(HumanBodyBones boneFingerID, float closeOffset, ref HumanPose humanPose)
        {
            int muscleID = QuickHumanTrait.GetMuscleFromBone(boneFingerID, 1);
            humanPose.muscles[muscleID] += closeOffset;
        }

        protected virtual void CorrectFingerSeparationFactor(HumanBodyBones boneFingerID, float separationOffset, ref HumanPose humanPose)
        {
            int muscleID = QuickHumanTrait.GetMuscleFromBone(boneFingerID, 2);
            humanPose.muscles[muscleID] += separationOffset;
        }

        #endregion

    }
}


