using System.Collections.Generic;
using System.IO;
using LastNight.Cutscenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Builds a Timeline cutscene in a few lines:
    /// <code>Cut.New("Ch3_TrainArrives", host, 12f).Move(train, 0, 10, from, to).Active(maren, 9, 3).Sound("SFX_train_arrive", 0).Done();</code>
    /// The camera is never moved: cutscenes are staged around the player.
    /// </summary>
    public class Cut
    {
        public const string Folder = Kit.Root + "/Timelines";

        TimelineAsset timeline;
        PlayableDirector director;
        readonly Dictionary<Object, TrackAsset> tracks = new Dictionary<Object, TrackAsset>();

        public static Cut New(string id, GameObject host, float duration)
        {
            Directory.CreateDirectory(Folder);
            var path = $"{Folder}/{id}.playable";
            AssetDatabase.DeleteAsset(path);
            var tl = ScriptableObject.CreateInstance<TimelineAsset>();
            tl.name = id;
            AssetDatabase.CreateAsset(tl, path);
            tl.durationMode = TimelineAsset.DurationMode.FixedLength;
            tl.fixedDuration = duration;

            var dir = Kit.Add<PlayableDirector>(host);
            dir.playableAsset = tl;
            dir.playOnAwake = false;
            dir.extrapolationMode = DirectorWrapMode.None;
            dir.timeUpdateMode = DirectorUpdateMode.GameTime;
            return new Cut { timeline = tl, director = dir };
        }

        T Track<T>(Object target, string name) where T : TrackAsset, new()
        {
            if (tracks.TryGetValue(target, out var existing) && existing is T t) return t;
            var track = timeline.CreateTrack<T>(null, name);
            director.SetGenericBinding(track, target);
            tracks[target] = track;
            return track;
        }

        /// <summary>Glides <paramref name="t"/> between two local positions (and rotations if given).</summary>
        public Cut Move(Transform t, float start, float duration, Vector3 from, Vector3 to,
            Vector3? fromEuler = null, Vector3? toEuler = null, float bobAmplitude = 0f, float bobFrequency = 0f, bool ease = true)
        {
            var track = Track<MoveTrack>(t, "Move " + t.name);
            var clip = track.CreateClip<MoveClip>();
            clip.start = start;
            clip.duration = duration;
            var m = (MoveClip)clip.asset;
            m.fromPosition = from;
            m.toPosition = to;
            m.rotate = fromEuler.HasValue || toEuler.HasValue;
            m.fromEuler = fromEuler ?? t.localEulerAngles;
            m.toEuler = toEuler ?? m.fromEuler;
            m.bobAmplitude = bobAmplitude;
            m.bobFrequency = bobFrequency;
            m.easeInOut = ease;
            EditorUtility.SetDirty(m);
            return this;
        }

        /// <summary>Holds an object still but bobbing (CPR compressions, breathing).</summary>
        public Cut Bob(Transform t, float start, float duration, float amplitude, float frequency)
            => Move(t, start, duration, t.localPosition, t.localPosition, null, null, amplitude, frequency, false);

        public Cut Scale(Transform t, float start, float duration, Vector3 from, Vector3 to)
        {
            var track = Track<MoveTrack>(t, "Move " + t.name);
            var clip = track.CreateClip<MoveClip>();
            clip.start = start;
            clip.duration = duration;
            var m = (MoveClip)clip.asset;
            m.fromPosition = m.toPosition = t.localPosition;
            m.scale = true;
            m.fromScale = from;
            m.toScale = to;
            EditorUtility.SetDirty(m);
            return this;
        }

        /// <summary>The object is active only between start and start + duration while this cutscene plays; it keeps its last state afterwards.</summary>
        public Cut Active(GameObject go, float start, float duration)
        {
            var track = Track<ActivationTrack>(go, "Show " + go.name);
            track.postPlaybackState = ActivationTrack.PostPlaybackState.LeaveAsIs;
            var clip = track.CreateDefaultClip();
            clip.start = start;
            clip.duration = duration;
            return this;
        }

        public Cut Sound(string clipId, float start, AudioSource source = null, float volume = 1f)
        {
            var audio = Kit.Clip(clipId);
            if (audio == null) return this;
            var track = timeline.CreateTrack<AudioTrack>(null, "Audio " + clipId);
            if (source != null)
            {
                director.SetGenericBinding(track, source);
                source.volume = volume;
            }
            var clip = track.CreateClip(audio);
            clip.start = start;
            return this;
        }

        public Cut Light(Light l, float start, float duration, float fromIntensity, float toIntensity, Color? fromColor = null, Color? toColor = null, float flicker = 0f)
        {
            var track = Track<LightTrack>(l, "Light " + l.name);
            var clip = track.CreateClip<LightClip>();
            clip.start = start;
            clip.duration = duration;
            var lc = (LightClip)clip.asset;
            lc.fromIntensity = fromIntensity;
            lc.toIntensity = toIntensity;
            lc.fromColor = fromColor ?? l.color;
            lc.toColor = toColor ?? lc.fromColor;
            lc.flicker = flicker;
            EditorUtility.SetDirty(lc);
            return this;
        }

        public PlayableDirector Done()
        {
            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
            return director;
        }
    }
}
