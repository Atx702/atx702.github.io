using System.IO;
using UnityEngine;

namespace LastNight.Core
{
    /// <summary>Saves GameState and Settings as JSON in Application.persistentDataPath.</summary>
    public static class SaveSystem
    {
        const string SaveFile = "lastnight_save.json";
        const string SettingsFile = "lastnight_settings.json";

        static string PathFor(string file) => Path.Combine(Application.persistentDataPath, file);

        public static bool HasSave => File.Exists(PathFor(SaveFile));

        public static void Save(GameState state) => Write(SaveFile, JsonUtility.ToJson(state, true));
        public static GameState Load() => Read<GameState>(SaveFile) ?? new GameState();

        public static void SaveSettings(GameSettings settings) => Write(SettingsFile, JsonUtility.ToJson(settings, true));
        public static GameSettings LoadSettings() => Read<GameSettings>(SettingsFile) ?? new GameSettings();

        /// <summary>New Game keeps endings, achievements and Lucid Mode; everything else resets.</summary>
        public static GameState NewGame(GameState previous, bool lucid)
        {
            var fresh = new GameState { lucidMode = lucid };
            if (previous != null)
            {
                fresh.endingsSeen.AddRange(previous.endingsSeen);
                fresh.achievements.AddRange(previous.achievements);
                fresh.drawingsFound.AddRange(previous.drawingsFound); // gallery stays unlocked
            }
            Save(fresh);
            return fresh;
        }

        static void Write(string file, string json)
        {
            var path = PathFor(file);
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        static T Read<T>(string file) where T : class
        {
            var path = PathFor(file);
            if (!File.Exists(path)) return null;
            try
            {
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not read {file}: {e.Message}");
                return null;
            }
        }
    }
}
