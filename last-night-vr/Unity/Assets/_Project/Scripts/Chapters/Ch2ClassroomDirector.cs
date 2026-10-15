using System.Collections;
using System.Collections.Generic;
using LastNight.Interaction;
using LastNight.Player;
using LastNight.Pull;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 2, "Roll Call": find three beeping speakers by ear, answer roll call, read the
    /// speech on the chalkboard, the pulse-clip leak, and the Pull's first stalk.
    /// Script: CH2_001-CH2_020.
    /// </summary>
    public class Ch2ClassroomDirector : ChapterDirector
    {
        [Header("Roll call")]
        [SerializeField] Transform adair;                     // teacher's voice position (front of class)

        [Header("Speakers")]
        [SerializeField] SpeakerBox[] speakers;               // 3 speaker prefabs
        [SerializeField] Transform[] speakerCandidates;       // 8 spots; 3 are picked at random
        [SerializeField] GameObject[] memoryLayers;           // restored as each speaker is silenced

        [Header("Speech")]
        [SerializeField] GestureDetector raiseHand;           // Kind = RaiseHand, disabled until asked
        [SerializeField] ChalkWord[] speechWords;             // revealed by gaze, in order
        [SerializeField] GameObject flashDrawing;             // one-frame yellow house drawing
        [SerializeField] Renderer chalkboardText;

        [Header("Leak and Pull")]
        [SerializeField] GrabSignal pulseClip;
        [SerializeField] AudioSource clipBeep;
        [SerializeField] Transform pullStart;
        [SerializeField] Collider exitTrigger;

        bool handRaised, atExit;

        public void OnHandRaised() => handRaised = true;
        public void OnExit() => atExit = true;

        protected override IEnumerator Run()
        {
            PlaceSpeakers();
            yield return Say("CH2_001");
            yield return SayFrom(adair, "CH2_002", "CH2_003");

            foreach (var s in speakers) s.StartBeeping();
            yield return Say("CH2_004");

            // ----- Find the speakers -----
            int silenced = 0;
            while (silenced < speakers.Length)
            {
                int now = 0;
                foreach (var s in speakers) if (s.Silenced) now++;
                if (now > silenced)
                {
                    silenced = now;
                    if (silenced - 1 < memoryLayers.Length) memoryLayers[silenced - 1].SetActive(true);
                    if (silenced == 1) yield return Say("CH2_005");
                    else if (silenced == 2)
                    {
                        yield return Say("CH2_006");
                        yield return SayFrom(adair, "CH2_007");
                        yield return Say("CH2_008");
                    }
                    else yield return Say("CH2_009");
                }
                yield return null;
            }

            // ----- Roll call -----
            yield return Say("CH2_010", adair);
            raiseHand.gameObject.SetActive(true);
            yield return Until(() => handRaised);
            Achievement("PRESENT");
            yield return Say("CH2_011", adair);
            yield return Say("CH2_012");

            // ----- The speech: words reveal as the player reads them -----
            int lineIndex = 0;
            string[] speechLines = { "CH2_013", "CH2_014" };
            for (int i = 0; i < speechWords.Length; i++)
            {
                yield return Until(() => speechWords[i].Revealed);
                // Halfway through the words, the Voice reads the first line; at the end, the second.
                if ((i == speechWords.Length / 2 || i == speechWords.Length - 1) && lineIndex < speechLines.Length)
                    Queue(speechLines[lineIndex++]);
            }
            yield return Until(() => !D.IsPlaying);
            yield return Say("CH2_015");
            foreach (var w in speechWords) w.Crumble();
            flashDrawing.SetActive(true);
            yield return null;
            yield return null; // two frames: just long enough to be unsure you saw it
            flashDrawing.SetActive(false);
            yield return Say("CH2_016");

            // ----- Leak: the pulse clip -----
            yield return Until(() => pulseClip.WasGrabbed || atExit);
            if (pulseClip.WasGrabbed)
            {
                if (clipBeep != null) clipBeep.Play();
                yield return Say("CH2_017");
                yield return Say("CH2_018", "CH2_019");
            }

            // ----- The Pull, between the desks -----
            pull.PlaceAt(pullStart);
            pull.SetMode(PullController.Mode.Stalk);
            yield return Wait(4f);
            yield return Say("CH2_020");

            yield return Until(() => atExit);
            pull.SetMode(PullController.Mode.Dormant);
            yield return Finish();
        }

        void PlaceSpeakers()
        {
            var spots = new List<Transform>(speakerCandidates);
            foreach (var s in speakers)
            {
                int i = Random.Range(0, spots.Count);
                s.transform.SetPositionAndRotation(spots[i].position, spots[i].rotation);
                spots.RemoveAt(i);
            }
        }
    }
}
