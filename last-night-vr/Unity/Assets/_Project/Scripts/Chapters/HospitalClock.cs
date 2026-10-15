using System.Collections;
using UnityEngine;

namespace LastNight.Chapters
{
    /// <summary>
    /// The only clock in the game that moves. Rotates hour and minute hands; Tick() animates from
    /// the current time to a target time with a loud tick per minute.
    /// </summary>
    public class HospitalClock : MonoBehaviour
    {
        [SerializeField] Transform hourHand, minuteHand;
        [SerializeField] Vector3 axis = Vector3.forward;
        [SerializeField] AudioSource tick;
        [SerializeField] int startHour = 3, startMinute = 9;

        int hour, minute;

        void Awake() => SetTime(startHour, startMinute);

        public void SetTime(int h, int m)
        {
            hour = h;
            minute = m;
            minuteHand.localRotation = Quaternion.AngleAxis(m * 6f, axis);
            hourHand.localRotation = Quaternion.AngleAxis((h % 12) * 30f + m * 0.5f, axis);
        }

        public void Tick(int toHour, int toMinute, float seconds) => StartCoroutine(TickRoutine(toHour, toMinute, seconds));

        IEnumerator TickRoutine(int toHour, int toMinute, float seconds)
        {
            int steps = (toHour * 60 + toMinute) - (hour * 60 + minute);
            if (steps <= 0) yield break;
            float wait = seconds / steps;
            for (int i = 0; i < steps; i++)
            {
                yield return new WaitForSeconds(wait);
                int total = hour * 60 + minute + 1;
                SetTime(total / 60, total % 60);
                if (tick != null) tick.Play();
            }
        }
    }
}
