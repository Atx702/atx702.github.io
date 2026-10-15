using LastNight.Core;
using UnityEngine;

namespace LastNight.UI
{
    /// <summary>
    /// Credits as paper drawings drifting past in the empty field (CS_CREDITS). Each page is a
    /// world-space quad; they float from far ahead, pass the player, and fade.
    /// </summary>
    public class CreditsRoll : MonoBehaviour
    {
        [SerializeField] Transform[] pages;
        [SerializeField] Transform head;
        [SerializeField] float speed = 0.9f;
        [SerializeField] float spacing = 6f;
        [SerializeField] float startDistance = 25f;
        [SerializeField] bool returnToMenuWhenDone = true;

        Vector3 forward;
        float travelled;

        void OnEnable()
        {
            if (head == null && Camera.main != null) head = Camera.main.transform;
            forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].position = head.position + forward * (startDistance + i * spacing) + Vector3.up * Random.Range(-0.3f, 0.4f);
                pages[i].rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
            }
            travelled = 0f;
        }

        void Update()
        {
            float step = speed * Time.deltaTime;
            travelled += step;
            foreach (var p in pages) p.position -= forward * step;

            if (travelled > startDistance + pages.Length * spacing + 3f)
            {
                enabled = false;
                if (returnToMenuWhenDone) GameManager.Instance?.ReturnToMainMenu();
            }
        }
    }
}
