using System;
using System.Collections;
using System.Collections.Generic;
using LastNight.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace LastNight.Dialogue
{
    /// <summary>
    /// Plays dialogue lines by ID with subtitles. The Voice is head-locked (2D); characters play
    /// from a world position; leak lines go through the muffled Leak mixer group.
    /// Lines are queued so they never overlap; Interrupt() cuts the queue for urgent lines.
    /// </summary>
    public class DialoguePlayer : MonoBehaviour
    {
        public static DialoguePlayer Instance { get; private set; }

        [SerializeField] DialogueDatabase database;
        [SerializeField] AudioSource voiceSource;   // 2D, on the camera
        [SerializeField] AudioSource worldSource;   // 3D, moved to the speaker
        [SerializeField] AudioMixerGroup voiceGroup, memoryGroup, leakGroup;
        [SerializeField] float gapBetweenLines = 0.35f;
        [Tooltip("Reading time used when a line has no audio yet (placeholder).")]
        [SerializeField] float secondsPerCharacterWithoutAudio = 0.06f;

        /// <summary>Raised when a line starts (for subtitles). Null when subtitles should clear.</summary>
        public event Action<DialogueLine, Transform> LineStarted;
        public event Action LineEnded;

        /// <summary>When true, leak lines play clean (Chapter 7 reveal).</summary>
        public bool LeaksUnmuffled { get; set; }

        readonly Queue<(DialogueLine line, Transform from, Action done)> queue = new();
        readonly HashSet<string> playedOnce = new();
        Coroutine running;

        public bool IsPlaying => running != null;

        /// <summary>
        /// Low-pass cutoff for leak lines when no mixer is assigned (filters on the sources themselves).
        /// The Ch.7 reveal raises this to 20 kHz.
        /// </summary>
        public float LeakCutoff { get; set; } = 900f;

        AudioLowPassFilter voiceFilter, worldFilter;

        void Awake()
        {
            Instance = this;
            voiceFilter = FilterOn(voiceSource);
            worldFilter = FilterOn(worldSource);
        }

        static AudioLowPassFilter FilterOn(AudioSource source)
        {
            if (source == null) return null;
            var f = source.GetComponent<AudioLowPassFilter>();
            if (f == null) f = source.gameObject.AddComponent<AudioLowPassFilter>();
            f.enabled = false;
            return f;
        }

        public void Play(string id, Transform speakerPosition = null, Action onDone = null)
        {
            var line = database.Get(id);
            if (line == null) { onDone?.Invoke(); return; }
            queue.Enqueue((line, speakerPosition, onDone));
            running ??= StartCoroutine(Run());
        }

        /// <summary>Plays the line only the first time it's requested this session.</summary>
        public void PlayOnce(string id, Transform speakerPosition = null, Action onDone = null)
        {
            if (!playedOnce.Add(id)) { onDone?.Invoke(); return; }
            Play(id, speakerPosition, onDone);
        }

        /// <summary>Picks one id at random from a pool, avoiding the last one played from that pool.</summary>
        public void PlayRandom(IList<string> pool, ref int lastIndex, Transform from = null)
        {
            if (pool.Count == 0) return;
            int i = UnityEngine.Random.Range(0, pool.Count);
            if (pool.Count > 1 && i == lastIndex) i = (i + 1) % pool.Count;
            lastIndex = i;
            Play(pool[i], from);
        }

        public void Interrupt()
        {
            queue.Clear();
            if (running != null) StopCoroutine(running);
            running = null;
            voiceSource.Stop();
            worldSource.Stop();
            LineEnded?.Invoke();
        }

        public IEnumerator PlayAndWait(string id, Transform from = null)
        {
            bool done = false;
            Play(id, from, () => done = true);
            while (!done) yield return null;
        }

        IEnumerator Run()
        {
            while (queue.Count > 0)
            {
                var (line, from, done) = queue.Dequeue();
                var source = line.speaker == "VOICE" || from == null ? voiceSource : worldSource;
                if (source == worldSource) worldSource.transform.position = from.position;

                bool muffled = line.leak && !LeaksUnmuffled;
                source.outputAudioMixerGroup = muffled ? leakGroup
                    : line.speaker == "VOICE" ? voiceGroup : memoryGroup;
                var filter = source == voiceSource ? voiceFilter : worldFilter;
                if (filter != null)
                {
                    // Without a mixer, the muffling happens on the source.
                    filter.enabled = muffled && leakGroup == null && LeakCutoff < 19000f;
                    filter.cutoffFrequency = LeakCutoff;
                }

                var settings = GameManager.Instance != null ? GameManager.Instance.Settings : null;
                if (settings != null)
                    source.volume = (line.leak ? settings.leaks : settings.voice) * settings.master;

                LineStarted?.Invoke(line, from);

                float duration;
                if (line.clip != null)
                {
                    source.clip = line.clip;
                    source.Play();
                    duration = line.clip.length;
                }
                else
                {
                    duration = Mathf.Max(1.5f, line.text.Length * secondsPerCharacterWithoutAudio);
                }

                yield return new WaitForSeconds(duration);
                LineEnded?.Invoke();
                done?.Invoke();
                yield return new WaitForSeconds(gapBetweenLines);
            }
            running = null;
        }
    }
}
