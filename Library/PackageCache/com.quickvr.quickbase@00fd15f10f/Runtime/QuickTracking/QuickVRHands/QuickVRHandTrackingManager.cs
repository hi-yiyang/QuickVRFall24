using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.OpenXR;

namespace QuickVR
{

    public abstract class QuickVRHandTrackingManagerBase
    {

        public abstract bool IsHandTrackingEnabled();

        public virtual void Update()
        {

        }

    }

    public class QuickVRHandTrackingManager : QuickVRHandTrackingManagerBase
    {

        #region PUBLIC ATTRIBUTES

        public static XRHandSubsystem _handSubsystem
        {
            get
            {
                if (m_HandSubsystem == null)
                {
                    //Avoid using the hand tracking when using the SteamVR over OpenXR, as it does not work
                    //as expected yet.
                    if (QuickVRManager._xrPlugin != QuickVRManager.XRPlugin.OpenXR || !OpenXRRuntime.name.ToLower().Contains("steamvr"))
                    {
                        SubsystemManager.GetSubsystems(s_SubsystemsReuse);
                        if (s_SubsystemsReuse.Count > 0)
                        {
                            m_HandSubsystem = s_SubsystemsReuse[0];
                        }
                    }
                }

                return m_HandSubsystem;
            }

            protected set
            {
                m_HandSubsystem = value;
            }
        }

        public static QuickVRHand _handLeft { get; protected set; }
        public static QuickVRHand _handRight { get; protected set; }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_handSubsystem != null)
            {
                _handLeft = new QuickVRHand(true);
                _handRight = new QuickVRHand(false);
            }
            else
            {
                Debug.LogWarning("[QuickVRHand] Hand Subsystem not found!!!");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterTrackingManager()
        {
            QuickVRManager.RegisterHandTrackingManager(new QuickVRHandTrackingManager());
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected static XRHandSubsystem m_HandSubsystem = null;
        protected static readonly List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();

        #endregion

        #region GET AND SET

        public static QuickVRHand GetVRHand(bool isLeft)
        {
            return isLeft ? _handLeft : _handRight;
        }

        public override bool IsHandTrackingEnabled()
        {
            return _handSubsystem != null && (_handLeft._isTracked || _handRight._isTracked);
        }

        #endregion

    }

}
