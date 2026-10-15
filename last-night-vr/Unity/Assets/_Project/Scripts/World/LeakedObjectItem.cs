using LastNight.Core;
using LastNight.Dialogue;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.World
{
    /// <summary>
    /// A leaked object from the real world (glove, pulse clip, scarf, ring, bracelet).
    /// Counts as found after being held for holdSeconds. Plays its reaction lines once.
    /// Needs an XRGrabInteractable on the same object.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class LeakedObjectItem : MonoBehaviour
    {
        [SerializeField] LeakedObject which;
        [SerializeField] float holdSeconds = 2f;
        [SerializeField] string[] linesOnPickup;

        XRGrabInteractable grab;
        float held;
        bool counted, linesPlayed;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            grab.selectEntered.AddListener(OnGrab);
        }

        void OnDestroy() => grab.selectEntered.RemoveListener(OnGrab);

        void OnGrab(SelectEnterEventArgs _)
        {
            if (linesPlayed || DialoguePlayer.Instance == null) return;
            linesPlayed = true;
            foreach (var id in linesOnPickup) DialoguePlayer.Instance.Play(id);
        }

        void Update()
        {
            if (counted || !grab.isSelected) { held = 0f; return; }
            held += Time.deltaTime;
            if (held >= holdSeconds)
            {
                counted = true;
                GameManager.Instance?.ReportLeakedObject(which);
            }
        }
    }
}
