using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.Interaction;
using LastNight.Player;
using LastNight.Pull;
using LastNight.World;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Field.unity: the open dream between memories. Hosts the Prologue (bed, boards, rise, toy boat),
    /// the walks between the five anchors, and Chapter 6 (Convergence) at the centre.
    /// Layout (metres): bed south at z -70, rise at z -22, centre/lamp at z +60, anchors ~130 m around it.
    /// </summary>
    public static class FieldBuilder
    {
        public static readonly Vector3 Center = new Vector3(0, 0, 60);
        static readonly Vector3 BedPos = new Vector3(0, 0, -70);
        static readonly Vector3 RisePos = new Vector3(0, 0, -22);

        struct Anchor { public Chapter chapter; public Vector3 pos; public Color color; public string section, model; public float height; }

        static readonly Anchor[] Anchors =
        {
            new Anchor { chapter = Chapter.TooFar, pos = new Vector3(-110, 0, 125), color = new Color(0.45f, 0.65f, 0.9f), section = "House", model = "House_exterior", height = 5f },
            new Anchor { chapter = Chapter.RollCall, pos = new Vector3(0, 0, 195), color = new Color(0.75f, 0.9f, 0.75f), section = "Classroom", model = "Chalkboard", height = 2.2f },
            new Anchor { chapter = Chapter.Line4, pos = new Vector3(110, 0, 125), color = new Color(0.95f, 0.35f, 0.3f), section = "Station", model = "Platform_shelter", height = 3.2f },
            new Anchor { chapter = Chapter.YellowKitchen, pos = new Vector3(100, 0, -40), color = new Color(1f, 0.85f, 0.35f), section = "Kitchen", model = "Kitchen_counter_set", height = 2.1f },
            new Anchor { chapter = Chapter.LongHall, pos = new Vector3(-100, 0, -40), color = new Color(0.45f, 0.85f, 0.85f), section = "Hospital", model = "Hospital_door", height = 2.1f },
        };

        public static void Build()
        {
            var scene = Kit.NewScene("Field");
            Kit.Atmosphere(Kit.Mood(new Color(0.8f, 0.81f, 0.82f), 8f, 150f, new Color(0.6f, 0.61f, 0.63f),
                new Color(1f, 0.97f, 0.92f), 0.55f, new Vector3(35, 30, 0)));
            var world = Kit.Go("World");
            Kit.Floor("Ground", world.transform, Vector3.zero, new Vector2(1200, 1200), Kit.C("grass"));
            Kit.Bounds(world.transform, Vector3.zero, 560f);
            Kit.Tufts(world.transform, Vector3.zero, 3f, 220f, 450, new Color(0.36f, 0.42f, 0.33f), 11);

            // ----- Ambience -----
            var amb = Kit.Go("Ambience");
            Kit.Sound(amb.transform, "Wind", Vector3.zero, "SFX_wind_field", loop: true, spatial: false, volume: 0.5f, playOnAwake: true);
            Kit.Sound(amb.transform, "Drone", Vector3.zero, "MUS_Drone_Field", loop: true, spatial: false, volume: 0.25f, playOnAwake: true);

            // ----- Spawns -----
            Kit.Spawn(world.transform, BedPos + new Vector3(1.2f, 0, 0.4f), 0f, true, name: "Spawn Bed");
            Kit.Spawn(world.transform, new Vector3(0, 0, -5), 0f, false, name: "Spawn Convergence", startOf: Chapter.Convergence);
            foreach (var a in Anchors)
            {
                var toCenter = (Center - a.pos).normalized;
                var yaw = Quaternion.LookRotation(toCenter).eulerAngles.y;
                Kit.Spawn(world.transform, a.pos + toCenter * 9f, yaw, false, a.chapter, $"Return from {a.chapter}");
            }

            // ----- Prologue -----
            var pro = Kit.Go("Prologue", world.transform);
            var bed = Kit.Model("Prologue_Field", "Old_double_bed", pro.transform, BedPos, 0f, length: 2.1f, rootName: "Bed");
            Kit.FitCollider(bed);
            var nightstand = Kit.Model("Prologue_Field", "Nightstand_and_digital_clock", pro.transform, BedPos + new Vector3(-1.5f, 0, 0.6f), 0f, height: 0.65f, rootName: "Nightstand");
            nightstand.AddComponent<StoppedClockEgg>();
            for (int i = 0; i < 20; i++)
                Kit.Model("Prologue_Field", "Floorboard_path_segment", pro.transform, BedPos + new Vector3(0, 0, 3f + i * 2f), 90f, length: 2f, rootName: "Board");
            var rise = Kit.Go("Rise", world.transform, RisePos);
            for (int i = 0; i < 6; i++)
            {
                var step = Kit.Shape(PrimitiveType.Cylinder, "Rise Step", rise.transform, new Vector3(0, 0.1f + i * 0.2f, 0),
                    new Vector3(20f - i * 2.8f, 0.1f, 20f - i * 2.8f), Kit.C("grass"), collider: false);
                step.AddComponent<MeshCollider>();
                Kit.Static(step);
            }
            var riseTrigger = Kit.Trigger("Rise Top", rise.transform, new Vector3(0, 2.2f, 0), new Vector3(7, 3, 7));
            var boat = Kit.Model("Prologue_Field", "Toy_sailboat", pro.transform, new Vector3(1.6f, 1.2f, -20f), 30f, length: 0.4f, rootName: "Toy Boat");
            var boatGrab = Kit.Grabbable(boat, 0.3f);
            Kit.Memory(boat);

            var prologue = Kit.Go("Prologue Director").AddComponent<PrologueDirector>();
            var cold = Kit.Sound(prologue.transform, "Cold Open", Vector3.zero, "SFX_heartbeat_stutter", spatial: false);
            Kit.Set(prologue, "coldOpenAudio", cold);
            Kit.Set(prologue, "handsOut", Kit.Gesture(prologue.transform, GestureDetector.Kind.HandsOut, prologue.OnHandsOut));
            Kit.Set(prologue, "boardsEndTrigger", riseTrigger.GetComponent<Collider>());
            Kit.OnEnter(riseTrigger, prologue.OnReachedRise);
            Kit.Set(prologue, "toyBoat", boatGrab);
            Kit.Set(prologue, "bed", bed);

            // ----- Anchors -----
            var anchorRoots = new List<Transform>();
            foreach (var a in Anchors)
            {
                var root = Kit.Go($"Anchor {a.chapter}", world.transform, a.pos);
                anchorRoots.Add(root.transform);
                var face = Quaternion.LookRotation(Center - a.pos).eulerAngles.y;
                Kit.Model(a.section, a.model, root.transform, Vector3.zero, face, height: a.height, rootName: "Landmark");
                var available = Kit.Go("Available", root.transform);
                Kit.Beacon("Beacon", available.transform, Vector3.zero, a.color);
                Kit.PointLight("Glow", available.transform, new Vector3(0, 2.5f, 0), a.color, 2f, 10f);
                var done = Kit.Go("Done", root.transform);
                Kit.Block("Ash", done.transform, new Vector3(0, 0.05f, 0), new Vector3(3, 0.1f, 3), "gray", false);
                done.SetActive(false);
                var portal = Kit.Trigger("Portal", root.transform, new Vector3(0, 1.5f, 0), new Vector3(5, 3, 5), once: false);
                Object.DestroyImmediate(portal); // AnchorPortal handles OnTriggerEnter itself
                var ap = root.transform.Find("Portal").gameObject.AddComponent<AnchorPortal>();
                Kit.Set(ap, "chapter", a.chapter);
                Kit.Set(ap, "visualsWhenAvailable", available);
                Kit.Set(ap, "visualsWhenDone", done);
            }

            // ----- Field events and easter eggs -----
            var events = Kit.Go("Field Events", world.transform);
            var door = Kit.Model("Prologue_Field", "Standalone_door_in_field", events.transform, new Vector3(30, 0, 30), 0f, height: 2.1f, rootName: "Standalone Door");
            Kit.FitCollider(door);
            door.SetActive(false);
            var rack = Kit.Model("Prologue_Field", "Coat_rack_and_coat", events.transform, new Vector3(-30, 0, 30), 0f, height: 1.8f, rootName: "Coat Rack");
            var pocket = Kit.Go("Pocket", rack.transform, new Vector3(0.15f, 1.0f, 0.1f));
            pocket.AddComponent<BoxCollider>().size = new Vector3(0.25f, 0.25f, 0.25f);
            var pocketSignal = Kit.Signal(pocket, grabbable: false);
            var receipt = Kit.Text3D("Receipt", rack.transform, new Vector3(0, 1.35f, 0.25f), "ST. ALDER'S CAFE\n1 COFFEE\n03:10", 0.6f, Kit.C("black"), 0f, 0.35f).gameObject;
            receipt.SetActive(false);
            rack.SetActive(false);
            var windLantern = Kit.Sound(events.transform, "Wind Lantern", Vector3.zero, "MUS_Lantern_MusicBox", spatial: false, volume: 0.35f);
            var biscuit = Kit.Model("Prologue_Field", "Biscuit_the_dog", events.transform, new Vector3(0, 0, -10), 180f, height: 0.5f, rootName: "Biscuit");
            var biscuitSignal = Kit.Signal(biscuit, grabbable: false);
            biscuit.SetActive(false);
            var eighth = Kit.Beacon("Eighth Anchor", events.transform, RisePos + new Vector3(35, 0, 25), new Color(1f, 0.95f, 0.8f), 8f, 0.3f);
            eighth.SetActive(false);
            var curtains = Kit.Go("Hospital Curtains", events.transform, new Vector3(-45, 0, 10));
            for (int i = 0; i < 7; i++)
                Kit.Block("Curtain", curtains.transform, new Vector3(i * 0.9f, 1.3f, Mathf.Sin(i) * 0.2f), new Vector3(0.85f, 2.4f, 0.04f), new Color(0.75f, 0.83f, 0.82f), false);
            Kit.Block("Rail", curtains.transform, new Vector3(2.7f, 2.55f, 0), new Vector3(6.6f, 0.04f, 0.04f), "steel", false);
            curtains.SetActive(false);
            var sightings = new List<Transform>();
            for (int i = 0; i < 8; i++)
            {
                float ang = i * Mathf.PI / 4f;
                sightings.Add(Kit.Go("Sighting", world.transform, RisePos + new Vector3(Mathf.Cos(ang) * 90f, 0, Mathf.Sin(ang) * 90f)).transform);
            }

            // ----- The Pull (sightings in the field; circles the lamp in Chapter 6) -----
            var centerT = Kit.Go("Convergence Center", world.transform, Center).transform;
            var pull = Kit.Pull(world.transform, Center + new Vector3(0, 0, -25), 180f, centerT);

            var field = Kit.Go("Field Director").AddComponent<FieldDirector>();
            Kit.Set(field, "hospitalCurtains", curtains);
            Kit.Set(field, "pull", pull);
            Kit.Set(field, "sightingPoints", sightings);
            Kit.Set(field, "standaloneDoor", door);
            Kit.Set(field, "coatRack", rack);
            Kit.Set(field, "windLantern", windLantern);
            Kit.Set(field, "biscuit", biscuit);
            Kit.Set(field, "biscuitPet", biscuitSignal);
            Kit.Set(field, "coatPocket", pocketSignal);
            Kit.Set(field, "receipt", receipt);
            Kit.Set(field, "eighthAnchor", eighth);
            Kit.Set(field, "anchors", anchorRoots);
            Kit.Set(field, "rise", rise.transform);

            // ----- Chapter 6: Convergence -----
            BuildConvergence(world.transform, centerT, pull);

            var collectible = Kit.Go("Drawing on the nightstand", world.transform, new Vector3(-1.5f, 0.66f, -69.4f), 0f);
            Kit.Drawing("Paper", collectible.transform, Vector3.zero, Vector3.zero, 7, 0.3f);
            Kit.Set(collectible.AddComponent<LastNight.World.DrawingCollectible>(), "index", 7);
            Kit.Save(scene, "Field");
        }

        static void BuildConvergence(Transform world, Transform center, PullController pull)
        {
            var root = Kit.Go("Convergence", world, Vector3.zero);

            // The portal at the centre: available once anchors 1-5 are done.
            var portalRoot = Kit.Go("Anchor Convergence", world, Center);
            var available = Kit.Go("Available", portalRoot.transform);
            Kit.Beacon("Beacon", available.transform, Vector3.zero, new Color(1f, 0.9f, 0.7f), 80f, 2f);
            var portal = Kit.Go("Portal", portalRoot.transform, new Vector3(0, 1.5f, 0));
            portal.AddComponent<BoxCollider>().isTrigger = true;
            portal.GetComponent<BoxCollider>().size = new Vector3(6, 3, 6);
            var ap = portal.AddComponent<AnchorPortal>();
            Kit.Set(ap, "chapter", Chapter.Convergence);
            Kit.Set(ap, "visualsWhenAvailable", available);

            // Fragments of every memory, drifting inward and tilting.
            var frags = new List<DriftingFragment>();
            (string s, string m, float len)[] pieces =
            {
                ("Kitchen", "Kitchen_counter_set", 2.4f), ("Station", "Train_car", 14f), ("Classroom", "School_desk", 1.2f),
                ("Classroom", "School_desk", 1.2f), ("Classroom", "School_desk", 1.2f), ("House", "Wooden_dock", 12f),
                ("Hospital", "Gurney", 1.9f), ("Station", "Bench", 1.8f), ("Hospital", "Corridor_module", 6f),
            };
            for (int i = 0; i < pieces.Length; i++)
            {
                float ang = i * Mathf.PI * 2f / pieces.Length + 0.3f;
                var pos = Center + new Vector3(Mathf.Cos(ang) * 55f, 0f, Mathf.Sin(ang) * 55f);
                var f = Kit.Model(pieces[i].s, pieces[i].m, root.transform, pos, i * 40f, length: pieces[i].len, rootName: "Fragment");
                Kit.FitCollider(f);
                frags.Add(f.AddComponent<DriftingFragment>());
            }
            var lamp = Kit.Go("Bedroom Lamp", root.transform, Center);
            Kit.Model("Kitchen", "Pull_chain_lamp", lamp.transform, new Vector3(0, 2.6f, 0), 0f, height: 0.8f);
            Kit.Block("Lamp Pole", lamp.transform, new Vector3(0, 1.6f, 0), new Vector3(0.06f, 3.2f, 0.06f), "darksteel");
            Kit.PointLight("Lamp Light", lamp.transform, new Vector3(0, 2.7f, 0), new Color(1f, 0.85f, 0.6f), 3f, 18f);
            lamp.SetActive(false);

            var shocksGo = Kit.Go("Shocks", root.transform);
            var shocks = shocksGo.AddComponent<ShockScheduler>();
            Kit.Set(shocks, "chargeAndShock", Kit.Sound(shocksGo.transform, "Defib", Vector3.zero, "SFX_defib_charge", spatial: false));
            Kit.Set(shocks, "realWorldLayer", (int)(1 << Mathf.Max(0, LayerMask.NameToLayer(QuestSetup.RealWorldLayer))));
            shocks.enabled = false;

            var dispatch = Kit.Go("Dispatch Voice", root.transform, Center + Vector3.up * 3f);
            var d = Kit.Go("Convergence Director").AddComponent<Ch6ConvergenceDirector>();
            Kit.Set(d, "chapter", Chapter.Convergence);
            Kit.Set(d, "pull", pull);
            Kit.Set(d, "beatVolume", 0.9f);
            Kit.Set(d, "beatIntensity", 0.6f);
            Kit.Set(d, "convergenceRoot", root);
            Kit.Set(d, "fragments", frags);
            Kit.Set(d, "shocks", shocks);
            Kit.Set(d, "center", center);
            Kit.Set(d, "bedroomLamp", lamp);
            Kit.Set(d, "dispatchVoice", dispatch.transform);
            root.SetActive(false);
        }
    }
}
