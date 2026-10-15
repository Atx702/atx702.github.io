using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>A piece of an earlier memory (kitchen floor, train roof, desks, dock, gurney) that
    /// drifts toward the center and slowly tilts. The player can stand on it.</summary>
    public class DriftingFragment : MonoBehaviour
    {
        [SerializeField] float driftSpeed = 0.15f;
        [SerializeField] float stopDistance = 18f;
        [Tooltip("Keep walkable fragments at 12° or less; decorative ones can tilt to 35°.")]
        [SerializeField] float maxTilt = 12f;
        [SerializeField] float tiltSpeed = 2f;
        [SerializeField] float bobAmplitude = 0.15f;

        Vector3 target;
        Vector3 tiltAxis;
        bool running;
        float seed, tilt;

        public void Begin(Vector3 center)
        {
            target = center;
            tiltAxis = Random.onUnitSphere;
            tiltAxis.y = 0f;
            tiltAxis.Normalize();
            seed = Random.value * 10f;
            running = true;
        }

        void Update()
        {
            if (!running) return;
            var flat = target - transform.position;
            flat.y = 0;
            if (flat.magnitude > stopDistance)
                transform.position += flat.normalized * driftSpeed * Time.deltaTime;

            tilt = Mathf.MoveTowards(tilt, maxTilt * Mathf.Sin(Time.time * 0.05f + seed), tiltSpeed * Time.deltaTime);
            transform.rotation = Quaternion.AngleAxis(tilt, tiltAxis);
            transform.position += Vector3.up * (Mathf.Sin(Time.time * 0.5f + seed) * bobAmplitude * Time.deltaTime);
        }
    }
}
