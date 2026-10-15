using Unity.XR.CoreUtils;
using UnityEngine;

namespace LastNight.Player
{
    /// <summary>
    /// Lives on the XR rig in Core.unity. Chapter scenes can't reference objects in another scene,
    /// so anything that needs the player's head, hands, mouth or ring looks here instead.
    /// </summary>
    public class PlayerRig : MonoBehaviour
    {
        public static PlayerRig Instance { get; private set; }

        [SerializeField] XROrigin origin;
        [SerializeField] Transform head, leftHand, rightHand, mouth;
        [SerializeField] GameObject handRing;

        public XROrigin Origin => origin;
        public Transform Head => head;
        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;
        public Transform Mouth => mouth;
        public GameObject HandRing => handRing;
        public Camera Camera => head != null ? head.GetComponent<Camera>() : Camera.main;

        void Awake() => Instance = this;
    }
}
