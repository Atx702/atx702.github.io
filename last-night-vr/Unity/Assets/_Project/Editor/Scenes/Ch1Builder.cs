using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.Interaction;
using LastNight.World;
using UnityEngine;
using G = LastNight.EditorTools.Rooms.Gap;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch1_House.unity, "Too Far": the shy house 220 m away, the boat rebuilt on the kitchen table,
    /// the lake behind the back door, Elio's rescue (CS_01), the glove, the first Pull sighting.
    /// Everything that belongs to the house is a child of the house root, because ShyObject moves it.
    /// </summary>
    public static class Ch1Builder
    {
        public static void Build()
        {
            var scene = Kit.NewScene("Ch1_House");
            Kit.Atmosphere(Kit.Mood(new Color(0.85f, 0.81f, 0.72f), 12f, 170f, new Color(0.64f, 0.6f, 0.52f),
                new Color(1f, 0.9f, 0.75f), 0.75f, new Vector3(30, 210, 0)));
            var world = Kit.Go("World");
            Kit.Floor("Ground", world.transform, new Vector3(0, 0, 150), new Vector2(700, 700), new Color(0.5f, 0.52f, 0.4f));
            Kit.Bounds(world.transform, new Vector3(0, 0, 150), 330f);
            Kit.Tufts(world.transform, new Vector3(0, 0, 110), 3f, 160f, 350, new Color(0.42f, 0.46f, 0.33f), 21);
            Kit.Spawn(world.transform, Vector3.zero, 0f);
            Kit.Sound(world.transform, "Cicadas", Vector3.zero, "SFX_cicadas", loop: true, spatial: false, volume: 0.45f, playOnAwake: true);

            // ----- The house -----
            var house = Kit.Go("House", null, new Vector3(0, 0, 220));
            var h = house.transform;
            Color stucco = Kit.C("white"), wood = Kit.C("wood");
            Kit.Floor("House Floor", h, new Vector3(0, 0.03f, 5), new Vector2(12, 10), Kit.C("pine"));
            Rooms.Wall("Front", h, new Vector3(-6, 0, 0), new Vector3(6, 0, 0), 2.7f, stucco, gaps: new G(6f));
            Rooms.Wall("Back", h, new Vector3(-6, 0, 10), new Vector3(6, 0, 10), 2.7f, stucco, gaps: new G(9f));
            Rooms.Wall("West", h, new Vector3(-6, 0, 0), new Vector3(-6, 0, 10), 2.7f, stucco);
            Rooms.Wall("East", h, new Vector3(6, 0, 0), new Vector3(6, 0, 10), 2.7f, stucco);
            Rooms.Wall("Middle", h, new Vector3(0, 0, 0), new Vector3(0, 0, 6), 2.7f, stucco, gaps: new G(4.5f));
            Rooms.Wall("Back Room", h, new Vector3(-6, 0, 6), new Vector3(6, 0, 6), 2.7f, stucco, gaps: new[] { new G(4f), new G(8f) });
            Rooms.Roof(h, new Vector3(-6, 0, 0), new Vector2(12, 10), 2.7f, new Color(0.55f, 0.57f, 0.6f));
            Kit.Block("Front Door (open)", h, new Vector3(-0.5f, 1.05f, -0.5f), new Vector3(0.06f, 2.1f, 1.05f), "wrongblue");
            Kit.Floor("Porch", h, new Vector3(0, 0.06f, -1.2f), new Vector2(4, 2.4f), Kit.C("wood"));
            Kit.PointLight("Kitchen Light", h, new Vector3(-3, 2.4f, 3), new Color(1f, 0.92f, 0.78f), 1.2f, 7f);
            Kit.PointLight("Bedroom Light", h, new Vector3(3, 2.4f, 3), new Color(1f, 0.92f, 0.78f), 1f, 7f);
            var chimes = Kit.Model("House", "Wind_chimes", h, new Vector3(1.6f, 2.0f, -1.6f), 0f, height: 0.5f);
            Kit.Sound(chimes.transform, "Chimes", Vector3.zero, "SFX_wind_chimes", loop: true, volume: 0.6f, playOnAwake: true, maxDistance: 25f);
            var lucia = Kit.Go("Lucia (voice)", h, new Vector3(0, 1.6f, -0.8f));
            var porch = Kit.Trigger("Porch Trigger", h, new Vector3(0, 1f, -1.2f), new Vector3(3.5f, 2f, 2.4f));

            // Kitchen (west): table with three sockets, fridge with the sail drawing, hull under the table.
            Kit.Model("House", "Kitchen_table_and_bowl", h, new Vector3(-3f, 0.03f, 3f), 0f, length: 1.4f, rootName: "Kitchen Table");
            Kit.Block("Table Top (collider)", h, new Vector3(-3f, 0.76f, 3f), new Vector3(1.3f, 0.04f, 0.8f), "wood", true).GetComponent<Renderer>().enabled = false;
            var socketsRoot = Kit.Go("Boat Sockets", h, new Vector3(-3f, 0.86f, 3f));
            var sockets = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>
            {
                Kit.Socket("Hull Socket", socketsRoot.transform, new Vector3(-0.35f, 0, 0)),
                Kit.Socket("Mast Socket", socketsRoot.transform, new Vector3(0, 0, 0)),
                Kit.Socket("Sail Socket", socketsRoot.transform, new Vector3(0.35f, 0, 0)),
            };
            var boatParts = socketsRoot.AddComponent<SocketSet>();
            Kit.Set(boatParts, "sockets", sockets);
            Kit.Model("House", "Fridge", h, new Vector3(-5.4f, 0.03f, 5.3f), 90f, height: 1.7f);
            var sail = Kit.Drawing("Sail Drawing", h, new Vector3(-4.95f, 1.25f, 5.3f), new Vector3(0, -90, 0), 3, 0.28f);
            sail.AddComponent<BoxCollider>().size = new Vector3(1f, 1f, 0.05f);
            var sailSignal = Kit.Signal(sail, gravity: false);
            var hull = Kit.Block("Boat Hull", h, new Vector3(-3.1f, 0.1f, 3.3f), new Vector3(0.3f, 0.08f, 0.11f), "wrongblue", false);
            Kit.Signal(hull, mass: 0.3f);
            // Bedroom (east): child's bed, a shelf with the mast.
            Kit.Model("House", "Child_s_bed_and_shelf", h, new Vector3(3.8f, 0.03f, 2.2f), -90f, length: 1.9f);
            Kit.Block("Shelf", h, new Vector3(5.75f, 1.2f, 4.5f), new Vector3(0.35f, 0.04f, 0.9f), "wood");
            var mast = Kit.Shape(PrimitiveType.Cylinder, "Boat Mast", h, new Vector3(5.7f, 1.32f, 4.5f), new Vector3(0.02f, 0.09f, 0.02f), Kit.C("pine"));
            Kit.Signal(mast, mass: 0.1f);
            var assembled = Kit.Model("Prologue_Field", "Toy_sailboat", h, new Vector3(-3f, 0.9f, 3f), 0f, length: 0.4f, rootName: "Assembled Boat");
            Kit.Grabbable(assembled, 0.3f);
            assembled.SetActive(false);
            // Back room: back door to the lake.
            var backLocked = Kit.Block("Back Door (locked)", h, new Vector3(3f, 1.05f, 10f), new Vector3(1.1f, 2.1f, 0.08f), "wrongblue");
            var backOpen = Kit.Block("Back Door (open)", h, new Vector3(3.55f, 1.05f, 10.5f), new Vector3(0.08f, 2.1f, 1.05f), "wrongblue");
            backOpen.SetActive(false);

            // ----- The lake -----
            var lake = Kit.Go("Lake", h);
            var water = Kit.Block("Water", lake.transform, new Vector3(0, 0.02f, 58f), new Vector3(80f, 0.04f, 84f), Kit.C("water"));
            var shore = Kit.Go("Shore Barrier", lake.transform);
            Kit.Add<BoxCollider>(Kit.Go("Shore West", shore.transform, new Vector3(-19f, 1f, 16f))).size = new Vector3(42f, 2f, 0.3f);
            Kit.Add<BoxCollider>(Kit.Go("Shore East", shore.transform, new Vector3(23f, 1f, 16f))).size = new Vector3(38f, 2f, 0.3f);
            Kit.Floor("Path", h, new Vector3(3f, 0.04f, 13f), new Vector2(1.4f, 6f), Kit.C("pine") * 0.9f);
            var dock = Kit.Go("Dock", lake.transform);
            Kit.Block("Deck", dock.transform, new Vector3(3f, 0.3f, 22f), new Vector3(2f, 0.12f, 12f), new Color(0.55f, 0.55f, 0.52f));
            for (int i = 0; i < 6; i++)
                for (int s = -1; s <= 1; s += 2)
                    Kit.Block("Post", dock.transform, new Vector3(3f + s * 0.9f, 0.1f, 16.5f + i * 2.2f), new Vector3(0.15f, 0.6f, 0.15f), "darkwood", false);
            Kit.Add<BoxCollider>(Kit.Go("Dock Rail West", dock.transform, new Vector3(1.95f, 1f, 22f))).size = new Vector3(0.1f, 2f, 12f);
            Kit.Add<BoxCollider>(Kit.Go("Dock Rail East", dock.transform, new Vector3(4.05f, 1f, 22f))).size = new Vector3(0.1f, 2f, 12f);
            // Low enough to drop the boat over, too high to step over.
            Kit.Add<BoxCollider>(Kit.Go("Dock End", dock.transform, new Vector3(3f, 0.7f, 28.6f))).size = new Vector3(2f, 0.7f, 0.1f);
            var waterZone = Kit.Trigger("Water Placement Zone", lake.transform, new Vector3(3f, 0.2f, 30.2f), new Vector3(4f, 1.6f, 3.6f));
            Kit.Set(waterZone, "requiredObject", assembled);
            var leaveDock = Kit.Trigger("Leave Dock", lake.transform, new Vector3(3f, 1f, 15.6f), new Vector3(2.5f, 2f, 1f));
            var glove = Kit.Model("House", "Medical_glove_leak", lake.transform, new Vector3(3.3f, 0.38f, 27.6f), 40f, length: 0.25f, rootName: "Glove");
            var gloveSignal = Kit.Signal(glove, mass: 0.05f);
            Kit.Set(glove.AddComponent<LeakedObjectItem>(), "which", LeakedObject.Glove);

            // CS_01: the water rises past your eyes, Elio comes, the water falls.
            var rising = Kit.Block("Rising Water", lake.transform, new Vector3(3f, -0.4f, 22f), new Vector3(30f, 0.06f, 30f), new Color(0.2f, 0.3f, 0.36f), false);
            var elioPuppet = Kit.Character("Elio", lake.transform, new Vector3(3f, 0.36f, 34f), 180f, lookAtPlayer: 0.7f, rootName: "Elio");
            var elio = elioPuppet.gameObject;
            elio.SetActive(false);
            var under = Kit.Sound(lake.transform, "Underwater", new Vector3(3f, 1.5f, 27f), null, spatial: false);
            var lakeMemory = Kit.Memory(lake);
            var lakeCut = Cut.New("CS_01_Lake", lake, 40f)
                .Move(rising.transform, 0f, 8f, new Vector3(3f, -0.4f, 22f), new Vector3(3f, 2.6f, 22f))
                .Move(rising.transform, 26f, 8f, new Vector3(3f, 2.6f, 22f), new Vector3(3f, -0.4f, 22f))
                .Active(elio, 14f, 26f)
                .Move(elio.transform, 14f, 7f, new Vector3(3f, 0.36f, 34f), new Vector3(3f, 0.36f, 28.9f))
                .Pose(elioPuppet, 14f, 7f, Characters.PuppetAnimator.Move.Walk)
                .Pose(elioPuppet, 21f, 19f, Characters.PuppetAnimator.Move.ReachDown)
                .Sound("SFX_underwater", 2f, under, 0.8f)
                .Sound("SFX_gasp", 30f)
                .Done();

            // Exit: a doorway standing in the grass beside the house.
            var exitDoor = Kit.Model("Prologue_Field", "Standalone_door_in_field", h, new Vector3(-12f, 0, 14f), 90f, height: 2.1f, rootName: "Exit Door");
            Kit.Beacon("Exit Glow", exitDoor.transform, Vector3.zero, Kit.C("wrongblue"), 6f, 0.5f);
            var exit = Kit.Trigger("Exit Trigger", h, new Vector3(-12f, 1f, 14f), new Vector3(2.5f, 2f, 2.5f));

            var pull = Kit.Pull(world.transform, new Vector3(3f, 0, 360f), 180f);
            var horizon = Kit.Go("Pull Horizon Point", h, new Vector3(3f, 0, 120f), 180f);

            // ----- Director -----
            Kit.Dynamic(house); // ShyObject moves the whole house, so nothing in it may be statically batched
            var shy = house.AddComponent<ShyObject>();
            var memory = Kit.Memory(house);
            var d = Kit.Director<Ch1HouseDirector>(Chapter.TooFar, memory, pull, 0f, 0f);
            Kit.On(shy, "onFirstRetreat", d.OnHouseRetreated);
            Kit.On(shy, "onFirstEyesClosedApproach", d.OnHouseEyesClosedApproach);
            Kit.On(shy, "onArrived", d.OnHouseArrived);
            Kit.OnEnter(porch, d.OnPorch);
            Kit.OnEnter(waterZone, d.OnBoatInWater);
            Kit.OnEnter(leaveDock, d.OnLeftDock);
            Kit.OnEnter(exit, d.OnExit);
            Kit.Set(d, "house", shy);
            Kit.Set(d, "lucia", lucia.transform);
            Kit.Set(d, "porchTrigger", porch.GetComponent<Collider>());
            Kit.Set(d, "boatParts", boatParts);
            Kit.Set(d, "sailDrawing", sailSignal);
            Kit.Set(d, "assembledBoat", assembled);
            Kit.Set(d, "boatPartObjects", new List<Object> { hull, mast, sail });
            Kit.Set(d, "backDoorLocked", backLocked);
            Kit.Set(d, "backDoorOpen", backOpen);
            Kit.Set(d, "waterPlacementZone", waterZone.GetComponent<Collider>());
            Kit.Set(d, "lakeTimeline", lakeCut);
            Kit.Set(d, "elio", elio.transform);
            Kit.Set(d, "lakeMemory", lakeMemory);
            Kit.Set(d, "glove", gloveSignal);
            Kit.Set(d, "pullHorizonPoint", horizon.transform);
            Kit.Set(d, "leaveDockTrigger", leaveDock.GetComponent<Collider>());
            Kit.Set(d, "chapterExitTrigger", exit.GetComponent<Collider>());

            var collectible = Kit.Go("Drawing in the bedroom", h, new Vector3(5.92f, 1.5f, 1.5f), 90f);
            Kit.Drawing("Paper", collectible.transform, Vector3.zero, Vector3.zero, 11, 0.3f);
            Kit.Set(collectible.AddComponent<LastNight.World.DrawingCollectible>(), "index", 11);
            Kit.Save(scene, "Ch1_House");
        }
    }
}
