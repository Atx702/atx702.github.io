using LastNight.Core;
using LastNight.World;
using UnityEngine;
using G = LastNight.EditorTools.Rooms.Gap;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch7_Bedroom.unity, "3:12 AM": the bedroom at night. A window in the floor looks down three metres
    /// into the real room: Tomas on the floor, Maren doing CPR (slowed to 30%), the paramedics arriving (CS_07).
    /// The Pull fills the doorway and doesn't move. After the last line, the closet opens.
    /// </summary>
    public static class Ch7Builder
    {
        public static void Build()
        {
            var scene = Kit.NewScene("Ch7_Bedroom");
            var night = new Color(0.1f, 0.11f, 0.15f);
            Kit.Atmosphere(Kit.Mood(night, 4f, 22f, new Color(0.2f, 0.22f, 0.3f), new Color(0.55f, 0.62f, 0.85f), 0.3f, new Vector3(40, 200, 0), 30f));
            var b = Kit.Go("Bedroom").transform;
            var wall = new Color(0.36f, 0.4f, 0.48f);
            // Room x -2..2, z 0..5; doorway on the south wall, closet on the east wall.
            Rooms.Wall("South", b, new Vector3(-2, 0, 0), new Vector3(2, 0, 0), 2.6f, wall, gaps: new G(1f, 0.9f));
            Rooms.Wall("North", b, new Vector3(-2, 0, 5), new Vector3(2, 0, 5), 2.6f, wall);
            Rooms.Wall("West", b, new Vector3(-2, 0, 0), new Vector3(-2, 0, 5), 2.6f, wall);
            Rooms.Wall("East", b, new Vector3(2, 0, 0), new Vector3(2, 0, 5), 2.6f, wall, gaps: new G(3.6f, 0.9f));
            Kit.Block("Ceiling", b, new Vector3(0, 2.65f, 2.5f), new Vector3(4.3f, 0.1f, 5.3f), wall * 0.9f, false);
            // Floor around a 1.6 x 1.2 m window (x -0.8..0.8, z 1.9..3.1); an invisible pane keeps you up.
            var carpet = Kit.C("carpet");
            Kit.Block("Floor W", b, new Vector3(-1.4f, -0.05f, 2.5f), new Vector3(1.2f, 0.1f, 5f), carpet);
            Kit.Block("Floor E", b, new Vector3(1.4f, -0.05f, 2.5f), new Vector3(1.2f, 0.1f, 5f), carpet);
            Kit.Block("Floor S", b, new Vector3(0, -0.05f, 0.95f), new Vector3(1.6f, 0.1f, 1.9f), carpet);
            Kit.Block("Floor N", b, new Vector3(0, -0.05f, 4.05f), new Vector3(1.6f, 0.1f, 1.9f), carpet);
            Kit.Add<BoxCollider>(Kit.Go("Window Pane", b, new Vector3(0, -0.05f, 2.5f))).size = new Vector3(1.6f, 0.1f, 1.2f);
            foreach (var (pos, size) in new[] { (new Vector3(0, 0.01f, 1.88f), new Vector3(1.7f, 0.03f, 0.05f)), (new Vector3(0, 0.01f, 3.12f), new Vector3(1.7f, 0.03f, 0.05f)),
                                                (new Vector3(-0.82f, 0.01f, 2.5f), new Vector3(0.05f, 0.03f, 1.3f)), (new Vector3(0.82f, 0.01f, 2.5f), new Vector3(0.05f, 0.03f, 1.3f)) })
                Kit.Block("Window Frame", b, pos, size, new Color(0.75f, 0.8f, 0.9f), false).GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(0.6f, 0.7f, 0.9f), 0.8f);
            var floorWindow = Kit.Go("Floor Window", b, new Vector3(0, 0, 2.5f));
            Kit.Spawn(b, new Vector3(-1.2f, 0, 1.2f), 30f);

            var bed = Kit.Model("Prologue_Field", "Old_double_bed", b, new Vector3(-1.0f, 0, 4.0f), 0f, length: 2.1f, rootName: "Bed");
            Kit.Model("Prologue_Field", "Nightstand_and_digital_clock", b, new Vector3(-1.75f, 0, 4.6f), 90f, height: 0.6f);
            Kit.Model("Bedroom_Wren_s_room", "Dresser", b, new Vector3(1.6f, 0, 1.0f), -90f, length: 1.2f);
            Kit.Block("Moon Window", b, new Vector3(0.8f, 1.6f, 4.98f), new Vector3(1f, 1.1f, 0.02f), Color.white, false).GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(0.5f, 0.58f, 0.8f), 1.5f);
            Kit.PointLight("Moonlight", b, new Vector3(0.6f, 2f, 4.2f), new Color(0.55f, 0.62f, 0.9f), 0.8f, 6f);
            Kit.Add<BoxCollider>(Kit.Go("Doorway Block", b, new Vector3(-1f, 1.2f, -0.3f))).size = new Vector3(1f, 2.4f, 0.3f);

            // Closet: closed door, then open with the way out behind it.
            var closetClosed = Kit.Block("Closet Door (closed)", b, new Vector3(2f, 1.05f, 3.6f), new Vector3(0.06f, 2.1f, 0.9f), "darkwood");
            var closetOpen = Kit.Go("Closet Door (open)", b);
            Kit.Block("Door Leaf", closetOpen.transform, new Vector3(2.4f, 1.05f, 3.15f), new Vector3(0.9f, 2.1f, 0.06f), "darkwood");
            Kit.Block("Light Inside", closetOpen.transform, new Vector3(2.9f, 1.2f, 3.6f), new Vector3(0.05f, 2.2f, 0.9f), Color.white, false)
                .GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(1f, 0.95f, 0.8f), 2.5f);
            Kit.Floor("Closet Floor", closetOpen.transform, new Vector3(2.5f, 0, 3.6f), new Vector2(1f, 1f), carpet);
            var exitGo = Kit.Go("Closet Exit", closetOpen.transform, new Vector3(2.55f, 1f, 3.6f));
            var exitBox = exitGo.AddComponent<BoxCollider>();
            exitBox.isTrigger = true;
            exitBox.size = new Vector3(0.6f, 2f, 0.8f);
            closetOpen.SetActive(false);

            // ----- The real room, three metres down -----
            var real = Kit.Go("Real Room", b, new Vector3(0, -3.2f, 2.5f));
            Kit.Block("Real Floor", real.transform, new Vector3(0, -0.05f, 0), new Vector3(5f, 0.1f, 5f), new Color(0.42f, 0.36f, 0.32f), false);
            Kit.PointLight("Real Lamp", real.transform, new Vector3(1.2f, 1.6f, 1f), new Color(1f, 0.85f, 0.65f), 1.6f, 6f);
            Kit.PointLight("Real Cold", real.transform, new Vector3(-1f, 2f, -1f), new Color(0.7f, 0.8f, 1f), 0.8f, 6f);
            // Tomas lies on his back, head toward -z; his chest is around z -0.4.
            var tomasPuppet = Kit.Character("Tomas", real.transform, new Vector3(-0.1f, 0.15f, 0.9f), 0f, Characters.PuppetAnimator.Move.Still, 0f, "Tomas (body)");
            tomasPuppet.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var marenPuppet = Kit.Character("Maren_tonight", real.transform, new Vector3(0.42f, 0, -0.4f), -90f, Characters.PuppetAnimator.Move.Cpr, 0f, "Maren (CPR)");
            var okaforPuppet = Kit.Character("Okafor", real.transform, new Vector3(-2.2f, 0, -2f), 30f, Characters.PuppetAnimator.Move.Idle, 0f, "Okafor");
            var lindPuppet = Kit.Character("Lind", real.transform, new Vector3(-2.2f, 0, -2.4f), 30f, Characters.PuppetAnimator.Move.Carry, 0f, "Lind");
            var maren = marenPuppet.gameObject;
            var okafor = okaforPuppet.gameObject;
            var lind = lindPuppet.gameObject;
            var defib = Kit.Model("Bedroom_Wren_s_room", "Defibrillator_monitor", real.transform, new Vector3(-0.6f, 0, -0.3f), 0f, length: 0.5f, rootName: "Defibrillator");
            var bag = Kit.Model("Bedroom_Wren_s_room", "Paramedic_bag", real.transform, new Vector3(-1.1f, 0, 0.4f), 20f, length: 0.5f, rootName: "Bag");
            okafor.SetActive(false); lind.SetActive(false); defib.SetActive(false); bag.SetActive(false);
            var marenSpeaker = Kit.Go("Maren Speaker", real.transform, new Vector3(0.45f, 1.0f, 0.05f));
            var okaforSpeaker = Kit.Go("Okafor Speaker", real.transform, new Vector3(-0.4f, 1.4f, -0.5f));
            var monitor = Kit.Sound(real.transform, "Monitor", Vector3.zero, "SFX_monitor_beep", volume: 0.5f);
            // CPR at 110 BPM slowed to 30%: about 0.55 compressions a second.
            var cut = Cut.New("CS_07_TheFloor", real, 150f)
                // CPR at 110 BPM slowed to 30%. At 58 s Okafor takes over and Maren falls back.
                .Pose(marenPuppet, 0f, 58f, Characters.PuppetAnimator.Move.Cpr, 0.55f)
                .Pose(marenPuppet, 58f, 92f, Characters.PuppetAnimator.Move.HandsOverMouth)
                .Move(maren.transform, 58f, 2f, new Vector3(0.42f, 0, -0.4f), new Vector3(0.95f, 0, -0.2f), new Vector3(0, -90, 0), new Vector3(0, -110, 0))
                .Active(okafor, 40f, 110f)
                .Move(okafor.transform, 40f, 7f, new Vector3(-2.2f, 0, -2f), new Vector3(-0.42f, 0, -0.4f), new Vector3(0, 30, 0), new Vector3(0, 90, 0))
                .Pose(okaforPuppet, 40f, 7f, Characters.PuppetAnimator.Move.Walk)
                .Pose(okaforPuppet, 47f, 11f, Characters.PuppetAnimator.Move.Kneel)
                .Pose(okaforPuppet, 58f, 92f, Characters.PuppetAnimator.Move.Cpr, 0.55f)
                .Active(lind, 46f, 104f)
                .Move(lind.transform, 46f, 7f, new Vector3(-2.2f, 0, -2.4f), new Vector3(-0.1f, 0, -1.45f), new Vector3(0, 30, 0), new Vector3(0, 0, 0))
                .Pose(lindPuppet, 46f, 7f, Characters.PuppetAnimator.Move.Carry)
                .Pose(lindPuppet, 53f, 97f, Characters.PuppetAnimator.Move.BagValve)
                .Active(bag, 44f, 106f)
                .Active(defib, 52f, 98f)
                .Sound("SFX_defib_charge", 70f, monitor)
                .Sound("SFX_defib_shock", 73f)
                .Done();

            // The Pull stands in the doorway, shaped like Maren.
            var pull = Kit.Pull(b, new Vector3(-1f, 0, -0.6f), 0f);
            var doorway = Kit.Go("Pull Doorway", b, new Vector3(-1f, 0, -0.6f), 0f);

            var memory = Kit.Memory(b.gameObject);
            var exit = exitGo.AddComponent<ChapterExit>();
            Kit.Set(exit, "chapter", Chapter.Bedroom);
            Kit.Set(exit, "decayOnExit", memory);
            Kit.Set(exit, "delay", 3f);

            var reveal = Kit.Go("Reveal Director").AddComponent<RevealDirector>();
            Kit.Set(reveal, "floorWindow", floorWindow.transform);
            Kit.Set(reveal, "realWorldScene", cut);
            Kit.Set(reveal, "pull", pull);
            Kit.Set(reveal, "pullDoorwayPoint", doorway.transform);
            Kit.Set(reveal, "closetDoorClosed", closetClosed);
            Kit.Set(reveal, "closetDoorOpen", closetOpen);
            Kit.Set(reveal, "marenSpeaker", marenSpeaker.transform);
            Kit.Set(reveal, "okaforSpeaker", okaforSpeaker.transform);

            Kit.Save(scene, "Ch7_Bedroom");
        }
    }
}
