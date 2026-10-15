using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Player;
using UnityEngine;
using UnityEngine.AI;

namespace LastNight.Pull
{
    /// <summary>
    /// The Pull: the tall white shape that is really Maren's rescue.
    /// It only ever moves on the compression beat, in discrete steps. It never runs; it arrives.
    ///
    /// Modes (set per chapter):
    /// - Sighting: stands still far away, visible for a few seconds, then fades.
    /// - Stalk:    steps toward the player only while unseen; freezes when looked at (Ch.2, Ch.3, Ch.4).
    /// - Chase:    steps every beat; being looked at only slows it (Ch.5 corridor).
    /// - Circle:   orbits a point, staying between the player and a goal (Ch.6).
    /// - Doorway:  stands still, never catches (Ch.7). Can be walked into after the reveal.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PullController : MonoBehaviour
    {
        public enum Mode { Dormant, Sighting, Stalk, Chase, Circle, Doorway }

        [Header("Behavior")]
        [SerializeField] Mode mode = Mode.Dormant;
        [SerializeField] float stepDistance = 0.7f;
        [SerializeField] float chaseStepWhenSeen = 0.35f;
        [SerializeField] float catchRadius = 0.8f;
        [SerializeField] int beatsToLoseTarget = 2;
        [SerializeField] float sightingSeconds = 2f;

        [Header("Circle mode")]
        [SerializeField] Transform circleCenter;
        [SerializeField] float circleRadius = 25f;

        [Header("Look and sound")]
        [SerializeField] Renderer body;
        [SerializeField] AudioClip stepClip;
        [SerializeField] AudioClip nearLoop;
        [SerializeField] float nearDistance = 5f;
        [Tooltip("Leak lines that can play when the player looks straight at it up close.")]
        [SerializeField] string[] seenUpCloseLines = { "CH4_016" };
        [SerializeField] string[] catchLines = { "SYS_CATCH_001", "SYS_CATCH_002", "SYS_CATCH_004" };
        [SerializeField] string[] actThreeCatchLines = { "SYS_CATCH_003" };

        static readonly int BeatId = Shader.PropertyToID("_Beat");
        static readonly int AlphaId = Shader.PropertyToID("_Alpha");

        AudioSource audioSource;
        MaterialPropertyBlock block;
        NavMeshPath path;
        int unseenBeats;
        bool targetLost;
        bool catching;
        int lastCatchLine = -1;
        float beatPulse;
        float alpha = 1f;
        bool seenUpCloseLinePlayed;

        public Mode CurrentMode => mode;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            block = new MaterialPropertyBlock();
            path = new NavMeshPath();
        }

        void OnEnable()
        {
            if (BeatClock.Instance != null) BeatClock.Instance.Beat += OnBeat;
        }

        void OnDisable()
        {
            if (BeatClock.Instance != null) BeatClock.Instance.Beat -= OnBeat;
        }

        public void SetMode(Mode m)
        {
            mode = m;
            unseenBeats = 0;
            targetLost = false;
            gameObject.SetActive(m != Mode.Dormant);
            if (m == Mode.Sighting) StartCoroutine(SightingRoutine());
        }

        public void PlaceAt(Transform t)
        {
            transform.SetPositionAndRotation(t.position, t.rotation);
        }

        IEnumerator SightingRoutine()
        {
            alpha = 1f;
            yield return new WaitForSeconds(sightingSeconds);
            for (float t = 0; t < 1f; t += Time.deltaTime) { alpha = 1f - t; yield return null; }
            SetMode(Mode.Dormant);
        }

        void OnBeat(int beat)
        {
            beatPulse = 1f;
            if (Gaze.Head == null || catching) return;

            var player = Gaze.Head.position;
            bool seen = Gaze.IsLookingAt(transform.position + Vector3.up * 1.3f, 25f, requireLineOfSight: true);
            bool eyesClosed = EyesClosed.Instance != null && EyesClosed.Instance.IsClosed;

            // Losing the player: eyes closed for a couple of beats makes it lose track.
            if (eyesClosed) { if (++unseenBeats >= beatsToLoseTarget) targetLost = true; }
            else { unseenBeats = 0; targetLost = false; }

            var reduced = GameManager.Instance != null && GameManager.Instance.Settings.Reduced;
            float step = stepDistance * (reduced && mode == Mode.Chase ? 0.7f : 1f);

            switch (mode)
            {
                case Mode.Stalk:
                    if (!seen && !targetLost) StepToward(player, step);
                    break;
                case Mode.Chase:
                    if (!targetLost) StepToward(player, seen ? chaseStepWhenSeen : step);
                    break;
                case Mode.Circle:
                    StepAlongCircle(player, step);
                    break;
            }

            if (stepClip != null && mode != Mode.Dormant && mode != Mode.Doorway)
                audioSource.PlayOneShot(stepClip, 0.6f);

            // Leak fragment when the player stares at it up close.
            if (seen && Gaze.DistanceTo(transform.position) < 3f && !seenUpCloseLinePlayed && seenUpCloseLines.Length > 0)
            {
                seenUpCloseLinePlayed = true;
                DialoguePlayer.Instance?.Play(seenUpCloseLines[Random.Range(0, seenUpCloseLines.Length)]);
            }
        }

        void StepToward(Vector3 target, float distance)
        {
            target.y = transform.position.y;
            Vector3 next;
            if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path) && path.corners.Length > 1)
                next = Vector3.MoveTowards(transform.position, path.corners[1], distance);
            else
                next = Vector3.MoveTowards(transform.position, target, distance);

            var dir = target - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
            transform.position = next;
        }

        void StepAlongCircle(Vector3 player, float distance)
        {
            if (circleCenter == null) return;
            // Aim for the point on the circle between the player and the center.
            var fromCenter = player - circleCenter.position;
            fromCenter.y = 0;
            var goal = circleCenter.position + fromCenter.normalized * circleRadius;
            StepToward(goal, distance);
        }

        void Update()
        {
            beatPulse = Mathf.MoveTowards(beatPulse, 0f, Time.deltaTime * 3f);
            if (body != null)
            {
                body.GetPropertyBlock(block);
                block.SetFloat(BeatId, beatPulse);
                block.SetFloat(AlphaId, alpha);
                body.SetPropertyBlock(block);
            }

            if (Gaze.Head == null || mode == Mode.Dormant) return;

            float d = Gaze.DistanceTo(transform.position + Vector3.up);
            // Proximity drives the heartbeat and the pressure loop.
            if (BeatClock.Instance != null)
                BeatClock.Instance.Intensity = Mathf.Max(BeatClock.Instance.Intensity * 0.99f, Mathf.InverseLerp(20f, 1f, d));
            if (nearLoop != null)
            {
                if (d < nearDistance && !audioSource.isPlaying) { audioSource.clip = nearLoop; audioSource.loop = true; audioSource.Play(); }
                audioSource.volume = Mathf.InverseLerp(nearDistance, 0.5f, d);
            }

            var reduced = GameManager.Instance != null && GameManager.Instance.Settings.Reduced;
            float radius = catchRadius * (reduced ? 0.5f : 1f);
            var flat = Gaze.Head.position - transform.position;
            flat.y = 0f;
            bool canCatch = mode == Mode.Stalk || mode == Mode.Chase || mode == Mode.Circle;
            if (canCatch && !catching && flat.magnitude < radius) StartCoroutine(Catch());
        }

        IEnumerator Catch()
        {
            catching = true;
            GameManager.Instance?.ReportPullCatch();
            BeatClock.Instance?.Jolt(1f, 0.35f);
            DialoguePlayer.Instance?.Interrupt();
            DialoguePlayer.Instance?.Play("OKAFOR_001");
            yield return ScreenFader.Flash(Color.white, 0.3f);

            PlayerSpawnPoint.RespawnAtCheckpoint();
            ResetToStart();

            bool actThree = GameManager.Instance != null && GameManager.Instance.State.currentChapter >= Chapter.LongHall;
            var pool = actThree && actThreeCatchLines.Length > 0 && Random.value < 0.4f ? actThreeCatchLines : catchLines;
            DialoguePlayer.Instance?.PlayRandom(pool, ref lastCatchLine);
            catching = false;
        }

        Vector3 startPosition;
        Quaternion startRotation;

        void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        public void ResetToStart()
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            unseenBeats = 0;
            targetLost = false;
        }
    }
}
