"""Download generated sound effects and music into the Unity project.

Usage:
    python tools/fetch_audio.py STATUS_JSON [STATUS_JSON ...]

Same idea as fetch_vo.py, but reads tools/audio_sessions.txt (AUDIO_ID,session_id) and
saves SFX_* to Audio/SFX/ and MUS_* to Audio/Music/. Prompts and settings for every
sound are in tools/audio_manifest.json.
"""
import json
import sys
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
AUDIO = ROOT / "Unity" / "Assets" / "_Project" / "Audio"
SESSIONS = ROOT / "tools" / "audio_sessions.txt"


def main(paths):
    by_session = dict(reversed(l.strip().split(",")) for l in SESSIONS.read_text().splitlines() if l.strip())
    saved, failed = 0, []
    for p in paths:
        data = json.loads(Path(p).read_text())
        for item in data.get("media", []) + data.get("generations", []):
            url = item.get("url") or item.get("content_url")
            session = next((s for s in by_session if url and f"/{s}/" in url), None)
            if session is None:
                continue
            audio_id = by_session[session]
            folder = AUDIO / ("Music" if audio_id.startswith("MUS_") else "SFX")
            folder.mkdir(parents=True, exist_ok=True)
            target = folder / f"{audio_id}.mp3"
            if target.exists() and target.stat().st_size > 1000:
                continue
            try:
                with urllib.request.urlopen(url, timeout=120) as r:
                    body = r.read()
                if len(body) < 1000:
                    raise ValueError(f"only {len(body)} bytes")
                target.write_bytes(body)
                saved += 1
            except Exception as e:
                failed.append((audio_id, str(e)))
    have = len(list((AUDIO / "SFX").glob("*.mp3"))) + len(list((AUDIO / "Music").glob("*.mp3")))
    print(f"saved {saved} new files; {have}/{len(by_session)} sounds now downloaded")
    for a, err in failed:
        print(f"  failed {a}: {err}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
