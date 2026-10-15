# LAST NIGHT — Cutscenes and Animations

## VR cutscene rules
1. **Never take the camera away.** The player always controls head rotation. "Cutscenes" are staged around the player.
2. Forced camera movement (the lake descent, the wake gasp) uses a slow move **or** a fade cut, chosen by the comfort setting.
3. Every cutscene can be skipped on a second playthrough by holding both grips for 2 seconds (a ring fills around the wrist).
4. All cutscenes are Unity **Timeline** assets in `Assets/_Project/Timelines/`, driven by `CutsceneDirector.cs`.

## Cutscene list

| ID | Name | Length | Chapter | What happens | Timeline tracks |
| --- | --- | --- | --- | --- | --- |
| CS_00 | Flatline | 40 s | Prologue | Black; heartbeat stutters and stops; breath out; fade into the field. | Audio, Post-processing (fade), Activation |
| CS_01 | The Lake | 75 s | 1 | Boat drifts, water rises past the eyes, underwater quiet, hand pulls you out, Elio's rescue, freeze, decay to dust. | Camera height, Water volume, Audio, Animation (Elio), Decay shader |
| CS_02 | Chalk | 20 s | 2 | Speech words appear, chalk crumbles, one-frame drawing. | Animation, Material, Audio |
| CS_03 | Train | 45 s | 3 | Train arrives with no tracks, Maren steps out, the gust. | Animation (train, Maren), Wind zone, Audio |
| CS_04A | Third Mug | 30 s | 4 | Maren's news. Mug drop / honey spread. | Animation, Decal, Audio |
| CS_04B | It Found Us | 10 s | 4 | Lights flicker, Pull fills the doorway. | Lights, Pull animation, Audio |
| CS_05 | Room 7 | 50 s | 5 | Clock ticks to 3:12, door closes, flatline, nurse lines. | Animation (door), Clock, Audio |
| CS_06 | Shock | 3 s | 6 | White flash, single real-world frame. | Post, Render layer swap, Haptics |
| CS_07 | The Floor | 3 min | 7 | Real-world scene through the floor, Maren's lines, paramedics, Pull in doorway. | Animation (4 characters), Audio (unmuffled), Pull |
| CS_08 | The Drawing | 25 s | 8 | Wren holds up her drawing. | Animation (Wren), Audio |
| CS_END_STAY | Gold | 90 s | End | Room turns gold, lantern stops, fog through window, flatline to music box. | Post, Lights, Audio |
| CS_END_WAKE | Gasp | 60 s | End | White-out, gasp, real ceiling, Okafor, Maren, drawing on carpet. | Post, Animation, Audio |
| CS_SECRET | Morning | 3 min | Secret | Hospital room, Maren's monologue, ring, field at morning. | Animation (Maren), Audio, Scene load |
| CS_CREDITS | Credits | 2 min | End | Credits drift past as paper drawings in the empty field. | UI, Audio |

## Character animation list

All humans use one shared humanoid rig (Mixamo-compatible bone names) so animations retarget.

### Maren
| Clip | Use | Source |
| --- | --- | --- |
| `maren_cpr_loop` | Chest compressions at 110 BPM, kneeling | Mocap or hand-keyed in Blender (keep elbows locked, hips over hands) |
| `maren_cpr_count_breath` | Pause, head down, breath, back to compressions | Hand-keyed |
| `maren_handoff` | Paramedic takes over; she falls back, hands over mouth | Hand-keyed |
| `maren_mem_platform_step` | Young Maren steps off the train | Mixamo "Walking" + edit |
| `maren_mem_catch_laugh` | Laughs, hand to chest | Mixamo "Laughing" |
| `maren_mem_wave_train` | Waves from the train window | Mixamo "Waving" |
| `maren_mem_kitchen_lean` | Leans on the counter, hand on belly | Hand-keyed |
| `maren_chair_sit_talk` | Secret ending: sits, leans forward, holds hand | Mixamo "Sitting Talking" |

### Wren
| Clip | Use |
| --- | --- |
| `wren_draw_loop` | Sitting cross-legged, drawing |
| `wren_look_up_smile` | Looks up when the player enters |
| `wren_hold_up_drawing` | Holds up the drawing with both hands |
| `wren_arms_up` | Asks to be picked up |
| `wren_held_idle` | Small breathing/fidgeting while held (attach to player's hands) |
| `wren_wave_goodbye` | Small wave from the bed |

### Others
| Character | Clips |
| --- | --- |
| Elio | `elio_lake_rescue_compressions`, `elio_lake_relief_hug` |
| Okafor | `okafor_take_over_cpr`, `okafor_paddles_charge`, `okafor_check_pulse`, `okafor_lean_over_player` |
| Lind | `lind_carry_monitor_enter`, `lind_bag_valve` |
| Nurse | `nurse_hallway_approach`, `nurse_hand_on_shoulder` |
| Ms. Adair | `adair_rollcall_idle` (silhouette only) |

## Non-character animations

| Object | Animation | Notes |
| --- | --- | --- |
| The Pull | `pull_step` (one step per beat), `pull_freeze`, `pull_reach`, `pull_fill_doorway` | Driven by `PullController` on the beat, not by Animator time. Vertex shimmer shader on each beat. |
| Player hands | Grip, point, pinch, cover-eyes pose, raise-hand pose, hold-child pose | XR Hands / XRI hand poses |
| House (Ch1) | Retreat/approach slide | Script-driven |
| Boat | Float bob, sink into ground | Shader + transform |
| Chalk words | Write-on | Shader reveal by UV.x |
| Train | Arrive/depart with no tracks, slight sway | Timeline |
| Kettle | Steam particles, whistle shake | Particle + transform |
| Pull-chain lamps | Flicker, die | Light intensity curve |
| Corridor lights | Sequential failure behind the player | Script |
| Anchor fragments (Ch6) | Drift and tilt toward center | Script with noise |
| Wren's lantern | Rotate, star projection | Cookie texture on a spot light |
| Memory decay | Desaturate → flatten → particle dust | `MemoryDecay.cs` + shader |
| Hospital clock | Tick 3:10 → 3:12 | Only clock that moves in the game |

## Facial animation
- Characters are seen at a distance or in silhouette most of the time. Only Maren (CS_07, CS_SECRET) and Wren (Ch.8) get faces up close.
- Use ElevenLabs audio + Oculus Lipsync (or uLipSync, free) to drive visemes.
- Eyes: blink every 3–6 s, look-at constraint on the player's head.
