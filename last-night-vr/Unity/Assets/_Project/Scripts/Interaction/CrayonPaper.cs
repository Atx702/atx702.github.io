using System.IO;
using UnityEngine;

namespace LastNight.Interaction
{
    /// <summary>
    /// A sheet of paper you can draw on with a crayon (Wren's Easel, and the crayons in Ch.8).
    /// Each Crayon's tip is checked against the paper's plane every frame; touching paints dots
    /// into a Texture2D. SavePng() writes the drawing to persistentDataPath/Drawings.
    /// The paper is a Quad (local X/Y = paper surface, 1x1 units scaled to size).
    /// </summary>
    public class CrayonPaper : MonoBehaviour
    {
        [SerializeField] Renderer paperRenderer;
        [SerializeField] Crayon[] crayons;
        [SerializeField] int resolution = 1024;
        [SerializeField] float touchDistance = 0.01f;
        [SerializeField] int brushRadius = 6;
        [SerializeField] Color paperColor = new Color(0.97f, 0.95f, 0.9f);

        Texture2D tex;
        Color32[] pixels;
        bool dirty;
        readonly Vector2?[] lastPoint = new Vector2?[16];

        void Awake()
        {
            tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            pixels = new Color32[resolution * resolution];
            Clear();
            paperRenderer.material.mainTexture = tex;
        }

        public void Clear()
        {
            var c = (Color32)paperColor;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            dirty = true;
        }

        void Update()
        {
            for (int i = 0; i < crayons.Length && i < lastPoint.Length; i++)
            {
                var tip = transform.InverseTransformPoint(crayons[i].Tip.position);
                bool touching = Mathf.Abs(tip.z * transform.lossyScale.z) < touchDistance &&
                                Mathf.Abs(tip.x) <= 0.5f && Mathf.Abs(tip.y) <= 0.5f;
                if (!touching) { lastPoint[i] = null; continue; }

                var uv = new Vector2(tip.x + 0.5f, tip.y + 0.5f) * resolution;
                if (lastPoint[i].HasValue) Line(lastPoint[i].Value, uv, crayons[i].Color);
                else Dot(uv, crayons[i].Color);
                lastPoint[i] = uv;
            }

            if (!dirty) return;
            tex.SetPixels32(pixels);
            tex.Apply(false);
            dirty = false;
        }

        void Line(Vector2 a, Vector2 b, Color32 c)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) / (brushRadius * 0.5f)));
            for (int s = 0; s <= steps; s++) Dot(Vector2.Lerp(a, b, s / (float)steps), c);
        }

        void Dot(Vector2 p, Color32 c)
        {
            int cx = (int)p.x, cy = (int)p.y, r2 = brushRadius * brushRadius;
            for (int y = -brushRadius; y <= brushRadius; y++)
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                if (x * x + y * y > r2 || Random.value < 0.25f) continue; // waxy, broken texture
                int px = cx + x, py = cy + y;
                if (px < 0 || py < 0 || px >= resolution || py >= resolution) continue;
                pixels[py * resolution + px] = c;
            }
            dirty = true;
        }

        public string SavePng()
        {
            var dir = Path.Combine(Application.persistentDataPath, "Drawings");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"easel_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());
            return path;
        }
    }
}
