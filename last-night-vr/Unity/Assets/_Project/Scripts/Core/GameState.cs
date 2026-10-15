using System;
using System.Collections.Generic;

namespace LastNight.Core
{
    public enum Chapter
    {
        Prologue = 0,
        TooFar = 1,
        RollCall = 2,
        Line4 = 3,
        YellowKitchen = 4,
        LongHall = 5,
        Convergence = 6,
        Bedroom = 7,
        LittleLantern = 8,
    }

    public enum Ending { None, Stay, Wake, MarensWords }

    public enum LeakedObject { Glove = 0, PulseClip = 1, Scarf = 2, Ring = 3, Bracelet = 4 }

    /// <summary>Everything that persists between sessions. Serialized to JSON by SaveSystem.</summary>
    [Serializable]
    public class GameState
    {
        public const int LeakedObjectCount = 5;
        public const int DrawingCount = 12;

        public Chapter currentChapter = Chapter.Prologue;
        public List<Chapter> completedChapters = new List<Chapter>();
        public List<LeakedObject> leakedObjects = new List<LeakedObject>();
        public List<int> drawingsFound = new List<int>();
        public List<string> flags = new List<string>();
        public List<string> achievements = new List<string>();
        public List<Ending> endingsSeen = new List<Ending>();
        public bool lucidMode;
        public int pullCatches;
        public float playTimeSeconds;

        public bool IsComplete(Chapter c) => completedChapters.Contains(c);

        public bool AllMemoryAnchorsComplete =>
            IsComplete(Chapter.TooFar) && IsComplete(Chapter.RollCall) && IsComplete(Chapter.Line4) &&
            IsComplete(Chapter.YellowKitchen) && IsComplete(Chapter.LongHall);

        public bool HasAllLeakedObjects => leakedObjects.Count >= LeakedObjectCount;

        public bool HasFlag(string flag) => flags.Contains(flag);

        public void SetFlag(string flag)
        {
            if (!flags.Contains(flag)) flags.Add(flag);
        }

        public bool AddUnique<T>(List<T> list, T value)
        {
            if (list.Contains(value)) return false;
            list.Add(value);
            return true;
        }
    }

    /// <summary>Well-known flag names, so typos don't silently break triggers.</summary>
    public static class Flags
    {
        public const string ChoseCoffee = "chose_coffee";
        public const string WalkedPastCoffee = "walked_past_coffee";
        public const string FoundBiscuit = "found_biscuit";
        public const string ReadReceipt = "read_receipt";
        public const string Ch4NoEyesClosed = "ch4_no_eyes_closed";
        public const string SangToWren = "sang_to_wren";
        public const string SawReveal = "saw_reveal";
    }
}
