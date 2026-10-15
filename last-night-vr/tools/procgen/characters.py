"""Low-poly stylised humanoids in A-pose, plus The Pull.

Every character is separate body-part meshes in one model (head, torso, arms, legs...) built
in a neutral A-pose with joints at standard heights, so Mixamo / Blender auto-rig works.
Clothing is colour regions on the same geometry (cheap on Quest 3).
"""
import numpy as np

from meshlib import Mesh, Model, box, capsule, cyl, ellipsoid, lathe, sphere, torus


def _orient_y_to(direction):
    """4x4 rotation taking +Y to the given direction."""
    d = np.asarray(direction, float)
    d = d / np.linalg.norm(d)
    y = np.array([0, 1.0, 0])
    v = np.cross(y, d)
    c = float(np.dot(y, d))
    if np.linalg.norm(v) < 1e-8:
        r = np.eye(3) if c > 0 else np.diag([1, -1, -1.0])
    else:
        vx = np.array([[0, -v[2], v[1]], [v[2], 0, -v[0]], [-v[1], v[0], 0]])
        r = np.eye(3) + vx + vx @ vx * (1 / (1 + c))
    m = np.eye(4); m[:3, :3] = r
    return m


def limb(a, b, r0, r1=None, seg=10):
    """Tapered capsule-ish limb from point a to point b."""
    a, b = np.asarray(a, float), np.asarray(b, float)
    r1 = r0 if r1 is None else r1
    length = np.linalg.norm(b - a)
    m = lathe([(0.0, -r0 * 0.8), (r0, 0.0), (r0 * 0.95 + r1 * 0.05, length * 0.1),
               (r1, length * 0.9), (r1 * 0.7, length + r1 * 0.5), (0.0, length + r1 * 0.7)], seg, False, False)
    m._apply(_orient_y_to(b - a))
    return m.move(*a)


def hair_curls(cx, cy, cz, r, n, rng):
    out = []
    for _ in range(n):
        a, b = rng.uniform(0, 2 * np.pi), rng.uniform(0.1, 1.3)
        if np.sin(b) * np.sin(a) > 0.3 and np.cos(b) < 0.8:  # keep the face clear
            a = -a
        p = (cx + r * np.sin(b) * np.cos(a), cy + r * np.cos(b) * 0.9, cz + r * np.sin(b) * np.sin(a) * 0.95)
        out.append(sphere(r * 0.38, 8, 5).move(*p))
    return out


def humanoid(name, height, skin, top, bottom, *, sleeve="short", shoes=None, hair="hair_dark", hairstyle="short",
             coat=None, gloves=None, scarf=None, mustache=False, head_scale=1.0, build=1.0, seed=1):
    rng = np.random.default_rng(seed)
    m = Model(name)
    s = height / 1.75  # base proportions are for 1.75 m
    P = []  # (mesh, colour, emissive?) built at 1.75 m then scaled

    def add(mesh, c, e=False):
        P.append((mesh, c, e))

    w = build
    # --- legs (hip -> knee -> ankle -> toe)
    for sx in (-1, 1):
        hip, knee, ankle = (sx * 0.09 * w, 0.92, 0.0), (sx * 0.095 * w, 0.5, 0.01), (sx * 0.095 * w, 0.08, 0.0)
        add(limb(hip, knee, 0.082 * w, 0.062 * w), bottom)
        add(limb(knee, ankle, 0.06 * w, 0.045 * w), bottom)
        add(sphere(0.07 * w, 10, 6).move(*hip), bottom)
        add(sphere(0.06 * w, 10, 6).move(*knee), bottom)
        foot_col = shoes if shoes else skin
        add(box(0.09, 0.07, 0.24).move(sx * 0.095 * w, 0.035, 0.05), foot_col)
        add(box(0.085, 0.04, 0.09).move(sx * 0.095 * w, 0.02, 0.16), foot_col)
    # --- torso
    add(ellipsoid(0.17 * w, 0.14, 0.1 * w, 14, 8).move(0, 0.97, 0), bottom)  # hips
    add(ellipsoid(0.15 * w, 0.17, 0.095 * w, 14, 8).move(0, 1.12, 0), top)  # waist
    add(ellipsoid(0.19 * w, 0.2, 0.11 * w, 14, 8).move(0, 1.3, 0), top)  # chest
    add(cyl(0.045, 0.05, 0.12, 8).move(0, 1.52, 0), skin)  # neck
    add(ellipsoid(0.205 * w, 0.045, 0.1 * w, 12, 6).move(0, 1.43, 0), top)  # shoulders
    # --- arms (A-pose, 35 degrees from vertical)
    ang = np.radians(35)
    d = np.array([np.sin(ang), -np.cos(ang), 0.02])
    for sx in (-1, 1):
        sh = np.array([sx * 0.2 * w, 1.43, 0.0])
        el = sh + np.array([sx * d[0], d[1], d[2]]) * 0.29
        wr = el + np.array([sx * d[0], d[1], d[2]]) * 0.27
        long_sleeve = sleeve == "long"
        add(limb(sh, el, 0.052, 0.042), top if sleeve in ("short", "long", "rolled") else skin)
        add(limb(el, wr, 0.042, 0.032), top if long_sleeve else skin)
        add(sphere(0.052, 8, 6).move(*sh), top)
        hand = wr + np.array([sx * d[0], d[1], d[2]]) * 0.04
        add(ellipsoid(0.04, 0.055, 0.022, 8, 6).rot(0, 0, sx * -35).move(*hand), gloves if gloves else skin)
        if sleeve == "rolled":
            add(torus(0.043, 0.012, 10, 5).rot(0, 0, sx * -55).move(*(el + (wr - el) * 0.02)), top)
    # --- head
    hs = head_scale
    hc = (0, 1.66, 0.01)
    add(ellipsoid(0.095 * hs, 0.118 * hs, 0.105 * hs, 16, 10).move(*hc), skin)
    add(box(0.02, 0.03, 0.02).move(0, hc[1] - 0.005 * hs, hc[2] + 0.108 * hs), skin)  # nose
    for sx in (-1, 1):
        add(sphere(0.012 * hs, 6, 4).move(sx * 0.035 * hs, hc[1] + 0.015 * hs, hc[2] + 0.098 * hs), "black")  # eyes
        add(ellipsoid(0.012, 0.025, 0.014, 6, 4).move(sx * 0.098 * hs, hc[1], hc[2]), skin)  # ears
    if mustache:
        add(box(0.07 * hs, 0.014, 0.012).move(0, hc[1] - 0.04 * hs, hc[2] + 0.106 * hs), "hair_dark")
    # --- hair
    if hairstyle in ("short",):
        add(ellipsoid(0.1 * hs, 0.085 * hs, 0.11 * hs, 14, 8).move(hc[0], hc[1] + 0.055 * hs, hc[2] - 0.01), hair)
    elif hairstyle == "messy_long":
        add(ellipsoid(0.105 * hs, 0.09 * hs, 0.115 * hs, 14, 8).move(hc[0], hc[1] + 0.05 * hs, hc[2] - 0.01), hair)
        add(box(0.19, 0.2, 0.06).rot(-6, 0, 0).move(0, hc[1] - 0.08, hc[2] - 0.085), hair)  # back, to shoulder length
        for sx in (-1, 1):
            add(box(0.03, 0.17, 0.07).rot(0, 0, sx * 5).move(sx * 0.1, hc[1] - 0.05, hc[2] - 0.02), hair)
        for _ in range(5):  # flyaway strands
            add(box(0.012, 0.07, 0.012).rot(*rng.uniform(-40, 40, 3)).move(rng.uniform(-0.09, 0.09), hc[1] + 0.13, rng.uniform(-0.05, 0.05)), hair)
    elif hairstyle == "ponytail":
        add(ellipsoid(0.1 * hs, 0.085 * hs, 0.11 * hs, 14, 8).move(hc[0], hc[1] + 0.055 * hs, hc[2] - 0.01), hair)
        add(limb((0, hc[1] + 0.02, hc[2] - 0.11), (0, hc[1] - 0.17, hc[2] - 0.2), 0.035, 0.015), hair)
    elif hairstyle == "bob":
        add(ellipsoid(0.108 * hs, 0.1 * hs, 0.118 * hs, 14, 8).move(hc[0], hc[1] + 0.03 * hs, hc[2] - 0.015), hair)
        add(box(0.2, 0.12, 0.1).move(0, hc[1] - 0.06, hc[2] - 0.05), hair)
    elif hairstyle == "curly":
        for c in hair_curls(hc[0], hc[1] + 0.02, hc[2] - 0.01, 0.12 * hs, 38, rng):
            add(c, hair)
    # --- outfit extras
    if coat:
        add(lathe([(0.2 * w, 1.42), (0.215 * w, 1.1), (0.25 * w, 0.62), (0.265 * w, 0.52)], 16, False, False), coat)
        add(box(0.01, 0.9, 0.2).move(0, 0.95, 0.2), coat)
        add(ellipsoid(0.2 * w, 0.05, 0.12 * w, 12, 6).move(0, 1.46, 0), coat)  # collar
    if scarf:
        add(torus(0.075, 0.03, 14, 6).move(0, 1.5, 0), scarf)
        add(box(0.09, 0.4, 0.03).rot(8, 0, 0).move(0.04, 1.3, 0.13), scarf)  # hanging end
        add(box(0.09, 0.3, 0.03).rot(-5, 0, 0).move(-0.04, 1.35, 0.14), scarf)
    for mesh, c, e in P:
        mesh.scale(s)
        m.add(mesh, c, e)
    return m


def maren_tonight():
    return humanoid("Maren_tonight", 1.68, "skin2", "gray", "darksteel", sleeve="short", hair="hair_dark",
                    hairstyle="messy_long", build=0.95, seed=3)


def maren_age_22():
    m = humanoid("Maren_age_22", 1.68, "skin2", "honey", "blue", sleeve="long", shoes="darkwood", hair="hair_dark",
                 hairstyle="bob", coat=(0.5, 0.3, 0.16), scarf="red", build=0.93, seed=4)
    # carrying a stack of books against the chest
    s = 1.68 / 1.75
    for i, c in enumerate(("green", "blue", "darkwood")):
        m.add(box(0.2, 0.04, 0.27).rot(0, 12 * i, 0).move(0.06, (1.12 + i * 0.045) * s, 0.2 * s), c)
    return m


def wren():
    return humanoid("Wren", 1.2, "skin2", "yellow", "yellow", sleeve="long", hair="hair_dark",
                    hairstyle="curly", head_scale=1.3, build=0.8, seed=5)


def elio():
    return humanoid("Elio", 1.78, "skin4", (0.5, 0.52, 0.5), "navy", sleeve="rolled", shoes="darkwood",
                    hair="hair_dark", hairstyle="short", mustache=True, build=1.05, seed=6)


def okafor():
    return humanoid("Okafor", 1.85, "skin3", "navy", "navy", sleeve="long", shoes="black", hair="hair_dark",
                    hairstyle="short", gloves="blue", build=1.08, seed=7)


def lind():
    return humanoid("Lind", 1.68, "skin1", "navy", "navy", sleeve="long", shoes="black", hair="hair_blond",
                    hairstyle="ponytail", gloves="blue", build=0.95, seed=8)


def tomas():
    return humanoid("Tomas", 1.80, "skin4", "ltgray", "darksteel", sleeve="short", hair="hair_gray",
                    hairstyle="short", build=1.0, seed=9)


def the_pull():
    """Maren's silhouette stretched: 2.6 m, narrow, no features, white emissive (SG_PullBody drives the glow)."""
    base = humanoid("The_Pull", 1.68, "pullwhite", "pullwhite", "pullwhite", sleeve="short",
                    shoes=None, hair="pullwhite", hairstyle="messy_long", build=0.95, seed=3)
    out = Model("The_Pull")
    for mesh, _, _ in base.parts:
        out.add(mesh.scale(0.82, 2.6 / 1.68, 0.82), "pullwhite", emissive=True)
    # long arms: re-stretch is already covered by y-scale; add a slight forward stoop
    for mesh, _, _ in out.parts:
        mesh.rot(6, 0, 0)
    return out


CHARACTERS = [("Characters", maren_tonight), ("Characters", maren_age_22), ("Characters", wren),
              ("Characters", elio), ("Characters", okafor), ("Characters", lind),
              ("Characters", tomas), ("Characters", the_pull)]
