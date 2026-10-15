using LastNight.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastNight.UI
{
    /// <summary>One line in the achievements list. Hidden achievements show "???" until unlocked.</summary>
    public class AchievementRow : MonoBehaviour
    {
        [SerializeField] TMP_Text title, description;
        [SerializeField] Image icon;
        [SerializeField] Color lockedColor = new Color(1, 1, 1, 0.35f);

        public void Show(Achievements.Info info, bool unlocked)
        {
            bool reveal = unlocked || !info.hidden;
            title.text = reveal ? info.name : "???";
            description.text = reveal ? info.description : "Keep walking.";
            var c = unlocked ? Color.white : lockedColor;
            title.color = c;
            description.color = c;
            if (icon != null) icon.color = c;
        }
    }
}
