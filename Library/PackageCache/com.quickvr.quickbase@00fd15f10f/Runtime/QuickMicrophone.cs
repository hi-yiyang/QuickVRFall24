using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR 
{

    public class QuickMicrophone : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _isInitialized {get; protected set;}

        public float _amplifyFactor = 1;

        public enum SamplingRate
        {
            HZ_08k   = 8000, 
            HZ_16k  = 16000, 
            HZ_22k  = 22000, 
            HZ_44k  = 44100, 
        }
        public SamplingRate _samplingRate = SamplingRate.HZ_44k;

        //The AudioClip that is being used for recording. 
        public AudioClip _audioClip { get; protected set; }

        //currentSample contains the current sample in the _audioClip where the microphone is recording. 
        public int _currentSample
        {
            get
            {
                return _isInitialized ? Microphone.GetPosition("") : 0;
            }
        }

        public float _silenceThreshold = 0.01f;  // Adjust this value to your needs
        public float _maxTimeSilent = 2;
        public bool _isSpeaking
        {
            get; protected set;
        }

        public bool _isRecording {get; protected set; }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected AudioClip _lastRecordedClip = null;   //The last recorded AudioClip

        protected int _sampleStart = 0;
        
        protected float[] _rawData = null;
        protected float[] _speakCheckData = new float[128];

        protected float _timeSilent = 0;    //The time the user is silent (not talking). 

        #endregion

        #region EVENTS

        public delegate void QuickMicrophoneAction();

        public event QuickMicrophoneAction OnSpeakingStart;
        public event QuickMicrophoneAction OnSpeakingEnd;

        #endregion

        #region CONSTANTS

        protected const int MIC_TIME = 600;

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        protected static void Init()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            QuickUtils.CheckPermission(UnityEngine.Android.Permission.Microphone);
#endif
        }

        protected virtual IEnumerator Start()
        {
            while (Microphone.devices.Length == 0)
            {
                yield return null;
            }
            _isInitialized = true;

            _rawData = new float[MIC_TIME * (int)_samplingRate];
            _audioClip = Microphone.Start(null, true, MIC_TIME, (int)_samplingRate);
            Debug.Log("MIC STARTED!!!");
            Debug.Log("NUM SAMPLES = " + _audioClip.samples);
        }

        #endregion

        #region GET AND SET

        public virtual bool IsInitialized()
        {
            return _isInitialized;
        }

        public virtual void Record()
        {
#if UNITY_WEBGL
        Debug.LogError("Microphone does not work on WebGL platform!!!");
#else
            if (_isInitialized && !_isRecording)
            {
                _sampleStart = Microphone.GetPosition("");
                _isRecording = true;
            }
            else
            {
                Debug.LogError("[QuickMicrophone.Record]:" + "_isInitialized = " + _isInitialized + " _isRecording = " + _isRecording);
            }
#endif
        }

        public virtual AudioClip StopRecording()
        {
            if (_isInitialized && _isRecording)
            {
                _lastRecordedClip = GetRecordingClip("MicRecord", _sampleStart, Microphone.GetPosition(""));
                
                //int sampleEnd = Microphone.GetPosition("");
                //int numSamples = sampleEnd > _sampleStart ? sampleEnd - _sampleStart : (_audioClip.samples - _sampleStart) + sampleEnd;
                ////int numSamples = _audioClip.samples;
                
                //_audioClip.GetData(_rawData, 0);
                //float[] recordData = new float[numSamples];
                //for (int i = 0; i < numSamples; i++)
                //{
                //    int sampleID = (_sampleStart + i) % _audioClip.samples;
                //    recordData[i] = _rawData[sampleID] * _amplifyFactor;
                //}

                //_lastRecordedClip = AudioClip.Create("MicRecord", numSamples, 1, MIC_FREQUENCY, false);
                //_lastRecordedClip.SetData(recordData, 0);
            }
            
            _isRecording = false;

            return _lastRecordedClip;
        }

        public virtual float[] GetRecordingData(int sampleStart, int sampleEnd)
        {
            float[] result = null;

            if (_isInitialized)
            {
                int numSamples = sampleEnd > sampleStart ? sampleEnd - sampleStart : (_audioClip.samples - sampleStart) + sampleEnd;
                //int numSamples = _audioClip.samples;

                _audioClip.GetData(_rawData, 0);
                result = new float[numSamples];
                for (int i = 0; i < numSamples; i++)
                {
                    int sampleID = (_sampleStart + i) % _audioClip.samples;
                    result[i] = _rawData[sampleID] * _amplifyFactor;
                }
            }

            return result;
        }

        public virtual AudioClip GetRecordingClip(string recordName, int sampleStart, int sampleEnd)
        {
            AudioClip result = null;
            float[] recordingData = GetRecordingData(sampleStart, sampleEnd);
            
            if (recordingData != null)
            {
                result = AudioClip.Create(recordName, recordingData.Length, 1, (int)_samplingRate, false);
                result.SetData(recordingData, 0);
            }

            return result;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            int sampleCurr = Microphone.GetPosition("");
            _audioClip.GetData(_speakCheckData, sampleCurr - _speakCheckData.Length);
            float loudness = ComputeLoudness(_speakCheckData);

            if (loudness > _silenceThreshold)
            {
                _timeSilent = 0;
                if (!_isSpeaking)
                {
                    _isSpeaking = true;
                    if (OnSpeakingStart != null)
                    {
                        OnSpeakingStart();
                    }
                }
            }

            if (_isSpeaking)
            {
                _timeSilent += Time.deltaTime;
                if (_timeSilent >= _maxTimeSilent)
                {
                    _isSpeaking = false;
                    if (OnSpeakingEnd != null)
                    {
                        OnSpeakingEnd();
                    }
                }
            }
        }

        protected virtual float ComputeLoudness(float[] data)
        {
            // Calculate loudness by measuring the RMS of the audio data
            float loudness = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float sample = data[i];
                loudness += sample * sample;

                data[i] = 0;    //Avoid sound being played back. 
            }
            loudness = Mathf.Sqrt(loudness / data.Length);

            return loudness;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            //// Calculate loudness by measuring the RMS of the audio data
            //float loudness = ComputeLoudness(data);

            //if (loudness > _silenceThreshold)
            //{
            //    _timeSilent = 0;
            //    if (!_isSpeaking)
            //    {
            //        _isSpeaking = true;
            //        if (OnSpeakingStart != null)
            //        {
            //            OnSpeakingStart();
            //        }
            //    }
            //}
        }

        #endregion

    }

}
