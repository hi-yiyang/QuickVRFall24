using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR.Interaction.Locomotion
{

    public class QuickTeleportationProvider : TeleportationProvider, IQuickLocomotionProvider
    {

        #region UPDATE

        public void UpdateLocomotion()
        {
            if (QuickVRInteractionManager._instance._locomotionTeleportEnabled)
            {
                QuickVRInteractionManager._instance._characterController.enabled = false;

                Update();

                QuickVRInteractionManager._instance._characterController.enabled = true;
            }
        }

        #endregion

    }

}


