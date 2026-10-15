# LAST NIGHT — Audio and ElevenLabs Production

## Voice casting (ElevenLabs)

| Character | Voice (ElevenLabs library) | Voice ID | Direction |
| --- | --- | --- | --- |
| The Voice | Yogi – Calm & Meditative | `OKqOM06abBsLyb8k3fXw` | Close, warm, unhurried. Always sounds a little too sure. |
| Maren (tonight + memories) | Heather Faith Cubed | `U7QdEsfRT6SXMGfRQbyN` | Intimate, restrained, breaks under pressure. Memory lines brighter and younger. |
| Wren | *not cast yet* | — | Sweet, soft, natural child voice. Sample generation was declined; pick a voice before Chapter 5 work. |
| Tomas | *to cast* | — | Quiet, few lines. Different from the Voice but same age. |
| Elio | *to cast* | — | Gruff, Spanish accent, desperate. |
| Lucia | *to cast* | — | Spanish, calling from far away. |
| Ms. Adair | *to cast* | — | Patient teacher, 50s. |
| Nurse | *to cast* | — | Gentle, tired, kind. |
| Dispatcher | *to cast* | — | Calm, professional 911 operator. |
| Okafor | *to cast* | — | Steady, confident male paramedic. |
| Lind | *to cast* | — | Brisk female paramedic. |

Model: `eleven_v4` (supports direction tags like `[whispering]`). Export **WAV 44.1 kHz mono**.

Sample takes already generated (flow "Last Night - voice samples" in ElevenLabs):
- The Voice: opening line (4 takes)
- The Voice: "Don't look at it… close your eyes" (4 takes)
- Maren: "Tomas, please. Come back." (4 takes)

## Batch workflow
1. Run `python tools/extract_lines.py` to regenerate `Unity/Assets/_Project/Data/dialogue.csv` from the script.
2. Generate each line in ElevenLabs with the character's voice. Name the file exactly `<LINE_ID>.wav` (for example `PRO_001.wav`).
3. Put files in `Unity/Assets/_Project/Audio/Resources/VO/`. The `DialogueDatabase` loads them by ID automatically.
4. Leak lines: export clean. Unity applies the leak filter at runtime so the same file can be clear in Ch.7.

## Audio processing (Unity mixer)

| Group | Effects |
| --- | --- |
| VO_Voice | Slight proximity warmth (low shelf +2 dB at 150 Hz), no reverb, head-locked (2D) |
| VO_Memory | Spatial 3D, room reverb matching the space |
| VO_Leak | Low-pass at 900 Hz, high-pass 200 Hz, reverb 2.5 s, random pan drift. In Ch.7 the low-pass opens to 20 kHz over 4 s. |
| Thump | Sub-bass kick at 110 BPM, sidechain-ducks the ambience by 3 dB |
| Ambience | Wind beds per chapter |
| Music | Sparse; only the Lantern melody and drones |

## SFX list (ElevenLabs sound effects or recorded)

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
