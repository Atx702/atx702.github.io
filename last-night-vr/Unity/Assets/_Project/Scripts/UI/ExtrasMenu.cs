using LastNight.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastNight.UI
{
    /// <summary>
    /// The photo frame on the main-menu bed: Gallery (Wren's drawings), Chapter Select,
    /// Achievements, Credits, and Wren's Easel (after any ending).
    /// Buttons/entries are spawned from prefabs into scroll views.
    /// </summary>
    public class ExtrasMenu : MonoBehaviour
    {
        [Header("Pages")]
        [SerializeField] GameObject galleryPage, chaptersPage, achievementsPage, creditsPage;
        [SerializeField] Button easelButton;

        [Header("Gallery")]
        [SerializeField] Image[] drawingSlots;            // 12 slots
        [SerializeField] Sprite[] drawings;               // 12 drawings, in collectible order
        [SerializeField] Sprite lockedSprite;
        [SerializeField] Image galleryLarge;

        [Header("Chapter select")]
        [SerializeField] Button[] chapterButtons;         // index = Chapter enum value

        [Header("Achievements")]
        [SerializeField] Transform achievementList;
        [SerializeField] AchievementRow rowPrefab;
        [SerializeField] TMP_Text achievementCount;

        [SerializeField] string easelScene = "Ch8_WrensRoom";

        void OnEnable()
        {
            var state = GameManager.Instance.State;
            if (easelButton != null) easelButton.gameObject.SetActive(state.endingsSeen.Count > 0);
            ShowPage(0);
        }

        public void ShowPage(int i)
        {
            galleryPage.SetActive(i == 0);
            chaptersPage.SetActive(i == 1);
            achievementsPage.SetActive(i == 2);
            creditsPage.SetActive(i == 3);
            if (i == 0) BuildGallery();
            if (i == 1) BuildChapters();
            if (i == 2) BuildAchievements();
        }

        void BuildGallery()
        {
            var found = GameManager.Instance.State.drawingsFound;
            for (int i = 0; i < drawingSlots.Length; i++)
            {
                bool has = found.Contains(i + 1);
                drawingSlots[i].sprite = has && i < drawings.Length ? drawings[i] : lockedSprite;
                var button = drawingSlots[i].GetComponent<Button>();
                if (button == null) continue;
                button.interactable = has;
                int index = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { galleryLarge.sprite = drawings[index]; galleryLarge.gameObject.SetActive(true); });
            }
        }

        void BuildChapters()
        {
            var state = GameManager.Instance.State;
            for (int i = 0; i < chapterButtons.Length; i++)
            {
                var chapter = (Chapter)i;
                bool reached = state.IsComplete(chapter) || state.currentChapter >= chapter || state.endingsSeen.Count > 0;
                chapterButtons[i].interactable = reached;
                chapterButtons[i].onClick.RemoveAllListeners();
                chapterButtons[i].onClick.AddListener(() => GameManager.Instance.StartChapter(chapter));
            }
        }

        void BuildAchievements()
        {
            foreach (Transform child in achievementList) Destroy(child.gameObject);
            var unlocked = GameManager.Instance.State.achievements;
            foreach (var a in Achievements.All)
                Instantiate(rowPrefab, achievementList).Show(a, unlocked.Contains(a.id));
            achievementCount.text = $"{unlocked.Count} / {Achievements.All.Length}";
        }

        /// <summary>Wren's Easel: free drawing in Wren's room, no story (after any ending).</summary>
        public void OpenEasel()
        {
            EaselMode.Active = true;
            GameManager.Instance.SwapTo(easelScene);
        }
    }
}
