# LAST NIGHT — VR Psychological Horror

A man dies in his sleep and walks through a dream built from his life. He thinks he's escaping.
He's actually refusing to be saved: the thing hunting him is his wife, doing CPR on the bedroom floor.

- **Engine:** Unity 6 (URP) + XR Interaction Toolkit 3 + OpenXR
- **Target:** Meta Quest 2 (standalone Android). Also runs on Quest 3/3S/Pro, and on PC through Quest Link.
- **Length:** 65–75 minutes, 9 chapters, 3 endings
- **Tools:** Meshy (props/characters), Blender (cleanup, rigging), ElevenLabs (voices, SFX), Unity

## Get it running on your Quest 2

You need a Windows or Mac computer, a USB-C cable and about 15 GB free.

**1. One-time setup**
1. Install **Unity Hub**, then in Hub > Installs install **Unity 6000.0 LTS** (6000.0.84f1 or any newer 6000.0) with the **Android Build Support** module ticked, including **OpenJDK** and **Android SDK & NDK Tools**.
2. Put your Quest 2 in **developer mode**: in the Meta Horizon phone app, go to Devices > your headset > Headset settings > Developer mode, and turn it on. (It asks you to make a free developer organisation the first time.)
3. Plug the headset into the computer and choose **Allow USB debugging** (tick "Always allow") inside the headset.

**2. Open the project**
1. In Unity Hub: **Add > Add project from disk**, and pick the `last-night-vr/Unity` folder.
2. Open it. The first open takes a while (it downloads packages and imports about 600 files). If Unity asks to enable the new input system backend, say **Yes**.

**3. Build the game (one menu item)**
1. In Unity's menu bar: **Last Night > Do Everything (1, 2, 3)**.
   - Step 1 sets up Android, OpenXR, Quest 2 support and the graphics settings. If Unity says it must restart for the input system, let it, then run **Do Everything** again.
   - Step 2 builds all 12 scenes from code (about a minute).
   - Step 3 builds `Builds/LastNight.apk` (the first build takes 10-20 minutes, later ones are faster).
2. To install straight onto the headset instead, use **Last Night > 3b. Build and Run on Quest 2** with it plugged in.
3. On the Quest, the game is under **Library > Unknown Sources > Last Night**.

**Testing without building:** with Quest Link (or Air Link) running, open **Last Night > Open Core + Field**, then press Play in Unity. The game runs in the headset straight from the editor.

The menu items can be run one at a time too: **1. Set Up Quest 2 Project**, **2. Build All Scenes**, **3. Build APK**. Rebuilding scenes is always safe: they are generated from the code in `Unity/Assets/_Project/Editor/Scenes/`.

## Controls (Quest 2 Touch controllers)

| Action | Input |
| --- | --- |
| Move | Left stick (smooth) or right stick forward (teleport) |
| Turn | Right stick left/right (snap by default) |
| Grab | Grip |
| Use / squeeze / press | Trigger while holding |
| Close your eyes | Cover your eyes with both hands, or hold **B** |
| Pause | Menu button (left controller) |
| Raise your hand, hold out your hands, wave | Do it: the game watches your controllers |

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
| `docs/09_Quest2_Build.md` | How the Quest 2 build is set up, performance budgets, and what each scene builder makes |
| `Unity/` | **The Unity project.** Open this folder in Unity Hub. |
| `Unity/Assets/_Project/Scripts/` | Game code (runtime) |
| `Unity/Assets/_Project/Editor/` | The one-click setup, scene builders and import rules |
| `tools/unity_compile_check.py` | Compiles all the C# against real Unity 6 libraries without opening Unity |
| `tools/check_wiring.py` | Checks every field name the scene builders wire up |
| `tools/procgen/` | Builds the models Meshy didn't (props, characters, The Pull) as GLB + OBJ |
| `tools/make_drawings.py` | Draws Wren's 12 crayon drawings |
| `tools/extract_lines.py` | Turns the script into `dialogue.csv` (run it after every script edit) |
| `tools/audio_manifest.json` | Prompt, length and loop setting for every sound effect and music cue |

## Status

| Area | Status |
| --- | --- |
| Story, script, dialogue | ✅ Written (v1) |
| Level design, cutscenes, menus, secrets | ✅ Designed (v1) |
| Voice | ✅ All 11 roles cast; all 212 lines recorded |
| Sound effects + music | ✅ 30 SFX, 6 music cues |
| 3D models | ✅ 25 from Meshy + 30 built in code (characters are static meshes: they glide, they don't walk) |
| Game code (systems, Pull AI, all chapter directors, puzzles, menus, easter eggs) | ✅ Written |
| **Compiles against Unity 6** | ✅ Every script compiles against the real Unity 6000.0.84f1 engine and package libraries, for both the editor and the Android/Quest build (`tools/unity_compile_check.py`) |
| Scenes | ✅ All 12 generated from code by **Last Night > Build All Scenes** |
| Cutscenes | ✅ 10 Timeline cutscenes generated from code |
| Quest 2 project setup and APK build | ✅ One menu item |
| **Tested in a headset** | 🔴 Not yet. Nothing here has been run inside Unity or on a Quest, so expect a round of fixes on the first run (see below) |
| Character rigging and animation | 🔴 To do (Mixamo auto-rig works on the A-pose characters) |
| Extras menu (gallery, chapter select, achievements list) | 🟡 Credits page only; the code for the rest exists |

## First run: what to expect

This was built without access to the Unity editor, so the first run is the real test. Things most likely to need a tweak:
- **Model orientation and size.** Meshy models are scaled to real-world sizes automatically, but some may face the wrong way. Fix the yaw for that model in its scene builder and rebuild the scenes.
- **Shaders.** The three custom shaders (`Shaders/`) haven't been through Unity's shader compiler. If something renders pink, the Console names the shader and the line.
- **Positions.** Triggers, sockets and spawn points are placed from the level design measurements. If one is hard to reach, adjust it in the builder and rebuild.

When something is off, copy the Console error (or describe what you see) and it can be fixed in the builder code.

## Roadmap

| Milestone | What gets built | Status |
| --- | --- | --- |
| M1 Graybox | Core scene, Field, Prologue | ✅ Generated |
| M2 Mechanics | Eyes closed, shy house, the Pull, save/load | ✅ Code + scenes |
| M3 All chapters | Directors for Ch.1-8, all scenes | ✅ Generated |
| M4 Voice + sound | 212 lines, 30 SFX, 6 music cues | ✅ Done |
| M5 Art | Meshy + procedural models in every chapter | ✅ First pass |
| M6 Cutscenes + animation | Timelines (done), character rigging and animation (to do) | 🟡 |
| M7 Menus + settings | Bed menu, pause, settings, boot | ✅ (Extras partial) |
| M8 Headset test + polish | First run on Quest 2, fixes, comfort pass, 72 fps | 🔴 Next |
