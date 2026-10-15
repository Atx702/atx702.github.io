using UnityEditor;
using UnityEngine;

namespace LastNight.EditorTools
{
    /// <summary>
    /// Import settings that keep the game inside Quest 2's memory and bandwidth: textures capped and
    /// ASTC-compressed, voice lines compressed in memory, music and ambience streamed, meshes compressed.
    /// Applied automatically to everything under Assets/_Project.
    /// </summary>
    public class QuestImportRules : AssetPostprocessor
    {
        static bool Ours(string path) => path.StartsWith("Assets/_Project/");

        void OnPreprocessTexture()
        {
            if (!Ours(assetPath)) return;
            var ti = (TextureImporter)assetImporter;
            ti.mipmapEnabled = true;
            ti.maxTextureSize = assetPath.Contains("/Drawings/") ? 512 : 1024;
            var android = ti.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = ti.maxTextureSize;
            android.format = TextureImporterFormat.ASTC_6x6;
            ti.SetPlatformTextureSettings(android);
        }

        static readonly string[] Streamed = { "MUS_", "wind", "cicadas", "hum", "static", "underwater", "rain", "reversed" };

        void OnPreprocessAudio()
        {
            if (!Ours(assetPath)) return;
            var ai = (AudioImporter)assetImporter;
            var s = ai.defaultSampleSettings;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            bool streamed = false;
            foreach (var key in Streamed) if (assetPath.Contains(key)) streamed = true;
            if (assetPath.Contains("/VO/"))
            {
                s.loadType = AudioClipLoadType.CompressedInMemory;
                s.quality = 0.55f;
                ai.forceToMono = true;
            }
            else if (streamed)
            {
                s.loadType = AudioClipLoadType.Streaming;
                s.quality = 0.6f;
            }
            else
            {
                s.loadType = AudioClipLoadType.DecompressOnLoad;
                s.quality = 0.7f;
                ai.forceToMono = true;
            }
            ai.defaultSampleSettings = s;
            ai.loadInBackground = true;
        }

        void OnPreprocessModel()
        {
            if (!Ours(assetPath)) return;
            var mi = (ModelImporter)assetImporter;
            mi.importAnimation = false;
            mi.importCameras = false;
            mi.importLights = false;
            mi.isReadable = false;
            mi.meshCompression = ModelImporterMeshCompression.Medium;
            mi.addCollider = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        }
    }
}
