﻿using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

using QuickVR.Interaction;
using System.Collections.Generic;

namespace QuickVR
{

    [DefaultExecutionOrder(-1000)]
    public class QuickVRManager : MonoBehaviour
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            XRSettings.gameViewRenderMode = GameViewRenderMode.RightEye;
        }

        //#region EXECUTION ORDER MANAGERS

        //[DefaultExecutionOrder(-1000)] //Execute as soon as possible
        //protected class QuickVRManagerEarly : MonoBehaviour
        //{
        //    public static event QuickVRManagerAction OnUpdate;

        //    protected virtual void Update()
        //    {
        //        if (OnUpdate != null)
        //        {
        //            OnUpdate();
        //        }
        //    }
        //}

        //[DefaultExecutionOrder(1000)] //Execute as late as possible
        //protected class QuickVRManagerLate : MonoBehaviour
        //{
        //    public static event QuickVRManagerAction OnLateUpdate;

        //    protected virtual void LateUpdate()
        //    {
        //        if (OnLateUpdate != null)
        //        {
        //            OnLateUpdate();
        //        }
        //    }
        //}

        //#endregion

        #region PUBLIC ATTRIBUTES

        public enum LogMode
        {
            Message,
            Warning,
            Error,
        }
        [BitMask(typeof(LogMode))]
        public int _logMode = -1;

        public bool _showFPS = false;

        public enum XRMode
        {
            LegacyXRSettings,
            XRPlugin
        }
        public XRMode _XRMode = XRMode.LegacyXRSettings;

        public enum HMDModel
        {
            None,

            Quest,
            Quest2,
            QuestPro,

            HTCVive,

            PicoNeo2,
            PicoNeo3,
        }

        public enum XRPlugin
        {
            None,

            OpenXR,
            OculusXR,
            OpenVR,
        }

        public static XRPlugin _xrPlugin
        {
            get
            {
                if (IsXREnabled() && m_XRPlugin == XRPlugin.None)
                {
                    XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
                    XRLoader activeLoader = xrSettings.AssignedSettings.activeLoader;

                    if (activeLoader != null)
                    {
                        string xrPluginName = activeLoader.name.Replace(" ", "").ToLower();
                        if (xrPluginName.Contains("openxr"))
                        {
                            m_XRPlugin = XRPlugin.OpenXR;
                        }
                        else if (xrPluginName.Contains("oculus"))
                        {
                            m_XRPlugin = XRPlugin.OculusXR;
                        }
                    }
                }

                return m_XRPlugin;
            }
        }
        private static XRPlugin m_XRPlugin = XRPlugin.None;

        public enum HandTrackingMode
        {
            Controllers,
            Hands,
        }

        public static HandTrackingMode _handTrackingMode
        {
            get; protected set;
        }

        #endregion

        #region PROTECTED PARAMETERS

        public static QuickVRManager _instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = QuickSingletonManager.GetInstance<QuickVRManager>();
                }

                return m_Instance;
            }
        }
        private static QuickVRManager m_Instance = null;

        public static HMDModel _hmdModel
        {
            get
            {

                if (IsXREnabled() && m_HMDModel == HMDModel.None)
                {
                    //List<InputDevice> devices = new List<InputDevice>();
                    //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);

                    //string hmdName = devices.Count > 0 ? devices[0].name.Replace(" ", "").ToLower() : "";
                    //Debug.Log("HMDName = " + hmdName);

                    string deviceName = SystemInfo.deviceName.Replace(" ", "").ToLower();

                    if (SystemInfo.deviceModel.ToLower().Contains("quest"))
                    {
                        if (deviceName.Contains("2") || deviceName.Contains("standalone"))
                        {
                            m_HMDModel = HMDModel.Quest2;
                        }
                        else if (deviceName.Contains("pro"))
                        {
                            m_HMDModel = HMDModel.QuestPro;
                        }
                        else
                        {
                            m_HMDModel = HMDModel.Quest;
                        }
                    }
                    else if (deviceName.Contains("vive"))
                    {
                        m_HMDModel = HMDModel.HTCVive;
                    }
                    else if (deviceName.Contains("piconeo"))
                    {
                        if (deviceName.Contains("2"))
                        {
                            m_HMDModel = HMDModel.PicoNeo2;
                        }
                        else
                        {
                            m_HMDModel = HMDModel.PicoNeo3;
                        }
                    }
                }

                return m_HMDModel;
            }
        }
        private static HMDModel m_HMDModel = HMDModel.None;

        protected Animator _animatorSource = null;
        protected Animator _animatorTarget = null;
        protected Transform _originalAnimatorTargetParent = null;
        protected bool _originalAnimatorTargetIKManagerEnabled = false;
        
        protected QuickUnityVR _unityVR = null;
        protected static List<QuickVRHandTrackingManagerBase> _handTrackingManagers = new List<QuickVRHandTrackingManagerBase>();

        protected QuickXRRig _xrRig = null;
        protected InputManager _inputManager = null;
        protected PerformanceFPS _fpsCounter = null;
        protected QuickCopyPoseBase _copyPose = null;

        protected QuickVRInteractionManager _interactionManager = null;

        protected bool _isCalibrationRequired = true;

        #endregion

        #region EVENTS

        public delegate void QuickVRManagerAction();

        public static event QuickVRManagerAction OnPreCalibrate;
        public static event QuickVRManagerAction OnPostCalibrate;

        public static event QuickVRManagerAction OnPreUpdateVRNodes;
        public static event QuickVRManagerAction OnPostUpdateVRNodes;

        public static event QuickVRManagerAction OnPreUpdateIKTargets;
        public static event QuickVRManagerAction OnPostUpdateIKTargets;

        public static event QuickVRManagerAction OnPreUpdateTracking;
        public static event QuickVRManagerAction OnPostUpdateTracking;

        public static event QuickVRManagerAction OnPreCopyPose;
        public static event QuickVRManagerAction OnPostCopyPose;

        public static event QuickVRManagerAction OnPreCameraUpdate;
        public static event QuickVRManagerAction OnPostCameraUpdate;

        public delegate void QuickVRManagerActionAnimator(Animator animator);
        public static event QuickVRManagerActionAnimator OnSourceAnimatorSet;
        public static event QuickVRManagerActionAnimator OnTargetAnimatorSet;

        #endregion

        #region CONSTANTS

        public const float DEFAULT_NEAR_CLIP_PLANE = 0.05f;
        public const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            //If we are in the Editor, the UpdateTracking function is called on LateUpdate, as onBeforeRender seems
            //not to be properly called when in Editor, i.e., the Animation is not correctly blended with the tracking. 
            //On Build, we do the UpdateTracking onBeforeRender because this provide the smoothest possible result as we
            //are delaying the application of the tracking as much as possible. onBeforeRender works as expected on Build. 
            if (!Application.isEditor)
            {
                Application.onBeforeRender += UpdateTracking;
            }

            //QuickVRManagerEarly.OnUpdate += UpdateEarly;
            //QuickVRManagerLate.OnLateUpdate += UpdateTracking;
            //Application.onBeforeRender += UpdateCamera;
        }

        protected virtual void OnDisable()
        {
            if (!Application.isEditor)
            {
                Application.onBeforeRender -= UpdateTracking;
            }

            //QuickVRManagerEarly.OnUpdate -= UpdateEarly;
            //QuickVRManagerLate.OnLateUpdate -= UpdateTracking;
            //Application.onBeforeRender -= UpdateCamera;
        }

        protected virtual void Awake()
        {

            Reset();

            _xrRig = QuickSingletonManager.GetInstance<QuickXRRig>();
            _inputManager = QuickSingletonManager.GetInstance<InputManager>();
            _fpsCounter = QuickSingletonManager.GetInstance<PerformanceFPS>();

            _copyPose = gameObject.GetOrCreateComponent<QuickCopyPoseBase>();
            //_copyPose = gameObject.GetOrCreateComponent<QuickCopyPoseDirect>();
            _copyPose.enabled = false;

            //Legacy XR Mode is deprecated on 2020 onwards. 
#if UNITY_2020_1_OR_NEWER
            _XRMode = XRMode.XRPlugin;
#endif
        }

        protected virtual void Reset()
        {
            name = "__QuickVRManager__";
            transform.ResetTransformation();
            _interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>(); //gameObject.GetOrCreateComponent<QuickVRInteractionManager>();
        }

        #endregion

        #region GET AND SET

        public static void RegisterHandTrackingManager(QuickVRHandTrackingManagerBase htManager)
        {
            _handTrackingManagers.Add(htManager);
        }

        public static bool IsXREnabled()
        {
            return UnityEngine.XR.XRSettings.enabled;
        }

        public virtual Animator GetAnimatorTarget()
        {
            return _animatorTarget != null? _animatorTarget : _animatorSource;
        }

        public virtual void SetAnimatorTarget(Animator animator)
        {
            if (_animatorSource == null)
            {
                Debug.LogError("ERROR: You must define AnimatorSource prior to AnimatorTarget!!!");
                return;
            }

            if (_animatorTarget != null && _animatorTarget != _animatorSource)
            {
                //We already have an _animatorTarget defined. Restore its parent. 
                _animatorTarget.transform.parent = _originalAnimatorTargetParent;
                _animatorTarget.GetComponent<QuickIKManager>().enabled = _originalAnimatorTargetIKManagerEnabled;
            }

            _animatorTarget = animator;
            _animatorTarget.applyRootMotion = false;
            _animatorTarget.Reset();

            //Add the reqquired components. 
            QuickCameraZNearDefiner zNearDefiner = _animatorTarget.GetComponent<QuickCameraZNearDefiner>();
            if (zNearDefiner)
            {
                _xrRig._cameraNearPlane = zNearDefiner._zNear;
            }

            if (_animatorTarget != _animatorSource)
            {
                QuickIKManager ikManager = animator.GetOrCreateComponent<QuickIKManager>();
                _originalAnimatorTargetIKManagerEnabled = ikManager.enabled;
                ikManager.enabled = false;
            }

            //Align the XRRig with this Animator
            _originalAnimatorTargetParent = _animatorTarget.transform.parent;
            _animatorTarget.transform.parent = null;
            _xrRig.transform.position = _animatorTarget.transform.position;
            _xrRig.transform.rotation = _animatorTarget.transform.rotation;
            _animatorTarget.transform.parent = _xrRig.transform;
            _animatorTarget.transform.localPosition = Vector3.zero;
            _animatorTarget.transform.localRotation = Quaternion.identity;

            _copyPose.SetAnimatorDest(animator);

            _xrRig.AlignVRCamera();
            
            if (OnTargetAnimatorSet != null)
            {
                OnTargetAnimatorSet(animator);
            }
        }

        public virtual Animator GetAnimatorSource()
        {
            return _animatorSource;
        }

        protected virtual void SetAnimatorSource(Animator animator)
        {
            _animatorSource = animator;
            _copyPose.SetAnimatorSource(animator);

            //_xrRig.Align();

            if (OnSourceAnimatorSet != null)
            {
                OnSourceAnimatorSet(animator);
            }
        }

        public virtual void AddUnityVRTrackingSystem(QuickUnityVR unityVR)
        {
            _unityVR = unityVR;

            Animator animator = _unityVR.GetComponent<Animator>();
            SetAnimatorSource(animator);
            SetAnimatorTarget(animator);
        }

        public virtual bool IsCalibrated()
        {
            return !_isCalibrationRequired;
        }

        public virtual void RequestCalibration()
        {
            _isCalibrationRequired = true;
        }

        protected virtual void Calibrate()
        {
            if (OnPreCalibrate != null) OnPreCalibrate();

            if (!_animatorSource || !_animatorTarget)
            {
                Debug.LogError("ERROR: You must define AnimatorSource and AnimatorTarget for Calibration!!!");
            }
            else
            {
                //To avoid precision problems, make the Animator to be in the World origin.
                Vector3 tmpPos = _animatorTarget.transform.position;
                Quaternion tmpRot = _animatorTarget.transform.rotation;
                _xrRig.transform.position = Vector3.zero;
                _xrRig.transform.rotation = Quaternion.identity;

                _animatorSource.transform.parent = _xrRig.transform;
                _animatorSource.transform.localPosition = Vector3.zero;
                _animatorSource.transform.localRotation = Quaternion.identity;

                _animatorTarget.transform.parent = _xrRig.transform;
                _animatorTarget.transform.localPosition = Vector3.zero;
                _animatorTarget.transform.localRotation = Quaternion.identity;

                //Calibrate the Master Avatar, i.e., the master is set on the original pose
                _animatorSource.GetComponent<QuickIKManager>().Calibrate();

                //Copy the pose to the TargetAvatar and make it to be its initial pose. This way, both avatars
                //start in the same reference pose. 
                _copyPose.CopyPose();
                QuickIKManager ikManagerTarget = _animatorTarget.GetComponent<QuickIKManager>();
                ikManagerTarget.SavePose();
                ikManagerTarget.Calibrate();

                _xrRig.Calibrate();

                //Restore the Target Avatar position and rotation
                _xrRig.transform.position = tmpPos;
                _xrRig.transform.rotation = tmpRot;
            }

            _isCalibrationRequired = false;

            if (OnPostCalibrate != null) OnPostCalibrate();
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        //protected virtual void UpdateEarly()
        {
            foreach (var htManager in _handTrackingManagers)
            {
                htManager.Update();
            }

            //Update the InputState
            _inputManager.UpdateState();

            if (_fpsCounter.gameObject.activeSelf != _showFPS)
            {
                _fpsCounter.gameObject.SetActive(_showFPS);
            }

            //Calibrate the TrackingManagers that needs to be calibrated. 
            if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CALIBRATE) || _isCalibrationRequired)
            {
                Calibrate();
            }
        }

#if UNITY_EDITOR
        protected virtual void LateUpdate()
        {
            UpdateTracking();
        }
#endif

        protected virtual void UpdateTracking()
        {
            //Vector3 tmpPos = _animatorSource.transform.position;
            //_animatorSource.transform.position = Vector3.zero;

            bool isHandTrackingEnabled = false;
            for (int i = 0; i < _handTrackingManagers.Count && !isHandTrackingEnabled; i++)
            {
                var htManager = _handTrackingManagers[i];
                isHandTrackingEnabled |= htManager.IsHandTrackingEnabled();
            }
            _handTrackingMode = isHandTrackingEnabled ? HandTrackingMode.Hands : HandTrackingMode.Controllers;

            //Update the VRNodes
            if (OnPreUpdateVRNodes != null) OnPreUpdateVRNodes();
            UpdateXRRig();
            if (OnPostUpdateVRNodes != null) OnPostUpdateVRNodes();

            //Update the IKTargets with the tracking information
            if (OnPreUpdateIKTargets != null) OnPreUpdateIKTargets();
            if (_unityVR)
            {
                _unityVR.UpdateIKTargets();
            }

            if (OnPostUpdateIKTargets != null) OnPostUpdateIKTargets();

            //Apply the tracking of the VRNodes
            if (OnPreUpdateTracking != null) OnPreUpdateTracking();
            if (_unityVR)
            {
                _unityVR.UpdateTracking();
            }
            //_animatorSource.transform.position = tmpPos;

            if (OnPostUpdateTracking != null) OnPostUpdateTracking();

            //Copy the pose of the source avatar to the target avatar
            if (OnPreCopyPose != null) OnPreCopyPose();
            _copyPose.CopyPose();
            _copyPose.AlignFingers();
            if (OnPostCopyPose != null) OnPostCopyPose();

            //Update the Camera position
            if (OnPreCameraUpdate != null) OnPreCameraUpdate();
            _xrRig.UpdateXRCamera();
            if (OnPostCameraUpdate != null) OnPostCameraUpdate();

            //RefineFinalPose();

            //if (_animatorSource)
            //{
            //    for (QuickHumanBodyBones role = 0; role != QuickHumanBodyBones.LastBone; role++)
            //    {
            //        QuickVRNode vrNode = _xrRig.GetVRNode(role);
            //        Transform tBone = _animatorSource.GetBoneTransform(role);
            //        if (vrNode && tBone && !vrNode._isTrackedOrInferred)
            //        {
            //            vrNode.SetTrackedPosition(tBone.position);
            //        }
            //    }
            //}
        }

        protected virtual void UpdateXRRig()
        {
            _xrRig.UpdateRig();
            //_interactionManager.UpdateLocomotionProviders();

            //Update the VRNodes of the feet if necessary, in case they are not tracked. 
            //QuickVRNode vrNodeLeftFoot = _xrRig.GetVRNode(HumanBodyBones.LeftFoot);
            //if (!vrNodeLeftFoot._isTracked)
            //{
            //    vrNodeLeftFoot.UpdateState();
            //}

            //QuickVRNode vrNodeRightFoot = _xrRig.GetVRNode(HumanBodyBones.LeftFoot);
            //if (!vrNodeLeftFoot._isTracked)
            //{
            //    vrNodeLeftFoot.UpdateState();
            //}
        }

        protected virtual void RefineFinalPose()
        {
            if (_animatorTarget)
            {
                QuickIKManager ikManager = _animatorTarget.GetComponent<QuickIKManager>();

                if (_unityVR._cameraMode == QuickUnityVR.CameraMode.FirstPerson)
                {
                    Vector3 offsetHead = Camera.main.transform.position - _animatorTarget.GetEyeCenterVR().position;
                    Vector3 offsetHips = Vector3.Scale(Vector3.up, offsetHead);

                    ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb.position += offsetHead;
                    ikManager.GetIKSolver(HumanBodyBones.Hips)._targetLimb.position += offsetHips;
                    //ikManager.GetIKSolver(HumanBodyBones.LeftFoot)._targetLimb.position += offsetHips;
                    //ikManager.GetIKSolver(HumanBodyBones.RightFoot)._targetLimb.position += offsetHips;
                }

                ikManager.UpdateTracking();
            }
        }

        #endregion

        public static void Log(object message)
        {
            if ((_instance._logMode & (1 << (int)LogMode.Message)) != 0)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if ((_instance._logMode & (1 << (int)LogMode.Warning)) != 0)
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogError(object message)
        {
            if ((_instance._logMode & (1 << (int)LogMode.Error)) != 0)
            {
                Debug.LogError(message);
            }
        }

    }

}

