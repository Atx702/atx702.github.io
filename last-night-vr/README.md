# LAST NIGHT — VR Psychological Horror

A man dies in his sleep and walks through a dream built from his life. He thinks he's escaping.
He's actually refusing to be saved: the thing hunting him is his wife, doing CPR on the bedroom floor.

- **Engine:** Unity 6 (URP) + XR Interaction Toolkit 3 + OpenXR
- **Target:** Meta Quest 3 (standalone), PC VR stretch
- **Length:** 65–75 minutes, 9 chapters, 3 endings
- **Tools:** Meshy (props/characters), Blender (cleanup, rigging), ElevenLabs (voices, SFX), Unity

## What's in this folder

| Path | What it is |
| --- | --- |
| `docs/01_Story_Bible.md` | Characters, backstory timeline, themes, the twist layer by layer, endings |
| `docs/02_Script.md` | **Every line of dialogue** (212 lines), scene by scene, with stage directions and triggers |
| `docs/03_Level_Design.md` | Map layout, each chapter's spaces, puzzles, timings, pacing chart |
| `docs/04_Cutscenes_Animations.md` | All 14 cutscenes, every character animation clip, object animations, lip-sync |
| `docs/05_Menus_UI_Settings.md` | Boot flow, diegetic main menu, pause menu, all settings, subtitles |
| `docs/06_Easter_Eggs_Secrets.md` | Collectibles, 12 easter eggs, Lucid Mode (NG+), 20 achievements |
| `docs/07_Art_Assets_Meshy.md` | Every asset with its Meshy prompt, triangle budgets, color rules, shaders |
| `docs/08_Audio_ElevenLabs.md` | Voice casting with ElevenLabs voice IDs, SFX list, music, mixer setup |
| `tools/extract_lines.py` | Turns the script into `dialogue.csv` (run it after every script edit) |
| `tools/procgen/` | Builds the models Meshy didn't (props, characters, The Pull) as GLB + OBJ |
| `tools/audio_manifest.json` | Prompt, length and loop setting for every sound effect and music cue |
| `Unity/Assets/_Project/` | Unity scripts and data. Copy this folder into your Unity project's `Assets/`. |

## Status

| Area | Status |
| --- | --- |
| Story, script, dialogue | ✅ Written (v1) |
| Level design, cutscenes, menus, secrets | ✅ Designed (v1) |
| Art list + Meshy prompts | ✅ Written |
| 3D models | ✅ 25 from Meshy (Field, House, Classroom, Station) + 30 built in code (`tools/procgen`: rest of the props, all characters, Tomas, The Pull). Rigging and animation still to do. |
| Voice casting + recording | ✅ All 11 roles cast; all 212 lines recorded and in `Audio/Resources/VO` |
| Sound effects + music | ✅ 30 SFX in `Audio/SFX`, 6 music cues in `Audio/Music` (ElevenLabs) |
| Core systems (save, scenes, beat, dialogue, subtitles) | ✅ Code written |
| Player mechanics (eyes closed, gaze, gestures, mic humming) | ✅ Code written |
| The Pull AI (sighting, stalk, chase, circle, doorway) | ✅ Code written |
| Chapter directors (all 9 chapters + field + secret ending) | ✅ Code written |
| Puzzles (boat sockets, speakers, chalk, timetable, kettle, radio, lamps, corridor loop, clock) | ✅ Code written |
| Menus (boot, bed menu, settings, pause, extras, gallery, achievements, credits, easel) | ✅ Code written; the panels themselves still need laying out in Unity |
| Easter eggs and achievements | ✅ Code written |
| **Compile check** | 🟡 All scripts parse cleanly (Roslyn syntax check). Not yet compiled against Unity, so expect a handful of small API fixes on first import. |
| Scenes, models, animation | 🔴 To do (needs the Unity editor, Meshy, Blender) |

## Script map

| Folder | Scripts |
| --- | --- |
| `Core/` | GameManager, GameState, GameSettings, SaveSystem, ScreenFader, PlayerSpawnPoint, Bootstrap, Achievements, EaselMode |
| `Audio/` | BeatClock (110 BPM compression rhythm, haptics) |
| `Dialogue/` | DialogueDatabase (CSV), DialoguePlayer (queue, leak filter), SubtitleView |
| `Player/` | EyesClosed, Gaze, GestureDetector |
| `Pull/` | PullController |
| `Interaction/` | SocketSet, Dial, PullChainLamp, GrabSignal, HumDetector, CrayonPaper, Crayon |
| `Chapters/` | ChapterDirector (base), FieldDirector, Ch1–Ch6 directors, SecretEndingDirector, SpeakerBox, ChalkWord, KitchenKettle, CorridorLoop, CorridorLights, HospitalClock, DriftingFragment, WrensRoomToys |
| `World/` | PrologueDirector, RevealDirector (Ch.7), EndingController (Ch.8), ShockScheduler, DialogueTrigger, ShyObject, MemoryDecay, LeakedObjectItem, DrawingCollectible, AnchorPortal, ChapterExit, IdleWatcher, StoppedClockEgg, ThrowWatcher |
| `UI/` | BootFlow, MainMenuItem, SettingsMenu, SettingsApplier, PauseMenu, ExtrasMenu, AchievementRow, AchievementToast, CreditsRoll |

Directors expose small public methods (`OnExit()`, `OnHandRaised()`, …). Wire them from trigger volumes and `UnityEvent`s in each scene; the field list at the top of each director says what to drag in.

## Unity setup (do this once)

1. **Install Unity 6** (6000.0 LTS or newer) with **Android Build Support** (includes OpenJDK, Android SDK and NDK).
2. **New project** → template **"3D (URP)"** → name it `LastNight`.
3. **Packages** (Window → Package Manager → Unity Registry). Install:
   - XR Plugin Management
   - OpenXR Plugin
   - XR Interaction Toolkit (3.x). In its Samples tab, import **Starter Assets** and **Hands Interaction Demo**.
   - XR Hands
   - Input System (say **Yes** to restart and enable the new input backend)
   - Timeline
   - AI Navigation (for the Pull's NavMesh)
   - TextMeshPro (import TMP Essentials when asked)
4. **XR settings** (Edit → Project Settings → XR Plug-in Management):
   - Android tab: tick **OpenXR**. Under OpenXR → Android: add **Meta Quest Support** feature and the **Oculus Touch Controller Profile** (plus **Hand Tracking Subsystem** for hands).
   - Windows tab (PC VR): tick **OpenXR**, add the Oculus Touch and Valve Index profiles.
   - Fix everything in **Project Validation** (the yellow warnings).
5. **Build settings:** File → Build Profiles → Android → Switch Platform. Texture compression **ASTC**. In Player settings: Minimum API Level 32, Scripting Backend **IL2CPP**, Target architecture **ARM64**, Color space **Linear**.
6. **Quality (URP asset):** Render scale 1.0, MSAA 4×, HDR off, shadows: 1 cascade, 30 m distance. Turn on **Foveated rendering** in the OpenXR Meta Quest feature.
7. **Copy code in:** copy `Unity/Assets/_Project` into your project's `Assets/` folder. Wait for it to compile and fix anything the console reports (this code has not been compiled yet; expect a few small fixes).
8. **Tags and layers:** add tag `Player`; add layer `RealWorld` (for the Chapter 6/7 real-room glimpses).
9. **Audio mixer:** create `Assets/_Project/Audio/MainMixer` with groups `Voice`, `Memory`, `Leak`, `World`, `Music`, `SFX`. On `Leak` add Lowpass (expose cutoff as `LeakLowpass`) and Reverb. On `World` add Lowpass (expose as `WorldLowpass`).

## Building the Core scene

Create `Assets/_Project/Scenes/Core.unity` (always loaded) containing:

| Object | Components | Notes |
| --- | --- | --- |
| `XR Origin (XR Rig)` | from XRI Starter Assets | Add a capsule trigger collider tagged `Player` on the camera. Enable both teleport and continuous move providers. |
| `Main Camera` (child) | ScreenFader, EyesClosed, SubtitleView canvas child | Fader and eyelid are small inverted spheres with unlit transparent black materials (render queue 5000). |
| `GameManager` | GameManager | Fill in scene names. |
| `Audio` | BeatClock, DialoguePlayer | DialoguePlayer needs a 2D AudioSource (Voice), a 3D AudioSource (World), the mixer groups, and the DialogueDatabase asset. |
| `IdleWatcher` | IdleWatcher | Assign the field center. |

Then: **Create → Last Night → Dialogue Database**, and drag `Data/dialogue.csv` into its CSV slot.

Add all scenes to Build Profiles in this order: `Core`, `MainMenu`, `Field`, `Ch1_House`, `Ch2_Classroom`, `Ch3_Station`, `Ch4_Kitchen`, `Ch5_Hospital`, `Ch7_Bedroom`, `Ch8_WrensRoom`, `End_Morning`. Put the `Bootstrap` component in Core; it loads `MainMenu` additively on start.

## Script execution order
Project Settings → Script Execution Order: `GameManager` (-100), `BeatClock` (-90), `DialoguePlayer` (-90), `EyesClosed` (-80). Everything else default. This makes sure the singletons exist before `SubtitleView` and the Pull subscribe to them.

## Voice lines

```bash
python tools/extract_lines.py
```

All 212 lines are already recorded and saved as `Unity/Assets/_Project/Audio/Resources/VO/<LINE_ID>.mp3`; the game loads them by ID. The command above regenerates `Data/dialogue.csv` after script edits. To re-record a line, see `docs/08_Audio_ElevenLabs.md`. A line with no audio file still works: it shows subtitles for a reading-time duration.

## Roadmap

| Milestone | What gets built | Done when |
| --- | --- | --- |
| **M1 Graybox** | Core scene, Field with primitive anchors, Prologue playable with placeholder VO | You can walk from the bed to the rise and pick up the boat in a headset |
| **M2 Mechanics** | Eyes closed, shy house, the Pull (stalk mode), save/load | Ch.1 and the Pull work in graybox |
| **M3 All chapters graybox** | Directors for Ch.1–5, Ch.6–8 hooked up | Whole game playable start to finish in graybox (~65 min) |
| **M4 Voice + sound** ✅ | All 212 lines, 30 sound effects and 6 music cues made in ElevenLabs | Done |
| **M5 Art** | Meshy assets cleaned in Blender, characters rigged, shaders | Every chapter dressed |
| **M6 Cutscenes + animation** | All 14 timelines, character animation, lip-sync | All story beats staged |
| **M7 Menus + settings** | Bed menu, pause, settings, extras, achievements | Every setting works |
| **M8 Polish + performance** | Audio mix, comfort pass, 72 fps on Quest 3 | Five outside playtesters finish without nausea |
