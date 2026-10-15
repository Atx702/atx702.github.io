using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Interaction;
using LastNight.Player;
using LastNight.Pull;
using UnityEngine;
using UnityEngine.Playables;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 4, "Yellow Kitchen": make tea, tune the radio to their song, Maren's news,
    /// the Pull's first stealth sequence with the pull-chain lamps, and the ring.
    /// Script: CH4_001-CH4_025, OKAFOR_001, EGG_005.
    /// </summary>
    public class Ch4KitchenDirector : ChapterDirector
    {
        [Header("Tea")]
        [SerializeField] Transform marenMem;           // voice from the dark hallway / by the counter
        [SerializeField] KitchenKettle kettle;
        [SerializeField] SocketSet mugsOnTable;         // 2 sockets
        [SerializeField] GrabSignal honey;
        [SerializeField] Transform mouth;               // child of the camera, at mouth height

        [Header("Radio")]
        [SerializeField] Dial radioDial;
        [SerializeField] AudioSource radioStatic, radioSong, radioReversed;
        [SerializeField] float songStation = 88f, leakStation1 = 23f, leakStation2 = 61f, reversedStation = 7f;
        [SerializeField] float stationWidth = 2f;

        [Header("News")]
        [SerializeField] PlayableDirector thirdMug;     // CS_04A

        [Header("Stealth")]
        [SerializeField] PlayableDirector itFoundUs;    // CS_04B
        [SerializeField] PullChainLamp[] lamps;
        [SerializeField] Transform pullDoorway;
        [SerializeField] float stealthSeconds = 180f;   // until the kettle whistles

        [Header("Ring")]
        [SerializeField] GameObject handRing;            // ring mesh on the player's hand
        [SerializeField] GameObject tableRing;           // leaked object on the table
        [SerializeField] Collider exitDoorTrigger;

        bool leak1Heard, leak2Heard, onSong, atDoor, honeyEaten;

        public void OnExitDoor() => atDoor = true;

        // Wired from Dial.onValueChanged.
        public void OnRadioTuned(float v)
        {
            bool Near(float station) => Mathf.Abs(v - station) < stationWidth;
            onSong = Near(songStation);
            radioSong.volume = onSong ? 1f : 0f;
            radioReversed.volume = Near(reversedStation) ? 1f : 0f;
            radioStatic.volume = onSong || Near(reversedStation) ? 0.05f : 0.6f;
            if (!leak1Heard && Near(leakStation1)) { leak1Heard = true; Queue("CH4_007"); Queue("CH4_009"); }
            if (!leak2Heard && Near(leakStation2)) { leak2Heard = true; Queue("CH4_008"); }
        }

        protected override IEnumerator Run()
        {
            // ----- Tea -----
            yield return Say("CH4_001");
            yield return Say("CH4_002", marenMem);
            yield return Say("CH4_003");

            yield return Until(() => kettle.OnStove);
            yield return SayFrom(marenMem, "CH4_004", "CH4_005");
            yield return Until(() => mugsOnTable.FilledCount == 2);

            // ----- Radio -----
            radioStatic.Play(); radioSong.Play(); radioReversed.Play();
            OnRadioTuned(radioDial.Value);
            yield return Say("CH4_006", marenMem);
            while (!onSong)
            {
                if (!honeyEaten && honey.IsHeld && Vector3.Distance(honey.transform.position, mouth.position) < 0.15f)
                {
                    honeyEaten = true;
                    Queue("EGG_005");
                }
                yield return null;
            }
            yield return Wait(2f);
            yield return Say("CH4_010", marenMem);

            // ----- The news -----
            thirdMug.Play();
            yield return SayFrom(marenMem, "CH4_011", "CH4_012");
            Achievement("THIRD_MUG");
            yield return Say("CH4_013");
            yield return Until(() => thirdMug.state != PlayState.Playing);

            // ----- It found us: stealth -----
            itFoundUs.Play();
            BeatClock.Instance?.SetVolume(0.6f);
            pull.PlaceAt(pullDoorway);
            pull.SetMode(PullController.Mode.Stalk);
            yield return Say("CH4_014", "CH4_015");

            foreach (var lamp in lamps) lamp.AutoFail = true;
            kettle.StartBoiling(stealthSeconds);
            int catchesAtStart = GM != null ? GM.State.pullCatches : 0;
            bool everClosedEyes = false;
            while (!kettle.Whistling)
            {
                // With every lamp out, the Pull hunts in Chase mode until the player closes their eyes.
                bool allOut = true;
                foreach (var lamp in lamps) if (lamp.IsOn) { allOut = false; break; }
                pull.SetMode(allOut ? PullController.Mode.Chase : PullController.Mode.Stalk);

                if (EyesClosed.Instance != null && EyesClosed.Instance.IsClosed) everClosedEyes = true;
                if (GM != null && GM.State.pullCatches > catchesAtStart)
                {
                    catchesAtStart = GM.State.pullCatches;
                    yield return Say("CH4_018"); // OKAFOR_001 already played by the catch itself
                }
                yield return null;
            }
            pull.SetMode(PullController.Mode.Dormant);
            foreach (var lamp in lamps) { lamp.AutoFail = false; lamp.TurnOn(); }
            if (!everClosedEyes) Achievement("EYES_OPEN");
            yield return Say("CH4_019", "CH4_020");

            // ----- The ring -----
            handRing.SetActive(false);
            tableRing.SetActive(true);
            var ringGrab = tableRing.GetComponent<GrabSignal>();
            yield return Until(() => ringGrab.WasGrabbed || atDoor);
            if (ringGrab.WasGrabbed)
            {
                yield return Say("CH4_021");
                yield return Say("CH4_022", "CH4_023");
                // Putting the ring back on: holding it near the ring finger re-shows the hand ring.
                handRing.SetActive(true);
            }

            yield return Until(() => atDoor);
            yield return Say("CH4_024");
            yield return Wait(1.5f);
            yield return Say("CH4_025");
            yield return Finish();
        }
    }
}
