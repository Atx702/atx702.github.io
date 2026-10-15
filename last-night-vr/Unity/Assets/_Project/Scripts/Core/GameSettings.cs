using System;

namespace LastNight.Core
{
    public enum MovementMode { Smooth, Teleport, Both }
    public enum TurnMode { Snap, Smooth }
    public enum EyesClosedInput { Gesture, Button, Both }
    public enum Intensity { Full, Reduced }

    /// <summary>All player-facing settings. See docs/05_Menus_UI_Settings.md.</summary>
    [Serializable]
    public class GameSettings
    {
        // Comfort
        public MovementMode movement = MovementMode.Both;
        public float moveSpeed = 1.4f;
        public TurnMode turn = TurnMode.Snap;
        public int snapAngle = 30;
        public float smoothTurnSpeed = 90f;
        public int vignette = 2; // 0 off, 1 low, 2 medium, 3 high
        public bool seated;
        public bool leftHanded;
        public bool fadeCutsForForcedMoves = true;

        // Intensity and accessibility
        public Intensity intensity = Intensity.Full;
        public bool flashReduction;
        public float heartbeatHaptics = 0.7f;
        public bool subtitles = true;
        public int subtitleSize = 1; // 0 S, 1 M, 2 L, 3 XL
        public float subtitleBackground = 0.6f;
        public bool speakerNames = true;
        public EyesClosedInput eyesClosedInput = EyesClosedInput.Both;
        public bool toggleInsteadOfHold;
        public bool micHumming;

        // Audio (0..1)
        public float master = 1f, voice = 1f, music = 0.8f, ambience = 0.8f, sfx = 1f, leaks = 1f;

        // Graphics (PC VR)
        public float renderScale = 1f;

        public bool Reduced => intensity == Intensity.Reduced;
    }
}
