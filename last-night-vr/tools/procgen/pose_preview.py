"""Render the PuppetAnimator moves on a character, to check pose angles without Unity.

    python tools/procgen/pose_preview.py OUT.png [Character]

A straight port of PuppetAnimator.Evaluate (keep them in step). The mesh is mirrored in X first, the way
Unity imports OBJ files, and rotations use Unity's Euler order (Z, then X, then Y).
"""
import math
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, str(Path(__file__).resolve().parent))
import characters  # noqa: E402
from meshlib import Mesh, Model  # noqa: E402
from preview import render  # noqa: E402

J = ["hips", "spine", "chest", "neck", "head", "upperarm_L", "forearm_L", "hand_L", "upperarm_R", "forearm_R", "hand_R",
     "thigh_L", "shin_L", "foot_L", "thigh_R", "shin_R", "foot_R"]


def rot(e):
    x, y, z = np.radians(e)
    rx = np.array([[1, 0, 0], [0, math.cos(x), -math.sin(x)], [0, math.sin(x), math.cos(x)]])
    ry = np.array([[math.cos(y), 0, math.sin(y)], [0, 1, 0], [-math.sin(y), 0, math.cos(y)]])
    rz = np.array([[math.cos(z), -math.sin(z), 0], [math.sin(z), math.cos(z), 0], [0, 0, 1]])
    return ry @ rx @ rz


def evaluate(move, rig, side, t=0.3):
    e = {j: np.zeros(3) for j in J}
    hipsY = rig["hips"][1][1]
    thigh = np.linalg.norm(np.subtract(rig["shin_L"][1], rig["thigh_L"][1]))

    def arms(fwd, out, elbow, fwdR=None, outR=None, elbowR=None):
        e["upperarm_L"] = np.array([fwd, 0, -side["L"] * (35 - out)])
        e["forearm_L"] = np.array([0, 0, 0]); e["elbow_L"] = elbow
        e["upperarm_R"] = np.array([fwd if fwdR is None else fwdR, 0, -side["R"] * (35 - (out if outR is None else outR))])
        e["forearm_R"] = np.array([0, 0, 0]); e["elbow_R"] = elbow if elbowR is None else elbowR

    def kneel():
        e["shin_L"][0] = e["shin_R"][0] = 90
        e["foot_L"][0] = e["foot_R"][0] = 10
        return hipsY - (thigh + 0.07)

    def chair():
        e["thigh_L"][0] = e["thigh_R"][0] = -88
        e["shin_L"][0] = e["shin_R"][0] = 88
        return max(0, hipsY - 0.48)

    drop = 0
    arms(0, 6, -6)
    if move == "Kneel":
        drop = kneel(); e["spine"][0] = 6; arms(-10, 4, -20)
    elif move == "Cpr":
        drop = kneel(); e["spine"][0] = 33; e["chest"][0] = 13; e["head"][0] = 12; arms(-62, -6, 0); drop += 0.05
    elif move == "SitTalk":
        drop = chair(); e["spine"][0] = 12; arms(-42, -4, -55)
    elif move == "SitFloorDraw":
        e["thigh_L"] = np.array([-80, side["L"] * 32, 0]); e["thigh_R"] = np.array([-80, side["R"] * 32, 0])
        e["shin_L"][0] = e["shin_R"][0] = 150; drop = max(0, hipsY - 0.12)
        e["spine"][0] = 24; e["head"][0] = 18; arms(-28, 0, -20, -48, -2, -35)
    elif move == "Wave":
        e["upperarm_R"] = np.array([-10, 0, side["R"] * 105]); e["forearm_R"] = np.array([0, 0, 20])
    elif move == "ArmsUp":
        arms(-150, 12, -10)
    elif move == "HoldUp":
        arms(-78, -8, -28); e["head"][0] = -6
    elif move == "HandsOverMouth":
        arms(-32, -14, -128); e["head"][0] = 14; e["spine"][0] = 8; drop = kneel()
    elif move == "Laugh":
        arms(0, 6, -6, -30, -16, -105); e["head"][0] = -12
    elif move == "Lean":
        e["spine"][0] = 14; e["hips"][1] = 10; arms(0, 6, -6, -55, 4, -12)
    elif move == "ReachDown":
        e["spine"][0] = 34; e["chest"][0] = 10; e["thigh_L"][0] = e["thigh_R"][0] = -18
        e["shin_L"][0] = e["shin_R"][0] = 22; arms(-72, -4, -8); drop = 0.06
    elif move == "Held":
        arms(-62, -16, -42); e["thigh_L"][0] = e["thigh_R"][0] = -42; e["shin_L"][0] = e["shin_R"][0] = 50
    elif move == "Carry":
        arms(-32, -6, -62)
    elif move == "Walk":
        ph = 0.25 * 2 * math.pi
        s, c = math.sin(ph), math.sin(ph + math.pi)
        e["thigh_L"][0] = -24 * s; e["thigh_R"][0] = -24 * c
        e["shin_L"][0] = 12 + 26 * max(0, -s); e["shin_R"][0] = 12 + 26 * max(0, -c)
        e["upperarm_L"][0] = 16 * s; e["upperarm_R"][0] = 16 * c
    return e, drop


def posed(name, move):
    m = getattr(characters, name.lower())()
    rig = {k: (p, np.array(pos, float) * [-1, 1, 1]) for k, (p, pos) in m.rig.items()}  # Unity mirrors X
    side = {"L": np.sign(rig["upperarm_L"][1][0]), "R": np.sign(rig["upperarm_R"][1][0])}
    e, drop = evaluate(move, rig, side)

    world = {}  # joint -> (R, origin) in model space

    def solve(j):
        if j in world:
            return world[j]
        parent, pos = rig[j]
        if parent is None:
            world[j] = (np.eye(3), np.zeros(3)); return world[j]
        pr, po = solve(parent)
        local = rot(e.get(j, np.zeros(3)))
        if j.startswith("forearm_"):
            k = j[-1]
            sd = side[k]
            local = rot([0, 0, sd * 35]) @ rot([e.get("elbow_" + k, 0), 0, 0]) @ rot([0, 0, -sd * 35]) @ local
        offset = pos - rig[parent][1]
        if j == "hips":
            offset = offset - np.array([0, drop, 0])
        world[j] = (pr @ local, po + pr @ offset)
        return world[j]

    out = Model(f"{name} {move}")
    for (mesh, c, em), j in zip(m.parts, m.joint_tags):
        v = mesh.v.astype(float) * [-1, 1, 1]
        n = mesh.n.astype(float) * [-1, 1, 1]
        R, o = solve(j)
        bind = rig[j][1]
        nv = (v - bind) @ R.T + o
        out.add(Mesh(nv, n @ R.T, mesh.f[:, ::-1].copy()), c, em)
    return out


def main():
    dst = sys.argv[1]
    name = sys.argv[2] if len(sys.argv) > 2 else "Maren_tonight"
    moves = ["Idle", "Walk", "Kneel", "Cpr", "SitTalk", "SitFloorDraw", "Wave", "ArmsUp", "HoldUp",
             "HandsOverMouth", "Laugh", "Lean", "ReachDown", "Held", "Carry"]
    S = 260
    sheet = Image.new("RGB", (5 * S, 3 * S), (18, 18, 20))
    for i, mv in enumerate(moves):
        im = render(posed(name, mv), S, yaw=25, pitch=10)
        ImageDraw.Draw(im).text((6, 4), mv, fill=(220, 220, 220))
        sheet.paste(im, ((i % 5) * S, (i // 5) * S))
    sheet.save(dst)
    print("wrote", dst)


if __name__ == "__main__":
    main()
