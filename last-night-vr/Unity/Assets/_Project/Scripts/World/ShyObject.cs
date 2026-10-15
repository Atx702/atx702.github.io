using LastNight.Player;
using UnityEngine;
using UnityEngine.Events;

namespace LastNight.World
{
    /// <summary>
    /// Chapter 1's house: it slides away while you look straight at it and comes closer when you
    /// look away, walk backwards, or close your eyes. Moves along the line toward the player.
    /// </summary>
    public class ShyObject : MonoBehaviour
    {
        [SerializeField] float retreatSpeed = 1f;
        [SerializeField] float approachSpeed = 3f;
        [SerializeField] float lookCone = 15f;
        [SerializeField] float arriveDistance = 12f;
        [SerializeField] float maxDistance = 260f;
        [SerializeField] UnityEvent onFirstRetreat;
        [SerializeField] UnityEvent onFirstEyesClosedApproach;
        [SerializeField] UnityEvent onArrived;

        bool retreated, eyesApproached, arrived;

        void Update()
        {
            if (arrived || Gaze.Head == null) return;

            var head = Gaze.Head.position;
            var toPlayer = head - transform.position;
            toPlayer.y = 0;
            float dist = toPlayer.magnitude;
            if (dist <= arriveDistance)
            {
                arrived = true;
                onArrived?.Invoke();
                return;
            }

            bool eyesClosed = EyesClosed.Instance != null && EyesClosed.Instance.IsClosed;
            bool staring = !eyesClosed && Gaze.IsLookingAt(transform.position + Vector3.up * 3f, lookCone);
            var dir = toPlayer / dist;

            if (staring)
            {
                if (dist < maxDistance) transform.position -= dir * retreatSpeed * Time.deltaTime;
                if (!retreated) { retreated = true; onFirstRetreat?.Invoke(); }
            }
            else
            {
                transform.position += dir * approachSpeed * Time.deltaTime;
                if (eyesClosed && !eyesApproached) { eyesApproached = true; onFirstEyesClosedApproach?.Invoke(); }
            }
        }
    }
}
