using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public abstract class QuickTrackedDevice 
    {

        #region PROTECTED ATTRIBUTES

        public QuickVRNode _vrNode = null;

        #endregion

        #region GET AND SET

        public abstract bool IsTracking();

        public abstract Vector3 GetTrackedPosition();
        public abstract Quaternion GetTrackedRotation();

        #endregion

        #region UPDATE

        public virtual void CheckDevice() 
        {
            
        }

        public virtual void UpdateTracking()
        {
            _vrNode.transform.localPosition = GetTrackedPosition();
            _vrNode.transform.localRotation = GetTrackedRotation();
        }

        #endregion


    }

}


