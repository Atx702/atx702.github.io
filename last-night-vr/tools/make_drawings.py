"""Wren's 12 crayon drawings (collectibles + the Ch.5 story order), drawn procedurally.

    python tools/make_drawings.py   ->  Unity/Assets/_Project/Art/Drawings/drawing_01.png ... 12

1 house, 2 lantern, 3 boat, 4 bird, 5 door 7 (the Ch.5 corridor order), 6 family, 7 sun and dog,
8 train, 9 three mugs, 10 stars, 11 lake, 12 "DADDY WAKE UP" (the drawing she holds up in Ch.8).
"""
import math
import random
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

OUT = Path(__file__).resolve().parents[1] / "Unity/Assets/_Project/Art/Drawings"
W, H = 512, 384
COL = {"red": (205, 50, 45), "blue": (60, 105, 200), "wrongblue": (110, 165, 215), "yellow": (240, 195, 40),
       "green": (70, 160, 70), "brown": (130, 85, 45), "black": (40, 35, 35), "orange": (235, 130, 40),
       "purple": (130, 80, 170), "pink": (230, 120, 160), "gray": (120, 120, 125)}


def crayon_line(d, pts, color, width=7, rnd=None):
    rnd = rnd or random
    for _ in range(3):  # overlapping wobbly passes give the waxy look
        jit = [(x + rnd.uniform(-2.5, 2.5), y + rnd.uniform(-2.5, 2.5)) for x, y in pts]
        d.line(jit, fill=COL[color] + (170,), width=width, joint="curve")


def poly(d, pts, color, closed=True, **k):
    crayon_line(d, pts + ([pts[0]] if closed else []), color, **k)


def scribble_fill(d, box, color, rnd):
    x0, y0, x1, y1 = box
    y = y0
    while y < y1:
        crayon_line(d, [(x0, y), (x1, y + rnd.uniform(-4, 4))], color, width=5, rnd=rnd)
        y += 9


def circle(d, cx, cy, r, color, fill=False, rnd=None):
    pts = [(cx + r * math.cos(a / 20 * 2 * math.pi), cy + r * math.sin(a / 20 * 2 * math.pi)) for a in range(21)]
    if fill:
        for rr in range(int(r), 0, -7):
            crayon_line(d, [(cx + rr * math.cos(a / 16 * 2 * math.pi), cy + rr * math.sin(a / 16 * 2 * math.pi)) for a in range(17)], color, 6, rnd)
    crayon_line(d, pts, color, 7, rnd)


def stick_person(d, x, y, h, color, rnd, hair=None):
    circle(d, x, y - h * 0.8, h * 0.14, color, rnd=rnd)
    if hair:
        crayon_line(d, [(x - h * 0.14, y - h * 0.86), (x, y - h * 0.98), (x + h * 0.14, y - h * 0.86)], hair, 8, rnd)
    crayon_line(d, [(x, y - h * 0.66), (x, y - h * 0.25)], color, 7, rnd)
    crayon_line(d, [(x - h * 0.22, y - h * 0.5), (x, y - h * 0.58), (x + h * 0.22, y - h * 0.5)], color, 7, rnd)
    crayon_line(d, [(x - h * 0.15, y), (x, y - h * 0.25), (x + h * 0.15, y)], color, 7, rnd)


def letters(d, text, x, y, size, color, rnd):
    """Very simple block capitals made of crayon strokes (a 7-year-old's handwriting)."""
    glyphs = {
        "A": [[(0, 1), (.5, 0), (1, 1)], [(.25, .55), (.75, .55)]], "D": [[(0, 0), (0, 1), (.6, 1), (1, .5), (.6, 0), (0, 0)]],
        "Y": [[(0, 0), (.5, .5), (1, 0)], [(.5, .5), (.5, 1)]], "W": [[(0, 0), (.25, 1), (.5, .4), (.75, 1), (1, 0)]],
        "K": [[(0, 0), (0, 1)], [(1, 0), (0, .5), (1, 1)]], "E": [[(1, 0), (0, 0), (0, 1), (1, 1)], [(0, .5), (.7, .5)]],
        "U": [[(0, 0), (0, 1), (1, 1), (1, 0)]], "P": [[(0, 1), (0, 0), (1, 0), (1, .5), (0, .5)]],
        "R": [[(0, 1), (0, 0), (1, 0), (1, .5), (0, .5), (1, 1)]], "N": [[(0, 1), (0, 0), (1, 1), (1, 0)]],
        "7": [[(0, 0), (1, 0), (.4, 1)]], "M": [[(0, 1), (0, 0), (.5, .5), (1, 0), (1, 1)]], "O": [[(0, 0), (1, 0), (1, 1), (0, 1), (0, 0)]],
        "L": [[(0, 0), (0, 1), (1, 1)]], "V": [[(0, 0), (.5, 1), (1, 0)]], "I": [[(.5, 0), (.5, 1)]], "S": [[(1, 0), (0, 0), (0, .5), (1, .5), (1, 1), (0, 1)]],
    }
    cx = x
    for ch in text:
        if ch == " ":
            cx += size * 0.7
            continue
        for stroke in glyphs.get(ch, []):
            crayon_line(d, [(cx + px * size * 0.7, y + py * size + rnd.uniform(-2, 2)) for px, py in stroke], color, 6, rnd)
        cx += size * 0.95 + rnd.uniform(-3, 3)


def draw(i):
    rnd = random.Random(i * 31)
    img = Image.new("RGBA", (W, H), (246, 241, 228, 255))
    d = ImageDraw.Draw(img, "RGBA")
    for _ in range(1500):  # paper grain
        x, y = rnd.randrange(W), rnd.randrange(H)
        d.point((x, y), fill=(225, 218, 200, 255))
    ground = lambda c="green": crayon_line(d, [(10, 330), (W - 10, 335)], c, 8, rnd)
    sun = lambda: (circle(d, 440, 70, 32, "yellow", True, rnd), [crayon_line(d, [(440 + 42 * math.cos(a), 70 + 42 * math.sin(a)), (440 + 62 * math.cos(a), 70 + 62 * math.sin(a))], "yellow", 6, rnd) for a in [k * math.pi / 4 for k in range(8)]])

    if i == 1:   # house: yellow walls, blue door (the "wrong blue")
        ground(); sun()
        scribble_fill(d, (150, 180, 350, 330), "yellow", rnd); poly(d, [(150, 180), (350, 180), (350, 330), (150, 330)], "brown")
        poly(d, [(130, 185), (250, 90), (370, 185)], "red"); scribble_fill(d, (225, 250, 275, 330), "wrongblue", rnd)
    elif i == 2:  # lantern with stars
        poly(d, [(200, 110), (310, 110), (300, 300), (210, 300)], "brown"); scribble_fill(d, (215, 120, 295, 290), "yellow", rnd)
        for sx, sy in [(90, 80), (420, 120), (120, 260), (400, 290), (260, 50)]:
            poly(d, [(sx, sy - 18), (sx + 6, sy - 4), (sx + 20, sy - 4), (sx + 8, sy + 6), (sx + 12, sy + 20), (sx, sy + 10), (sx - 12, sy + 20), (sx - 8, sy + 6), (sx - 20, sy - 4), (sx - 6, sy - 4)], "yellow")
    elif i == 3:  # sailboat on blue water
        scribble_fill(d, (10, 270, W - 10, 370), "blue", rnd)
        poly(d, [(170, 250), (350, 250), (320, 290), (200, 290)], "wrongblue"); crayon_line(d, [(260, 250), (260, 90)], "brown", 7, rnd)
        poly(d, [(265, 95), (340, 235), (265, 235)], "red")
    elif i == 4:  # bird
        sun(); ground()
        crayon_line(d, [(150, 160), (200, 130), (250, 160)], "black", 8, rnd); crayon_line(d, [(250, 160), (300, 130), (350, 160)], "black", 8, rnd)
        circle(d, 250, 230, 30, "orange", True, rnd)
    elif i == 5:  # door with a big 7
        poly(d, [(180, 60), (330, 60), (330, 340), (180, 340)], "gray"); scribble_fill(d, (190, 70, 320, 330), "gray", rnd)
        letters(d, "7", 225, 140, 80, "black", rnd); circle(d, 305, 210, 8, "yellow", True, rnd)
    elif i == 6:  # family: daddy, mommy, me
        ground(); sun()
        stick_person(d, 140, 320, 200, "blue", rnd, "brown"); stick_person(d, 260, 320, 185, "red", rnd, "black"); stick_person(d, 360, 320, 110, "yellow", rnd, "black")
        letters(d, "DADDY", 60, 30, 34, "blue", rnd)
    elif i == 7:  # sun and dog
        ground(); sun()
        circle(d, 230, 270, 45, "brown", True, rnd); circle(d, 290, 235, 25, "brown", True, rnd)
        for lx in (200, 255): crayon_line(d, [(lx, 300), (lx, 330)], "brown", 8, rnd)
    elif i == 8:  # train
        crayon_line(d, [(10, 320), (W - 10, 320)], "black", 6, rnd)
        for k in range(3):
            poly(d, [(40 + k * 150, 200), (170 + k * 150, 200), (170 + k * 150, 300), (40 + k * 150, 300)], "red")
            circle(d, 70 + k * 150, 310, 16, "black", rnd=rnd); circle(d, 140 + k * 150, 310, 16, "black", rnd=rnd)
            poly(d, [(70 + k * 150, 220), (140 + k * 150, 220), (140 + k * 150, 260), (70 + k * 150, 260)], "wrongblue")
    elif i == 9:  # three mugs
        crayon_line(d, [(30, 300), (480, 300)], "brown", 8, rnd)
        for k, c in enumerate(["red", "blue", "yellow"]):
            x = 90 + k * 140 - (25 if k == 2 else 0)
            s = 0.6 if k == 2 else 1.0
            poly(d, [(x, 300 - 110 * s), (x + 80 * s, 300 - 110 * s), (x + 80 * s, 300), (x, 300)], c)
    elif i == 10:  # stars
        for k in range(9):
            sx, sy = rnd.randint(60, 450), rnd.randint(50, 330)
            poly(d, [(sx, sy - 22), (sx + 7, sy - 5), (sx + 24, sy - 5), (sx + 10, sy + 7), (sx + 14, sy + 24), (sx, sy + 12), (sx - 14, sy + 24), (sx - 10, sy + 7), (sx - 24, sy - 5), (sx - 7, sy - 5)], "yellow")
    elif i == 11:  # lake with a dock and a man
        scribble_fill(d, (10, 230, W - 10, 370), "blue", rnd)
        crayon_line(d, [(30, 230), (220, 230)], "brown", 12, rnd)
        stick_person(d, 120, 225, 120, "green", rnd, "black")
    elif i == 12:  # the drawing she holds up: "DADDY WAKE UP"
        ground(); sun()
        stick_person(d, 160, 320, 170, "blue", rnd, "brown"); stick_person(d, 330, 320, 110, "yellow", rnd, "black")
        letters(d, "DADDY", 40, 30, 46, "red", rnd); letters(d, "WAKE UP", 40, 100, 46, "red", rnd)
    return img.filter(ImageFilter.SMOOTH)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for i in range(1, 13):
        draw(i).convert("RGB").save(OUT / f"drawing_{i:02d}.png", optimize=True)
    print(f"wrote 12 drawings to {OUT}")


if __name__ == "__main__":
    main()
