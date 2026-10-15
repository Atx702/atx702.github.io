using System.Collections;
using LastNight.Audio;
using LastNight.Player;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 5 chase: ceiling lights die one by one behind the player, every two beats,
    /// so darkness follows them down the hall.
    /// </summary>
    public class CorridorLights : MonoBehaviour
    {
        [Tooltip("Lights in order from the corridor start to the exit.")]
        [SerializeField] Light[] lights;
        [SerializeField] AudioSource failSound;
        [SerializeField] int beatsPerLight = 2;
        [SerializeField] float keepLitAhead = 6f;

        int next, beats;
        bool running;

        public void Begin()
        {
            running = true;
            if (BeatClock.Instance != null) BeatClock.Instance.Beat += OnBeat;
        }

        public void Stop()
        {
            running = false;
            if (BeatClock.Instance != null) BeatClock.Instance.Beat -= OnBeat;
        }

        void OnDestroy() => Stop();

        void OnBeat(int _)
        {
            if (!running || next >= lights.Length || ++beats % beatsPerLight != 0) return;
            // Never kill a light the player hasn't passed yet (keeps the way forward lit).
            var head = Gaze.Head;
            if (head != null)
            {
                var toLight = lights[next].transform.position - head.position;
                toLight.y = 0f;
                bool ahead = Vector3.Dot(toLight, head.forward) > 0f;
                if (ahead || toLight.magnitude < keepLitAhead) return;
            }
            StartCoroutine(Fail(lights[next++]));
        }

        IEnumerator Fail(Light l)
        {
            if (failSound != null)
            {
                failSound.transform.position = l.transform.position;
                failSound.Play();
            }
            float baseI = l.intensity;
            for (int i = 0; i < 4; i++)
            {
                l.intensity = baseI * 0.1f; yield return new WaitForSeconds(0.05f);
                l.intensity = baseI; yield return new WaitForSeconds(0.07f);
            }
            l.enabled = false;
        }
    }
}
