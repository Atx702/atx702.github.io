using UnityEngine;

namespace LastNight.Player
{
    /// <summary>
    /// A capsule that follows the headset so trigger volumes (chapter exits, porches, anchor portals)
    /// see the player. Lives on its own child of the XR Origin, tagged "Player", with a kinematic
    /// Rigidbody so trigger events fire.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerBody : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] float radius = 0.2f;

        CapsuleCollider capsule;

        void Awake()
        {
            capsule = GetComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = radius;
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            if (head == null && Camera.main != null) head = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (head == null) return;
            // The body stands under the head; its height follows the head (sitting, kneeling).
            var parent = transform.parent;
            var floorY = parent != null ? parent.position.y : 0f;
            float height = Mathf.Max(0.5f, head.position.y - floorY + 0.1f);
            transform.position = new Vector3(head.position.x, floorY, head.position.z);
            transform.rotation = Quaternion.identity;
            capsule.height = height;
            capsule.center = new Vector3(0f, height * 0.5f, 0f);
        }
    }
}
