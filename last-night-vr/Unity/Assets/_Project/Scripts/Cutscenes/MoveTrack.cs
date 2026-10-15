using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LastNight.Cutscenes
{
    /// <summary>
    /// Timeline track that glides a Transform between two local poses. Cutscenes are staged around the
    /// player (never moving the camera), so most of their motion is "this object drifts from A to B".
    /// </summary>
    [TrackColor(0.85f, 0.75f, 0.4f)]
    [TrackBindingType(typeof(Transform))]
    [TrackClipType(typeof(MoveClip))]
    public class MoveTrack : TrackAsset { }

    [Serializable]
    public class MoveClip : PlayableAsset, ITimelineClipAsset
    {
        public Vector3 fromPosition, toPosition;
        public Vector3 fromEuler, toEuler;
        public bool rotate;
        public Vector3 fromScale = Vector3.one, toScale = Vector3.one;
        public bool scale;
        [Tooltip("Vertical bob added on top (CPR compressions, breathing): amplitude in metres, cycles per second.")]
        public float bobAmplitude, bobFrequency;
        public bool easeInOut = true;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MoveBehaviour>.Create(graph);
            playable.GetBehaviour().clip = this;
            return playable;
        }
    }

    public class MoveBehaviour : PlayableBehaviour
    {
        public MoveClip clip;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var t = playerData as Transform;
            if (t == null || clip == null) return;
            double duration = playable.GetDuration();
            float k = duration > 0 ? Mathf.Clamp01((float)(playable.GetTime() / duration)) : 1f;
            if (clip.easeInOut) k = k * k * (3f - 2f * k);

            var pos = Vector3.Lerp(clip.fromPosition, clip.toPosition, k);
            if (clip.bobAmplitude > 0f)
                pos.y += Mathf.Sin((float)playable.GetTime() * clip.bobFrequency * Mathf.PI * 2f) * clip.bobAmplitude;
            t.localPosition = pos;
            if (clip.rotate) t.localRotation = Quaternion.Slerp(Quaternion.Euler(clip.fromEuler), Quaternion.Euler(clip.toEuler), k);
            if (clip.scale) t.localScale = Vector3.Lerp(clip.fromScale, clip.toScale, k);
        }
    }
}
