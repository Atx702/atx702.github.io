using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.Interaction
{
    /// <summary>
    /// A knob you grab and twist (radio tuner, stove knob, alarm clock). Reads the hand's roll
    /// relative to when it grabbed, maps it to a 0..max value, and rotates the knob visual.
    /// Needs an XRSimpleInteractable (not a grab interactable, so the knob doesn't come off).
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class Dial : MonoBehaviour
    {
        [SerializeField] Transform knobVisual;
        [SerializeField] Vector3 localAxis = Vector3.forward;
        [SerializeField] float minValue = 0f, maxValue = 100f;
        [SerializeField] float degreesForFullRange = 270f;
        [SerializeField] float startValue;
        [SerializeField] UnityEvent<float> onValueChanged;

        XRSimpleInteractable interactable;
        Transform hand;
        float grabRoll, grabValue;

        public float Value { get; private set; }

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(OnGrab);
            interactable.selectExited.AddListener(_ => hand = null);
            SetValue(startValue);
        }

        void OnGrab(SelectEnterEventArgs args)
        {
            hand = args.interactorObject.transform;
            grabRoll = Roll(hand);
            grabValue = Value;
        }

        void Update()
        {
            if (hand == null) return;
            float delta = Mathf.DeltaAngle(grabRoll, Roll(hand));
            float range = maxValue - minValue;
            SetValue(grabValue - delta / degreesForFullRange * range);
        }

        float Roll(Transform t)
        {
            // Roll of the hand around the dial's axis, in the dial's space.
            var axis = transform.TransformDirection(localAxis);
            var up = Vector3.ProjectOnPlane(t.up, axis);
            var reference = Vector3.ProjectOnPlane(transform.up, axis);
            return Vector3.SignedAngle(reference, up, axis);
        }

        public void SetValue(float v)
        {
            v = Mathf.Clamp(v, minValue, maxValue);
            if (Mathf.Approximately(v, Value)) return;
            Value = v;
            if (knobVisual != null)
            {
                float t = Mathf.InverseLerp(minValue, maxValue, v);
                knobVisual.localRotation = Quaternion.AngleAxis(-t * degreesForFullRange, localAxis);
            }
            onValueChanged?.Invoke(v);
        }
    }
}
