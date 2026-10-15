# LAST NIGHT — Art Assets and Meshy Prompts

## Pipeline per asset
1. **Meshy:** generate with the prompt below. Use "Realistic" style, PBR textures on. Generate 4, pick 1.
2. **Blender:** import → check scale (1 unit = 1 m) → Decimate or retopologize to the triangle budget → fix UVs → bake textures to a 1024² (props) or 2048² (hero) atlas → apply the project's desaturation (see color rules) → export FBX to `Unity/Assets/_Project/Art/Models/<Chapter>/`.
3. **Unity:** URP Lit material, enable GPU instancing, set the LOD group (LOD0 100%, LOD1 50%, cull at 2%).

## Batch generation
`tools/meshy_generate.py` makes every model in the prompt tables below through the Meshy API (mesh, then PBR textures) and saves GLB + FBX to `Unity/Assets/_Project/Art/Models/<Section>/`. It needs `MESHY_API_KEY` set. Run `--dry-run` first to see the list (53 models, about 1,600 credits). It can be stopped and re-run safely; progress is in `tools/meshy_tasks.json`. It makes one take per model; re-roll any you don't like with `--only "<Section>/<Asset>"` after deleting its entry.

## Stand-in models (made in code)
Meshy credits ran out after 25 of the 53 models, so the other **28 are built by `tools/procgen/`** (a small Python modeller; no Blender needed): the Station scarf and lost-property bin, all of the Kitchen, Hospital and Bedroom / Wren's room props, the six characters, plus **Tomas** (his body, for the bed and CPR scenes) and **The Pull** (2.6 m, white emissive silhouette, as the design asks). Run `python tools/procgen/build_models.py` to rebuild them; `python tools/procgen/preview.py sheet.png` renders a contact sheet.

They are flat-coloured low-poly (150 to 6,000 triangles, well under budget) in the project palette, written as GLB plus OBJ/MTL (Unity reads OBJ natively). Characters are in A-pose, ready for Mixamo auto-rigging. They are placeholders in look, not final art: when you get more Meshy credits, run `python tools/meshy_generate.py`, and the Meshy FBX for a name will be used in preference (the builder skips any name that already has a Meshy file).

## Triangle budgets (Quest 2)
| Type | Budget |
| --- | --- |
| Small prop (mug, glove) | 300–1,500 |
| Medium prop (chair, radio) | 1,500–5,000 |
| Large prop (bed, vending machine) | 5,000–10,000 |
| Building shell (house, shelter) | 10,000–25,000 |
| Character | 15,000–25,000 (Maren, Wren), 8,000 (background) |
| Whole scene visible at once | under 300,000 |

## Color rules
- Base everything at about 30% saturation.
- Each chapter has **one accent color** that is the only saturated color in it:

| Chapter | Accent |
| --- | --- |
| Prologue / Field | None |
| 1 House | Faded "wrong" blue (boat, door) |
| 2 Classroom | Chalk white + pale green board |
| 3 Station | Red (the scarf) |
| 4 Kitchen | Warm yellow (walls, honey) |
| 5 Hospital | Cold teal |
| 6 Convergence | All accents, flickering |
| 7 Bedroom | Gray-blue night |
| 8 Wren's room | Golden yellow (the only fully saturated room) |
| Leaked objects | Cold blue rim light |

## Meshy prompt list

Prompt format tips: name the object, the era, the wear, the material and "game asset, single object, neutral background."

### Prologue / Field
| Asset | Meshy prompt |
| --- | --- |
| Old double bed | "Old wooden double bed with rumpled white sheets and one pillow, 2000s American bedroom, slightly worn, game asset, single object" |
| Nightstand + digital clock | "Small wooden nightstand with a red LED digital alarm clock on top, worn edges, game asset" |
| Floorboard path segment | "Section of old weathered pine floorboards, 2 meters long, gaps between boards, game asset" |
| Toy sailboat | "Hand-carved wooden toy sailboat painted pale blue with a paper sail, child's toy, game asset" |
| Standalone door in field | "Old white interior door in its frame, freestanding, paint chipped, brass knob, game asset" |
| Coat rack + coat | "Wooden coat rack with a gray wool men's coat hanging on it, game asset" |
| Biscuit the dog | "Small scruffy brown mixed-breed dog sitting, friendly, stylized realistic, game asset" |

### Chapter 1: House
| Asset | Meshy prompt |
| --- | --- |
| House exterior | "Small one-story white stucco house with a blue front door and a tin roof, 1980s, simple, game asset" |
| Kitchen table + bowl | "Simple 1980s wooden kitchen table with one ceramic bowl, game asset" |
| Child's bed + shelf | "Small child's bed with a striped blanket and a wall shelf with toy boats, 1980s, game asset" |
| Fridge | "1980s white refrigerator with a child's drawing held on with a magnet, game asset" |
| Wooden dock | "Short weathered wooden lake dock, 12 meters, gray planks, game asset" |
| Medical glove (leak) | "Single blue nitrile medical glove, crumpled, wet, game asset" |
| Wind chimes | "Simple metal wind chimes hanging from a hook, game asset" |

### Chapter 2: Classroom
| Asset | Meshy prompt |
| --- | --- |
| School desk | "1990s school desk with attached chair, metal frame and laminate top, scratched, game asset" |
| Chalkboard | "Large green chalkboard with wooden frame and chalk tray, game asset" |
| Wall clock | "Round analog school wall clock, white face, black numbers, game asset" |
| Old speaker box | "Old wooden classroom PA speaker box with a fabric grille and a toggle switch, game asset" |
| Pulse-oximeter clip (leak) | "Gray plastic finger pulse oximeter clip with a short cut cable, medical, game asset" |
| Paper airplane | "Folded white paper airplane, game asset" |

### Chapter 3: Station
| Asset | Meshy prompt |
| --- | --- |
| Platform shelter | "Outdoor commuter train platform shelter, metal frame and glass, bench underneath, 2000s, game asset" |
| Bench | "Metal slatted platform bench, green paint, worn, game asset" |
| Flip timetable board | "Split-flap departure board for a train station, black with white letters, game asset" |
| Vending machine | "Snack vending machine, 2000s, glowing front, game asset" |
| Train car | "Modern commuter train car, single carriage, silver with a red stripe, game asset" |
| Red scarf | "Long knitted red wool scarf, soft folds, game asset" |
| Lost-property bin | "Metal lost and found bin with a hinged lid, train station, game asset" |

### Chapter 4: Kitchen
| Asset | Meshy prompt |
| --- | --- |
| Kitchen counter set | "Small apartment kitchen counter with sink and stove, yellow walls, 2009, cozy, game asset" |
| Kettle | "Stainless steel stovetop whistling kettle, game asset" |
| Mugs (×3) | "Ceramic coffee mug, cream color, simple, game asset" |
| Honey jar | "Glass honey jar with a wooden dipper, game asset" |
| Radio | "Small retro tabletop radio with a tuning dial, cream plastic, game asset" |
| Pull-chain lamp | "Bare bulb pendant lamp with a pull chain, game asset" |
| Wedding ring (leak) | "Plain gold wedding band, men's, game asset" |

### Chapter 5: Hospital
| Asset | Meshy prompt |
| --- | --- |
| Corridor module | "Hospital corridor section, pale teal walls, linoleum floor, ceiling lights, handrail, 6 meters, game asset" |
| Hospital door | "Hospital patient room door with a small window and a room number plate, game asset" |
| Waiting chair | "Hospital waiting room chair, vinyl seat, metal arms, game asset" |
| Vending machine (coffee) | "Hospital coffee vending machine, glowing, 2020, game asset" |
| Wall clock | (reuse the classroom clock with a teal frame) |
| Crayon drawings | 2D textures painted by hand or generated, not Meshy |
| Patient bracelet (leak) | "White plastic hospital patient wristband with printed text label, game asset" |
| Gurney | "Hospital gurney with white sheet, game asset" |

### Chapter 7/8: Bedroom, Wren's room
| Asset | Meshy prompt |
| --- | --- |
| Bedroom set | (reuse the prologue bed + nightstand, plus) "Bedroom dresser with mirror, 2020s, game asset" |
| Defibrillator monitor | "Portable defibrillator monitor with paddles and cables, paramedic, game asset" |
| Paramedic bag | "Red paramedic medical bag, open, game asset" |
| Child's bed | "Small child's bed with a yellow blanket and star pillow, game asset" |
| Night lantern | "Child's rotating star night light lantern, paper and wood, game asset" |
| Rubber duck | "Classic yellow rubber duck toy, game asset" |
| Crayons box | "Box of wax crayons, some broken, game asset" |

### Characters (Meshy for base mesh only; then rig in Blender or Mixamo)
| Character | Prompt |
| --- | --- |
| Maren (tonight) | "Woman in her early 40s, Latina, shoulder-length dark hair messy, gray T-shirt and pajama pants, barefoot, realistic, A-pose" |
| Maren (age 22) | "Young woman, early 20s, Latina, dark hair, red knitted scarf, autumn coat, carrying books, realistic, A-pose" |
| Wren | "Girl age 7, curly dark hair, yellow pajamas with stars, realistic stylized, A-pose" |
| Elio | "Man in his 30s, 1980s, mustache, rolled-up shirt sleeves, soaked, realistic, A-pose" |
| Okafor | "Male paramedic, 30s, Black, navy paramedic uniform, gloves, realistic, A-pose" |
| Lind | "Female paramedic, 40s, blond ponytail, navy paramedic uniform, realistic, A-pose" |
| The Pull | **Not Meshy.** Build in Blender: a tall (2.6 m) stretched human silhouette mesh, unlit emissive shader with edge glow, vertex shimmer. Its proportions should match Maren's silhouette stretched by 1.4×. |

## Hands
Use the XR Interaction Toolkit hand models as a base. Add: ring mesh on the left ring finger (toggleable), calloused right index finger detail in the normal map, slightly gray skin tone to match the world.

## Shaders to build (Shader Graph)
| Shader | What it does |
| --- | --- |
| `SG_MemoryDecay` | Lerps saturation → 0, adds noise dissolve to dust, driven by `_Decay` (0–1). |
| `SG_PullBody` | Emissive white, fresnel edge, vertex shimmer pulse driven by `_Beat`. |
| `SG_ChalkReveal` | Reveals chalk text along UV.x by `_Reveal`. |
| `SG_LeakRim` | Cold blue rim light added to any material. |
| `SG_FloorWindow` | Renders the real-world layer through the floor with a water-ripple distortion (Ch.7). |
| `SG_HoneySpread` | Animated decal spreading on the floor. |
