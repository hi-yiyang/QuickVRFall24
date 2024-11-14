using QuickVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [ExecuteInEditMode]
    public class QuickVRHandAnimatorHumanoid : QuickVRHandAnimatorBase
    {

        #region PROTECTED ATTRIBUTES

        protected HumanPose _humanPose = new HumanPose();
        public Animator _animator
        {
            get
            {
                return m_Animator;
            }
            set
            {
                if (m_Animator != value)
                {
                    m_Animator = value;
                    InitFingerBones();
                }
            }
        }
        protected Animator m_Animator = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            if (_handPoseNeutral == null)
            {
                _handPoseNeutral = Resources.Load<QuickHandPose>("HandPoses/HandPose_Neutral_Humanoid");
            }
            if (_handPoseClosed == null)
            {
                _handPoseClosed = Resources.Load<QuickHandPose>("HandPoses/HandPose_Closed_Humanoid");
            }
            if (_handPoseThumbUp == null)
            {
                _handPoseThumbUp = Resources.Load<QuickHandPose>("HandPoses/HandPose_ThumbUp_Humanoid");
            }
        }

        [ButtonMethod]
        public virtual void InitFingerBones()
        {
            if (_animator)
            {
                InitFingerBonesThumb();
                InitFingerBonesIndex();
                InitFingerBonesMiddle();
                InitFingerBonesRing();
                InitFingerBonesLittle();
            }
        }

        protected virtual void InitFingerBonesThumb()
        {
            QuickHumanBodyBones[] boneFingersLeft =
            {
                QuickHumanBodyBones.LeftThumbProximal,
                QuickHumanBodyBones.LeftThumbIntermediate,
                QuickHumanBodyBones.LeftThumbDistal,
                QuickHumanBodyBones.LeftThumbTip
            };

            QuickHumanBodyBones[] boneFingersRight =
            {
                QuickHumanBodyBones.RightThumbProximal,
                QuickHumanBodyBones.RightThumbIntermediate,
                QuickHumanBodyBones.RightThumbDistal,
                QuickHumanBodyBones.RightThumbTip
            };

            InitFingerBones(_isLeft ? boneFingersLeft : boneFingersRight, _fingerBonesThumb);
        }

        protected virtual void InitFingerBonesIndex()
        {
            QuickHumanBodyBones[] boneFingersLeft =
            {
                QuickHumanBodyBones.LeftIndexProximal,
                QuickHumanBodyBones.LeftIndexIntermediate,
                QuickHumanBodyBones.LeftIndexDistal,
                QuickHumanBodyBones.LeftIndexTip
            };

            QuickHumanBodyBones[] boneFingersRight =
            {
                QuickHumanBodyBones.RightIndexProximal,
                QuickHumanBodyBones.RightIndexIntermediate,
                QuickHumanBodyBones.RightIndexDistal,
                QuickHumanBodyBones.RightIndexTip
            };

            InitFingerBones(_isLeft ? boneFingersLeft : boneFingersRight, _fingerBonesIndex);
        }

        protected virtual void InitFingerBonesMiddle()
        {
            QuickHumanBodyBones[] boneFingersLeft =
            {
                QuickHumanBodyBones.LeftMiddleProximal,
                QuickHumanBodyBones.LeftMiddleIntermediate,
                QuickHumanBodyBones.LeftMiddleDistal,
                QuickHumanBodyBones.LeftMiddleTip
            };

            QuickHumanBodyBones[] boneFingersRight =
            {
                QuickHumanBodyBones.RightMiddleProximal,
                QuickHumanBodyBones.RightMiddleIntermediate,
                QuickHumanBodyBones.RightMiddleDistal,
                QuickHumanBodyBones.RightMiddleTip
            };

            InitFingerBones(_isLeft ? boneFingersLeft : boneFingersRight, _fingerBonesMiddle);
        }

        protected virtual void InitFingerBonesRing()
        {
            QuickHumanBodyBones[] boneFingersLeft =
            {
                QuickHumanBodyBones.LeftRingProximal,
                QuickHumanBodyBones.LeftRingIntermediate,
                QuickHumanBodyBones.LeftRingDistal,
                QuickHumanBodyBones.LeftRingTip
            };

            QuickHumanBodyBones[] boneFingersRight =
            {
                QuickHumanBodyBones.RightRingProximal,
                QuickHumanBodyBones.RightRingIntermediate,
                QuickHumanBodyBones.RightRingDistal,
                QuickHumanBodyBones.RightRingTip
            };

            InitFingerBones(_isLeft ? boneFingersLeft : boneFingersRight, _fingerBonesRing);
        }

        protected virtual void InitFingerBonesLittle()
        {
            QuickHumanBodyBones[] boneFingersLeft =
            {
                QuickHumanBodyBones.LeftLittleProximal,
                QuickHumanBodyBones.LeftLittleIntermediate,
                QuickHumanBodyBones.LeftLittleDistal,
                QuickHumanBodyBones.LeftLittleTip
            };

            QuickHumanBodyBones[] boneFingersRight =
            {
                QuickHumanBodyBones.RightLittleProximal,
                QuickHumanBodyBones.RightLittleIntermediate,
                QuickHumanBodyBones.RightLittleDistal,
                QuickHumanBodyBones.RightLittleTip
            };

            InitFingerBones(_isLeft ? boneFingersLeft : boneFingersRight, _fingerBonesLittle);
        }

        protected virtual void InitFingerBones(QuickHumanBodyBones[] boneFingers, FingerBones result)
        {
            for (int i = 0; i <  boneFingers.Length; i++)
            {
                result[i] = _animator.GetBoneTransformNormalized(boneFingers[i]);
            }
        }

        #endregion

        #region UPDATE

        protected override void UpdateFingers()
        {
            if (_animator)
            {
                QuickHumanPoseHandler.GetHumanPose(_animator, ref _humanPose);

                //Close factor
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        float mValueStretch = 0;

                        if (i == 0 && _blendThumbUp > 0)
                        {
                            mValueStretch = Mathf.Lerp(_handPoseNeutral[i].GetCloseFactor(j), _handPoseThumbUp[i].GetCloseFactor(j), _blendThumbUp);
                        }
                        else if (i == 1 && _blendPoint > 0)
                        {
                            mValueStretch = Mathf.Lerp(_handPoseNeutral[i].GetCloseFactor(j), _pointingValue, _blendPoint);
                        }
                        else
                        {
                            mValueStretch = Mathf.Lerp(_handPoseNeutral[i].GetCloseFactor(j), _handPoseClosed[i].GetCloseFactor(j), _closeFactor);
                        }

                        //Separation factor
                        float mValueSpread = 0;
                        if (i == 0 && _blendThumbUp > 0)
                        {
                            mValueSpread = Mathf.Lerp(_handPoseNeutral[i]._separation, _handPoseThumbUp[i]._separation, _blendThumbUp);
                        }
                        else
                        {
                            mValueSpread = Mathf.Lerp(_handPoseNeutral[i]._separation, _handPoseClosed[i]._separation, _closeFactor);
                        }

                        HumanBodyBones bone = (_isLeft ? HumanBodyBones.LeftThumbProximal : HumanBodyBones.RightThumbProximal) + (i * 3 + j);
                        UpdateFingerStretch(bone, mValueStretch, ref _humanPose);
                        UpdateFingerSpread(bone, mValueSpread, ref _humanPose);
                    }
                }

                QuickHumanPoseHandler.SetHumanPose(_animator, ref _humanPose);
            }
        }

        protected virtual void UpdateFingerStretch(HumanBodyBones boneID, float mValue, ref HumanPose humanPose)
        {
            int muscleID = QuickHumanTrait.GetMuscleFromBone(boneID, 2);
            if (muscleID != -1)
            {
                humanPose.muscles[muscleID] = mValue;
            }
        }

        protected virtual void UpdateFingerSpread(HumanBodyBones boneID, float mValue, ref HumanPose humanPose)
        {
            int muscleID = QuickHumanTrait.GetMuscleFromBone(boneID, 1);
            if (muscleID != -1)
            {
                humanPose.muscles[muscleID] = mValue;
            }
        }

        public virtual QuickHandPose Get_handPoseNeutral()
        {
            return _handPoseNeutral;
        }

        [ButtonMethod]
        public virtual void PrintHandPose()
        {
            QuickHumanPoseHandler.GetHumanPose(_animator, ref _humanPose);

            for (int i = 0; i < 5; i++)
            {
                HumanBodyBones boneID = (_isLeft ? HumanBodyBones.LeftThumbProximal : HumanBodyBones.RightThumbProximal) + (i * 3);
                float mValueSeparation = _humanPose.muscles[QuickHumanTrait.GetMuscleFromBone(boneID, 1)];

                for (int j = 0; j < 3; j++)
                {
                    float mValueStretch = _humanPose.muscles[QuickHumanTrait.GetMuscleFromBone(boneID + j, 2)];
                    Debug.Log(mValueStretch.ToString("f3"));
                }
                Debug.Log(mValueSeparation.ToString("f3"));
                Debug.Log("================");
            }
        }

        #endregion

    }

}

