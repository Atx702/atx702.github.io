using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Interaction;
using LastNight.Player;
using LastNight.Pull;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// The open Field between anchors. Sets the mood for how far the story has gone (thump volume,
    /// curtains after Ch.3, distant Pull sightings), and runs the walk-time field events and
    /// the Biscuit / receipt / 8th-anchor easter eggs. Inactive during Convergence.
    /// </summary>
    public class FieldDirector : MonoBehaviour
    {
        [Header("Progress dressing")]
        [SerializeField] GameObject hospitalCurtains;      // appear after Ch.3
        [SerializeField] PullController pull;
        [SerializeField] Transform[] sightingPoints;       // far points on the horizon

        [Header("Field events (each plays once, by distance walked)")]
        [SerializeField] GameObject standaloneDoor;
        [SerializeField] GameObject coatRack;
        [SerializeField] AudioSource windLantern;          // 3 s of the Lantern tune on the wind
        [SerializeField] float[] eventDistances = { 60f, 160f, 280f };

        [Header("Easter eggs")]
        [SerializeField] GameObject biscuit;               // the dog, hidden until summoned
        [SerializeField] GrabSignal biscuitPet;            // simple interactable on the dog
        [SerializeField] GrabSignal coatPocket;            // reaching into the pocket
        [SerializeField] GameObject receipt;               // "ST. ALDER'S CAFE — 1 COFFEE — 03:10"
        [SerializeField] GameObject eighthAnchor;          // Lucid Mode only
        [SerializeField] Transform[] anchors;              // for "facing away from all anchors"
        [SerializeField] Transform rise;                   // prologue hill

        GameManager GM => GameManager.Instance;
        float walked;
        Vector3 lastPos;
        int nextEvent;

        void Start()
        {
            if (GM == null || GM.State.currentChapter == Chapter.Convergence) { enabled = false; return; }
            var s = GM.State;

            int done = 0;
            foreach (var c in new[] { Chapter.TooFar, Chapter.RollCall, Chapter.Line4, Chapter.YellowKitchen, Chapter.LongHall })
                if (s.IsComplete(c)) done++;

            hospitalCurtains.SetActive(s.IsComplete(Chapter.Line4));
            if (s.IsComplete(Chapter.Line4) && BeatClock.Instance != null)
                BeatClock.Instance.StartBeat(Mathf.Lerp(0.25f, 0.6f, done / 5f), 0.15f + done * 0.08f);
            else BeatClock.Instance?.StopBeat();

            if (done >= 2) StartCoroutine(Sightings());
            StartCoroutine(Biscuit());
            if (s.lucidMode) StartCoroutine(EighthAnchor());
            if (Gaze.Head != null) lastPos = Gaze.Head.position;
        }

        void Update()
        {
            if (Gaze.Head == null) return;
            var p = Gaze.Head.position;
            walked += Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(lastPos.x, 0, lastPos.z));
            lastPos = p;

            if (nextEvent < eventDistances.Length && walked > eventDistances[nextEvent])
            {
                SpawnEventAhead(nextEvent);
                nextEvent++;
            }

            if (coatPocket != null && coatPocket.WasGrabbed && !receipt.activeSelf)
            {
                receipt.SetActive(true);
                GM.SetFlag(Flags.ReadReceipt);
                GM.Unlock("0310");
            }
        }

        void SpawnEventAhead(int i)
        {
            var head = Gaze.Head;
            var ahead = head.position + Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * 25f;
            ahead.y = 0f;
            switch (i)
            {
                case 0: Place(standaloneDoor, ahead); break;
                case 1: Place(coatRack, ahead); break;
                case 2: if (windLantern != null) windLantern.Play(); break;
            }
        }

        static void Place(GameObject go, Vector3 at)
        {
            if (go == null) return;
            go.transform.position = at;
            go.SetActive(true);
        }

        IEnumerator Sightings()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(45f, 90f));
                if (sightingPoints.Length == 0 || pull == null) yield break;
                // Pick a point currently in view so it's actually seen.
                foreach (var p in sightingPoints)
                {
                    if (!Gaze.IsInView(p.position)) continue;
                    pull.PlaceAt(p);
                    pull.SetMode(PullController.Mode.Sighting);
                    break;
                }
            }
        }

        /// <summary>Close your eyes for 10 s in the open field, open them facing away from every anchor.</summary>
        IEnumerator Biscuit()
        {
            if (biscuit == null || GM.State.HasFlag(Flags.FoundBiscuit)) yield break;
            var eyes = EyesClosed.Instance;
            while (true)
            {
                yield return null;
                if (eyes == null || !eyes.IsClosed || eyes.ClosedFor < 10f) continue;
                while (eyes.IsClosed) yield return null;
                if (FacingAwayFromAllAnchors()) break;
            }
            var head = Gaze.Head;
            biscuit.transform.position = head.position + Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * 3f;
            biscuit.transform.position = new Vector3(biscuit.transform.position.x, 0f, biscuit.transform.position.z);
            biscuit.transform.rotation = Quaternion.LookRotation(-Vector3.ProjectOnPlane(head.forward, Vector3.up));
            biscuit.SetActive(true);
            while (!biscuitPet.WasGrabbed && biscuitPet.ActivateCount == 0) yield return null;
            GM.SetFlag(Flags.FoundBiscuit);
            GM.Unlock("GOOD_BOY");
            DialoguePlayer.Instance.Play("EGG_001");
        }

        bool FacingAwayFromAllAnchors()
        {
            foreach (var a in anchors)
                if (Gaze.IsInView(a.position)) return false;
            return true;
        }

        /// <summary>Lucid Mode: eyes closed on the rise for 20 s shows a tiny 8th light.</summary>
        IEnumerator EighthAnchor()
        {
            var eyes = EyesClosed.Instance;
            while (true)
            {
                yield return null;
                if (eyes == null || rise == null || Gaze.DistanceTo(rise.position) > 15f) continue;
                if (eyes.IsClosed && eyes.ClosedFor > 20f) break;
            }
            eighthAnchor.SetActive(true);
        }
    }
}
