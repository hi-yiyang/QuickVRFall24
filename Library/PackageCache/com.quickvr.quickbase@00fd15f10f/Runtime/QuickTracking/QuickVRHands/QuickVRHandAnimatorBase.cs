using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public abstract class QuickVRHandAnimatorBase : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeft = false;

        public QuickHandPose _handPoseNeutral = null;
        public QuickHandPose _handPoseClosed = null;
        public QuickHandPose _handPoseThumbUp = null;

        [Range(0.0f, 1.0f)]
        public float _closeFactor = 0;

        public bool _isPointing = false;
        [Range(-1.0f, 1.0f)]
        public float _pointingValue = 0.75f;

        public bool _isThumbUp = false;

        [System.Serializable]
        public class FingerBones
        {
            public Transform _proximal = null;
            public Transform _intermediate = null;
            public Transform _distal = null;
            public Transform _tip = null;

            public enum RotAxis
            {
                Right,
                Up,
                Forward,
                Left,
                Down,
                Back
            }

            public RotAxis _axisClose;
            public RotAxis _axisSeparate;

            private static Vector3[] _rotAxisToVector3 = { Vector3.right, Vector3.up, Vector3.forward, Vector3.left, Vector3.down, Vector3.back };

            public virtual Transform this[int boneIndex]
            {
                get
                {
                    if (boneIndex == 0) return _proximal;
                    if (boneIndex == 1) return _intermediate;
                    if (boneIndex == 2) return _distal;
                    if (boneIndex == 3) return _tip;

                    return null;
                }
                set
                {
                    if (boneIndex == 0)
                    {
                        _proximal = value;
                    }
                    if (boneIndex == 1)
                    {
                        _intermediate = value;
                    }
                    if (boneIndex == 2)
                    {
                        _distal = value;
                    }
                    if (boneIndex == 3)
                    {
                        _tip = value;
                    }
                }
            }

            public virtual Vector3 GetRotAxisClose()
            {
                return _rotAxisToVector3[(int)_axisClose];
            }

            public virtual Vector3 GetRotAxisSeparate()
            {
                return _rotAxisToVector3[(int)_axisSeparate];
            }

            public virtual bool CheckTransforms()
            {
                return _proximal && _intermediate && _distal && _tip;
            }

        }

        public FingerBones _fingerBonesThumb = new FingerBones();
        public FingerBones _fingerBonesIndex = new FingerBones();
        public FingerBones _fingerBonesMiddle = new FingerBones();
        public FingerBones _fingerBonesRing = new FingerBones();
        public FingerBones _fingerBonesLittle = new FingerBones();

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickHandGestureSettings _handGestureSettings
        {
            get
            {
                QuickHandGestureSettings result = null;
                if (Application.isPlaying)
                {
                    Animator animatorSrc = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource();
                    if (animatorSrc)
                    {
                        QuickUnityVR unityVR = animatorSrc.GetComponent<QuickUnityVR>();
                        result = _isLeft ? unityVR._gestureSettingsLeftHand : unityVR._gestureSettingsRightHand;
                    }
                }

                return result;
            }

        }

        protected float _blendPoint = 0;
        protected float _blendThumbUp = 0;

        protected Collider[] _colliders = null;

        #endregion

        #region CONSTANTS

        protected const float INPUT_RATE_CHANGE = 20.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();

            _colliders = GetComponentsInChildren<Collider>(true);
        }

        protected virtual void Reset()
        {

        }

        #endregion

        #region GET AND SET

        public FingerBones this[int i]
        {
            get
            {
                if (i == 0) return _fingerBonesThumb;
                if (i == 1) return _fingerBonesIndex;
                if (i == 2) return _fingerBonesMiddle;
                if (i == 3) return _fingerBonesRing;

                return _fingerBonesLittle;
            }
        }

        protected virtual bool CheckHandPoses()
        {
            return _handPoseNeutral && _handPoseClosed && _handPoseThumbUp;
        }

        protected virtual float InputValueRateChange(bool isDown, float value)
        {
            float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
            float sign = isDown ? 1.0f : -1.0f;
            return Mathf.Clamp01(value + rateDelta * sign);
        }

        public virtual Collider[] GetColliders()
        {
            return _colliders;
        }

        #endregion

        #region UPDATE

        public virtual void Update()
        {
            if (CheckHandPoses())
            {
                UpdateInputStates();
                UpdateFingers();
            }
        }

        protected virtual void UpdateInputStates()
        {
            if (_handGestureSettings != null)
            {
                _isPointing = _handGestureSettings.IsPointing();
                _closeFactor = InputManagerVR.GetAxis(_isLeft ? InputManagerVR.AxisCode.LeftGrip : InputManagerVR.AxisCode.RightGrip);
                _isThumbUp = _handGestureSettings.IsThumbUp() && _closeFactor >= 0.75f;
            }

            _blendPoint = InputValueRateChange(_isPointing, _blendPoint);
            _blendThumbUp = InputValueRateChange(_isThumbUp, _blendThumbUp);
        }

        protected abstract void UpdateFingers();

        #endregion

    }

}

