"""Extract every dialogue line from docs/02_Script.md into a CSV for Unity and ElevenLabs.

Usage (from the last-night-vr folder):
    python tools/extract_lines.py

Line format in the script:
    - **PRO_001** VOICE — *(softly)* "Hello, Tomas."
"""
import csv
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SCRIPT = ROOT / "docs" / "02_Script.md"
OUT = ROOT / "Unity" / "Assets" / "_Project" / "Data" / "dialogue.csv"

LINE_RE = re.compile(
    r'^- \*\*(?P<id>[A-Z0-9_]+)\*\* (?P<speaker>[A-Z_]+) — '
    r'(?:\*\((?P<direction>[^)]*)\)\* )?"(?P<text>.*)"\s*$'
)
CHAPTER_RE = re.compile(r"^## (CHAPTER \d+|SECRET ENDING|IDLE|EASTER EGG)")

LEAK_HINT = "leak"


def main() -> int:
    rows = []
    seen = set()
    chapter = "PROLOGUE"
    in_code = False
    for n, raw in enumerate(SCRIPT.read_text(encoding="utf-8").splitlines(), 1):
        if raw.startswith("```"):
            in_code = not in_code
            continue
        if in_code:
            continue
        heading = CHAPTER_RE.match(raw)
        if heading:
            chapter = heading.group(1).replace(" ", "_")
            continue
        m = LINE_RE.match(raw)
        if not m:
            if raw.startswith("- **") and "— " in raw:
                print(f"warning: line {n} looks like dialogue but didn't parse: {raw[:60]}", file=sys.stderr)
            continue
        line_id = m.group("id")
        if line_id in seen:
            print(f"error: duplicate line id {line_id} at line {n}", file=sys.stderr)
            return 1
        seen.add(line_id)
        direction = m.group("direction") or ""
        is_leak = LEAK_HINT in direction.lower()
        rows.append({
            "id": line_id,
            "chapter": chapter,
            "speaker": m.group("speaker"),
            "leak": "1" if is_leak else "0",
            "direction": direction,
            "text": m.group("text"),
        })

    OUT.parent.mkdir(parents=True, exist_ok=True)
    with OUT.open("w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=["id", "chapter", "speaker", "leak", "direction", "text"])
        writer.writeheader()
        writer.writerows(rows)

    by_speaker = {}
    for r in rows:
        by_speaker[r["speaker"]] = by_speaker.get(r["speaker"], 0) + 1
    print(f"wrote {len(rows)} lines to {OUT.relative_to(ROOT)}")
    for speaker, count in sorted(by_speaker.items(), key=lambda kv: -kv[1]):
        print(f"  {speaker:10} {count}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
