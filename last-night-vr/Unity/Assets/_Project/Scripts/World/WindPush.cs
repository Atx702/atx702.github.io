using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// A gust that actually moves things: WindZone only affects particles and trees, so this pushes the
    /// listed rigidbodies (the Ch.3 scarf) while its GameObject is active. Gusty, not constant.
    /// </summary>
    public class WindPush : MonoBehaviour
    {
        [SerializeField] Rigidbody[] bodies;
        [SerializeField] Vector3 direction = new Vector3(0, 0.3f, -1f);
        [SerializeField] float strength = 1.6f;
        [SerializeField] float gustiness = 0.8f;

        void FixedUpdate()
        {
            float gust = 1f + Mathf.Sin(Time.time * 2.3f) * gustiness * 0.5f + (Mathf.PerlinNoise(Time.time * 0.9f, 0.3f) - 0.5f) * gustiness;
            foreach (var b in bodies)
            {
                if (b == null || b.isKinematic) continue;
                b.AddForce(direction.normalized * strength * Mathf.Max(0f, gust), ForceMode.Acceleration);
            }
        }
    }
}
