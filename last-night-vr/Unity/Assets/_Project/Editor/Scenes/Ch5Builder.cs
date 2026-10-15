using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.World;
using UnityEngine;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch5_Hospital.unity, "The Long Hall": three looping corridor modules (follow Wren's drawings in story
    /// order to break the loop), the 3:10 coffee choice, room 7 (CS_05), the bracelet, a 150 m chase.
    /// The corridor runs along +z; every module is 6 m long, 2.4 m wide.
    /// </summary>
    public static class Ch5Builder
    {
        const float M = 6f;

        static GameObject Module(Transform parent, float z, string name = "Corridor")
        {
            var m = Kit.Model("Hospital", "Corridor_module", parent, new Vector3(0, 0, z + M / 2), 0f, rootName: name);
            foreach (var mf in m.GetComponentsInChildren<MeshFilter>()) mf.gameObject.AddComponent<MeshCollider>();
            Kit.Static(m);
            return m;
        }

        public static void Build()
        {
            var scene = Kit.NewScene("Ch5_Hospital");
            var fog = new Color(0.55f, 0.65f, 0.66f);
            Kit.Atmosphere(Kit.Mood(fog, 4f, 26f, new Color(0.45f, 0.55f, 0.56f), Color.white, 0f, Vector3.zero, 30f));
            var h = Kit.Go("Hospital").transform;
            Kit.Spawn(h, new Vector3(0, 0, 1.5f), 0f);
            Kit.Sound(h, "Hum", Vector3.zero, "SFX_fluorescent_hum", loop: true, spatial: false, volume: 0.35f, playOnAwake: true);
            Kit.Block("Start Wall", h, new Vector3(0, 1.35f, -0.1f), new Vector3(2.6f, 2.7f, 0.2f), "teal");

            // ----- The loop: three identical modules; reaching the third sends you back to the first. -----
            var loop = Kit.Go("Loop", h);
            for (int i = 0; i < 3; i++) Module(loop.transform, i * M);
            var blocker = Kit.Block("Loop Blocker", loop.transform, new Vector3(0, 1.35f, 3 * M + 0.1f), new Vector3(2.6f, 2.7f, 0.2f), "teal");
            var loopEnd = Kit.Trigger("Loop End Trigger", loop.transform, new Vector3(0, 1.2f, 2 * M + 4f), new Vector3(2.4f, 2.4f, 0.6f), once: false);
            var loopStartPoint = Kit.Go("Loop Start", loop.transform, new Vector3(0, 0, 0f + 4f));
            var loopEndPoint = Kit.Go("Loop End", loop.transform, new Vector3(0, 0, 2 * M + 4f));
            var slots = new List<Transform>();
            for (int i = 0; i < 4; i++)
            {
                slots.Add(Kit.Go("Drawing Slot", loop.transform, new Vector3(-1.13f, 0.9f, 1.5f + i * 2.8f), -90f).transform);
                slots.Add(Kit.Go("Drawing Slot", loop.transform, new Vector3(1.13f, 0.9f, 2.9f + i * 2.8f), 90f).transform);
            }
            var drawings = new List<DrawingCollectible>();
            string[] lines = { "CH5_004", "CH5_005", "CH5_006", "CH5_007", "CH5_008" };
            for (int i = 0; i < 5; i++)
            {
                var root = Kit.Go($"Story Drawing {i + 1}", loop.transform, slots[i].localPosition, slots[i].localEulerAngles.y);
                Kit.Drawing("Paper", root.transform, Vector3.zero, Vector3.zero, i + 1, 0.42f);
                var dc = root.AddComponent<DrawingCollectible>();
                Kit.Set(dc, "index", i + 1);
                Kit.Set(dc, "lineOnFound", lines[i]);
                drawings.Add(dc);
            }
            var hum = Kit.Sound(loop.transform, "Reset Hum", Vector3.zero, "SFX_memory_decay", spatial: false, volume: 0.7f);

            // ----- Beyond the loop (enabled when solved) -----
            var beyond = Kit.Go("Beyond", h);
            float z = 3 * M;
            Module(beyond.transform, z);                       // coffee module
            var vending = Kit.Model("Hospital", "Vending_machine_coffee", beyond.transform, new Vector3(0.75f, 0, z + 3f), -90f, height: 1.85f, rootName: "Coffee Machine");
            Kit.FitCollider(vending);
            var button = Kit.Block("Coffee Button", beyond.transform, new Vector3(0.28f, 1.3f, z + 2.8f), new Vector3(0.04f, 0.08f, 0.08f), new Color(0.9f, 0.85f, 0.4f), false);
            var coffee = Kit.Signal(button, grabbable: false);
            Kit.Sound(beyond.transform, "Pour", new Vector3(0.5f, 1f, z + 3f), "SFX_vending_coffee");
            var pastMachine = Kit.Trigger("Past Machine", beyond.transform, new Vector3(0, 1.2f, z + 5.5f), new Vector3(2.4f, 2.4f, 0.6f));
            z += M;
            Module(beyond.transform, z);                       // room 7 module
            Module(beyond.transform, z + M);
            var door = Kit.Model("Hospital", "Hospital_door", beyond.transform, new Vector3(-1.2f, 0, z + 3f), 90f, height: 2.1f, rootName: "Room 7 Door");
            // Door leaf: starts open (sticking into the corridor), swings shut in CS_05.
            var leafOpen = new Vector3(-0.67f, 1.05f, z + 2.5f);
            var leafShut = new Vector3(-1.17f, 1.05f, z + 3f);
            var leaf = Kit.Block("Door Leaf", beyond.transform, leafOpen, new Vector3(0.05f, 2.05f, 1f), new Color(0.88f, 0.9f, 0.88f), yaw: 90f);
            Kit.Text3D("Number", beyond.transform, new Vector3(-1.15f, 2.25f, z + 3f), "7", 2.5f, Kit.C("navy"), -90f, 0.4f);
            Kit.Dynamic(leaf);
            var nurse = Kit.Go("Nurse Voice", beyond.transform, new Vector3(0.6f, 1.6f, z + 5f));
            var approach = Kit.Trigger("Room 7 Approach", beyond.transform, new Vector3(0, 1.2f, z + 3f), new Vector3(2.4f, 2.4f, 2f));
            var clockRoot = Kit.Model("Classroom", "Wall_clock", beyond.transform, new Vector3(1.15f, 2.1f, z + 3f), -90f, height: 0.4f, rootName: "Clock");
            var hourHand = Kit.Block("Hour Hand", clockRoot.transform, new Vector3(-0.04f, 0.2f, 0), new Vector3(0.01f, 0.09f, 0.012f), "black", false);
            var minuteHand = Kit.Block("Minute Hand", clockRoot.transform, new Vector3(-0.045f, 0.2f, 0), new Vector3(0.008f, 0.14f, 0.01f), "black", false);
            var tick = Kit.Sound(clockRoot.transform, "Tick", Vector3.zero, "SFX_flip_tile", volume: 0.3f);
            var clock = clockRoot.AddComponent<HospitalClock>();
            Kit.Set(clock, "hourHand", hourHand.transform);
            Kit.Set(clock, "minuteHand", minuteHand.transform);
            Kit.Set(clock, "axis", Vector3.right);
            Kit.Set(clock, "tick", tick);
            var chair = Kit.Model("Hospital", "Waiting_chair", beyond.transform, new Vector3(0.8f, 0, z + 4.2f), -90f, height: 0.95f, rootName: "Chair");
            Kit.FitCollider(chair);
            var bracelet = Kit.Model("Hospital", "Patient_bracelet_leak", beyond.transform, new Vector3(0.8f, 0.5f, z + 4.2f), 0f, length: 0.07f, rootName: "Bracelet");
            bracelet.AddComponent<BoxCollider>().size = new Vector3(0.08f, 0.04f, 0.08f);
            var braceletSignal = Kit.Signal(bracelet, mass: 0.02f);
            Kit.Set(bracelet.AddComponent<LeakedObjectItem>(), "which", LeakedObject.Bracelet);
            Kit.Text3D("Name", bracelet.transform, new Vector3(0, 0.04f, 0), "REYES, TOMAS\nDOB 03/12", 0.25f, Kit.C("black"), 0f, 0.1f).transform.localRotation = Quaternion.Euler(90, 0, 0);
            bracelet.SetActive(false);
            var r7Host = Kit.Go("CS_05", beyond.transform);
            var room7 = Cut.New("CS_05_Room7", r7Host, 12f)
                .Move(leaf.transform, 2f, 1.5f, leafOpen, leafShut, new Vector3(0, 90, 0), Vector3.zero)
                .Sound("SFX_door_close_soft", 3.2f)
                .Sound("SFX_flatline", 5f)
                .Done();

            // The chase: 150 m of corridor, lights failing behind you on the beat.
            z += 2 * M;
            var chaseStart = Kit.Go("Pull Chase Start", beyond.transform, new Vector3(0, 0, z - 14f));
            var lights = new List<Light>();
            for (int i = 0; i < 25; i++)
            {
                Module(beyond.transform, z + i * M);
                if (i % 2 == 0) lights.Add(Kit.PointLight("Chase Light", beyond.transform, new Vector3(0, 2.5f, z + i * M + 3f), new Color(0.85f, 1f, 1f), 1.2f, 7f));
                if (i % 4 == 2)
                {
                    var c2 = Kit.Model("Hospital", "Waiting_chair", beyond.transform, new Vector3(-0.85f, 0, z + i * M + 3f), 90f, height: 0.95f, rootName: "Alcove Chair");
                    Kit.FitCollider(c2);
                }
            }
            var fail = Kit.Sound(beyond.transform, "Light Fail", Vector3.zero, "SFX_light_fail", volume: 0.8f);
            var chaseLights = beyond.AddComponent<CorridorLights>();
            Kit.Set(chaseLights, "lights", lights);
            Kit.Set(chaseLights, "failSound", fail);
            float end = z + 25 * M;
            Kit.Block("Way Out", beyond.transform, new Vector3(0, 1.35f, end + 0.3f), new Vector3(2.4f, 2.7f, 0.1f), Color.white, false)
                .GetComponent<Renderer>().sharedMaterial = Kit.Mat(Color.white, 2f);
            var fieldExit = Kit.Trigger("Field Exit", beyond.transform, new Vector3(0, 1.2f, end - 1f), new Vector3(2.4f, 2.4f, 1f));
            beyond.SetActive(false);

            var corridor = loop.AddComponent<CorridorLoop>();
            Kit.Set(corridor, "storyOrder", drawings);
            Kit.Set(corridor, "drawingSlots", slots);
            Kit.Set(corridor, "loopEndTrigger", loopEnd.GetComponent<Collider>());
            Kit.Set(corridor, "loopStart", loopStartPoint.transform);
            Kit.Set(corridor, "loopEnd", loopEndPoint.transform);
            Kit.Set(corridor, "exitModule", beyond);
            Kit.Set(corridor, "loopBlocker", blocker);
            Kit.Set(corridor, "resetHum", hum);
            Kit.OnEnter(loopEnd, corridor.OnReachedLoopEnd);

            var pull = Kit.Pull(h, new Vector3(0, 0, -30f), 0f);
            var memory = Kit.Memory(h.gameObject);
            var d = Kit.Director<Ch5HospitalDirector>(Chapter.LongHall, memory, pull, 0.5f, 0.3f);
            Kit.OnEnter(pastMachine, d.OnPastMachine);
            Kit.OnEnter(approach, d.OnRoom7);
            Kit.OnEnter(fieldExit, d.OnFieldExit);
            Kit.Set(d, "corridor", corridor);
            Kit.Set(d, "nurseVoice", nurse.transform);
            Kit.Set(d, "coffeeButton", coffee);
            Kit.Set(d, "pastMachineTrigger", pastMachine.GetComponent<Collider>());
            Kit.Set(d, "clock", clock);
            Kit.Set(d, "room7", room7);
            Kit.Set(d, "room7Approach", approach.GetComponent<Collider>());
            Kit.Set(d, "bracelet", braceletSignal);
            Kit.Set(d, "pullChaseStart", chaseStart.transform);
            Kit.Set(d, "chaseLights", chaseLights);
            Kit.Set(d, "fieldExit", fieldExit.GetComponent<Collider>());

            Kit.Save(scene, "Ch5_Hospital");
        }
    }
}
