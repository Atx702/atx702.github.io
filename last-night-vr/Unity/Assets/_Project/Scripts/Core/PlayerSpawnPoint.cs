using Unity.XR.CoreUtils;
using UnityEngine;

namespace LastNight.Core
{
    /// <summary>
    /// Where the player appears when a scene loads, and where the Pull sends them back to.
    /// Each chapter scene has one default spawn; extra ones can be activated as checkpoints.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        static PlayerSpawnPoint activeCheckpoint;

        [SerializeField] bool isDefault = true;
        [Tooltip("In the Field scene: use this spawn when returning from this chapter's anchor.")]
        [SerializeField] bool isReturnPoint;
        [SerializeField] Chapter returnFrom;
        [Tooltip("Use this spawn when this chapter starts in a scene shared by several chapters (the Field).")]
        [SerializeField] bool isChapterStart;
        [SerializeField] Chapter startOf;

        public bool IsDefault => isDefault;
        public bool IsReturnPointFor(Chapter c) => isReturnPoint && returnFrom == c;
        public bool IsStartFor(Chapter c) => isChapterStart && startOf == c;

        void OnEnable()
        {
            if (isDefault && activeCheckpoint == null) activeCheckpoint = this;
        }

        void OnDisable()
        {
            if (activeCheckpoint == this) activeCheckpoint = null;
        }

        public void SetAsCheckpoint() => activeCheckpoint = this;

        public static void RespawnAtCheckpoint()
        {
            if (activeCheckpoint != null) activeCheckpoint.MovePlayerHere();
        }

        public void MovePlayerHere()
        {
            var origin = FindAnyObjectByType<XROrigin>();
            if (origin == null) return;
            origin.MoveCameraToWorldLocation(transform.position + Vector3.up * origin.CameraInOriginSpaceHeight);
            origin.MatchOriginUpCameraForward(Vector3.up, transform.forward);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = isDefault ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}
