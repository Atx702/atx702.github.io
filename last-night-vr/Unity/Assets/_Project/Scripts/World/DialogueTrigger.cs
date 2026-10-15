using LastNight.Dialogue;
using LastNight.Player;
using UnityEngine;
using UnityEngine.Events;

namespace LastNight.World
{
    /// <summary>
    /// Plays one or more lines when the player walks into this trigger, or looks at it, or both.
    /// The everyday building block for scripting the story in the editor without code.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour
    {
        public enum When { OnEnter, OnLook, OnEnterAndLook, Manual }

        [SerializeField] When when = When.OnEnter;
        [SerializeField] string[] lineIds;
        [SerializeField] Transform speaker;
        [SerializeField] float lookCone = 12f;
        [SerializeField] float lookSeconds = 0.6f;
        [SerializeField] float maxLookDistance = 30f;
        [SerializeField] bool interruptCurrent;
        [SerializeField] UnityEvent onFinished;

        bool inside, fired;
        float lookTimer;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            inside = true;
            if (when == When.OnEnter) Fire();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) inside = false;
        }

        void Update()
        {
            if (fired || (when != When.OnLook && when != When.OnEnterAndLook)) return;
            if (when == When.OnEnterAndLook && !inside) return;

            var target = speaker != null ? speaker.position : transform.position;
            bool looking = Gaze.DistanceTo(target) < maxLookDistance && Gaze.IsLookingAt(target, lookCone);
            lookTimer = looking ? lookTimer + Time.deltaTime : 0f;
            if (lookTimer >= lookSeconds) Fire();
        }

        public void Fire()
        {
            if (fired || DialoguePlayer.Instance == null) return;
            fired = true;
            if (interruptCurrent) DialoguePlayer.Instance.Interrupt();
            for (int i = 0; i < lineIds.Length; i++)
            {
                bool last = i == lineIds.Length - 1;
                DialoguePlayer.Instance.Play(lineIds[i], speaker, last ? () => onFinished?.Invoke() : null);
            }
        }
    }
}
