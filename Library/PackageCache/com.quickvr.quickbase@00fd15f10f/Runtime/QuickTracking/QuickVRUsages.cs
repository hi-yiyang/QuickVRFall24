using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

namespace QuickVR
{

    public static class QuickVRUsages
    {

        public static string leftFoot
        {
            get
            {
                return "Left Foot";
            }
        }

        public static string rightFoot
        {
            get
            {
                return "Right Foot";
            }
        }

        public static string leftShoulder
        {
            get
            {
                return "Left Shoulder";
            }
        }

        public static string rightShoulder
        {
            get
            {
                return "Right Shoulder";
            }
        }

        public static string leftElbow
        {
            get
            {
                return "Left Elbow";
            }
        }

        public static string rightElbow
        {
            get
            {
                return "Right Elbow";
            }
        }

        public static string leftKnee
        {
            get
            {
                return "Left Knee";
            }
        }

        public static string rightKnee
        {
            get
            {
                return "Right Knee";
            }
        }

        public static string leftWrist
        {
            get
            {
                return "Left Wrist";
            }
        }

        public static string rightWrist
        {
            get
            {
                return "Right Wrist";
            }
        }

        public static string leftAnkle
        {
            get
            {
                return "Left Ankle";
            }
        }

        public static string rightAnkle
        {
            get
            {
                return "Right Ankle";
            }
        }

        public static string hips
        {
            get
            {
                return "Waist";
            }
        }

        //public static string thumbStick
        //{
        //    get
        //    {

        //    }
        //}

        #region TRIGGER TOUCH

        public static InputFeatureUsage<bool> triggerTouch
        {
            get
            {
                if (QuickVRManager._xrPlugin == QuickVRManager.XRPlugin.OpenXR)
                {
                    return triggerTouchOpenXR;
                }

                return triggerTouchDefault;
            }
        }
        private static InputFeatureUsage<bool> triggerTouchDefault = new InputFeatureUsage<bool>("IndexTouch");
        private static InputFeatureUsage<bool> triggerTouchOpenXR = new InputFeatureUsage<bool>("TriggerTouch");

        #endregion

        public static InputFeatureUsage<Vector3> pointerPosition = new InputFeatureUsage<Vector3>("PointerPosition");
        public static InputFeatureUsage<Quaternion> pointerRotation = new InputFeatureUsage<Quaternion>("PointerRotation");

        public static InputFeatureUsage<Vector3> combineEyePoint = new InputFeatureUsage<Vector3>("CombinedEyeGazePoint");
        public static InputFeatureUsage<Vector3> combineEyeVector = new InputFeatureUsage<Vector3>("CombinedEyeGazeVector");
        public static InputFeatureUsage<Vector3> leftEyePoint = new InputFeatureUsage<Vector3>("LeftEyeGazePoint");
        public static InputFeatureUsage<Vector3> leftEyeVector = new InputFeatureUsage<Vector3>("LeftEyeGazeVector");
        public static InputFeatureUsage<Vector3> rightEyePoint = new InputFeatureUsage<Vector3>("RightEyeGazePoint");
        public static InputFeatureUsage<Vector3> rightEyeVector = new InputFeatureUsage<Vector3>("RightEyeGazeVector");
        public static InputFeatureUsage<float> leftEyeOpenness = new InputFeatureUsage<float>("LeftEyeOpenness");
        public static InputFeatureUsage<float> rightEyeOpenness = new InputFeatureUsage<float>("RightEyeOpenness");
        public static InputFeatureUsage<uint> leftEyePoseStatus = new InputFeatureUsage<uint>("LeftEyePoseStatus");
        public static InputFeatureUsage<uint> rightEyePoseStatus = new InputFeatureUsage<uint>("RightEyePoseStatus");
        public static InputFeatureUsage<uint> combinedEyePoseStatus = new InputFeatureUsage<uint>("CombinedEyePoseStatus");
        public static InputFeatureUsage<float> leftEyePupilDilation = new InputFeatureUsage<float>("LeftEyePupilDilation");
        public static InputFeatureUsage<float> rightEyePupilDilation = new InputFeatureUsage<float>("RightEyePupilDilation");
        public static InputFeatureUsage<Vector3> leftEyePositionGuide = new InputFeatureUsage<Vector3>("LeftEyePositionGuide");
        public static InputFeatureUsage<Vector3> rightEyePositionGuide = new InputFeatureUsage<Vector3>("RightEyePositionGuide");
        public static InputFeatureUsage<Vector3> foveatedGazeDirection = new InputFeatureUsage<Vector3>("FoveatedGazeDirection");
        public static InputFeatureUsage<uint> foveatedGazeTrackingState = new InputFeatureUsage<uint>("FoveatedGazeTrackingState");
    }

}
