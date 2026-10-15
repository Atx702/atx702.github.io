using System;
using LastNight.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastNight.Player
{
    /// <summary>
    /// "Close your eyes": the core mechanic. Triggered by covering the headset with both hands
    /// (both controllers within a radius of the eyes) or by holding a button. While closed, the
    /// view fades to black, sound is muffled, and the Pull loses track of the player.
    /// </summary>
    public class EyesClosed : MonoBehaviour
    {
        public static EyesClosed Instance { get; private set; }

        [SerializeField] Transform head;
        [SerializeField] Transform leftHand, rightHand;
        [SerializeField] InputActionReference closeEyesButton;
        [SerializeField] float gestureRadius = 0.16f;
        [SerializeField] float fadeSpeed = 4f;
        [SerializeField] Renderer eyelidRenderer; // inverted sphere around the head, black unlit, alpha-blended
        [SerializeField] UnityEngine.Audio.AudioMixer mixer;
        [SerializeField] string muffleParameter = "WorldLowpass"; // exposed lowpass cutoff on the World group

        public bool IsClosed { get; private set; }
        public float ClosedFor { get; private set; }
        /// <summary>0 = fully open, 1 = fully closed (visual).</summary>
        public float Amount { get; private set; }

        public event Action Closed;
        public event Action Opened;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        MaterialPropertyBlock block;
        bool toggledClosed;
        bool buttonWasDown;
        AudioLowPassFilter listenerFilter;

        void Awake()
        {
            Instance = this;
            block = new MaterialPropertyBlock();
        }

        void OnEnable() => closeEyesButton?.action.Enable();

        void Update()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.Settings : new GameSettings();
            bool gesture = s.eyesClosedInput != EyesClosedInput.Button && HandsOverEyes();
            bool buttonDown = s.eyesClosedInput != EyesClosedInput.Gesture &&
                              closeEyesButton != null && closeEyesButton.action.IsPressed();

            bool button;
            if (s.toggleInsteadOfHold)
            {
                if (buttonDown && !buttonWasDown) toggledClosed = !toggledClosed;
                button = toggledClosed;
            }
            else button = buttonDown;
            buttonWasDown = buttonDown;

            bool want = gesture || button;
            if (want != IsClosed)
            {
                IsClosed = want;
                ClosedFor = 0f;
                if (IsClosed) Closed?.Invoke(); else Opened?.Invoke();
            }
            if (IsClosed) ClosedFor += Time.deltaTime;

            Amount = Mathf.MoveTowards(Amount, IsClosed ? 1f : 0f, fadeSpeed * Time.deltaTime);
            ApplyVisuals();
        }

        bool HandsOverEyes()
        {
            var eyes = head.position + head.forward * 0.08f;
            return Vector3.Distance(leftHand.position, eyes) < gestureRadius &&
                   Vector3.Distance(rightHand.position, eyes) < gestureRadius;
        }

        void ApplyVisuals()
        {
            if (eyelidRenderer != null)
            {
                eyelidRenderer.enabled = Amount > 0.001f;
                eyelidRenderer.GetPropertyBlock(block);
                block.SetColor(ColorId, new Color(0, 0, 0, Amount * 0.97f)); // a sliver of light gets through
                eyelidRenderer.SetPropertyBlock(block);
            }
            if (mixer != null)
                mixer.SetFloat(muffleParameter, Mathf.Lerp(22000f, 700f, Amount));
            else
            {
                // No mixer: muffle everything the listener hears.
                if (listenerFilter == null && head != null)
                {
                    listenerFilter = head.GetComponent<AudioLowPassFilter>();
                    if (listenerFilter == null && head.GetComponent<AudioListener>() != null)
                        listenerFilter = head.gameObject.AddComponent<AudioLowPassFilter>();
                }
                if (listenerFilter != null)
                {
                    listenerFilter.enabled = Amount > 0.001f;
                    listenerFilter.cutoffFrequency = Mathf.Lerp(22000f, 700f, Amount);
                }
            }
        }
    }
}
