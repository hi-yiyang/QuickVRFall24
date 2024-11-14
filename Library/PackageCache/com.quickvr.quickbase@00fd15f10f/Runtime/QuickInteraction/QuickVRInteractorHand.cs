using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;

namespace QuickVR.Interaction
{

    public class QuickVRInteractorHand : QuickVRInteractor
    {

        #region PUBLIC ATTRIBUTES

        public override XRNode _xrNode
        {
            get
            {
                return m_XRNode;
            }
            set
            {
                m_XRNode = value;

                //Load the default ActionMap for this controller
                _actionMapDefault = InputManager.GetInputActionsDefault().FindActionMap(_xrNode == XRNode.LeftHand ? ACTION_MAP_CONTROLLER_LEFT : ACTION_MAP_CONTROLLER_RIGHT);

                ConfigureInteractorGrabDirect();
                ConfigureInteractorGrabRay();
                ConfigureInteractorTeleportRay();
                ConfigureInteractorUIRay();
                //ConfigureInteractorUIGeneric();
            }
        }
        protected XRNode m_XRNode = XRNode.LeftHand;

        public bool _isLeft
        {
            get
            {
                return _xrNode == XRNode.LeftHand;
            }
        }
            
        #endregion

        #region PROTECTED ATTRIBUTES

        protected enum ActionType
        {
            Select,
            Activate,
            Haptic,
            UI,
        }

        protected InputActionMap _actionMapDefault = null;
        protected List<XRRayInteractor> _rayInteractors = new List<XRRayInteractor>();

        #endregion

        #region CONSTANTS

        protected const string ACTION_MAP_CONTROLLER_LEFT = "LeftController";
        protected const string ACTION_MAP_CONTROLLER_RIGHT = "RightController";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCopyPose += ActionPostCopyPose;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCopyPose -= ActionPostCopyPose;
        }

        protected override void CreateInteractors()
        {
            base.CreateInteractors();

            QuickVRInteractionManager interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();

            _interactors[InteractorType.GrabDirect] = CreateInteractor(interactionManager._pfInteractorGrabDirect);
            _interactors[InteractorType.Grab] = CreateInteractor(interactionManager._pfInteractorGrabRay);
            _interactors[InteractorType.Teleport] = CreateInteractor(interactionManager._pfInteractorTeleportRay);
            _interactors[InteractorType.UI] = CreateInteractor(interactionManager._pfInteractorUIRay);

            _rayInteractors = new List<XRRayInteractor>(GetComponentsInChildren<XRRayInteractor>());
        }

        protected virtual XRBaseControllerInteractor CreateInteractor(XRBaseControllerInteractor pfInteractor)
        {
            //Create the Interactor Direct
            XRBaseControllerInteractor result = Instantiate(pfInteractor, transform);
            ActionBasedController aController = result.GetComponent<ActionBasedController>();
            if (aController)
            {
                aController.enableInputTracking = false;
            }

            return result;
        }

        /// <summary>
        /// Configures a Direct Interactor to interact with objects containing the XRGrabInteractable component.  
        /// </summary>
        protected virtual void ConfigureInteractorGrabDirect()
        {
            //Configure the direct interactor
            ActionBasedController interactor = GetInteractor(InteractorType.GrabDirect).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.Select, "Grab");
            SetInputAction(interactor, ActionType.Activate, "Use");
        }

        /// <summary>
        /// Configures a Ray Interactor to interact with objects containing the XRGrabInteractable component.  
        /// </summary>
        protected virtual void ConfigureInteractorGrabRay()
        {
            //Configure the grab ray
            ActionBasedController interactor = GetInteractor(InteractorType.Grab).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.Select, "Grab");
            SetInputAction(interactor, ActionType.Activate, "Use");
            SetInputAction(interactor, ActionType.Haptic, "Haptic Device");

            QuickXRRayInteractor ray = interactor.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Grab;
            ray.enableUIInteraction = false;
        }

        /// <summary>
        /// Configures a Ray Interactor to interact with objects containing the BaseTeleportationInteractable component.  
        /// </summary>
        protected virtual void ConfigureInteractorTeleportRay()
        {
            //Configure the teleport ray
            ActionBasedController interactor = GetInteractor(InteractorType.Teleport).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.Select, "Teleport");
            SetInputAction(interactor, ActionType.Haptic, "Haptic Device");
            
            QuickXRRayInteractor ray = interactor.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Teleport;
            ray.enableUIInteraction = false;
        }

        /// <summary>
        /// Configures a Ra Interactor to interact with the UI elements. 
        /// </summary>
        protected virtual void ConfigureInteractorUIRay()
        {
            //Configure the UI ray
            ActionBasedController interactor = GetInteractor(InteractorType.UI).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.UI, "Use");

            QuickXRRayInteractor ray = interactor.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.UI;
            ray.enableUIInteraction = true;
        }

        protected virtual void ConfigureInteractorGenericRay()
        {

        }

        protected virtual void SetInputAction(ActionBasedController interactor, ActionType actionType, string actionName)
        {
            InputAction action = _actionMapDefault.FindAction(actionName);
            if (actionType == ActionType.Activate)
            {
                interactor.activateAction = new InputActionProperty(action);
            }
            else if (actionType == ActionType.Select)
            {
                interactor.selectAction = new InputActionProperty(action);
            }
            else if (actionType == ActionType.Haptic)
            {
                interactor.hapticDeviceAction = new InputActionProperty(action);
            }
            else if (actionType == ActionType.UI)
            {
                interactor.uiPressAction = new InputActionProperty(action);
            }
        }

        #endregion

        #region GET AND SET

        public virtual void UpdateNewAnimatorTarget(Animator animator)
        {
            HumanBodyBones boneHandID = _isLeft? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
            Transform tHand = animator.GetBoneTransform(_isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            Transform tMiddle = animator.GetBoneTransform(_isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);

            //Set this interactor to be child of the corresponding hand. 
            QuickIKManager ikManager = animator.GetComponent<QuickIKManager>();
            ikManager.UpdateTracking();

            transform.parent = tHand;
            transform.ResetTransformation();
            transform.rotation = ikManager.GetIKSolver(boneHandID)._targetLimb.localRotation;

            //Configure the DirectInteractor
            XRBaseControllerInteractor interactor = GetInteractor(InteractorType.GrabDirect);
            Transform tAttach = interactor.GetComponent<XRDirectInteractor>().attachTransform;
            tAttach.position = Vector3.Lerp(tHand.position, tMiddle.position, 0.5f);

            Transform tIndex = animator.GetBoneTransform(_isLeft ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal);
            Transform tLittle = animator.GetBoneTransform(_isLeft ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal);
            CapsuleCollider collider = interactor.GetComponent<CapsuleCollider>();
            collider.height = Vector3.Distance(tIndex.position, tLittle.position);
            collider.center = tAttach.localPosition;
            collider.radius = Vector3.Distance(tHand.position, tMiddle.position) * 0.5f;

            //Add the PokeInteractor
            XRPokeInteractor pokeInteractor = animator.GetBoneTransformFingerTip(_isLeft ? HumanBodyBones.LeftIndexDistal : HumanBodyBones.RightIndexDistal).GetOrCreateComponent<XRPokeInteractor>();
            pokeInteractor.GetOrCreateComponent<QuickPokeInteractorVisualizer>(); 

            //Define the radius
            //SphereCollider sCollider = _interactorGrabDirect.GetComponent<SphereCollider>();
            //sCollider.center = tAttach.localPosition;
            //sCollider.radius = Vector3.Distance(tHand.position, tMiddle.position) * 0.5f;
        }

        #endregion

        #region UPDATE

        protected virtual void ActionPostCopyPose()
        {
            QuickVRHand h = QuickVRHandTrackingManager.GetVRHand(_isLeft);
            Transform tRayOrigin = null;

            if (h != null && h._isTracked)
            {
                QuickVRNode vrNodeHand = QuickXRRig._instance.GetVRNode(_isLeft ? QuickHumanBodyBones.LeftHand : QuickHumanBodyBones.RightHand);
                QuickVRNode vrNodeHandAim = QuickXRRig._instance.GetVRNode(_isLeft? QuickHumanBodyBones.LeftHandAim : QuickHumanBodyBones.RightHandAim);

                Matrix4x4 m = Matrix4x4.TRS(vrNodeHand.transform.position, vrNodeHand.transform.rotation, Vector3.one);

                Vector3 localPos = m.inverse.MultiplyPoint(vrNodeHandAim.transform.position);

                Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
                HumanBodyBones boneHandID = _isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
                Transform tBoneHand = animator.GetBoneTransform(boneHandID);
                QuickIKSolver ikSolverHand = animator.GetComponent<QuickIKManager>().GetIKSolver(boneHandID);

                Matrix4x4 m2 = Matrix4x4.TRS(tBoneHand.position, ikSolverHand._targetLimb.rotation, Vector3.one);

                tRayOrigin = animator.GetBoneTransform(_isLeft ? QuickHumanBodyBones.LeftHandAim : QuickHumanBodyBones.RightHandAim);
                tRayOrigin.position = m2.MultiplyPoint(localPos);
                tRayOrigin.rotation = vrNodeHandAim.transform.rotation;
            }
            
            foreach (XRRayInteractor interactor in _rayInteractors)
            {
                interactor.rayOriginTransform = tRayOrigin;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            QuickVRHand h = QuickVRHandTrackingManager.GetVRHand(_isLeft);
            if (h != null)
            {
                h.DebugHand();
            }
        }

        #endregion

    }
}


