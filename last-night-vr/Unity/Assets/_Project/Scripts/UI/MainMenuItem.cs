using LastNight.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.UI
{
    /// <summary>
    /// One object on the main-menu bed (pillow, alarm clock, lamp, photo frame, door, lantern).
    /// Activating it (trigger while hovering or grabbing) runs its action.
    /// See docs/05_Menus_UI_Settings.md.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class MainMenuItem : MonoBehaviour
    {
        public enum Action { Continue, NewGame, Settings, Extras, Quit, LucidMode }

        [SerializeField] Action action;
        [SerializeField] GameObject panelToOpen;      // settings / extras panels
        [SerializeField] GameObject confirmPanel;     // new game / quit confirmation

        XRBaseInteractable interactable;

        void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            interactable.activated.AddListener(_ => Run());
        }

        void Start()
        {
            var state = GameManager.Instance != null ? GameManager.Instance.State : null;
            if (action == Action.Continue) gameObject.SetActive(SaveSystem.HasSave && state != null && state.playTimeSeconds > 0f);
            if (action == Action.LucidMode) gameObject.SetActive(state != null && state.endingsSeen.Count > 0);
        }

        void Run()
        {
            var gm = GameManager.Instance;
            switch (action)
            {
                case Action.Continue: gm.Continue(); break;
                case Action.NewGame:
                    if (SaveSystem.HasSave && confirmPanel != null && !confirmPanel.activeSelf) confirmPanel.SetActive(true);
                    else gm.NewGame();
                    break;
                case Action.LucidMode: gm.NewGame(lucid: true); break;
                case Action.Settings:
                case Action.Extras:
                    if (panelToOpen != null) panelToOpen.SetActive(!panelToOpen.activeSelf);
                    break;
                case Action.Quit:
                    if (confirmPanel != null && !confirmPanel.activeSelf) { confirmPanel.SetActive(true); break; }
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }

        /// <summary>Hook the confirm panel's "yes" button here.</summary>
        public void Confirm()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (action == Action.NewGame) GameManager.Instance.NewGame();
            else if (action == Action.Quit) Application.Quit();
        }
    }
}
