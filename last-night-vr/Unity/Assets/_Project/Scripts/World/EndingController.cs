using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.World
{
    /// <summary>
    /// Chapter 8, Wren's room: the final choice.
    /// STAY: hold Wren and sit on the bed for 3 s, or hold her for 20 s.
    /// WAKE: after hearing CH8_009, set Wren down after holding her, or walk to the window.
    /// The secret ending replaces WAKE's credits when all 5 leaked objects were found.
    /// </summary>
    public class EndingController : MonoBehaviour
    {
        [SerializeField] XRGrabInteractable wren;        // Wren's body is grabbable with two hands
        [SerializeField] Collider bedSeat;               // trigger volume on the bed
        [SerializeField] Collider windowZone;            // trigger volume in front of the window
        [SerializeField] Transform wrenSpeaker;
        [SerializeField] PlayableDirector stayCutscene;  // CS_END_STAY
        [SerializeField] PlayableDirector wakeCutscene;  // CS_END_WAKE
        [SerializeField] float holdToStaySeconds = 20f;
        [SerializeField] float sitToStaySeconds = 3f;
        [SerializeField] float hesitateSeconds = 15f;

        bool choiceOpen, ended, everHeld, playerOnBed;
        float holdTimer, sitTimer, waitTimer;

        IEnumerator Start()
        {
            if (EaselMode.Active) { enabled = false; yield break; } // free drawing, no story
            var d = DialoguePlayer.Instance;
            yield return d.PlayAndWait("CH8_001", wrenSpeaker);
            yield return d.PlayAndWait("CH8_002", wrenSpeaker);
            yield return d.PlayAndWait("CH8_003", wrenSpeaker);
            // CH8_004/005 (the lantern song) play from LanternInteraction if the player picks it up.
        }

        /// <summary>Called by Wren's drawing timeline (CS_08) when she holds up the drawing.</summary>
        public void OnDrawingShown() => StartCoroutine(DrawingConversation());

        IEnumerator DrawingConversation()
        {
            var d = DialoguePlayer.Instance;
            yield return d.PlayAndWait("CH8_006", wrenSpeaker);
            yield return d.PlayAndWait("TOMAS_006");
            yield return d.PlayAndWait("CH8_007", wrenSpeaker);
            yield return d.PlayAndWait("CH8_008", wrenSpeaker);
            yield return d.PlayAndWait("CH8_009", wrenSpeaker);
            choiceOpen = true; // Wren raises her arms (animator trigger on her side)
        }

        public void PlayerSatOnBed(bool seated) => playerOnBed = seated;

        void Update()
        {
            if (!choiceOpen || ended) return;

            if (wren.isSelected)
            {
                everHeld = true;
                holdTimer += Time.deltaTime;
                sitTimer = playerOnBed ? sitTimer + Time.deltaTime : 0f;
                if (holdTimer >= holdToStaySeconds || sitTimer >= sitToStaySeconds) StartCoroutine(Stay());
                return;
            }

            holdTimer = 0f;
            sitTimer = 0f;
            if (everHeld) { StartCoroutine(Wake()); return; }

            waitTimer += Time.deltaTime;
            if (waitTimer > hesitateSeconds) { waitTimer = float.MinValue; DialoguePlayer.Instance.Play("CH8_010"); }
        }

        void OnTriggerEnter(Collider other)
        {
            // This component's GameObject holds the windowZone trigger.
            if (choiceOpen && !ended && other.CompareTag("Player")) StartCoroutine(Wake());
        }

        IEnumerator Stay()
        {
            ended = true;
            var d = DialoguePlayer.Instance;
            d.Interrupt();
            yield return d.PlayAndWait("END_S_001", wrenSpeaker);
            yield return d.PlayAndWait("END_S_002");
            d.Play("END_S_003");
            d.Play("END_S_004");
            BeatClock.Instance?.StopBeat();
            stayCutscene.Play();
            yield return new WaitForSeconds((float)stayCutscene.duration);
            GameManager.Instance.FinishGame(Ending.Stay);
            GameManager.Instance.ReturnToMainMenu();
        }

        IEnumerator Wake()
        {
            ended = true;
            var d = DialoguePlayer.Instance;
            d.Interrupt();
            foreach (var id in new[] { "END_W_001", "END_W_002", "END_W_003" })
                yield return d.PlayAndWait(id, wrenSpeaker);
            yield return d.PlayAndWait("TOMAS_007");
            yield return d.PlayAndWait("END_W_004", wrenSpeaker);
            foreach (var id in new[] { "END_W_005", "END_W_006", "END_W_007" })
                yield return d.PlayAndWait(id);

            // The player walks into the window; the Pull takes their hand. (Handled by the timeline.)
            d.LeaksUnmuffled = true;
            wakeCutscene.Play();
            yield return new WaitForSeconds((float)wakeCutscene.duration);
            d.LeaksUnmuffled = false;

            var gm = GameManager.Instance;
            if (gm.State.HasAllLeakedObjects)
            {
                gm.FinishGame(Ending.Wake);
                gm.LoadSecretEnding();
            }
            else
            {
                gm.FinishGame(Ending.Wake);
                gm.ReturnToMainMenu();
            }
        }
    }
}
