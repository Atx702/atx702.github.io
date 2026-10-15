"""Tiny procedural low-poly modeller (numpy only) that writes GLB + OBJ/MTL.

Units are metres, +Y up, the model's front faces +Z, origin on the floor at the centre.
Each part has a flat colour (and optionally emissive), so a model is a handful of
materials: cheap on Quest 3. Unity imports OBJ/MTL natively; GLB works with glTFast.

    m = Model("Kettle")
    m.add(cyl(0.1, 0.08, 0.15, 16).move(0, 0.075, 0), "steel")
    m.save(folder)
"""
import json
import struct
from pathlib import Path

import numpy as np

# Project palette: about 30% saturation, per docs/07 colour rules.
PALETTE = {
    "wood": (0.42, 0.33, 0.25), "darkwood": (0.27, 0.2, 0.15), "pine": (0.55, 0.45, 0.33),
    "white": (0.82, 0.82, 0.8), "cream": (0.8, 0.77, 0.66), "paper": (0.9, 0.88, 0.8),
    "steel": (0.55, 0.57, 0.58), "darksteel": (0.28, 0.3, 0.32), "black": (0.07, 0.07, 0.08),
    "gray": (0.45, 0.46, 0.46), "ltgray": (0.65, 0.66, 0.66),
    "red": (0.62, 0.16, 0.14), "redbag": (0.55, 0.12, 0.1), "yellow": (0.85, 0.7, 0.28),
    "honey": (0.78, 0.5, 0.12), "gold": (0.8, 0.62, 0.2), "teal": (0.36, 0.55, 0.55),
    "ltteal": (0.6, 0.72, 0.72), "blue": (0.35, 0.48, 0.62), "navy": (0.14, 0.17, 0.27),
    "glass": (0.6, 0.72, 0.78), "green": (0.3, 0.42, 0.34), "skin1": (0.55, 0.4, 0.32),
    "skin2": (0.6, 0.45, 0.36), "skin3": (0.38, 0.26, 0.2), "skin4": (0.7, 0.55, 0.46),
    "hair_dark": (0.1, 0.07, 0.06), "hair_blond": (0.7, 0.58, 0.35), "hair_gray": (0.5, 0.5, 0.5),
    "mattress": (0.78, 0.76, 0.7), "sheet": (0.85, 0.85, 0.83), "star": (0.9, 0.78, 0.35),
    "pullwhite": (1.0, 1.0, 1.0),
}


def _col(c):
    return PALETTE[c] if isinstance(c, str) else tuple(c)


class Mesh:
    def __init__(self, v, n, f):
        self.v = np.asarray(v, np.float32)
        self.n = np.asarray(n, np.float32)
        self.f = np.asarray(f, np.uint32)

    def _apply(self, m):
        r = m[:3, :3]
        self.v = (self.v @ r.T + m[:3, 3]).astype(np.float32)
        nm = np.linalg.inv(r).T
        n = self.n @ nm.T
        ln = np.linalg.norm(n, axis=1, keepdims=True)
        self.n = (n / np.where(ln == 0, 1, ln)).astype(np.float32)
        return self

    def move(self, x=0, y=0, z=0):
        m = np.eye(4); m[:3, 3] = (x, y, z)
        return self._apply(m)

    def scale(self, x=1, y=None, z=None):
        y = x if y is None else y
        z = x if z is None else z
        return self._apply(np.diag([x, y, z, 1.0]))

    def rot(self, x=0, y=0, z=0):
        """Degrees, applied X then Y then Z."""
        a, b, c = np.radians([x, y, z])
        rx = np.array([[1, 0, 0], [0, np.cos(a), -np.sin(a)], [0, np.sin(a), np.cos(a)]])
        ry = np.array([[np.cos(b), 0, np.sin(b)], [0, 1, 0], [-np.sin(b), 0, np.cos(b)]])
        rz = np.array([[np.cos(c), -np.sin(c), 0], [np.sin(c), np.cos(c), 0], [0, 0, 1]])
        m = np.eye(4); m[:3, :3] = rz @ ry @ rx
        return self._apply(m)


def _quad(v, i, j, k, l):
    return [(i, j, k), (i, k, l)]


def box(w, h, d):
    """Axis-aligned box centred on the origin, flat shaded."""
    hx, hy, hz = w / 2, h / 2, d / 2
    faces = [  # normal, 4 corners CCW seen from outside
        ((0, 0, 1), [(-hx, -hy, hz), (hx, -hy, hz), (hx, hy, hz), (-hx, hy, hz)]),
        ((0, 0, -1), [(hx, -hy, -hz), (-hx, -hy, -hz), (-hx, hy, -hz), (hx, hy, -hz)]),
        ((1, 0, 0), [(hx, -hy, hz), (hx, -hy, -hz), (hx, hy, -hz), (hx, hy, hz)]),
        ((-1, 0, 0), [(-hx, -hy, -hz), (-hx, -hy, hz), (-hx, hy, hz), (-hx, hy, -hz)]),
        ((0, 1, 0), [(-hx, hy, hz), (hx, hy, hz), (hx, hy, -hz), (-hx, hy, -hz)]),
        ((0, -1, 0), [(-hx, -hy, -hz), (hx, -hy, -hz), (hx, -hy, hz), (-hx, -hy, hz)]),
    ]
    v, n, f = [], [], []
    for nrm, c in faces:
        b = len(v)
        v += c; n += [nrm] * 4
        f += [(b, b + 1, b + 2), (b, b + 2, b + 3)]
    return Mesh(v, n, f)


def lathe(profile, seg=16, closed_bottom=True, closed_top=True):
    """Surface of revolution about Y. profile = [(radius, y), ...] bottom to top. Smooth shaded."""
    p = np.array(profile, np.float32)
    rings = len(p)
    v, n, f = [], [], []
    # profile tangent -> outward normal in the (r, y) plane
    tang = np.gradient(p, axis=0)
    nrm2 = np.stack([tang[:, 1], -tang[:, 0]], 1)
    ln = np.linalg.norm(nrm2, axis=1, keepdims=True)
    nrm2 = nrm2 / np.where(ln == 0, 1, ln)
    for s in range(seg + 1):
        a = 2 * np.pi * s / seg
        ca, sa = np.cos(a), np.sin(a)
        for r in range(rings):
            v.append((p[r, 0] * ca, p[r, 1], p[r, 0] * sa))
            n.append((nrm2[r, 0] * ca, nrm2[r, 1], nrm2[r, 0] * sa))
    for s in range(seg):
        for r in range(rings - 1):
            a = s * rings + r
            b = (s + 1) * rings + r
            f += [(a, a + 1, b + 1), (a, b + 1, b)]
    m = Mesh(v, n, f)
    # flip winding if the profile runs the other way (so faces point outward)
    if _signed_volume(m) < 0:
        m.f = m.f[:, ::-1].copy()
    out = [m]
    if closed_bottom and p[0, 0] > 1e-5:
        out.append(_disc(p[0, 0], p[0, 1], seg, -1))
    if closed_top and p[-1, 0] > 1e-5:
        out.append(_disc(p[-1, 0], p[-1, 1], seg, 1))
    return merge(out)


def _signed_volume(m):
    a, b, c = (m.v[m.f[:, i]] for i in range(3))
    return float(np.einsum("ij,ij->i", a, np.cross(b, c)).sum() / 6)


def _disc(r, y, seg, up):
    v = [(0, y, 0)] + [(r * np.cos(2 * np.pi * i / seg), y, r * np.sin(2 * np.pi * i / seg)) for i in range(seg)]
    f = [(0, 1 + (i + 1) % seg, 1 + i) if up > 0 else (0, 1 + i, 1 + (i + 1) % seg) for i in range(seg)]
    # make the winding match the facing direction
    return Mesh(v, [(0, up, 0)] * len(v), f)


def cyl(r_bottom, r_top, h, seg=16):
    """Cylinder/cone along Y, centred on origin."""
    return lathe([(r_bottom, -h / 2), (r_top, h / 2)], seg)


def sphere(r, seg=16, rings=10):
    prof = [(r * np.sin(np.pi * i / rings), -r * np.cos(np.pi * i / rings)) for i in range(rings + 1)]
    prof[0] = (0.0, prof[0][1]); prof[-1] = (0.0, prof[-1][1])
    return lathe(prof, seg, False, False)


def ellipsoid(rx, ry, rz, seg=16, rings=10):
    return sphere(1.0, seg, rings).scale(rx, ry, rz)


def capsule(r, h, seg=12):
    """Vertical capsule of total height h (>= 2r)."""
    half = max(h / 2 - r, 0)
    steps = 4
    prof = [(r * np.sin(np.pi / 2 * i / steps), -half - r * np.cos(np.pi / 2 * i / steps)) for i in range(steps + 1)]
    prof += [(r * np.cos(np.pi / 2 * i / steps), half + r * np.sin(np.pi / 2 * i / steps)) for i in range(steps + 1)]
    prof[0] = (0.0, prof[0][1]); prof[-1] = (0.0, prof[-1][1])
    return lathe(prof, seg, False, False)


def torus(R, r, seg=20, sides=8):
    v, n, f = [], [], []
    for i in range(seg + 1):
        a = 2 * np.pi * i / seg
        for j in range(sides + 1):
            b = 2 * np.pi * j / sides
            cx, cy = np.cos(a), np.sin(a)
            v.append(((R + r * np.cos(b)) * cx, r * np.sin(b), (R + r * np.cos(b)) * cy))
            n.append((np.cos(b) * cx, np.sin(b), np.cos(b) * cy))
    for i in range(seg):
        for j in range(sides):
            a = i * (sides + 1) + j
            b = (i + 1) * (sides + 1) + j
            f += [(a, b, b + 1), (a, b + 1, a + 1)]
    m = Mesh(v, n, f)
    if _signed_volume(m) < 0:
        m.f = m.f[:, ::-1].copy()
    return m


def wedge(w, h, d):
    """Triangular prism (roof): ridge along X, base w x d, apex height h."""
    hx, hz = w / 2, d / 2
    tris = [  # (normal, verts)
        ((0, 0, 1), [(-hx, 0, hz), (hx, 0, hz), (hx, 0, hz), (0, h, 0)]),
    ]
    v, n, f = [], [], []

    def quad(nrm, pts):
        b = len(v); v.extend(pts); n.extend([nrm] * len(pts))
        f.extend([(b, b + 1, b + 2)] + ([(b, b + 2, b + 3)] if len(pts) == 4 else []))
    s = np.hypot(h, hz)
    quad((0, hz / s, h / s) if False else (0, hz / s, h / s), [(-hx, 0, hz), (hx, 0, hz), (hx, h, 0), (-hx, h, 0)])
    quad((0, hz / s, -h / s), [(hx, 0, -hz), (-hx, 0, -hz), (-hx, h, 0), (hx, h, 0)])
    quad((0, -1, 0), [(-hx, 0, -hz), (hx, 0, -hz), (hx, 0, hz), (-hx, 0, hz)])
    quad((1, 0, 0), [(hx, 0, hz), (hx, 0, -hz), (hx, h, 0)])
    quad((-1, 0, 0), [(-hx, 0, -hz), (-hx, 0, hz), (-hx, h, 0)])
    return Mesh(v, n, f)


def merge(meshes):
    v = np.concatenate([m.v for m in meshes])
    n = np.concatenate([m.n for m in meshes])
    off, faces = 0, []
    for m in meshes:
        faces.append(m.f + off); off += len(m.v)
    return Mesh(v, n, np.concatenate(faces))


class Model:
    def __init__(self, name):
        self.name = name
        self.parts = []  # (mesh, colour tuple, emissive)

    def add(self, mesh, colour, emissive=False):
        self.parts.append((mesh, _col(colour), emissive))
        return self

    def add_model(self, other, x=0, y=0, z=0, ry=0):
        for mesh, c, e in other.parts:
            m = Mesh(mesh.v.copy(), mesh.n.copy(), mesh.f.copy()).rot(0, ry, 0).move(x, y, z)
            self.parts.append((m, c, e))
        return self

    def _groups(self):
        groups = {}
        for mesh, c, e in self.parts:
            groups.setdefault((c, e), []).append(mesh)
        return [(k, merge(v)) for k, v in groups.items()]

    def tri_count(self):
        return sum(len(m.f) for m, _, _ in self.parts)

    def bounds(self):
        allv = np.concatenate([m.v for m, _, _ in self.parts])
        return allv.min(0), allv.max(0)

    def save(self, folder):
        folder = Path(folder); folder.mkdir(parents=True, exist_ok=True)
        self._save_glb(folder / f"{self.name}.glb")
        self._save_obj(folder / f"{self.name}.obj", folder / f"{self.name}.mtl")

    def _save_obj(self, obj, mtl):
        lines = [f"mtllib {mtl.name}", f"o {self.name}"]
        mats, base = [], 1
        for i, ((c, e), m) in enumerate(self._groups()):
            mats.append((f"m{i}", c, e))
            lines.append(f"usemtl m{i}")
            for v in m.v: lines.append("v %.5f %.5f %.5f" % tuple(v))
            for n in m.n: lines.append("vn %.4f %.4f %.4f" % tuple(n))
            for a, b, c2 in m.f + base:
                lines.append(f"f {a}//{a} {b}//{b} {c2}//{c2}")
            base += len(m.v)
        obj.write_text("\n".join(lines) + "\n")
        out = []
        for name, c, e in mats:
            out += [f"newmtl {name}", "Kd %.4f %.4f %.4f" % c, "Ks 0 0 0", "Ns 10"]
            if e: out.append("Ke %.4f %.4f %.4f" % c)
            out.append("")
        mtl.write_text("\n".join(out))

    def _save_glb(self, path):
        buf = bytearray(); views, accessors, prims, materials = [], [], [], []

        def push(data, target):
            while len(buf) % 4: buf.append(0)
            views.append({"buffer": 0, "byteOffset": len(buf), "byteLength": len(data), "target": target})
            buf.extend(data)
            return len(views) - 1

        for gi, ((c, e), m) in enumerate(self._groups()):
            pv = push(m.v.astype("<f4").tobytes(), 34962)
            nv = push(m.n.astype("<f4").tobytes(), 34962)
            iv = push(m.f.astype("<u4").ravel().tobytes(), 34963)
            a = len(accessors)
            accessors += [
                {"bufferView": pv, "componentType": 5126, "count": len(m.v), "type": "VEC3",
                 "min": m.v.min(0).tolist(), "max": m.v.max(0).tolist()},
                {"bufferView": nv, "componentType": 5126, "count": len(m.n), "type": "VEC3"},
                {"bufferView": iv, "componentType": 5125, "count": int(m.f.size), "type": "SCALAR"},
            ]
            mat = {"name": f"m{gi}", "pbrMetallicRoughness": {
                "baseColorFactor": [*c, 1.0], "metallicFactor": 0.0, "roughnessFactor": 0.85}, "doubleSided": True}
            if e: mat["emissiveFactor"] = list(c)
            materials.append(mat)
            prims.append({"attributes": {"POSITION": a, "NORMAL": a + 1}, "indices": a + 2, "material": gi})
        gltf = {"asset": {"version": "2.0", "generator": "last-night procgen"},
                "scene": 0, "scenes": [{"nodes": [0]}], "nodes": [{"mesh": 0, "name": self.name}],
                "meshes": [{"name": self.name, "primitives": prims}], "materials": materials,
                "buffers": [{"byteLength": len(buf)}], "bufferViews": views, "accessors": accessors}
        js = json.dumps(gltf, separators=(",", ":")).encode()
        js += b" " * (-len(js) % 4)
        while len(buf) % 4: buf.append(0)
        total = 12 + 8 + len(js) + 8 + len(buf)
        path.write_bytes(struct.pack("<III", 0x46546C67, 2, total)
                         + struct.pack("<II", len(js), 0x4E4F534A) + js
                         + struct.pack("<II", len(buf), 0x004E4942) + bytes(buf))
