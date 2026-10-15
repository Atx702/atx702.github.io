using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using LastNight.Core;
using LastNight.Player;
using LastNight.Pull;
using LastNight.World;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Object = UnityEngine.Object;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Building blocks the scene builders share: models, materials, interactables, triggers, wiring,
    /// audio, lights and text. Every scene is generated from code, so rebuilding is always safe.
    /// </summary>
    public static class Kit
    {
        public const string Root = "Assets/_Project";
        public const string ScenesFolder = Root + "/Scenes";
        public const string ModelsFolder = Root + "/Art/Models";
        public const string MaterialsFolder = Root + "/Art/Materials";
        public const string AudioFolder = Root + "/Audio";

        // ---------------------------------------------------------------- scenes

        public static Scene NewScene(string name)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = name;
            return scene;
        }

        public static void Save(Scene scene, string name)
        {
            Directory.CreateDirectory(ScenesFolder);
            EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{name}.unity");
        }

        public static GameObject Go(string name, Transform parent = null, Vector3 pos = default, float yaw = 0f)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return go;
        }

        public static T Add<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        // ---------------------------------------------------------------- colours and materials

        public static readonly Dictionary<string, Color> Palette = new Dictionary<string, Color>
        {
            ["grass"] = new Color(0.44f, 0.48f, 0.4f), ["fog"] = new Color(0.78f, 0.79f, 0.8f),
            ["wood"] = new Color(0.42f, 0.33f, 0.25f), ["darkwood"] = new Color(0.27f, 0.2f, 0.15f),
            ["pine"] = new Color(0.55f, 0.45f, 0.33f), ["white"] = new Color(0.82f, 0.82f, 0.8f),
            ["cream"] = new Color(0.8f, 0.77f, 0.66f), ["paper"] = new Color(0.9f, 0.88f, 0.8f),
            ["steel"] = new Color(0.55f, 0.57f, 0.58f), ["darksteel"] = new Color(0.28f, 0.3f, 0.32f),
            ["black"] = new Color(0.07f, 0.07f, 0.08f), ["gray"] = new Color(0.45f, 0.46f, 0.46f),
            ["ltgray"] = new Color(0.65f, 0.66f, 0.66f), ["red"] = new Color(0.62f, 0.16f, 0.14f),
            ["yellow"] = new Color(0.85f, 0.7f, 0.28f), ["gold"] = new Color(0.8f, 0.62f, 0.2f),
            ["teal"] = new Color(0.36f, 0.55f, 0.55f), ["ltteal"] = new Color(0.6f, 0.72f, 0.72f),
            ["blue"] = new Color(0.35f, 0.48f, 0.62f), ["wrongblue"] = new Color(0.42f, 0.6f, 0.78f),
            ["navy"] = new Color(0.14f, 0.17f, 0.27f), ["green"] = new Color(0.3f, 0.42f, 0.34f),
            ["board"] = new Color(0.22f, 0.32f, 0.27f), ["water"] = new Color(0.25f, 0.33f, 0.38f),
            ["platform"] = new Color(0.5f, 0.5f, 0.48f), ["linoleum"] = new Color(0.62f, 0.66f, 0.62f),
            ["night"] = new Color(0.16f, 0.18f, 0.24f), ["warmwall"] = new Color(0.86f, 0.72f, 0.42f),
            ["carpet"] = new Color(0.45f, 0.38f, 0.33f), ["sky"] = new Color(0.9f, 0.86f, 0.74f),
        };

        public static Color C(string name) => Palette.TryGetValue(name, out var c) ? c : Color.magenta;

        static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();

        public static Shader WorldShader => Shader.Find("LastNight/World");

        /// <summary>A flat-colour material on the project's Quest shader (cached as an asset).</summary>
        public static Material Mat(Color c, float emission = 0f)
        {
            var key = $"col_{ColorUtility.ToHtmlStringRGB(c)}_{(int)(emission * 100)}";
            if (matCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var path = $"{MaterialsFolder}/{key}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                Directory.CreateDirectory(MaterialsFolder);
                m = new Material(WorldShader) { name = key, enableInstancing = true };
                m.SetColor("_BaseColor", c);
                m.SetColor("_EmissionColor", c * emission);
                AssetDatabase.CreateAsset(m, path);
            }
            matCache[key] = m;
            return m;
        }

        public static Material Mat(string paletteName, float emission = 0f) => Mat(C(paletteName), emission);

        public static Material ShaderMat(string shaderName, string assetName, Action<Material> setup = null)
        {
            var path = $"{MaterialsFolder}/{assetName}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                Directory.CreateDirectory(MaterialsFolder);
                m = new Material(Shader.Find(shaderName)) { name = assetName };
                setup?.Invoke(m);
                AssetDatabase.CreateAsset(m, path);
            }
            return m;
        }

        // ---------------------------------------------------------------- primitives

        public static GameObject Block(string name, Transform parent, Vector3 center, Vector3 size, Color color, bool collider = true, float yaw = 0f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = Mat(color);
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            Static(go);
            return go;
        }

        public static GameObject Block(string name, Transform parent, Vector3 center, Vector3 size, string color, bool collider = true, float yaw = 0f)
            => Block(name, parent, center, size, C(color), collider, yaw);

        public static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(color);
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>A large walkable floor (a flat box so its collider is solid).</summary>
        public static GameObject Floor(string name, Transform parent, Vector3 center, Vector2 size, Color color)
            => Block(name, parent, center - new Vector3(0, 0.05f, 0), new Vector3(size.x, 0.1f, size.y), color);

        public static void Static(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }

        // ---------------------------------------------------------------- models

        /// <summary>
        /// Places a model from Art/Models/{section}/{name} (Meshy .fbx first, then the procedural .obj).
        /// Returns a root at <paramref name="pos"/> with the model sitting on it (bottom on the root, centred).
        /// <paramref name="height"/> &gt; 0 scales the model to that height in metres (Meshy models come in
        /// arbitrary units); <paramref name="length"/> &gt; 0 scales by the longest horizontal side instead.
        /// </summary>
        public static GameObject Model(string section, string name, Transform parent, Vector3 pos, float yaw = 0f,
            float height = 0f, float length = 0f, string rootName = null)
        {
            var root = Go(rootName ?? name, parent, pos, yaw);
            var asset = LoadModel(section, name);
            if (asset == null)
            {
                Debug.LogWarning($"[Last Night] Model {section}/{name} not found; using a placeholder box.");
                Block(name + "_placeholder", root.transform, new Vector3(0, Mathf.Max(height, 0.3f) / 2, 0),
                    Vector3.one * Mathf.Max(height, 0.3f), "gray", false);
                return root;
            }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
            PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;
            ConvertMaterials(inst, section, name);

            var b = LocalBounds(inst, root.transform);
            float scale = 1f;
            if (height > 0f && b.size.y > 0.0001f) scale = height / b.size.y;
            else if (length > 0f && Mathf.Max(b.size.x, b.size.z) > 0.0001f) scale = length / Mathf.Max(b.size.x, b.size.z);
            inst.transform.localScale = Vector3.one * scale;
            b = LocalBounds(inst, root.transform);
            inst.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
            return root;
        }

        static GameObject LoadModel(string section, string name)
        {
            foreach (var ext in new[] { ".fbx", ".obj" })
            {
                var a = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsFolder}/{section}/{name}{ext}");
                if (a != null) return a;
            }
            return null;
        }

        /// <summary>Every model ends up on LastNight/World so fog and memory decay work on everything.</summary>
        static void ConvertMaterials(GameObject inst, string section, string name)
        {
            var mtl = ReadMtl($"{ModelsFolder}/{section}/{name}.mtl");
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var src = mats[i];
                    var srcName = src != null ? src.name : "";
                    if (mtl != null && mtl.TryGetValue(srcName, out var col))
                    {
                        mats[i] = Mat(col.diffuse, col.emissive ? 1f : 0f);
                        continue;
                    }
                    mats[i] = Converted(src, $"{section}_{name}_{i}");
                }
                r.sharedMaterials = mats;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
        }

        static Material Converted(Material src, string key)
        {
            var path = $"{MaterialsFolder}/Converted/{key}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            Directory.CreateDirectory($"{MaterialsFolder}/Converted");
            m = new Material(WorldShader) { name = key, enableInstancing = true };
            if (src != null)
            {
                Texture tex = src.HasProperty("_BaseMap") ? src.GetTexture("_BaseMap") : null;
                if (tex == null && src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex");
                if (tex != null) m.SetTexture("_BaseMap", tex);
                var c = src.HasProperty("_BaseColor") ? src.GetColor("_BaseColor") : src.HasProperty("_Color") ? src.GetColor("_Color") : Color.white;
                m.SetColor("_BaseColor", tex != null ? Color.white : c);
            }
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        struct MtlColor { public Color diffuse; public bool emissive; }

        static Dictionary<string, MtlColor> ReadMtl(string path)
        {
            if (!File.Exists(path)) return null;
            var result = new Dictionary<string, MtlColor>();
            string current = null;
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                var parts = line.Split(' ');
                if (parts[0] == "newmtl") { current = parts[1]; result[current] = new MtlColor { diffuse = Color.white }; }
                else if (current != null && (parts[0] == "Kd" || parts[0] == "Ke") && parts.Length >= 4)
                {
                    var c = new Color(F(parts[1]), F(parts[2]), F(parts[3]));
                    var e = result[current];
                    if (parts[0] == "Kd") e.diffuse = c; else e.emissive = true;
                    result[current] = e;
                }
            }
            return result;
        }

        static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

        public static Bounds LocalBounds(GameObject go, Transform space)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            var b = new Bounds();
            foreach (var r in rs)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var mb = mf.sharedMesh.bounds;
                var m = space.worldToLocalMatrix * r.transform.localToWorldMatrix;
                for (int i = 0; i < 8; i++)
                {
                    var corner = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var p = m.MultiplyPoint3x4(corner);
                    if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
                    else b.Encapsulate(p);
                }
            }
            return b;
        }

        /// <summary>A box collider around everything this object renders.</summary>
        public static BoxCollider FitCollider(GameObject go, bool trigger = false, float pad = 0f)
        {
            var b = LocalBounds(go, go.transform);
            var col = Add<BoxCollider>(go);
            col.center = b.center;
            col.size = b.size + Vector3.one * pad;
            col.isTrigger = trigger;
            return col;
        }

        // ---------------------------------------------------------------- interaction

        public static XRGrabInteractable Grabbable(GameObject go, float mass = 0.4f, bool gravity = true, bool kinematic = false)
        {
            if (go.GetComponent<Collider>() == null) FitCollider(go);
            var rb = Add<Rigidbody>(go);
            rb.mass = mass;
            rb.useGravity = gravity;
            rb.isKinematic = kinematic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var grab = Add<XRGrabInteractable>(go);
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.useDynamicAttach = true;
            grab.throwOnDetach = true;
            grab.retainTransformParent = false;
            return grab;
        }

        public static XRSimpleInteractable Simple(GameObject go)
        {
            if (go.GetComponent<Collider>() == null) FitCollider(go);
            return Add<XRSimpleInteractable>(go);
        }

        public static XRSocketInteractor Socket(string name, Transform parent, Vector3 pos, float radius = 0.12f)
        {
            var go = Go(name, parent, pos);
            var s = go.AddComponent<SphereCollider>();
            s.isTrigger = true;
            s.radius = radius;
            var socket = go.AddComponent<XRSocketInteractor>();
            socket.showInteractableHoverMeshes = true;
            return socket;
        }

        public static TriggerRelay Trigger(string name, Transform parent, Vector3 center, Vector3 size, bool once = true)
        {
            var go = Go(name, parent, center);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var relay = go.AddComponent<TriggerRelay>();
            Set(relay, "once", once);
            return relay;
        }

        // ---------------------------------------------------------------- wiring

        /// <summary>Sets a serialized field by name. Throws if it doesn't exist, so typos fail loudly.</summary>
        public static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field) ?? throw new ArgumentException($"{target.GetType().Name} has no serialized field '{field}'");
            Assign(p, value);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Assign(SerializedProperty p, object value)
        {
            switch (value)
            {
                case null: p.objectReferenceValue = null; break;
                case Object o: p.objectReferenceValue = o; break;
                case bool b: p.boolValue = b; break;
                case int i when p.propertyType == SerializedPropertyType.Enum: p.enumValueIndex = i; break;
                case int i: p.intValue = i; break;
                case float f: p.floatValue = f; break;
                case double d: p.floatValue = (float)d; break;
                case string s: p.stringValue = s; break;
                case Vector2 v2: p.vector2Value = v2; break;
                case Vector3 v3: p.vector3Value = v3; break;
                case Color c: p.colorValue = c; break;
                case Enum e: p.enumValueIndex = Array.IndexOf(Enum.GetValues(e.GetType()), e); break;
                case System.Collections.IList list:
                    p.arraySize = list.Count;
                    for (int k = 0; k < list.Count; k++) Assign(p.GetArrayElementAtIndex(k), list[k]);
                    break;
                default: throw new ArgumentException($"Can't assign {value.GetType().Name} to {p.propertyPath}");
            }
        }

        static object EventField(Object target, string field)
        {
            for (var t = target.GetType(); t != null; t = t.BaseType)
            {
                var f = t.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (f != null)
                {
                    var ev = f.GetValue(target);
                    if (ev == null) { ev = Activator.CreateInstance(f.FieldType); f.SetValue(target, ev); }
                    return ev;
                }
            }
            throw new ArgumentException($"{target.GetType().Name} has no event '{field}'");
        }

        /// <summary>Adds a saved (persistent) listener to a UnityEvent field, e.g. TriggerRelay.onEnter -> director.OnPorch.</summary>
        public static void On(Object target, string eventField, UnityAction call)
        {
            UnityEventTools.AddPersistentListener((UnityEvent)EventField(target, eventField), call);
            EditorUtility.SetDirty(target);
        }

        public static void On<T>(Object target, string eventField, UnityAction<T> call)
        {
            UnityEventTools.AddPersistentListener((UnityEvent<T>)EventField(target, eventField), call);
            EditorUtility.SetDirty(target);
        }

        public static void OnBool(Object target, string eventField, UnityAction<bool> call, bool argument)
        {
            UnityEventTools.AddBoolPersistentListener((UnityEventBase)EventField(target, eventField), call, argument);
            EditorUtility.SetDirty(target);
        }

        public static void OnEnter(TriggerRelay relay, UnityAction call) => On(relay, "onEnter", call);

        // ---------------------------------------------------------------- audio

        public static AudioClip Clip(string id)
        {
            foreach (var path in new[] { $"{AudioFolder}/SFX/{id}.mp3", $"{AudioFolder}/Music/{id}.mp3", $"{AudioFolder}/Resources/VO/{id}.mp3" })
            {
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (c != null) return c;
            }
            Debug.LogWarning($"[Last Night] Audio {id} not found.");
            return null;
        }

        public static AudioSource Sound(Transform parent, string name, Vector3 pos, string clipId, bool loop = false,
            bool spatial = true, float volume = 1f, bool playOnAwake = false, float maxDistance = 30f)
        {
            var go = Go(name, parent, pos);
            var a = go.AddComponent<AudioSource>();
            a.clip = clipId != null ? Clip(clipId) : null;
            a.loop = loop;
            a.volume = volume;
            a.playOnAwake = playOnAwake;
            a.spatialBlend = spatial ? 1f : 0f;
            a.rolloffMode = AudioRolloffMode.Linear;
            a.minDistance = 1f;
            a.maxDistance = maxDistance;
            a.dopplerLevel = 0f;
            return a;
        }

        // ---------------------------------------------------------------- light and atmosphere

        public struct Look
        {
            public Color fog, ambient, sun, background;
            public float fogStart, fogEnd, sunIntensity, farClip;
            public Vector3 sunEuler;
        }

        public static Look Mood(Color fog, float fogStart, float fogEnd, Color ambient, Color sun, float sunIntensity, Vector3 sunEuler, float farClip = 0f)
            => new Look { fog = fog, background = fog, fogStart = fogStart, fogEnd = fogEnd, ambient = ambient, sun = sun,
                          sunIntensity = sunIntensity, sunEuler = sunEuler, farClip = farClip > 0 ? farClip : fogEnd + 5f };

        public static void Atmosphere(Look look)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = look.fog;
            RenderSettings.fogStartDistance = look.fogStart;
            RenderSettings.fogEndDistance = look.fogEnd;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = look.ambient;
            RenderSettings.skybox = null;
            if (look.sunIntensity > 0f)
            {
                var sun = Go("Sun", null, Vector3.zero).AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = look.sun;
                sun.intensity = look.sunIntensity;
                sun.shadows = LightShadows.None;
                sun.transform.rotation = Quaternion.Euler(look.sunEuler);
                RenderSettings.sun = sun;
            }
            var lookGo = Go("Scene Look");
            var sl = lookGo.AddComponent<SceneLook>();
            Set(sl, "background", look.background);
            Set(sl, "farClip", look.farClip);
        }

        public static Light PointLight(string name, Transform parent, Vector3 pos, Color color, float intensity, float range)
        {
            var l = Go(name, parent, pos).AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;
            l.renderMode = LightRenderMode.ForcePixel;
            return l;
        }

        // ---------------------------------------------------------------- text

        public static TextMeshPro Text3D(string name, Transform parent, Vector3 pos, string text, float size, Color color, float yaw = 0f, float width = 2f)
        {
            var go = Go(name, parent, pos, yaw);
            var t = go.AddComponent<TextMeshPro>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.rectTransform.sizeDelta = new Vector2(width, size * 0.2f + 0.3f);
            return t;
        }

        // ---------------------------------------------------------------- game pieces

        public static PlayerSpawnPoint Spawn(Transform parent, Vector3 pos, float yaw, bool isDefault = true, Chapter? returnFrom = null, string name = "Spawn")
        {
            var sp = Go(name, parent, pos, yaw).AddComponent<PlayerSpawnPoint>();
            Set(sp, "isDefault", isDefault);
            if (returnFrom.HasValue)
            {
                Set(sp, "isReturnPoint", true);
                Set(sp, "returnFrom", returnFrom.Value);
            }
            return sp;
        }

        /// <summary>The Pull: Maren's stretched silhouette, white and glowing. Starts dormant (inactive).</summary>
        public static PullController Pull(Transform parent, Vector3 pos, float yaw = 0f, Transform circleCenter = null)
        {
            var root = Model("Characters", "The_Pull", parent, pos, yaw, height: 2.6f, rootName: "The Pull");
            var mat = ShaderMat("LastNight/PullBody", "PullBody");
            foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = mat;
            var src = root.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.playOnAwake = false;
            src.maxDistance = 25f;
            src.rolloffMode = AudioRolloffMode.Linear;
            var pull = root.AddComponent<PullController>();
            Set(pull, "body", root.GetComponentInChildren<Renderer>(true));
            Set(pull, "stepClip", Clip("SFX_pull_step"));
            Set(pull, "nearLoop", Clip("SFX_pull_near"));
            if (circleCenter != null) Set(pull, "circleCenter", circleCenter);
            root.SetActive(false);
            return pull;
        }

        public static MemoryDecay Memory(GameObject root)
        {
            var m = Add<MemoryDecay>(root);
            var snd = Sound(root.transform, "Decay Sound", Vector3.up, "SFX_memory_decay", volume: 0.8f);
            Set(m, "decaySound", snd);
            return m;
        }

        public static GestureDetector Gesture(Transform parent, GestureDetector.Kind kind, UnityAction onGesture, bool startActive = false)
        {
            var go = Go($"Gesture {kind}", parent);
            var g = go.AddComponent<GestureDetector>();
            Set(g, "kind", kind);
            if (onGesture != null) On(g, "onGesture", onGesture);
            go.SetActive(startActive);
            return g;
        }

        public static DialogueTrigger Lines(string name, Transform parent, Vector3 pos, Vector3 size, DialogueTrigger.When when, Transform speaker, params string[] ids)
        {
            var go = Go(name, parent, pos);
            if (when == DialogueTrigger.When.OnEnter || when == DialogueTrigger.When.OnEnterAndLook)
            {
                var box = go.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = size;
            }
            var t = go.AddComponent<DialogueTrigger>();
            Set(t, "when", when);
            Set(t, "lineIds", ids);
            if (speaker != null) Set(t, "speaker", speaker);
            return t;
        }

        public static T Director<T>(Chapter chapter, MemoryDecay memory, PullController pull, float beatVolume, float beatIntensity) where T : Chapters.ChapterDirector
        {
            var d = Go("Director").AddComponent<T>();
            Set(d, "chapter", chapter);
            if (memory != null) Set(d, "memory", memory);
            if (pull != null) Set(d, "pull", pull);
            Set(d, "beatVolume", beatVolume);
            Set(d, "beatIntensity", beatIntensity);
            return d;
        }

        /// <summary>A flat image (a drawing on paper) with a procedural texture from Art/Drawings, if present.</summary>
        public static GameObject Drawing(string name, Transform parent, Vector3 pos, Vector3 euler, int index, float size = 0.3f)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = pos;
            quad.transform.localRotation = Quaternion.Euler(euler);
            quad.transform.localScale = new Vector3(size, size * 0.75f, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/Art/Drawings/drawing_{index:00}.png");
            var key = $"drawing_{index:00}";
            var m = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/{key}.mat");
            if (m == null)
            {
                m = new Material(WorldShader) { name = key };
                m.SetColor("_BaseColor", Color.white);
                m.SetColor("_EmissionColor", new Color(0.25f, 0.25f, 0.22f));
                if (tex != null) m.SetTexture("_BaseMap", tex);
                m.SetFloat("_Cull", 0f);
                Directory.CreateDirectory(MaterialsFolder);
                AssetDatabase.CreateAsset(m, $"{MaterialsFolder}/{key}.mat");
            }
            quad.GetComponent<Renderer>().sharedMaterial = m;
            return quad;
        }
    }
}
