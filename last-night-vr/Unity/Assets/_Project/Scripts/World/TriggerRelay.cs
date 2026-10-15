using UnityEngine;
using UnityEngine.Events;

namespace LastNight.World
{
    /// <summary>
    /// A trigger volume that tells a director the player arrived ("OnPorch", "OnExit"...).
    /// Needs a Collider with isTrigger. The player's body collider is tagged "Player".
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerRelay : MonoBehaviour
    {
        [SerializeField] bool once = true;
        [Tooltip("If set, fires when this object enters (e.g. the toy boat dropped in the lake) instead of the player.")]
        [SerializeField] GameObject requiredObject;
        [SerializeField] UnityEvent onEnter;
        [SerializeField] UnityEvent onExit;

        bool fired;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        bool Matches(Collider other) => requiredObject != null
            ? other.attachedRigidbody != null ? other.attachedRigidbody.gameObject == requiredObject : other.gameObject == requiredObject
            : other.CompareTag("Player");

        void OnTriggerEnter(Collider other)
        {
            if (!Matches(other) || (once && fired)) return;
            fired = true;
            onEnter?.Invoke();
        }

        void OnTriggerExit(Collider other)
        {
            if (Matches(other)) onExit?.Invoke();
        }
    }
}
