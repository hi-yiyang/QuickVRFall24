using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class PlayAnimationSample : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip _clip;

    public float _startTime = 0;

    #endregion

    #region CREATION AND DESTRUCTION

    protected override void InitPlayableGraph()
    {
        //AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _clip, out _playableGraph);
        AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", GetComponent<Animator>());

        // Wrap the clip in a playable
        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_playableGraph, _clip);
        clipPlayable.SetTime(_startTime);

        // Connect the Playable to an output
        playableOutput.SetSourcePlayable(clipPlayable);
    }

    #endregion

}
