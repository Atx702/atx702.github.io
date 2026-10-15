using System;
using System.Collections.Generic;
using LastNight.Core;
using UnityEngine;
using UnityEngine.XR;

namespace LastNight.Audio
{
    /// <summary>
    /// The chest-compression rhythm that drives the whole game: thump audio, controller haptics,
    /// and the Pull's steps all happen on this beat. Uses DSP time so audio stays locked.
    /// </summary>
    public class BeatClock : MonoBehaviour
    {
        public static BeatClock Instance { get; private set; }

        [SerializeField] float bpm = 110f;
        [SerializeField] AudioSource thumpSource;
        [SerializeField] AudioClip thumpClip;
        [Range(0, 1)] [SerializeField] float thumpVolume = 0f; // 0 until Chapter 3; raised by chapters
        [SerializeField] float hapticDuration = 0.06f;

        /// <summary>Fired on every beat. Arg = beat number since start.</summary>
        public event Action<int> Beat;

        public float Bpm => bpm;
        public float SecondsPerBeat => 60f / bpm;
        public bool Running { get; private set; }

        /// <summary>0..1, scales thump volume and haptics. Set by chapter scripts and the Pull's distance.</summary>
        public float Intensity { get; set; }

        double nextBeatDsp;
        int beatIndex;
        readonly List<InputDevice> hands = new List<InputDevice>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartBeat(float startVolume, float startIntensity)
        {
            thumpVolume = startVolume;
            Intensity = startIntensity;
            nextBeatDsp = AudioSettings.dspTime + 0.1;
            Running = true;
        }

        public void StopBeat() => Running = false;

        public void SetVolume(float v) => thumpVolume = Mathf.Clamp01(v);

        void Update()
        {
            if (!Running) return;

            // Schedule slightly ahead so the clip starts sample-accurate.
            while (AudioSettings.dspTime + 0.05 >= nextBeatDsp)
            {
                var reduced = GameManager.Instance != null && GameManager.Instance.Settings.Reduced;
                if (thumpClip != null && thumpVolume > 0f)
                {
                    thumpSource.volume = thumpVolume * Mathf.Lerp(0.6f, 1f, Intensity) * (reduced ? 0.5f : 1f);
                    thumpSource.clip = thumpClip;
                    thumpSource.PlayScheduled(nextBeatDsp);
                }
                beatIndex++;
                Beat?.Invoke(beatIndex);
                PulseHaptics();
                nextBeatDsp += SecondsPerBeat;
            }
        }

        void PulseHaptics()
        {
            var strength = GameManager.Instance != null ? GameManager.Instance.Settings.heartbeatHaptics : 0.7f;
            var amp = strength * Intensity;
            if (amp <= 0.01f) return;

            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand, hands);
            foreach (var d in hands)
                if (d.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
                    d.SendHapticImpulse(0, Mathf.Clamp01(amp), hapticDuration);
        }

        /// <summary>One strong jolt in both hands (shocks, Pull catches).</summary>
        public void Jolt(float amplitude = 1f, float duration = 0.3f)
        {
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand, hands);
            foreach (var d in hands) d.SendHapticImpulse(0, amplitude, duration);
        }
    }
}
