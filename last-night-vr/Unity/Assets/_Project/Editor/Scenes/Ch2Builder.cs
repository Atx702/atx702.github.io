using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.Player;
using LastNight.World;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using G = LastNight.EditorTools.Rooms.Gap;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch2_Classroom.unity, "Roll Call": a 40 x 60 m classroom of desks in fog. Find three beeping
    /// speakers by ear, raise your hand, read the speech on the board, the pulse clip, the Pull's first stalk.
    /// </summary>
    public static class Ch2Builder
    {
        static readonly string[] Speech = { "When", "I", "grow", "up", "I", "want", "to", "build", "houses", "that", "keep", "people", "safe." };

        public static void Build()
        {
            var scene = Kit.NewScene("Ch2_Classroom");
            var fog = new Color(0.68f, 0.72f, 0.69f);
            Kit.Atmosphere(Kit.Mood(fog, 5f, 34f, new Color(0.5f, 0.54f, 0.52f), new Color(0.9f, 0.95f, 1f), 0.35f, new Vector3(50, 160, 0)));
            var room = Kit.Go("Classroom");
            var r = room.transform;
            var wall = new Color(0.62f, 0.67f, 0.62f);
            Rooms.Box("Room", r, new Vector3(-20, 0, -10), new Vector2(40, 60), 4.2f, wall, Kit.C("linoleum"), south: new[] { new G(20f, 1.6f, 2.4f) });
            Kit.Spawn(r, new Vector3(0, 0, 42.5f), 0f);
            Kit.Sound(r, "Rain", new Vector3(0, 3, 20), "SFX_rain_windows", loop: true, spatial: false, volume: 0.45f, playOnAwake: true);
            Kit.Sound(r, "Hum", new Vector3(0, 3, 20), "SFX_fluorescent_hum", loop: true, spatial: false, volume: 0.15f, playOnAwake: true);
            for (int i = 0; i < 6; i++) // tall windows along the east wall, glowing grey
                Kit.Block("Window", r, new Vector3(19.9f, 2.3f, -4 + i * 9), new Vector3(0.05f, 2.6f, 3f), new Color(0.85f, 0.88f, 0.9f), false)
                    .GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(0.8f, 0.84f, 0.86f), 0.8f);

            // Desks (5 x 6) and the player's own desk near the board.
            for (int col = 0; col < 5; col++)
                for (int row = 0; row < 6; row++)
                {
                    var p = new Vector3(-10 + col * 5, 0, 2 + row * 6.5f);
                    if (Mathf.Abs(p.x) < 1f && p.z > 30f) continue;
                    var desk = Kit.Model("Classroom", "School_desk", r, p, 0f, length: 1.2f, rootName: "Desk");
                    Kit.FitCollider(desk);
                }
            var myDesk = Kit.Model("Classroom", "School_desk", r, new Vector3(0, 0, 44f), 0f, length: 1.2f, rootName: "Your Desk");
            Kit.FitCollider(myDesk);

            // The board and the speech.
            var board = Kit.Model("Classroom", "Chalkboard", r, new Vector3(0, 0.6f, 49.6f), 180f, length: 6f, rootName: "Chalkboard");
            Kit.Model("Classroom", "Wall_clock", r, new Vector3(5.5f, 3.2f, 49.85f), 180f, height: 0.4f);
            var words = new List<ChalkWord>();
            var chalkSound = Kit.Sound(r, "Chalk", new Vector3(0, 1.8f, 49.4f), "SFX_chalk", volume: 0.8f);
            ChalkWord previous = null;
            for (int i = 0; i < Speech.Length; i++)
            {
                int line = i < 5 ? 0 : i < 9 ? 1 : 2;
                int first = line == 0 ? 0 : line == 1 ? 5 : 9;
                int count = line == 0 ? 5 : 4;
                float x = (i - first - (count - 1) / 2f) * 1.0f;
                var t = Kit.Text3D($"Word {i + 1}", r, new Vector3(x, 2.35f - line * 0.45f, 49.45f), Speech[i], 2.2f, new Color(0.95f, 0.95f, 0.9f), 0f, 1.2f);
                var w = t.gameObject.AddComponent<ChalkWord>();
                if (previous != null) Kit.Set(w, "previous", previous);
                Kit.Set(w, "chalkSound", chalkSound);
                words.Add(w);
                previous = w;
            }
            var flash = Kit.Drawing("Flash Drawing", r, new Vector3(0, 1.9f, 49.4f), Vector3.zero, 1, 1.6f);
            flash.SetActive(false);
            var adair = Kit.Go("Ms. Adair (voice)", r, new Vector3(-2.5f, 1.6f, 48f));

            // Speakers: three boxes, eight places they might be.
            var candidates = new List<Transform>();
            Vector3[] spots =
            {
                new Vector3(-19.7f, 2.6f, 5), new Vector3(19.7f, 2.6f, 30), new Vector3(-12, 0.8f, 15), new Vector3(10, 0.8f, 27),
                new Vector3(-19.7f, 1.2f, 38), new Vector3(19.7f, 1.2f, 10), new Vector3(0, 3.9f, 20), new Vector3(-5, 0.8f, 2),
            };
            foreach (var s in spots)
            {
                float yaw = s.x < -19f ? 90f : s.x > 19f ? -90f : 180f;
                candidates.Add(Kit.Go("Speaker Spot", r, s, yaw).transform);
            }
            var speakers = new List<SpeakerBox>();
            for (int i = 0; i < 3; i++)
            {
                var sp = Kit.Model("Classroom", "Old_speaker_box", r, spots[i], 0f, height: 0.4f, rootName: $"Speaker {i + 1}");
                Kit.FitCollider(sp);
                var sw = Kit.Block("Switch", sp.transform, new Vector3(0, 0.2f, -0.12f), new Vector3(0.08f, 0.12f, 0.06f), "darksteel", false);
                var simple = Kit.Simple(sw);
                var beep = Kit.Sound(sp.transform, "Beep", new Vector3(0, 0.2f, 0), "SFX_monitor_beep", maxDistance: 45f);
                beep.spatialize = true;
                var clunk = Kit.Sound(sp.transform, "Clunk", new Vector3(0, 0.2f, 0), "SFX_flip_tile");
                var box = sp.AddComponent<SpeakerBox>();
                Kit.Set(box, "beep", beep);
                Kit.Set(box, "toggleSwitch", simple);
                Kit.Set(box, "switchVisual", sw.transform);
                Kit.Set(box, "clunk", clunk);
                Kit.Dynamic(sp); // speakers get shuffled at runtime
                speakers.Add(box);
            }

            // Memory layers come back as each speaker is silenced: lights, the other children's things, a paper plane.
            var layerLights = Kit.Go("Layer Lights", r);
            for (int i = 0; i < 4; i++) Kit.PointLight("Ceiling Light", layerLights.transform, new Vector3(0, 3.8f, 6 + i * 11), new Color(1f, 0.97f, 0.9f), 1.4f, 14f);
            layerLights.SetActive(false);
            var layerThings = Kit.Go("Layer Backpacks", r);
            for (int i = 0; i < 10; i++)
                Kit.Block("Backpack", layerThings.transform, new Vector3(-10 + (i % 5) * 5 + 0.6f, 0.2f, 2 + (i / 5) * 6.5f + 0.5f), new Vector3(0.3f, 0.4f, 0.18f),
                    i % 2 == 0 ? Kit.C("red") : Kit.C("blue"), false);
            layerThings.SetActive(false);
            var layerPlane = Kit.Model("Classroom", "Paper_airplane", r, new Vector3(1.5f, 2.2f, 40f), 200f, length: 0.3f, rootName: "Layer Paper Plane");
            layerPlane.SetActive(false);

            // Leak: the pulse clip on your desk.
            var clip = Kit.Model("Classroom", "Pulse_oximeter_clip_leak", r, new Vector3(0.3f, 0.85f, 44.1f), 30f, length: 0.08f, rootName: "Pulse Clip");
            var clipSignal = Kit.Signal(clip, mass: 0.05f);
            Kit.Set(clip.AddComponent<LeakedObjectItem>(), "which", LeakedObject.PulseClip);
            var clipBeep = Kit.Sound(clip.transform, "Clip Beep", Vector3.zero, "SFX_monitor_beep", volume: 0.6f);

            // Exit through the back door; the Pull waits between the desks.
            var exit = Kit.Trigger("Exit", r, new Vector3(0, 1.2f, -11f), new Vector3(2.5f, 2.4f, 1.5f));
            Kit.Beacon("Exit Glow", r, new Vector3(0, 0, -10.5f), new Color(0.85f, 0.95f, 0.85f), 3f, 0.4f);
            Kit.Floor("Outside", r, new Vector3(0, 0, -16f), new Vector2(6f, 10f), fog * 0.9f);
            var pull = Kit.Pull(r, new Vector3(0, 0, 9f), 0f);
            var pullStart = Kit.Go("Pull Start", r, new Vector3(0, 0, 9f), 0f);

            var memory = Kit.Memory(room);
            var d = Kit.Director<Ch2ClassroomDirector>(Chapter.RollCall, memory, pull, 0f, 0f);
            var raise = Kit.Gesture(d.transform, GestureDetector.Kind.RaiseHand, d.OnHandRaised);
            Kit.OnEnter(exit, d.OnExit);
            Kit.Set(d, "adair", adair.transform);
            Kit.Set(d, "speakers", speakers);
            Kit.Set(d, "speakerCandidates", candidates);
            Kit.Set(d, "memoryLayers", new List<Object> { layerLights, layerThings, layerPlane });
            Kit.Set(d, "raiseHand", raise);
            Kit.Set(d, "speechWords", words);
            Kit.Set(d, "flashDrawing", flash);
            Kit.Set(d, "chalkboardText", board.GetComponentInChildren<Renderer>());
            Kit.Set(d, "pulseClip", clipSignal);
            Kit.Set(d, "clipBeep", clipBeep);
            Kit.Set(d, "pullStart", pullStart.transform);
            Kit.Set(d, "exitTrigger", exit.GetComponent<Collider>());

            var collectible = Kit.Go("Drawing on the wall", r, new Vector3(-19.9f, 1.4f, 30f), -90f);
            Kit.Drawing("Paper", collectible.transform, Vector3.zero, Vector3.zero, 8, 0.3f);
            Kit.Set(collectible.AddComponent<LastNight.World.DrawingCollectible>(), "index", 8);
            Kit.Save(scene, "Ch2_Classroom");
        }
    }
}
