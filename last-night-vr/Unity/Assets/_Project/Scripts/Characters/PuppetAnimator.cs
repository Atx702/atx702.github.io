using LastNight.Player;
using UnityEngine;

namespace LastNight.Characters
{
    /// <summary>
    /// Poses a jointed character procedurally: no Animator, no clips. The characters are built as rigid
    /// body parts hung from joints (Kit.Character), and every move here is a few joint angles plus
    /// motion over time (breathing, walking, CPR compressions on the beat, waving...).
    ///
    /// Angles are in the joint's parent space with the body in its bind A-pose: X swings a limb forward
    /// (negative) or back, Z swings arms in and out. Arms are mirrored automatically by which side of
    /// the body they're on, so poses read the same for left and right.
    /// Moves blend over a fraction of a second; walking layers on by itself whenever the character moves.
    /// </summary>
    public class PuppetAnimator : MonoBehaviour
    {
        public enum Move
        {
            Idle, Walk, Kneel, Cpr, Sit, SitTalk, SitFloorDraw, Wave, ArmsUp, HoldUp, HandsOverMouth,
            Laugh, Lean, Reach, ReachDown, Held, BagValve, Carry, Still,
        }

        // Joint order used everywhere in this file.
        public const int Hips = 0, Spine = 1, Chest = 2, Neck = 3, Head = 4,
            UArmL = 5, FArmL = 6, HandL = 7, UArmR = 8, FArmR = 9, HandR = 10,
            ThighL = 11, ShinL = 12, FootL = 13, ThighR = 14, ShinR = 15, FootR = 16, Count = 17;

        public static readonly string[] JointNames =
        {
            "hips", "spine", "chest", "neck", "head", "upperarm_L", "forearm_L", "hand_L", "upperarm_R", "forearm_R", "hand_R",
            "thigh_L", "shin_L", "foot_L", "thigh_R", "shin_R", "foot_R",
        };

        [SerializeField] Transform[] joints = new Transform[Count];
        [SerializeField] Move move = Move.Idle;
        [Tooltip("Turn the head toward the player (0 = never, 1 = fully, within ±60°).")]
        [Range(0, 1)] [SerializeField] float lookAtPlayer = 0.5f;
        [SerializeField] bool autoWalk = true;
        [Tooltip("Chest compressions per second for Cpr (110 BPM = 1.83; the Ch.7 slow motion uses 0.55).")]
        [SerializeField] float cprRate = 1.83f;
        [SerializeField] float seatHeight = 0.48f;
        [SerializeField] float blendSeconds = 0.4f;

        public Move Current => move;

        Quaternion[] from = new Quaternion[Count];
        float fromDrop, blend = 1f, time;
        float hipsY, thighLen, shinLen, sideL = -1f, sideR = 1f;
        Vector3 hipsBind, lastPos;
        float speed;

        void Awake()
        {
            if (joints[Hips] == null) return;
            hipsBind = joints[Hips].localPosition;
            hipsY = hipsBind.y;
            thighLen = joints[ShinL] != null ? joints[ShinL].localPosition.magnitude : hipsY * 0.45f;
            shinLen = joints[FootL] != null ? joints[FootL].localPosition.magnitude : hipsY * 0.45f;
            // Which way is "out" for each arm depends on which side it ended up after import.
            if (joints[UArmL] != null) sideL = Mathf.Sign(transform.InverseTransformPoint(joints[UArmL].position).x);
            if (joints[UArmR] != null) sideR = Mathf.Sign(transform.InverseTransformPoint(joints[UArmR].position).x);
            if (Mathf.Approximately(sideL, sideR)) { sideL = -1f; sideR = 1f; }
            lastPos = transform.position;
            time = Random.value * 10f;
            for (int i = 0; i < Count; i++) from[i] = Quaternion.identity;
        }

        /// <summary>Blend to another move (cutscenes and directors call this).</summary>
        public void Play(Move m)
        {
            if (m == move) return;
            for (int i = 0; i < Count; i++) from[i] = joints[i] != null ? joints[i].localRotation : Quaternion.identity;
            fromDrop = joints[Hips] != null ? hipsBind.y - joints[Hips].localPosition.y : 0f;
            move = m;
            blend = 0f;
        }

        public void SetCprRate(float compressionsPerSecond) => cprRate = compressionsPerSecond;

        void LateUpdate()
        {
            if (joints[Hips] == null) return;
            time += Time.deltaTime;
            var flat = transform.position - lastPos;
            flat.y = 0f;
            float instant = Time.deltaTime > 0f ? flat.magnitude / Time.deltaTime : 0f;
            speed = Mathf.Lerp(speed, instant, 1f - Mathf.Exp(-8f * Time.deltaTime));
            lastPos = transform.position;

            var euler = new Vector3[Count];
            elbowL = elbowR = 0f;
            float drop = Evaluate(move, euler);

            // Walking layers onto standing moves whenever the character is being moved.
            float walk = autoWalk && (move == Move.Idle || move == Move.Carry || move == Move.Still) ? Mathf.InverseLerp(0.15f, 0.6f, speed) : 0f;
            if (move == Move.Walk) walk = 1f;
            if (walk > 0f) Gait(euler, walk, move != Move.Carry);

            blend = Mathf.MoveTowards(blend, 1f, Time.deltaTime / Mathf.Max(0.01f, blendSeconds));
            float k = blend * blend * (3f - 2f * blend);
            for (int i = 0; i < Count; i++)
            {
                if (joints[i] == null) continue;
                var target = Quaternion.Euler(euler[i]);
                // Elbows bend around the arm's own hinge, which in the A-pose is tilted 35° with the arm.
                if (i == FArmL) target = Hinge(sideL, elbowL) * target;
                if (i == FArmR) target = Hinge(sideR, elbowR) * target;
                joints[i].localRotation = k >= 1f ? target : Quaternion.Slerp(from[i], target, k);
            }
            joints[Hips].localPosition = hipsBind - Vector3.up * Mathf.Lerp(fromDrop, drop, k);
            LookAtPlayer();
        }

        // ------------------------------------------------------------------ poses

        float elbowL, elbowR;

        static Quaternion Hinge(float side, float bend) =>
            Quaternion.Euler(0f, 0f, side * 35f) * Quaternion.Euler(bend, 0f, 0f) * Quaternion.Euler(0f, 0f, -side * 35f);

        void Arms(Vector3[] e, float forward, float outward, float elbow, float forwardR = float.NaN, float outwardR = float.NaN, float elbowR = float.NaN)
        {
            // From the bind A-pose (35° out), "outward" 0 puts the arm straight down at the side.
            e[UArmL] = new Vector3(forward, 0f, -sideL * (35f - outward));
            e[FArmL] = Vector3.zero;
            this.elbowL = elbow;
            e[UArmR] = new Vector3(float.IsNaN(forwardR) ? forward : forwardR, 0f, -sideR * (35f - (float.IsNaN(outwardR) ? outward : outwardR)));
            e[FArmR] = Vector3.zero;
            this.elbowR = float.IsNaN(elbowR) ? elbow : elbowR;
        }

        void Breathe(Vector3[] e, float amount = 1f)
        {
            e[Chest].x += Mathf.Sin(time * 1.6f) * 1.2f * amount;
            e[Head].x += Mathf.Sin(time * 1.6f + 0.6f) * 0.8f * amount;
        }

        float Kneel(Vector3[] e)
        {
            e[ShinL].x = e[ShinR].x = 90f;
            e[FootL].x = e[FootR].x = 10f;
            return hipsY - (thighLen + 0.07f);
        }

        float SitChair(Vector3[] e)
        {
            e[ThighL].x = e[ThighR].x = -88f;
            e[ShinL].x = e[ShinR].x = 88f;
            return Mathf.Max(0f, hipsY - seatHeight);
        }

        float Evaluate(Move m, Vector3[] e)
        {
            float drop = 0f;
            Arms(e, 0f, 6f, -6f);
            switch (m)
            {
                case Move.Idle:
                case Move.Walk:
                    Breathe(e);
                    break;

                case Move.Still:
                    break;

                case Move.Kneel:
                    drop = Kneel(e);
                    e[Spine].x = 6f;
                    Arms(e, -10f, 4f, -20f);
                    Breathe(e);
                    break;

                case Move.Cpr:
                {
                    drop = Kneel(e);
                    // Push on the down-stroke of each compression: arms locked, weight over the hands.
                    float push = Mathf.Max(0f, Mathf.Sin(time * cprRate * Mathf.PI * 2f));
                    e[Spine].x = 26f + push * 7f;
                    e[Chest].x = 10f + push * 3f;
                    e[Head].x = 12f;
                    Arms(e, -62f, -6f, 0f);
                    drop += push * 0.05f;
                    break;
                }

                case Move.Sit:
                    drop = SitChair(e);
                    Arms(e, -35f, 2f, -45f);
                    Breathe(e);
                    break;

                case Move.SitTalk:
                    drop = SitChair(e);
                    e[Spine].x = 12f;
                    Arms(e, -42f + Mathf.Sin(time * 1.3f) * 4f, -4f, -55f);
                    Breathe(e);
                    break;

                case Move.SitFloorDraw:
                {
                    e[ThighL] = new Vector3(-80f, sideL * 32f, 0f);
                    e[ThighR] = new Vector3(-80f, sideR * 32f, 0f);
                    e[ShinL].x = e[ShinR].x = 150f;
                    drop = Mathf.Max(0f, hipsY - 0.12f);
                    e[Spine].x = 24f;
                    e[Head].x = 18f;
                    Arms(e, -28f, 0f, -20f, -48f + Mathf.Sin(time * 3.1f) * 6f, -2f + Mathf.Cos(time * 3.1f) * 6f, -35f);
                    break;
                }

                case Move.Wave:
                    Breathe(e);
                    Arms(e, 0f, 6f, -6f);
                    e[UArmR] = new Vector3(-10f, 0f, sideR * 105f);
                    e[FArmR] = new Vector3(0f, 0f, Mathf.Sin(time * 7f) * 28f);
                    break;

                case Move.ArmsUp:
                    Arms(e, -150f + Mathf.Sin(time * 2.2f) * 5f, 12f, -10f);
                    Breathe(e);
                    break;

                case Move.HoldUp:
                    Arms(e, -78f, -8f, -28f);
                    e[Head].x = -6f;
                    break;

                case Move.HandsOverMouth:
                    Arms(e, -32f, -14f, -128f);
                    e[Head].x = 14f;
                    e[Spine].x = 8f;
                    drop = Kneel(e);
                    break;

                case Move.Laugh:
                    Arms(e, 0f, 6f, -6f, -30f, -16f, -105f);
                    e[Head].x = -12f + Mathf.Sin(time * 9f) * 3f;
                    e[Chest].x = Mathf.Sin(time * 9f) * 2.5f;
                    break;

                case Move.Lean:
                    e[Spine].x = 14f;
                    e[Hips].y = 10f;
                    Arms(e, 0f, 6f, -6f, -55f, 4f, -12f);
                    Breathe(e);
                    break;

                case Move.Reach:
                    drop = SitChair(e);
                    e[Spine].x = 16f;
                    Arms(e, -35f, 2f, -45f, -78f, -4f, -6f);
                    break;

                case Move.ReachDown:
                    e[Spine].x = 34f;
                    e[Chest].x = 10f;
                    e[ThighL].x = e[ThighR].x = -18f;
                    e[ShinL].x = e[ShinR].x = 22f;
                    Arms(e, -72f, -4f, -8f);
                    drop = 0.06f;
                    break;

                case Move.Held:
                    Arms(e, -62f, -16f, -42f);
                    e[ThighL].x = e[ThighR].x = -42f;
                    e[ShinL].x = e[ShinR].x = 50f;
                    e[Head].x = 8f;
                    Breathe(e, 1.5f);
                    break;

                case Move.BagValve:
                {
                    drop = Kneel(e);
                    e[Spine].x = 20f;
                    float squeeze = Mathf.Max(0f, Mathf.Sin(time * 0.9f * Mathf.PI * 2f));
                    Arms(e, -60f, -6f, -38f - squeeze * 10f);
                    break;
                }

                case Move.Carry:
                    Arms(e, -32f, -6f, -62f);
                    Breathe(e);
                    break;
            }
            return drop;
        }

        void Gait(Vector3[] e, float weight, bool swingArms)
        {
            float cadence = Mathf.Lerp(1.4f, 2.2f, Mathf.InverseLerp(0.5f, 2f, speed));
            float ph = time * cadence * Mathf.PI * 2f;
            float s = Mathf.Sin(ph), c = Mathf.Sin(ph + Mathf.PI);
            e[ThighL].x = Mathf.Lerp(e[ThighL].x, -24f * s, weight);
            e[ThighR].x = Mathf.Lerp(e[ThighR].x, -24f * c, weight);
            e[ShinL].x = Mathf.Lerp(e[ShinL].x, 12f + 26f * Mathf.Max(0f, -s), weight);
            e[ShinR].x = Mathf.Lerp(e[ShinR].x, 12f + 26f * Mathf.Max(0f, -c), weight);
            if (swingArms)
            {
                e[UArmL].x = Mathf.Lerp(e[UArmL].x, 16f * s, weight);
                e[UArmR].x = Mathf.Lerp(e[UArmR].x, 16f * c, weight);
            }
            e[Spine].x += 3f * weight;
        }

        void LookAtPlayer()
        {
            var head = joints[Head];
            if (lookAtPlayer <= 0f || head == null || Gaze.Head == null || joints[Neck] == null) return;
            var local = joints[Neck].InverseTransformPoint(Gaze.Head.position);
            float yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -60f, 60f);
            float pitch = Mathf.Clamp(-Mathf.Atan2(local.y - head.localPosition.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg, -25f, 25f);
            if (local.z < -0.2f) return; // behind: don't snap the head around
            head.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(pitch, yaw, 0f), lookAtPlayer) * head.localRotation;
        }
    }
}
