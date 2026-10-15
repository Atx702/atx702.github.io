using System.Collections.Generic;
using System.Linq;
using LastNight.Audio;
using LastNight.Core;
using LastNight.Dialogue;
using LastNight.Player;
using LastNight.UI;
using LastNight.World;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LastNight.EditorTools
{
    /// <summary>Core.unity: always loaded. The player rig, game systems, audio, subtitles, menus.</summary>
    public static class CoreBuilder
    {
        public const string RigPrefab = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string VignettePrefab = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";

        public static void Build()
        {
            var scene = Kit.NewScene("Core");

            // ----- Systems -----
            var systems = Kit.Go("Game Systems");
            var gm = systems.AddComponent<GameManager>();
            Kit.Go("XR Interaction Manager").AddComponent<XRInteractionManager>();
            var es = Kit.Go("Event System");
            es.AddComponent<EventSystem>();
            es.AddComponent<XRUIInputModule>();
            var worldCenter = Kit.Go("World Center");

            // ----- Rig -----
            var rigAsset = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefab);
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigAsset);
            rig.transform.position = Vector3.zero;
            var origin = rig.GetComponent<XROrigin>();
            var cam = origin.Camera;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 160f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Kit.C("fog");
            var head = cam.transform;
            var left = FindDeep(rig.transform, "Left Controller");
            var right = FindDeep(rig.transform, "Right Controller");

            var body = Kit.Go("Player Body", rig.transform);
            body.tag = "Player";
            body.AddComponent<CapsuleCollider>();
            body.AddComponent<Rigidbody>();
            Kit.Set(body.AddComponent<PlayerBody>(), "head", head);

            var mouth = Kit.Go("Mouth", head, new Vector3(0f, -0.09f, 0.06f));
            var ring = Kit.Model("Kitchen", "Wedding_ring_leak", left, new Vector3(-0.01f, -0.02f, -0.03f), 0f, length: 0.021f, rootName: "Hand Ring");

            var playerRig = rig.AddComponent<PlayerRig>();
            Kit.Set(playerRig, "origin", origin);
            Kit.Set(playerRig, "head", head);
            Kit.Set(playerRig, "leftHand", left);
            Kit.Set(playerRig, "rightHand", right);
            Kit.Set(playerRig, "mouth", mouth.transform);
            Kit.Set(playerRig, "handRing", ring);

            // ----- Fades and eyelids: small spheres around the eyes, drawn last -----
            var overlay = Kit.ShaderMat("LastNight/Overlay", "Overlay", m => m.SetColor("_Color", new Color(0, 0, 0, 0)));
            var faderGo = Kit.Shape(PrimitiveType.Sphere, "Fader", head, Vector3.zero, Vector3.one * 0.3f, Color.black);
            faderGo.GetComponent<Renderer>().sharedMaterial = overlay;
            var fader = faderGo.AddComponent<ScreenFader>();
            Kit.Set(fader, "fadeRenderer", faderGo.GetComponent<Renderer>());
            var lidGo = Kit.Shape(PrimitiveType.Sphere, "Eyelids", head, Vector3.zero, Vector3.one * 0.25f, Color.black);
            lidGo.GetComponent<Renderer>().sharedMaterial = overlay;

            var eyes = head.gameObject.AddComponent<EyesClosed>();
            Kit.Set(eyes, "head", head);
            Kit.Set(eyes, "leftHand", left);
            Kit.Set(eyes, "rightHand", right);
            Kit.Set(eyes, "closeEyesButton", UiKit.Action("CloseEyes"));
            Kit.Set(eyes, "eyelidRenderer", lidGo.GetComponent<Renderer>());

            // ----- Comfort settings applier -----
            var applier = systems.AddComponent<SettingsApplier>();
            Kit.Set(applier, "origin", origin);
            Kit.Set(applier, "moveProvider", rig.GetComponentInChildren<ContinuousMoveProvider>(true));
            Kit.Set(applier, "teleportProvider", rig.GetComponentInChildren<TeleportationProvider>(true));
            Kit.Set(applier, "snapTurn", rig.GetComponentInChildren<SnapTurnProvider>(true));
            Kit.Set(applier, "smoothTurn", rig.GetComponentInChildren<ContinuousTurnProvider>(true));
            var teleportRays = rig.GetComponentsInChildren<XRRayInteractor>(true)
                .Where(r => r.name.Contains("Teleport")).Select(r => (Object)r.gameObject).ToList();
            Kit.Set(applier, "teleportRays", teleportRays);
            var vignette = Vignette(head, rig);
            if (vignette != null) Kit.Set(applier, "vignette", vignette);

            // ----- Audio -----
            var audio = Kit.Go("Audio");
            var thump = Kit.Sound(audio.transform, "Thump", Vector3.zero, "SFX_thump", spatial: false, volume: 0.8f);
            var beat = audio.AddComponent<BeatClock>();
            Kit.Set(beat, "thumpSource", thump);
            Kit.Set(beat, "thumpClip", Kit.Clip("SFX_thump"));
            var voice = Kit.Sound(head, "Voice", Vector3.zero, null, spatial: false);
            voice.priority = 0;
            var worldVoice = Kit.Sound(audio.transform, "World Voice", Vector3.zero, null, spatial: true, maxDistance: 40f);
            worldVoice.priority = 1;
            var dialogue = audio.AddComponent<DialoguePlayer>();
            Kit.Set(dialogue, "database", Database());
            Kit.Set(dialogue, "voiceSource", voice);
            Kit.Set(dialogue, "worldSource", worldVoice);

            var idle = systems.AddComponent<IdleWatcher>();
            Kit.Set(idle, "worldCenter", worldCenter.transform);

            // ----- UI -----
            Subtitles(head);
            Toast(head);
            var settings = SettingsPanel(applier);
            Pause(head);
            Boot(applier);

            Kit.Save(scene, "Core");
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            Debug.LogWarning($"[Last Night] {name} not found in the rig.");
            return root;
        }

        static TunnelingVignetteController Vignette(Transform head, GameObject rig)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VignettePrefab);
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, head);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var v = go.GetComponent<TunnelingVignetteController>();
            if (v == null) return null;
            var providers = new List<LocomotionVignetteProvider>();
            foreach (var p in new LocomotionProvider[]
                     {
                         rig.GetComponentInChildren<ContinuousMoveProvider>(true),
                         rig.GetComponentInChildren<ContinuousTurnProvider>(true),
                         rig.GetComponentInChildren<SnapTurnProvider>(true),
                         rig.GetComponentInChildren<TeleportationProvider>(true),
                     })
            {
                if (p != null) providers.Add(new LocomotionVignetteProvider { locomotionProvider = p, enabled = true });
            }
            v.locomotionVignetteProviders = providers;
            EditorUtility.SetDirty(v);
            return v;
        }

        public static DialogueDatabase Database()
        {
            const string path = Kit.Root + "/Data/DialogueDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(path);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<DialogueDatabase>();
                AssetDatabase.CreateAsset(db, path);
            }
            Kit.Set(db, "dialogueCsv", AssetDatabase.LoadAssetAtPath<TextAsset>(Kit.Root + "/Data/dialogue.csv"));
            return db;
        }

        // Subtitles live in metres (SubtitleView sets font sizes like 0.045 m).
        static void Subtitles(Transform head)
        {
            var go = new GameObject("Subtitles", typeof(RectTransform));
            go.transform.position = new Vector3(0, 1.3f, 1.6f);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1.1f, 0.2f);
            go.AddComponent<CanvasGroup>();
            var bg = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
            bg.transform.SetParent(go.transform, false);
            var bgr = (RectTransform)bg.transform;
            bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one; bgr.sizeDelta = Vector2.zero;
            bg.color = new Color(0, 0, 0, 0.6f);
            var label = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(go.transform, false);
            var lr = (RectTransform)label.transform;
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.sizeDelta = new Vector2(-0.04f, -0.02f);
            label.fontSize = 0.045f;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.text = "";
            var view = go.AddComponent<SubtitleView>();
            Kit.Set(view, "head", head);
            Kit.Set(view, "label", label);
            Kit.Set(view, "background", bg);
        }

        static void Toast(Transform head)
        {
            var canvas = UiKit.Panel("Achievement Toast", null, new Vector3(0, 1.2f, 1.2f), 0f, new Vector2(520, 90), new Color(0.08f, 0.08f, 0.09f, 0.85f));
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            var label = UiKit.Label(canvas.transform, "", Vector2.zero, new Vector2(500, 80), 32f, color: new Color(0.95f, 0.9f, 0.75f));
            var toast = canvas.gameObject.AddComponent<AchievementToast>();
            Kit.Set(toast, "group", group);
            Kit.Set(toast, "label", label);
            Kit.Set(toast, "head", head);
        }

        static void Pause(Transform head)
        {
            var holder = Kit.Go("Pause Menu");
            var card = UiKit.Panel("Pause Card", holder.transform, new Vector3(0, 1.2f, 0.6f), 0f, new Vector2(420, 430), UiKit.Paper);
            var pause = holder.AddComponent<PauseMenu>();
            UiKit.Label(card.transform, "Paused", new Vector2(0, 165), new Vector2(380, 60), 40f);
            UiKit.Button(card.transform, "Resume", new Vector2(0, 85), new Vector2(340, 64), pause.Resume);
            UiKit.Button(card.transform, "Settings", new Vector2(0, 10), new Vector2(340, 64), pause.OpenSettings);
            UiKit.Button(card.transform, "Restart from anchor", new Vector2(0, -65), new Vector2(340, 64), pause.RestartFromAnchor);
            UiKit.Button(card.transform, "Quit to bed", new Vector2(0, -140), new Vector2(340, 64), pause.QuitToBed);
            card.gameObject.SetActive(false);
            Kit.Set(pause, "menuButton", UiKit.Action("Menu"));
            Kit.Set(pause, "card", card.gameObject);
            Kit.Set(pause, "head", head);
        }

        static void Boot(SettingsApplier applier)
        {
            var holder = Kit.Go("Boot");
            var warning = UiKit.Panel("Warning Card", holder.transform, new Vector3(0, 1.5f, 1.4f), 0f, new Vector2(900, 420), new Color(0.06f, 0.06f, 0.07f, 1f));
            UiKit.Label(warning.transform, "LAST NIGHT", new Vector2(0, 150), new Vector2(800, 70), 54f, color: Color.white);
            UiKit.Label(warning.transform,
                "This game deals with death, grief and the loss of a child.\nIt contains flashing lights (they can be reduced in Settings).",
                new Vector2(0, 20), new Vector2(820, 160), 30f, color: new Color(0.85f, 0.85f, 0.85f));
            UiKit.Label(warning.transform, "Press A or pull a trigger", new Vector2(0, -150), new Vector2(800, 50), 26f, color: new Color(0.6f, 0.6f, 0.6f));
            warning.gameObject.SetActive(false);

            var calibration = UiKit.Panel("Calibration Card", holder.transform, new Vector3(0, 1.5f, 1.4f), 0f, new Vector2(900, 300), new Color(0.06f, 0.06f, 0.07f, 1f));
            UiKit.Label(calibration.transform, "Stand or sit comfortably, look straight ahead\nand press A.", Vector2.zero, new Vector2(820, 200), 34f, color: Color.white);
            calibration.gameObject.SetActive(false);

            var boot = holder.AddComponent<BootFlow>();
            Kit.Set(boot, "warningCard", warning.gameObject);
            Kit.Set(boot, "calibrationCard", calibration.gameObject);
            Kit.Set(boot, "confirm", UiKit.Action("Confirm"));
            Kit.Set(boot, "applier", applier);
        }

        static SettingsMenu SettingsPanel(SettingsApplier applier)
        {
            var canvas = UiKit.Panel("Settings", null, new Vector3(0, 1.3f, 0.9f), 0f, new Vector2(760, 820), UiKit.Paper);
            var menu = canvas.gameObject.AddComponent<SettingsMenu>();
            var t = canvas.transform;
            UiKit.Label(t, "Settings", new Vector2(0, 370), new Vector2(700, 60), 42f);

            string[] tabs = { "Comfort", "Access", "Audio", "Graphics" };
            var tabRoots = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                var b = UiKit.Button(t, tabs[i], new Vector2(-270 + i * 180, 305), new Vector2(165, 52), null);
                UnityEditor.Events.UnityEventTools.AddIntPersistentListener(b.onClick, menu.ShowTab, i);
                tabRoots[i] = new GameObject(tabs[i] + " Tab", typeof(RectTransform));
                tabRoots[i].transform.SetParent(t, false);
                UiKit.Place(tabRoots[i], new Vector2(0, -20), new Vector2(720, 600));
            }

            UnityEngine.Events.UnityAction changed = menu.OnAnyChanged;
            int row = 0;
            Transform tab = tabRoots[0].transform;
            Vector2 L() => new Vector2(-170, 250 - row * 62);
            Vector2 R() => new Vector2(160, 250 - row * 62);
            void Name(string s) => UiKit.Label(tab, s, L(), new Vector2(320, 56), 26f, TextAlignmentOptions.Left);
            TMP_Dropdown Drop(string s, params string[] o) { Name(s); var d = UiKit.Dropdown(tab, R(), new Vector2(300, 50), o, changed); row++; return d; }
            Slider Slide(string s, float min, float max) { Name(s); var x = UiKit.Slider(tab, R(), new Vector2(300, 30), min, max, changed); row++; return x; }
            Toggle Tog(string s) { Name(s); var x = UiKit.Toggle(tab, R(), new Vector2(40, 40), changed); row++; return x; }

            Kit.Set(menu, "movement", Drop("Movement", "Smooth", "Teleport", "Both"));
            Kit.Set(menu, "moveSpeed", Slide("Move speed", 1f, 2.5f));
            Kit.Set(menu, "turn", Drop("Turning", "Snap", "Smooth"));
            Kit.Set(menu, "snapAngle", Drop("Snap angle", "15°", "30°", "45°", "90°"));
            Kit.Set(menu, "smoothTurnSpeed", Slide("Smooth turn speed", 30f, 180f));
            Kit.Set(menu, "vignette", Drop("Vignette", "Off", "Low", "Medium", "High"));
            Kit.Set(menu, "seated", Tog("Seated play"));
            Kit.Set(menu, "leftHanded", Tog("Left handed"));
            Kit.Set(menu, "fadeCuts", Tog("Fade cuts for forced moves"));

            tab = tabRoots[1].transform; row = 0;
            Kit.Set(menu, "intensity", Drop("Intensity", "Full", "Reduced"));
            Kit.Set(menu, "flashReduction", Tog("Flash reduction"));
            Kit.Set(menu, "heartbeatHaptics", Slide("Heartbeat haptics", 0f, 1f));
            Kit.Set(menu, "subtitles", Tog("Subtitles"));
            Kit.Set(menu, "speakerNames", Tog("Speaker names"));
            Kit.Set(menu, "subtitleSize", Drop("Subtitle size", "S", "M", "L", "XL"));
            Kit.Set(menu, "subtitleBackground", Slide("Subtitle background", 0f, 1f));
            Kit.Set(menu, "eyesClosedInput", Drop("Close eyes with", "Hands over eyes", "B button", "Both"));
            Kit.Set(menu, "toggleMode", Tog("Toggle instead of hold"));
            Kit.Set(menu, "micHumming", Tog("Hum into the mic"));

            tab = tabRoots[2].transform; row = 0;
            Kit.Set(menu, "master", Slide("Master", 0f, 1f));
            Kit.Set(menu, "voice", Slide("Voices", 0f, 1f));
            Kit.Set(menu, "music", Slide("Music", 0f, 1f));
            Kit.Set(menu, "ambience", Slide("Ambience", 0f, 1f));
            Kit.Set(menu, "sfx", Slide("Effects", 0f, 1f));
            Kit.Set(menu, "leaks", Slide("Muffled voices", 0f, 1f));

            tab = tabRoots[3].transform; row = 0;
            Kit.Set(menu, "renderScale", Slide("Render scale", 0.7f, 1.3f));

            UiKit.Button(t, "Recenter", new Vector2(-230, -360), new Vector2(200, 56), menu.Recenter);
            UiKit.Button(t, "Defaults", new Vector2(0, -360), new Vector2(200, 56), menu.ResetDefaults);
            UiKit.Button(t, "Back", new Vector2(230, -360), new Vector2(200, 56), menu.Back);

            Kit.Set(menu, "comfortTab", tabRoots[0]);
            Kit.Set(menu, "accessibilityTab", tabRoots[1]);
            Kit.Set(menu, "audioTab", tabRoots[2]);
            Kit.Set(menu, "graphicsTab", tabRoots[3]);
            Kit.Set(menu, "applier", applier);
            canvas.gameObject.SetActive(false);
            return menu;
        }
    }
}
