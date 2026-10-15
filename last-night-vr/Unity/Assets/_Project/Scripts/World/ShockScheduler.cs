using System.Collections;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using UnityEngine;

namespace LastNight.World
{
    /// <summary>
    /// Chapter 6: every ~60 s the sky flashes white (a defibrillator shock). The second flash
    /// shows a single frame of the real bedroom by enabling a "real world" camera layer.
    /// </summary>
    public class ShockScheduler : MonoBehaviour
    {
        [SerializeField] float interval = 60f;
        [SerializeField] AudioSource chargeAndShock;      // SFX_defib_charge -> SFX_defib_shock
        [SerializeField] float chargeLeadTime = 2.5f;
        [SerializeField] Camera playerCamera;
        [SerializeField] LayerMask realWorldLayer;
        [SerializeField] string[] lineSets = { "CH6_005,CH6_006", "CH6_007" };

        int shocks;

        // Start disabled in the scene; enabling the component starts the shocks, disabling stops them.
        void OnEnable() => StartCoroutine(Loop());
        void OnDisable() => StopAllCoroutines();

        IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(interval - chargeLeadTime);
                if (chargeAndShock != null) chargeAndShock.Play();
                yield return new WaitForSeconds(chargeLeadTime);
                yield return Shock();
            }
        }

        IEnumerator Shock()
        {
            shocks++;
            BeatClock.Instance?.Jolt(1f, 0.25f);

            if (playerCamera == null) playerCamera = Camera.main;
            if (shocks == 2 && playerCamera != null)
            {
                int oldMask = playerCamera.cullingMask;
                playerCamera.cullingMask = realWorldLayer;
                yield return null; // exactly one frame of the real room
                playerCamera.cullingMask = oldMask;
            }
            yield return ScreenFader.Flash(Color.white, 0.25f);

            int set = Mathf.Min(shocks - 1, lineSets.Length - 1);
            foreach (var id in lineSets[set].Split(','))
                DialoguePlayer.Instance?.PlayOnce(id.Trim());
        }
    }
}
