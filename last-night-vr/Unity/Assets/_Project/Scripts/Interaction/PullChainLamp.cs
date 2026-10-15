using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.Interaction
{
    /// <summary>
    /// Bare-bulb lamp with a pull chain (Chapter 4 stealth). Fails on its own after a random time;
    /// tugging the chain (select) turns it back on. Flickers before failing.
    /// </summary>
    public class PullChainLamp : MonoBehaviour
    {
        [SerializeField] Light bulb;
        [SerializeField] Renderer bulbRenderer;
        [SerializeField] XRSimpleInteractable chain;
        [SerializeField] AudioSource clickSound;
        [SerializeField] Vector2 failAfterSeconds = new Vector2(15f, 25f);
        [SerializeField] float flickerSeconds = 1.5f;
        [SerializeField] UnityEvent<bool> onStateChanged;

        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        float baseIntensity;
        Color baseEmission;
        MaterialPropertyBlock block;
        Coroutine failing;

        public bool IsOn { get; private set; } = true;
        public bool AutoFail { get; set; }

        void Awake()
        {
            block = new MaterialPropertyBlock();
            baseIntensity = bulb.intensity;
            if (bulbRenderer != null) baseEmission = bulbRenderer.sharedMaterial.GetColor(EmissionId);
            chain.selectEntered.AddListener(_ => TurnOn());
        }

        void OnEnable() => ScheduleFailure();

        public void TurnOn()
        {
            if (clickSound != null) clickSound.Play();
            SetLit(1f);
            if (!IsOn) { IsOn = true; onStateChanged?.Invoke(true); }
            ScheduleFailure();
        }

        public void TurnOff()
        {
            SetLit(0f);
            if (IsOn) { IsOn = false; onStateChanged?.Invoke(false); }
        }

        void ScheduleFailure()
        {
            if (failing != null) StopCoroutine(failing);
            failing = StartCoroutine(FailLater());
        }

        IEnumerator FailLater()
        {
            while (!AutoFail) yield return null;
            yield return new WaitForSeconds(Random.Range(failAfterSeconds.x, failAfterSeconds.y));
            for (float t = 0; t < flickerSeconds; t += 0.08f)
            {
                SetLit(Random.value > 0.5f ? 1f : 0.1f);
                yield return new WaitForSeconds(0.08f);
            }
            TurnOff();
        }

        void SetLit(float k)
        {
            bulb.intensity = baseIntensity * k;
            if (bulbRenderer == null) return;
            bulbRenderer.GetPropertyBlock(block);
            block.SetColor(EmissionId, baseEmission * k);
            bulbRenderer.SetPropertyBlock(block);
        }
    }
}
