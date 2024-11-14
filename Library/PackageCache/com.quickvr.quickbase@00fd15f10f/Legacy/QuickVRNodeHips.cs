using UnityEngine;

using QuickVR.Interaction;

namespace QuickVR
{

    public class QuickVRNodeHips : QuickVRNode
    {

        #region PROTECTED ATTRIBUTES

        protected Vector3 _currentVelocity = Vector3.zero;

        protected Vector3 _lastRootPosLS
        {
            get
            {
                return m_LastRootPosLS;
            }
            set
            {
                m_LastRootPosLS = Vector3.ProjectOnPlane(value, _xrRigUp);
            }
        }
        protected Vector3 m_LastRootPosLS = Vector3.zero;

        protected Vector3 _targetRootForward = Vector3.zero;

        #endregion

        #region UPDATE

        public override void UpdateState()
        {
            base.UpdateState();

            if (!_isTracked)
            {
                //UpdateHipsRootRotationInferred();

                //Update the IKSolver and transfer the solution to this transform. 
                QuickIKSolver ikSolverSpine = _xrRig._ikSolverSpine;
                ikSolverSpine._targetHint.position = ikSolverSpine._boneUpper.position - _xrRig.transform.forward * 1.0f;
                ikSolverSpine.UpdateIK();
                transform.position = ikSolverSpine._boneMid.position;
                transform.rotation = ikSolverSpine._boneMid.rotation;

                _state = State.Inferred;
            }
        }

        //protected virtual void UpdateHipsRootRotationInferred()
        //{
        //    //if (QuickVRInteractionManager._instance._locomotionDirectTurnEnabled)
        //    {
        //        //Estimate hips root rotation. 
        //        Vector3 rootPosLS = Vector3.ProjectOnPlane(_ikBoneSpineRoot.localPosition, _xrRigUp);
        //        Vector3 dirLook = Vector3.ProjectOnPlane(_vrNodeHead.GetTrackedObject().transform.forward, _xrRigUp).normalized;
        //        if (Vector3.Distance(rootPosLS, _lastRootPosLS) > 0.1f)
        //        {
        //            Vector3 dirLS = rootPosLS - _lastRootPosLS;
        //            Vector3 dirWS = _xrRig.transform.TransformDirection(dirLS).normalized;
        //            if (Vector3.Dot(dirWS, dirLook) < 0)
        //            {
        //                dirWS *= -1;
        //            }

        //            _targetRootForward = dirWS;

        //            _lastRootPosLS = rootPosLS;
        //        }
        //        else
        //        {
        //            //_targetRootForward = Vector3.Lerp(_ikBoneSpineRoot.forward, dirLook, 0.5f);
        //            _targetRootForward = dirLook;
        //        }

        //        //Vector3 tFwd = Vector3.Lerp(_ikBoneSpineRoot.forward, _targetRootForward, Time.deltaTime * 10.0f).normalized;
        //        Vector3 tFwd = Vector3.SmoothDamp(_ikBoneSpineRoot.forward, _targetRootForward, ref _currentVelocity, 0.2f);
        //        float rotAngle = Vector3.SignedAngle(_ikBoneSpineRoot.forward, tFwd, _xrRigUp);
        //        //_ikBoneSpineRoot.RotateAround(_ikSolverSpine._targetLimb.position, _xrRigUp, rotAngle);
        //        //_ikBoneSpineRoot.RotateAround(_ikBoneSpineLimb.position, _xrRigUp, rotAngle);
        //        //_ikBoneSpineRoot.RotateAround(_xrRig.transform.position, _xrRigUp, rotAngle);
        //        _ikBoneSpineRoot.RotateAround(_ikBoneSpineMid.position, _xrRigUp, rotAngle);
        //        //_ikBoneSpineRoot.forward = Vector3.Lerp(_ikBoneSpineRoot.forward, _targetRootForward, Time.deltaTime * 10.0f).normalized;
        //    }
        //}

        #endregion

    }

}
