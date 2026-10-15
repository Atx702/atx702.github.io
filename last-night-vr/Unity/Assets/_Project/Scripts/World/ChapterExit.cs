using LastNight.Core;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>Put at the end of each chapter (or call Complete() from a director) to finish it.</summary>
    public class ChapterExit : MonoBehaviour
    {
        [SerializeField] Chapter chapter;
        [SerializeField] MemoryDecay decayOnExit;
        [SerializeField] float delay = 4f;

        bool done;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) Complete();
        }

        public void Complete()
        {
            if (done) return;
            done = true;
            if (decayOnExit != null) decayOnExit.Dissolve(delay);
            Invoke(nameof(Finish), delay);
        }

        void Finish() => GameManager.Instance?.CompleteChapter(chapter);
    }
}
