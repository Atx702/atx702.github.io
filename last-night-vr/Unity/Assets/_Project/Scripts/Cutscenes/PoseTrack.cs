using System;
using LastNight.Characters;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LastNight.Cutscenes
{
    /// <summary>Timeline track that switches a character's move (walk, kneel, CPR, wave...) while a clip plays.</summary>
    [TrackColor(0.6f, 0.8f, 0.6f)]
    [TrackBindingType(typeof(PuppetAnimator))]
    [TrackClipType(typeof(PoseClip))]
    public class PoseTrack : TrackAsset { }

    [Serializable]
    public class PoseClip : PlayableAsset, ITimelineClipAsset
    {
        public PuppetAnimator.Move move;
        [Tooltip("Compressions per second while this clip plays (Cpr only). 0 keeps the current rate.")]
        public float cprRate;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var p = ScriptPlayable<PoseBehaviour>.Create(graph);
            p.GetBehaviour().clip = this;
            return p;
        }
    }

    public class PoseBehaviour : PlayableBehaviour
    {
        public PoseClip clip;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var puppet = playerData as PuppetAnimator;
            if (puppet == null || clip == null) return;
            if (clip.cprRate > 0f) puppet.SetCprRate(clip.cprRate);
            puppet.Play(clip.move);
        }
    }
}
