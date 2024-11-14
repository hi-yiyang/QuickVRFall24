using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Utilities;

using UnityEngine.XR.OpenXR.Features.Interactions;

namespace QuickVR
{

    public static class QuickTrackedDeviceManager
    {

        public static TrackedDevice _dummyDevice = new TrackedDevice();

        #region GET AND SET

        public static TrackedDevice GetTrackedDeviceAt(HumanBodyBones boneID)
        {
            return GetTrackedDeviceAt((QuickHumanBodyBones)boneID);
        }

        public static TrackedDevice GetTrackedDeviceAt(QuickHumanBodyBones boneID)
        {
            TrackedDevice tDevice = null;

            if (boneID == QuickHumanBodyBones.Head)
            {
                tDevice = InputSystem.GetDevice<XRHMD>();
            }
            else if (boneID == QuickHumanBodyBones.LeftHand)
            {
                tDevice = XRController.leftHand;
            }
            else if (boneID == QuickHumanBodyBones.RightHand)
            {
                tDevice = XRController.rightHand;
            }
            else if (boneID == QuickHumanBodyBones.LeftHandAim || boneID == QuickHumanBodyBones.RightHandAim)
            {
                QuickVRHand vrHand = QuickVRHandTrackingManager.GetVRHand(boneID == QuickHumanBodyBones.LeftHandAim);
                if (vrHand != null)
                {
                    tDevice = vrHand._metaHand;
                }
            }
            else if (boneID == QuickHumanBodyBones.Hips)
            {
                tDevice = GetTrackedDeviceWithUsage(QuickVRUsages.hips);
            }
            else if (boneID == QuickHumanBodyBones.LeftFoot)
            {
                tDevice = GetTrackedDeviceWithUsage(QuickVRUsages.leftFoot);
            }
            else if (boneID == QuickHumanBodyBones.RightFoot)
            {
                tDevice = GetTrackedDeviceWithUsage(QuickVRUsages.rightFoot);
            }

            return tDevice;
        }

        public static TrackedDevice GetTrackedDeviceWithUsage(string usage)
        {
            TrackedDevice result = null;

            foreach (InputDevice d in InputSystem.devices)
            {
                if (d is TrackedDevice)
                {
                    TrackedDevice tmp = (TrackedDevice)d;
                    if (tmp.IsValid() && tmp.HasUsage(usage))
                    {
                        result = tmp;
                    }
                }                
            }

            return result;
        }

        #endregion

        #region EXTENSIONS

        public static bool IsTracked(this TrackedDevice device)
        {
            return device.IsValid() && device.isTracked.isPressed;
        }

        public static void PrintControls(this InputDevice device)
        {
            ReadOnlyArray<InputControl> controls = device.allControls;
            foreach (InputControl c in controls)
            {
                Debug.Log(c.path);
            }
        }

        public static bool HasUsage(this InputDevice device, string usage)
        {
            foreach (string s in device.usages)
            {
                if (s.CompareTo(usage) == 0) return true;
            }

            return false;
        }

        #endregion

    }

}


