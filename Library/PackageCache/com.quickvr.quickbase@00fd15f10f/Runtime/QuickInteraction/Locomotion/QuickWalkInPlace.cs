using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

namespace QuickVR.Interaction.Locomotion
{

    public class QuickWalkInPlace : MonoBehaviour
    {

        #region CONSTANTS

        protected const float MIN_TIME_STEP = 0.2f;    //5 steps per second
        protected const float MAX_TIME_STEP = 1.0f;    //1 steps per second

        #endregion

        #region PUBLIC ATTRIBUTES

        public float _speedMin = 1.0f;  //1m/s  =>  3.6km/h
        public float _speedMax = 5.0f;  //5m/s  =>  18km/h

        public float _positiveAccelerationFactor = 1.0f;
        public float _negativeAccelerationFactor = 10.0f;

        public float _currentSpeed
        {
            get; protected set;
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _timeLastStep = -1.0f;
        protected float _timeStep = Mathf.Infinity;

        protected float _sampleLast = 0.0f;

        [SerializeField, ReadOnly]
        protected float _sampleNew = 0.0f;

        protected QuickTrackedObject _trackedObject = null;

        protected float _targetSpeed = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            StartCoroutine(CoUpdate());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }

        protected virtual void Start()
        {
            Reset();
        }

        protected virtual void Reset()
        {
            _trackedObject = QuickXRRig._instance.GetVRNode(HumanBodyBones.Head).GetTrackedObject();
            _trackedObject._trackData = true;

            _sampleLast = _sampleNew = 0.0f;
            _timeLastStep = Time.time;
            _timeStep = Mathf.Infinity;
            _targetSpeed = 0.0f;
        }

        #endregion

        #region GET AND SET

        protected virtual float ComputeTargetSpeed(float tStep)
        {
            float t = 1.0f - ((tStep - MIN_TIME_STEP) / (MAX_TIME_STEP - MIN_TIME_STEP));

            return Mathf.Lerp(_speedMin, _speedMax, t);
        }

        protected virtual float GetSample()
        {
            return _trackedObject.GetVelocity().y;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            float f = _targetSpeed > _currentSpeed ? _positiveAccelerationFactor : _negativeAccelerationFactor;
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * f);
        }

        protected virtual IEnumerator CoUpdate()
        {
            while (true)
            {
                //Wait for a new sample
                yield return StartCoroutine(CoUpdateSample());

                UpdateTargetLinearVelocity();

                //Check the real displacement of the user in the room. If it is big enough, the contribution
                //of the WiP is ignored. 
                //Vector3 disp = Vector3.Scale(_headTracking.GetDisplacement(), Vector3.forward + Vector3.right);

                //if (disp.magnitude > 0.005f)
                //{
                //    _rigidBody.velocity = Vector3.Scale(_rigidBody.velocity, Vector3.up);
                //    Init();
                //}
            }
        }

        protected virtual void UpdateTargetLinearVelocity()
        {
            if ((_sampleLast <= 0) && (_sampleNew > 0))
            {
                //We have reached a local minima, i.e., a step has been detected. 
                //Compute the time step. 
                _timeStep = Time.time - _timeLastStep;
                _timeLastStep = Time.time;

                if (_timeStep >= MIN_TIME_STEP)
                {
                    _targetSpeed = ComputeTargetSpeed(_timeStep);
                }
            }

            _sampleLast = _sampleNew;

            //float timeSinceLastStep = Time.time - _timeLastStep;
            //if (timeSinceLastStep >= MAX_TIME_STEP / 2)
            //{
            //    _targetSpeed = 0;
            //}

            //float timeSinceLastStep = Time.time - _timeLastStep;
            //if (timeSinceLastStep >= MIN_TIME_STEP)
            //{
            //    _targetSpeed = Mathf.Lerp(ComputeTargetSpeed(_timeStep), 0.0f, timeSinceLastStep / _timeStep);
            //}

            float timeSinceLastStep = Time.time - _timeLastStep;
            _targetSpeed = Mathf.Lerp(_targetSpeed, 0, timeSinceLastStep / MAX_TIME_STEP);
        }

        protected virtual IEnumerator CoUpdateSample()
        {
            float sample = 0.0f;
            int numSamples = 5;

            for (int i = 0; i < numSamples; i++)
            {
                yield return new WaitForFixedUpdate();

                sample += GetSample();
            }
            sample /= (float)numSamples;

            if (Mathf.Abs(sample - _sampleNew) > 0.05f) _sampleNew = sample;
        }

        #endregion

    }

}
