using LastNight.Core;
using LastNight.Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastNight.UI
{
    /// <summary>
    /// Menu button opens a small paper card at chest height. The world desaturates and time
    /// stops (Time.timeScale = 0, AudioListener paused). Lives in Core.unity.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] InputActionReference menuButton;
        [SerializeField] GameObject card;                 // Resume / Settings / Restart / Chapters / Quit
        [SerializeField] GameObject settingsPanel;
        [SerializeField] GameObject chapterSelectPanel;
        [SerializeField] Transform head;
        [SerializeField] float distance = 0.55f, drop = 0.25f;
        [SerializeField] UnityEngine.Rendering.Volume pauseVolume; // desaturation post-process

        public bool Paused { get; private set; }

        void OnEnable()
        {
            menuButton.action.Enable();
            menuButton.action.performed += Toggle;
        }

        void OnDisable() => menuButton.action.performed -= Toggle;

        void Toggle(InputAction.CallbackContext _)
        {
            if (Paused) Resume(); else Pause();
        }

        public void Pause()
        {
            if (GameManager.Instance == null) return;
            Paused = true;
            var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            card.transform.position = head.position + fwd * distance + Vector3.down * drop;
            card.transform.rotation = Quaternion.LookRotation(fwd);
            card.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (pauseVolume != null) pauseVolume.weight = 1f;

            // One in ten times, the Voice comments. It plays after unpausing so it isn't lost.
            if (Random.value < 0.1f) pendingVoiceLine = true;
        }

        bool pendingVoiceLine;

        public void Resume()
        {
            Paused = false;
            card.SetActive(false);
            settingsPanel.SetActive(false);
            if (chapterSelectPanel != null) chapterSelectPanel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (pauseVolume != null) pauseVolume.weight = 0f;
            if (pendingVoiceLine) { pendingVoiceLine = false; DialoguePlayer.Instance?.PlayOnce("SYS_PAUSE_001"); }
        }

        public void OpenSettings() => settingsPanel.SetActive(true);
        public void OpenChapterSelect() { if (chapterSelectPanel != null) chapterSelectPanel.SetActive(true); }

        public void RestartFromAnchor()
        {
            Resume();
            DialoguePlayer.Instance?.Interrupt();
            GameManager.Instance.StartChapter(GameManager.Instance.State.currentChapter);
        }

        public void QuitToBed()
        {
            Resume();
            DialoguePlayer.Instance?.Interrupt();
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
