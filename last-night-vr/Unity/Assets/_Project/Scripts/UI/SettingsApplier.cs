using LastNight.Core;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;

namespace LastNight.UI
{
    /// <summary>
    /// Pushes GameSettings into the XR rig and audio mixer whenever settings change.
    /// Lives on the XR Origin in Core.unity. Locomotion component types are from XRI 3.
    /// </summary>
    public class SettingsApplier : MonoBehaviour
    {
        [Header("Locomotion (XRI 3 Starter Assets rig)")]
        [SerializeField] ContinuousMoveProvider moveProvider;
        [SerializeField] TeleportationProvider teleportProvider;
        [SerializeField] GameObject[] teleportRays;           // ray interactors used for teleporting
        [SerializeField] SnapTurnProvider snapTurn;
        [SerializeField] ContinuousTurnProvider smoothTurn;
        [SerializeField] TunnelingVignetteController vignette;
        [SerializeField] float[] vignetteApertures = { 1f, 0.85f, 0.7f, 0.55f }; // off, low, med, high

        [Header("Body")]
        [SerializeField] XROrigin origin;
        [SerializeField] float seatedHeightBoost = 0.45f;

        [Header("Audio")]
        [SerializeField] AudioMixer mixer;

        void OnEnable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.SettingsChanged += Apply;
            Apply();
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.SettingsChanged -= Apply;
        }

        public void Apply()
        {
            var s = GameManager.Instance.Settings;

            bool smooth = s.movement != MovementMode.Teleport;
            bool teleport = s.movement != MovementMode.Smooth;
            if (moveProvider != null) { moveProvider.enabled = smooth; moveProvider.moveSpeed = s.moveSpeed; }
            if (teleportProvider != null) teleportProvider.enabled = teleport;
            foreach (var r in teleportRays) if (r != null) r.SetActive(teleport);

            if (snapTurn != null) { snapTurn.enabled = s.turn == TurnMode.Snap; snapTurn.turnAmount = s.snapAngle; }
            if (smoothTurn != null) { smoothTurn.enabled = s.turn == TurnMode.Smooth; smoothTurn.turnSpeed = s.smoothTurnSpeed; }

            if (vignette != null)
            {
                vignette.gameObject.SetActive(s.vignette > 0);
                var p = vignette.defaultParameters;
                p.apertureSize = vignetteApertures[Mathf.Clamp(s.vignette, 0, vignetteApertures.Length - 1)];
                vignette.defaultParameters = p;
            }

            // Seated mode raises the camera so the world is at standing scale.
            // (In Floor tracking mode CameraYOffset is ignored, so move the floor offset object directly.)
            if (origin != null && origin.CameraFloorOffsetObject != null)
                origin.CameraFloorOffsetObject.transform.localPosition = new Vector3(0f, s.seated ? seatedHeightBoost : 0f, 0f);

            if (mixer != null)
            {
                SetVolume("MasterVolume", s.master);
                SetVolume("VoiceVolume", s.voice);
                SetVolume("MusicVolume", s.music);
                SetVolume("AmbienceVolume", s.ambience);
                SetVolume("SfxVolume", s.sfx);
                SetVolume("LeakVolume", s.leaks);
            }

            else AudioListener.volume = s.master; // no mixer: master volume only
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = s.renderScale;
        }

        void SetVolume(string param, float linear) =>
            mixer.SetFloat(param, linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f);

        /// <summary>Recenter: rotate the rig so the player faces the scene's forward again.</summary>
        public void Recenter()
        {
            if (origin == null) return;
            origin.MatchOriginUpCameraForward(Vector3.up, Vector3.ProjectOnPlane(origin.transform.forward, Vector3.up));
        }
    }
}
