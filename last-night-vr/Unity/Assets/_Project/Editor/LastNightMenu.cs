using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LastNight.EditorTools
{
    /// <summary>
    /// The "Last Night" menu in the Unity menu bar. Run 1, 2, 3 in order on a fresh checkout:
    ///   1. Set Up Quest 2 Project   (Android, OpenXR, URP, input; may ask for one editor restart)
    ///   2. Build All Scenes          (generates every scene from code and adds them to the build)
    ///   3. Build APK / Build and Run (makes Builds/LastNight.apk, or installs it on a connected Quest 2)
    /// "Do Everything" runs all three.
    /// </summary>
    public static class LastNightMenu
    {
        public static readonly string[] SceneOrder =
        {
            "Core", "MainMenu", "Field", "Ch1_House", "Ch2_Classroom", "Ch3_Station", "Ch4_Kitchen",
            "Ch5_Hospital", "Ch7_Bedroom", "Ch8_WrensRoom", "End_Morning", "End_MorningField",
        };

        const string ApkPath = "Builds/LastNight.apk";

        [MenuItem("Last Night/1. Set Up Quest 2 Project", priority = 1)]
        public static void SetUp()
        {
            if (!QuestSetup.Apply())
            {
                EditorUtility.DisplayDialog("Last Night",
                    "Settings applied. Unity needs to restart once to switch on the new input system.\n\n" +
                    "Click OK, let Unity restart (or close and reopen the project), then run step 2.", "OK");
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                return;
            }
            EditorUtility.DisplayDialog("Last Night", "Quest 2 project settings are applied. Next: Last Night > 2. Build All Scenes.", "OK");
        }

        [MenuItem("Last Night/2. Build All Scenes", priority = 2)]
        public static void BuildScenesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var problems = BuildScenes();
            EditorUtility.DisplayDialog("Last Night", problems.Count == 0
                ? "All 12 scenes are built and in the build list.\n\nPress Play with Core open to test in the editor (Quest Link), or run step 3."
                : "Built with problems (see the Console):\n\n" + string.Join("\n", problems), "OK");
        }

        public static List<string> BuildScenes()
        {
            var problems = new List<string>();
            var builders = new (string name, Action build)[]
            {
                ("Core", CoreBuilder.Build), ("MainMenu", MainMenuBuilder.Build), ("Field", FieldBuilder.Build),
                ("Ch1_House", Ch1Builder.Build), ("Ch2_Classroom", Ch2Builder.Build), ("Ch3_Station", Ch3Builder.Build),
                ("Ch4_Kitchen", Ch4Builder.Build), ("Ch5_Hospital", Ch5Builder.Build), ("Ch7_Bedroom", Ch7Builder.Build),
                ("Ch8_WrensRoom", Ch8Builder.Build), ("End_Morning", EndingsBuilder.BuildHospitalRoom),
                ("End_MorningField", EndingsBuilder.BuildMorningField),
            };
            try
            {
                for (int i = 0; i < builders.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Last Night", $"Building {builders[i].name}", (float)i / builders.Length);
                    try
                    {
                        builders[i].build();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        problems.Add($"{builders[i].name}: {e.Message}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorBuildSettings.scenes = SceneOrder
                .Select(n => $"{Kit.ScenesFolder}/{n}.unity")
                .Where(File.Exists)
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene($"{Kit.ScenesFolder}/Core.unity");
            return problems;
        }

        [MenuItem("Last Night/3. Build APK", priority = 3)]
        public static void BuildApk() => Build(false);

        [MenuItem("Last Night/3b. Build and Run on Quest 2", priority = 4)]
        public static void BuildAndRun() => Build(true);

        [MenuItem("Last Night/Do Everything (1, 2, 3)", priority = 20)]
        public static void DoEverything()
        {
            if (!QuestSetup.Apply())
            {
                EditorUtility.DisplayDialog("Last Night", "Unity needs one restart for the input system. Reopen the project and run Do Everything again.", "OK");
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                return;
            }
            var problems = BuildScenes();
            if (problems.Count > 0)
            {
                EditorUtility.DisplayDialog("Last Night", "Scene building had problems, so the APK wasn't built:\n\n" + string.Join("\n", problems), "OK");
                return;
            }
            Build(false);
        }

        static void Build(bool run)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Last Night", "No scenes in the build yet. Run 2. Build All Scenes first.", "OK");
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = run ? BuildOptions.AutoRunPlayer : BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            if (s.result == BuildResult.Succeeded)
                EditorUtility.DisplayDialog("Last Night", $"Built {ApkPath} ({s.totalSize / (1024 * 1024)} MB) in {s.totalTime:mm\\:ss}." +
                    (run ? "\n\nIt's installing on your Quest 2. Put the headset on." : "\n\nInstall it with Meta Quest Developer Hub or: adb install -r " + ApkPath), "OK");
            else
                EditorUtility.DisplayDialog("Last Night", $"Build {s.result} with {s.totalErrors} errors. See the Console.", "OK");
        }

        /// <summary>Opens Core (the game's first scene) and presses Play: the whole game from the start, through Air Link or Link.</summary>
        [MenuItem("Last Night/Play the Game (Air Link)", priority = 30)]
        public static void PlayFromStart()
        {
            if (!System.IO.File.Exists($"{Kit.ScenesFolder}/Core.unity"))
            {
                EditorUtility.DisplayDialog("Last Night", "Build the scenes first: Last Night > 2. Build All Scenes.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene($"{Kit.ScenesFolder}/Core.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Last Night/Open Core + Field (test in editor)", priority = 40)]
        public static void OpenForTesting()
        {
            EditorSceneManager.OpenScene($"{Kit.ScenesFolder}/Core.unity", OpenSceneMode.Single);
            EditorSceneManager.OpenScene($"{Kit.ScenesFolder}/Field.unity", OpenSceneMode.Additive);
        }
    }
}
