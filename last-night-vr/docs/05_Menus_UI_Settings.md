# LAST NIGHT — Menus, UI and Settings

## Philosophy
No floating HUD during play. Everything is **diegetic** (part of the world) except subtitles and the pause menu.

## Boot sequence
1. Unity splash (black, 2 s).
2. Health and safety card: "This game deals with death, grief and the loss of a child. It contains flashing lights (can be reduced in Settings)." Press any trigger.
3. Height calibration (first launch only): "Stand or sit comfortably and press A." Sets floor offset.
4. Main menu.

## Main menu: "The Bed"
The player sits on the bed from the prologue, in the field at dusk. Fog. The 3:12 clock on the nightstand. Objects on and around the bed are the menu items. Point and pull the trigger, or pick them up.

| Object | Action | Note |
| --- | --- | --- |
| Pillow | **Continue** | Only present if there is a save. Lie back (fade) to load. |
| Alarm clock | **New Game** | Twist the dial. Confirms if a save exists ("This will wake you somewhere else."). |
| Bedside lamp | **Settings** | Turn it on to open the settings panel (world-space canvas on the lampshade's light). |
| Photo frame | **Extras** | Gallery, chapter select, achievements, credits. |
| Door in the grass | **Quit** | Opening it shows black. Confirm. |
| Lantern (after first ending) | **Lucid Mode** (New Game+) | |

Music: the Little Lantern melody, slow, on detuned piano, with wind.

## Pause menu
Press Menu button. The world desaturates and slows; a small paper card floats in front of the player at chest height.

- Resume
- Settings
- Restart from last anchor
- Chapter select (only chapters already reached)
- Quit to bed (main menu)

## Settings

### Comfort
| Setting | Options | Default |
| --- | --- | --- |
| Movement | Smooth / Teleport / Both | Both |
| Movement speed | 1.0–2.5 m/s | 1.4 |
| Turning | Snap / Smooth | Snap |
| Snap angle | 15 / 30 / 45 / 90° | 30 |
| Smooth turn speed | 30–180 °/s | 90 |
| Vignette while moving | Off / Low / Medium / High | Medium |
| Play mode | Standing / Seated | Standing |
| Dominant hand | Left / Right | Right |
| Forced camera moves | Smooth / Fade cuts | Fade cuts |
| Recenter view | Button | — |
| Height recalibrate | Button | — |

### Intensity and accessibility
| Setting | Options | Default |
| --- | --- | --- |
| Intensity | Full / Reduced | Full |
| Flash reduction | Off / On | Off |
| Heartbeat haptics strength | 0–100% | 70% |
| Subtitles | Off / On | On |
| Subtitle size | S / M / L / XL | M |
| Subtitle background | 0–100% | 60% |
| Speaker names in subtitles | Off / On | On |
| Eyes-closed input | Hands-over-eyes gesture / Button hold / Both | Both |
| Hold vs toggle | Hold / Toggle (for grab and eyes-closed) | Hold |
| Microphone humming | Off / On | Off |

"Reduced" intensity: the Pull's catch radius halves, flashes become fades, thump volume drops 50%, the chase in Chapter 5 is 30% slower.

### Audio
Master, Voice, Music, Ambience, SFX, Leaks (0–100%). "Leaks" is separate so players can turn the muffled real-world voices up if they can't hear them.

### Graphics (PC VR only)
Render scale (0.7–1.5), MSAA (off/2×/4×), fog quality (low/high), shadow quality, film grain (on/off).

### Language
English (voice + subtitles). Subtitles planned: Spanish, French, German, Italian, Portuguese (BR), Japanese.

## Subtitles
- World-locked to the speaker's position when the speaker is visible; head-locked below center otherwise.
- Speaker colors: Voice = warm white, Maren = soft red, Wren = yellow, others = light gray. Leaks are italic.
- Max 2 lines, 42 characters per line.

## In-world UI elements
| Element | Look |
| --- | --- |
| Save indicator | A small sailboat icon drifts across the bottom of view for 2 s at each anchor entrance. |
| Chapter title cards | Large handwritten text appears on a sheet of paper that floats past at the start of each chapter. |
| Interaction hint | Objects glow faintly at the edges when the hand is within 15 cm. No button prompts after the prologue. |
| Leaked object | Has a faint cold blue rim light instead of the warm memory glow. |

## Extras
- **Gallery:** Wren's 12 drawings (unlock by finding), concept art, the Meshy-to-final model viewer.
- **Chapter select:** chapters reached.
- **Achievements:** see `06_Easter_Eggs_Secrets.md`.
- **Credits.**
- **Wren's Easel:** a free-draw mode with crayons in Wren's room (unlocked after any ending). Drawings can be saved as PNG.
