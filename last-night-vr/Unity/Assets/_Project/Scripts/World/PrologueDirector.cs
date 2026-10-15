using System.Collections;
using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Player;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.World
{
    /// <summary>
    /// Chapter 0. Runs the cold open, the look/hands/move/grab tutorial and the first Voice lines.
    /// This is the reference example for how a chapter director is written; the other chapters
    /// follow the same pattern (a coroutine that waits on player actions and plays line IDs).
    /// </summary>
    public class PrologueDirector : MonoBehaviour
    {
        [SerializeField] AudioSource coldOpenAudio;       // heartbeat stutter + breath (CS_00)
        [SerializeField] GestureDetector handsOut;        // Kind = HandsOut
        [SerializeField] Collider boardsEndTrigger;       // the rise
        [SerializeField] XRGrabInteractable toyBoat;
        [SerializeField] GameObject bed;
        [SerializeField] GameObject comfortMenu;          // first-launch comfort settings panel
        [SerializeField] float standStillPrompt = 20f;

        bool handsShown, onRise, boatGrabbed, boatReleased;

        public void OnHandsOut() => handsShown = true;           // wired from GestureDetector.onGesture
        public void OnReachedRise() => onRise = true;            // wired from a trigger's UnityEvent

        IEnumerator Start()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.State.IsComplete(Chapter.Prologue)) yield break; // returning to the field
            var d = DialoguePlayer.Instance;

            // CS_00: black, heartbeat stutters and stops, a breath out.
            yield return ScreenFader.FadeOut(0f);
            if (coldOpenAudio != null)
            {
                coldOpenAudio.Play();
                yield return new WaitForSeconds(coldOpenAudio.clip.length);
            }
            yield return ScreenFader.FadeIn(3f);

            if (gm != null && gm.State.lucidMode) yield return d.PlayAndWait("EGG_007");
            yield return d.PlayAndWait("PRO_001");
            yield return d.PlayAndWait("PRO_002");

            yield return WaitForFullLookAround();
            yield return d.PlayAndWait("PRO_003");

            handsOut.gameObject.SetActive(true);
            while (!handsShown) yield return null;
            yield return d.PlayAndWait("PRO_004");
            yield return d.PlayAndWait("PRO_005");

            if (comfortMenu != null && !PlayerPrefs.HasKey("comfort_seen"))
            {
                comfortMenu.SetActive(true);
                while (comfortMenu.activeSelf) yield return null;
                PlayerPrefs.SetInt("comfort_seen", 1);
            }

            yield return d.PlayAndWait("PRO_006");
            float still = 0f;
            var start = Gaze.Head.position;
            while (!onRise)
            {
                still = Vector3.Distance(Gaze.Head.position, start) < 0.5f ? still + Time.deltaTime : -999f;
                if (still > standStillPrompt) { d.PlayOnce("PRO_007"); still = -999f; }
                yield return null;
            }

            yield return d.PlayAndWait("PRO_008");
            yield return d.PlayAndWait("PRO_009");
            yield return d.PlayAndWait("TOMAS_001");
            yield return d.PlayAndWait("PRO_010");

            toyBoat.selectEntered.AddListener(_ => boatGrabbed = true);
            toyBoat.selectExited.AddListener(_ => boatReleased = true);
            yield return d.PlayAndWait("PRO_011");
            while (!boatGrabbed) yield return null;
            yield return d.PlayAndWait("PRO_012");
            while (!boatReleased) yield return null;
            toyBoat.GetComponent<MemoryDecay>()?.Dissolve(2f);

            yield return d.PlayAndWait("PRO_013");
            gm?.CompleteChapter(Chapter.Prologue);
            gm?.Unlock("THERE_YOU_ARE");

            // Optional: if the player turns back to the bed, it's gone.
            if (bed == null) yield break;
            while (bed.activeSelf)
            {
                if (!Gaze.IsInView(bed.transform.position)) { bed.SetActive(false); break; }
                yield return null;
            }
            while (!Gaze.IsInView(bed.transform.position)) yield return null;
            d.PlayOnce("PRO_014");
        }

        IEnumerator WaitForFullLookAround()
        {
            // Track yaw coverage in 8 sectors of 45°.
            var seen = new bool[8];
            int count = 0;
            while (count < 8)
            {
                var f = Gaze.Head.forward;
                float yaw = Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg + 180f;
                int s = Mathf.Clamp((int)(yaw / 45f), 0, 7);
                if (!seen[s]) { seen[s] = true; count++; }
                yield return null;
            }
        }
    }
}
