using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.Interaction
{
    /// <summary>
    /// Small helper so directors can wait for "grabbed", "released", "activated (trigger)" or
    /// "held for N seconds" on any interactable without wiring listeners each time.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class GrabSignal : MonoBehaviour
    {
        [SerializeField] UnityEvent onGrabbed, onReleased, onActivated;

        public XRBaseInteractable Interactable { get; private set; }
        public bool WasGrabbed { get; private set; }
        public bool WasReleased { get; private set; }
        public int ActivateCount { get; private set; }
        public float HeldSeconds { get; private set; }
        public bool IsHeld => Interactable.isSelected;

        void Awake()
        {
            Interactable = GetComponent<XRBaseInteractable>();
            Interactable.selectEntered.AddListener(_ => { WasGrabbed = true; onGrabbed?.Invoke(); });
            Interactable.selectExited.AddListener(_ => { WasReleased = true; onReleased?.Invoke(); });
            Interactable.activated.AddListener(_ => { ActivateCount++; onActivated?.Invoke(); });
        }

        void Update() => HeldSeconds = IsHeld ? HeldSeconds + Time.deltaTime : 0f;

        public void ResetSignals()
        {
            WasGrabbed = WasReleased = false;
            ActivateCount = 0;
        }
    }
}
