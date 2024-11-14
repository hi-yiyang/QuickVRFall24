using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace QuickVR
{

    public class QuickLocomotionAnimationBase : PlayableSampleBase
    {

        #region PUBLIC ATTRIBUTES

        [Range(0.0f, 1.0f)]
        public float _weight = 1.0f;

        public float _smoothness = 0.1f;

        public QuickLocomotionTracker _locomotionTracker
        {
            get
            {
                if (!m_LocomotionTracker)
                {
                    m_LocomotionTracker = gameObject.GetOrCreateComponent<QuickLocomotionTracker>();
                }

                return m_LocomotionTracker;
            }
            set
            {
                m_LocomotionTracker = value;
            }
        }
        protected QuickLocomotionTracker m_LocomotionTracker = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickIKManager _ikManager = null;

        protected AnimatorControllerPlayable _animatorPlayable;

        protected Vector3 _lastLocalDisplacement = Vector3.zero;
        protected float _lastSpeed = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _animator.applyRootMotion = false;
            _ikManager = GetComponent<QuickIKManager>();
        }

        protected override void InitPlayableGraph()
        {
            // Creates AnimationClipPlayable and connects them to the mixer.
            _animatorPlayable = AnimatorControllerPlayable.Create(_playableGraph, Resources.Load<RuntimeAnimatorController>("QuickLocomotionMaster"));
            _playableOutput.SetSourcePlayable(_animatorPlayable);
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            _playableOutput.SetWeight(_weight);

            float speed = _locomotionTracker._speed;
            speed = Mathf.Lerp(_lastSpeed, speed, _smoothness);

            _animatorPlayable.SetBool("ShouldMove", speed > 0.001f);

            Vector3 localDisplacement = _locomotionTracker._localDisplacement;
            localDisplacement = Vector3.Lerp(_lastLocalDisplacement, localDisplacement, _smoothness);
            localDisplacement = localDisplacement.normalized * speed;

            _animatorPlayable.SetFloat("VelX", localDisplacement.x);
            _animatorPlayable.SetFloat("VelZ", localDisplacement.z);

            _lastLocalDisplacement = localDisplacement;
            _lastSpeed = speed;
        }

        protected virtual void LateUpdate()
        {
            if (_ikManager)
            {
                //Update the IKTargets
                UpdateFoot(true);
                UpdateFoot(false);
            }
        }

        protected virtual void UpdateFoot(bool isLeft)
        {
            HumanBodyBones boneID = isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            QuickIKSolver ikSolverFoot = _ikManager.GetIKSolver(boneID);

            Transform tBone = _animator.GetBoneTransformNormalized(boneID);
            ikSolverFoot._targetLimb.position = tBone.position;
            ikSolverFoot._targetLimb.rotation = tBone.rotation;

            ikSolverFoot.UpdateIK();
        }

        public override void Evaluate(float deltaTime)
        {
            Update();

            base.Evaluate(deltaTime);

            LateUpdate();
        }

        #endregion

    }

}


