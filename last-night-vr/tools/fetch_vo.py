"""Download recorded voice lines into the Unity project.

Usage:
    python tools/fetch_vo.py STATUS_JSON [STATUS_JSON ...]

Each STATUS_JSON is the saved output of ElevenLabs `creative_get_flow_run_status`.
Takes are matched to script lines through tools/vo_sessions.txt (LINE_ID,session_id):
the session id appears in each take's content URL. Files are saved as
Unity/Assets/_Project/Audio/Resources/VO/<LINE_ID>.mp3, which DialogueDatabase loads by ID.
Download links expire about two hours after the status call, so run this right away.
"""
import json
import sys
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "Unity" / "Assets" / "_Project" / "Audio" / "Resources" / "VO"
SESSIONS = ROOT / "tools" / "vo_sessions.txt"


def main(paths):
    by_session = {}
    for line in SESSIONS.read_text().splitlines():
        if line.strip():
            line_id, session = line.strip().split(",")
            by_session[session] = line_id

    OUT.mkdir(parents=True, exist_ok=True)
    saved, failed = 0, []
    for p in paths:
        data = json.loads(Path(p).read_text())
        for item in data.get("media", []) + data.get("generations", []):
            url = item.get("url") or item.get("content_url")
            if not url:
                continue
            session = next((s for s in by_session if f"/{s}/" in url), None)
            if session is None:
                continue
            target = OUT / f"{by_session[session]}.mp3"
            if target.exists() and target.stat().st_size > 1000:
                continue
            try:
                with urllib.request.urlopen(url, timeout=60) as r:
                    body = r.read()
                if len(body) < 1000:
                    raise ValueError(f"only {len(body)} bytes")
                target.write_bytes(body)
                saved += 1
            except Exception as e:  # report and keep going
                failed.append((by_session[session], str(e)))
    have = len(list(OUT.glob("*.mp3")))
    print(f"saved {saved} new files; {have}/{len(by_session)} lines now have audio")
    for line_id, err in failed:
        print(f"  failed {line_id}: {err}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
