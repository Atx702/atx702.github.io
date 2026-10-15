using System.Collections;
using LastNight.Audio;
using LastNight.Player;
using LastNight.Pull;
using LastNight.World;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 6, "Convergence": fragments of every memory drift toward the center and tilt;
    /// the player crosses them as stepping stones while the Pull circles and shocks flash.
    /// Lives in the Field scene and turns on once chapters 1-5 are complete.
    /// Script: CH6_001-CH6_007 (005-007 come from ShockScheduler).
    /// </summary>
    public class Ch6ConvergenceDirector : ChapterDirector
    {
        [SerializeField] GameObject convergenceRoot;      // fragments and lamp, hidden on ordinary field visits
        [SerializeField] DriftingFragment[] fragments;
        [SerializeField] ShockScheduler shocks;
        [SerializeField] Transform center;
        [SerializeField] GameObject bedroomLamp;          // the tall lit lamp at the center
        [SerializeField] float arriveRadius = 4f;
        [SerializeField] Transform dispatchVoice;

        protected override void Start()
        {
            // The Field scene also hosts the prologue and the walks between anchors.
            if (GM == null || GM.State.currentChapter != Core.Chapter.Convergence) return;
            base.Start();
        }

        protected override IEnumerator Run()
        {
            if (convergenceRoot != null) convergenceRoot.SetActive(true);
            BeatClock.Instance?.StartBeat(0.9f, 0.6f);
            bedroomLamp.SetActive(true);
            foreach (var f in fragments) f.Begin(center.position);
            shocks.enabled = true;
            pull.SetMode(PullController.Mode.Circle);

            yield return Say("CH6_001", "CH6_002");
            yield return Wait(10f);
            yield return Say("CH6_003", dispatchVoice);
            yield return Say("CH6_004");

            yield return Until(() =>
            {
                var head = Gaze.Head.position;
                head.y = center.position.y;
                return Vector3.Distance(head, center.position) < arriveRadius;
            });

            shocks.enabled = false;
            pull.SetMode(PullController.Mode.Dormant);
            yield return Finish(0f);
        }
    }
}
