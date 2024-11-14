using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

using System.Collections.Generic;

namespace QuickVR 
{

    public class QuickUnityVR : QuickIKManager
    {

        #region CONSTANTS

        protected static float HUMAN_HEADS_TALL = 7.5f;
        protected static float HUMAN_HEADS_TALL_EYES = HUMAN_HEADS_TALL - 0.5f;
        protected static float HUMAN_HEADS_TALL_HEAD = HUMAN_HEADS_TALL - 1.0f;

        #endregion

        #region PUBLIC ATTRIBUTES

        public QuickHandGestureSettings _gestureSettingsLeftHand = null;
        public QuickHandGestureSettings _gestureSettingsRightHand = null;

        public bool _useFootprints = true;

        public enum ControlType
        {
            Tracking,
            Animation,
            IK,
        }

        public bool _applyHeadRotation
        {
            get
            {
                return m_ApplyHeadRotation;
            }
            set
            {
                if (value != m_ApplyHeadRotation)
                {
                    GetIKSolver(IKBone.Head)._weightIKRot = value ? 1 : 0;

                    m_ApplyHeadRotation = value;
                }
            }
        }
        [SerializeField, HideInInspector]
        protected bool m_ApplyHeadRotation = true;

        public bool _applyHeadPosition
        {
            get
            {
                return m_ApplyHeadPosition;
            }
            set
            {
                if (value != m_ApplyHeadPosition)
                {
                    GetIKSolver(IKBone.Head)._weightIKPos = value ? 1 : 0;

                    m_ApplyHeadPosition = value;
                }
            }
        }
        [SerializeField, HideInInspector]
        protected bool m_ApplyHeadPosition = true;

        public enum CameraMode
        {
            FirstPerson,
            ThirdPperson,
        }
        public CameraMode _cameraMode = CameraMode.FirstPerson;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickXRRig _xrRig = null;
        
        protected float _maxHipsToHeadDistance = 0;

        protected PositionConstraint _footprints = null;

        protected List<ControlType> _ikControls
        {
            get
            {
                if (m_IKControls == null || m_IKControls.Count == 0)
                {
                    m_IKControls = new List<ControlType>();
                    for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
                    {
                        m_IKControls.Add(ControlType.Tracking);
                    }
                }

                return m_IKControls;
            }
        }
        [SerializeField, HideInInspector]
        protected List<ControlType> m_IKControls;

        protected List<Vector3> _ikTrackingOffset
        {
            get
            {
                if (m_IKTrackingUpdateMode == null || m_IKTrackingUpdateMode.Count == 0)
                {
                    m_IKTrackingUpdateMode = new List<Vector3>(new Vector3[(int)IKBone.LastBone]); ;
                }

                return m_IKTrackingUpdateMode;
            }
        }
        [SerializeField, HideInInspector]
        protected List<Vector3> m_IKTrackingUpdateMode;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _animator.applyRootMotion = false;
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            _maxHipsToHeadDistance = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.Hips).position, _animator.GetBoneTransform(HumanBodyBones.Head).position);

            _xrRig = QuickSingletonManager.GetInstance<QuickXRRig>();

            CreateFootPrints();
        }

        protected override void CreateIKSolvers()
        {
            base.CreateIKSolvers();

            for (IKBone ikBoneFinger = IKBone.LeftThumbDistal; ikBoneFinger <= IKBone.RightLittleDistal; ikBoneFinger++)
            {
                GetIKSolver(ikBoneFinger)._updateIKHintPos = false;
            }
        }

        protected virtual void Start()
        {
            _vrManager.AddUnityVRTrackingSystem(this);

            if (_gestureSettingsLeftHand == null)
            {
                _gestureSettingsLeftHand = LoadHandGestureSettings(true);
            }
            if (_gestureSettingsRightHand == null)
            {
                _gestureSettingsRightHand = LoadHandGestureSettings(false);
            }
        }

        protected virtual QuickHandGestureSettings LoadHandGestureSettings(bool isLeft)
        {
            string s = "";
            const string infix = "_HandGestureSettings_";

            switch (QuickVRManager._hmdModel)
            {
                case QuickVRManager.HMDModel.Quest:
                case QuickVRManager.HMDModel.Quest2:
                case QuickVRManager.HMDModel.QuestPro:
                    s = "Touch";
                    break;

                default:
                    s = "Default";
                    break;
            }

            return Resources.Load<QuickHandGestureSettings>("HandGestureSettings/" + s + infix + (isLeft ? "Left" : "Right"));
        }

        protected virtual void CreateFootPrints()
        {
            _footprints = Instantiate<GameObject>(Resources.Load<GameObject>("Footprints/Footprints")).GetOrCreateComponent<PositionConstraint>();
            _footprints.transform.ResetTransformation();
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = transform;
            source.weight = 1.0f;
            _footprints.AddSource(source);
            _footprints.constraintActive = true;
        }

        #endregion

        #region GET AND SET

        public virtual ControlType GetIKControl(IKBone ikBone)
        {
            return _ikControls[(int)ikBone];
        }

        public virtual ControlType GetIKControl(QuickHumanFingers f, bool isLeft)
        {
            ControlType result = ControlType.Tracking;

            if (f == QuickHumanFingers.Thumb)
            {
                result = GetIKControl(isLeft ? IKBone.LeftThumbDistal : IKBone.RightThumbDistal);
            }
            else if (f == QuickHumanFingers.Index)
            {
                result = GetIKControl(isLeft ? IKBone.LeftIndexDistal : IKBone.RightIndexDistal);
            }
            else if (f == QuickHumanFingers.Middle)
            {
                result = GetIKControl(isLeft ? IKBone.LeftMiddleDistal : IKBone.RightMiddleDistal);
            }
            else if (f == QuickHumanFingers.Ring)
            {
                result = GetIKControl(isLeft ? IKBone.LeftRingDistal : IKBone.RightRingDistal);
            }
            else if (f == QuickHumanFingers.Little) 
            {
                result = GetIKControl(isLeft ? IKBone.LeftLittleDistal : IKBone.RightLittleDistal);
            }

            return result;
        }

        public virtual Vector3 GetIKTrackingOffset(HumanBodyBones boneID)
        {
            return GetIKTrackingOffset(ToIKBone(boneID));
        }

        public virtual Vector3 GetIKTrackingOffset(IKBone ikBone)
        {
            return _ikTrackingOffset[(int)ikBone];
        }

        public virtual void SetIKControl(IKBone ikBone, ControlType cType)
        {
            _ikControls[(int)ikBone] = cType;
        }

        public virtual void SetIKTrackingOffset(IKBone ikBone, Vector3 offset)
        {
            _ikTrackingOffset[(int)ikBone] = offset;
        }

        protected virtual void ResetTrackingOffsets()
        {
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                _ikTrackingOffset[(int)ikBone] = Vector3.zero;
            }
        }

        public override void Calibrate()
        {
            base.Calibrate();

            ResetTrackingOffsets();

            transform.localScale = Vector3.one;

            _footprints.translationOffset = Vector3.zero;
            _footprints.transform.rotation = transform.rotation;
        }

        #endregion

        #region UPDATE

        protected override void LateUpdate()
        {
            
        }

        public virtual void UpdateIKTargets()
        {
            if (Application.isPlaying)
            {
                //1) Update all the IKTargets taking into consideration its ControlType. 
                for (IKBone ikBone = IKBone.Hips; ikBone < IKBone.LastBone; ikBone++)
                {
                    ControlType cType = GetIKControl(ikBone);
                    HumanBodyBones boneID = ToHumanBodyBones(ikBone);
                    QuickIKSolver ikSolver = GetIKSolver(ikBone);
                    ikSolver._enableIK = cType != ControlType.Animation;

                    if (cType == ControlType.Tracking)
                    {
                        QuickVRNode node = _xrRig.GetVRNode(boneID);
                        if (node._isTrackedOrInferred)
                        {
                            //Update the IKTarget with the information of the corresponding QuickVRNode
                            if (boneID == HumanBodyBones.LeftEye || boneID == HumanBodyBones.RightEye) continue;

                            ikSolver._targetLimb.position = node.GetTrackedObject().transform.position;
                            ikSolver._targetLimb.rotation = node.GetTrackedObject().transform.rotation;

                            if (boneID == HumanBodyBones.Head)
                            {
                                QuickIKSolver ikSolverHead = GetIKSolver(IKBone.Head);
                                if (!_applyHeadPosition)
                                {
                                    ikSolverHead._weightIKPos = 0;
                                }
                                if (!_applyHeadRotation)
                                {
                                    ikSolverHead._weightIKRot = 0;
                                }
                                if (GetIKControl(IKBone.Hips) == ControlType.IK)
                                {
                                    Vector3 n = (ikSolver._targetLimb.position - ikSolver._boneUpper.position).normalized;
                                    ikSolver._targetLimb.position = ikSolver._boneUpper.position + n * ikSolver.GetChainLength();
                                }
                            }
                            else if (boneID == HumanBodyBones.LeftEye || boneID == HumanBodyBones.RightEye)
                            {
                                QuickIKSolverEye ikSolverEye = (QuickIKSolverEye)ikSolver;
                                ikSolverEye._weightBlink = ((QuickVRNodeEye)node).GetBlinkFactor();
                            }
                        }
                        //else
                        //{
                        //    //Keep the position and rotation that comes from the animation. 
                        //    //if (ikSolver._boneLimb)
                        //    //{
                        //    //    ikSolver._targetLimb.position = ikSolver._boneLimb.position;
                        //    //}
                        //    //ikSolver._targetLimb.GetChild(0).rotation = _animator.GetBoneTransform(boneID).rotation;
                        //}
                    }
                }

                _footprints.gameObject.SetActive(_useFootprints);
            }
        }

        protected virtual void ApplyFingerRotation(QuickHumanBodyBones fingerBoneID, QuickHumanBodyBones fingerBoneNextID)
        {
            Transform bone0 = _animator.GetBoneTransform(fingerBoneID);
            Transform bone1 = _animator.GetBoneTransform(fingerBoneNextID);
            Transform n0 = _xrRig.GetVRNode(fingerBoneID).GetTrackedObject().transform;
            Transform n1 = _xrRig.GetVRNode(fingerBoneNextID).GetTrackedObject().transform;

            bone0.Align(bone1, n0, n1);
        }

        protected override void UpdateIKFingers()
        {
            if (_xrRig)
            {
                foreach (bool isLeft in new bool[] { true, false })
                {
                    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                    {
                        if (GetIKControl(f, isLeft) == ControlType.Tracking)
                        {
                            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
                            Transform n0 = _xrRig.GetVRNode(fingerBones[0]).GetTrackedObject().transform;
                            Transform n1 = _xrRig.GetVRNode(fingerBones[1]).GetTrackedObject().transform;
                            Transform n2 = _xrRig.GetVRNode(fingerBones[2]).GetTrackedObject().transform;

                            Vector3 v = (n1.position - n0.position).normalized;
                            Vector3 w = (n2.position - n1.position).normalized;

                            QuickIKSolver ikSolver = GetIKSolver((HumanBodyBones)fingerBones[2]);
                            Vector3 posBoneMid = ikSolver._boneUpper.position + v * ikSolver.GetUpperLength();
                            ikSolver._targetLimb.position = posBoneMid + w * ikSolver.GetMidLength();
                            ikSolver._targetLimb.rotation = n2.rotation;
                            ikSolver._targetHint.position = posBoneMid + n1.up * 0.05f;

                            //ApplyFingerRotation(fingerBones[0], fingerBones[1]);
                            //ApplyFingerRotation(fingerBones[1], fingerBones[2]);
                            //ApplyFingerRotation(fingerBones[2], fingerBones[3]);

                            //Transform tBoneLimb = _animator.GetBoneTransformNormalized(fingerBones[2]);
                            //Transform tBoneMid = _animator.GetBoneTransformNormalized(fingerBones[1]);
                            //ikSolver._targetLimb.position = tBoneLimb.position;
                            //ikSolver._targetLimb.rotation = tBoneLimb.rotation;
                            //ikSolver._targetHint.position =  tBoneMid.position + tBoneMid.up * 0.05f;
                        }
                    }
                }

            }

            base.UpdateIKFingers();
        }
        
        #endregion

    }

}
