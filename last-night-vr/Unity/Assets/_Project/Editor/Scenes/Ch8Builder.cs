using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Interaction;
using LastNight.World;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using G = LastNight.EditorTools.Rooms.Gap;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch8_WrensRoom.unity, "Little Lantern": the only warm, fog-free room. Wren drawing on the floor,
    /// the lantern, the duck, crayons and paper, her bed. Hold her and sit (STAY), or set her down / go to
    /// the window (WAKE). Also hosts Wren's Easel (free drawing) from the Extras menu.
    /// </summary>
    public static class Ch8Builder
    {
        public static void Build()
        {
            var scene = Kit.NewScene("Ch8_WrensRoom");
            Kit.Atmosphere(Kit.Mood(new Color(0.95f, 0.85f, 0.6f), 30f, 80f, new Color(0.55f, 0.47f, 0.33f), new Color(1f, 0.9f, 0.7f), 0.2f, new Vector3(30, 180, 0), 40f));
            var r = Kit.Go("Wren's Room").transform;
            var gold = new Color(0.9f, 0.78f, 0.45f);
            Rooms.Box("Room", r, new Vector3(-1.75f, 0, 0), new Vector2(3.5f, 4f), 2.5f, gold, new Color(0.6f, 0.45f, 0.3f),
                south: new[] { new G(1.75f, 0.9f) }, north: new[] { new G(1.75f, 1.2f, 1.6f) }, ceilingColor: new Color(0.95f, 0.88f, 0.7f));
            Kit.Block("Window Sill", r, new Vector3(0, 0.45f, 4f), new Vector3(1.2f, 0.9f, 0.15f), gold);
            Kit.Block("Night Sky", r, new Vector3(0, 1.4f, 4.4f), new Vector3(1.6f, 1.4f, 0.05f), Color.black, false)
                .GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(0.08f, 0.1f, 0.2f), 0.6f);
            Kit.Add<BoxCollider>(Kit.Go("Hall Block", r, new Vector3(0, 1.2f, -0.4f))).size = new Vector3(1f, 2.4f, 0.2f);
            Kit.Spawn(r, new Vector3(0, 0, 0.5f), 0f);
            var light = Kit.PointLight("Ceiling Light", r, new Vector3(0, 2.3f, 2f), new Color(1f, 0.85f, 0.6f), 1.4f, 7f);

            // Wren, sitting on the floor drawing.
            var wrenPuppet = Kit.Character("Wren", r, new Vector3(0.5f, 0, 2.4f), 180f, Characters.PuppetAnimator.Move.SitFloorDraw, 0.6f, "Wren");
            var wren = wrenPuppet.gameObject;
            Kit.FitCollider(wren);
            var wrenGrab = Kit.Grabbable(wren, 18f, gravity: false, kinematic: true);
            wrenGrab.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;
            wrenGrab.throwOnDetach = false;
            var wrenSpeaker = Kit.Go("Wren Speaker", wren.transform, new Vector3(0, 1.0f, 0));

            var bed = Kit.Model("Bedroom_Wren_s_room", "Child_bed", r, new Vector3(-1.05f, 0, 2.6f), 90f, length: 1.7f, rootName: "Wren's Bed");
            var seat = Kit.Trigger("Bed Seat", r, new Vector3(-1.05f, 0.9f, 2.6f), new Vector3(0.9f, 0.9f, 1.6f), once: false);

            // Toys.
            var lantern = Kit.Model("Bedroom_Wren_s_room", "Night_lantern", r, new Vector3(1.3f, 0, 3.5f), 0f, height: 0.25f, rootName: "Lantern");
            var lanternSignal = Kit.Signal(lantern, mass: 0.4f);
            var stars = Kit.Go("Star Projector", lantern.transform, new Vector3(0, 0.12f, 0));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Kit.Shape(PrimitiveType.Sphere, "Star", stars.transform, new Vector3(Mathf.Cos(a) * 1.4f, 1.6f + (i % 3) * 0.3f, Mathf.Sin(a) * 1.4f), Vector3.one * 0.05f, Kit.C("yellow"))
                    .GetComponent<Renderer>().sharedMaterial = Kit.Mat(Kit.C("yellow"), 2f);
            }
            Kit.PointLight("Lantern Glow", lantern.transform, new Vector3(0, 0.15f, 0), new Color(1f, 0.85f, 0.5f), 0.8f, 3f);
            var song = Kit.Sound(lantern.transform, "Lantern Song", Vector3.zero, "MUS_Lantern_Hum", volume: 0.9f);
            var humGo = Kit.Go("Hum Detector", r);
            var hum = humGo.AddComponent<HumDetector>();
            hum.enabled = false;
            var duck = Kit.Model("Bedroom_Wren_s_room", "Rubber_duck", r, new Vector3(0.1f, 0, 1.5f), 160f, length: 0.16f, rootName: "Duck");
            var duckSignal = Kit.Signal(duck, mass: 0.1f);
            var squeak = Kit.Sound(duck.transform, "Squeak", Vector3.zero, "SFX_duck_squeak");

            var toys = Kit.Go("Toys").AddComponent<WrensRoomToys>();
            Kit.On(hum, "onHummed", toys.Sing);
            Kit.Set(toys, "wren", wrenSpeaker.transform);
            Kit.Set(toys, "lantern", lanternSignal);
            Kit.Set(toys, "lanternStars", stars.transform);
            Kit.Set(toys, "hum", hum);
            Kit.Set(toys, "lanternSong", song);
            Kit.Set(toys, "duck", duckSignal);
            Kit.Set(toys, "squeak", squeak);

            // Crayons and paper (also Wren's Easel).
            Kit.Model("Bedroom_Wren_s_room", "Crayons_box", r, new Vector3(0.05f, 0, 2.15f), 20f, length: 0.2f);
            var paper = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paper.name = "Paper";
            paper.transform.SetParent(r, false);
            paper.transform.localPosition = new Vector3(0.3f, 0.012f, 1.95f);
            paper.transform.localRotation = Quaternion.Euler(90, 0, 0);
            paper.transform.localScale = new Vector3(0.42f, 0.3f, 1f);
            Object.DestroyImmediate(paper.GetComponent<Collider>());
            paper.GetComponent<Renderer>().sharedMaterial = Kit.ShaderMat("LastNight/World", "Paper", m => m.SetColor("_BaseColor", Color.white));
            var crayons = new List<Crayon>();
            Color[] colours = { new Color(0.85f, 0.2f, 0.2f), new Color(0.25f, 0.45f, 0.85f), new Color(1f, 0.8f, 0.2f) };
            for (int i = 0; i < 3; i++)
            {
                var c = Kit.Shape(PrimitiveType.Cylinder, "Crayon", r, new Vector3(-0.05f + i * 0.05f, 0.01f, 2.3f), new Vector3(0.012f, 0.045f, 0.012f), colours[i]);
                c.transform.localRotation = Quaternion.Euler(0, 0, 90);
                Kit.Grabbable(c, 0.02f);
                var tip = Kit.Go("Tip", c.transform, new Vector3(0, 1f, 0));
                var crayon = c.AddComponent<Crayon>();
                Kit.Set(crayon, "tip", tip.transform);
                Kit.Set(crayon, "color", colours[i]);
                crayons.Add(crayon);
            }
            var cp = paper.AddComponent<CrayonPaper>();
            Kit.Set(cp, "paperRenderer", paper.GetComponent<Renderer>());
            Kit.Set(cp, "crayons", crayons);

            // Drawings: one on the wall, and the one she holds up.
            var wallDrawing = Kit.Go("Drawing (family)", r, new Vector3(-1.72f, 1.3f, 1.2f), -90f);
            Kit.Drawing("Paper", wallDrawing.transform, Vector3.zero, Vector3.zero, 6, 0.4f);
            Kit.Set(wallDrawing.AddComponent<DrawingCollectible>(), "index", 6);
            var held = Kit.Go("Held Drawing", r, new Vector3(0.5f, 0.7f, 2.15f));
            Kit.Drawing("Paper", held.transform, Vector3.zero, Vector3.zero, 12, 0.36f);
            Kit.Set(held.AddComponent<DrawingCollectible>(), "index", 12);
            held.SetActive(false);

            // Cutscenes.
            var drawingCut = Cut.New("CS_08_TheDrawing", Kit.Go("CS_08", r), 6f)
                .Active(held, 0.5f, 5.5f)
                .Pose(wrenPuppet, 0f, 6f, Characters.PuppetAnimator.Move.HoldUp)
                .Done();
            var fogSheet = Kit.Block("Fog In The Window", r, new Vector3(0, 1.4f, 4.2f), new Vector3(1.5f, 1.4f, 0.05f), Color.white, false);
            fogSheet.GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(0.95f, 0.95f, 0.92f), 1.5f);
            fogSheet.SetActive(false);
            Kit.Dynamic(wren);
            var stay = Cut.New("CS_END_STAY", Kit.Go("CS_END_STAY", r), 26f)
                .Light(light, 0f, 20f, 1.4f, 3.5f, null, new Color(1f, 0.75f, 0.3f))
                .Active(fogSheet, 6f, 20f)
                .Sound("MUS_Lantern_MusicBox", 2f)
                .Sound("SFX_flatline", 19f)
                .Done();
            var whiteout = Kit.Shape(PrimitiveType.Sphere, "Whiteout", r, new Vector3(0, 1.3f, 2f), Vector3.one * 6f, Color.white);
            whiteout.GetComponent<Renderer>().sharedMaterial = Kit.ShaderMat("LastNight/World", "Whiteout", m =>
            {
                m.SetColor("_BaseColor", Color.white);
                m.SetColor("_EmissionColor", Color.white * 3f);
                m.SetFloat("_Cull", 0f);
            });
            whiteout.SetActive(false);
            var wake = Cut.New("CS_END_WAKE", Kit.Go("CS_END_WAKE", r), 20f)
                .Sound("SFX_defib_shock", 1.5f)
                .Active(whiteout, 4f, 16f)
                .Sound("SFX_gasp", 6f)
                .Sound("SFX_monitor_beep", 9f)
                .Sound("SFX_monitor_beep", 10.1f)
                .Sound("SFX_monitor_beep", 11.2f)
                .Done();

            // The choice. EndingController lives on the window trigger (its own OnTriggerEnter = WAKE).
            var windowGo = Kit.Go("Window Zone", r, new Vector3(0, 1.1f, 3.5f));
            var wz = windowGo.AddComponent<BoxCollider>();
            wz.isTrigger = true;
            wz.size = new Vector3(1.6f, 2.2f, 1f);
            var ending = windowGo.AddComponent<EndingController>();
            Kit.OnBool(seat, "onEnter", ending.PlayerSatOnBed, true);
            Kit.OnBool(seat, "onExit", ending.PlayerSatOnBed, false);
            Kit.Set(ending, "wren", wrenGrab);
            Kit.Set(ending, "bedSeat", seat.GetComponent<Collider>());
            Kit.Set(ending, "windowZone", wz);
            Kit.Set(ending, "wrenSpeaker", wrenSpeaker.transform);
            Kit.Set(ending, "stayCutscene", stay);
            Kit.Set(ending, "wakeCutscene", wake);
            Kit.Set(ending, "drawingCutscene", drawingCut);
            Kit.Set(ending, "wrenPuppet", wrenPuppet);

            Kit.Save(scene, "Ch8_WrensRoom");
        }
    }
}
