using LastNight.UI;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace LastNight.EditorTools
{
    /// <summary>
    /// MainMenu.unity, "The Bed": the prologue bed in the field at dusk. Objects around it are the menu.
    /// Point at one (or reach it) and grip or pull the trigger.
    /// </summary>
    public static class MainMenuBuilder
    {
        public static void Build()
        {
            var scene = Kit.NewScene("MainMenu");
            var dusk = new Color(0.52f, 0.47f, 0.47f);
            Kit.Atmosphere(Kit.Mood(dusk, 4f, 60f, new Color(0.42f, 0.4f, 0.42f), new Color(1f, 0.75f, 0.55f), 0.45f, new Vector3(8, 250, 0)));
            var world = Kit.Go("World");
            Kit.Floor("Ground", world.transform, Vector3.zero, new Vector2(200, 200), Kit.C("grass") * 0.8f);
            Kit.Tufts(world.transform, Vector3.zero, 2f, 40f, 160, new Color(0.3f, 0.34f, 0.28f), 3);
            Kit.Spawn(world.transform, Vector3.zero, 0f);

            var amb = Kit.Go("Ambience");
            Kit.Sound(amb.transform, "Piano", Vector3.zero, "MUS_Lantern_Piano", loop: true, spatial: false, volume: 0.55f, playOnAwake: true);
            Kit.Sound(amb.transform, "Wind", Vector3.zero, "SFX_wind_field", loop: true, spatial: false, volume: 0.35f, playOnAwake: true);

            var bed = Kit.Model("Prologue_Field", "Old_double_bed", world.transform, new Vector3(-1.3f, 0, 0.9f), 90f, length: 2.1f, rootName: "Bed");
            Kit.FitCollider(bed);
            var stand = Kit.Model("Prologue_Field", "Nightstand_and_digital_clock", world.transform, new Vector3(0.75f, 0, 0.75f), 180f, height: 0.65f, rootName: "Nightstand");
            Kit.FitCollider(stand);
            Kit.PointLight("Lamp Glow", world.transform, new Vector3(0.9f, 1.3f, 0.8f), new Color(1f, 0.8f, 0.55f), 1.4f, 5f);

            // Menu items. Each gets a small label so first-time players know what they do.
            var pillow = Item("Continue", MainMenuItem.Action.Continue, Kit.Block("Pillow", world.transform, new Vector3(-1.3f, 0.62f, 0.25f), new Vector3(0.55f, 0.14f, 0.35f), "white", false), "Continue");
            var clock = Item("New Game", MainMenuItem.Action.NewGame, Kit.Block("Alarm Clock", world.transform, new Vector3(0.62f, 0.72f, 0.62f), new Vector3(0.16f, 0.09f, 0.08f), new Color(0.5f, 0.1f, 0.08f), false), "New Game");
            Kit.Text3D("Clock Face", clock.transform, new Vector3(0, 0, -0.045f), "3:12", 0.5f, new Color(1f, 0.25f, 0.2f), 0f, 0.2f);
            var lamp = Item("Settings", MainMenuItem.Action.Settings, Kit.Model("Bedroom_Wren_s_room", "Night_lantern", world.transform, new Vector3(0.95f, 0.65f, 0.85f), 0f, height: 0.25f, rootName: "Bedside Lamp"), "Settings");
            var frame = Kit.Block("Photo Frame", world.transform, new Vector3(0.45f, 0.78f, 0.88f), new Vector3(0.18f, 0.22f, 0.02f), "darkwood", false);
            Kit.Drawing("Photo", frame.transform, new Vector3(0, 0, -0.6f), new Vector3(0, 0, 0), 6, 0.8f);
            var extras = Item("Extras", MainMenuItem.Action.Extras, frame, "Extras");
            var door = Item("Quit", MainMenuItem.Action.Quit, Kit.Model("Prologue_Field", "Standalone_door_in_field", world.transform, new Vector3(0.3f, 0, 4.5f), 180f, height: 2.1f, rootName: "Door"), "Quit");
            var lantern = Item("Lucid Mode", MainMenuItem.Action.LucidMode, Kit.Model("Bedroom_Wren_s_room", "Night_lantern", world.transform, new Vector3(-0.45f, 0, 1.1f), 0f, height: 0.3f, rootName: "Lantern"), "Lucid Mode");

            Confirm(clock, "This will wake you somewhere else.", new Vector3(0.2f, 1.25f, 1.0f));
            Confirm(door, "Leave?", new Vector3(0.3f, 1.4f, 2.0f));
            Confirm(pillow, null, Vector3.zero);
            ExtrasPanel(extras);

            Kit.Save(scene, "MainMenu");
        }

        static MainMenuItem Item(string name, MainMenuItem.Action action, GameObject go, string label)
        {
            go.name = name;
            Kit.Simple(go);
            var item = go.AddComponent<MainMenuItem>();
            Kit.Set(item, "action", action);
            var b = Kit.LocalBounds(go, go.transform);
            Kit.Text3D("Label", go.transform, new Vector3(b.center.x, b.max.y + 0.08f, b.center.z), label, 0.9f, new Color(0.95f, 0.93f, 0.88f, 0.85f),
                0f, 0.6f).transform.rotation = Quaternion.LookRotation(go.transform.position - new Vector3(0, 1.5f, -0.3f));
            return item;
        }

        static void Confirm(MainMenuItem item, string question, Vector3 pos)
        {
            if (question == null) return;
            var panel = UiKit.Panel(item.name + " Confirm", null, pos, 0f, new Vector2(520, 220), UiKit.Paper);
            UiKit.Label(panel.transform, question, new Vector2(0, 50), new Vector2(480, 90), 30f);
            UiKit.Button(panel.transform, "Yes", new Vector2(-110, -55), new Vector2(180, 60), item.Confirm);
            var no = UiKit.Button(panel.transform, "No", new Vector2(110, -55), new Vector2(180, 60), null);
            UnityEventTools.AddBoolPersistentListener(no.onClick, panel.gameObject.SetActive, false);
            panel.gameObject.SetActive(false);
            Kit.Set(item, "confirmPanel", panel.gameObject);
        }

        static void ExtrasPanel(MainMenuItem item)
        {
            var panel = UiKit.Panel("Extras", null, new Vector3(0.6f, 1.35f, 1.3f), 15f, new Vector2(700, 560), UiKit.Paper);
            UiKit.Label(panel.transform, "Last Night", new Vector2(0, 220), new Vector2(640, 70), 46f);
            UiKit.Label(panel.transform,
                "A story about Tomas, Maren and Wren.\n\nWritten and directed with Claude.\nVoices, music and sound: ElevenLabs.\nModels: Meshy and procedural.\nBuilt in Unity.\n\nThere are three endings, twelve drawings\nand five things that don't belong.",
                new Vector2(0, -10), new Vector2(640, 360), 26f);
            var close = UiKit.Button(panel.transform, "Close", new Vector2(0, -230), new Vector2(200, 56), null);
            UnityEventTools.AddBoolPersistentListener(close.onClick, panel.gameObject.SetActive, false);
            panel.gameObject.SetActive(false);
            Kit.Set(item, "panelToOpen", panel.gameObject);
        }
    }
}
