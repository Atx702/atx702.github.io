using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastNight.Core
{
    /// <summary>
    /// Lives in Core.unity. Loads the main menu on start. In the editor, if you press Play with a
    /// chapter scene open alongside Core, it stays in that chapter instead (handy for testing).
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        void Start()
        {
            if (SceneManager.sceneCount > 1) return; // a chapter is already open (editor testing)
            // Goes through GameManager so it tracks the menu scene and unloads it later.
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
