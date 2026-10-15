# LAST NIGHT — Level Design

Each chapter is a separate Unity scene, loaded additively on top of `Core.unity` (player rig, audio, UI, game state).
Distances are in meters. Times are for a first-time player who isn't rushing.

## Reference screenshots → how we use them

These are **mood references only**. Never trace, copy or rebuild them.

| Screenshot | What we take | Where |
| --- | --- | --- |
| Rooftop with towers in fog | Huge scale, near-white fog, structures that fade at 80–150 m | The Field, Convergence |
| Looking up at lattice towers and tilted buildings | Disorienting tilted architecture overhead | Convergence (anchors drifting and tilting) |
| Night forest with a glowing doorway shape | A tall bright shape in the dark | The Pull's look, especially in the Hospital chase |
| Small fog/tree thumbnails | Soft silhouettes, low contrast | Edges of every anchor |

Our look differs on purpose: **soft desaturated color** (not 1-bit dither), one accent color per anchor, domestic and organic shapes rather than brutalist concrete.

---

## Global layout: The Field

```
                         [2 Classroom]
                               |
          [1 House]            |            [3 Station]
                 \             |            /
                  \     (6 Bedroom lamp)   /
                   \     [8 Wren's light] /
                    \          |         /
                     [Prologue bed + rise]
                    /                     \
          [5 Hospital]                [4 Kitchen]
```

- Field radius: 600 m, flat with gentle swells. Fog density tuned so anchors are silhouettes from the rise.
- Anchors sit 180–250 m from the center. Walking between anchors takes 60–90 s at normal speed; that's deliberate quiet time.
- **Order:** chapters 1–5 are open order, but the script is written for 1→5. The `ChapterGate` system unlocks 6 only when 1–5 are complete. If the player goes out of order, the Voice's leak reactions use the "first/second/third leak" variants (the dialogue system picks by count, not by chapter).
- **Field events between anchors** (each plays once, triggered by distance walked):
  - A single door standing in the grass. Opening it shows more field. (Voice: SYS_IDLE_002 if lingered.)
  - A coat rack with Tomas's coat. Pockets hold a coffee receipt dated 2020 (easter egg).
  - Wind carries the sound of the Lantern tune for 3 seconds.
  - After Ch.3: hospital curtains on rails hang in the open field, swaying.

---

## Chapter 0: Prologue (6 min)

| Beat | Space | Teaches | Line IDs |
| --- | --- | --- | --- |
| Wake | Bed in the field, 2 × 2 m | Look | PRO_001–002 |
| Hands | Same | Hand tracking, ring | PRO_003–005 |
| Boards path | 40 m of floorboards | Movement / comfort | PRO_006–007 |
| The rise | Small hill, 360° view | Anchors visible | PRO_008–010, TOMAS_001 |
| Toy boat | Grass by path | Grab and release | PRO_011–012 |
| Go | — | Free roam | PRO_013–014 |

Settings check: the comfort menu pops up automatically on the bed before the boards (see menus doc).

---

## Chapter 1: Too Far (9 min)

- **Approach (2 min):** The house sits at 220 m but retreats 1 m per second of direct gaze (raycast from head, 15° cone). Looking away, walking backwards, or eyes-closed brings it forward at 3 m/s. Lucia calls once.
- **House interior (4 min):** 3 rooms, 6 × 5 m each. Boat parts: hull under table, mast on bedroom shelf, sail drawing on fridge (two-hand fold interaction).
- **Lake (3 min):** Dock 12 m. Boat placed on water → scripted descent (camera lowers 0.3 m/s; comfort mode uses fade cuts instead). Underwater 20 s, then Elio's rescue, then decay.
- **Leak:** Glove on dock end. Pull sighting on leaving: 2 s, horizon, no audio.

## Chapter 2: Roll Call (8 min)

- **Room:** 40 × 60 m, desks in fog. Player desk near the board.
- **Speaker hunt (5 min):** 3 speakers at randomized spots from 8 candidate points. Each emits the beep with 3D audio (HRTF). Each switch press restores a "memory layer" (lights, desks, a paper plane).
- **Roll call + speech (2 min):** Raise-hand gesture (either hand above head height for 1 s). Words reveal on gaze.
- **Leak:** Pulse clip on desk. Pull appears at 35 m, advances one row (2 m) each time it's out of view.

## Chapter 3: Line 4 (9 min)

- **Platform:** 80 m long, 8 m wide, drop into fog on one side.
- **Tiles (3 min):** 4 flip tiles hidden: in the vending machine tray, under the bench, on top of the shelter (reach up), in a lost-property bin.
- **Scarf catch (1 min):** Gust physics; slow-motion assist (0.4× time) after one miss.
- **Train chase (1 min):** Train moves at 2.5 m/s; teleport players get guaranteed catch points.
- **Bench (3 min):** The Voice's confession. No interaction needed; very slow lighting change.
- **Leak + Pull (1 min):** Real scarf on bench. Pull under the clock at 70 m. Thump bed starts here and never stops.

## Chapter 4: Yellow Kitchen (9 min)

- **Kitchen:** 5 × 4 m, floating in black void. Doorways fade to black after 1 m.
- **Tea (3 min):** Kettle fill (tap), stove dial, two mugs, honey (pour; drop → spread decal).
- **Radio (2 min):** Dial 0–100. Leak stations at 23 and 61. Their song at 88.
- **Stealth (3 min):** 3 pull-chain lamps. Each fails at random every 15–25 s. Pull path: doorway → table → stove → doorway loop at 0.7 m per beat. Catch radius 0.8 m. Eyes-closed makes the Pull lose target after 2 beats.
- **Leak:** Ring on the table; the hand model's ring disappears.

## Chapter 5: The Long Hall (11 min)

- **Corridor:** 6 × 3 m modules, 25 m long, looped. Drawings at 0.9 m height.
- **Drawing order (4 min):** 5 drawings in a loop of 3 corridor modules; the order of drawings changes each loop. The correct sequence (house → lantern → boat → bird → door 7) advances the corridor to a new module.
- **Coffee (1 min):** Choice is tracked (`chose_coffee`) for the Voice line and for one line variant in the secret ending.
- **Room 7 (2 min):** Scripted door close. Player can knock (no response) and try the handle (locked).
- **Leak:** Bracelet outside room 7 (big readable text on a world-space canvas when held to the face).
- **Chase (3 min):** 150 m straight corridor with 6 chair alcoves. Lights fail behind the player every 2 beats. Pull speed: 1 m per beat (about 1.8 m/s); can be outwalked at normal speed only if the player doesn't stop.

## Chapter 6: Convergence (5 min)

- **Space:** The field's center, 120 m radius, with fragments of anchors 1–5 drifting inward and tilting (up to 35°). Path: 9 fragments as stepping platforms (kitchen floor, train car roof, a row of desks, the dock, a hospital gurney).
- **Shock flashes:** every 60 s, 0.25 s white-out (reduced to a soft fade in low-intensity mode). Flash 2 shows a single frame of the real room.
- **Pull:** circles the center at 25 m, staying between the player and the lamp; the player must go around it.

## Chapter 7: 3:12 AM (6 min)

- **Bedroom:** 4 × 5 m. Floor center has a "window" mesh showing the real-world scene below (separate render layer, slow motion).
- **Interactive cutscene:** the player can walk, look, kneel. Leaving is blocked by the Pull standing in the doorway (it doesn't move here).
- **Exit:** the closet door opens after CH7_014.

## Chapter 8: Little Lantern (5 min)

- **Wren's room:** 3.5 × 4 m. The only fog-free space.
- **Interactables:** lantern (rotates, light stars), duck (squeak), 4 drawings, crayons (can draw on paper, see menus doc), bed (sit).
- **Choice triggers:**
  - STAY: grab Wren (two-hand lift) and sit on the bed for 3 s, OR hold Wren for 20 s.
  - WAKE: set Wren down after holding her, OR walk to within 1 m of the window, after hearing CH8_009.
- No timer. The game waits as long as the player needs.

---

## Pacing chart

| Minute | Chapter | Intensity (0–5) |
| --- | --- | --- |
| 0–6 | Prologue | 0 |
| 6–15 | Too Far | 1 → 2 (lake) |
| 15–23 | Roll Call | 2 |
| 23–32 | Line 4 | 1 → 3 (confession) |
| 32–41 | Yellow Kitchen | 1 → 4 (stealth) |
| 41–52 | The Long Hall | 3 → 5 (chase) |
| 52–57 | Convergence | 4 |
| 57–63 | 3:12 AM | 5 (emotional, not scary) |
| 63–68 | Little Lantern | 1 |
| 68–74 | Ending | — |

Field walks between chapters add about 5 minutes total of quiet time.
