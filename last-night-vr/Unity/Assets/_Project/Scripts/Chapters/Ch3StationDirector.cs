using System.Collections;
using LastNight.Audio;
using LastNight.Interaction;
using LastNight.Player;
using LastNight.Pull;
using UnityEngine;
using UnityEngine.Playables;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 3, "Line 4": fix the timetable, meet Maren, catch her scarf, chase the train,
    /// the Voice's confession on the bench, the real scarf, the thump begins for good.
    /// Script: CH3_001-CH3_025, TOMAS_002-003, EGG_010.
    /// </summary>
    public class Ch3StationDirector : ChapterDirector
    {
        [Header("Timetable")]
        [SerializeField] SocketSet timetableTiles;          // 4 sockets: "7", ":", "4", "0"
        [SerializeField] AudioSource[] tileMemorySounds;    // laugh, cup, page, horn (one per tile)

        [Header("Train and Maren")]
        [SerializeField] PlayableDirector trainArrives;     // CS_03
        [SerializeField] Transform marenMem;                // young Maren
        [SerializeField] Rigidbody scarf;                   // memory scarf (grabbable)
        [SerializeField] GrabSignal scarfGrab;
        [SerializeField] WindZone gust;
        [SerializeField] float scarfFallY = -1.5f;          // below platform height = missed
        [SerializeField] Transform scarfRespawn;
        [SerializeField] PlayableDirector trainDeparts;     // train pulls away at 2.5 m/s
        [SerializeField] Transform trainWindow;             // moves with the train
        [SerializeField] GestureDetector waveAtTrain;       // Kind = Wave (easter egg)

        [Header("Bench")]
        [SerializeField] Transform bench;
        [SerializeField] Light benchLight;
        [SerializeField] Light[] platformLights;

        [Header("Leak and Pull")]
        [SerializeField] GrabSignal realScarf;
        [SerializeField] Transform pullUnderClock;
        [SerializeField] Collider exitTrigger;

        bool waved, atExit;
        int tilesHeard;

        public void OnWaved() => waved = true;
        public void OnTileFilled(int index)                  // wired from SocketSet.onEachFilled
        {
            if (index < tileMemorySounds.Length) tileMemorySounds[index].Play();
            string[] lines = { "CH3_004", "CH3_005", "CH3_006", "CH3_007" };
            Queue(lines[Mathf.Min(tilesHeard++, lines.Length - 1)]);
        }
        public void OnExit() => atExit = true;

        protected override IEnumerator Run()
        {
            yield return Say("CH3_001", "CH3_002", "CH3_003");
            yield return Until(() => timetableTiles.FilledCount == 4 && !D.IsPlaying);

            // ----- Train arrives, Maren steps out, the gust -----
            trainArrives.Play();
            yield return Until(() => trainArrives.state != PlayState.Playing);
            gust.gameObject.SetActive(true);
            scarf.isKinematic = false;

            // Catch the scarf. One miss turns on slow motion.
            bool slowMo = false;
            while (!scarfGrab.WasGrabbed)
            {
                if (scarf.position.y < scarfFallY)
                {
                    scarf.linearVelocity = Vector3.zero;
                    scarf.position = scarfRespawn.position;
                    if (!slowMo) { slowMo = true; Time.timeScale = 0.4f; Time.fixedDeltaTime = 0.02f * 0.4f; }
                }
                yield return null;
            }
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            gust.gameObject.SetActive(false);
            Achievement("SCARF_GUY");

            yield return SayFrom(marenMem, "CH3_008", "CH3_009");
            yield return Say("TOMAS_002");
            yield return SayFrom(marenMem, "CH3_010", "CH3_011");

            // ----- She boards; the doors close with the scarf still in your hand -----
            trainDeparts.Play();
            waveAtTrain.gameObject.SetActive(true);
            bool caughtUp = false;
            while (trainDeparts.state == PlayState.Playing && !caughtUp)
            {
                // Holding the scarf near the moving window counts as giving it back.
                caughtUp = scarfGrab.IsHeld && Vector3.Distance(scarf.position, trainWindow.position) < 0.8f;
                if (waved) { waved = false; Queue("EGG_010", trainWindow); }
                yield return null;
            }
            yield return Say("CH3_012", trainWindow);
            yield return Say("CH3_013");

            // ----- The bench: the Voice admits he died -----
            foreach (var l in platformLights) l.enabled = false;
            benchLight.enabled = true;
            yield return Say("CH3_014");
            yield return Until(() => Gaze.DistanceTo(bench.position) < 2.5f);
            yield return Wait(3f);
            yield return Say("CH3_015", "CH3_016", "CH3_017", "CH3_018");
            yield return Say("TOMAS_003");
            yield return Say("CH3_019");

            // ----- The real scarf appears on the bench -----
            realScarf.gameObject.SetActive(true);
            yield return Until(() => realScarf.WasGrabbed || atExit);
            if (realScarf.WasGrabbed)
            {
                yield return Say("CH3_020");
                yield return Say("CH3_021", "CH3_022");
                yield return Say("CH3_023");
            }

            // ----- The Pull under the clock. The thump starts and never stops. -----
            BeatClock.Instance?.StartBeat(0.35f, 0.2f);
            pull.PlaceAt(pullUnderClock);
            pull.SetMode(PullController.Mode.Stalk);
            yield return Say("CH3_024", "CH3_025");

            yield return Until(() => atExit);
            pull.SetMode(PullController.Mode.Dormant);
            yield return Finish();
        }

        void OnDisable()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}
