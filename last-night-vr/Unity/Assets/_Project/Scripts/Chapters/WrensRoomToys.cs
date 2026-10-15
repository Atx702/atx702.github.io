using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Interaction;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// Chapter 8's interactables: the lantern (ask to sing; sing via mic hum or the trigger),
    /// the rubber duck (7 squeezes = Quack-quack the Seventh). Script: CH8_004-005, EGG_006.
    /// </summary>
    public class WrensRoomToys : MonoBehaviour
    {
        [SerializeField] Transform wren;
        [SerializeField] GrabSignal lantern;
        [SerializeField] Transform lanternStars;           // rotating star projector
        [SerializeField] HumDetector hum;
        [SerializeField] AudioSource lanternSong;          // MUS_Lantern_Hum (Tomas + Wren)
        [SerializeField] GrabSignal duck;
        [SerializeField] AudioSource squeak;
        [SerializeField] float starSpeed = 12f;

        bool askedToSing, sang;
        int squeezes;

        void Update()
        {
            if (lanternStars != null) lanternStars.Rotate(Vector3.up, starSpeed * Time.deltaTime, Space.Self);

            if (!askedToSing && lantern.WasGrabbed)
            {
                askedToSing = true;
                DialoguePlayer.Instance.Play("CH8_004", wren);
                if (hum != null) hum.enabled = true;
            }
            // Without the mic, pulling the trigger while holding the lantern sings.
            if (askedToSing && !sang && lantern.ActivateCount > 0) Sing();

            if (duck.ActivateCount > squeezes)
            {
                squeezes = duck.ActivateCount;
                if (squeak != null) squeak.Play();
                if (squeezes == 7)
                {
                    DialoguePlayer.Instance.Play("EGG_006", wren);
                    GameManager.Instance?.Unlock("QUACK");
                }
            }
        }

        /// <summary>Also wired from HumDetector.onHummed.</summary>
        public void Sing()
        {
            if (sang) return;
            sang = true;
            if (lanternSong != null) lanternSong.Play();
            DialoguePlayer.Instance.Play("CH8_005", wren);
            GameManager.Instance?.SetFlag(Flags.SangToWren);
            GameManager.Instance?.Unlock("LITTLE_LANTERN");
        }
    }
}
