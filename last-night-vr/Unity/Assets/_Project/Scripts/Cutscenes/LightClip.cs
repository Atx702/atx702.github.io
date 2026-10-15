using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LastNight.Cutscenes
{
    /// <summary>Timeline track that fades a Light's intensity and colour (the room turning gold, lamps failing).</summary>
    [TrackColor(1f, 0.9f, 0.5f)]
    [TrackBindingType(typeof(Light))]
    [TrackClipType(typeof(LightClip))]
    public class LightTrack : TrackAsset { }

    [Serializable]
    public class LightClip : PlayableAsset, ITimelineClipAsset
    {
        public float fromIntensity = 1f, toIntensity = 1f;
        public Color fromColor = Color.white, toColor = Color.white;
        [Tooltip("Random flicker on top, 0 = steady.")]
        public float flicker;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var p = ScriptPlayable<LightBehaviour>.Create(graph);
            p.GetBehaviour().clip = this;
            return p;
        }
    }

    public class LightBehaviour : PlayableBehaviour
    {
        public LightClip clip;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var l = playerData as Light;
            if (l == null || clip == null) return;
            double d = playable.GetDuration();
            float k = d > 0 ? Mathf.Clamp01((float)(playable.GetTime() / d)) : 1f;
            float i = Mathf.Lerp(clip.fromIntensity, clip.toIntensity, k);
            if (clip.flicker > 0f && UnityEngine.Random.value < clip.flicker) i *= 0.1f;
            l.intensity = i;
            l.color = Color.Lerp(clip.fromColor, clip.toColor, k);
            l.enabled = i > 0.001f;
        }
    }
}
