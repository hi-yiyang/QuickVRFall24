using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace QuickVR
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Oculus TouchControllers interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "[QuickVR] Oculus Touch Controller Profile",
        BuildTargetGroups = new[] { UnityEditor.BuildTargetGroup.Standalone, UnityEditor.BuildTargetGroup.WSA, UnityEditor.BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows for mapping input to the Oculus Touch Controller interaction profile.",
        DocumentationLink = UnityEngine.XR.OpenXR.Constants.k_DocumentationManualURL + "features/oculustouchcontrollerprofile.html",
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = "com.quickvr.input.oculustouch")]
#endif

    public class QuickOculusTouchControllerProfile : OculusTouchControllerProfile
    {

        protected override void RegisterActionMapsWithRuntime()
        {
            if (Application.isEditor || 
                QuickVRManager._hmdModel == QuickVRManager.HMDModel.Quest || 
                QuickVRManager._hmdModel == QuickVRManager.HMDModel.Quest2)
            {
                base.RegisterActionMapsWithRuntime();
            }
        }

    }
}
