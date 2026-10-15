using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Step 1: project settings for Meta Quest 2 (standalone Android) plus PC Link for testing in the editor.
    /// Everything here is idempotent, so running it again is safe.
    /// </summary>
    public static class QuestSetup
    {
        public const string SettingsFolder = "Assets/_Project/Settings";
        const string UrpAssetPath = SettingsFolder + "/LastNight_Quest2_URP.asset";
        public const string RealWorldLayer = "RealWorld";

        /// <summary>Returns false if Unity must restart before continuing (input backend change).</summary>
        public static bool Apply()
        {
            SwitchToAndroid();
            Player();
            Graphics();
            Quality();
            TagsAndLayers();
            XR(BuildTargetGroup.Android);
            XR(BuildTargetGroup.Standalone); // Quest Link: press Play in the editor with the headset on
            ExecutionOrder();
            bool restart = InputBackend();
            AssetDatabase.SaveAssets();
            Debug.Log("[Last Night] Quest 2 project settings applied.");
            return !restart;
        }

        static void SwitchToAndroid()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        }

        static void Player()
        {
            var android = NamedBuildTarget.Android;
            PlayerSettings.companyName = "atx702";
            PlayerSettings.productName = "Last Night";
            PlayerSettings.SetApplicationIdentifier(android, "com.atx702.lastnight");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29; // Quest 2 runs Android 12+
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Minimal);
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.runInBackground = false;
        }

        static void Graphics()
        {
            Directory.CreateDirectory(SettingsFolder);
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (asset == null)
            {
                asset = UniversalRenderPipelineAsset.Create();
                AssetDatabase.CreateAsset(asset, UrpAssetPath);
                asset.LoadBuiltinRendererData(); // saves Assets/UniversalRenderer.asset with URP's defaults
            }

            // Quest 2 budget: 4x MSAA, no HDR, no realtime shadows, no extra screen copies.
            asset.msaaSampleCount = 4;
            asset.supportsHDR = false;
            asset.renderScale = 1f;
            asset.shadowDistance = 0f;
            asset.shadowCascadeCount = 1;
            asset.maxAdditionalLightsCount = 2;
            asset.supportsCameraDepthTexture = false;
            asset.supportsCameraOpaqueTexture = false;
            asset.useSRPBatcher = true;
            EditorUtility.SetDirty(asset);

            GraphicsSettings.defaultRenderPipeline = asset;
        }

        static void Quality()
        {
            var asset = GraphicsSettings.defaultRenderPipeline;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = asset;
                QualitySettings.vSyncCount = 0;             // the headset paces frames
                QualitySettings.antiAliasing = 0;           // URP asset owns MSAA
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.pixelLightCount = 2;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                QualitySettings.lodBias = 1f;
                QualitySettings.skinWeights = SkinWeights.TwoBones;
            }
            QualitySettings.SetQualityLevel(current, false);
        }

        static void TagsAndLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            bool has = false;
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == RealWorldLayer) has = true;
            for (int i = 8; !has && i < layers.arraySize; i++)
            {
                var p = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(p.stringValue)) { p.stringValue = RealWorldLayer; has = true; }
            }
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static void XR(BuildTargetGroup group)
        {
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget perTarget) || perTarget == null)
            {
                Directory.CreateDirectory("Assets/XR");
                perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perTarget, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
            }
            if (!perTarget.HasManagerSettingsForBuildTarget(group))
                perTarget.CreateDefaultManagerSettingsForBuildTarget(group);

            var general = perTarget.SettingsForBuildTarget(group);
            general.InitManagerOnStart = true;
            XRPackageMetadataStore.AssignLoader(general.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", group);
            EditorUtility.SetDirty(general);

            var openxr = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (openxr == null)
            {
                // A fresh project (or a headless build) may not have created OpenXR's settings asset yet.
                var t = System.Type.GetType("UnityEditor.XR.OpenXR.OpenXRPackageSettings, Unity.XR.OpenXR.Editor");
                t?.GetMethod("GetOrCreateInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
                openxr = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            }
            if (openxr == null)
            {
                Debug.LogWarning("[Last Night] OpenXR settings aren't created yet. Open Edit > Project Settings > XR Plug-in Management once, then run Set Up again.");
                return;
            }
            openxr.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            openxr.depthSubmissionMode = OpenXRSettings.DepthSubmissionMode.Depth16Bit;

            Enable(group, OculusTouchControllerProfile.featureId);
            if (group == BuildTargetGroup.Android)
                Enable(group, MetaQuestFeature.featureId); // Quest 2 is in its target device list by default
            EditorUtility.SetDirty(openxr);
        }

        static void Enable(BuildTargetGroup group, string featureId)
        {
            var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(group, featureId);
            if (feature == null) { Debug.LogWarning($"[Last Night] OpenXR feature {featureId} not found for {group}."); return; }
            feature.enabled = true;
            EditorUtility.SetDirty(feature);
        }

        /// <summary>Singletons first, so everything that subscribes to them in OnEnable finds them.</summary>
        static void ExecutionOrder()
        {
            void Order(System.Type t, int order)
            {
                foreach (var s in MonoImporter.GetAllRuntimeMonoScripts())
                    if (s != null && s.GetClass() == t && MonoImporter.GetExecutionOrder(s) != order)
                        MonoImporter.SetExecutionOrder(s, order);
            }
            Order(typeof(LastNight.Core.GameManager), -100);
            Order(typeof(LastNight.Player.PlayerRig), -95);
            Order(typeof(LastNight.Audio.BeatClock), -90);
            Order(typeof(LastNight.Dialogue.DialoguePlayer), -90);
            Order(typeof(LastNight.Player.EyesClosed), -80);
        }

        /// <summary>Both input backends: XRI uses the Input System, a few Unity bits still poll the old one.</summary>
        static bool InputBackend()
        {
            var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settings.Length == 0) return false;
            var so = new SerializedObject(settings[0]);
            var p = so.FindProperty("activeInputHandler");
            if (p == null || p.intValue == 2) return false;
            p.intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
