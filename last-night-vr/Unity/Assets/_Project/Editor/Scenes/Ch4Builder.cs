using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.Interaction;
using LastNight.World;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using G = LastNight.EditorTools.Rooms.Gap;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Ch4_Kitchen.unity, "Yellow Kitchen": a 5 x 4 m kitchen floating in black. Tea, the radio, the
    /// third mug (CS_04A), "it found us" (CS_04B) and three minutes of pull-chain-lamp stealth, the ring.
    /// </summary>
    public static class Ch4Builder
    {
        public static void Build()
        {
            var scene = Kit.NewScene("Ch4_Kitchen");
            Kit.Atmosphere(Kit.Mood(Color.black, 2.5f, 11f, new Color(0.24f, 0.2f, 0.15f), Color.white, 0f, Vector3.zero, 14f));
            var k = Kit.Go("Kitchen").transform;
            var yellow = Kit.C("warmwall");
            Rooms.Box("Room", k, new Vector3(-2.5f, 0, 0), new Vector2(5, 4), 2.6f, yellow, Kit.C("linoleum") * 0.9f,
                south: new[] { new G(2.5f, 1f) }, west: new[] { new G(2f, 1f) }, ceilingColor: new Color(0.9f, 0.86f, 0.75f));
            Kit.Floor("Hall", k, new Vector3(-3.5f, 0, 2f), new Vector2(2f, 1.2f), Color.black);
            Kit.Floor("Door Step", k, new Vector3(0, 0, -0.8f), new Vector2(1.2f, 1.6f), Color.black);
            Kit.Add<BoxCollider>(Kit.Go("Hall End", k, new Vector3(-4.6f, 1f, 2f))).size = new Vector3(0.2f, 2f, 1.2f);
            Kit.Spawn(k, new Vector3(0, 0, 1.2f), 0f);

            // Counter, sink tap, stove socket.
            var counter = Kit.Model("Kitchen", "Kitchen_counter_set", k, new Vector3(0, 0, 3.62f), 180f, length: 2.4f, rootName: "Counter");
            Kit.Block("Counter Top (collider)", k, new Vector3(0, 0.88f, 3.62f), new Vector3(2.4f, 0.06f, 0.6f), "wood").GetComponent<Renderer>().enabled = false;
            var tap = Kit.Go("Tap Stream", k, new Vector3(-0.5f, 1.05f, 3.55f));
            var tapCol = tap.AddComponent<BoxCollider>();
            tapCol.isTrigger = true;
            tapCol.size = new Vector3(0.12f, 0.25f, 0.12f);
            var stove = Kit.Socket("Stove", k, new Vector3(0.65f, 0.98f, 3.55f), 0.14f);

            // Kettle.
            var kettle = Kit.Model("Kitchen", "Kettle", k, new Vector3(0.1f, 0.92f, 3.45f), 0f, length: 0.3f, rootName: "Kettle");
            Kit.Grabbable(kettle, 0.6f);
            var steam = Kit.Go("Steam", kettle.transform, new Vector3(0.12f, 0.2f, 0)).AddComponent<ParticleSystem>();
            var main = steam.main;
            main.startLifetime = 1.5f; main.startSpeed = 0.3f; main.startSize = 0.08f; main.playOnAwake = false;
            main.startColor = new Color(1f, 1f, 1f, 0.35f);
            steam.GetComponent<ParticleSystemRenderer>().sharedMaterial = Kit.Mat(Color.white, 0.4f);
            var boil = Kit.Sound(kettle.transform, "Boil", Vector3.zero, "SFX_underwater", loop: true, volume: 0.1f);
            var whistle = Kit.Sound(kettle.transform, "Whistle", Vector3.zero, "SFX_kettle_whistle", volume: 0.9f);
            var kk = kettle.AddComponent<KitchenKettle>();
            Kit.Set(kk, "stoveSocket", stove);
            Kit.Set(kk, "tapStream", tapCol);
            Kit.Set(kk, "steam", steam);
            Kit.Set(kk, "boilLoop", boil);
            Kit.Set(kk, "whistle", whistle);
            Kit.Set(kk, "shakeVisual", kettle.transform.GetChild(0));

            // Table, mugs, honey.
            Kit.Block("Table", k, new Vector3(0, 0.37f, 1.9f), new Vector3(1f, 0.74f, 0.7f), "wood");
            var mugSockets = new List<XRSocketInteractor>
            {
                Kit.Socket("Mug Spot 1", k, new Vector3(-0.22f, 0.8f, 1.9f), 0.09f),
                Kit.Socket("Mug Spot 2", k, new Vector3(0.22f, 0.8f, 1.9f), 0.09f),
            };
            var mugsRoot = Kit.Go("Mugs On Table", k);
            var mugSet = mugsRoot.AddComponent<SocketSet>();
            Kit.Set(mugSet, "sockets", mugSockets);
            for (int i = 0; i < 2; i++)
                Kit.Grabbable(Kit.Model("Kitchen", "Mugs", k, new Vector3(-0.9f + i * 0.18f, 0.92f, 3.5f), 0f, height: 0.1f, rootName: "Mug"), 0.25f);
            var thirdMug = Kit.Model("Kitchen", "Mugs", k, new Vector3(0, 0.76f, 1.75f), 0f, height: 0.06f, rootName: "Third Mug");
            thirdMug.SetActive(false);
            var honey = Kit.Model("Kitchen", "Honey_jar", k, new Vector3(-1.05f, 0.92f, 3.45f), 0f, height: 0.2f, rootName: "Honey");
            var honeySignal = Kit.Signal(honey, mass: 0.3f);

            // Radio with its dial.
            var radio = Kit.Model("Kitchen", "Radio", k, new Vector3(-1.55f, 0.92f, 3.5f), 180f, length: 0.3f, rootName: "Radio");
            var knob = Kit.Shape(PrimitiveType.Cylinder, "Dial", radio.transform, new Vector3(0.09f, 0.06f, -0.06f), new Vector3(0.045f, 0.01f, 0.045f), Kit.C("darkwood"), collider: true);
            knob.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Kit.Simple(knob);
            var dial = knob.AddComponent<Dial>();
            Kit.Set(dial, "knobVisual", knob.transform);
            Kit.Set(dial, "localAxis", Vector3.up);
            Kit.Set(dial, "startValue", 40f);
            var radioStatic = Kit.Sound(radio.transform, "Static", Vector3.zero, "SFX_radio_static", loop: true, volume: 0.6f);
            var radioSong = Kit.Sound(radio.transform, "Song", Vector3.zero, "MUS_Lantern_Piano", loop: true, volume: 0f);
            var radioRev = Kit.Sound(radio.transform, "Reversed", Vector3.zero, "SFX_reversed_voice", loop: true, volume: 0f);

            // Lamps.
            var lamps = new List<PullChainLamp>();
            Vector3[] lampSpots = { new Vector3(-1.5f, 2.6f, 2f), new Vector3(0f, 2.6f, 2.6f), new Vector3(1.5f, 2.6f, 1.4f) };
            foreach (var p in lampSpots)
            {
                var lampModel = Kit.Model("Kitchen", "Pull_chain_lamp", k, p - new Vector3(0, 0.76f, 0), 0f, height: 0.76f, rootName: "Lamp");
                var bulb = Kit.PointLight("Bulb", lampModel.transform, new Vector3(0, 0.1f, 0), new Color(1f, 0.85f, 0.55f), 1.6f, 6f);
                var chain = Kit.Go("Chain", lampModel.transform, new Vector3(0.03f, -0.05f, 0));
                chain.AddComponent<BoxCollider>().size = new Vector3(0.06f, 0.2f, 0.06f);
                var chainSimple = chain.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                var click = Kit.Sound(lampModel.transform, "Click", Vector3.zero, "SFX_flip_tile", volume: 0.6f);
                var lamp = lampModel.AddComponent<PullChainLamp>();
                Kit.Set(lamp, "bulb", bulb);
                Kit.Set(lamp, "bulbRenderer", lampModel.GetComponentInChildren<Renderer>());
                Kit.Set(lamp, "chain", chainSimple);
                Kit.Set(lamp, "clickSound", click);
                lamps.Add(lamp);
            }
            var roomLight = Kit.PointLight("Room Glow", k, new Vector3(0, 2.3f, 2f), new Color(1f, 0.9f, 0.7f), 0.6f, 6f);

            // Maren (memory) by the counter; the news.
            var marenPuppet = Kit.Character("Maren_tonight", k, new Vector3(1.4f, 0, 3.0f), 200f, Characters.PuppetAnimator.Move.Lean, 0.8f, "Maren (memory)");
            var maren = marenPuppet.gameObject;
            maren.SetActive(false);
            var marenVoice = Kit.Go("Maren Voice", k, new Vector3(-3.6f, 1.6f, 2f));
            var newsHost = Kit.Go("CS_04A", k);
            var news = Cut.New("CS_04A_ThirdMug", newsHost, 14f)
                .Active(maren, 0f, 14f)
                .Pose(marenPuppet, 0f, 9f, Characters.PuppetAnimator.Move.Lean)
                .Pose(marenPuppet, 9f, 5f, Characters.PuppetAnimator.Move.HandsOverMouth)
                .Active(thirdMug, 3f, 11f)
                .Light(roomLight, 0f, 14f, 0.6f, 1.3f, null, new Color(1f, 0.85f, 0.55f))
                .Sound("SFX_honey_drop", 9f)
                .Done();
            var foundHost = Kit.Go("CS_04B", k);
            var found = Cut.New("CS_04B_ItFoundUs", foundHost, 6f)
                .Light(roomLight, 0f, 6f, 0.6f, 0.15f, null, null, 0.35f)
                .Sound("SFX_light_fail", 0.2f)
                .Sound("SFX_pull_near", 2f)
                .Done();

            // The ring on the table.
            var ring = Kit.Model("Kitchen", "Wedding_ring_leak", k, new Vector3(0.1f, 0.76f, 1.65f), 0f, length: 0.021f, rootName: "Ring");
            var ringCol = ring.AddComponent<SphereCollider>();
            ringCol.radius = 0.03f;
            Kit.Signal(ring, mass: 0.02f);
            Kit.Set(ring.AddComponent<LeakedObjectItem>(), "which", LeakedObject.Ring);
            ring.SetActive(false);

            var exit = Kit.Trigger("Exit Door", k, new Vector3(0, 1f, -1.2f), new Vector3(1.2f, 2f, 0.8f));
            var pull = Kit.Pull(k, new Vector3(-3.4f, 0, 2f), 90f);
            var doorway = Kit.Go("Pull Doorway", k, new Vector3(-3.0f, 0, 2f), 90f);

            var memory = Kit.Memory(k.gameObject);
            var d = Kit.Director<Ch4KitchenDirector>(Chapter.YellowKitchen, memory, pull, 0.4f, 0.25f);
            Kit.On<float>(dial, "onValueChanged", d.OnRadioTuned);
            Kit.OnEnter(exit, d.OnExitDoor);
            Kit.Set(d, "marenMem", marenVoice.transform);
            Kit.Set(d, "kettle", kk);
            Kit.Set(d, "mugsOnTable", mugSet);
            Kit.Set(d, "honey", honeySignal);
            Kit.Set(d, "radioDial", dial);
            Kit.Set(d, "radioStatic", radioStatic);
            Kit.Set(d, "radioSong", radioSong);
            Kit.Set(d, "radioReversed", radioRev);
            Kit.Set(d, "thirdMug", news);
            Kit.Set(d, "itFoundUs", found);
            Kit.Set(d, "lamps", lamps);
            Kit.Set(d, "pullDoorway", doorway.transform);
            Kit.Set(d, "tableRing", ring);
            Kit.Set(d, "exitDoorTrigger", exit.GetComponent<Collider>());

            var collectible = Kit.Go("Drawing on the fridge wall", k, new Vector3(2.47f, 1.4f, 1.2f), 90f);
            Kit.Drawing("Paper", collectible.transform, Vector3.zero, Vector3.zero, 10, 0.3f);
            Kit.Set(collectible.AddComponent<LastNight.World.DrawingCollectible>(), "index", 10);
            Kit.Save(scene, "Ch4_Kitchen");
        }
    }
}
