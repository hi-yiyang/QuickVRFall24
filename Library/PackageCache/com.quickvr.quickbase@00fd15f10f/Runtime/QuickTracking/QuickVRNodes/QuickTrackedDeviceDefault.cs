using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuickVR
{

    public class QuickTrackedDeviceDefault : QuickTrackedDevice
    {

        #region PUBLIC ATTRIBUTES

        public TrackedDevice _inputDevice
        {
            get
            {
                return m_InputDevice != null ? m_InputDevice : QuickTrackedDeviceManager._dummyDevice;
            }
            protected set
            {
                if (value != null)
                {
                    m_InputDevice = value;
                    if (m_InputDevice.IsValid())
                    {
                        _vrNode.GetTrackedObject().transform.ResetTransformation();
                        _vrNode.UpdateState();
                    }
                }
            }
        }
        protected TrackedDevice m_InputDevice = null;

        #endregion

        #region GET AND SET

        public override void CheckDevice()
        {
            if (!_inputDevice.IsValid())
            {
                _inputDevice = QuickTrackedDeviceManager.GetTrackedDeviceAt(_vrNode.GetRole());
            }
        }

        public override bool IsTracking()
        {
            return _inputDevice.IsTracked();
        }

        public override Vector3 GetTrackedPosition()
        {
            return _inputDevice.devicePosition.ReadValue();
        }

        public override Quaternion GetTrackedRotation()
        {
            return _inputDevice.deviceRotation.ReadValue();
        }

        #endregion

    }

}

