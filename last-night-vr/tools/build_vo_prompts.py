"""Builds ElevenLabs prompts for every script line: tools/vo_prompts.json.
Delivery directions become inline v4 tags like [whispering]; trigger notes (if/after/tile...) are dropped."""
import csv, json, re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
VOICES = {
    "VOICE": "OKqOM06abBsLyb8k3fXw",      # Yogi - Calm & Meditative
    "MAREN": "U7QdEsfRT6SXMGfRQbyN",      # Heather Faith Cubed
    "MAREN_MEM": "U7QdEsfRT6SXMGfRQbyN",
    "WREN": "MkTSSXNgnBULS6ek4pon",       # Tilly
    "TOMAS": "cjVigY5qzO86Huf0OWal",      # Eric
    "ELIO": "IP2syKL31S2JthzSSfZH",       # Ivan
    "LUCIA": "7DTpboyigwqXiCkvQjMX",      # Carli
    "ADAIR": "CH8NA3Iu30hBBcNIzv9I",      # Marilyn
    "NURSE": "qY71f9HwmIHc4kJb8RLH",      # Faye
    "DISPATCH": "iGjiBXtLd3IyXUaAQk6M",   # Kathryn
    "OKAFOR": "QZA1myOcnbLYY9uuhixV",     # Dean
    "LIND": "GxGSne3qX4QsbtuEIkbJ",       # Isabelle
}
TRIGGER = re.compile(r"^(if|after|when|tile|speaker|lucid|all |player|platform|rubber|biscuit|gives|wrong station|"
                     r"memory$|leak$|act |ch\.|through|from |inside|over him|under|halfway|reading|young|far away|"
                     r"echoing|close$|phone)", re.I)
KEEP_EXTRA = {"far away": "calling from far away", "echoing": "echoing",
              "through the window": "calling", "over him": "gently", "reading": "reading slowly",
              "under her breath": "under her breath"}

def tags(direction):
    out = []
    for part in [p.strip() for p in direction.split(",") if p.strip()]:
        low = part.lower()
        if low in ("leak", "memory", "memory version"):
            continue
        if any(ch.isdigit() for ch in part):
            continue
        if low.startswith(("if ", "after ", "when ", "tile", "speaker", "lucid", "all ", "player", "platform",
                           "rubber", "biscuit", "gives", "wrong station", "act ", "halfway", "ch.")):
            continue
        tag = KEEP_EXTRA.get(low, low)
        if low in DROP or tag in out:
            continue
        out.append(tag)
    return "".join(f"[{t}] " for t in out)

DROP = {"boat complete", "under the white flash", "from the dark hallway", "very clear for one second",
        "clearer than ever", "inside the pull's sound", "over her", "loud now", "new game+", "first line",
        "pull first seen", "lake", "phone speaker", "distant siren", "not looking up", "close", "young",
        "through the window", "calling", "distant"}

def clean(text):
    # Inline pauses written as: Alvarez." *(pause)* "Bennett.
    return re.sub(r'"\s*\*\(pause\)\*\s*"', " ... ", text)

rows = list(csv.DictReader(open(ROOT / "Unity/Assets/_Project/Data/dialogue.csv", encoding="utf-8")))
items = [{"id": r["id"], "speaker": r["speaker"], "voice_id": VOICES[r["speaker"]],
          "prompt": (tags(r["direction"]) + clean(r["text"])).strip()} for r in rows]
(ROOT / "tools/vo_prompts.json").write_text(json.dumps(items, indent=1, ensure_ascii=False))
print(len(items))
for i in items: print(i["id"], "|", i["prompt"])
