using System.Collections;
using System.Collections.Generic;
using LastNight.Core;
using TMPro;
using UnityEngine;

namespace LastNight.UI
{
    /// <summary>
    /// A small sailboat card that drifts across the bottom of view when an achievement unlocks
    /// (same style as the save indicator). Queues if several unlock together.
    /// </summary>
    public class AchievementToast : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] TMP_Text label;
        [SerializeField] Transform head;
        [SerializeField] float showSeconds = 3f;

        readonly Queue<string> pending = new Queue<string>();
        bool showing;

        void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.AchievementUnlocked += Enqueue;
            group.alpha = 0f;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.AchievementUnlocked -= Enqueue;
        }

        void Enqueue(string id)
        {
            var info = Achievements.Find(id);
            pending.Enqueue(info.HasValue ? info.Value.name : id);
            if (!showing) StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            showing = true;
            while (pending.Count > 0)
            {
                label.text = pending.Dequeue();
                for (float t = 0; t < 1f; t += Time.unscaledDeltaTime * 2f) { group.alpha = t; Place(); yield return null; }
                for (float t = 0; t < showSeconds; t += Time.unscaledDeltaTime) { Place(); yield return null; }
                for (float t = 1; t > 0f; t -= Time.unscaledDeltaTime * 2f) { group.alpha = t; Place(); yield return null; }
                group.alpha = 0f;
            }
            showing = false;
        }

        void Place()
        {
            var fwd = head.forward;
            transform.position = head.position + fwd * 1.2f + head.up * -0.45f;
            transform.rotation = Quaternion.LookRotation(fwd, head.up);
        }
    }
}
