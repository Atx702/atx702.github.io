using LastNight.Core;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// Placed at each memory anchor in the Field. Walking in loads that chapter.
    /// Hides itself once the chapter is complete, and stays locked until its requirements are met
    /// (Convergence needs chapters 1-5).
    /// </summary>
    public class AnchorPortal : MonoBehaviour
    {
        [SerializeField] Chapter chapter;
        [SerializeField] GameObject visualsWhenAvailable;
        [SerializeField] GameObject visualsWhenDone;

        void Start() => Refresh();

        void Refresh()
        {
            var state = GameManager.Instance != null ? GameManager.Instance.State : new GameState();
            bool done = state.IsComplete(chapter);
            bool available = !done && (chapter != Chapter.Convergence || state.AllMemoryAnchorsComplete);
            if (visualsWhenAvailable != null) visualsWhenAvailable.SetActive(available);
            if (visualsWhenDone != null) visualsWhenDone.SetActive(done);
            enabled = available;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!enabled || !other.CompareTag("Player")) return;
            GameManager.Instance?.StartChapter(chapter);
        }
    }
}
