using LastNight.Dialogue;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.World
{
    /// <summary>
    /// Put on memory objects. If the player throws one (released fast), the Voice comments
    /// (SYS_THROW_001/002), at most once every couple of minutes.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class ThrowWatcher : MonoBehaviour
    {
        [SerializeField] float throwSpeed = 2.5f;

        static readonly string[] Lines = { "SYS_THROW_001", "SYS_THROW_002" };
        static float lastSpoke = -999f;
        static int lastLine = -1;

        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            GetComponent<XRGrabInteractable>().selectExited.AddListener(_ => Invoke(nameof(Check), 0.05f));
        }

        void Check()
        {
            if (body.linearVelocity.magnitude < throwSpeed || Time.time - lastSpoke < 120f) return;
            if (DialoguePlayer.Instance == null || DialoguePlayer.Instance.IsPlaying) return;
            lastSpoke = Time.time;
            DialoguePlayer.Instance.PlayRandom(Lines, ref lastLine);
        }
    }
}
