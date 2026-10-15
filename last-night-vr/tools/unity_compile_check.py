"""Compile the Unity project's C# without the Unity editor, the way Unity's script pipeline does.

Needs (paths via environment or defaults below):
  UNITY_EDITOR_DATA  Editor/Data folder of a Unity 6000.0 install (only Managed/, NetStandard/ and
                     Resources/PackageManager/BuiltInPackages/ are used; no licence needed)
  UNITY_PKGS         folder holding registry packages unpacked as <name>/ (versions from manifest.json)
  DOTNET             dotnet executable (any SDK >= 6; uses its Roslyn csc)

    python tools/unity_compile_check.py            # editor pass (what you get when opening the project)
    python tools/unity_compile_check.py --player   # player pass (Android/Quest build, no UNITY_EDITOR)

Every .asmdef in the packages and in Assets/ becomes one assembly, compiled in dependency order with
Unity's scripting defines and each asmdef's versionDefines/defineConstraints. Scripts outside any
asmdef go into Assembly-CSharp (and Editor folders into Assembly-CSharp-Editor), like Unity.
Prints errors for our own code; package errors are summarised (they usually mean a missing define).
"""
import glob
import json
import os
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "Unity"
DATA = Path(os.environ.get("UNITY_EDITOR_DATA", "/home/user/unityref/Editor/Data"))
PKGS = Path(os.environ.get("UNITY_PKGS", "/home/user/unitypkgs"))
DOTNET = os.environ.get("DOTNET", "/home/user/dotnet/dotnet")
OUT = Path(os.environ.get("UNITY_CHECK_OUT", "/home/user/unitycheck/out"))
BUILTIN = DATA / "Resources" / "PackageManager" / "BuiltInPackages"
UNITY_VERSION = (6000, 0, 84)

# Packages whose source only compiles inside Unity (Burst/IL post-processing, HLSL generators,
# InternalsVisibleTo chains). They are treated as not installed; the few render-pipeline types our
# code touches come from tools/compile_stubs/ instead (signatures copied from the real source).
NOT_COMPILED = ("com.unity.burst", "com.unity.collections", "com.unity.render-pipelines.",
                "com.unity.shadergraph", "com.unity.rendering.light-transport", "com.unity.xr.hands",
                "com.unity.visualeffectgraph", "com.unity.searcher", "com.unity.nuget.mono-cecil",
                "com.unity.test-framework", "com.unity.ext.nunit")
STUBS = Path(__file__).resolve().parent / "compile_stubs"
ALIASES = {"Unity.ugui": "UnityEngine.UI"}
# Unity 6 references uGUI from every assembly (and its editor half from every editor-time compile)
IMPLICIT = ("UnityEngine.UI",) if "--player" in sys.argv else ("UnityEngine.UI", "UnityEditor.UI")

PLAYER = "--player" in sys.argv


def csc_path():
    sdk = sorted(glob.glob(str(Path(DOTNET).parent / "sdk" / "*" / "Roslyn" / "bincore" / "csc.dll")))
    return sdk[-1]


def vkey(v):
    m = re.match(r"(\d+)(?:\.(\d+))?(?:\.(\d+))?", v.strip())
    return tuple(int(x or 0) for x in m.groups()) if m else (0, 0, 0)


def in_range(version, expr):
    """Unity versionDefines expression: '1.2.3', '[1.0,2.0)', '(1.0,]', etc."""
    v = vkey(version)
    expr = expr.strip()
    if not expr:
        return True
    if expr[0] not in "[(":
        return v >= vkey(expr)
    lo, hi = expr[1:-1].split(",") if "," in expr else (expr[1:-1], expr[1:-1])
    ok = True
    if lo.strip():
        ok &= v >= vkey(lo) if expr[0] == "[" else v > vkey(lo)
    if hi.strip():
        ok &= v <= vkey(hi) if expr[-1] == "]" else v < vkey(hi)
    return ok


def package_versions():
    manifest = json.loads((ROOT / "Packages" / "manifest.json").read_text())["dependencies"]
    vers = {}
    roots = {}
    for name in sorted(os.listdir(BUILTIN)):
        pj = BUILTIN / name / "package.json"
        if pj.exists():
            vers[name] = json.loads(pj.read_text())["version"]
            roots[name] = BUILTIN / name
    for name in os.listdir(PKGS):
        pj = PKGS / name / "package.json"
        if pj.exists():
            vers[name] = json.loads(pj.read_text())["version"]
            roots[name] = PKGS / name
    # only packages reachable from the manifest count as installed
    installed, todo = {}, list(manifest)
    while todo:
        n = todo.pop()
        if n in installed or n not in vers:
            continue
        installed[n] = vers[n]
        deps = json.loads((roots[n] / "package.json").read_text()).get("dependencies", {})
        todo += list(deps)
    installed = {n: v for n, v in installed.items() if not n.startswith(NOT_COMPILED)}
    return installed, {n: roots[n] for n in installed}


def unity_defines():
    d = ["UNITY_6000_0_OR_NEWER", "UNITY_6000_0", "UNITY_6000", f"UNITY_6000_0_{UNITY_VERSION[2]}",
         "UNITY_5_3_OR_NEWER", "UNITY_5_4_OR_NEWER", "UNITY_5_5_OR_NEWER", "UNITY_5_6_OR_NEWER"]
    for y in range(2017, 2024):
        for m in range(1, 5):
            if y == 2023 and m > 3:
                break
            d.append(f"UNITY_{y}_{m}_OR_NEWER")
    d += ["PLATFORM_ARCH_64", "UNITY_64", "UNITY_INCLUDE_TESTS_DISABLED", "ENABLE_AUDIO", "ENABLE_CLOTH",
          "ENABLE_DIRECTOR", "ENABLE_PHYSICS", "ENABLE_UNITYEVENTS", "ENABLE_VR", "ENABLE_XR_MODULE",
          "ENABLE_INPUT_SYSTEM", "ENABLE_LEGACY_INPUT_MANAGER", "ENABLE_MONO" if not PLAYER else "ENABLE_IL2CPP",
          "NET_STANDARD_2_1", "NET_STANDARD", "NETSTANDARD2_1", "NETSTANDARD", "CSHARP_7_3_OR_NEWER",
          "UNITY_ANDROID", "PLATFORM_ANDROID", "UNITY_POST_PROCESSING_STACK_V2_DISABLED",
          "ENABLE_UNITYWEBREQUEST", "ENABLE_CLOUD_SERVICES", "ENABLE_RUNTIME_GI", "ENABLE_PROFILER" if not PLAYER else "UNITY_ANDROID_PLAYER",
          "UNITY_ASSERTIONS", "DEVELOPMENT_BUILD" if PLAYER else "UNITY_EDITOR_LINUX"]
    if not PLAYER:
        d += ["UNITY_EDITOR", "UNITY_EDITOR_64", "UNITY_EDITOR_LINUX"]
    return d


def is_managed(dll):
    try:
        b = Path(dll).read_bytes()[:4096]
        pe = int.from_bytes(b[0x3C:0x40], "little")
        magic = int.from_bytes(b[pe + 24:pe + 26], "little")
        dd = pe + 24 + (112 if magic == 0x20B else 96)
        clr = int.from_bytes(b[dd + 14 * 8: dd + 14 * 8 + 4], "little")
        return clr != 0
    except Exception:
        return False


class Asm:
    def __init__(self, path, data, pkg):
        self.path, self.data, self.pkg = Path(path), data, pkg
        self.name = data["name"]
        self.dir = self.path.parent
        meta = Path(str(path) + ".meta")
        self.guid = re.search(r"guid: (\w+)", meta.read_text()).group(1) if meta.exists() else None
        self.sources = []
        self.editor_only = data.get("includePlatforms") == ["Editor"]


def collect_asmdefs(pkg_roots):
    asms = []
    for pkg, root in pkg_roots.items():
        for a in root.rglob("*.asmdef"):
            rel = a.relative_to(root).as_posix().lower()
            if any(part.endswith("~") or part == "Tests" for part in a.relative_to(root).parts):
                continue
            asms.append(Asm(a, json.loads(a.read_text()), pkg))
    for a in (ROOT / "Assets").rglob("*.asmdef"):
        asms.append(Asm(a, json.loads(a.read_text()), None))
    return asms


def assign_sources(asms, roots):
    """Each .cs belongs to the asmdef in its closest parent folder."""
    by_dir = {a.dir: a for a in asms}
    loose = {"Assembly-CSharp": [], "Assembly-CSharp-Editor": []}
    for base in list(roots) + [ROOT / "Assets"]:
        for cs in Path(base).rglob("*.cs"):
            parts = cs.relative_to(base).parts
            if any(p.endswith("~") or p.startswith(".") for p in parts):
                continue
            d = cs.parent
            owner = None
            while True:
                if d in by_dir:
                    owner = by_dir[d]; break
                if d == Path(base) or d.parent == d:
                    break
                d = d.parent
            if owner:
                owner.sources.append(cs)
            elif base == ROOT / "Assets":
                key = "Assembly-CSharp-Editor" if "Editor" in parts else "Assembly-CSharp"
                loose[key].append(cs)
    return loose


def main():
    installed, roots = package_versions()
    asms = collect_asmdefs(roots)
    loose = assign_sources(asms, roots.values())
    by_name = {a.name: a for a in asms}
    by_guid = {a.guid: a for a in asms if a.guid}
    base_defines = unity_defines()

    engine_refs = [p for p in glob.glob(str(DATA / "Managed" / "UnityEngine" / "*.dll"))
                   if PLAYER is False or not Path(p).name.startswith("UnityEditor")]
    netstd = [str(DATA / "NetStandard" / "ref" / "2.1.0" / "netstandard.dll")] + \
        glob.glob(str(DATA / "NetStandard" / "compat" / "2.1.0" / "shims" / "netstandard" / "*.dll"))
    plugin_dlls = {}
    for pkg, root in roots.items():
        for dll in root.rglob("*.dll"):
            if any(p.endswith("~") for p in dll.relative_to(root).parts):
                continue
            if "Editor" in dll.parts and PLAYER:
                continue
            if is_managed(dll) and "test" not in dll.name.lower():
                plugin_dlls.setdefault(dll.name, str(dll))
    # editor-only managed libs that packages expect (Cecil, Newtonsoft) in the editor pass
    extra_editor = [str(DATA / "Managed" / n) for n in ("Unity.Cecil.dll", "Unity.Cecil.Rocks.dll", "Newtonsoft.Json.dll", "Unity.CompilationPipeline.Common.dll")]

    def resolve(r):
        r = ALIASES.get(r, r)
        if r.startswith("GUID:"):
            return by_guid.get(r[5:])
        return by_name.get(r)

    def active(a):
        if PLAYER and a.editor_only:
            return False
        inc = a.data.get("includePlatforms") or []
        exc = a.data.get("excludePlatforms") or []
        if inc and not ("Android" in inc or (not PLAYER and "Editor" in inc)):
            return False
        if "Android" in exc:
            return False
        return bool(a.sources)

    def defines_for(a):
        d = list(base_defines)
        for vd in a.data.get("versionDefines", []):
            res = vd.get("name", "")
            ver = installed.get(res)
            if res.startswith("com.unity.modules.") and res in installed:
                ver = "1.0.0"
            if res == "Unity":
                ver = ".".join(map(str, UNITY_VERSION))
            if ver is not None and in_range(ver, vd.get("expression", "")):
                d.append(vd["define"])
        return d

    def constraints_ok(a, defs):
        for c in a.data.get("defineConstraints", []):
            neg = c.startswith("!")
            sym = c.lstrip("!")
            alts = sym.split("||")
            hit = any(s.strip() in defs for s in alts)
            if hit == neg:
                return False
        return True

    OUT.mkdir(parents=True, exist_ok=True)
    built, failed, skipped = {}, {}, []
    csc = csc_path()

    # Assembly-CSharp(-Editor) reference everything that is autoReferenced
    pseudo = []
    for name, srcs in loose.items():
        if srcs:
            a = Asm.__new__(Asm)
            a.path = ROOT / "Assets"; a.name = name; a.dir = ROOT / "Assets"; a.pkg = None; a.guid = None
            a.sources = srcs; a.editor_only = name.endswith("Editor")
            a.data = {"name": name, "allowUnsafeCode": True, "includePlatforms": ["Editor"] if a.editor_only else []}
            pseudo.append(a)

    order = []
    seen = set()

    def visit(a, stack=()):
        if a.name in seen:
            return
        seen.add(a.name)
        for r in a.data.get("references", []):
            dep = resolve(r)
            if dep:
                visit(dep, stack + (a.name,))
        order.append(a)

    for n in IMPLICIT:
        if n in by_name:
            visit(by_name[n])
    for a in asms:
        visit(a)
    for p in pseudo:
        order.append(p)

    for stub in sorted(STUBS.glob("*.cs")) if STUBS.exists() else []:
        out = OUT / f"{stub.stem}.dll"
        r = subprocess.run([DOTNET, csc, "-nologo", "-target:library", f"-out:{out}", "-langversion:9", "-nostdlib",
                            "-nowarn:CS0067,CS0649"] + [f"-r:{x}" for x in netstd + engine_refs + list(built.values())] + [str(stub)],
                           capture_output=True, text=True)
        if r.returncode:
            print("stub failed", stub.name, r.stdout[-2000:]); return 1
        built[stub.stem] = str(out)

    ours_errors = []
    for a in order:
        if not active(a):
            continue
        defs = defines_for(a)
        if not constraints_ok(a, defs):
            skipped.append(a.name); continue
        refs = []
        if a.name.startswith("Assembly-CSharp"):
            deps = [x for x in built if (by_name.get(x) is None or by_name[x].data.get("autoReferenced", True))]
            if a.name == "Assembly-CSharp-Editor" and "Assembly-CSharp" in built:
                deps.append("Assembly-CSharp")
            if a.name == "Assembly-CSharp" :
                deps = [x for x in deps if not (by_name.get(x) and by_name[x].editor_only)]
            refs = [built[x] for x in dict.fromkeys(deps)]
        else:
            for r in a.data.get("references", []):
                dep = resolve(r)
                if dep and dep.name in built:
                    refs.append(built[dep.name])
            refs += [v for k, v in built.items() if (STUBS / f"{k}.cs").exists()]
            for imp in IMPLICIT:
                if imp in built and imp != a.name and not (imp == "UnityEditor.UI" and a.name == "UnityEngine.UI"):
                    refs.append(built[imp])
        pre = []
        if a.data.get("overrideReferences"):
            for n in a.data.get("precompiledReferences", []):
                if n in plugin_dlls:
                    pre.append(plugin_dlls[n])
        else:
            pre = list(plugin_dlls.values())
        if not PLAYER:
            pre += extra_editor
        out = OUT / f"{a.name}.dll"
        args = ["-nologo", "-target:library", f"-out:{out}", "-langversion:9", "-nostdlib", "-noconfig",
                "-nowarn:CS0618,CS0649,CS0414,CS0169,CS0067,CS1701,CS1702,CS0436,CS8019",
                "-unsafe" if a.data.get("allowUnsafeCode") else "", "-deterministic",
                "-define:" + ";".join(defs)]
        args = [x for x in args if x]
        args += [f"-r:{x}" for x in netstd + engine_refs + refs + pre]
        args += [str(s) for s in a.sources]
        rsp = OUT / f"{a.name}.rsp"
        rsp.write_text("\n".join(f'"{x}"' if " " in x else x for x in args))
        res = subprocess.run([DOTNET, csc, f"@{rsp}"], capture_output=True, text=True)
        errs = [l for l in res.stdout.splitlines() if ": error " in l]
        if res.returncode == 0 and out.exists():
            built[a.name] = str(out)
        else:
            failed[a.name] = errs
            if a.pkg is None:
                ours_errors += errs
    mode = "player (Android)" if PLAYER else "editor"
    print(f"[{mode}] built {len(built)} assemblies, {len(failed)} failed, {len(skipped)} skipped by constraints")
    for n, errs in failed.items():
        tag = "OURS" if (by_name.get(n) is None or by_name[n].pkg is None) else "pkg"
        print(f"  FAILED [{tag}] {n}: {len(errs)} errors")
        for e in (errs if tag == "OURS" else errs[:3]):
            print("     ", e.replace(str(ROOT) + "/", "").replace(str(PKGS) + "/", "").replace(str(BUILTIN) + "/", ""))
    ours_failed = [n for n in failed if by_name.get(n) is None or by_name[n].pkg is None]
    print("OUR CODE:", "FAILED" if ours_failed else "OK (compiles cleanly)")
    return 1 if ours_failed else 0


if __name__ == "__main__":
    sys.exit(main())
