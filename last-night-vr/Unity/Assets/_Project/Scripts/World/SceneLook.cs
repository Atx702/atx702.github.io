using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// Per-scene camera look. The camera lives in Core.unity, so each chapter scene tells it what to
    /// clear to (matching the fog, so the world dissolves into it) and how far to draw.
    /// Fog, ambient light and the sun come from the active scene's Lighting settings.
    /// </summary>
    public class SceneLook : MonoBehaviour
    {
        [SerializeField] Color background = new Color(0.78f, 0.79f, 0.8f);
        [SerializeField] float farClip = 160f;

        void OnEnable() => Apply();

        void Start() => Apply(); // the camera may arrive after this scene (editor testing)

        void Apply()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.farClipPlane = farClip;
        }
    }
}
