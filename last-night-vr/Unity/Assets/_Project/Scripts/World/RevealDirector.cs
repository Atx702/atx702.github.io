using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Player;
using LastNight.Pull;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace LastNight.World
{
    /// <summary>
    /// Chapter 7, "3:12 AM". The player looks down through the floor at their own body and Maren
    /// doing CPR. The leak filter opens, so every muffled voice in the game is suddenly clear.
    /// </summary>
    public class RevealDirector : MonoBehaviour
    {
        [SerializeField] Transform floorWindow;             // center of the see-through floor
        [SerializeField] PlayableDirector realWorldScene;   // CS_07: Maren, paramedics, slowed to 30%
        [SerializeField] PullController pull;
        [SerializeField] Transform pullDoorwayPoint;
        [SerializeField] GameObject closetDoorClosed, closetDoorOpen;
        [SerializeField] AudioMixer mixer;
        [SerializeField] string leakLowpassParam = "LeakLowpass";
        [SerializeField] Transform marenSpeaker, okaforSpeaker;

        IEnumerator Start()
        {
            var d = DialoguePlayer.Instance;
            BeatClock.Instance?.SetVolume(0.9f);
            pull.SetMode(PullController.Mode.Dormant);

            yield return d.PlayAndWait("CH7_001");

            // Wait until the player actually looks down at the floor window.
            while (!Gaze.IsLookingAt(floorWindow.position, 30f)) yield return null;

            GameManager.Instance?.SetFlag(Flags.SawReveal);
            GameManager.Instance?.Unlock("312_AM");
            realWorldScene.Play();
            yield return OpenLeakFilter(4f);
            d.LeaksUnmuffled = true;

            yield return d.PlayAndWait("CH7_002", marenSpeaker);
            yield return d.PlayAndWait("CH7_003", marenSpeaker);
            yield return d.PlayAndWait("CH7_004", marenSpeaker);
            yield return d.PlayAndWait("CH7_005", marenSpeaker);
            yield return d.PlayAndWait("CH7_006", okaforSpeaker);
            yield return d.PlayAndWait("CH7_007", marenSpeaker);

            // The Pull steps into the doorway, still and quiet, shaped like Maren.
            pull.PlaceAt(pullDoorwayPoint);
            pull.SetMode(PullController.Mode.Doorway);
            yield return new WaitForSeconds(3f);

            foreach (var id in new[] { "CH7_008", "TOMAS_005", "CH7_009", "CH7_010", "CH7_011", "CH7_012", "CH7_013", "CH7_014" })
                yield return d.PlayAndWait(id);

            closetDoorClosed.SetActive(false);
            closetDoorOpen.SetActive(true);
            // The ChapterExit trigger inside the closet door completes the chapter.
        }

        IEnumerator OpenLeakFilter(float seconds)
        {
            if (mixer == null) yield break;
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                mixer.SetFloat(leakLowpassParam, Mathf.Lerp(900f, 20000f, t / seconds));
                yield return null;
            }
            mixer.SetFloat(leakLowpassParam, 20000f);
        }
    }
}
