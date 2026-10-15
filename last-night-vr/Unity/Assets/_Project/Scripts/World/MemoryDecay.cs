using System.Collections;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// Memories wear away. Drives the _Decay property (0..1) of SG_MemoryDecay on every renderer
    /// under this object. Slowly creeps up while the player is inside the anchor; Dissolve()
    /// plays the "goodbye" effect at the end of a chapter.
    /// </summary>
    public class MemoryDecay : MonoBehaviour
    {
        [SerializeField] float creepPerMinute = 0.08f;
        [SerializeField] float maxCreep = 0.45f;
        [SerializeField] ParticleSystem dustBurst;
        [SerializeField] AudioSource decaySound;

        static readonly int DecayId = Shader.PropertyToID("_Decay");
        Renderer[] renderers;
        MaterialPropertyBlock block;
        float decay;
        bool creeping = true;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            block = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (!creeping) return;
            decay = Mathf.Min(maxCreep, decay + creepPerMinute / 60f * Time.deltaTime);
            Apply();
        }

        public void Dissolve(float seconds = 4f) => StartCoroutine(DissolveRoutine(seconds));

        IEnumerator DissolveRoutine(float seconds)
        {
            creeping = false;
            if (dustBurst != null) dustBurst.Play();
            if (decaySound != null) decaySound.Play();
            float start = decay;
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                decay = Mathf.Lerp(start, 1f, t / seconds);
                Apply();
                yield return null;
            }
            decay = 1f;
            Apply();
            foreach (var r in renderers) r.enabled = false;
        }

        void Apply()
        {
            foreach (var r in renderers)
            {
                r.GetPropertyBlock(block);
                block.SetFloat(DecayId, decay);
                r.SetPropertyBlock(block);
            }
        }
    }
}
