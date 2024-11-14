//using System.Collections;
//using System.Collections.Generic;

//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

//namespace QuickVR.Interaction.Locomotion
//{

//    public class QuickDirectTurnProvider : QuickContinuousTurnProviderBase
//    {

//        #region PUBLIC ATTRIBUTES

//        public float _minActivaAngle = -20;
//        public float _maxActiveAngle = 20;

//        #endregion

//        #region PROTECTED ATTRIBUTES

//        protected QuickXRRig _xrRig
//        {
//            get
//            {
//                if (!m_XRRig)
//                {
//                    m_XRRig = QuickSingletonManager.GetInstance<QuickXRRig>();
//                }

//                return m_XRRig;
//            }
//        }
//        protected QuickXRRig m_XRRig = null;

//        protected QuickVRNode _nodeCamera
//        {
//            get
//            {
//                if (!m_NodeCamera)
//                {
//                    m_NodeCamera = _xrRig.GetVRNodeCamera();
//                }

//                return m_NodeCamera;
//            }
//        }
//        protected QuickVRNode m_NodeCamera = null;

//        #endregion

//        #region GET AND SET

//        /// <summary>
//        /// Reads the current value of the turn input.
//        /// </summary>
//        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
//        protected override Vector2 ReadInput()
//        {
//            Vector3 currFwd = _xrRig.transform.forward;
//            Vector3 targetFwd = Vector3.ProjectOnPlane(_nodeCamera.transform.forward, _xrRig.transform.up);
//            float rotAngle = Vector3.SignedAngle(currFwd, targetFwd, _xrRig.transform.up);

//            Vector2 result = new Vector2(rotAngle, 0);
//            turnSpeed = result.magnitude / Time.deltaTime;

//            return result.normalized;
//        }

//        #endregion

//        #region UPDATE

//        public override void UpdateLocomotion()
//        {
//            Vector3 fwdBefore = Vector3.ProjectOnPlane(_nodeCamera.transform.forward, _xrRig.transform.up);

//            base.UpdateLocomotion();

//            //Rotate the XRRig's tracking offset in a way that the vrNodeHead ends up with the same rotation that it had before 
//            //updating the locomotion. 
//            Vector3 fwdAfter = Vector3.ProjectOnPlane(_nodeCamera.transform.forward, _xrRig.transform.up);
//            float rotAngle = Vector3.SignedAngle(fwdAfter, fwdBefore, _xrRig.transform.up);
//            _xrRig._trackingOffset.RotateAround(_nodeCamera.transform.position, _xrRig.transform.up, rotAngle);
//        }

//        #endregion

//    }

//}

