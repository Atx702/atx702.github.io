using System;
using LastNight.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastNight.UI
{
    /// <summary>
    /// World-space settings panel (on the bedside lamp in the main menu, or the pause card).
    /// Each control is bound to one GameSettings field. Changes apply live; Back saves.
    /// Build the panel with uGUI + TMP under a Canvas with a TrackedDeviceGraphicRaycaster.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] GameObject comfortTab, accessibilityTab, audioTab, graphicsTab;

        [Header("Comfort")]
        [SerializeField] TMP_Dropdown movement;          // Smooth / Teleport / Both
        [SerializeField] Slider moveSpeed;               // 1.0 - 2.5
        [SerializeField] TMP_Dropdown turn;              // Snap / Smooth
        [SerializeField] TMP_Dropdown snapAngle;         // 15 / 30 / 45 / 90
        [SerializeField] Slider smoothTurnSpeed;         // 30 - 180
        [SerializeField] TMP_Dropdown vignette;          // Off / Low / Medium / High
        [SerializeField] Toggle seated, leftHanded, fadeCuts;

        [Header("Intensity and accessibility")]
        [SerializeField] TMP_Dropdown intensity;         // Full / Reduced
        [SerializeField] Toggle flashReduction;
        [SerializeField] Slider heartbeatHaptics;
        [SerializeField] Toggle subtitles, speakerNames, toggleMode, micHumming;
        [SerializeField] TMP_Dropdown subtitleSize;      // S / M / L / XL
        [SerializeField] Slider subtitleBackground;
        [SerializeField] TMP_Dropdown eyesClosedInput;   // Gesture / Button / Both

        [Header("Audio")]
        [SerializeField] Slider master, voice, music, ambience, sfx, leaks;

        [Header("Graphics")]
        [SerializeField] Slider renderScale;             // 0.7 - 1.5

        [SerializeField] SettingsApplier applier;

        static readonly int[] SnapAngles = { 15, 30, 45, 90 };

        /// <summary>The one settings panel (in Core). The bed menu and pause card both open it.</summary>
        public static SettingsMenu Instance
        {
            get
            {
                // The panel starts hidden, so Awake hasn't run yet: find it even while inactive.
                if (instance == null) instance = FindAnyObjectByType<SettingsMenu>(FindObjectsInactive.Include);
                return instance;
            }
        }
        static SettingsMenu instance;

        void Awake()
        {
            instance = this;
            if (applier == null) applier = FindAnyObjectByType<SettingsApplier>();
        }

        /// <summary>Shows the panel in front of the player, at chest height.</summary>
        public void OpenInFrontOfPlayer()
        {
            var head = Camera.main != null ? Camera.main.transform : null;
            if (head != null)
            {
                var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
                transform.position = head.position + fwd * 0.9f + Vector3.down * 0.15f;
                transform.rotation = Quaternion.LookRotation(fwd);
            }
            gameObject.SetActive(true);
        }
        GameSettings s;
        bool loading;

        void OnEnable()
        {
            s = GameManager.Instance.Settings;
            Load();
            ShowTab(0);
        }

        public void ShowTab(int i)
        {
            comfortTab.SetActive(i == 0);
            accessibilityTab.SetActive(i == 1);
            audioTab.SetActive(i == 2);
            if (graphicsTab != null) graphicsTab.SetActive(i == 3);
        }

        void Load()
        {
            loading = true;
            movement.value = (int)s.movement;
            moveSpeed.value = s.moveSpeed;
            turn.value = (int)s.turn;
            snapAngle.value = Mathf.Max(0, Array.IndexOf(SnapAngles, s.snapAngle));
            smoothTurnSpeed.value = s.smoothTurnSpeed;
            vignette.value = s.vignette;
            seated.isOn = s.seated;
            leftHanded.isOn = s.leftHanded;
            fadeCuts.isOn = s.fadeCutsForForcedMoves;

            intensity.value = (int)s.intensity;
            flashReduction.isOn = s.flashReduction;
            heartbeatHaptics.value = s.heartbeatHaptics;
            subtitles.isOn = s.subtitles;
            speakerNames.isOn = s.speakerNames;
            toggleMode.isOn = s.toggleInsteadOfHold;
            micHumming.isOn = s.micHumming;
            subtitleSize.value = s.subtitleSize;
            subtitleBackground.value = s.subtitleBackground;
            eyesClosedInput.value = (int)s.eyesClosedInput;

            master.value = s.master; voice.value = s.voice; music.value = s.music;
            ambience.value = s.ambience; sfx.value = s.sfx; leaks.value = s.leaks;
            if (renderScale != null) renderScale.value = s.renderScale;
            loading = false;
        }

        /// <summary>Wire every control's onValueChanged to this (no arguments needed).</summary>
        public void OnAnyChanged()
        {
            if (loading) return;
            s.movement = (MovementMode)movement.value;
            s.moveSpeed = moveSpeed.value;
            s.turn = (TurnMode)turn.value;
            s.snapAngle = SnapAngles[Mathf.Clamp(snapAngle.value, 0, SnapAngles.Length - 1)];
            s.smoothTurnSpeed = smoothTurnSpeed.value;
            s.vignette = vignette.value;
            s.seated = seated.isOn;
            s.leftHanded = leftHanded.isOn;
            s.fadeCutsForForcedMoves = fadeCuts.isOn;

            s.intensity = (Intensity)intensity.value;
            s.flashReduction = flashReduction.isOn;
            s.heartbeatHaptics = heartbeatHaptics.value;
            s.subtitles = subtitles.isOn;
            s.speakerNames = speakerNames.isOn;
            s.toggleInsteadOfHold = toggleMode.isOn;
            s.micHumming = micHumming.isOn;
            s.subtitleSize = subtitleSize.value;
            s.subtitleBackground = subtitleBackground.value;
            s.eyesClosedInput = (EyesClosedInput)eyesClosedInput.value;

            s.master = master.value; s.voice = voice.value; s.music = music.value;
            s.ambience = ambience.value; s.sfx = sfx.value; s.leaks = leaks.value;
            if (renderScale != null) s.renderScale = renderScale.value;

            GameManager.Instance.ApplySettings(s);
        }

        public void Recenter() => applier?.Recenter();

        public void ResetDefaults()
        {
            s = new GameSettings();
            GameManager.Instance.ApplySettings(s);
            Load();
        }

        public void Back() => gameObject.SetActive(false);
    }
}
