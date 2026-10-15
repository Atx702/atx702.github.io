using UnityEngine;

namespace LastNight.Player
{
    /// <summary>
    /// Static helpers for "is the player looking at this?" Used by the Pull (freeze when seen),
    /// the shy house in Chapter 1, the chalk words, and several easter eggs.
    /// </summary>
    public static class Gaze
    {
        static Transform head;

        public static Transform Head
        {
            get
            {
                if (head == null && Camera.main != null) head = Camera.main.transform;
                return head;
            }
        }

        /// <summary>True if the point is within coneDegrees of the view center and eyes are open.</summary>
        public static bool IsLookingAt(Vector3 point, float coneDegrees = 15f, bool requireLineOfSight = false, int occluderMask = ~0)
        {
            if (Head == null) return false;
            if (EyesClosed.Instance != null && EyesClosed.Instance.Amount > 0.5f) return false;

            var to = point - Head.position;
            if (Vector3.Angle(Head.forward, to) > coneDegrees) return false;
            if (!requireLineOfSight) return true;
            return !Physics.Raycast(Head.position, to.normalized, to.magnitude - 0.2f, occluderMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>True if the point is anywhere on screen (roughly the headset's field of view).</summary>
        public static bool IsInView(Vector3 point) => IsLookingAt(point, 50f);

        public static float DistanceTo(Vector3 point) => Head == null ? float.MaxValue : Vector3.Distance(Head.position, point);
    }
}
