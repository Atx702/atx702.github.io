using UnityEngine;

namespace LastNight.EditorTools
{
    /// <summary>Walkable architecture from blocks: walls with door gaps, floors, gable roofs.</summary>
    public static class Rooms
    {
        public struct Gap
        {
            public float at, width, height;
            public Gap(float at, float width = 1.1f, float height = 2.1f) { this.at = at; this.width = width; this.height = height; }
        }

        /// <summary>
        /// An axis-aligned wall from <paramref name="a"/> to <paramref name="b"/> (floor points), with door gaps.
        /// Gap.at is the distance from <paramref name="a"/> along the wall to the gap's centre.
        /// </summary>
        public static GameObject Wall(string name, Transform parent, Vector3 a, Vector3 b, float height, Color color, float thickness = 0.15f, params Gap[] gaps)
        {
            var root = Kit.Go(name, parent);
            var dir = b - a;
            float length = dir.magnitude;
            var axis = dir / length;
            bool alongX = Mathf.Abs(axis.x) > Mathf.Abs(axis.z);

            void Piece(float from, float to, float y0, float y1)
            {
                if (to - from < 0.01f || y1 - y0 < 0.01f) return;
                var mid = a + axis * ((from + to) / 2f) + Vector3.up * ((y0 + y1) / 2f);
                var size = alongX ? new Vector3(to - from, y1 - y0, thickness) : new Vector3(thickness, y1 - y0, to - from);
                Kit.Block("Wall", root.transform, mid, size, color);
            }

            System.Array.Sort(gaps, (g1, g2) => g1.at.CompareTo(g2.at));
            float cursor = 0f;
            foreach (var g in gaps)
            {
                float g0 = g.at - g.width / 2f, g1 = g.at + g.width / 2f;
                Piece(cursor, g0, 0f, height);
                Piece(g0, g1, g.height, height); // lintel
                cursor = g1;
            }
            Piece(cursor, length, 0f, height);
            return root;
        }

        /// <summary>A rectangular room (four walls + floor), min corner at <paramref name="min"/>. Gaps per side: south (-z), north (+z), west (-x), east (+x).</summary>
        public static GameObject Box(string name, Transform parent, Vector3 min, Vector2 size, float height, Color wall, Color floor,
            Gap[] south = null, Gap[] north = null, Gap[] west = null, Gap[] east = null, bool ceiling = true, Color? ceilingColor = null)
        {
            var root = Kit.Go(name, parent);
            var max = min + new Vector3(size.x, 0, size.y);
            Kit.Floor("Floor", root.transform, new Vector3((min.x + max.x) / 2, min.y + 0.02f, (min.z + max.z) / 2), size, floor);
            Wall("South Wall", root.transform, new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), height, wall, gaps: south ?? new Gap[0]);
            Wall("North Wall", root.transform, new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), height, wall, gaps: north ?? new Gap[0]);
            Wall("West Wall", root.transform, new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z), height, wall, gaps: west ?? new Gap[0]);
            Wall("East Wall", root.transform, new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), height, wall, gaps: east ?? new Gap[0]);
            if (ceiling)
                Kit.Block("Ceiling", root.transform, new Vector3((min.x + max.x) / 2, min.y + height + 0.05f, (min.z + max.z) / 2),
                    new Vector3(size.x + 0.3f, 0.1f, size.y + 0.3f), ceilingColor ?? wall * 0.95f, false);
            return root;
        }

        /// <summary>A simple gable roof over a footprint (ridge along x).</summary>
        public static void Roof(Transform parent, Vector3 min, Vector2 size, float wallHeight, Color color, float pitch = 28f)
        {
            float half = size.y / 2f + 0.4f;
            float slope = half / Mathf.Cos(pitch * Mathf.Deg2Rad);
            float rise = half * Mathf.Tan(pitch * Mathf.Deg2Rad);
            var cx = min.x + size.x / 2f;
            var cz = min.z + size.y / 2f;
            for (int s = -1; s <= 1; s += 2)
            {
                var b = Kit.Block("Roof", parent, new Vector3(cx, min.y + wallHeight + rise / 2f, cz + s * half / 2f),
                    new Vector3(size.x + 0.8f, 0.08f, slope), color, false);
                b.transform.localRotation = Quaternion.Euler(s * pitch, 0, 0);
            }
        }
    }
}
