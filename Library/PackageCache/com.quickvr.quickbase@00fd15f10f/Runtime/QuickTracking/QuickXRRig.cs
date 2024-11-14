using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using QuickVR.Interaction;
using QuickVR.Interaction.Locomotion;

namespace QuickVR
{

    public class QuickXRRig : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public static QuickXRRig _instance = null;

        public Transform _trackingOffset
        {
            get; protected set;
        }

        public Transform _cameraRoot
        {
            get; protected set;
        }

        public Transform _cameraOffset
        {
            get; protected set;
        }

        [Header("VR Camera")]
        public LayerMask _visibleLayers = -1;	//The layers that will be rendered by the cameras of the head tracking system. 

        public Camera _pfCamera = null;
        public float _cameraNearPlane = DEFAULT_NEAR_CLIP_PLANE;
        public float _cameraFarPlane = DEFAULT_FAR_CLIP_PLANE;

        public float _cameraMonoSpeed = 2.0f;
        
        #endregion

        #region PROTECTED ATTRIBUTES

        protected Dictionary<QuickHumanBodyBones, QuickVRNode> _vrNodes = new Dictionary<QuickHumanBodyBones, QuickVRNode>();
        protected QuickVRNode _vrNodeCamera = null;

        protected static Camera _camera = null;

        protected Animator _animatorSource
        {
            get
            {
                return QuickVRManager._instance.GetAnimatorSource();
            }
        }

        protected Animator _animatorTarget
        {
            get
            {
                return QuickVRManager._instance.GetAnimatorTarget();
            }
        }

        protected Vector3 _customUserForward = Vector3.zero;  //A custom user forward provided by the application. 

        protected Dictionary<HumanBodyBones, float> _fingerLength = new Dictionary<HumanBodyBones, float>();

        protected Vector3 _calibrationMoveOriginLS = Vector3.zero;
        protected float _headHeight = 0;

        public Transform _ikBoneSpineRoot
        {
            get
            {
                if (!m_IKBoneSpineRoot)
                {
                    m_IKBoneSpineRoot = transform.CreateChild("__IKBoneSpineRoot__");
                }

                return m_IKBoneSpineRoot;
            }
        }
        protected Transform m_IKBoneSpineRoot = null;

        protected Transform _ikBoneSpineUpper
        {
            get
            {
                if (!m_IKBoneSpineUpper)
                {
                    m_IKBoneSpineUpper = _ikBoneSpineRoot.transform.CreateChild("__IKBoneSpineUpper__");
                }

                return m_IKBoneSpineUpper;
            }
        }
        protected Transform m_IKBoneSpineUpper = null;

        protected Transform _ikBoneSpineMid
        {
            get
            {
                if (!m_IKBoneSpineMid)
                {
                    m_IKBoneSpineMid = _ikBoneSpineUpper.CreateChild("__IKBoneSpineMid__");
                }

                return m_IKBoneSpineMid;
            }
        }
        protected Transform m_IKBoneSpineMid = null;

        protected Transform _ikBoneSpineLimb
        {
            get
            {
                if (!m_IKBoneSpineLimb)
                {
                    m_IKBoneSpineLimb = _ikBoneSpineMid.CreateChild("__IKBoneSpineLimb__");
                }

                return m_IKBoneSpineLimb;
            }
        }
        protected Transform m_IKBoneSpineLimb = null;

        public QuickIKSolver _ikSolverSpine
        {
            get; protected set;
        }

        protected float _offsetH = 0;
        protected float _offsetV = 0;

        protected QuickVRHandAnimatorBase _handAnimatorLeftHand = null;
        protected QuickVRHandAnimatorBase _handAnimatorRightHand = null;

        #endregion

        #region CONSTANTS

        public const float DEFAULT_NEAR_CLIP_PLANE = 0.05f;
        public const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

        //Rotation limits for CameraMono
        const float MAX_HORIZONTAL_ANGLE = 80;
        const float MAX_VERTICAL_ANGLE = 45;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _instance = this;

            _trackingOffset = transform.CreateChild("__TrackingOffset__");
            _cameraRoot = transform.CreateChild("__CameraRoot__");
            _cameraOffset = _cameraRoot.CreateChild("__CameraOffset__");

            CreateVRNodes();
            _handAnimatorLeftHand = CreateHandAnimator(true);
            _handAnimatorRightHand = CreateHandAnimator(false);

            for (HumanBodyBones boneID = HumanBodyBones.LeftThumbProximal; boneID <= HumanBodyBones.RightLittleProximal; boneID++)
            {
                _fingerLength[boneID] = 0;
            }

            //Create the IKSolver for the spine
            _ikSolverSpine = gameObject.GetOrCreateComponent<QuickIKSolver>();

            //Init the bones
            _ikSolverSpine._boneUpper = _ikBoneSpineUpper;
            _ikSolverSpine._boneMid = _ikBoneSpineMid;
            _ikSolverSpine._boneLimb = _ikBoneSpineLimb; //_xrRig.GetVRNode(HumanBodyBones.Head).GetTrackedObject().transform;

            //Configure the IKTarget and other properties
            _ikSolverSpine._targetLimb.parent = GetVRNode(HumanBodyBones.Head).GetTrackedObject().transform;
            _ikSolverSpine._targetLimb.ResetTransformation();
            _ikSolverSpine._updateIKHintPos = false;
            _ikSolverSpine.enabled = false;
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPreCalibrate += ActionOnPrecalibrate;
            QuickVRManager.OnPostCalibrate += ActionOnPostCalibrate;
            QuickVRManager.OnSourceAnimatorSet += ActionOnSourceAnimatorSet;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPreCalibrate -= ActionOnPrecalibrate;
            QuickVRManager.OnPostCalibrate -= ActionOnPostCalibrate;
            QuickVRManager.OnSourceAnimatorSet -= ActionOnSourceAnimatorSet;
        }

        private void ActionOnSourceAnimatorSet(Animator animator)
        {
            if (!Camera.main)
            {
                Camera camera = _pfCamera ? Instantiate(_pfCamera) : new GameObject().GetOrCreateComponent<Camera>();
                camera.name = "__Camera__";
                camera.tag = "MainCamera";
                camera.gameObject.GetOrCreateComponent<FlareLayer>();
            }

            _camera = Camera.main;
            _camera.transform.parent = _vrNodeCamera.transform;
            _camera.transform.ResetTransformation();

            _camera.GetOrCreateComponent<AudioListener>();

            if (!QuickVRManager.IsXREnabled())
            {
                _camera.fieldOfView = 70.0f;//90.0f;
            }
        }

        private void ActionOnPrecalibrate()
        {
            _offsetH = 0;
            _offsetV = 0;

            if (_animatorSource != null)
            {
                Transform tBone = _animatorSource.GetBoneTransformNormalized(HumanBodyBones.Head);
                Vector3 headOffset = tBone.position - _animatorSource.GetEyeCenterVR().position;
                QuickVRNode vrNodeHead = GetVRNode(HumanBodyBones.Head);
                vrNodeHead.GetTrackedObject().transform.localPosition = tBone.InverseTransformVector(headOffset);
                _vrNodeCamera.GetTrackedObject().transform.localPosition = vrNodeHead.GetTrackedObject().transform.localPosition;
            }
        }

        private void ActionOnPostCalibrate()
        {
            QuickVRNode vrNodeHead = GetVRNode(HumanBodyBones.Head);
            Vector3 p = vrNodeHead.GetTrackedObject().transform.position;
            _calibrationMoveOriginLS = transform.InverseTransformPoint(new Vector3(p.x, 0, p.z));
            _headHeight = transform.InverseTransformPoint(p).y;

            CalibrateIKSolverSpine();
        }

        protected virtual void CalibrateIKSolverSpine()
        {
            Vector3 tmpPos = transform.position;
            Quaternion tmpRot = transform.rotation;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            //Reset the bones for the spine pose calculation. 
            //_ikBoneSpineUpper.position = Vector3.Lerp(
            //    _xrRig.GetVRNode(HumanBodyBones.LeftLowerLeg).transform.position,
            //    _xrRig.GetVRNode(HumanBodyBones.RightLowerLeg).transform.position,
            //    0.5f);
            Vector3 p0 = GetVRNode(HumanBodyBones.LeftLowerLeg).transform.position;
            Vector3 p1 = GetVRNode(HumanBodyBones.RightLowerLeg).transform.position;

            Vector3 n = (p1 - p0).normalized;
            Vector3 v = GetVRNode(HumanBodyBones.Hips).GetTrackedObject().transform.position - p0;
            _ikBoneSpineRoot.position = Vector3.ProjectOnPlane(p0 + Vector3.Project(v, n), transform.right);
            _ikBoneSpineRoot.rotation = transform.rotation;
            //_lastRootPosLS = _ikBoneSpineRoot.localPosition;

            _ikBoneSpineUpper.ResetTransformation();
            _ikBoneSpineMid.ResetTransformation();
            _ikBoneSpineMid.position = GetVRNode(HumanBodyBones.Hips).transform.position;
            _ikBoneSpineLimb.ResetTransformation();
            _ikBoneSpineLimb.position = GetVRNode(HumanBodyBones.Head).GetTrackedObject().transform.position;

            //Reset the QuickIKSolver for the spine. 
            _ikSolverSpine._targetHint.parent = _ikBoneSpineRoot.parent;
            _ikSolverSpine.RecomputeBonesLength();

            transform.position = tmpPos;
            transform.rotation = tmpRot;
        }

        protected virtual void CreateVRNodes()
        {
            //Create a VRNode for each of the QuickHumanBodyBones
            for (QuickHumanBodyBones role = 0; role != QuickHumanBodyBones.LastBone; role++)
            {
                _vrNodes[role] = CreateVRNode(role, _trackingOffset);
            }

            //Create a VRNode specifically for the Camera
            _vrNodeCamera = CreateVRNode(QuickHumanBodyBones.Head, _cameraOffset);
        }

        protected virtual QuickVRNode CreateVRNode(QuickHumanBodyBones role, Transform nodeParent)
        {
            Transform tNode = nodeParent.CreateChild("VRNode" + role.ToString());
            QuickVRNode n = null;

            if (role == QuickHumanBodyBones.LeftEye || role == QuickHumanBodyBones.RightEye)
            {
                n = tNode.GetOrCreateComponent<QuickVRNodeEye>();
            }
            else
            {
                n = tNode.GetOrCreateComponent<QuickVRNode>();
            }

            n.SetRole(role);

            return n;
        }

        protected QuickVRHandAnimatorBase CreateHandAnimator(bool isLeft)
        {
            QuickVRNode vrNodeHand = GetVRNode(isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            QuickVRHandAnimatorBase result = Instantiate(Resources.Load<QuickVRHandAnimatorBase>("Prefabs/" + (isLeft ? "pf_HandLeft" : "pf_HandRight")), vrNodeHand.transform);
            result.transform.localPosition = Vector3.zero;

            foreach (SkinnedMeshRenderer r in result.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMesh = null;
            }

            return result;
        }

        #endregion

        #region GET AND SET

        //public static Matrix4x4 ComputeHeadMatrix()
        //{
        //    QuickVRNode vrNodeHead = _instance.GetVRNode(HumanBodyBones.Head);
        //    Vector3 fwd = Vector3.ProjectOnPlane(vrNodeHead.transform.forward, _instance.transform.up);
        //    Quaternion rot = Quaternion.LookRotation(fwd, _instance.transform.up);
        //    Matrix4x4 mat = Matrix4x4.TRS(vrNodeHead.GetTrackedObject().transform.position, rot, Vector3.one);

        //    return mat;
        //}

        public virtual QuickVRNode GetVRNodeCamera()
        {
            return _vrNodeCamera;
        }

        public virtual QuickVRNode GetVRNode(HumanBodyBones role)
        {
            return GetVRNode((QuickHumanBodyBones)role);
        }

        public virtual QuickVRNode GetVRNode(QuickHumanBodyBones role)
        {
            _vrNodes.TryGetValue(role, out QuickVRNode result);
            return result;
        }

        public virtual Vector3 GetUserForward()
        {
            if (_customUserForward == Vector3.zero)
            {
                return Vector3.ProjectOnPlane(GetVRNodeMain().transform.forward, transform.up);
            }
            return _customUserForward;
        }

        public virtual void SetUserForward(Vector3 fwd)
        {
            _customUserForward = fwd;
        }

        public virtual void ResetUserForward()
        {
            _customUserForward = Vector3.zero;
        }

        public virtual QuickVRNode GetVRNodeMain()
        {
            QuickVRNode nodeHips = GetVRNode(HumanBodyBones.Hips);
            QuickVRNode nodeHead = GetVRNode(HumanBodyBones.Head);

            return nodeHips && nodeHips._isTracked ? nodeHips : nodeHead;
        }

        public virtual void Calibrate()
        {
            foreach (var pair in _vrNodes)
            {
                pair.Value.UpdateState();
            }
            _vrNodeCamera.UpdateState();

            _trackingOffset.ResetTransformation();

            //Rotate the rig so it is aligned with the Animator
            _trackingOffset.Align(Vector3.ProjectOnPlane(GetUserForward(), transform.up).normalized, _animatorSource.transform.forward);

            //Force the head of the AnimatorSource to be aligned with the VRNodeHead
            QuickVRNode vrNodeHead = GetVRNode(HumanBodyBones.Head);
            Transform tBoneHeadNormalized = _animatorSource.GetBoneTransformNormalized(QuickHumanBodyBones.Head);
            _animatorSource.GetBoneTransform(HumanBodyBones.Head).AlignAround(tBoneHeadNormalized.position, tBoneHeadNormalized.forward, vrNodeHead.transform.forward);

            //Position the rig so the VRHead is aligned with the EyeCenter
            Vector3 pos = vrNodeHead.GetTrackedObject().transform.position;
            Vector3 targetPos = _animatorSource.GetBoneTransform(HumanBodyBones.Head).position;

            _trackingOffset.position += targetPos - pos;

            //Align the cameraOffset to match with the position of the Camera
            AlignVRCamera();

            //Calibrate each of the VRNodes
            foreach (var pair in _vrNodes)
            {
                CalibrateVRNode(pair.Value, _animatorSource);
            }
            CalibrateVRNode(_vrNodeCamera, _animatorTarget);
        }

        public virtual void AlignVRCamera()
        {
            _cameraRoot.ResetTransformation();
            _cameraOffset.position = _trackingOffset.position;
            _cameraOffset.rotation = _trackingOffset.rotation;

            //Save the current human pose
            HumanPose tmpPoseSrc = new HumanPose();
            QuickHumanPoseHandler.GetHumanPose(_animatorSource, ref tmpPoseSrc);
            HumanPose tmpPoseDst = new HumanPose();
            QuickHumanPoseHandler.GetHumanPose(_animatorTarget, ref tmpPoseDst);

            _animatorSource.EnforceTPose();
            _animatorTarget.EnforceTPose();

            _cameraOffset.position += _animatorTarget.GetEyeCenterVR().position - _animatorSource.GetEyeCenterVR().position;

            //Restore the human poses
            QuickHumanPoseHandler.SetHumanPose(_animatorSource, ref tmpPoseSrc);
            QuickHumanPoseHandler.SetHumanPose(_animatorTarget, ref tmpPoseDst);
        }

        protected virtual void CalibrateVRNode(QuickVRNode n, Animator animator)
        {
            QuickHumanBodyBones boneID = n.GetRole();
            Transform tBone = animator.GetBoneTransformNormalized(boneID);
            if (tBone)
            {
                n.UpdateState();
                if (!n._isTracked)
                {
                    if (boneID == QuickHumanBodyBones.Head)
                    {
                        n.transform.position = animator.GetEyeCenterVR().position;
                        n.transform.rotation = animator.transform.rotation;
                    }
                    else
                    {
                        n.transform.position = tBone.position;
                        n.transform.rotation = tBone.rotation;
                    }
                }

                QuickTrackedObject tObject = n.GetTrackedObject();
                if (boneID != QuickHumanBodyBones.Head) tObject.transform.ResetTransformation();

                //Compute the position offset of the TrackedObject
                if (boneID == QuickHumanBodyBones.Hips || boneID == QuickHumanBodyBones.LeftFoot || boneID == QuickHumanBodyBones.RightFoot)
                {
                    tObject.transform.position = tBone.position;
                }
                
                //Compute the rotation offset of the TrackedObject
                if (boneID == QuickHumanBodyBones.LeftFoot || boneID == QuickHumanBodyBones.RightFoot)
                {
                    tObject.transform.rotation = animator.transform.rotation;
                }

                //Save the current pose
                tObject.Reset();
            }
        }

        protected virtual bool IsNodeLeftSide(QuickVRNode vrNode)
        {
            QuickVRNode nodeHead = GetVRNode(HumanBodyBones.Head);
            Vector3 fwd = Vector3.ProjectOnPlane(nodeHead.transform.forward, transform.up);
            Vector3 v = Vector3.ProjectOnPlane(vrNode.transform.position - nodeHead.transform.position, transform.up);

            return Vector3.SignedAngle(fwd, v, transform.up) < 0;
        }

        public virtual float GetFingerLength(QuickHumanFingers f, bool isLeft)
        {
            List<QuickHumanBodyBones> boneFingers = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
            HumanBodyBones boneID = (HumanBodyBones)boneFingers[0];
            if (_fingerLength[boneID] == 0)
            {
                QuickVRNode n0 = GetVRNode(boneFingers[0]);
                QuickVRNode n1 = GetVRNode(boneFingers[1]);
                QuickVRNode n2 = GetVRNode(boneFingers[2]);

                if (n0._isTrackedOrInferred && n1._isTrackedOrInferred && n2._isTrackedOrInferred)
                {
                    _fingerLength[boneID] = Vector3.Distance(n0.transform.position, n1.transform.position) + Vector3.Distance(n1.transform.position, n2.transform.position);
                }
            }

            return _fingerLength[boneID];
        }

        //public virtual bool IsVRNodesSwaped(HumanBodyBones typeNodeLeft, HumanBodyBones typeNodeRight, bool doSwaping = true)
        //{
        //    return IsVRNodesSwaped(GetVRNode(typeNodeLeft), GetVRNode(typeNodeRight), doSwaping);
        //}

        //public virtual bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        //{
        //    bool result = false;

        //    QuickVRNode hmdNode = GetVRNode(HumanBodyBones.Head);
        //    if (nodeLeft._isTracked && nodeRight._isTracked)
        //    {
        //        float dLeft = Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right);
        //        float dRight = Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right);

        //        result = dLeft > dRight;
        //        if (result && doSwaping)
        //        {
        //            SwapQuickVRNode(nodeLeft, nodeRight);
        //        }
        //    }

        //    return result;
        //}

        //protected virtual void SwapQuickVRNode(QuickVRNode vrNodeA, QuickVRNode vrNodeB)
        //{
        //    TrackedDevice deviceA = vrNodeA._inputDevice;
        //    vrNodeA._inputDevice = vrNodeB._inputDevice;
        //    vrNodeB._inputDevice = deviceA;
        //}

        #endregion

        #region UPDATE

        public virtual void UpdateRig()
        {
            foreach (var pair in _vrNodes)
            {
                QuickVRNode vrNode = pair.Value;
                vrNode.UpdateState();
            }
            _vrNodeCamera.UpdateState();

            UpdateRoot();

            //Estimate the tracking data of those nodes that are not tracked and 
            //its tracking data can be estimated from other nodes or input systems. 
            EstimateVRNodeHead();
            EstimateVRNodeHips();
            EstimateVRNodeHand(GetVRNode(HumanBodyBones.LeftHand));
            EstimateVRNodeHand(GetVRNode(HumanBodyBones.RightHand));
            EstimateVRNodeFoot(GetVRNode(HumanBodyBones.LeftFoot));
            EstimateVRNodeFoot(GetVRNode(HumanBodyBones.RightFoot));
        }

        protected virtual void UpdateRoot()
        {
            QuickVRNode vrNodeHead = GetVRNode(HumanBodyBones.Head);
            if (QuickVRInteractionManager._instance._locomotionDirectTurnEnabled)
            {
                Vector3 targetFwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, transform.up);
                float rotAngle = Vector3.SignedAngle(transform.forward, targetFwd, transform.up);

                //if (Mathf.Abs(rotAngle) > 1)
                {
                    Vector3 pivotPoint = vrNodeHead.GetTrackedObject().transform.position;
                    transform.RotateAround(pivotPoint, transform.up, rotAngle);
                    _trackingOffset.RotateAround(pivotPoint, transform.up, -rotAngle);
                    _cameraRoot.RotateAround(pivotPoint, transform.up, -rotAngle);

                    //_trackingOffset.parent = null;
                    //_cameraRoot.parent = null;

                    //QuickContinuousTurnProvider lProvider = QuickVRInteractionManager._instance.GetLocomotionProvider<QuickContinuousTurnProvider>();
                    //lProvider.UpdateLocomotion(rotAngle);

                    //_trackingOffset.parent = transform;
                    //_cameraRoot.parent = transform;
                }
            }

            if (QuickVRInteractionManager._instance._locomotionDirectMoveEnabled)
            {
                Vector3 p = transform.InverseTransformPoint(vrNodeHead.GetTrackedObject().transform.position);
                if (Vector3.Distance(p, _calibrationMoveOriginLS) >= _headHeight * 0.99f)
                {
                    Vector3 originOffsetWS = transform.TransformDirection(Vector3.ProjectOnPlane(p - _calibrationMoveOriginLS, transform.up));
                    _trackingOffset.position -= originOffsetWS;
                    _cameraRoot.position -= originOffsetWS;

                    QuickContinuousMoveProvider lProvider = QuickVRInteractionManager._instance.GetLocomotionProvider<QuickContinuousMoveProvider>();
                    Vector3 originOffsetLS = transform.InverseTransformDirection(originOffsetWS);
                    Vector2 moveDir = new Vector2(originOffsetLS.x, originOffsetLS.z);

                    lProvider.AddInputData(moveDir.normalized, moveDir.magnitude / Time.deltaTime);
                }
            }

            QuickVRInteractionManager._instance.UpdateLocomotionProviders();
        }

        protected virtual void EstimateVRNodeHead()
        {
            QuickVRNode vrNodeHead = GetVRNode(QuickHumanBodyBones.Head);
            if (!vrNodeHead._isTracked)
            {
                if (QuickVRManager._instance.IsCalibrated() && !InputManagerKeyboard.GetKey(Key.LeftCtrl))
                {
                    float x = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
                    float y = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
                    _offsetH += _cameraMonoSpeed * x;
                    _offsetV -= _cameraMonoSpeed * y;

                    _offsetH = Mathf.Clamp(_offsetH, -MAX_HORIZONTAL_ANGLE, MAX_HORIZONTAL_ANGLE);
                    _offsetV = Mathf.Clamp(_offsetV, -MAX_VERTICAL_ANGLE, MAX_VERTICAL_ANGLE);
                }

                EstimateVRNodeHead(vrNodeHead);
                EstimateVRNodeHead(_vrNodeCamera);
            }
        }

        protected virtual void EstimateVRNodeHead(QuickVRNode vrNode)
        {
            Vector3 pivotPos = vrNode.GetTrackedObject().transform.position;
            vrNode.transform.RotateAround(pivotPos, transform.up, _offsetH);
            vrNode.transform.RotateAround(pivotPos, vrNode.transform.right, _offsetV);

            vrNode._state = QuickVRNode.State.Inferred;
        }

        protected virtual void EstimateVRNodeHips()
        {
            QuickVRNode vrNodeHips = GetVRNode(HumanBodyBones.Hips);
            if (!vrNodeHips._isTracked)
            {
                _ikSolverSpine._targetHint.position = _ikSolverSpine._boneUpper.position - transform.forward * 1.0f;
                _ikSolverSpine.UpdateIK();
                vrNodeHips.transform.position = _ikSolverSpine._boneMid.position;
                vrNodeHips.transform.rotation = _ikSolverSpine._boneMid.rotation;

                vrNodeHips._state = QuickVRNode.State.Inferred;
            }
        }

        protected virtual void EstimateVRNodeHand(QuickVRNode vrNodeHand)
        {
            if (QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Controllers)
            {
                QuickTrackedObject tObject = vrNodeHand.GetTrackedObject();
                QuickVRHandAnimatorBase handAnimator = vrNodeHand.GetRole() == QuickHumanBodyBones.LeftHand ? _handAnimatorLeftHand : _handAnimatorRightHand;
                QuickVRHandOffset handOffset = handAnimator.GetComponent<QuickVRHandOffset>();
                tObject.transform.position = handOffset._handOrigin.position;
                tObject.transform.rotation = handOffset._handOrigin.rotation;
                UpdateFingersController(handAnimator);
            }
        }

        protected virtual void UpdateFingersController(QuickVRHandAnimatorBase handAnimator)
        {
            //Update the nodes of the fingers
            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, handAnimator._isLeft);
                for (int i = 0; i < QuickHumanTrait.NUM_BONES_PER_FINGER; i++)
                {
                    QuickVRNode nFinger = GetVRNode(fingerBones[i]);

                    //The finger is tracked.
                    Transform t = handAnimator[(int)f][i]; // .GetBoneFingerTransform(f, i);
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

                    nFinger._state = QuickVRNode.State.Inferred;
                }
            }
        }

        protected virtual void EstimateVRNodeFoot(QuickVRNode vrNodeFoot)
        {
            if (!vrNodeFoot._isTracked)
            {
                QuickLocomotionTracker locomotionTracker = _animatorSource.GetComponent<QuickLocomotionTracker>();
                //if (locomotionTracker && locomotionTracker._speed > 0.001f)
                {
                    Transform tBoneFoot = _animatorSource.GetBoneTransformNormalized(vrNodeFoot.GetRole());

                    //Update foot position
                    vrNodeFoot.transform.position = tBoneFoot.position;

                    //Update foot rotation
                    vrNodeFoot.transform.rotation = tBoneFoot.rotation;
                }
                //else
                //{
                //    vrNodeFoot.transform.localPosition = vrNodeFoot._initialLocalPosition;
                //    vrNodeFoot.transform.localRotation = vrNodeFoot._initialLocalRotation;
                //}

                vrNodeFoot._state = QuickVRNode.State.Inferred;
            }
        }

        public virtual void UpdateXRCamera()
        {
            foreach (Camera cam in Camera.allCameras)
            {
                cam.nearClipPlane = _cameraNearPlane;
                cam.farClipPlane = _cameraFarPlane;
                cam.cullingMask = _visibleLayers.value;
            }

            //_vrNodeCamera.UpdateState();
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            //Gizmos.color = Color.black;
            //Gizmos.DrawSphere(transform.TransformPoint(ComputeMoveOriginLS()), 0.1f);

            DebugExtension.DrawCoordinatesSystem(transform.position, transform.right, transform.up, transform.forward, 0.1f);

            //Gizmos.color = Color.green;
            foreach (var pair in _vrNodes)
            {
                QuickVRNode n = pair.Value;
                //if (n.IsTracked())
                {
                    DebugExtension.DrawCoordinatesSystem(n.transform.position, n.transform.right, n.transform.up, n.transform.forward, 0.05f);

                    if (n._state == QuickVRNode.State.NotTracked)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if (n._state == QuickVRNode.State.Tracked)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                    }

                    float s = 0.0125f;
                    Vector3 cSize = Vector3.one * s;

                    Gizmos.matrix = n.transform.localToWorldMatrix;
                    Gizmos.DrawCube(Vector3.zero, cSize);
                    QuickTrackedObject tObject = n.GetTrackedObject();
                    if (tObject.transform.localPosition != Vector3.zero)
                    {
                        Gizmos.DrawSphere(tObject.transform.localPosition, s * 0.5f);
                        Gizmos.DrawLine(Vector3.zero, tObject.transform.localPosition);
                    }
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }

            //Gizmos.color = Color.cyan;
            //QuickVRNode vrNodeHead = GetVRNode(HumanBodyBones.Head);
            //Vector3 fwd = Vector3.ProjectOnPlane(vrNodeHead.transform.forward, transform.up);
            //Quaternion rot = Quaternion.LookRotation(fwd, transform.up);
            //Matrix4x4 mat = Matrix4x4.TRS(vrNodeHead.transform.position, rot, Vector3.one);

            //Gizmos.DrawLine(vrNodeHead.transform.position, mat.MultiplyPoint(_headToHipsOffsetLS));
        }

        #endregion

    }

}


