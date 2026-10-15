# LAST NIGHT — Full Game Script

Target runtime: **65–75 minutes** for a first playthrough.

## How to read this script

Every spoken line has a unique ID, a speaker, an optional direction and the text:

```
- **PRO_001** VOICE — *(softly)* "Hello, Tomas."
```

`tools/extract_lines.py` turns every line in this file into `Unity/Assets/_Project/Data/dialogue.csv`.
That CSV feeds the Unity subtitle system and the ElevenLabs batch generator, so **keep this format exact**.

**Speakers:** `VOICE`, `TOMAS`, `MAREN` (tonight, leaking in), `MAREN_MEM` (memories), `WREN`, `ELIO`, `LUCIA`, `ADAIR`, `NURSE`, `DISPATCH`, `OKAFOR`, `LIND`.

**Trigger notes** are in *[brackets]*. "Leak" lines are processed in Unity with the `Leak` audio filter (muffled, low-passed, slight reverb, from no clear direction).

| # | Chapter | Location | Target time |
|---|---|---|---|
| 0 | Prologue: 3:12 | The Field | 6 min |
| 1 | Too Far | Childhood house | 9 min |
| 2 | Roll Call | Classroom | 8 min |
| 3 | Line 4 | Station platform | 9 min |
| 4 | Yellow Kitchen | Apartment kitchen | 9 min |
| 5 | The Long Hall | Hospital corridor | 11 min |
| 6 | Convergence | The collapsing field | 5 min |
| 7 | 3:12 AM | The bedroom (reveal) | 6 min |
| 8 | Little Lantern | Wren's room (choice) | 5 min |
| E | Endings | — | 2–6 min |

---

## CHAPTER 0 — PROLOGUE: "3:12"

**Setting:** Black. Then a bed alone in a flat gray field at dawn. Fog to the horizon. A digital clock on a nightstand reads 3:12 and never changes. In the distance, seven faint shapes: a house, a tall classroom window, a train-station roof, a lit kitchen window, a hospital sign, a bedroom lamp, and a tiny yellow light.

**Cold open (cutscene CS_00, 40 s):** Black screen. A slow heartbeat, then a stutter, then silence. One long breath out. Fade in.

- **PRO_001** VOICE — *(close, gentle)* "There you are."
- **PRO_002** VOICE — *(softly)* "Take your time. Look around. Nothing here can hurt you."

*[Tutorial: look around. When the player has looked 360°:]*

- **PRO_003** VOICE — "Hold out your hands, Tomas."

*[Player raises hands. Wedding ring visible.]*

- **PRO_004** VOICE — *(warm)* "Good. Still yours."
- **PRO_005** VOICE — "You're dreaming. That's all this is. A long dream at the end of a long night."

*[Tutorial: movement. A path of old floorboards leads off the bed into the grass.]*

- **PRO_006** VOICE — "Walk with me. Just follow the boards."
- **PRO_007** VOICE — *(if player stands still 20 s)* "It's all right. There's no hurry here. There's never any hurry here."

*[Player reaches a rise and sees all seven anchors at once.]*

- **PRO_008** VOICE — "Every place you ever lived is out there. Some of them are a little worn."
- **PRO_009** VOICE — "Walk to each one. Say goodbye properly. Then the door at the center will open."
- **TOMAS_001** TOMAS — *(barely audible)* "What door?"
- **PRO_010** VOICE — *(pause, then kindly)* "The one you've been walking toward for a long time."

*[Tutorial: grab. A small wooden toy sailboat lies in the grass.]*

- **PRO_011** VOICE — "Pick that up. You made that with your father. Remember?"
- **PRO_012** VOICE — "That's how this works. Hold something. Remember it. Let it go."

*[The player sets the boat down; it sinks into the ground like water. First, very faint thump in the distance, once. The Voice does not react.]*

- **PRO_013** VOICE — "Start wherever you like. The house is closest."

*[Optional: player looks back at the bed. It is gone.]*

- **PRO_014** VOICE — *(if player looks back)* "Don't worry about the bed. You won't need it."

---

## CHAPTER 1 — "TOO FAR" (Childhood house, age 6)

**Setting:** A small white house with a blue door on an endless lawn. Summer light, but gray. A lake glints behind it. Wind chimes. Distant cicadas.

**Mechanic introduced:** The house slides farther away while you look straight at it. It only gets closer when you look away, walk backwards, or close your eyes (hands over the headset). This teaches the eyes-closed mechanic and the rule *"things move when you don't look."*

- **CH1_001** VOICE — "That's the house on Calle Verda. You were six."
- **CH1_002** LUCIA — *(far away, calling)* "¡Tomás! ¡A cenar!"
- **CH1_003** VOICE — "Your mother. She always called you in too early."

*[House recedes. After the player notices:]*

- **CH1_004** VOICE — "Ah. It's shy. Memories are like that. Don't stare at it."
- **CH1_005** VOICE — "Look away. Or close your eyes, if you like. Cover them with your hands."

*[Player covers eyes: screen fades to black, sound becomes muffled; footsteps continue. The house jumps closer.]*

- **CH1_006** VOICE — *(pleased)* "There. See? When you close your eyes, everything gets easier."

*[Second thump, a little closer. Player reaches the porch.]*

### Inside the house
Three rooms, mostly empty: kitchen table with one bowl, his bedroom with a model sailboat shelf (one gap), and a back door to the lake.

**Puzzle:** Rebuild the toy sailboat: hull (kitchen, under the table), mast (bedroom, on the shelf), sail (a drawing pinned to the fridge, fold it). Assemble on the kitchen table. The back door unlocks.

- **CH1_007** VOICE — "Your father carved the hull from a fence post. You painted it the wrong blue."
- **CH1_008** VOICE — *(when player finds the sail drawing)* "You drew that. The sun has a face. Every sun you drew had a face."
- **CH1_009** VOICE — *(boat complete)* "There. Now take it to the water. That was the day, wasn't it?"

### The lake (memory scene, CS_01, playable)
The lake is perfectly still. The player places the boat on the water. It floats away. The player's view slowly lowers until the camera is at waterline. The water rises over the view. Underwater: quiet, blue-gray, muffled.

- **CH1_010** VOICE — *(calm, too calm)* "You went in after it. You couldn't swim yet."
- **CH1_011** VOICE — "It was very quiet down there. Do you remember how quiet?"
- **CH1_012** VOICE — "You weren't scared. Not really. It was almost peaceful."

*[A big hand plunges in and pulls. White light. Gasping. The player is on the shore; a blurred man kneels over them.]*

- **CH1_013** ELIO — *(desperate, rhythmic, pressing)* "Breathe, mijo. Breathe. Come on. Come on."
- **CH1_014** ELIO — *(breaking)* "Don't you do this to me. Breathe!"
- **CH1_015** VOICE — *(over him, gently)* "He was always so loud. You'd have been fine."

*[Player coughs. The memory freezes, then decays to gray dust.]*

- **CH1_016** VOICE — "That's one. Say goodbye to the house, Tomas."

### Leaked object #1: Latex glove
On the dock: a single blue medical glove, wet, very out of place.

- **CH1_017** VOICE — *(if picked up, slight hesitation)* "Hm. That doesn't belong to this one. Leave it."
- **CH1_018** MAREN — *(leak, muffled, under her breath)* "...okay, okay, okay..."

*[The Voice says nothing more. First appearance of the Pull: a thin white line on the far horizon, only visible for 2 seconds as the player leaves the dock. No music sting.]*

---

## CHAPTER 2 — "ROLL CALL" (Classroom, age 14)

**Setting:** A single school desk and a chalkboard in a gray room the size of a gymnasium. Rows of desks fade into fog. A big analog wall clock stopped at 3:12. Rain on high windows.

**Mechanic:** Spatial audio. A beep (the real heart monitor, leaking) bounces around the room. Hidden among the fog desks are three old speaker boxes. Find each by listening, and press its switch to silence it. Each one silenced restores part of the classroom memory.

- **CH2_001** VOICE — "Ms. Adair's class. You sat by the window so you could look at the buildings."
- **CH2_002** ADAIR — *(echoing, warm)* "Settle down, everyone. Roll call."
- **CH2_003** ADAIR — "Alvarez." *(pause)* "Bennett." *(pause)* "Cho."

*[A sharp electronic beep cuts her off. Then another. The room goes darker.]*

- **CH2_004** VOICE — *(irritated)* "That noise. It's been following you. Find it, turn it off."

*[Speaker 1 found:]*
- **CH2_005** VOICE — "One. Good. You always hated loud rooms."

*[Speaker 2 found: the desks nearby sharpen; a paper airplane drifts past.]*
- **CH2_006** VOICE — "Two. Listen. Can you hear her again?"
- **CH2_007** ADAIR — "Okafor." *(pause)* "Lind."
- **CH2_008** VOICE — *(quickly)* "Those weren't your classmates. Ignore that."

*[Speaker 3 found:]*
- **CH2_009** VOICE — "Three. Quiet now."
- **CH2_010** ADAIR — "Reyes? Tomas Reyes?"

*[Prompt: raise hand. If the player raises a hand:]*

- **CH2_011** ADAIR — "There you are. Your speech, Tomas. Come up."
- **CH2_012** VOICE — "You stuttered all the way through it. Everybody laughed."

### The speech (playable)
The player walks to the chalkboard. Words appear one at a time as the player looks at them. The Voice reads; younger Tomas's words are written in chalk.

- **CH2_013** VOICE — *(reading, slow)* "When I grow up... I want to build houses..."
- **CH2_014** VOICE — "...that keep people safe."
- **CH2_015** VOICE — *(soft)* "You built a lot of houses, Tomas. And you couldn't keep her safe in any of them."

*[First mention of "her". The chalk crumbles. The board shows a child's drawing for one frame: a yellow house with a sun that has a face.]*

- **CH2_016** VOICE — "Don't think about that yet. One thing at a time."

### Leaked object #2: Pulse-oximeter clip
On Tomas's desk: a gray plastic finger clip with a cable cut short. It beeps once when picked up.

- **CH2_017** VOICE — *(curt)* "Put that down."
- **CH2_018** DISPATCH — *(leak, phone speaker, calm)* "Is he breathing? Ma'am, is he breathing?"
- **CH2_019** MAREN — *(leak)* "No. No, he's not. I'm starting compressions."

*[Silence. Then the second Pull sighting: the white shape stands at the far end of the classroom in the fog, between desks. When looked at, it doesn't move. When the player looks away and back, it is one row closer.]*

- **CH2_020** VOICE — *(low)* "Don't look at that. Come away. Let's go somewhere warmer."

---

## CHAPTER 3 — "LINE 4" (Station platform, age 22)

**Setting:** An outdoor train platform with no tracks: just a drop into fog. One bench, a timetable board, an overhead clock, a vending machine. Late autumn. Leaves blowing upward.

**Mechanics:** Rebuild the timetable board (find 4 flip-letter tiles in the area) so the train can come. Then reenact meeting Maren: catch her dropped scarf before it blows off the edge, and run after the train to give it back.

- **CH3_001** VOICE — *(lighter, almost happy)* "Oh. This one I like."
- **CH3_002** VOICE — "Line 4. 7:40. You were late for an exam."
- **CH3_003** VOICE — "The board's broken. It always was, in your head. Fix it, and she'll come."

*[Tiles: "7", ":", "4", "0". Each tile found plays a small memory sound: a laugh, a coffee cup, a page turning, a train horn.]*

- **CH3_004** VOICE — *(tile 1)* "You noticed her before she noticed you."
- **CH3_005** VOICE — *(tile 2)* "Red scarf. Library books. Laughing at something on her phone."
- **CH3_006** VOICE — *(tile 3)* "You practiced a sentence to say to her. You never said it."
- **CH3_007** VOICE — *(tile 4)* "There. 7:40."

### The meeting (playable)
A train arrives out of the fog with no tracks. A silhouette of young Maren steps onto the platform. A gust; her red scarf blows toward the edge. The player must grab it before it falls (slow-motion assist if they miss once).

- **CH3_008** MAREN_MEM — *(laughing, surprised)* "Oh! Good catch."
- **CH3_009** MAREN_MEM — "Thank you. That's my grandmother's. She'd have killed me."
- **TOMAS_002** TOMAS — *(young, nervous)* "It's... it's a nice scarf."
- **CH3_010** MAREN_MEM — *(amused)* "It's a nice scarf? That's what you're going with?"
- **CH3_011** MAREN_MEM — "I'm Maren. You're going to miss your train, scarf guy."

*[Maren's silhouette steps onto the train before the player hands the scarf back. The doors close with the scarf still in the player's hand. The train pulls away. Prompt: run (or teleport) along the platform and hold the scarf out to the window.]*

- **CH3_012** MAREN_MEM — *(through the window, laughing)* "Keep it! You'll have to give it back to me tomorrow!"
- **CH3_013** VOICE — *(quietly)* "You gave it back to her every day for twenty years."

### The Voice admits the truth (part 1)
The platform goes dim. The bench is now the only lit thing.

- **CH3_014** VOICE — "Sit with me a minute."

*[Player sits (or stands near bench). Long pause.]*

- **CH3_015** VOICE — "You know what this is, don't you? You've known since the field."
- **CH3_016** VOICE — "You went to sleep, and your heart got tired, and it stopped."
- **CH3_017** VOICE — *(gentle)* "You died, Tomas. In your sleep. It didn't hurt."
- **CH3_018** VOICE — "This is the part where you get to see it all, once more. Then you rest."
- **TOMAS_003** TOMAS — *(quiet)* "Maren..."
- **CH3_019** VOICE — "She'll be all right. She was always the strong one. You know that."

### Leaked object #3: The red scarf (real)
On the bench, where nothing was before: the same red scarf, now old, faded, and wet with something. It smells (described by the Voice) like hospital soap.

- **CH3_020** VOICE — *(uneasy)* "That's not the same one. That one's... older."
- **CH3_021** MAREN — *(leak, close, breathless)* "Twenty-eight. Twenty-nine. Thirty."
- **CH3_022** MAREN — *(leak)* "Come on, Tom. Come on."
- **CH3_023** VOICE — "Leave it on the bench. Please."

*[The Pull is on the platform now, at the far end, under the clock. Its edges flicker with each thump. Thumps are clearly audible from here on, about 110 BPM, quiet.]*

- **CH3_024** VOICE — *(firm)* "Don't let it near you. It's the thing that erases. It takes everything you are and wipes it white."
- **CH3_025** VOICE — "If it gets close, close your eyes. It can't find you when you're not looking."

---

## CHAPTER 4 — "YELLOW KITCHEN" (Apartment, first years married)

**Setting:** One warm kitchen — yellow walls, a radio, a kettle, two mugs, a window full of city lights — floating in black. The only colorful place in the game so far. The rest of the apartment fades into darkness at the doorways.

**Mechanics:**
1. Make tea for two (fill kettle, boil, mugs, tea bags, carry to the table): a slow, cozy interaction.
2. Tune the radio (turn the dial) to find "their song". Wrong stations play leaked fragments.
3. First Pull stealth sequence: the Pull enters through the dark doorway. The player must keep the kitchen lights on (pull-chain lamps) and use eyes-closed to avoid it until the kettle whistles.

- **CH4_001** VOICE — *(content)* "The yellow kitchen. You painted it yourself. Two coats."
- **CH4_002** MAREN_MEM — *(from the dark hallway, playful)* "Is that tea for me or are you just holding the kettle?"
- **CH4_003** VOICE — "Make her tea. She takes it with too much honey."

*[Kettle filled, set on stove.]*
- **CH4_004** MAREN_MEM — "You know what I like about you, Tomas? You never rush anything."
- **CH4_005** MAREN_MEM — "Even the tea. Even me."

*[Mugs set. Radio interaction. Turning the dial:]*
- **CH4_006** MAREN_MEM — *(laughing)* "Not that station. Find ours."
- **CH4_007** DISPATCH — *(leak, wrong station 1)* "You're doing great. Push hard. Push fast. Let it come all the way back up."
- **CH4_008** LIND — *(leak, wrong station 2, distant siren)* "Two minutes out. Tell her to keep going."
- **CH4_009** VOICE — *(quickly)* "Just static. Keep turning."

*[Correct station: a soft piano version of a lullaby melody — the "Little Lantern" tune, before the player knows its name.]*

- **CH4_010** MAREN_MEM — *(soft)* "Oh, I love this one."
- **CH4_011** MAREN_MEM — *(hesitant, happy)* "Tomas. Sit down. I have to tell you something."
- **CH4_012** MAREN_MEM — *(laughing and crying a little)* "We're going to need a third mug."
- **CH4_013** VOICE — *(very quietly)* "And you dropped the honey. All over the floor. You didn't care."

*[The player's mug (if held) can be dropped. Honey spreads on the floor in gold.]*

### The Pull enters (stealth sequence, ~3 minutes)
The kitchen lights flicker. The thumps get louder. The radio warps. The Pull appears in the dark hall doorway, filling it.

- **CH4_014** VOICE — *(urgent whisper)* "It found us. Don't look at it. Keep the lights on. It doesn't like the warm places."
- **CH4_015** VOICE — "When the lights fail, close your eyes. Wait for the kettle."

*[Three pull-chain lamps. Each flickers out over time; player re-pulls them. When all are out, the player must cover their eyes until the Pull passes. If the player looks directly at the Pull while it's within 3 m, it stops, and a fragment plays:]*

- **CH4_016** MAREN — *(leak, very clear for one second)* "Tomas, look at me!"
- **CH4_017** VOICE — *(sharp)* "No. Eyes closed. Now."

*[If the Pull touches the player: white flash, a jolt in the controllers, a heavy THUMP, and the player resets to the doorway.]*

- **CH4_018** VOICE — *(after a catch, shaken)* "That was close. Too close. Did you feel that? Like being torn."
- **OKAFOR_001** OKAFOR — *(leak, under the white flash)* "Clear!"

*[The kettle whistles. The Pull retreats. The kitchen is fine again, but colder.]*

- **CH4_019** VOICE — "It's gone. It's gone. You're safe."
- **CH4_020** VOICE — "You see why we can't let it near you, don't you? It would take this away."

### Leaked object #4: Wedding ring, cold
On the table where Maren's mug was: Tomas's own wedding ring. The player looks at their hand: the ring is no longer on their finger.

- **CH4_021** VOICE — *(lying, gently)* "It slipped off. Your hands are tired. Put it back on, if you like."
- **CH4_022** MAREN — *(leak)* "I took it off, I'm sorry, I'm sorry, I needed to, your hand was—"
- **CH4_023** DISPATCH — *(leak)* "Ma'am, stay with him. Keep going."

*[Leaving the kitchen: the doorway to the hall now leads straight into the hospital corridor.]*

- **CH4_024** VOICE — *(dread)* "Not that door. Not yet."
- **CH4_025** VOICE — *(resigned)* "...All right. All right. We'll do it together."

---

## CHAPTER 5 — "THE LONG HALL" (Hospital, 2020)

**Setting:** A hospital corridor that never ends. Pale teal walls. Numbered doors, all locked. Vending machine humming. A clock reading 3:10. Wren's crayon drawings are taped along the wall at child height.

**Mechanics:**
1. The corridor loops. Follow Wren's drawings in the right order (they show a story: a house, a lantern, a boat, a bird, a door marked 7) to break the loop.
2. Find room 7. On the way, the "coffee" choice: a vending machine offers coffee. **Taking it** costs time and the corridor gets longer. **Ignoring it** makes the Voice uneasy. Either way, the memory of 3:10 AM plays.
3. The Pull chase: in the final stretch, the Pull follows on the beat. Lights go out behind the player. Hide in alcoves (eyes closed) or keep moving.

- **CH5_001** VOICE — *(flat)* "St. Alder's. Fourth floor. Pediatrics."
- **CH5_002** VOICE — "Nine days. You slept in the chair. You learned every sound the machines make."
- **CH5_003** NURSE — *(memory, distant)* "Mr. Reyes, she's resting. Go get something to eat."

*[Drawings: as the player looks at each, Wren's voice (memory) describes it. These are the only Wren lines before Chapter 8.]*

- **CH5_004** WREN — *(memory, little, proud)* "That's our house. The sun is happy."
- **CH5_005** WREN — *(memory)* "That's the lantern song, daddy. You sing it wrong."
- **CH5_006** WREN — *(memory)* "That's the boat. It's going somewhere nice."
- **CH5_007** WREN — *(memory, sleepy)* "That's a bird. It flew out the window because it was finished."
- **CH5_008** WREN — *(memory, very small)* "That's my door. Number seven. Same as me."

*[If drawings are followed in the wrong order, the corridor resets with a long hum.]*

- **CH5_009** VOICE — *(if reset)* "Again. You never could find your way in here."

### 3:10 AM: The coffee
The vending machine glows. The clock on the wall ticks to 3:10 for the first time.

- **CH5_010** VOICE — *(tight)* "You were so tired. You went for coffee. Four minutes."
- **CH5_011** VOICE — "Four minutes, Tomas."

*[If player takes the coffee:]*
- **CH5_012** VOICE — "Of course. You did it again."

*[If player walks past:]*
- **CH5_013** VOICE — *(cracking)* "It doesn't matter now. Walking past it now doesn't change anything."

### Room 7
The door is open a crack. Golden light inside. As the player reaches it, the door closes softly and a flatline tone plays from inside. The clock reads 3:12.

- **CH5_014** NURSE — *(memory, gentle)* "Mr. Reyes. I'm so sorry. She went very peacefully."
- **CH5_015** NURSE — "She asked for you. Right at the end. I told her you were coming."
- **TOMAS_004** TOMAS — *(breaking)* "I was coming."
- **CH5_016** VOICE — *(tender, cruel)* "But you weren't there. And ever since, you've been the one who wasn't there."
- **CH5_017** VOICE — "That's why you're tired. That's why it's all right to stop."

*[The door to room 7 will not open. It's the only door in the game that cannot be opened from this side.]*

### Leaked object #5: Patient ID bracelet
On the floor outside room 7: a hospital wristband. The player reads it (it's large and legible): **REYES, TOMAS. DOB 03/14/1982. ADMIT: TONIGHT. ST. ALDER'S ED.**

- **CH5_018** VOICE — *(fast)* "That's hers. That's Wren's bracelet."
- **CH5_019** VOICE — *(after the player turns it over)* "...Put it down, Tomas."
- **CH5_020** OKAFOR — *(leak, clearer than ever)* "We've got a rhythm— no. Lost it. Back on the chest."
- **CH5_021** MAREN — *(leak)* "He's forty-four. He's forty-four, he's got a daughter— had— he's—"

### The chase (~2 minutes)
Lights die in the corridor behind the player. The Pull walks out of the dark on the beat. The corridor finally opens on the far end into the field. Alcoves with chairs (the chairs he slept in) are hiding spots.

- **CH5_022** VOICE — *(frightened)* "Run. Don't look back. Don't look at it. Run."
- **CH5_023** MAREN — *(leak, inside the Pull's sound)* "Tomas. Tomas. Please."
- **CH5_024** VOICE — *(over her)* "That's not her. It uses her voice. It uses everything."

*[Player escapes into the field.]*

---

## CHAPTER 6 — "CONVERGENCE" (The collapsing field)

**Setting:** The field, but the anchors are sliding toward the center. Buildings tilt, pieces of the station, the kitchen and the corridor drift and overlap. The sky flickers white with each shock (every ~60 s). The bed's location is now marked by a tall bedroom lamp, lit. The tiny yellow light (Wren's room) sits just behind it.

**Mechanics:** Traversal through drifting fragments of all prior anchors (platforming-lite: step across floating kitchen tiles, train cars, school desks). The Pull walks in circles around the center. The thumps are loud, the field throbs with them.

- **CH6_001** VOICE — *(breathless)* "It's falling apart. It's all falling toward the center. That's fine. That's what's supposed to happen."
- **CH6_002** VOICE — "The door's right there. Go to the light. The yellow one. Not the white one."
- **CH6_003** DISPATCH — *(leak, loud now)* "The ambulance is outside. Keep going until they take over."
- **CH6_004** VOICE — *(pleading)* "Don't listen. Don't listen. Close your eyes and walk."

*[Shock flash 1:]*
- **CH6_005** OKAFOR — *(leak)* "Charging. Clear."
- **CH6_006** VOICE — *(in pain)* "Ah— That. That's what it does. Do you see?"

*[Shock flash 2: for a single frame, the player sees the real bedroom ceiling, a paramedic's face, a light.]*
- **CH6_007** VOICE — "You didn't see anything."

*[The player reaches the center. The bedroom lamp turns. The bedroom anchor opens.]*

---

## CHAPTER 7 — "3:12 AM" (The bedroom: THE REVEAL)

**Setting:** Their bedroom, exactly as it is tonight. Gray-blue night light. Clock: 3:12, blinking. The bed is empty, sheets torn back. The floor between bed and door is lit from above as if by a hole in the ceiling.

**The reveal plays as an interactive cutscene (CS_07). The player can move freely in a small space but cannot leave.**

- **CH7_001** VOICE — *(very quiet)* "Don't look down."

*[The player looks down. Through the lit floor, as if through water: their own body on the carpet below. Maren, in a T-shirt, hair everywhere, doing chest compressions. The phone on speaker beside her. Two paramedics coming through the door with a monitor. Everything is slowed to about 30% speed. Sound becomes clear for the first time: no muffling.]*

- **CH7_002** MAREN — *(counting, crying, exhausted)* "...twenty-six, twenty-seven, twenty-eight, twenty-nine, thirty."
- **CH7_003** MAREN — "Tomas Reyes, you do not get to do this."
- **CH7_004** MAREN — "You do not get to leave me too."
- **CH7_005** MAREN — *(breaking)* "I lost her too. I lost her too, and I stayed. I stayed for you. Come back."
- **CH7_006** OKAFOR — "Ma'am, we've got him. Let us take over. You did good."
- **CH7_007** MAREN — *(refusing, then letting go)* "No— okay. Okay. Thirty. He's— okay."

*[The player hears the thumps clearly now — they are her hands. The white flashes were the defibrillator. The beeping was the monitor. The Pull steps into the bedroom doorway behind the player. For the first time it is still, and quiet, and shaped exactly like Maren.]*

- **CH7_008** VOICE — *(after a long silence)* "...I didn't want you to see that."
- **TOMAS_005** TOMAS — "You knew."
- **CH7_009** VOICE — "I'm you, Tomas. Of course I knew."
- **CH7_010** VOICE — "I'm the part of you that sat in the chair outside room seven and never got up."
- **CH7_011** VOICE — "Six years I've been asking you to stop. Tonight you finally did."
- **CH7_012** VOICE — *(pleading, softer than ever)* "And she's in there. Wren's in there. Right behind that door. You can see her. You can be there this time."
- **CH7_013** VOICE — "Out there, she's gone. In here, she's waiting."
- **CH7_014** VOICE — "Don't make me let her go twice."

*[A small door opens on the far side of the bedroom, where the closet should be. Yellow light. A child's humming: the Little Lantern melody.]*

---

## CHAPTER 8 — "LITTLE LANTERN" (Wren's room: THE CHOICE)

**Setting:** Wren's bedroom, perfect. Yellow walls, a rubber duck on the shelf, crayon drawings everywhere, a night lantern turning slowly and casting stars. Wren sits on the bed, drawing. Outside the window: not the city — the white of the Pull, waiting patiently.

**This scene has no Voice lines unless the player hesitates. It is the only scene without fog.**

- **CH8_001** WREN — *(not looking up, happy)* "Daddy! You came."
- **CH8_002** WREN — "I'm drawing you. Hold still."
- **CH8_003** WREN — "You're really bad at holding still."

*[Player can sit on the bed, look at drawings, pick up toys. If the player picks up the lantern:]*

- **CH8_004** WREN — "Sing it. You promised you'd sing it right one time."

*[Optional: player can hum (microphone detection) or press to hum. The tune plays; Wren sings along off-key.]*

- **CH8_005** WREN — *(singing softly)* "Little lantern, little light, keep me company tonight..."

*[Wren finishes her drawing and holds it up: a man with a sun-face standing next to a door, and a woman on the other side of the door, waiting.]*

- **CH8_006** WREN — "That's you. And that's mommy. She's waiting for you."
- **TOMAS_006** TOMAS — *(barely)* "I wasn't there, Wren. I wasn't there."
- **CH8_007** WREN — *(simply)* "You were getting coffee, silly. That's okay."
- **CH8_008** WREN — "Mommy was there. She held my hand. She said you'd come right back."
- **CH8_009** WREN — *(a little sad)* "But you didn't come back. Not really. Not for a long time."

*[The choice: Wren raises her arms to be picked up. The window glows white behind her.]*

- **CH8_010** VOICE — *(if player hesitates > 15 s, whispering)* "Pick her up. Just pick her up, Tomas. Nobody would blame you."

### Choice A: STAY (pick Wren up, sit on the bed)
- **END_S_001** WREN — *(sleepy, content)* "Okay, daddy. You can stay."
- **END_S_002** VOICE — *(relieved, almost crying)* "There. There. No more hurry."
- **END_S_003** MAREN — *(leak, fading, very far)* "...Tomas? ...Tomas..."
- **END_S_004** OKAFOR — *(leak, flat)* "Time of death..."

*[Cutscene CS_END_STAY: the room fills with gold. The lantern stops turning. The fog comes in through the window, soft. A single, long flatline tone, which slowly becomes the Little Lantern melody on a music box. Black. Card: "3:12 AM".]*

### Choice B: WAKE (set Wren down, walk to the window)
*[If the player puts Wren down, or says goodbye, or walks toward the window:]*

- **END_W_001** WREN — *(smiling)* "Go on. Mommy's tired. She's been pushing really hard."
- **END_W_002** WREN — *(gives the player her drawing)* "Give her this."
- **END_W_003** WREN — "And sing it right, okay? For her."
- **TOMAS_007** TOMAS — *(crying)* "I love you, little lantern."
- **END_W_004** WREN — "I know. Go."
- **END_W_005** VOICE — *(defeated, quiet, kind for the first time)* "...She's right. She was always right."
- **END_W_006** VOICE — "Go on, then. I'll stay here with her. Someone should."
- **END_W_007** VOICE — "Goodbye, Tomas."

*[The player walks to the window. The Pull reaches through and takes their hand. It's warm. White-out.]*

*[Cutscene CS_END_WAKE: a ragged gasp. Real light, blurry. The ceiling of the bedroom. Paramedic Okafor's face. Then Maren's face, upside down, hair stuck to her cheeks.]*

- **END_W_008** OKAFOR — "We've got a pulse. We've got him. Tomas, can you hear me?"
- **END_W_009** MAREN — *(laughing and sobbing)* "Hi. Hi. There you are. Don't you ever—"
- **TOMAS_008** TOMAS — *(hoarse)* "...Maren. She... said to give you..."

*[The player looks at their hand. It's empty. Then the camera tilts: a crayon drawing lies on the carpet next to them, the one Wren made. (Ambiguous — was it always in the nightstand? The player never learns.) Black. Card: "3:23 AM".]*

---

## SECRET ENDING — "MAREN'S WORDS"

**Requirement:** All 5 leaked objects collected (glove, pulse clip, scarf, ring, bracelet), then choose WAKE.

*[After CS_END_WAKE, instead of credits: a hospital room, morning, two days later. The player is in the bed, with IVs and a monitor beeping a normal rhythm. The camera is free (seated). Maren sits in the chair: the chair from Chapter 5. She's wearing the red scarf.]*

- **SEC_001** MAREN — "They said you might not remember any of it. The doctor said it's normal."
- **SEC_002** MAREN — "So I'm going to tell you what I said. Because I said a lot of things on that floor."
- **SEC_003** MAREN — "I said I was angry at you. I am. For six years you were in the house and you weren't in the house."
- **SEC_004** MAREN — "I said I lost her too. I needed you to know that I lost her too."
- **SEC_005** MAREN — "And I said you don't get to leave. Not because I can't do it alone. I can. I'm good at alone now."
- **SEC_006** MAREN — *(softer)* "But I don't want to be. I want you. The slow version. The tea version."
- **SEC_007** MAREN — "You were humming. When you came back. The lantern song. You haven't hummed that since..."
- **SEC_008** MAREN — *(laughing through tears)* "You still sing it wrong."

*[Prompt: hold out your hand. If the player does, Maren takes it and puts the ring back on. The monitor beeps steadily. Fade to white, then to the field at morning — no fog, all the anchors gone, the sailboat floating in a small pond. Card: "Morning."]*

---

## IDLE, FAIL, AND REACTIVE LINES

### Pull catches the player (random, not repeating back-to-back)
- **SYS_CATCH_001** VOICE — "No, no. Not yet. Back you go."
- **SYS_CATCH_002** VOICE — "That's what it does. It pulls. Don't let it."
- **SYS_CATCH_003** VOICE — *(Act 3 only, shaken)* "I felt that. I felt your chest."
- **SYS_CATCH_004** VOICE — "It almost had you. Close your eyes sooner."

### Eyes closed for a long time (> 30 s)
- **SYS_EYES_001** VOICE — "That's right. Just rest your eyes."
- **SYS_EYES_002** VOICE — "You could stay like this. It's so much quieter."
- **SYS_EYES_003** MAREN — *(leak, faint, Act 2+)* "Open your eyes. Tom, open your eyes."

### Player idle (> 60 s)
- **SYS_IDLE_001** VOICE — "No hurry. There's never any hurry here."
- **SYS_IDLE_002** VOICE — "You used to stand like that in doorways. Halfway in, halfway out."
- **SYS_IDLE_003** VOICE — "You can wait as long as you like. She can't."

### Player tries to walk off the map edge (into deep fog)
- **SYS_EDGE_001** VOICE — "Nothing out there, Tomas. You never went anywhere else."
- **SYS_EDGE_002** VOICE — "That way's just fog. All the way down."

### Player throws an object
- **SYS_THROW_001** VOICE — "Careful with that. It's the only one."
- **SYS_THROW_002** VOICE — *(mild)* "Well. It was yours to throw."

### Pause menu opened (Voice comments, rare, 1 in 10)
- **SYS_PAUSE_001** VOICE — "Taking a breath? Good. Somebody should."

---

## EASTER EGG LINES

See `06_Easter_Eggs_Secrets.md` for triggers.

- **EGG_001** VOICE — *(Biscuit the dog found)* "Biscuit. You forgot about Biscuit. He waited at the door every day after she was gone."
- **EGG_002** VOICE — *(all 12 Wren drawings found)* "She drew every day. You kept every single one. In the nightstand. You never looked at them."
- **EGG_003** VOICE — *(player hums the Lantern tune in Ch.1 via mic)* "Where'd you learn that? Oh. You made it up. Later."
- **EGG_004** VOICE — *(player stares at the 3:12 clock for 60 s)* "It's not going to move. Not until somebody makes it."
- **EGG_005** VOICE — *(player tries to eat the honey in Ch.4)* "...Really? Now?"
- **EGG_006** WREN — *(rubber duck squeezed 7 times in Ch.8)* "Quack quack quack! That's his name. Quack-quack the Seventh."
- **EGG_007** VOICE — *(Lucid Mode, New Game+, first line)* "Oh. You again. You know how this ends, don't you? ...Then why are you still walking?"
- **EGG_008** VOICE — *(Lucid Mode, Pull first seen)* "You know what that is now. You can't un-know it."
- **EGG_009** ELIO — *(Lucid Mode, lake, after the rescue)* "You'll do the same for somebody someday, mijo."
- **EGG_010** MAREN_MEM — *(platform, if the player waves at Maren's train 3 times)* "Okay, okay, I see you, scarf guy!"
