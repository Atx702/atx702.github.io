using System.Collections;
using UnityEngine;

namespace LastNight.Core
{
    /// <summary>
    /// Fades the view to a color. Put this on a small inverted sphere (or quad) parented to the
    /// XR camera, using an unlit transparent material with "_Color". Rendered last (queue 5000).
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        static ScreenFader instance;

        [SerializeField] Renderer fadeRenderer;
        [SerializeField] Color color = Color.black;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        MaterialPropertyBlock block;
        float alpha;

        void Awake()
        {
            instance = this;
            block = new MaterialPropertyBlock();
            Apply(0f);
        }

        public static IEnumerator FadeOut(float seconds, Color? to = null) => Fade(1f, seconds, to);
        public static IEnumerator FadeIn(float seconds) => Fade(0f, seconds, null);

        /// <summary>Instant flash for shocks and Pull catches. Respects flash reduction.</summary>
        public static IEnumerator Flash(Color flashColor, float hold = 0.25f)
        {
            var reduced = GameManager.Instance != null && GameManager.Instance.Settings.flashReduction;
            if (reduced)
            {
                yield return Fade(0.6f, 0.4f, flashColor);
                yield return Fade(0f, 0.6f, null);
                yield break;
            }
            if (instance == null) yield break;
            instance.color = flashColor;
            instance.Apply(1f);
            yield return new WaitForSecondsRealtime(hold);
            yield return Fade(0f, 0.5f, null);
        }

        static IEnumerator Fade(float target, float seconds, Color? to)
        {
            if (instance == null) yield break;
            if (to.HasValue) instance.color = to.Value;
            var start = instance.alpha;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                instance.Apply(Mathf.Lerp(start, target, t / seconds));
                yield return null;
            }
            instance.Apply(target);
        }

        void Apply(float a)
        {
            alpha = a;
            fadeRenderer.enabled = a > 0.001f;
            fadeRenderer.GetPropertyBlock(block);
            block.SetColor(ColorId, new Color(color.r, color.g, color.b, a));
            fadeRenderer.SetPropertyBlock(block);
        }
    }
}
