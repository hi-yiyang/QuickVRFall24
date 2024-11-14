using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [Serializable]
    public struct QuickTuple<T1, T2>
    {
        public T1 _item1;
        public T2 _item2;

        public QuickTuple(T1 item1, T2 item2)
        {
            _item1 = item1;
            _item2 = item2;
        }
    }

    public class QuickVRHandOffset : MonoBehaviour
    {

        public Transform _handOrigin = null;

        public QuickTuple<Vector3, Quaternion> _offsetOpenXR;
        public QuickTuple<Vector3, Quaternion> _offsetOculusXR;

        protected QuickTuple<Vector3, Quaternion> _offsetOrigin
        {
            get
            {
                QuickTuple<Vector3, Quaternion> result = new QuickTuple<Vector3, Quaternion>();
                QuickVRManager.XRPlugin xrPlugin = QuickVRManager._xrPlugin;

                if (xrPlugin == QuickVRManager.XRPlugin.OpenXR)
                {
                    result = _offsetOpenXR;
                }
                else if (xrPlugin == QuickVRManager.XRPlugin.OculusXR)
                {
                    result = _offsetOculusXR;
                }

                return result;
            }
        }

        void Update()
        {
            _handOrigin.localPosition = _offsetOrigin._item1;
            _handOrigin.localRotation = _offsetOrigin._item2;
        }
    }


}

