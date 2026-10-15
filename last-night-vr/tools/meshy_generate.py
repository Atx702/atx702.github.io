"""Generate every 3D model in docs/07_Art_Assets_Meshy.md with the Meshy API.

Usage:
    export MESHY_API_KEY=...          # from meshy.ai -> Settings -> API
    python tools/meshy_generate.py --dry-run          # list what would be made and the cost
    python tools/meshy_generate.py                    # make everything
    python tools/meshy_generate.py --only "Kettle"    # make one asset (name from the doc)

For each quoted prompt in the doc's tables it runs a Meshy text-to-3D preview (the mesh),
then a refine (PBR textures), and saves GLB + FBX to
Unity/Assets/_Project/Art/Models/<Section>/<Asset>.{glb,fbx}.
Rows without a quoted prompt ("reuse ...", "Not Meshy") are skipped.

Progress is kept in tools/meshy_tasks.json, so the script can be stopped and re-run:
finished assets are skipped and running tasks are picked up again, never re-paid for.
To re-roll one asset, delete its entry (keyed Section/Asset) from that file and its files, then run with --only.
"""
import argparse
import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
DOC = ROOT / "docs" / "07_Art_Assets_Meshy.md"
OUT = ROOT / "Unity" / "Assets" / "_Project" / "Art" / "Models"
STATE = ROOT / "tools" / "meshy_tasks.json"
API = "https://api.meshy.ai/openapi/v2/text-to-3d"

# Triangle targets from the doc's budget table; Blender decimates further if needed.
LARGE = ("bed", "house", "counter", "vending", "shelter", "train", "corridor", "gurney", "dresser", "dock", "chalkboard")
MEDIUM = ("chair", "desk", "table", "radio", "door", "rack", "nightstand", "fridge", "bench", "board", "lamp", "monitor",
          "bag", "speaker", "dog", "bin", "floorboard", "lantern")

# Meshy credit costs per task (preview, refine) at the time of writing.
COST_PREVIEW, COST_REFINE = 20, 10


def slug(name):
    return re.sub(r"[^A-Za-z0-9]+", "_", name).strip("_")


def parse_doc():
    """Return [{section, name, prompt, character}] from the prompt tables."""
    assets, section = [], None
    for line in DOC.read_text(encoding="utf-8").splitlines():
        if line.startswith("### "):
            section = line[4:].split("(")[0].strip()
            continue
        if section is None or not line.startswith("| "):
            continue
        cells = [c.strip() for c in line.strip("|").split("|")]
        if len(cells) < 2:
            continue
        quoted = re.findall(r'"([^"]+)"', cells[1])
        if not quoted:
            continue
        name = re.sub(r"\s*\(×\d+\)", "", cells[0]).replace("+", "and").strip()
        if cells[1].startswith("(reuse"):  # "(reuse the bed, plus) "Dresser..."" -> name the new piece
            name = "Dresser"
        assets.append({
            "section": slug(section.split(":")[-1]),
            "name": name,
            "prompt": quoted[0],
            "character": section.startswith("Characters"),
        })
    return assets


def key_of(asset):
    return f"{asset['section']}/{slug(asset['name'])}"


def polycount(asset):
    if asset["character"]:
        return 20000
    n = asset["name"].lower()
    if any(k in n for k in LARGE):
        return 8000
    if any(k in n for k in MEDIUM):
        return 4000
    return 1500


def call(method, url, key, body=None):
    req = urllib.request.Request(url, method=method, headers={
        "Authorization": f"Bearer {key}", "Content-Type": "application/json"})
    data = json.dumps(body).encode() if body is not None else None
    for attempt in range(5):
        try:
            with urllib.request.urlopen(req, data=data, timeout=60) as r:
                return json.loads(r.read())
        except urllib.error.HTTPError as e:
            msg = e.read().decode(errors="replace")
            if e.code in (429, 500, 502, 503) and attempt < 4:
                time.sleep(2 ** (attempt + 2))
                continue
            raise RuntimeError(f"{method} {url} -> {e.code}: {msg}") from None
        except urllib.error.URLError:
            if attempt == 4:
                raise
            time.sleep(2 ** (attempt + 2))


def wait(task_id, key, label):
    while True:
        t = call("GET", f"{API}/{task_id}", key)
        status = t.get("status")
        if status == "SUCCEEDED":
            return t
        if status in ("FAILED", "CANCELED", "EXPIRED"):
            raise RuntimeError(f"{label}: {status} {t.get('task_error')}")
        print(f"    {label}: {status} {t.get('progress', 0)}%", flush=True)
        time.sleep(15)


def save_state(state):
    STATE.write_text(json.dumps(state, indent=2) + "\n")


def make(asset, key, state):
    entry = state.setdefault(key_of(asset), {})
    if not entry.get("preview"):
        body = {
            "mode": "preview",
            "prompt": asset["prompt"],
            "art_style": "realistic",
            "should_remesh": True,
            "topology": "triangle",
            "target_polycount": polycount(asset),
        }
        if asset["character"]:
            body["pose_mode"] = "a-pose"
        entry["preview"] = call("POST", API, key, body)["result"]
        save_state(state)  # logged before waiting, so a crash never pays twice
    wait(entry["preview"], key, "mesh")

    if not entry.get("refine"):
        entry["refine"] = call("POST", API, key, {
            "mode": "refine", "preview_task_id": entry["preview"], "enable_pbr": True})["result"]
        save_state(state)
    done = wait(entry["refine"], key, "textures")

    folder = OUT / asset["section"]
    folder.mkdir(parents=True, exist_ok=True)
    for fmt in ("glb", "fbx"):
        url = done.get("model_urls", {}).get(fmt)
        if url:
            with urllib.request.urlopen(url, timeout=300) as r:
                (folder / f"{slug(asset['name'])}.{fmt}").write_bytes(r.read())
    entry["done"] = True
    save_state(state)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--only", help="asset name as written in the doc")
    args = ap.parse_args()

    assets = parse_doc()
    if args.only:
        assets = [a for a in assets if args.only.lower() in (a["name"].lower(), key_of(a).lower())]
        if not assets:
            sys.exit(f"no asset named {args.only!r}; run --dry-run for the list")
    state = json.loads(STATE.read_text()) if STATE.exists() else {}
    todo = [a for a in assets if not state.get(key_of(a), {}).get("done")]

    print(f"{len(assets)} assets in the doc, {len(todo)} still to make, "
          f"about {len(todo) * (COST_PREVIEW + COST_REFINE)} Meshy credits")
    if args.dry_run:
        for a in todo:
            print(f"  {a['section']}/{slug(a['name'])}  ({polycount(a)} tris)  {a['prompt']}")
        return 0

    key = os.environ.get("MESHY_API_KEY")
    if not key:
        sys.exit("set MESHY_API_KEY first (meshy.ai -> Settings -> API)")
    failed = []
    for i, a in enumerate(todo, 1):
        print(f"[{i}/{len(todo)}] {a['section']}/{a['name']}", flush=True)
        try:
            make(a, key, state)
        except Exception as e:  # report and keep going
            print(f"    failed: {e}")
            failed.append(key_of(a))
    print(f"done; {len(todo) - len(failed)} made, {len(failed)} failed")
    for n in failed:
        print(f"  failed: {n}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
