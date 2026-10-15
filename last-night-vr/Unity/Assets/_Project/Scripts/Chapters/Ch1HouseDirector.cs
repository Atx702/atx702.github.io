using System.Collections;
using LastNight.Core;
using LastNight.Interaction;
using LastNight.Pull;
using LastNight.World;
using UnityEngine;
using UnityEngine.Playables;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 1, "Too Far": the shy house, the toy boat, the lake, Elio's rescue, the glove.
    /// Script: docs/02_Script.md, CH1_001-CH1_018.
    /// </summary>
    public class Ch1HouseDirector : ChapterDirector
    {
        [Header("Approach")]
        [SerializeField] ShyObject house;
        [SerializeField] Transform lucia;               // mother's voice position (far, on the porch)
        [SerializeField] Collider porchTrigger;

        [Header("Inside")]
        [SerializeField] SocketSet boatParts;           // hull, mast, sail sockets on the kitchen table
        [SerializeField] GrabSignal sailDrawing;        // fridge drawing (Wren's drawing #1 is also here)
        [SerializeField] GameObject assembledBoat;      // replaces the three parts when complete
        [SerializeField] GameObject backDoorLocked, backDoorOpen;

        [Header("Lake")]
        [SerializeField] Collider waterPlacementZone;   // boat dropped in here starts the lake memory
        [SerializeField] PlayableDirector lakeTimeline; // CS_01 (camera lower, water, Elio)
        [SerializeField] Transform elio;
        [SerializeField] MemoryDecay lakeMemory;

        [Header("Leak")]
        [SerializeField] GrabSignal glove;
        [SerializeField] Transform pullHorizonPoint;
        [SerializeField] Collider leaveDockTrigger;
        [SerializeField] Collider chapterExitTrigger;

        bool retreated, eyesApproached, arrived, onPorch, boatInWater, leftDock, atExit;

        // Wired from ShyObject UnityEvents and trigger relays in the scene.
        public void OnHouseRetreated() => retreated = true;
        public void OnHouseEyesClosedApproach() => eyesApproached = true;
        public void OnHouseArrived() => arrived = true;
        public void OnPorch() => onPorch = true;
        public void OnBoatInWater() => boatInWater = true;
        public void OnLeftDock() => leftDock = true;
        public void OnExit() => atExit = true;

        protected override IEnumerator Run()
        {
            // ----- Approach -----
            yield return Say("CH1_001");
            yield return Say("CH1_002", lucia);
            yield return Say("CH1_003");

            yield return Until(() => retreated || arrived);
            if (!arrived)
            {
                yield return Say("CH1_004", "CH1_005");
                yield return Until(() => eyesApproached || arrived);
                if (eyesApproached) yield return Say("CH1_006");
            }
            yield return Until(() => arrived && onPorch);

            // ----- Inside: rebuild the boat -----
            bool sailLinePlayed = false;
            int lastCount = 0;
            while (boatParts.FilledCount < 3)
            {
                if (!sailLinePlayed && sailDrawing.WasGrabbed) { sailLinePlayed = true; Queue("CH1_008"); }
                if (boatParts.FilledCount > lastCount && lastCount == 0) Queue("CH1_007");
                lastCount = boatParts.FilledCount;
                yield return null;
            }
            yield return Wait(0.5f);
            assembledBoat.SetActive(true);
            Achievement("WRONG_BLUE");
            yield return Say("CH1_009");
            backDoorLocked.SetActive(false);
            backDoorOpen.SetActive(true);

            // ----- The lake -----
            yield return Until(() => boatInWater);
            lakeTimeline.Play();
            yield return Say("CH1_010", "CH1_011", "CH1_012");
            yield return Until(() => lakeTimeline.time >= lakeTimeline.duration * 0.55f); // the hand pulls you out
            yield return SayFrom(elio, "CH1_013", "CH1_014");
            yield return Say("CH1_015");
            if (Lucid) yield return Say("EGG_009", elio);
            yield return Until(() => lakeTimeline.state != PlayState.Playing);
            if (lakeMemory != null) lakeMemory.Dissolve(3f);
            yield return Say("CH1_016");

            // ----- Leak: the glove -----
            bool gloveLines = false;
            while (!leftDock)
            {
                if (!gloveLines && glove.WasGrabbed)
                {
                    gloveLines = true;
                    yield return Say("CH1_017");
                    yield return Say("CH1_018"); // leak line, muffled
                }
                yield return null;
            }

            // First sighting: a thin white line on the horizon for 2 seconds.
            if (pull != null)
            {
                pull.PlaceAt(pullHorizonPoint);
                pull.SetMode(PullController.Mode.Sighting);
            }

            yield return Until(() => atExit);
            yield return Finish();
        }
    }
}
