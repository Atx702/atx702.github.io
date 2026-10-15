using LastNight.Core;
using LastNight.Dialogue;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>One of Wren's 12 drawings. Found by looking at it from close range for a moment.</summary>
    public class DrawingCollectible : MonoBehaviour
    {
        [Range(1, GameState.DrawingCount)] [SerializeField] int index = 1;
        [SerializeField] float lookSeconds = 1f;
        [SerializeField] float maxDistance = 2.5f;
        [SerializeField] string lineOnFound; // e.g. CH5_004 for the corridor drawings

        float timer;
        bool found;

        public bool Found => found;
        public int Index => index;

        void Update()
        {
            if (found) return;
            bool looking = Player.Gaze.DistanceTo(transform.position) < maxDistance &&
                           Player.Gaze.IsLookingAt(transform.position, 12f);
            timer = looking ? timer + Time.deltaTime : 0f;
            if (timer < lookSeconds) return;

            found = true;
            GameManager.Instance?.ReportDrawing(index);
            if (!string.IsNullOrEmpty(lineOnFound)) DialoguePlayer.Instance?.PlayOnce(lineOnFound, transform);
            if (GameManager.Instance != null && GameManager.Instance.State.drawingsFound.Count == GameState.DrawingCount)
                DialoguePlayer.Instance?.PlayOnce("EGG_002");
        }

        /// <summary>Chapter 5 corridor: lets the drawing be "seen" again on the next loop.
        /// The gallery keeps it either way.</summary>
        public void ResetForLoop()
        {
            found = false;
            timer = 0f;
        }
    }
}
