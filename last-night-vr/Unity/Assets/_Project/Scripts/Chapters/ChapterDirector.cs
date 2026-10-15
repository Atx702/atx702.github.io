using System;
using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Pull;
using LastNight.World;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Base class for every chapter's story script. A director is one long coroutine (Run) that
    /// plays line IDs from docs/02_Script.md and waits on player actions. Helpers keep each
    /// chapter readable as a list of beats.
    /// </summary>
    public abstract class ChapterDirector : MonoBehaviour
    {
        [SerializeField] protected Chapter chapter;
        [SerializeField] protected MemoryDecay memory;
        [SerializeField] protected PullController pull;
        [Tooltip("Thump volume while this chapter runs (0 before Ch.3, rising after).")]
        [SerializeField] protected float beatVolume;
        [SerializeField] protected float beatIntensity;

        protected DialoguePlayer D => DialoguePlayer.Instance;
        protected GameManager GM => GameManager.Instance;
        protected bool Lucid => GM != null && GM.State.lucidMode;

        protected virtual void Start()
        {
            var clock = BeatClock.Instance;
            if (clock != null)
            {
                if (beatVolume > 0f || beatIntensity > 0f) clock.StartBeat(beatVolume, beatIntensity);
                else clock.StopBeat();
            }
            StartCoroutine(Run());
        }

        protected abstract IEnumerator Run();

        // ---------- Helpers ----------

        protected IEnumerator Say(string id, Transform from = null) => D.PlayAndWait(id, from);

        protected IEnumerator Say(params string[] ids)
        {
            foreach (var id in ids) yield return D.PlayAndWait(id);
        }

        protected IEnumerator SayFrom(Transform from, params string[] ids)
        {
            foreach (var id in ids) yield return D.PlayAndWait(id, from);
        }

        /// <summary>Plays without waiting (for leak fragments over other action).</summary>
        protected void Queue(string id, Transform from = null) => D.Play(id, from);

        protected static IEnumerator Until(Func<bool> condition)
        {
            while (!condition()) yield return null;
        }

        protected static IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        protected void Achievement(string id) => GM?.Unlock(id);

        protected IEnumerator Finish(float dissolveSeconds = 4f)
        {
            if (memory != null)
            {
                memory.Dissolve(dissolveSeconds);
                yield return new WaitForSeconds(dissolveSeconds);
            }
            GM?.CompleteChapter(chapter);
        }
    }
}
