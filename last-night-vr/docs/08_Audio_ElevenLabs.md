# LAST NIGHT — Audio and ElevenLabs Production

## Voice casting (ElevenLabs)

| Character | Voice (ElevenLabs library) | Voice ID | Direction |
| --- | --- | --- | --- |
| The Voice | Yogi – Calm & Meditative | `OKqOM06abBsLyb8k3fXw` | Close, warm, unhurried. Always sounds a little too sure. |
| Maren (tonight + memories) | Heather Faith Cubed | `U7QdEsfRT6SXMGfRQbyN` | Intimate, restrained, breaks under pressure. Memory lines brighter and younger. |
| Wren | Tilly – Bright, Spirited, Naive | `MkTSSXNgnBULS6ek4pon` | Sweet, soft, natural child voice. |
| Tomas | Eric – Smooth, Trustworthy | `cjVigY5qzO86Huf0OWal` | Quiet, few lines. Different from the Voice but same age. |
| Elio | Ivan (Latin American accent) | `IP2syKL31S2JthzSSfZH` | Gruff, desperate. |
| Lucia | Carli – Calm, Warm and Expressive | `7DTpboyigwqXiCkvQjMX` | Calling from far away. |
| Ms. Adair | Marilyn – Warm, Clear British | `CH8NA3Iu30hBBcNIzv9I` | Patient teacher. |
| Nurse | Faye – Warm, Confident & Comforting | `qY71f9HwmIHc4kJb8RLH` | Gentle, tired, kind. |
| Dispatcher | Kathryn – Public Announce | `iGjiBXtLd3IyXUaAQk6M` | Calm, professional 911 operator. |
| Okafor | Dean – steady American | `QZA1myOcnbLYY9uuhixV` | Steady, confident male paramedic. |
| Lind | Isabelle – Articulate & Trustworthy | `GxGSne3qX4QsbtuEIkbJ` | Brisk female paramedic. |

Model: `eleven_v4` (supports direction tags like `[whispering]`).

## Recorded

**All 212 lines are recorded** (one take each) and saved in `Unity/Assets/_Project/Audio/Resources/VO/<LINE_ID>.mp3` (MP3, 128 kbps, 44.1 kHz mono, about 17 MB total). `DialogueDatabase` loads them by line ID, so no wiring is needed.

Every take also lives in the ElevenLabs flow "Last Night - voice samples" (`4xeFnJuRsauJwGI4FStd`), where you can re-roll any line you don't like.

## Re-recording a line
1. Edit the line in `docs/02_Script.md`, then run `python tools/extract_lines.py` and `python tools/build_vo_prompts.py`.
2. Generate it in ElevenLabs with the prompt and voice from `tools/vo_prompts.json`.
3. Replace its row in `tools/vo_sessions.txt` with the new session ID, delete the old `.mp3`, and run `python tools/fetch_vo.py <saved status json>` (links expire about 2 hours after the status call).
4. Leak lines are recorded clean. Unity applies the leak filter at runtime, so the same file plays clear in Ch.7.

## Audio processing (Unity mixer)

| Group | Effects |
| --- | --- |
| VO_Voice | Slight proximity warmth (low shelf +2 dB at 150 Hz), no reverb, head-locked (2D) |
| VO_Memory | Spatial 3D, room reverb matching the space |
| VO_Leak | Low-pass at 900 Hz, high-pass 200 Hz, reverb 2.5 s, random pan drift. In Ch.7 the low-pass opens to 20 kHz over 4 s. |
| Thump | Sub-bass kick at 110 BPM, sidechain-ducks the ambience by 3 dB |
| Ambience | Wind beds per chapter |
| Music | Sparse; only the Lantern melody and drones |

## Sound effects and music: generated

**All 30 sound effects and 6 music cues are generated** in ElevenLabs and saved in the Unity project:
- `Unity/Assets/_Project/Audio/SFX/<ID>.mp3` (sound effects model `eleven_text_to_sound_v2`; ambiences and hums are made as seamless loops)
- `Unity/Assets/_Project/Audio/Music/<ID>.mp3` (music model `eleven_music_v2_5`; all instrumental except MUS_Lantern_Hum, which hums the Little Lantern lyrics)

Every prompt, length and loop setting is in `tools/audio_manifest.json`. All takes live in the ElevenLabs flow `0llPTruv4Ziz3CBXrk8G`, where you can re-roll any of them.

To regenerate one sound: change its prompt in the manifest, generate it on the flow, replace its row in `tools/audio_sessions.txt` with the new session ID, delete the old `.mp3`, and run `python tools/fetch_audio.py <saved status json>` straight away (links expire after about 2 hours).

In Unity, set looped files (wind, cicadas, rain, hum, underwater, static, drones) to **Load Type: Streaming** and tick **Loop** on their AudioSource. Short one-shots work best as **Decompress On Load**.

## SFX list

| ID | Sound | Notes |
| --- | --- | --- |
| SFX_heartbeat_stutter | Heartbeat that stutters and stops | CS_00 |
| SFX_breath_out | Long exhale | CS_00 |
| SFX_thump | Dull chest compression thump, muffled through a body | Core loop, 110 BPM |
| SFX_flatline | ECG flatline tone | Ch.5, STAY ending |
| SFX_monitor_beep | Heart monitor beep | Ch.2 speakers |
| SFX_defib_charge | Rising defibrillator whine | Ch.6 |
| SFX_defib_shock | Sharp thud + crackle | Ch.6, catches |
| SFX_gasp | Ragged gasp for air | WAKE ending |
| SFX_wind_field | Soft wind across grass, loopable | Field |
| SFX_cicadas | Summer cicadas, distant | Ch.1 |
| SFX_wind_chimes | Metal chimes | Ch.1 |
| SFX_underwater | Muffled underwater ambience | Ch.1 lake |
| SFX_rain_windows | Rain on high windows | Ch.2 |
| SFX_chalk | Chalk on board | Ch.2 |
| SFX_flip_tile | Split-flap tile clack | Ch.3 |
| SFX_train_arrive | Train arriving and doors opening | Ch.3 |
| SFX_kettle_whistle | Kettle boiling to a whistle | Ch.4 |
| SFX_radio_static | Radio static, tunable | Ch.4 |
| SFX_honey_drop | Glass jar breaking softly, sticky | Ch.4 |
| SFX_fluorescent_hum | Hospital fluorescent hum | Ch.5 |
| SFX_vending_coffee | Coffee machine pouring | Ch.5 |
| SFX_door_close_soft | Hospital door closing softly | Ch.5 |
| SFX_light_fail | Fluorescent tube dying with a click | Ch.5 chase |
| SFX_pull_step | Low, airy whoomp, like a breath in a big room | Pull, per beat |
| SFX_pull_near | Rising tone of pressure, like ears popping | Pull within 5 m |
| SFX_lantern_turn | Soft music-box mechanism | Ch.8 |
| SFX_duck_squeak | Rubber duck squeak | Ch.8 |
| SFX_memory_decay | Sand pouring + soft crackle | Every anchor |
| SFX_footsteps_grass | Slow footsteps on soft grass | Field, the Pull's approach |
| SFX_reversed_voice | Reversed, breathy whisper | Ch.6 distortion, easter eggs |

## Music
| Cue | Use |
| --- | --- |
| MUS_Lantern_Piano | Main menu; Ch.4 radio |
| MUS_Lantern_MusicBox | STAY ending, credits |
| MUS_Lantern_Hum | Wren and Tomas humming (Ch.8) |
| MUS_Drone_Field | Field, very low |
| MUS_Drone_Chase | Ch.5 chase, synced to 110 BPM |
| MUS_Morning | Secret ending, the only major-key cue |

"Little Lantern" melody (original, 8 bars, in C): **E E G E | D C D E | E E G A | G E D . | C D E G | A G E D | E D C D | C . . .**
Lyrics: "Little lantern, little light / keep me company tonight / when the dark gets big and wide / little lantern, stay inside."
