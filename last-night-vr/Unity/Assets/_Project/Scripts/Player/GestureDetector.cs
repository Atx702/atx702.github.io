using UnityEngine;
using UnityEngine.Events;

namespace LastNight.Player
{
    /// <summary>
    /// Simple body gestures used by the story:
    /// - RaiseHand: either hand above the head for holdTime (roll call, Ch.2)
    /// - HandsOut: both hands forward at chest height (prologue "hold out your hands", secret ending ring)
    /// - Wave: side-to-side hand motion (easter egg at the train, Ch.3)
    /// - HeadUpsideDown: head roll past 150° (blueprint easter egg, Ch.2)
    /// Enable only the gestures a scene needs; each fires its UnityEvent once until Rearm().
    /// </summary>
    public class GestureDetector : MonoBehaviour
    {
        public enum Kind { RaiseHand, HandsOut, Wave, HeadUpsideDown }

        [SerializeField] Kind kind;
        [SerializeField] Transform head, leftHand, rightHand;
        [SerializeField] float holdTime = 1f;
        [SerializeField] int wavesRequired = 3;
        [SerializeField] UnityEvent onGesture;

        float held;
        bool fired;

        void Awake()
        {
            // Chapter scenes can't point at the rig in Core, so fill in from PlayerRig.
            var rig = PlayerRig.Instance;
            if (rig == null) return;
            if (head == null) head = rig.Head;
            if (leftHand == null) leftHand = rig.LeftHand;
            if (rightHand == null) rightHand = rig.RightHand;
        }
        int waveCount;
        float lastX;
        int lastDir;
        float waveTimer;

        public void Rearm()
        {
            fired = false;
            held = 0;
            waveCount = 0;
        }

        void Update()
        {
            if (fired || head == null || leftHand == null || rightHand == null) return;
            bool active = kind switch
            {
                Kind.RaiseHand => leftHand.position.y > head.position.y + 0.1f || rightHand.position.y > head.position.y + 0.1f,
                Kind.HandsOut => HandOut(leftHand) && HandOut(rightHand),
                Kind.HeadUpsideDown => Vector3.Angle(head.up, Vector3.up) > 150f,
                Kind.Wave => UpdateWave(),
                _ => false,
            };

            if (kind == Kind.Wave)
            {
                if (active) Fire();
                return;
            }

            held = active ? held + Time.deltaTime : 0f;
            if (held >= holdTime) Fire();
        }

        bool HandOut(Transform hand)
        {
            var local = head.InverseTransformPoint(hand.position);
            return local.z > 0.35f && local.y < 0f && local.y > -0.5f;
        }

        bool UpdateWave()
        {
            var hand = rightHand.position.y > leftHand.position.y ? rightHand : leftHand;
            if (hand.position.y < head.position.y - 0.2f) { waveCount = 0; return false; }

            var x = head.InverseTransformPoint(hand.position).x;
            int dir = x > lastX + 0.02f ? 1 : x < lastX - 0.02f ? -1 : lastDir;
            if (dir != lastDir && lastDir != 0) { waveCount++; waveTimer = 0f; }
            lastDir = dir;
            lastX = x;

            waveTimer += Time.deltaTime;
            if (waveTimer > 0.8f) waveCount = 0;
            return waveCount >= wavesRequired * 2; // two direction changes per wave
        }

        void Fire()
        {
            fired = true;
            onGesture?.Invoke();
        }
    }
}
