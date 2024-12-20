using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class PlayWithTimeControlSample : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip clip;
    public float time;

    #endregion

    #region PROTECTED ATTRIBUTES

    AnimationClipPlayable playableClip;

    #endregion

    #region CREATION AND DESTRUCTION

    protected override void InitPlayableGraph()
    {
        // Wrap the clip in a playable
        playableClip = AnimationClipPlayable.Create(_playableGraph, clip);

        // Connect the Playable to an output
        _playableOutput.SetSourcePlayable(playableClip);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Stops time from progressing automatically.
        playableClip.Pause();
    }

    #endregion

    #region UPDATE

    protected virtual void Update()
    {
        // Control the time manually
        playableClip.SetTime(time);
    }

    #endregion

}
