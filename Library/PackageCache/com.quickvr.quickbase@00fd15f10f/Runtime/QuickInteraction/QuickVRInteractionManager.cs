using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using Unity.XR.CoreUtils;
using QuickVR.Interaction.Locomotion;

namespace QuickVR.Interaction
{

    public interface IQuickLocomotionProvider
    {

        public void UpdateLocomotion();

    }

    [RequireComponent(typeof(XRInteractionManager))]
    public class QuickVRInteractionManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public static QuickVRInteractionManager _instance
        {
            get
            {
                if (!m_InteractionManager)
                {
                    m_InteractionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();
                }

                return m_InteractionManager;
            }
        }
        protected static QuickVRInteractionManager m_InteractionManager = null;

        public XRBaseControllerInteractor _pfInteractorGrabDirect = null;
        public XRBaseControllerInteractor _pfInteractorGrabRay = null;
        public XRBaseControllerInteractor _pfInteractorTeleportRay = null;
        public XRBaseControllerInteractor _pfInteractorUIRay = null;

        public QuickVRInteractor _interactorHead { get; protected set; }
        public QuickVRInteractorHand _interactorHandLeft { get; protected set; }
        public QuickVRInteractorHand _interactorHandRight { get; protected set; }

        public CharacterController _characterController { get; protected set; }

        [Header("Locomotion Providers")]
        public bool _locomotionDirectMoveEnabled = false;
        public bool _locomotionDirectTurnEnabled = false;
        public bool _locomotionContinuousMoveEnabled = false;
        public bool _locomotionContinuousTurnEnabled = false;
        public bool _locomotionWalkInPlaceEnabled = false;
        public bool _locomotionTeleportEnabled = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRManager _vrManager = null;
        protected QuickXRRig _xrRig = null;
        protected XRInteractionManager _xrInteractionManager = null;

        protected XROrigin _xrOrigin = null;

        protected Animator _animator = null;
        protected QuickVRHandAnimatorBase _handAnimatorLeft = null;
        protected QuickVRHandAnimatorBase _handAnimatorRight = null;
        protected List<XRGrabInteractable> _grabInteractables = new List<XRGrabInteractable>();

        protected List<IQuickLocomotionProvider> _locomotionProviders = new List<IQuickLocomotionProvider>();
        protected QuickWalkInPlace _walkInPlace = null;

        //The _locomotionAnimator is used in case that the feet are not tracked. 
        protected QuickLocomotionAnimationBase _locomotionAnimator
        {
            get
            {
                if (!m_LocomotionAnimator)
                {
                    Animator animatorSrc = _vrManager.GetAnimatorSource();

                    if (animatorSrc != null)
                    {
                        m_LocomotionAnimator = animatorSrc.GetOrCreateComponent<QuickLocomotionAnimationBase>();
                        m_LocomotionAnimator.enabled = false;
                        m_LocomotionAnimator.SetTimeUpdateMode(UnityEngine.Playables.DirectorUpdateMode.Manual);
                        //m_LocomotionAnimator._locomotionTracker = _ikBoneSpineRoot.GetOrCreateComponent<QuickLocomotionTracker>();
                    }
                }

                return m_LocomotionAnimator;
            }
        }
        protected QuickLocomotionAnimationBase m_LocomotionAnimator = null;

        #endregion

        #region CONSTANTS

        protected const string GRAB_PIVOT_NAME = "GrabPivot";

        protected const string PF_INTERACTOR_GRAB_DIRECT = "Prefabs/pf_InteractorGrabDirect";
        protected const string PF_INTERACTOR_GRAB_RAY = "Prefabs/pf_InteractorGrabRay";
        protected const string PF_INTERACTOR_TELEPORT_RAY = "Prefabs/pf_InteractorTeleportRay";
        protected const string PF_INTERACTOR_UI_RAY = "Prefabs/pf_InteractorUIRay";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();
            CheckPrefabs();
            CheckEventSystem();

            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
            _xrRig = QuickSingletonManager.GetInstance<QuickXRRig>();
            _xrInteractionManager = QuickSingletonManager.GetInstance<XRInteractionManager>();

            _interactorHead = transform.CreateChild("__InteractorHead__").GetOrCreateComponent<QuickVRInteractor>();
            _interactorHead._xrNode = XRNode.Head;

            _interactorHandLeft = transform.CreateChild("__InteractorHandLeft__").GetOrCreateComponent<QuickVRInteractorHand>();
            _interactorHandLeft._xrNode = XRNode.LeftHand;

            _interactorHandRight = transform.CreateChild("__InteractorHandRight__").GetOrCreateComponent<QuickVRInteractorHand>();
            _interactorHandRight._xrNode = XRNode.RightHand;

            BaseTeleportationInteractable[] teleportationInteractables = FindObjectsOfType<BaseTeleportationInteractable>();
            foreach (BaseTeleportationInteractable t in teleportationInteractables)
            {
                t.teleportationProvider = GetLocomotionProvider<QuickTeleportationProvider>();
            }

            _grabInteractables = new List<XRGrabInteractable>(FindObjectsOfType<XRGrabInteractable>());
            foreach (XRGrabInteractable g in _grabInteractables)
            {
                if (!g.attachTransform)
                {
                    //Try to find the default attach transform
                    g.attachTransform = g.transform.Find(GRAB_PIVOT_NAME);
                }
            }
        }

        protected virtual void Start()
        {
            _handAnimatorLeft = _xrRig.GetVRNode(HumanBodyBones.LeftHand).GetComponentInChildren<QuickVRHandAnimatorBase>();
            _handAnimatorRight = _xrRig.GetVRNode(HumanBodyBones.RightHand).GetComponentInChildren<QuickVRHandAnimatorBase>();

            _interactorHead.transform.parent = _xrRig.GetVRNodeCamera().transform;
            _interactorHead.transform.ResetTransformation();

            _characterController = _xrRig.GetOrCreateComponent<CharacterController>();
            foreach (XRGrabInteractable g in _grabInteractables)
            {
                foreach (Collider c in g.GetComponentsInChildren<Collider>(true))
                {
                    Physics.IgnoreCollision(_characterController, c, true);
                }
            }

            _xrOrigin.Camera = Camera.main;
            _xrOrigin.Origin = _xrRig.gameObject;
        }

        protected virtual void Reset()
        {
            _xrOrigin = transform.CreateChild("__XROrigin__").GetOrCreateComponent<XROrigin>();
            _xrOrigin.GetOrCreateComponent<LocomotionSystem>();
            CreateLocomotionProviders();
        }

        protected virtual void CreateLocomotionProviders()
        {
            CreateLocomotionProvider<QuickContinuousMoveProvider>();
            CreateLocomotionProvider<QuickContinuousTurnProvider>();
            CreateLocomotionProvider<QuickTeleportationProvider>();
            _walkInPlace = gameObject.GetOrCreateComponent<QuickWalkInPlace>();
        }

        protected virtual T CreateLocomotionProvider<T>() where T : LocomotionProvider, IQuickLocomotionProvider
        {
            T lProvider = gameObject.GetOrCreateComponent<T>();
            lProvider.enabled = false;  //Disable the component, as we manually manage the Update. 

            lProvider.system = _xrOrigin.GetComponent<LocomotionSystem>();

            _locomotionProviders.Add(lProvider);

            return lProvider;
        }

        protected virtual void CheckPrefabs()
        {
            if (_pfInteractorGrabDirect == null)
            {
                _pfInteractorGrabDirect = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_GRAB_DIRECT);
            }
            if (_pfInteractorGrabRay == null)
            {
                _pfInteractorGrabRay = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_GRAB_RAY);
            }
            if (_pfInteractorTeleportRay == null)
            {
                _pfInteractorTeleportRay = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_TELEPORT_RAY);
            }
            if (_pfInteractorUIRay == null)
            {
                _pfInteractorUIRay = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_UI_RAY);
            }
        }

        protected virtual void CheckEventSystem()
        {
            //Look if there is an EventSystem already created, and if this is the case, destroy it and 
            //create our own one to be able to interact with the UI in VR. 
            EventSystem eSystem = FindObjectOfType<EventSystem>();
            if (eSystem)
            {
                Destroy(eSystem.gameObject);
            }

            GameObject go = new GameObject("__EventSystem__");
            go.AddComponent<EventSystem>();
            go.AddComponent<XRUIInputModule>();
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCameraUpdate += UpdateCharacterController;
            QuickVRManager.OnTargetAnimatorSet += UpdateNewAnimatorTarget;

            //_continousMoveProvider.beginLocomotion += OnEndMove;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCameraUpdate += UpdateCharacterController;
            QuickVRManager.OnTargetAnimatorSet -= UpdateNewAnimatorTarget;

            //_continousMoveProvider.beginLocomotion -= OnEndMove;
        }

        #endregion

        #region GET AND SET

        public virtual T GetLocomotionProvider<T>() where T : LocomotionProvider
        {
            T result = null;

            for (int i = 0; result == null && i < _locomotionProviders.Count; i++)
            {
                if (_locomotionProviders[i].GetType() == typeof(T))
                {
                    result = (T)_locomotionProviders[i];
                }
            }
            
            return result;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateLocomotionProviders()
        {
            //Set the default input data for the continuous move provider. 
            QuickContinuousMoveProvider lProviderMove =  GetLocomotionProvider<QuickContinuousMoveProvider>();
            if (_locomotionContinuousMoveEnabled)
            {
                float dX = InputManagerVR.GetAxis(InputManagerVR.AxisCode.LeftStick_Horizontal);
                float dY = InputManagerVR.GetAxis(InputManagerVR.AxisCode.LeftStick_Vertical);

                lProviderMove.AddInputData(new Vector2(dX, dY), 1);
            }

            //Set the input for the walk in place. 
            if (_locomotionWalkInPlaceEnabled) 
            {
                lProviderMove.AddInputData(Vector2.up, _walkInPlace._currentSpeed);
            }

            //Update the locomotion providers. 
            foreach (IQuickLocomotionProvider lProvider in _locomotionProviders)
            {
                lProvider.UpdateLocomotion();
            }

            //Apply the locomotion animation if necessary. 
            if (_locomotionAnimator)
            {
                _locomotionAnimator.StartPlayableGraph();
                _locomotionAnimator.Evaluate(Time.deltaTime);
            }
        }

        protected virtual void OnGrabInteractable(SelectEnterEventArgs args)
        {
            foreach (Collider c in args.interactableObject.colliders)
            {
                Physics.IgnoreCollision(_characterController, c, true);

                foreach (Collider cHand in _handAnimatorLeft.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, true);
                }

                foreach (Collider cHand in _handAnimatorRight.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, true);
                }
            }
        }

        protected virtual void OnDropInteractable(SelectExitEventArgs args)
        {
            foreach (Collider c in args.interactableObject.colliders)
            {
                Physics.IgnoreCollision(_characterController, c, false);

                foreach (Collider cHand in _handAnimatorLeft.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, false);
                }

                foreach (Collider cHand in _handAnimatorRight.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, false);
                }
            }
        }

        protected virtual void UpdateNewAnimatorTarget(Animator animator)
        {
            _interactorHandLeft.UpdateNewAnimatorTarget(animator);
            _interactorHandRight.UpdateNewAnimatorTarget(animator);

            _animator = animator;
        }

        protected virtual void SetHandInteractorPosition(Animator animator, bool isLeft)
        {
            Transform tInteractor = isLeft ? _interactorHandLeft.transform : _interactorHandRight.transform;
            Transform tHand = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            Transform tMiddle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);

            //Define the position of the interactor
            tInteractor.parent = tHand;
            tInteractor.transform.ResetTransformation();
            tInteractor.transform.LookAt(tMiddle, transform.up);
            tInteractor.transform.position = Vector3.Lerp(tHand.position, tMiddle.position, 0.5f);

            //Define the radius
            tInteractor.GetComponent<SphereCollider>().radius = Vector3.Distance(tHand.position, tMiddle.position) * 0.5f;
        }

        protected virtual void UpdateCharacterController()
        {
            if (_characterController && _animator && _animator.gameObject.activeInHierarchy)
            {
                //Compute the height of the collider
                float h = _animator.GetEyeCenterVR().position.y - _animator.transform.position.y;
                _characterController.height = h;
                _characterController.center = new Vector3(0, h * 0.5f + _characterController.skinWidth, 0);

                //Compute the radius
                Vector3 v = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position - _animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
                v = Vector3.ProjectOnPlane(v, _animator.transform.up);
                _characterController.radius = v.magnitude / 2;
            }
        }

        #endregion

    }

}


