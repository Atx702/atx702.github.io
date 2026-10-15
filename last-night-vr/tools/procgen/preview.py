"""Render a contact sheet of the procedural models (flat-shaded, painter's algorithm).

    python tools/procgen/preview.py OUT.png [Name ...]
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, str(Path(__file__).resolve().parent))
from characters import CHARACTERS  # noqa: E402
from props import PROPS  # noqa: E402
from build_models import RENAME  # noqa: E402


def render(model, size=360, yaw=35, pitch=18):
    verts, faces, cols = [], [], []
    off = 0
    for mesh, c, e in model.parts:
        verts.append(mesh.v); faces.append(mesh.f + off); off += len(mesh.v)
        cols += [(c, e)] * len(mesh.f)
    v = np.concatenate(verts).astype(float); f = np.concatenate(faces)
    lo, hi = v.min(0), v.max(0)
    ctr = (lo + hi) / 2
    v = v - ctr
    a, b = np.radians(yaw), np.radians(pitch)
    ry = np.array([[np.cos(a), 0, np.sin(a)], [0, 1, 0], [-np.sin(a), 0, np.cos(a)]])
    rx = np.array([[1, 0, 0], [0, np.cos(b), -np.sin(b)], [0, np.sin(b), np.cos(b)]])
    v = v @ ry.T @ rx.T
    ext = np.abs(v[:, :2]).max() * 2.15
    px = np.stack([v[:, 0] / ext * size + size / 2, size / 2 - v[:, 1] / ext * size], 1)
    tri = v[f]
    n = np.cross(tri[:, 1] - tri[:, 0], tri[:, 2] - tri[:, 0])
    ln = np.linalg.norm(n, axis=1, keepdims=True); n = n / np.where(ln == 0, 1, ln)
    light = np.array([-0.4, 0.7, 0.6]); light /= np.linalg.norm(light)
    shade = np.clip(n @ light, 0, 1) * 0.65 + 0.35
    depth = tri[:, :, 2].mean(1)
    img = Image.new("RGB", (size, size), (26, 28, 32))
    d = ImageDraw.Draw(img)
    for i in np.argsort(depth):
        if n[i, 2] < -0.0:  # camera looks down -Z from +Z: skip back faces
            continue
        (c, e) = cols[i]
        k = 1.0 if e else shade[i]
        col = tuple(int(min(255, x * k * 255 * (1.15 if not e else 1))) for x in c)
        d.polygon([tuple(px[j]) for j in f[i]], fill=col)
    return img


def main():
    out = sys.argv[1]
    names = sys.argv[2:]
    models = []
    for _, fn in PROPS + CHARACTERS:
        m = fn(); m.name = RENAME.get(m.name, m.name)
        if not names or m.name in names:
            models.append(m)
    cols = 5
    rows = (len(models) + cols - 1) // cols
    S = 300
    sheet = Image.new("RGB", (cols * S, rows * S), (18, 18, 20))
    for i, m in enumerate(models):
        im = render(m, S)
        d = ImageDraw.Draw(im); d.text((6, 4), m.name, fill=(220, 220, 220))
        sheet.paste(im, ((i % cols) * S, (i // cols) * S))
    sheet.save(out)
    print("wrote", out, len(models), "models")


if __name__ == "__main__":
    main()
