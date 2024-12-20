﻿//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace QuickVR
//{

//    public class QuickWalkInPlace_v2 : QuickCharacterControllerBase
//    {

//        #region CONSTANTS

//        protected const float MIN_TIME_STEP = 0.2f;    //5 steps per second
//        protected const float MAX_TIME_STEP = 1.0f;    //1 steps per second

//        #endregion

//        #region PUBLIC ATTRIBUTES

//        public float _speedMin = 1.0f;  //1m/s  =>  3.6km/h
//        public float _speedMax = 5.0f;  //5m/s  =>  18km/h

//        #endregion

//        #region PROTECTED ATTRIBUTES

//        protected float _timeLastStep = -1.0f;
//        protected float _timeStep = Mathf.Infinity;

//        protected float _sampleLast = 0.0f;

//        [SerializeField, ReadOnly]
//        protected float _sampleNew = 0.0f;

//        [SerializeField, ReadOnly]
//        protected float _desiredSpeed = 0.0f;

//        protected QuickTrackedObject _trackedObject = null;

//        protected QuickUnityVR _headTracking = null;
//        protected QuickVRPlayArea _vrPlayArea = null;

//        #endregion

//        #region CREATION AND DESTRUCTION

//        protected override void Start()
//        {
//            base.Start();

//            _headTracking = GetComponent<QuickUnityVR>();
//            QuickIKManager ikManager = GetComponent<QuickIKManager>();
//            //ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
//            //ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
//        }

//        protected virtual void OnEnable()
//        {
//            StartCoroutine(CoUpdate());
//        }

//        protected virtual void Init()
//        {
//            _sampleLast = _sampleNew = 0.0f;
//            _timeLastStep = Time.time;
//            _timeStep = Mathf.Infinity;
//            _desiredSpeed = 0.0f;
//        }

//        #endregion

//        #region GET AND SET

//        protected override void ComputeTargetLinearVelocity()
//        {

//        }

//        protected override void ComputeTargetAngularVelocity()
//        {

//        }

//        public override float GetMaxLinearSpeed()
//        {
//            return _speedMax;
//        }

//        protected virtual float ComputeDesiredSpeed(float tStep)
//        {
//            if (tStep > MAX_TIME_STEP) return 0.0f;

//            return (1.0f - ((tStep - MIN_TIME_STEP) / (MAX_TIME_STEP - MIN_TIME_STEP))) * (_speedMax - _speedMin) + _speedMin;
//        }

//        protected virtual float GetSample()
//        {
//            return _trackedObject.GetVelocity().y;
//        }

//        #endregion

//        #region UPDATE

//        protected virtual IEnumerator CoUpdate()
//        {
//            //Wait for the node of the head to be created. 
//            QuickUnityVR hTracking = GetComponent<QuickUnityVR>();
//            QuickVRNode nodeHead = null;
//            while (nodeHead == null)
//            {
//                nodeHead = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
//                yield return null;
//            }
//            _trackedObject = nodeHead.GetTrackedObject();

//            while (true)
//            {
//                //Debug.Log("disp = " + disp.ToString("f3"));

//                UpdateTrackedNode();

//                //Wait for a new sample
//                yield return StartCoroutine(CoUpdateSample());

//                UpdateTargetLinearVelocity();

//                //Check the real displacement of the user in the room. If it is big enough, the contribution
//                //of the WiP is ignored. 
//                //Vector3 disp = Vector3.Scale(_headTracking.GetDisplacement(), Vector3.forward + Vector3.right);

//                //if (disp.magnitude > 0.005f)
//                //{
//                //    _rigidBody.velocity = Vector3.Scale(_rigidBody.velocity, Vector3.up);
//                //    Init();
//                //}
//            }
//        }

//        protected virtual void UpdateTrackedNode()
//        {
//            QuickVRNode hipsNode = _vrPlayArea.GetVRNode(HumanBodyBones.Hips);
//            if (hipsNode)
//            {
//                QuickTrackedObject tObject = hipsNode.IsTracked()? hipsNode.GetTrackedObject() : _vrPlayArea.GetVRNode(HumanBodyBones.Head).GetTrackedObject();
//                if (tObject != _trackedObject)
//                {
//                    _trackedObject = tObject;

//                    Init();
//                }
//            }
//        }

//        protected virtual void UpdateTargetLinearVelocity()
//        {
//            if ((_sampleLast <= 0) && (_sampleNew > 0))
//            {
//                //We have reached a local minima, i.e., a step has been detected. 
//                //Compute the time step. 
//                _timeStep = Time.time - _timeLastStep;
//                _timeLastStep = Time.time;

//                if (_timeStep >= MIN_TIME_STEP)
//                {
//                    _desiredSpeed = ComputeDesiredSpeed(_timeStep);
//                }
//            }

//            _sampleLast = _sampleNew;

//            float timeSinceLastStep = Time.time - _timeLastStep;
//            if (timeSinceLastStep >= MIN_TIME_STEP)
//            {
//                _desiredSpeed = Mathf.Lerp(ComputeDesiredSpeed(_timeStep), 0.0f, timeSinceLastStep / _timeStep);
//            }

//            // Calculate how fast we should be moving
//            _targetLinearVelocity = transform.forward * _desiredSpeed;
//            _rigidBody.velocity = transform.forward * _rigidBody.velocity.magnitude;
//        }

//        protected virtual IEnumerator CoUpdateSample()
//        {
//            float sample = 0.0f;
//            int numSamples = 5;

//            for (int i = 0; i < numSamples; i++)
//            {
//                yield return new WaitForFixedUpdate();

//                sample += GetSample();
//            }
//            sample /= (float)numSamples;

//            if (Mathf.Abs(sample - _sampleNew) > 0.05f) _sampleNew = sample;
//        }

//        #endregion

//    }

//}
