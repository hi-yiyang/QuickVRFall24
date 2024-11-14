using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using UnityEngine.InputSystem;

namespace QuickVR
{

    public class QuickVRNode : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public bool _showModel = false;

        public enum State
        {
            NotTracked,
            Tracked,
            Inferred,
        }
        public State _state = State.NotTracked;
                
        public bool _isTracked
        {
            get
            {
                return _state == State.Tracked;
            }
        }

        public bool _isTrackedOrInferred
        {
            get
            {
                return _isTracked || _state == State.Inferred;
            }
        }

        public Vector3 _initialLocalPosition { get; protected set; } = Vector3.zero;
        public Quaternion _initialLocalRotation {get; protected set; } = Quaternion.identity;

        #endregion

        #region PROTECTED PARAMETERS

        protected List<QuickTrackedDevice> _trackedDevices = new List<QuickTrackedDevice>();

        protected QuickXRRig _xrRig
        {
            get
            {
                if (m_XRRig == null)
                {
                    m_XRRig = QuickSingletonManager.GetInstance<QuickXRRig>();
                }
                
                return m_XRRig;
            }
        }
        protected QuickXRRig m_XRRig = null;

        protected Vector3 _xrRigUp
        {
            get
            {
                return _xrRig.transform.up;
            }
        }

        protected QuickTrackedObject _trackedObject = null;

        protected Transform _model = null;

        protected QuickHumanBodyBones _role = QuickHumanBodyBones.Head;

        [SerializeField, ReadOnly]
        protected bool m_IsTracked = false;

        protected Vector3 _lastValidLocalPosition = Vector3.zero;
        protected Quaternion _lastValidLocalRotation = Quaternion.identity;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _trackedObject = transform.CreateChild("__TrackedObject__").gameObject.GetOrCreateComponent<QuickTrackedObject>();
            AddTrackedDevice(new QuickTrackedDeviceDefault());
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCalibrate += ActionOnPostCalibrate;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCalibrate -= ActionOnPostCalibrate;
        }

        protected virtual void ActionOnPostCalibrate()
        {
            _initialLocalPosition = transform.localPosition;
            _initialLocalRotation = transform.localRotation;
            _lastValidLocalPosition = _initialLocalPosition;
            _lastValidLocalRotation = _initialLocalRotation;
        }

        protected virtual string GetVRModelName()
        {
            if (_role == QuickHumanBodyBones.Head)
            {
                return "pf_Generic_HMD";
            }

            if (_role == QuickHumanBodyBones.LeftHand || _role == QuickHumanBodyBones.RightHand)
            {
                string modelName = "";
                bool isLeft = _role == QuickHumanBodyBones.LeftHand;
                QuickVRManager.HMDModel hmdModel = QuickVRManager._hmdModel;

                if (hmdModel == QuickVRManager.HMDModel.HTCVive)
                {
                    modelName = "pf_VIVE_Controller";
                }
                else if (hmdModel == QuickVRManager.HMDModel.Quest)
                {
                    modelName = isLeft ? "pf_OculusCV1_Controller_Left" : "pf_OculusCV1_Controller_Right";
                }
                else if (hmdModel == QuickVRManager.HMDModel.Quest2)
                {
                    modelName = isLeft ? "pf_Oculus_Quest2_Controller_Left" : "pf_Oculus_Quest2_Controller_Right";
                }
                else if (hmdModel == QuickVRManager.HMDModel.QuestPro)
                {
                    modelName = isLeft ? "pf_Oculus_Quest2_Controller_Left" : "pf_Oculus_Quest2_Controller_Right";
                }

                return modelName;
            }

            return "pf_VIVE_Tracker";
        }

        protected virtual void LoadVRModel()
        {
            if (_model)
            {
                DestroyImmediate(_model.gameObject);
            }

            string pfName = GetVRModelName();

            if (pfName.Length != 0)
            {
                _model = Instantiate(Resources.Load<Transform>("Prefabs/" + pfName));
                _model.parent = transform;
                _model.ResetTransformation();
                _model.name = "Model";
            }

            SetModelVisible(_showModel);
        }

        #endregion

        #region GET AND SET

        public virtual void AddTrackedDevice(QuickTrackedDevice tDevice)
        {
            tDevice._vrNode = this;
            _trackedDevices.Add(tDevice);

            //Debug.Log("Added Tracked Device of type " + tDevice.GetType().Name + " at VRNode " + _role.ToString());
        }

        public virtual Transform GetModel()
        {
            return _model;
        }

        protected virtual void SetModelVisible(bool v)
        {
            if (_model && (_showModel != _model.gameObject.activeSelf))
            {
                _model.gameObject.SetActive(v);
            }
        }

        public virtual QuickHumanBodyBones GetRole()
        {
            return _role;
        }

        public virtual void SetRole(QuickHumanBodyBones role)
        {
            _role = role;

            LoadVRModel();

            _trackedObject.Reset();
        }

        public virtual QuickTrackedObject GetTrackedObject()
        {
            return _trackedObject;
        }

        public virtual void SetTrackedPosition(Vector3 position, Space space = Space.World)
        {
            if (space == Space.World)
            {
                transform.position = position;
            }
            else
            {
                transform.localPosition = position;
            }

            _lastValidLocalPosition = transform.localPosition;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateState()
        {
            _state = State.NotTracked;

            for (int i = 0; i < _trackedDevices.Count && _state == State.NotTracked; i++)
            {
                var device = _trackedDevices[i];
                device.CheckDevice();
                if (device.IsTracking())
                {
                    device.UpdateTracking();
                    
                    _lastValidLocalPosition = transform.localPosition;
                    _lastValidLocalRotation = transform.localRotation;

                    _state = State.Tracked;
                }
            }

            if (_state == State.NotTracked)
            {
                transform.localPosition = _lastValidLocalPosition;
                transform.localRotation = _lastValidLocalRotation;
            }
        }

        protected virtual void UpdateTrackedPosition(Vector3 localPos)
        {
            transform.localPosition = localPos;
        }

        protected virtual void UpdateTrackedRotation(Quaternion localRot)
        {
            transform.localRotation = localRot;
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            SetModelVisible(_showModel && _isTrackedOrInferred);
        }

        #endregion

    }

}
