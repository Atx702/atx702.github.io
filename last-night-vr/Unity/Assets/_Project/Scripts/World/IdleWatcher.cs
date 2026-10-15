using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Player;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// Reactive Voice lines: standing still, keeping eyes closed a long time, walking into the
    /// deep fog at the edge of the map. Lives on the player rig in Core.unity.
    /// </summary>
    public class IdleWatcher : MonoBehaviour
    {
        [SerializeField] float idleSeconds = 60f;
        [SerializeField] float idleMoveThreshold = 0.25f;
        [SerializeField] float eyesClosedSeconds = 30f;
        [SerializeField] float edgeRadius = 520f;
        [SerializeField] Transform worldCenter;
        [SerializeField] float cooldown = 45f;

        static readonly string[] IdleLines = { "SYS_IDLE_001", "SYS_IDLE_002", "SYS_IDLE_003" };
        static readonly string[] EyesLines = { "SYS_EYES_001", "SYS_EYES_002" };
        static readonly string[] EyesLeakLines = { "SYS_EYES_003" };
        static readonly string[] EdgeLines = { "SYS_EDGE_001", "SYS_EDGE_002" };

        Vector3 lastPos;
        float idleTimer, lastSpoke = -999f;
        int lastIdle = -1, lastEyes = -1, lastEdge = -1;
        bool eyesLinePlayed;

        void Update()
        {
            var head = Gaze.Head;
            if (head == null || DialoguePlayer.Instance == null || DialoguePlayer.Instance.IsPlaying) return;
            bool ready = Time.time - lastSpoke > cooldown;

            // Idle
            var flat = new Vector3(head.position.x, 0, head.position.z);
            if (Vector3.Distance(flat, lastPos) > idleMoveThreshold) { idleTimer = 0f; lastPos = flat; }
            else idleTimer += Time.deltaTime;
            if (ready && idleTimer > idleSeconds)
            {
                // The last idle line ("She can't wait") only makes sense after the twist is hinted (Ch.3+).
                var chapter = GameManager.Instance != null ? GameManager.Instance.State.currentChapter : Chapter.Prologue;
                var pool = chapter >= Chapter.Line4 ? IdleLines : new[] { IdleLines[0], IdleLines[1] };
                Speak(pool, ref lastIdle);
                idleTimer = 0f;
            }

            // Eyes closed for a long time
            var eyes = EyesClosed.Instance;
            if (eyes != null)
            {
                if (!eyes.IsClosed) eyesLinePlayed = false;
                else if (ready && !eyesLinePlayed && eyes.ClosedFor > eyesClosedSeconds)
                {
                    eyesLinePlayed = true;
                    var chapter = GameManager.Instance != null ? GameManager.Instance.State.currentChapter : Chapter.Prologue;
                    if (chapter >= Chapter.RollCall && Random.value < 0.35f) Speak(EyesLeakLines, ref lastEyes);
                    else Speak(EyesLines, ref lastEyes);
                }
            }

            // Edge of the world
            if (ready && worldCenter != null && Vector3.Distance(flat, new Vector3(worldCenter.position.x, 0, worldCenter.position.z)) > edgeRadius)
                Speak(EdgeLines, ref lastEdge);
        }

        void Speak(string[] pool, ref int last)
        {
            DialoguePlayer.Instance.PlayRandom(pool, ref last);
            lastSpoke = Time.time;
        }
    }
}
