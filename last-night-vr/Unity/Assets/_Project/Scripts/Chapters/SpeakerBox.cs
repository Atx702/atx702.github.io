using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 2's old PA speaker. Beeps (the real heart monitor, leaking) from 3D space until the
    /// player presses its switch. Uses a 3D AudioSource with HRTF spatialization so it can be found by ear.
    /// </summary>
    public class SpeakerBox : MonoBehaviour
    {
        [SerializeField] AudioSource beep;
        [SerializeField] XRSimpleInteractable toggleSwitch;
        [SerializeField] Transform switchVisual;
        [SerializeField] AudioSource clunk;
        [SerializeField] float beepInterval = 1.1f;

        public bool Silenced { get; private set; }
        bool beeping;
        float timer;

        void Awake() => toggleSwitch.selectEntered.AddListener(_ => Silence());

        public void StartBeeping()
        {
            beeping = true;
            timer = Random.Range(0f, beepInterval); // desync the three speakers
        }

        void Update()
        {
            if (!beeping || Silenced) return;
            timer += Time.deltaTime;
            if (timer >= beepInterval)
            {
                timer = 0f;
                beep.Play();
            }
        }

        void Silence()
        {
            if (Silenced || !beeping) return;
            Silenced = true;
            if (clunk != null) clunk.Play();
            if (switchVisual != null) switchVisual.localRotation = Quaternion.Euler(-30f, 0, 0);
        }
    }
}
