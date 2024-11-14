//using System.Collections;
//using System.Collections.Generic;

//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.XR;

//namespace QuickVR.Interaction.Locomotion
//{
    
//    public class QuickDirectMoveProvider : QuickContinuousMoveProviderBase
//    {

//        #region PROTECTED ATTRIBUTES

//        protected QuickXRRig _xrRig
//        {
//            get
//            {
//                return QuickSingletonManager.GetInstance<QuickXRRig>();
//            }
//        }

//        protected QuickVRNode _nodeHead
//        {
//            get
//            {
//                if (!m_NodeHead)
//                {
//                    m_NodeHead = _xrRig.GetVRNode(HumanBodyBones.Head);
//                }

//                return m_NodeHead;
//            }
//        }
//        protected QuickVRNode m_NodeHead = null;

//        protected QuickIKManager _ikManager
//        {
//            get
//            {
//                if (!m_IKManager)
//                {
//                    m_IKManager = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickIKManager>();
//                }

//                return m_IKManager;
//            }
//        }
//        protected QuickIKManager m_IKManager = null;

//        #endregion

//        #region CREATION AND DESTRUCTION

//        protected override void Awake()
//        {
//            base.Awake();

//            forwardSource = _xrRig.transform;
//        }

//        #endregion

//        #region GET AND SET

//        /// <summary>
//        /// Reads the current value of the move input.
//        /// </summary>
//        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
//        protected override Vector2 ReadInput()
//        {
//            Vector3 offsetWS = _xrRig.ComputeOriginPosition() - _xrRig.transform.position;
//            Vector3 offsetLS = _xrRig.transform.InverseTransformDirection(offsetWS);

//            Vector2 result = new Vector2(offsetLS.x, offsetLS.z);
//            moveSpeed = result.magnitude / Time.deltaTime;

//            return result.normalized;
//        }

//        #endregion

//        #region UPDATE

//        public override void UpdateLocomotion()
//        {
//            //QuickVRInteractionManager._instance.GetLocomotionProvider(QuickVRInteractionManager.DefaultLocomotionProvider.DirectTurn).UpdateLocomotion();

//            Vector3 posBefore = _nodeHead.transform.position;

//            base.UpdateLocomotion();

//            //Move the XRRig's tracking offset in a way that the vrNodeHead ends up at the same position that it had before 
//            //updating the locomotion. 
//            Vector3 posOffset = Vector3.ProjectOnPlane(posBefore - _nodeHead.transform.position, _xrRig.transform.up);
//            _xrRig._trackingOffset.Translate(posOffset, Space.World);
//            _xrRig._cameraRoot.Translate(posOffset, Space.World);
//        }

//        #endregion

//    }

//}


