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
        [SerializeField] UnityEvent onEnter;
        [SerializeField] UnityEvent onExit;

        bool fired;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || (once && fired)) return;
            fired = true;
            onEnter?.Invoke();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) onExit?.Invoke();
        }
    }
}
