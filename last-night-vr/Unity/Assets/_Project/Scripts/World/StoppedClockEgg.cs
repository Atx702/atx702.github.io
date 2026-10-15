using LastNight.Dialogue;
using LastNight.Player;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// Put on any 3:12 clock. Staring at it for 60 seconds plays EGG_004 (once per session).
    /// </summary>
    public class StoppedClockEgg : MonoBehaviour
    {
        [SerializeField] float seconds = 60f;
        [SerializeField] float maxDistance = 6f;

        float timer;

        void Update()
        {
            bool staring = Gaze.DistanceTo(transform.position) < maxDistance && Gaze.IsLookingAt(transform.position, 8f);
            timer = staring ? timer + Time.deltaTime : 0f;
            if (timer >= seconds)
            {
                DialoguePlayer.Instance?.PlayOnce("EGG_004", transform);
                enabled = false;
            }
        }
    }
}
