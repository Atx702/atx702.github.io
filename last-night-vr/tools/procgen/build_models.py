"""Build every model Meshy didn't make (props + characters + The Pull) into the Unity project.

    python tools/procgen/build_models.py            # build all, print a budget report
    python tools/procgen/build_models.py Kettle     # build one

Writes <Name>.glb + <Name>.obj + <Name>.mtl into Unity/Assets/_Project/Art/Models/<Section>/.
Skips any model Meshy already made for that name (so a later Meshy run is never overwritten
by these stand-ins unless you pass --force).
"""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from characters import CHARACTERS  # noqa: E402
from props import PROPS  # noqa: E402

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Unity" / "Assets" / "_Project" / "Art" / "Models"
RENAME = {"Mug": "Mugs", "Wedding_ring": "Wedding_ring_leak", "Patient_bracelet": "Patient_bracelet_leak"}
BUDGET = {"Characters": 25000}


def main(argv):
    force = "--force" in argv
    only = [a for a in argv if not a.startswith("--")]
    rows, bad = [], 0
    for section, fn in PROPS + CHARACTERS:
        m = fn()
        m.name = RENAME.get(m.name, m.name)
        if only and m.name not in only:
            continue
        if (OUT / section / f"{m.name}.fbx").exists() and not force:
            print(f"skip {section}/{m.name} (Meshy model exists)")
            continue
        m.save(OUT / section)
        lo, hi = m.bounds()
        size = hi - lo
        tris = m.tri_count()
        budget = BUDGET.get(section, 10000 if m.name in ("Corridor_module", "Kitchen_counter_set") else 5000)
        flag = "" if tris <= budget else "  OVER BUDGET"
        bad += bool(flag)
        rows.append((section, m.name, tris, size))
        print(f"{section}/{m.name:24s} {tris:6d} tris  {size[0]:.2f} x {size[1]:.2f} x {size[2]:.2f} m  floor y={lo[1]:+.2f}{flag}")
    print(f"{len(rows)} models built, {bad} over budget")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
