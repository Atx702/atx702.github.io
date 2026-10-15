# LAST NIGHT — Quest 2 Build

How the game is set up for Meta Quest 2, what the one-click tools do, and the budgets each scene has to fit.

## Target hardware

| | Quest 2 |
| --- | --- |
| Chip | Snapdragon XR2 (Adreno 650) |
| Refresh | 72 Hz (13.9 ms per frame) |
| Per-eye resolution | 1832 × 1920 |
| Frame budget we aim for | under 11 ms GPU, under 9 ms CPU |
| Triangles on screen | under 300,000 (Meta's ceiling is about 750,000; we leave room) |
| Draw calls | under 120 after batching |
| Lights touching any object | main light + 2 point lights |

The same APK runs on Quest 3, 3S and Pro; they just have more headroom.

## What "1. Set Up Quest 2 Project" changes

`Unity/Assets/_Project/Editor/QuestSetup.cs`. Safe to run any number of times.

| Area | Setting |
| --- | --- |
| Platform | Android, ASTC textures |
| Player | IL2CPP, ARM64 only, min API 29, Vulkan, linear colour, landscape, no splash, minimal stripping |
| Package name | `com.atx702.lastnight`, version 0.1.0 |
| XR | OpenXR loader for Android **and** Windows (so Quest Link works in Play mode); single pass instanced; 16-bit depth submission (lets the headset reproject smoothly) |
| OpenXR features | Meta Quest Support (Quest 2 is in its target device list by default), Oculus Touch controller profile |
| Graphics | `Settings/LastNight_Quest2_URP.asset`: 4× MSAA, no HDR, no realtime shadows, no depth or opaque texture, SRP batcher, 2 per-object lights |
| Quality | every level uses that asset; vsync off (the headset paces frames), no shadows, 2 pixel lights |
| Input | both backends (XRI uses the Input System); may need one editor restart |
| Layers | `RealWorld` (the one-frame glimpse during Ch.6 shocks) |
| Script order | GameManager, PlayerRig, BeatClock, DialoguePlayer, EyesClosed run first, so everything that subscribes to them finds them |

## Import rules (automatic)

`Editor/QuestImportRules.cs` applies to everything under `Assets/_Project`:
- **Textures:** mipmaps on, capped at 1024 px (drawings 512), ASTC 6×6 on Android. Meshy's 2048 px textures are cut to 1024; only the base colour maps end up in the build.
- **Voice lines:** Vorbis, compressed in memory, mono.
- **Music and ambience loops:** streamed from disk.
- **Short sound effects:** decompressed on load, mono.
- **Models:** no animation, cameras or lights; medium mesh compression; not readable.

## Rendering choices

- One custom lit shader (`LastNight/World`) for every surface: main light + ambient + fog, no shadows. It also does the memory decay: colour drains below `_Decay` 0.5, the surface dissolves above it.
- `LastNight/Overlay` for fades and closed eyes (two tiny spheres around the camera, drawn last).
- `LastNight/PullBody` for the Pull and the anchor beacons: additive, ignores fog, so they cut through it.
- Fog is the main tool for both mood and performance: every scene's camera far plane sits just past its fog, so anything beyond it is never drawn.
- Static batching on everything that doesn't move. Anything that is grabbed, shuffled or animated is kept dynamic (`Kit.Dynamic`), otherwise it would stay frozen in its batched position.

## What "2. Build All Scenes" makes

All scenes are generated from code in `Editor/Scenes/`. Change the code and rebuild; never edit the generated scenes by hand (they're overwritten).

| Scene | Builder | Contents |
| --- | --- | --- |
| Core | `CoreBuilder` | XRI Starter rig, player body collider, fader and eyelids, subtitles, achievement toasts, pause card, settings panel, boot cards, game systems, audio |
| MainMenu | `MainMenuBuilder` | The bed at dusk; pillow (Continue), alarm clock (New Game), lantern lamp (Settings), photo (Extras), door (Quit), lantern (Lucid Mode) |
| Field | `FieldBuilder` | Prologue bed, boards and rise; five anchors with beacons and return points; field events and easter eggs; Chapter 6 fragments, lamp, shocks |
| Ch1_House | `Ch1Builder` | The shy house (3 rooms), boat sockets, the lake and dock, CS_01, the glove, the exit door |
| Ch2_Classroom | `Ch2Builder` | 40 × 60 m classroom, 30 desks, 3 speakers over 8 spots, chalk speech, memory layers, pulse clip |
| Ch3_Station | `Ch3Builder` | 80 m platform, 4 hidden tiles, the train and young Maren (CS_03), the gust and scarf, the bench |
| Ch4_Kitchen | `Ch4Builder` | The kitchen in the void, kettle, mugs, honey, radio dial, 3 pull-chain lamps, CS_04A/B, the ring |
| Ch5_Hospital | `Ch5Builder` | 3-module corridor loop with the 5 story drawings, coffee, room 7 (CS_05), the bracelet, 150 m chase |
| Ch7_Bedroom | `Ch7Builder` | The bedroom with the floor window; the real room 3 m below with CPR (CS_07); the Pull in the doorway; the closet |
| Ch8_WrensRoom | `Ch8Builder` | Wren, the lantern, the duck, crayons and paper, the bed, the window; CS_08, STAY and WAKE |
| End_Morning | `EndingsBuilder` | The hospital room two days later (secret ending) |
| End_MorningField | `EndingsBuilder` | The field at morning, the sailboat on the pond, the credits |

## Checking the code without Unity

```bash
python tools/unity_compile_check.py            # compiles everything the way the editor does
python tools/unity_compile_check.py --player   # the Android/Quest build
python tools/check_wiring.py                   # every field name the scene builders set exists
```

The compile check needs the Unity 6000.0 editor's managed libraries and the package sources (see the top of the script for paths). It compiles each assembly definition in dependency order with Unity's defines, just like the editor.

## Before shipping (not done yet)

- First run in the headset; fix positions, model orientation and anything the Console reports.
- Profile each chapter with the OVR Metrics Tool (target: steady 72 fps, GPU level 3 or lower).
- Fixed foveated rendering (OpenXR Meta Quest feature) once the scenes are final.
- Rig the characters (Mixamo) and replace the gliding with walk, CPR and hold animations.
- An Oculus/Meta store build needs a signed release keystore (Player Settings > Publishing Settings).
