using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.Player;
using LastNight.World;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch3_Station.unity, "Line 4": an 80 x 8 m platform over fog. Find the four timetable tiles, the train
    /// brings young Maren, catch her scarf in the gust, chase the train, the bench, the real scarf.
    /// The track side (z &lt; 0) drops into fog.
    /// </summary>
    public static class Ch3Builder
    {
        public static void Build()
        {
            var scene = Kit.NewScene("Ch3_Station");
            var fog = new Color(0.7f, 0.73f, 0.77f);
            Kit.Atmosphere(Kit.Mood(fog, 6f, 70f, new Color(0.55f, 0.57f, 0.62f), new Color(0.9f, 0.93f, 1f), 0.5f, new Vector3(25, 120, 0)));
            var st = Kit.Go("Station").transform;
            Kit.Block("Platform", st, new Vector3(0, -0.25f, 4f), new Vector3(80f, 0.5f, 8f), "platform");
            Kit.Block("Edge Line", st, new Vector3(0, 0.005f, 0.4f), new Vector3(80f, 0.01f, 0.12f), "yellow", false);
            Kit.Block("Back Wall", st, new Vector3(0, 1.5f, 8.1f), new Vector3(80f, 3f, 0.2f), new Color(0.52f, 0.54f, 0.56f));
            Kit.Add<BoxCollider>(Kit.Go("Edge Barrier", st, new Vector3(0, 1f, -0.05f))).size = new Vector3(80f, 2f, 0.1f);
            Kit.Add<BoxCollider>(Kit.Go("End Barrier", st, new Vector3(40.1f, 1f, 4f))).size = new Vector3(0.2f, 2f, 8f);
            for (int i = 0; i < 9; i++) Kit.Block("Sleeper", st, new Vector3(-40 + i * 10, -1.6f, -2.2f), new Vector3(0.3f, 0.15f, 3f), "darkwood", false);
            Kit.Spawn(st, new Vector3(-2f, 0, 3f), 0f);
            Kit.Sound(st, "Wind", Vector3.zero, "SFX_wind_field", loop: true, spatial: false, volume: 0.35f, playOnAwake: true);

            var lights = new List<Light>();
            for (int i = 0; i < 6; i++)
            {
                Kit.Block("Lamp Post", st, new Vector3(-30 + i * 12, 1.6f, 7.4f), new Vector3(0.1f, 3.2f, 0.1f), "darksteel", false);
                lights.Add(Kit.PointLight("Platform Light", st, new Vector3(-30 + i * 12, 3.1f, 6.8f), new Color(1f, 0.95f, 0.85f), 1.3f, 10f));
            }

            // Props and the four hidden tiles.
            var shelter = Kit.Model("Station", "Platform_shelter", st, new Vector3(-10f, 0, 5.8f), 180f, length: 6f, rootName: "Shelter");
            Kit.FitCollider(shelter);
            var bench = Kit.Model("Station", "Bench", st, new Vector3(9f, 0, 6.9f), 180f, length: 1.8f, rootName: "Bench");
            Kit.FitCollider(bench);
            var benchLight = Kit.PointLight("Bench Light", st, new Vector3(9f, 2.6f, 6.6f), new Color(1f, 0.85f, 0.6f), 1.8f, 6f);
            benchLight.enabled = false;
            var vending = Kit.Model("Station", "Vending_machine", st, new Vector3(-20f, 0, 7.4f), 180f, height: 1.85f, rootName: "Vending Machine");
            Kit.FitCollider(vending);
            var bin = Kit.Model("Station", "Lost_property_bin", st, new Vector3(20f, 0, 7.4f), 180f, height: 0.9f, rootName: "Lost Property");
            var clockPost = Kit.Block("Clock Post", st, new Vector3(35f, 1.6f, 6.5f), new Vector3(0.12f, 3.2f, 0.12f), "darksteel");
            Kit.Model("Classroom", "Wall_clock", st, new Vector3(35f, 3.0f, 6.4f), 180f, height: 0.5f);

            var board = Kit.Model("Station", "Flip_timetable_board", st, new Vector3(0f, 1.8f, 7.85f), 180f, length: 2.4f, rootName: "Timetable");
            Kit.Text3D("Board Label", st, new Vector3(0f, 2.95f, 7.6f), "LINE 4", 1.6f, Color.white, 0f, 2f);
            var socketRoot = Kit.Go("Tile Sockets", st, new Vector3(0f, 2.35f, 7.5f));
            var sockets = new List<XRSocketInteractor>();
            for (int i = 0; i < 4; i++) sockets.Add(Kit.Socket($"Tile Socket {i + 1}", socketRoot.transform, new Vector3(-0.45f + i * 0.3f, 0, 0), 0.1f));
            var tiles = Kit.Go("Tiles", st);
            (string text, Vector3 pos)[] tileSpots =
            {
                ("7", new Vector3(-19.7f, 0.45f, 6.9f)),   // vending tray
                (":", new Vector3(9f, 0.08f, 7f)),          // under the bench
                ("4", new Vector3(-10f, 3.1f, 6.2f)),      // on the shelter roof
                ("0", new Vector3(20f, 0.7f, 7.4f)),       // in the lost-property bin
            };
            foreach (var (text, pos) in tileSpots)
            {
                var tile = Kit.Block("Tile " + text, tiles.transform, pos, new Vector3(0.22f, 0.3f, 0.03f), "black", false);
                Kit.Text3D("Digit", tile.transform, new Vector3(0, 0, -0.6f), text, 20f, Color.white, 0f, 1f).transform.localScale = new Vector3(1 / 0.22f, 1 / 0.3f, 1) * 0.05f;
                Kit.Grabbable(tile, 0.2f, gravity: false);
            }
            var tileSet = socketRoot.AddComponent<Interaction.SocketSet>();
            Kit.Set(tileSet, "sockets", sockets);
            var tileSounds = new List<AudioSource>
            {
                Kit.Sound(st, "Tile Memory 1", new Vector3(0, 2, 7), "SFX_wind_chimes"),
                Kit.Sound(st, "Tile Memory 2", new Vector3(0, 2, 7), "SFX_vending_coffee"),
                Kit.Sound(st, "Tile Memory 3", new Vector3(0, 2, 7), "SFX_chalk"),
                Kit.Sound(st, "Tile Memory 4", new Vector3(0, 2, 7), "SFX_flip_tile"),
            };

            // The train and young Maren.
            var train = Kit.Model("Station", "Train_car", st, new Vector3(-90f, -1.4f, -2.3f), 90f, length: 16f, rootName: "Train");
            var window = Kit.Go("Train Window", train.transform, new Vector3(0, 2.6f, 1.6f));
            var marenInTrain = Kit.Character("Maren_age_22", train.transform, new Vector3(0, 1.0f, 0.5f), 0f, Characters.PuppetAnimator.Move.Wave, 0.8f, "Maren In Window").gameObject;
            marenInTrain.SetActive(false);
            var marenPuppet = Kit.Character("Maren_age_22", st, new Vector3(1f, 0, -0.6f), 0f, Characters.PuppetAnimator.Move.Carry, 0.7f, "Maren (memory)");
            var maren = marenPuppet.gameObject;
            maren.SetActive(false);
            var arrive = Cut.New("CS_03_TrainArrives", train, 12f)
                .Move(train.transform, 0f, 9f, new Vector3(-90f, -1.4f, -2.3f), new Vector3(0f, -1.4f, -2.3f))
                .Active(maren, 9f, 3f)
                .Move(maren.transform, 9f, 3f, new Vector3(1f, 0, -0.6f), new Vector3(1f, 0, 2.2f))
                .Sound("SFX_train_arrive", 0f)
                .Done();
            var departHost = Kit.Go("Train Departs", st);
            var depart = Cut.New("CS_03_TrainDeparts", departHost, 22f)
                .Move(maren.transform, 0f, 1.5f, new Vector3(1f, 0, 2.2f), new Vector3(1f, 0, -0.6f))
                .Active(maren, 0f, 1.5f)
                .Active(marenInTrain, 1.5f, 20.5f)
                .Move(train.transform, 2f, 20f, new Vector3(0f, -1.4f, -2.3f), new Vector3(50f, -1.4f, -2.3f), ease: false)
                .Done();

            // The scarf and the gust.
            var scarf = Kit.Model("Station", "Red_scarf", st, new Vector3(1f, 1.45f, 2.3f), 0f, length: 1.2f, rootName: "Scarf (memory)");
            var scarfSignal = Kit.Signal(scarf, mass: 0.1f, kinematic: true);
            var scarfBody = scarf.GetComponent<Rigidbody>();
            scarfBody.linearDamping = 2.5f;
            scarfBody.angularDamping = 2f;
            var respawn = Kit.Go("Scarf Respawn", st, new Vector3(1f, 2.2f, 1.5f));
            var gust = Kit.Go("Gust", st, new Vector3(1f, 1.5f, 2f));
            var wind = gust.AddComponent<WindZone>();
            wind.mode = WindZoneMode.Directional;
            var push = gust.AddComponent<WindPush>();
            Kit.Set(push, "bodies", new List<Object> { scarfBody });
            gust.SetActive(false);

            var realScarf = Kit.Model("Station", "Red_scarf", st, new Vector3(9f, 0.5f, 6.8f), 0f, length: 1.0f, rootName: "Real Scarf");
            foreach (var rend in realScarf.GetComponentsInChildren<Renderer>()) rend.sharedMaterial = Kit.Mat(new Color(0.4f, 0.1f, 0.1f));
            var realSignal = Kit.Signal(realScarf, mass: 0.1f);
            Kit.Set(realScarf.AddComponent<LeakedObjectItem>(), "which", LeakedObject.Scarf);
            realScarf.SetActive(false);

            var exit = Kit.Trigger("Exit Stairs", st, new Vector3(-39f, 1f, 4f), new Vector3(2f, 2f, 7f));
            Kit.Beacon("Exit Glow", st, new Vector3(-39.5f, 0, 4f), new Color(1f, 0.7f, 0.65f), 4f, 0.4f);
            var pull = Kit.Pull(st, new Vector3(35f, 0, 4f), -90f);
            var underClock = Kit.Go("Pull Under Clock", st, new Vector3(35f, 0, 4.5f), -90f);

            var memory = Kit.Memory(st.gameObject);
            var d = Kit.Director<Ch3StationDirector>(Chapter.Line4, memory, pull, 0f, 0f);
            Kit.On<int>(tileSet, "onEachFilled", d.OnTileFilled);
            Kit.OnEnter(exit, d.OnExit);
            Kit.Set(d, "timetableTiles", tileSet);
            Kit.Set(d, "tileMemorySounds", tileSounds);
            Kit.Set(d, "trainArrives", arrive);
            Kit.Set(d, "marenMem", maren.transform);
            Kit.Set(d, "marenPuppet", marenPuppet);
            Kit.Set(d, "scarf", scarfBody);
            Kit.Set(d, "scarfGrab", scarfSignal);
            Kit.Set(d, "gust", wind);
            Kit.Set(d, "scarfFallY", -1.5f);
            Kit.Set(d, "scarfRespawn", respawn.transform);
            Kit.Set(d, "trainDeparts", depart);
            Kit.Set(d, "trainWindow", window.transform);
            Kit.Set(d, "waveAtTrain", Kit.Gesture(d.transform, GestureDetector.Kind.Wave, d.OnWaved));
            Kit.Set(d, "bench", bench.transform);
            Kit.Set(d, "benchLight", benchLight);
            Kit.Set(d, "platformLights", lights);
            Kit.Set(d, "realScarf", realSignal);
            Kit.Set(d, "pullUnderClock", underClock.transform);
            Kit.Set(d, "exitTrigger", exit.GetComponent<Collider>());

            var collectible = Kit.Go("Drawing in the shelter", st, new Vector3(-10f, 1.3f, 7.95f), 0f);
            Kit.Drawing("Paper", collectible.transform, Vector3.zero, Vector3.zero, 9, 0.3f);
            Kit.Set(collectible.AddComponent<LastNight.World.DrawingCollectible>(), "index", 9);
            Kit.Save(scene, "Ch3_Station");
        }
    }
}
