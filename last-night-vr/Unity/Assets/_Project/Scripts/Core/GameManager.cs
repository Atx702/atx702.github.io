using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastNight.Core
{
    /// <summary>
    /// Lives in Core.unity (never unloaded). Owns the GameState and Settings, loads chapter scenes
    /// additively, and is the one place other systems report progress to.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Scene names indexed by Chapter enum value. Field scene hosts chapters 0 and 6.")]
        [SerializeField] string[] chapterScenes =
        {
            "Field", "Ch1_House", "Ch2_Classroom", "Ch3_Station", "Ch4_Kitchen",
            "Ch5_Hospital", "Field", "Ch7_Bedroom", "Ch8_WrensRoom",
        };
        [SerializeField] string mainMenuScene = "MainMenu";
        [SerializeField] string secretEndingScene = "End_Morning";

        public GameState State { get; private set; }
        public GameSettings Settings { get; private set; }

        public event Action<Chapter> ChapterStarted;
        public event Action<Chapter> ChapterCompleted;
        public event Action<LeakedObject> LeakedObjectFound;
        public event Action<string> AchievementUnlocked;
        public event Action SettingsChanged;

        string loadedScene;
        bool playing;
        Chapter? lastCompleted;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            State = SaveSystem.Load();
            Settings = SaveSystem.LoadSettings();
        }

        void Update()
        {
            if (playing) State.playTimeSeconds += Time.unscaledDeltaTime;
        }

        // ---------- Flow ----------

        public void NewGame(bool lucid = false)
        {
            State = SaveSystem.NewGame(State, lucid);
            StartChapter(Chapter.Prologue);
        }

        public void Continue() => StartChapter(State.currentChapter);

        public void StartChapter(Chapter chapter)
        {
            State.currentChapter = chapter;
            SaveSystem.Save(State);
            StartCoroutine(SwapScene(chapterScenes[(int)chapter], () =>
            {
                playing = true;
                ChapterStarted?.Invoke(chapter);
            }));
        }

        public void CompleteChapter(Chapter chapter)
        {
            if (State.AddUnique(State.completedChapters, chapter))
                ChapterCompleted?.Invoke(chapter);
            lastCompleted = chapter;

            // Chapters 1-5 return to the field; the field opens Convergence once all five are done.
            Chapter next = chapter switch
            {
                Chapter.Prologue => Chapter.Prologue, // the field stays loaded; player walks to an anchor
                Chapter.Convergence => Chapter.Bedroom,
                Chapter.Bedroom => Chapter.LittleLantern,
                _ when chapter <= Chapter.LongHall => State.AllMemoryAnchorsComplete ? Chapter.Convergence : Chapter.Prologue,
                _ => chapter,
            };
            SaveSystem.Save(State);
            if (next != chapter) StartChapter(next);
        }

        public void ReturnToMainMenu()
        {
            playing = false;
            EaselMode.Active = false;
            SaveSystem.Save(State);
            StartCoroutine(SwapScene(mainMenuScene, null));
        }

        public void FinishGame(Ending ending)
        {
            playing = false;
            State.AddUnique(State.endingsSeen, ending);
            Unlock(ending switch
            {
                Ending.Stay => "STAY",
                Ending.Wake => "WAKE",
                Ending.MarensWords => "MARENS_WORDS",
                _ => null,
            });
            if (State.lucidMode) Unlock("LUCID");
            if (State.pullCatches == 0) Unlock("NEVER_CAUGHT");
            SaveSystem.Save(State);
        }

        public void LoadSecretEnding() => StartCoroutine(SwapScene(secretEndingScene, null));

        /// <summary>Swap to any scene (used by endings). Runs on the GameManager, so it survives the caller's scene unloading.</summary>
        public void SwapTo(string sceneName) => StartCoroutine(SwapScene(sceneName, null));

        public void ReturnToMainMenuAfter(float seconds) => StartCoroutine(MenuAfter(seconds));

        IEnumerator MenuAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            ReturnToMainMenu();
        }

        // ---------- Progress reporting ----------

        public void ReportLeakedObject(LeakedObject obj)
        {
            if (!State.AddUnique(State.leakedObjects, obj)) return;
            LeakedObjectFound?.Invoke(obj);
            if (State.HasAllLeakedObjects) Unlock("SOMETHING_DOESNT_BELONG");
            SaveSystem.Save(State);
        }

        public void ReportDrawing(int index)
        {
            if (!State.AddUnique(State.drawingsFound, index)) return;
            if (State.drawingsFound.Count >= GameState.DrawingCount) Unlock("THE_NIGHTSTAND");
            SaveSystem.Save(State);
        }

        public void ReportPullCatch() => State.pullCatches++;

        public void SetFlag(string flag)
        {
            State.SetFlag(flag);
            SaveSystem.Save(State);
        }

        public void Unlock(string achievement)
        {
            if (string.IsNullOrEmpty(achievement)) return;
            if (!State.AddUnique(State.achievements, achievement)) return;
            AchievementUnlocked?.Invoke(achievement);
            // Platform hook: call Oculus/Steam achievements here.
        }

        public void ApplySettings(GameSettings settings)
        {
            Settings = settings;
            SaveSystem.SaveSettings(settings);
            SettingsChanged?.Invoke();
        }

        // ---------- Scene loading ----------

        IEnumerator SwapScene(string sceneName, Action onLoaded)
        {
            yield return ScreenFader.FadeOut(0.6f);
            if (!string.IsNullOrEmpty(loadedScene) && SceneManager.GetSceneByName(loadedScene).isLoaded)
                yield return SceneManager.UnloadSceneAsync(loadedScene);

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            loadedScene = sceneName;

            PlayerSpawnPoint spawn = null;
            foreach (var s in FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None))
            {
                if (lastCompleted.HasValue && s.IsReturnPointFor(lastCompleted.Value)) { spawn = s; break; }
                if (s.IsDefault && spawn == null) spawn = s;
            }
            if (spawn != null)
            {
                spawn.SetAsCheckpoint();
                spawn.MovePlayerHere();
            }
            lastCompleted = null;

            onLoaded?.Invoke();
            yield return ScreenFader.FadeIn(0.8f);
        }
    }
}
