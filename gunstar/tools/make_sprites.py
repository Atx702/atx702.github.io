"""Generates the placeholder pixel-art sprite sheets in ../assets.
Replace the PNGs with hand-drawn art later; keep the same frame layout:
  heroes  (32x32): 0-1 idle, 2-5 run, 6 jump, 7 slide, 8 hurt
  soldier (32x32): 0-3 walk, 4 hurt
"""
from PIL import Image, ImageDraw
import os

OUT = os.path.join(os.path.dirname(__file__), "..", "assets")
F = 32

def rect(d, ox, x, y, w, h, c):
    d.rectangle([ox + x, y, ox + x + w - 1, y + h - 1], fill=c)

def hero(d, ox, pose, pal, female):
    skin, skin_dk, hair, jacket, jacket_dk, pants, boots, gun = pal
    bob = 1 if pose in ("idle1",) else 0
    if pose == "slide":
        # low sliding pose
        rect(d, ox, 4, 24, 20, 5, pants)
        rect(d, ox, 22, 25, 6, 4, boots)
        rect(d, ox, 6, 17, 12, 8, jacket)
        rect(d, ox, 15, 12, 8, 7, skin)
        rect(d, ox, 15, 10, 8, 3, hair)
        rect(d, ox, 20, 14, 2, 2, (20, 20, 20))
        rect(d, ox, 18, 20, 9, 3, gun)
        return
    # legs
    legs = {"idle0": (0, 0), "idle1": (0, 0), "run0": (-3, 3), "run1": (0, 0),
            "run2": (3, -3), "run3": (0, 0), "jump": (-2, 2), "hurt": (-1, 1)}[pose]
    lift = {"jump": 3}.get(pose, 0)
    rect(d, ox, 12 + legs[0], 21 - lift, 4, 7, pants)
    rect(d, ox, 17 + legs[1], 21 - lift, 4, 7, pants)
    rect(d, ox, 11 + legs[0], 27 - lift, 5, 3, boots)
    rect(d, ox, 17 + legs[1], 27 - lift, 5, 3, boots)
    # torso / jacket
    y0 = 12 + bob - lift
    rect(d, ox, 11, y0, 11, 10, jacket)
    rect(d, ox, 11, y0 + 8, 11, 2, jacket_dk)
    rect(d, ox, 15, y0 + 1, 2, 7, (240, 240, 240))  # shirt stripe
    # head
    hy = 4 + bob - lift
    rect(d, ox, 13, hy, 8, 8, skin)
    rect(d, ox, 13, hy + 6, 8, 2, skin_dk)
    rect(d, ox, 18, hy + 3, 2, 2, (25, 15, 10))      # eye
    if female:
        rect(d, ox, 12, hy - 1, 9, 3, hair)            # hairline
        rect(d, ox, 9, hy - 4, 6, 6, hair)             # afro puff back
        rect(d, ox, 17, hy - 5, 5, 5, hair)            # afro puff top
        rect(d, ox, 12, hy + 1, 2, 4, hair)
        rect(d, ox, 20, hy + 5, 1, 1, (255, 210, 60))  # earring
    else:
        rect(d, ox, 12, hy - 4, 9, 5, hair)            # high-top fade
        rect(d, ox, 12, hy + 1, 2, 3, hair)
        rect(d, ox, 17, hy + 6, 4, 1, hair)            # goatee line
    # arm + gun
    if pose == "hurt":
        rect(d, ox, 8, y0 + 1, 4, 3, skin)
    else:
        rect(d, ox, 19, y0 + 3, 6, 3, jacket_dk)
        rect(d, ox, 24, y0 + 3, 2, 3, skin)
        rect(d, ox, 24, y0 + 1, 7, 3, gun)

def sheet(name, frames, drawfn):
    img = Image.new("RGBA", (F * len(frames), F), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for i, p in enumerate(frames):
        drawfn(d, i * F, p)
    img.save(os.path.join(OUT, name))

HERO_FRAMES = ["idle0", "idle1", "run0", "run1", "run2", "run3", "jump", "slide", "hurt"]
# Jax: red jacket, high-top fade
JAX = ((111, 64, 40), (88, 50, 30), (22, 16, 14), (200, 40, 40), (140, 25, 30),
       (40, 50, 90), (30, 30, 30), (120, 120, 130))
# Zuri: purple jacket, afro puffs
ZURI = ((120, 72, 45), (95, 56, 34), (28, 18, 14), (130, 60, 190), (90, 40, 140),
        (30, 30, 40), (220, 180, 60), (120, 120, 130))
sheet("jax.png", HERO_FRAMES, lambda d, ox, p: hero(d, ox, p, JAX, False))
sheet("zuri.png", HERO_FRAMES, lambda d, ox, p: hero(d, ox, p, ZURI, True))

def soldier(d, ox, p):
    green, dk, visor = (90, 110, 60), (60, 75, 40), (255, 80, 40)
    off = [(-2, 2), (0, 0), (2, -2), (0, 0), (0, 0)][p]
    rect(d, ox, 11 + off[0], 21, 4, 8, dk); rect(d, ox, 17 + off[1], 21, 4, 8, dk)
    rect(d, ox, 10, 11, 12, 11, green)
    rect(d, ox, 11, 3, 10, 9, (70, 70, 80))
    rect(d, ox, 11, 6, 6, 3, visor if p < 4 else (255, 255, 255))
    rect(d, ox, 4, 14, 8, 3, (60, 60, 60))  # rifle points left
sheet("soldier.png", range(5), soldier)

def tiles(d, ox, t):
    if t == 0:   # ground top
        rect(d, ox, 0, 0, 32, 32, (120, 80, 50)); rect(d, ox, 0, 0, 32, 6, (70, 170, 70))
        for x in range(0, 32, 8): rect(d, ox, x + 2, 12 + (x % 16) // 2, 3, 3, (95, 62, 38))
    elif t == 1: # dirt
        rect(d, ox, 0, 0, 32, 32, (120, 80, 50))
        for x in range(0, 32, 8): rect(d, ox, x + 3, (x * 3) % 24 + 4, 3, 3, (95, 62, 38))
    else:        # metal platform
        rect(d, ox, 0, 0, 32, 32, (110, 115, 130)); rect(d, ox, 0, 0, 32, 4, (170, 175, 190))
        rect(d, ox, 0, 28, 32, 4, (70, 72, 85))
        for x in (4, 26): rect(d, ox, x, 14, 2, 2, (60, 60, 70))
sheet("tiles.png", range(3), tiles)

def fx(d, ox, t):
    if t == 0:  rect(d, ox, 10, 13, 12, 6, (255, 240, 120)); rect(d, ox, 13, 14, 6, 4, (255, 255, 255))
    elif t == 1: rect(d, ox, 12, 12, 8, 8, (255, 90, 60)); rect(d, ox, 14, 14, 4, 4, (255, 220, 200))
    else:
        r = 4 + (t - 2) * 4
        d.ellipse([ox + 16 - r, 16 - r, ox + 16 + r, 16 + r], fill=(255, 160 - (t - 2) * 30, 40))
        d.ellipse([ox + 16 - r // 2, 16 - r // 2, ox + 16 + r // 2, 16 + r // 2], fill=(255, 250, 200))
# 0 player shot, 1 enemy shot, 2-5 explosion
sheet("fx.png", range(6), fx)
