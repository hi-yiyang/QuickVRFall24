using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace QuickVR
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of Meta Quest Pro controller interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "[QuickVR] Meta Quest Touch Pro Controller Profile",
        BuildTargetGroups = new[] { UnityEditor.BuildTargetGroup.Standalone, UnityEditor.BuildTargetGroup.WSA, UnityEditor.BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows for mapping input to the Meta Quest Touch Pro Controller interaction profile.",
        DocumentationLink = UnityEngine.XR.OpenXR.Constants.k_DocumentationManualURL + "features/metaquesttouchprocontrollerprofile.html",
        OpenxrExtensionStrings = "XR_FB_touch_controller_pro",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = "com.quickvr.input.metaquestpro")]
#endif

    public class QuickMetaQuestTouchProControllerProfile : MetaQuestTouchProControllerProfile
    {

        protected override void RegisterActionMapsWithRuntime()
        {
            if (Application.isEditor ||
                QuickVRManager._hmdModel == QuickVRManager.HMDModel.QuestPro)
            {
                base.RegisterActionMapsWithRuntime();
            }
        }

    }

}