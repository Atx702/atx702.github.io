using LastNight.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LastNight.Interaction
{
    /// <summary>
    /// Detects the player humming into the headset mic: sustained voiced sound with a steady
    /// pitch for a couple of seconds. Only runs when "Microphone humming" is on in Settings.
    /// Used for the Lantern song (Ch.8) and the early-humming easter egg (Ch.1).
    /// </summary>
    public class HumDetector : MonoBehaviour
    {
        [SerializeField] float requiredSeconds = 2.5f;
        [SerializeField] float rmsThreshold = 0.02f;
        [SerializeField] float minPitchHz = 80f, maxPitchHz = 600f;
        [SerializeField] UnityEvent onHummed;

        const int SampleRate = 16000;
        const int Window = 1024;
        AudioClip micClip;
        string device;
        readonly float[] buffer = new float[Window];
        float voicedTime;
        bool fired;

        void OnEnable()
        {
            fired = false;
            var s = GameManager.Instance != null ? GameManager.Instance.Settings : null;
            if (s == null || !s.micHumming || Microphone.devices.Length == 0) { enabled = false; return; }
            device = Microphone.devices[0];
            micClip = Microphone.Start(device, true, 2, SampleRate);
        }

        void OnDisable()
        {
            if (device != null && Microphone.IsRecording(device)) Microphone.End(device);
        }

        void Update()
        {
            if (fired || micClip == null) return;
            int pos = Microphone.GetPosition(device) - Window;
            if (pos < 0) return;
            micClip.GetData(buffer, pos);

            float sum = 0f;
            for (int i = 0; i < Window; i++) sum += buffer[i] * buffer[i];
            float rms = Mathf.Sqrt(sum / Window);
            float pitch = EstimatePitch();
            bool voiced = rms > rmsThreshold && pitch >= minPitchHz && pitch <= maxPitchHz;

            voicedTime = voiced ? voicedTime + Time.deltaTime : Mathf.Max(0f, voicedTime - Time.deltaTime * 2f);
            if (voicedTime >= requiredSeconds)
            {
                fired = true;
                onHummed?.Invoke();
            }
        }

        /// <summary>Autocorrelation pitch estimate; good enough to tell humming from noise.</summary>
        float EstimatePitch()
        {
            int minLag = SampleRate / (int)maxPitchHz;
            int maxLag = SampleRate / (int)minPitchHz;
            float best = 0f;
            int bestLag = 0;
            for (int lag = minLag; lag <= maxLag && lag < Window; lag++)
            {
                float c = 0f;
                for (int i = 0; i + lag < Window; i++) c += buffer[i] * buffer[i + lag];
                if (c > best) { best = c; bestLag = lag; }
            }
            return bestLag > 0 ? (float)SampleRate / bestLag : 0f;
        }
    }
}
