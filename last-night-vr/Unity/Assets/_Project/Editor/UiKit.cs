using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LastNight.EditorTools
{
    /// <summary>World-space uGUI for VR: panels in metres, controls in "pixels" (1 px = 1 mm).</summary>
    public static class UiKit
    {
        public const float PixelsPerMetre = 1000f;
        public static readonly Color Paper = new Color(0.93f, 0.91f, 0.85f, 0.97f);
        public static readonly Color Ink = new Color(0.16f, 0.15f, 0.14f);

        static DefaultControls.Resources res;
        static TMP_DefaultControls.Resources tmpRes;

        static DefaultControls.Resources Res
        {
            get
            {
                if (res.standard != null) return res;
                res.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                res.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                res.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
                res.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                res.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
                res.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
                res.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
                return res;
            }
        }

        static TMP_DefaultControls.Resources TmpRes
        {
            get
            {
                var r = Res;
                tmpRes.standard = r.standard; tmpRes.background = r.background; tmpRes.inputField = r.inputField;
                tmpRes.knob = r.knob; tmpRes.checkmark = r.checkmark; tmpRes.dropdown = r.dropdown; tmpRes.mask = r.mask;
                return tmpRes;
            }
        }

        /// <summary>A world-space canvas, <paramref name="sizePx"/> pixels at 1 mm each, that XR rays can click.</summary>
        public static Canvas Panel(string name, Transform parent, Vector3 pos, float yaw, Vector2 sizePx, Color? background = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            go.transform.localScale = Vector3.one / PixelsPerMetre;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            ((RectTransform)go.transform).sizeDelta = sizePx;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;
            go.AddComponent<TrackedDeviceGraphicRaycaster>();
            if (background.HasValue)
            {
                var img = go.AddComponent<Image>();
                img.color = background.Value;
                img.sprite = Res.standard;
                img.type = Image.Type.Sliced;
            }
            return canvas;
        }

        public static RectTransform Rect(GameObject go) => (RectTransform)go.transform;

        public static void Place(GameObject go, Vector2 anchoredPos, Vector2 size)
        {
            var rt = Rect(go);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        public static TextMeshProUGUI Label(Transform parent, string text, Vector2 pos, Vector2 size, float fontSize = 30f, TextAlignmentOptions align = TextAlignmentOptions.Center, Color? color = null)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color ?? Ink;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            Place(go, pos, size);
            return t;
        }

        public static Button Button(Transform parent, string text, Vector2 pos, Vector2 size, UnityAction onClick)
        {
            var go = TMP_DefaultControls.CreateButton(TmpRes);
            go.name = "Button " + text;
            go.transform.SetParent(parent, false);
            Place(go, pos, size);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 28f;
            label.color = Ink;
            var b = go.GetComponent<Button>();
            if (onClick != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, onClick);
            return b;
        }

        public static Slider Slider(Transform parent, Vector2 pos, Vector2 size, float min, float max, UnityAction onChanged)
        {
            var go = DefaultControls.CreateSlider(Res);
            go.transform.SetParent(parent, false);
            Place(go, pos, size);
            var s = go.GetComponent<Slider>();
            s.minValue = min;
            s.maxValue = max;
            if (onChanged != null) UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(s.onValueChanged, onChanged);
            return s;
        }

        public static Toggle Toggle(Transform parent, Vector2 pos, Vector2 size, UnityAction onChanged)
        {
            var go = DefaultControls.CreateToggle(Res);
            go.transform.SetParent(parent, false);
            Place(go, pos, size);
            var label = go.GetComponentInChildren<Text>();
            if (label != null) Object.DestroyImmediate(label.gameObject);
            var t = go.GetComponent<Toggle>();
            if (onChanged != null) UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(t.onValueChanged, onChanged);
            return t;
        }

        public static TMP_Dropdown Dropdown(Transform parent, Vector2 pos, Vector2 size, string[] options, UnityAction onChanged)
        {
            var go = TMP_DefaultControls.CreateDropdown(TmpRes);
            go.transform.SetParent(parent, false);
            Place(go, pos, size);
            var d = go.GetComponent<TMP_Dropdown>();
            d.ClearOptions();
            d.AddOptions(options.ToList());
            foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true)) t.fontSize = 24f;
            if (onChanged != null) UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(d.onValueChanged, onChanged);
            return d;
        }

        public static InputActionReference Action(string name)
        {
            var path = "Assets/_Project/Settings/LastNightInput.inputactions";
            var r = AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>()
                .FirstOrDefault(x => x.action != null && x.action.name == name);
            if (r == null) Debug.LogWarning($"[Last Night] Input action {name} not found in {path}.");
            return r;
        }
    }
}
