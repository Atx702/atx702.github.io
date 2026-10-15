using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Player;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Secret ending, "Maren's Words": the hospital room two days later. Maren in the chair from
    /// Chapter 5, wearing the red scarf. Hold out your hand and she puts the ring back on.
    /// Then the field at morning, no fog, a sailboat on a pond. Script: SEC_001-SEC_008.
    /// </summary>
    public class SecretEndingDirector : ChapterDirector
    {
        [SerializeField] Transform maren;
        [SerializeField] Animator marenAnimator;
        [SerializeField] Characters.PuppetAnimator marenPuppet;
        [SerializeField] GestureDetector handsOut;          // Kind = HandsOut
        [SerializeField] GameObject handRing;
        [SerializeField] AudioSource monitorSteady;         // normal sinus rhythm beeps
        [SerializeField] AudioSource morningMusic;          // MUS_Morning
        [SerializeField] string morningFieldScene = "End_MorningField";
        [SerializeField] float creditsDelay = 20f;

        bool handOut;

        public void OnHandOut() => handOut = true;

        protected override IEnumerator Run()
        {
            BeatClock.Instance?.StopBeat();
            if (handRing == null && PlayerRig.Instance != null) handRing = PlayerRig.Instance.HandRing;
            if (handRing != null) handRing.SetActive(false);
            if (monitorSteady != null) monitorSteady.Play();
            yield return Wait(4f);

            foreach (var id in new[] { "SEC_001", "SEC_002", "SEC_003", "SEC_004", "SEC_005", "SEC_006", "SEC_007", "SEC_008" })
                yield return Say(id, maren);

            handsOut.gameObject.SetActive(true);
            yield return Until(() => handOut);
            if (marenAnimator != null) marenAnimator.SetTrigger("RingOn");
            if (marenPuppet != null) marenPuppet.Play(Characters.PuppetAnimator.Move.Reach);
            yield return Wait(2.5f);
            if (marenPuppet != null) marenPuppet.Play(Characters.PuppetAnimator.Move.SitTalk);
            if (handRing != null) handRing.SetActive(true);
            yield return Wait(3f);

            // Everything after this runs on the GameManager, because this scene is about to unload.
            Achievement("MARENS_WORDS");
            GM?.FinishGame(Ending.MarensWords);
            if (morningMusic != null)
            {
                morningMusic.transform.SetParent(null);
                DontDestroyOnLoad(morningMusic.gameObject); // keeps playing across the swap
                morningMusic.Play();
                Destroy(morningMusic.gameObject, morningMusic.clip.length + 1f);
            }
            yield return ScreenFader.FadeOut(3f, Color.white);
            GM?.SwapTo(morningFieldScene);
            GM?.ReturnToMainMenuAfter(creditsDelay);
        }
    }
}
