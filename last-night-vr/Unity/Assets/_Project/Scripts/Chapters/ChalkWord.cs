using System.Collections;
using LastNight.Player;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// One word of the Chapter 2 speech. Writes itself on (SG_ChalkReveal _Reveal 0->1) when the
    /// player looks at it, but only after the previous word is done, so it reads in order.
    /// </summary>
    public class ChalkWord : MonoBehaviour
    {
        [SerializeField] ChalkWord previous;
        [SerializeField] Renderer chalk;
        [SerializeField] float writeSeconds = 0.6f;
        [SerializeField] AudioSource chalkSound;
        [SerializeField] ParticleSystem crumbleDust;

        static readonly int RevealId = Shader.PropertyToID("_Reveal");
        MaterialPropertyBlock block;
        bool writing;

        public bool Revealed { get; private set; }

        void Awake()
        {
            block = new MaterialPropertyBlock();
            SetReveal(0f);
        }

        void Update()
        {
            if (Revealed || writing) return;
            if (previous != null && !previous.Revealed) return;
            if (Gaze.IsLookingAt(transform.position, 10f)) StartCoroutine(Write());
        }

        IEnumerator Write()
        {
            writing = true;
            if (chalkSound != null) chalkSound.Play();
            for (float t = 0; t < writeSeconds; t += Time.deltaTime)
            {
                SetReveal(t / writeSeconds);
                yield return null;
            }
            SetReveal(1f);
            Revealed = true;
            writing = false;
        }

        public void Crumble()
        {
            if (crumbleDust != null) crumbleDust.Play();
            SetReveal(0f);
        }

        void SetReveal(float v)
        {
            chalk.GetPropertyBlock(block);
            block.SetFloat(RevealId, v);
            chalk.SetPropertyBlock(block);
        }
    }
}
