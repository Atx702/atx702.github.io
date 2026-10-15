using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Interaction;
using LastNight.Pull;
using UnityEngine;
using UnityEngine.Playables;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 5, "The Long Hall": the looping corridor solved by following Wren's drawings in
    /// story order, the 3:10 coffee choice, room 7, the bracelet with Tomas's own name, and the chase.
    /// Script: CH5_001-CH5_024, TOMAS_004.
    /// </summary>
    public class Ch5HospitalDirector : ChapterDirector
    {
        [Header("Corridor loop")]
        [SerializeField] CorridorLoop corridor;
        [SerializeField] Transform nurseVoice;

        [Header("Coffee")]
        [SerializeField] GrabSignal coffeeButton;          // vending machine button (activate)
        [SerializeField] Collider pastMachineTrigger;      // beyond the machine
        [SerializeField] HospitalClock clock;

        [Header("Room 7")]
        [SerializeField] PlayableDirector room7;           // CS_05: door closes, flatline
        [SerializeField] Collider room7Approach;

        [Header("Leak")]
        [SerializeField] GrabSignal bracelet;

        [Header("Chase")]
        [SerializeField] Transform pullChaseStart;
        [SerializeField] CorridorLights chaseLights;
        [SerializeField] Collider fieldExit;

        bool pastMachine, atRoom7, atExit;

        public void OnPastMachine() => pastMachine = true;
        public void OnRoom7() => atRoom7 = true;
        public void OnFieldExit() => atExit = true;

        protected override IEnumerator Run()
        {
            yield return Say("CH5_001", "CH5_002");
            yield return Say("CH5_003", nurseVoice);

            // ----- Follow the drawings: house, lantern, boat, bird, door 7 -----
            corridor.Begin();
            int lastResets = 0;
            while (!corridor.Solved)
            {
                if (corridor.Resets > lastResets)
                {
                    lastResets = corridor.Resets;
                    if (lastResets == 1 || lastResets % 3 == 0) Queue("CH5_009");
                }
                yield return null;
            }

            // ----- 3:10 AM -----
            clock.SetTime(3, 10);
            yield return Say("CH5_010", "CH5_011");
            yield return Until(() => coffeeButton.ActivateCount > 0 || pastMachine);
            if (coffeeButton.ActivateCount > 0)
            {
                GM?.SetFlag(Flags.ChoseCoffee);
                corridor.Lengthen(2); // the corridor gets longer: two extra modules before room 7
                yield return Say("CH5_012");
            }
            else
            {
                GM?.SetFlag(Flags.WalkedPastCoffee);
                Achievement("WALKED_PAST");
                yield return Say("CH5_013");
            }

            // ----- Room 7 -----
            yield return Until(() => atRoom7);
            clock.Tick(3, 12, 6f);
            room7.Play();
            yield return Wait(4f);
            yield return SayFrom(nurseVoice, "CH5_014", "CH5_015");
            yield return Say("TOMAS_004");
            yield return Say("CH5_016", "CH5_017");
            Achievement("FOUR_MINUTES");

            // ----- The bracelet -----
            bracelet.gameObject.SetActive(true);
            yield return Until(() => bracelet.WasGrabbed || bracelet.HeldSeconds > 0f);
            yield return Say("CH5_018");
            yield return Until(() => bracelet.HeldSeconds > 2.5f || bracelet.WasReleased);
            yield return Say("CH5_019");
            yield return Say("CH5_020", "CH5_021");

            // ----- The chase -----
            BeatClock.Instance?.SetVolume(0.8f);
            chaseLights.Begin();
            pull.PlaceAt(pullChaseStart);
            pull.SetMode(PullController.Mode.Chase);
            yield return Say("CH5_022");
            yield return Wait(6f);
            Queue("CH5_023");
            Queue("CH5_024");

            yield return Until(() => atExit);
            pull.SetMode(PullController.Mode.Dormant);
            chaseLights.Stop();
            yield return Finish(2f);
        }
    }
}
