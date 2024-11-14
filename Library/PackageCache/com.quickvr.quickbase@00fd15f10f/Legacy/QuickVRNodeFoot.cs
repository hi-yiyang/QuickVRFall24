using UnityEngine;

namespace QuickVR
{

    public class QuickVRNodeFoot : QuickVRNode
    {

        #region UPDATE

        public override void UpdateState()
        {
            base.UpdateState();

            if (!_isTracked)
            {
                Animator animatorSrc = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource();
                Transform tBoneFoot = animatorSrc.GetBoneTransformNormalized(_role);

                //Update foot position
                transform.position = tBoneFoot.position;

                //Update foot rotation
                transform.rotation = tBoneFoot.rotation;

                _state = State.Inferred;
            }
        }

        #endregion

    }

}
