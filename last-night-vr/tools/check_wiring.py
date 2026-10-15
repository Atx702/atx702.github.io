"""Check every Kit.Set / Kit.On field name in the scene builders against the real script fields.

    python tools/check_wiring.py

Kit.Set(target, "field", value) addresses serialized fields by name, which the C# compiler can't
check. This resolves each target's type from how the variable was created (AddComponent<T>,
Kit.Director<T>, Kit.Pull(...), ...) and confirms the field exists on that class or a base class.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "Unity" / "Assets" / "_Project"
SCRIPTS = ROOT / "Scripts"
EDITOR = ROOT / "Editor"

FACTORY = {  # Kit helper -> returned type
    "Kit.Pull": "PullController", "Kit.Memory": "MemoryDecay", "Kit.Spawn": "PlayerSpawnPoint",
    "Kit.Gesture": "GestureDetector", "Kit.Lines": "DialogueTrigger", "Kit.Trigger": "TriggerRelay",
    "Kit.Sound": "AudioSource", "Kit.Socket": "XRSocketInteractor", "Kit.Grabbable": "XRGrabInteractable",
    "Kit.Simple": "XRSimpleInteractable", "Kit.PointLight": "Light", "Kit.Text3D": "TextMeshPro",
    "CoreBuilder.Database": "DialogueDatabase",
}


def classes():
    """class -> (base, set of field names) for our runtime scripts."""
    out = {}
    for f in SCRIPTS.rglob("*.cs"):
        src = f.read_text()
        for m in re.finditer(r"class\s+(\w+)\s*(?::\s*([\w.]+))?", src):
            name, base = m.group(1), (m.group(2) or "").split(".")[-1]
            # fields: from this class's opening brace to the next top-level class (good enough per file)
            start = m.end()
            nxt = re.search(r"\n    (?:public|internal)\s+(?:abstract\s+|static\s+|sealed\s+)*class\s", src[start:])
            body = src[start: start + nxt.start()] if nxt else src[start:]
            fields = set()
            for d in re.finditer(r"(?:\[SerializeField\][^;\n]*?|public\s+)([\w.<>\[\],]+)\s+([\w\s,=.\-+f{}\"()]+?);", body):
                decl = d.group(2)
                for part in re.split(r",(?![^{]*})", decl):
                    n = part.split("=")[0].strip()
                    if re.fullmatch(r"\w+", n):
                        fields.add(n)
            out[name] = (base, fields)
    return out


def has_field(cls, field, table):
    while cls in table:
        base, fields = table[cls]
        if field in fields:
            return True
        cls = base
    return False


def main():
    table = classes()
    problems, checked = [], 0
    for f in sorted(EDITOR.rglob("*.cs")):
        src = f.read_text()
        types = {}
        for m in re.finditer(r"var\s+(\w+)\s*=\s*([^;]*?)AddComponent<(\w+)>", src):
            types[m.group(1)] = m.group(3)
        for m in re.finditer(r"var\s+(\w+)\s*=\s*(?:Kit\.)?(?:Director|Add|GetComponent|GetComponentInChildren)<(\w+)>", src):
            types[m.group(1)] = m.group(2)
        for m in re.finditer(r"var\s+(\w+)\s*=\s*([\w.]+)\(", src):
            if m.group(2) in FACTORY:
                types[m.group(1)] = FACTORY[m.group(2)]
        for m in re.finditer(r"\b(\w+)\s+(\w+)\s*=", src):
            if m.group(1) in table:
                types[m.group(2)] = m.group(1)
        for m in re.finditer(r"Kit\.(Set|On|OnBool)(?:<\w+>)?\(\s*([^,]+?),\s*\"(\w+)\"", src):
            target, field = m.group(2).strip(), m.group(3)
            inline = re.search(r"AddComponent<(\w+)>\(\)$", target) or re.search(r"GetComponent<(\w+)>\(\)$", target)
            t = inline.group(1) if inline else types.get(target)
            if t is None or t not in table:
                continue  # Unity/XRI types or unknown: Kit.Set throws at build time anyway
            checked += 1
            if not has_field(t, field, table):
                line = src[: m.start()].count("\n") + 1
                problems.append(f"{f.relative_to(ROOT)}:{line}: {t} has no field '{field}'")
    for p in problems:
        print(p)
    print(f"checked {checked} field references, {len(problems)} problems")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
