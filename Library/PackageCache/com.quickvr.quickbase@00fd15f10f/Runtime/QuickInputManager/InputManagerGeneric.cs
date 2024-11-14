using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace QuickVR
{
    ///<summary>
    ///T is The type of the InputDevice; 
    ///U is The enum type defining the Axis for this InputDevice; 
    ///V is The enum type defining the Buttons for this InputDevice;
    /// </summary>

    public abstract class InputManagerGeneric<T, U, V> : BaseInputManager<U, V>
    where T : InputDevice   
    where U : struct
    where V : struct
    {

        #region PROTECTED ATTRIBUTES

        protected T _inputDevice
        {
            get
            {
                if (m_InputDevice == null)
                {
                    m_InputDevice = GetInputDevice();
                }

                return m_InputDevice;
            }
            set
            {
                m_InputDevice = value;
            }
        }
        private T m_InputDevice = null;
        
        #endregion

        #region GET AND SET

        protected abstract T GetInputDevice();

        protected override float ImpGetAxis(string axisName)
        {
            float result = 0;

            U axis = _stringToAxis[axisName];
            SetInputDevice(axis);

            if (_inputDevice != null && _inputDevice.added)
            {
                result = ImpGetAxis(axis);
            }

            return result;
        }

        protected override bool ImpGetButton(string buttonName)
        {
            bool result = false;

            V button = _stringToButton[buttonName];
            SetInputDevice(button);

            if (_inputDevice != null && _inputDevice.added)
            {
                result = ImpGetButton(button);
            }

            return result;
        }

        #endregion

    }
}


