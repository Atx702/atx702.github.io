using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 4's kettle: filled at the tap (trigger volume), placed on the stove (socket),
    /// then boils for a set time during the stealth sequence and whistles.
    /// </summary>
    public class KitchenKettle : MonoBehaviour
    {
        [SerializeField] XRSocketInteractor stoveSocket;
        [SerializeField] Collider tapStream;
        [SerializeField] ParticleSystem steam;
        [SerializeField] AudioSource boilLoop, whistle;
        [SerializeField] Transform shakeVisual;

        public bool Filled { get; private set; }
        public bool OnStove => Filled && stoveSocket.hasSelection;
        public bool Whistling { get; private set; }

        void OnTriggerEnter(Collider other)
        {
            if (other == tapStream) Filled = true;
        }

        public void StartBoiling(float seconds) => StartCoroutine(Boil(seconds));

        IEnumerator Boil(float seconds)
        {
            if (boilLoop != null) boilLoop.Play();
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                float k = t / seconds;
                if (boilLoop != null) boilLoop.volume = Mathf.Lerp(0.1f, 0.7f, k);
                if (steam != null && k > 0.6f && !steam.isPlaying) steam.Play();
                yield return null;
            }
            Whistling = true;
            if (whistle != null) whistle.Play();
            for (float t = 0; t < 2f; t += Time.deltaTime)
            {
                if (shakeVisual != null) shakeVisual.localPosition = Random.insideUnitSphere * 0.003f;
                yield return null;
            }
            if (shakeVisual != null) shakeVisual.localPosition = Vector3.zero;
        }
    }
}
