using System.Collections;
using LastNight.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastNight.UI
{
    /// <summary>
    /// First thing the player sees: the content warning card, then height calibration on first
    /// launch, then the main menu. Replaces Bootstrap in Core.unity once the UI exists.
    /// </summary>
    public class BootFlow : MonoBehaviour
    {
        [SerializeField] GameObject warningCard;    // "This game deals with death, grief and the loss of a child..."
        [SerializeField] GameObject calibrationCard; // "Stand or sit comfortably and press A."
        [SerializeField] InputActionReference confirm;
        [SerializeField] SettingsApplier applier;

        IEnumerator Start()
        {
            // Editor testing: a chapter scene opened next to Core plays straight away.
            if (UnityEngine.SceneManagement.SceneManager.sceneCount > 1) yield break;
            confirm.action.Enable();

            warningCard.SetActive(true);
            yield return new WaitForSeconds(1.5f); // don't let a held button skip it
            yield return WaitForPress();
            warningCard.SetActive(false);

            if (!PlayerPrefs.HasKey("calibrated"))
            {
                calibrationCard.SetActive(true);
                yield return WaitForPress();
                applier?.Recenter();
                PlayerPrefs.SetInt("calibrated", 1);
                calibrationCard.SetActive(false);
            }

            GameManager.Instance.ReturnToMainMenu();
        }

        IEnumerator WaitForPress()
        {
            while (confirm.action.IsPressed()) yield return null;
            while (!confirm.action.WasPressedThisFrame()) yield return null;
        }
    }
}
