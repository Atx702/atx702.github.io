"""The 22 prop models Meshy didn't get to (Station / Kitchen / Hospital / Bedroom / Wren's room)."""
import numpy as np

from meshlib import Model, box, cyl, ellipsoid, lathe, sphere, torus, wedge, capsule


def leg_table(m, w, d, h, thick, leg, top_col, leg_col, y0=0.0):
    m.add(box(w, thick, d).move(0, y0 + h - thick / 2, 0), top_col)
    for sx in (-1, 1):
        for sz in (-1, 1):
            m.add(box(leg, h - thick, leg).move(sx * (w / 2 - leg), y0 + (h - thick) / 2, sz * (d / 2 - leg)), leg_col)


# ---------- Station ----------
def red_scarf():
    m = Model("Red_scarf")
    # a long strip lying in soft folds: stacked slabs with a wave
    for i in range(14):
        x = -0.7 + i * 0.1
        y = 0.02 + 0.015 * np.sin(i * 0.9)
        m.add(box(0.11, 0.025, 0.2).rot(0, 0, 6 * np.sin(i * 0.9)).move(x, y, 0.02 * np.sin(i * 0.5)), "red")
    for k in range(5):  # fringe
        m.add(box(0.012, 0.01, 0.07).move(0.78, 0.02, -0.07 + k * 0.035), "red")
        m.add(box(0.012, 0.01, 0.07).move(-0.78, 0.02, -0.07 + k * 0.035), "red")
    return m


def lost_property_bin():
    m = Model("Lost_property_bin")
    m.add(box(0.7, 0.6, 0.45).move(0, 0.3, 0), "darksteel")
    m.add(box(0.74, 0.04, 0.49).rot(-28, 0, 0).move(0, 0.75, -0.12), "steel")  # hinged lid, ajar
    m.add(box(0.5, 0.12, 0.01).move(0, 0.36, 0.226), "paper")
    m.add(box(0.05, 0.05, 0.05).move(0.2, 0.55, 0.23), "steel")  # latch
    return m


# ---------- Kitchen ----------
def kitchen_counter_set():
    m = Model("Kitchen_counter_set")
    m.add(box(2.4, 0.86, 0.6).move(0, 0.43, 0), "cream")
    m.add(box(2.46, 0.04, 0.66).move(0, 0.88, 0.02), "wood")
    m.add(box(0.6, 0.02, 0.45).move(-0.5, 0.895, 0.02), "darksteel")  # sink basin
    m.add(cyl(0.015, 0.015, 0.28, 8).move(-0.5, 1.03, -0.22), "steel")  # tap
    m.add(cyl(0.012, 0.012, 0.14, 8).rot(90, 0, 0).move(-0.5, 1.16, -0.15), "steel")
    for i, x in enumerate((0.45, 0.85)):  # stove burners
        m.add(cyl(0.11, 0.11, 0.015, 16).move(x, 0.915, 0.0), "black")
        m.add(cyl(0.11, 0.11, 0.015, 16).move(x, 0.915, 0.0), "black")
    for x in (-0.6, 0.0, 0.6, 1.0):  # door seams
        m.add(box(0.01, 0.7, 0.005).move(x, 0.4, 0.303), "gray")
    m.add(box(2.4, 0.9, 0.04).move(0, 1.4, -0.3), "yellow")  # back wall piece
    m.add(box(2.4, 0.35, 0.3).move(0, 1.95, -0.15), "cream")  # upper cabinet
    return m


def kettle():
    m = Model("Kettle")
    m.add(lathe([(0.0, 0.0), (0.095, 0.0), (0.12, 0.05), (0.115, 0.13), (0.07, 0.18), (0.02, 0.2)], 16), "steel")
    m.add(sphere(0.022, 8, 6).move(0, 0.215, 0), "black")  # lid knob
    m.add(cyl(0.014, 0.025, 0.12, 8).rot(0, 0, -50).move(0.14, 0.14, 0), "steel")  # spout
    m.add(torus(0.07, 0.012, 14, 6).rot(90, 0, 0).scale(1, 1, 1.3).move(-0.1, 0.16, 0), "black")  # handle
    return m


def mug():
    m = Model("Mug")
    m.add(lathe([(0.0, 0.0), (0.04, 0.0), (0.043, 0.005), (0.045, 0.09), (0.0, 0.09)], 14), "cream")
    m.add(cyl(0.039, 0.039, 0.003, 14).move(0, 0.086, 0), "darkwood")  # coffee
    m.add(torus(0.027, 0.006, 12, 6).rot(90, 0, 0).move(0.05, 0.05, 0), "cream")
    return m


def honey_jar():
    m = Model("Honey_jar")
    m.add(lathe([(0.0, 0.0), (0.055, 0.0), (0.06, 0.02), (0.06, 0.13), (0.045, 0.15), (0.045, 0.165), (0.0, 0.165)], 16), "honey")
    m.add(cyl(0.05, 0.05, 0.02, 16).move(0, 0.175, 0), "steel")  # lid, ajar
    m.add(cyl(0.006, 0.006, 0.22, 6).rot(0, 0, 14).move(0.04, 0.2, 0), "wood")  # dipper
    m.add(sphere(0.02, 8, 6).scale(1, 0.6, 1).move(0.075, 0.08, 0.0), "honey")  # a drip
    return m


def radio():
    m = Model("Radio")
    m.add(box(0.3, 0.17, 0.1).move(0, 0.085, 0), "cream")
    m.add(box(0.12, 0.1, 0.01).move(-0.07, 0.09, 0.052), "darksteel")  # speaker grille
    m.add(box(0.1, 0.03, 0.01).move(0.07, 0.12, 0.052), "paper")  # tuning strip
    m.add(cyl(0.022, 0.022, 0.015, 12).rot(90, 0, 0).move(0.09, 0.06, 0.055), "darkwood")  # dial
    m.add(cyl(0.005, 0.003, 0.3, 6).rot(0, 0, -35).move(0.12, 0.3, -0.03), "steel")  # antenna
    return m


def pull_chain_lamp():
    m = Model("Pull_chain_lamp")
    m.add(cyl(0.005, 0.005, 0.6, 6).move(0, 0.3, 0), "black")  # cord from ceiling at y 0.6
    m.add(cyl(0.02, 0.02, 0.04, 10).move(0, 0.0, 0), "darksteel")  # socket
    m.add(sphere(0.04, 10, 8).move(0, -0.05, 0), "yellow", emissive=True)  # bulb
    for i in range(6):  # pull chain
        m.add(sphere(0.006, 6, 4).move(0.03, -0.02 - i * 0.02, 0), "steel")
    m.add(sphere(0.012, 6, 4).move(0.03, -0.15, 0), "steel")
    for m_ in m.parts: pass
    # lift so the hanging point is at the model's top: origin at the bulb height ~0.6
    for mesh, _, _ in m.parts:
        mesh.v[:, 1] += 0.1
    return m


def wedding_ring():
    m = Model("Wedding_ring")
    m.add(torus(0.0095, 0.0025, 20, 8).rot(90, 0, 0).move(0, 0.0125, 0), "gold")
    return m


# ---------- Hospital ----------
def corridor_module():
    m = Model("Corridor_module")
    L, W, H = 6.0, 2.4, 2.7
    m.add(box(W, 0.05, L).move(0, 0.025, 0), "ltgray")  # floor
    m.add(box(W, 0.05, L).move(0, H - 0.025, 0), "white")  # ceiling
    for s in (-1, 1):
        m.add(box(0.05, H, L).move(s * W / 2, H / 2, 0), "teal")  # walls
        m.add(box(0.06, 0.08, L).move(s * (W / 2 - 0.05), 0.9, 0), "steel")  # handrail
        for z in np.arange(-2.5, 2.6, 1.0):
            m.add(box(0.05, 0.08, 0.04).move(s * (W / 2 - 0.04), 0.86, z), "darksteel")
        m.add(box(0.06, 0.15, L).move(s * (W / 2 - 0.02), 0.075, 0), "ltteal")  # skirting
    for z in (-2.0, 0.0, 2.0):  # ceiling lights
        m.add(box(0.5, 0.04, 1.1).move(0, H - 0.07, z), "white", emissive=True)
    for z in (-2.9, 2.9):  # frame ribs
        m.add(box(W, 0.08, 0.1).move(0, H - 0.04, z), "ltteal")
    return m


def hospital_door():
    m = Model("Hospital_door")
    m.add(box(1.1, 0.06, 0.12).move(0, 2.1, 0), "ltteal")  # frame top
    for s in (-1, 1):
        m.add(box(0.06, 2.1, 0.12).move(s * 0.52, 1.05, 0), "ltteal")
    m.add(box(0.98, 2.04, 0.045).move(0, 1.02, 0), "white")
    m.add(box(0.28, 0.5, 0.05).move(0, 1.55, 0.002), "glass")  # small window
    m.add(box(0.12, 0.07, 0.01).move(0.3, 1.95, 0.03), "navy")  # number plate
    m.add(box(0.12, 0.02, 0.05).move(-0.38, 1.0, 0.04), "steel")  # lever
    m.add(box(0.98, 0.25, 0.05).move(0, 0.13, 0.003), "ltgray")  # kick plate
    return m


def waiting_chair():
    m = Model("Waiting_chair")
    m.add(box(0.5, 0.08, 0.5).move(0, 0.45, 0), "teal")  # seat
    m.add(box(0.5, 0.45, 0.07).rot(-8, 0, 0).move(0, 0.75, -0.24), "teal")  # back
    for s in (-1, 1):
        m.add(box(0.035, 0.035, 0.45).move(s * 0.27, 0.62, 0), "steel")  # armrest
        m.add(box(0.035, 0.2, 0.035).move(s * 0.27, 0.52, 0.17), "steel")
        for z in (-0.2, 0.2):
            m.add(box(0.035, 0.45, 0.035).move(s * 0.22, 0.225, z), "steel")  # legs
    return m


def hospital_vending_machine():
    m = Model("Vending_machine_coffee")
    m.add(box(0.95, 1.85, 0.75).move(0, 0.925, 0), "gray")
    m.add(box(0.6, 1.2, 0.02).move(-0.13, 1.1, 0.376), "teal", emissive=True)  # glowing front panel
    m.add(box(0.2, 0.75, 0.03).move(0.34, 1.2, 0.376), "darksteel")  # keypad column
    for i in range(6):
        m.add(box(0.12, 0.04, 0.02).move(0.34, 1.45 - i * 0.1, 0.395), "ltgray")
    m.add(box(0.5, 0.3, 0.1).move(-0.1, 0.4, 0.33), "black")  # dispense bay
    m.add(box(0.3, 0.02, 0.2).move(-0.1, 0.27, 0.38), "steel")  # drip tray
    return m


def patient_bracelet():
    m = Model("Patient_bracelet")
    m.add(torus(0.032, 0.005, 20, 5).rot(90, 0, 0).scale(1.0, 1.0, 0.8).move(0, 0.005, 0), "white")
    m.add(box(0.06, 0.003, 0.015).move(0, 0.037, 0), "paper")  # printed label
    return m


def gurney():
    m = Model("Gurney")
    m.add(box(0.7, 0.05, 1.9).move(0, 0.85, 0), "steel")  # frame
    m.add(box(0.66, 0.1, 1.8).move(0, 0.92, 0), "sheet")  # mattress + sheet
    m.add(box(0.38, 0.08, 0.28).move(0, 1.0, -0.7), "white")  # pillow
    for z in (-0.8, 0.8):
        for s in (-1, 1):
            m.add(box(0.04, 0.55, 0.04).move(s * 0.3, 0.57, z), "steel")  # legs
            m.add(cyl(0.05, 0.05, 0.04, 10).rot(0, 0, 90).move(s * 0.3, 0.05, z), "black")  # wheels
        m.add(box(0.6, 0.04, 0.04).move(0, 0.32, z), "steel")
    for s in (-1, 1):
        m.add(box(0.03, 0.18, 1.1).move(s * 0.355, 1.05, 0.1), "steel")  # side rails
    return m


# ---------- Bedroom / Wren's room ----------
def dresser():
    m = Model("Dresser")
    m.add(box(1.2, 0.85, 0.48).move(0, 0.425, 0), "darkwood")
    for i in range(3):
        m.add(box(1.12, 0.22, 0.02).move(0, 0.17 + i * 0.26, 0.245), "wood")
        for s in (-1, 1):
            m.add(sphere(0.018, 6, 4).move(s * 0.12, 0.17 + i * 0.26, 0.265), "gold")
    m.add(box(0.9, 0.7, 0.03).move(0, 1.25, -0.1), "glass")  # mirror
    m.add(box(0.96, 0.76, 0.03).move(0, 1.25, -0.115), "darkwood")  # mirror frame
    m.add(box(0.1, 0.4, 0.06).move(-0.35, 0.95, -0.1), "darkwood")  # mirror posts
    m.add(box(0.1, 0.4, 0.06).move(0.35, 0.95, -0.1), "darkwood")
    return m


def defibrillator_monitor():
    m = Model("Defibrillator_monitor")
    m.add(box(0.34, 0.26, 0.2).move(0, 0.13, 0), "redbag")  # body
    m.add(box(0.22, 0.14, 0.01).move(-0.03, 0.17, 0.101), "green", emissive=True)  # screen
    m.add(box(0.07, 0.07, 0.015).move(0.12, 0.1, 0.1), "yellow")  # shock button
    m.add(torus(0.07, 0.01, 14, 6).rot(90, 0, 0).move(0, 0.29, 0), "black")  # carry handle
    for s in (-1, 1):  # paddles
        m.add(box(0.1, 0.06, 0.1).move(s * 0.22, 0.1, 0.1), "darksteel")
        m.add(box(0.04, 0.04, 0.09).move(s * 0.22, 0.14, 0.1), "black")
    m.add(cyl(0.006, 0.006, 0.35, 6).rot(0, 0, 90).move(0, 0.03, 0.16), "black")  # cable
    return m


def paramedic_bag():
    m = Model("Paramedic_bag")
    m.add(box(0.5, 0.28, 0.3).move(0, 0.14, 0), "redbag")
    m.add(box(0.5, 0.03, 0.15).rot(-65, 0, 0).move(0, 0.3, -0.19), "redbag")  # open flap
    m.add(box(0.12, 0.02, 0.01).move(0, 0.18, 0.151), "white")  # cross arms
    m.add(box(0.02, 0.12, 0.01).move(0, 0.18, 0.151), "white")
    m.add(torus(0.12, 0.01, 14, 6).rot(90, 0, 0).move(0, 0.28, 0), "black")  # strap
    m.add(box(0.18, 0.04, 0.12).move(-0.05, 0.28, 0.0), "white")  # bandages
    return m


def child_bed():
    m = Model("Child_bed")
    m.add(box(0.95, 0.12, 1.7).move(0, 0.3, 0), "wood")
    m.add(box(0.88, 0.14, 1.6).move(0, 0.44, 0), "mattress")
    m.add(box(0.9, 0.06, 1.1).move(0, 0.52, 0.2), "yellow")  # yellow blanket
    m.add(box(0.95, 0.5, 0.06).move(0, 0.55, -0.85), "wood")  # headboard
    m.add(box(0.95, 0.35, 0.06).move(0, 0.48, 0.85), "wood")  # footboard
    for s in (-1, 1):
        for z in (-0.82, 0.82):
            m.add(box(0.06, 0.24, 0.06).move(s * 0.44, 0.12, z), "wood")
    m.add(box(0.4, 0.09, 0.28).move(0, 0.55, -0.6), "white")  # pillow
    m.add(wedge(0.14, 0.04, 0.14).rot(0, 0, 0).scale(1, 1, 1).move(0, 0.6, -0.6), "star")  # star on the pillow
    return m


def night_lantern():
    m = Model("Night_lantern")
    m.add(cyl(0.11, 0.1, 0.03, 12).move(0, 0.015, 0), "wood")  # base
    m.add(cyl(0.1, 0.1, 0.03, 12).move(0, 0.2, 0), "wood")  # top ring
    for k in range(4):
        a = k * 90
        m.add(box(0.012, 0.18, 0.012).move(0.085 * np.cos(np.radians(a)), 0.108, 0.085 * np.sin(np.radians(a))), "wood")
    m.add(cyl(0.085, 0.085, 0.16, 12).move(0, 0.108, 0), "paper", emissive=True)  # paper shade
    m.add(box(0.04, 0.04, 0.17).move(0, 0.108, 0), "star", emissive=True)  # cut-out stars (inner bar)
    m.add(sphere(0.012, 6, 4).move(0, 0.23, 0), "gold")  # finial
    return m


def rubber_duck():
    m = Model("Rubber_duck")
    m.add(ellipsoid(0.04, 0.032, 0.05, 14, 8).move(0, 0.032, 0), "yellow")  # body
    m.add(sphere(0.026, 12, 8).move(0, 0.078, 0.03), "yellow")  # head
    m.add(box(0.03, 0.008, 0.026).move(0, 0.073, 0.062), "honey")  # beak
    m.add(ellipsoid(0.014, 0.022, 0.03, 8, 6).move(0, 0.05, -0.05), "yellow")  # tail
    for s in (-1, 1):
        m.add(sphere(0.004, 4, 3).move(s * 0.014, 0.088, 0.05), "black")
    return m


def crayons_box():
    m = Model("Crayons_box")
    m.add(box(0.14, 0.02, 0.1).move(0, 0.01, 0), "paper")
    m.add(box(0.14, 0.04, 0.005).move(0, 0.04, -0.05), "paper")  # open lid
    cols = ["red", "yellow", "blue", "green", "honey", "teal", "gray", "star"]
    for i, c in enumerate(cols):
        x = -0.056 + i * 0.016
        broken = i in (2, 5)
        h = 0.04 if broken else 0.085
        m.add(cyl(0.006, 0.006, h, 8).rot(90, 0, 0).move(x, 0.026, 0.0), c)
    m.add(cyl(0.006, 0.006, 0.04, 8).rot(90, 0, 14).move(0.12, 0.006, 0.06), "red")  # loose broken crayon
    return m


PROPS = [
    ("Station", red_scarf), ("Station", lost_property_bin),
    ("Kitchen", kitchen_counter_set), ("Kitchen", kettle), ("Kitchen", mug), ("Kitchen", honey_jar),
    ("Kitchen", radio), ("Kitchen", pull_chain_lamp), ("Kitchen", wedding_ring),
    ("Hospital", corridor_module), ("Hospital", hospital_door), ("Hospital", waiting_chair),
    ("Hospital", hospital_vending_machine), ("Hospital", patient_bracelet), ("Hospital", gurney),
    ("Bedroom_Wren_s_room", dresser), ("Bedroom_Wren_s_room", defibrillator_monitor),
    ("Bedroom_Wren_s_room", paramedic_bag), ("Bedroom_Wren_s_room", child_bed),
    ("Bedroom_Wren_s_room", night_lantern), ("Bedroom_Wren_s_room", rubber_duck),
    ("Bedroom_Wren_s_room", crayons_box),
]
