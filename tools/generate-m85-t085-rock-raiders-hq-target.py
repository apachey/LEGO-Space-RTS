#!/usr/bin/env python3
"""Generate the T085 Rock Raiders HQ ADAPTED composition-target renders.

Project-authored review aid for `building.rock_raiders.hq`.  The renders show
the proposed arrangement of the official LEGO 4990 base sections on the
authoritative 8x8-cell footprint, the visible game adaptations and the state
beats.  They are NOT official LEGO imagery: every module's exact bricks,
colours and connections remain locked to the official 4990 instructions,
which could not be retrieved in the authoring environment (network policy).

Standard library only.  Rendering drives a local Chromium/Chrome in headless
mode (WebGL2).  `--check` rebuilds the scene in memory and verifies the scene
hash and the committed render hashes without launching a browser.
"""
import argparse
import glob
import hashlib
import json
import math
import os
import shutil
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PKG = ROOT / 'ArtSource/M85/T085/RockRaidersHQV1'
RENDERER = PKG / 'render/composition_renderer.html'
RENDERS = PKG / 'renders'
MANIFEST = PKG / 'composition_render_manifest.json'

STUD = 0.4    # metres per stud: 8 mm x 50
PLATE = 0.16  # metres per plate: 3.2 mm x 50
EDGE = 40     # plinth edge in studs: 8 cells x 2 m / 0.4 m

# LEGO 1999 palette approximations (sRGB).  Distribution per module is a
# proposal pending the official 4990 pixel check.
LTGRAY, DKGRAY, TURQ, YELLOW, BLACK, CRYSTAL, LAMP, TEAM, PRINT, ROCK, FIGURE, HILITE, DIM, TRANS_BLACK = range(14)
PALETTE = ['#A7ACA8', '#6D6E5C', '#008F9B', '#F2CD37', '#2E2E2E', '#B8F000', '#FFF1B0',
           '#2F6FD6', '#7A807C', '#5E5F58', '#D9D9D6', '#BFC4C0', '#4A4B45', '#3B4247']

PLAT = 6  # plateau top in plates above ground (baseplate 1 plate + 5 plates relief)
BREAK = 36  # tower ring where the upper frame fails in the Critical state
DECK = 46   # tower deck (plates); crown top = DECK + 12 plates = 58 plates, about 9.3 m


def hsh(*parts):
    return int(hashlib.sha256('|'.join(str(p) for p in parts).encode()).hexdigest()[:8], 16) / 0xFFFFFFFF


# ---------------------------------------------------------------- matrices
def ident():
    return [1.0, 0, 0, 0, 0, 1.0, 0, 0, 0, 0, 1.0, 0, 0, 0, 0, 1.0]


def mmul(a, b):
    o = [0.0] * 16
    for c in range(4):
        for r in range(4):
            o[c * 4 + r] = sum(a[k * 4 + r] * b[c * 4 + k] for k in range(4))
    return o


def trans(x, y, z):
    m = ident()
    m[12], m[13], m[14] = x, y, z
    return m


def rot(axis, deg):
    a = math.radians(deg)
    c, s = math.cos(a), math.sin(a)
    m = ident()
    if axis == 'y':
        m[0], m[2], m[8], m[10] = c, -s, s, c
    elif axis == 'x':
        m[5], m[6], m[9], m[10] = c, s, -s, c
    else:
        m[0], m[1], m[4], m[5] = c, s, -s, c
    return m


def about(pivot, axis, deg):
    """Rotation about a pivot given in metres."""
    return mmul(trans(*pivot), mmul(rot(axis, deg), trans(-pivot[0], -pivot[1], -pivot[2])))


def M(x, z, y):
    """LEGO units (studs from the NW plinth corner, plates above ground) to metres."""
    return ((x - EDGE / 2) * STUD, y * PLATE, (z - EDGE / 2) * STUD)


# ---------------------------------------------------------------- builder
class Builder:
    def __init__(self, name):
        self.name = name
        self.prims = []
        self.matrices = []
        self.stack = [None]
        self.solids = []
        self.studs = []
        self._id = 0

    def nid(self):
        self._id = self._id % 254 + 1
        return self._id

    def cur(self):
        return self.stack[-1]

    def push(self, m):
        top = self.cur()
        self.stack.append(m if top is None else mmul(self.matrices[top], m))
        self.matrices.append(self.stack[-1])
        self.stack[-1] = len(self.matrices) - 1

    def pop(self):
        self.stack.pop()

    def _mi(self):
        return -1 if self.cur() is None else self.cur()

    def brick(self, x, z, y, w, d, h, col, studs=True, emi=0.0):
        cx, cy, cz = M(x + w / 2, z + d / 2, y + h / 2)
        self.prims.append(['b', cx, cy, cz, w * STUD, h * PLATE, d * STUD, col, emi, self.nid(), self._mi()])
        self.solids.append((x, x + w, z, z + d, y, y + h, self.cur()))
        if studs:
            for i in range(int(round(w))):
                for j in range(int(round(d))):
                    self.studs.append((x + i + 0.5, z + j + 0.5, y + h, col, self.cur()))

    def tile(self, x, z, y, w, d, col, emi=0.0):
        self.brick(x, z, y, w, d, 0.5, col, studs=False, emi=emi)

    def vcyl(self, x, z, y, r, h, col, emi=0.0):
        bx, by, bz = M(x, z, y)
        self.prims.append(['c', bx, by, bz, r * STUD, h * PLATE, 0, col, emi, self.nid(), self._mi()])
        self.solids.append((x - r, x + r, z - r, z + r, y, y + h, self.cur()))

    def hcyl(self, x, z, y, r, length, axis, col, emi=0.0):
        bx, by, bz = M(x, z, y)
        self.prims.append(['c', bx, by, bz, r * STUD, length * STUD, 1 if axis == 'x' else 2, col, emi, self.nid(), self._mi()])

    def wedge(self, x, z, y, w, d, h, direction, low, col):
        cx, cy, cz = M(x + w / 2, z + d / 2, y + h / 2)
        self.prims.append(['w', cx, cy, cz, w * STUD, h * PLATE, d * STUD, col, 0.0, self.nid(),
                           ['+z', '-z', '+x', '-x'].index(direction), low, self._mi()])

    def crystal(self, x, z, y, r, h, col, emi):
        cx, cy, cz = M(x, z, y + h / 2)
        self.prims.append(['x', cx, cy, cz, r * STUD, h * PLATE, 0, col, emi, self.nid(), self._mi()])

    def rock(self, x, z, y, r, seed, col=ROCK):
        cx, cy, cz = M(x, z, y + r * STUD / PLATE * 0.6)
        self.prims.append(['r', cx, cy, cz, r * STUD, seed, 0, col, 0.0, self.nid(), self._mi()])

    def beam(self, p0, p1, t, col, emi=0.0):
        """Square-section beam between two LEGO-unit points (x, z, y); t in studs."""
        a, b = M(*p0), M(*p1)
        dx, dy, dz = b[0] - a[0], b[1] - a[1], b[2] - a[2]
        length = math.sqrt(dx * dx + dy * dy + dz * dz)
        yaw = math.degrees(math.atan2(-dz, dx))
        pitch = math.degrees(math.atan2(dy, math.hypot(dx, dz)))
        mid = ((a[0] + b[0]) / 2, (a[1] + b[1]) / 2, (a[2] + b[2]) / 2)
        m = mmul(trans(*mid), mmul(rot('y', yaw), rot('z', pitch)))
        self.push(m)
        self.prims.append(['b', 0.0, 0.0, 0.0, length, t * STUD, t * STUD, col, emi, self.nid(), self._mi()])
        self.pop()

    def finish(self):
        cells = {}
        for solid in self.solids:
            x0, x1, z0, z1 = solid[:4]
            for cx in range(int(math.floor(x0)), int(math.ceil(x1))):
                for cz in range(int(math.floor(z0)), int(math.ceil(z1))):
                    cells.setdefault((cx, cz), []).append(solid)
        for (x, z, ytop, col, mi) in self.studs:
            covered = False
            for (x0, x1, z0, z1, y0, y1, smi) in cells.get((int(math.floor(x)), int(math.floor(z))), ()):
                if smi == mi and x0 < x < x1 and z0 < z < z1 and y0 <= ytop + 1e-6 < y1:
                    covered = True
                    break
            if not covered:
                sx, sy, sz = M(x, z, ytop)
                self.prims.append(['s', sx, sy, sz, col, self.nid(), -1 if mi is None else mi])
        out = {'prims': [[round(v, 4) if isinstance(v, float) else v for v in p] for p in self.prims],
               'matrices': [[round(v, 6) for v in m] for m in self.matrices]}
        return out


# ---------------------------------------------------------------- HQ composition
def in_pit(x, z):
    """Four recessed corner seats, after the four corner pits of 30271px3."""
    return (x < 14 or x >= 26) and (z < 14 or z >= 26)


def in_ramp(x, z):
    return 16 <= x < 24 and z >= 28


def block_is_edge(bx, bz):
    return (in_pit(bx - 2, bz) or in_pit(bx + 2, bz) or in_pit(bx, bz - 2) or in_pit(bx, bz + 2)
            or bx == 0 or bx == EDGE - 2 or bz == 0 or bz == EDGE - 2)


def ground_y(x, z):
    """Top surface height (plates) of the plinth at a stud position."""
    if in_ramp(x, z):
        return 1 + (PLAT - 1) * max(0.0, min(1.0, (EDGE - z) / 12.0))
    if in_pit(x, z):
        return 1
    bx, bz = int(x // 2) * 2, int(z // 2) * 2
    return 4 if block_is_edge(bx, bz) else PLAT


def plinth(b, state):
    # Baseplate: light-grey rock baseplate after 30271px3, adapted to 40 x 40 studs.
    b.brick(0, 0, 0, EDGE, EDGE, 1, LTGRAY)
    # Raised rock plateau: plus-shaped relief leaving four recessed corner seats
    # (exact contour, pit shape and module seating pending the official source check).
    for bx in range(0, EDGE, 2):
        for bz in range(0, EDGE, 2):
            if in_pit(bx, bz) or in_ramp(bx, bz):
                continue
            h = 3 if block_is_edge(bx, bz) else PLAT - 1
            n = hsh('print', bx // 6, bz // 6) * 0.65 + hsh('print2', bx, bz) * 0.35
            col = PRINT if n > 0.63 else (HILITE if n < 0.18 else LTGRAY)
            b.brick(bx, bz, 1, 2, 2, h, col)
    # South ramp from the plateau to the production exit (smooth slope, no studs).
    b.wedge(16, 28, 1, 8, 12, PLAT - 1, '+z', 0.0, LTGRAY)
    # Rock outcrops at the seat edges.
    for i, (x, z) in enumerate([(12.8, 3.5), (26.9, 1.8), (12.8, 30.5), (3.2, 12.8), (38.4, 1.6),
                                (26.9, 38.4), (38.2, 38.3), (9.5, 27.0)]):
        b.rock(x, z, ground_y(x, z), 1.2 + 0.5 * hsh('rk', i), 11 + i, ROCK if i % 3 else PRINT)


def tower(b, state):
    """NW seat: the official Mining Laser tower (tallest part), adapted as the command lamp tower."""
    phase, dmg = state['construction'], state['damage']
    b.brick(2, 2, 1, 10, 10, 3, DKGRAY)
    lower_top = 16 if (phase >= 0.5 and dmg < 4) else 10
    b.brick(3, 3, 4, 8, 8, lower_top - 4, TURQ, studs=lower_top < 16)
    b.brick(6, 11.03, 4, 2, 0.15, 9, BLACK, studs=False)  # service door (source-unverified detail)
    if phase < 0.5 or dmg >= 4:
        return
    corners = [(3, 3), (9, 3), (3, 9), (9, 9)]
    drop = [0, 0.12, 0.3, 0.5, 1][dmg]

    def frame(y0, y1, missing=()):
        for (x, z) in corners:
            if (x, z) not in missing:
                b.brick(x, z, y0, 2, 2, y1 - y0, DKGRAY, studs=False)
        b.brick(5, 5, y0, 4, 4, y1 - y0, DKGRAY, studs=False)

    def braces(y0, y1):
        if hsh('brace', y0, state['name']) < drop:
            return
        b.beam((3.5, 11.1, y0 + 1), (10.5, 11.1, y1 - 1), 0.35, BLACK)
        b.beam((11.1, 3.5, y0 + 1), (11.1, 10.5, y1 - 1), 0.35, BLACK)

    def ladder(y0, y1):
        b.brick(4.2, 11.05, y0, 0.25, 0.3, y1 - y0, BLACK, studs=False)
        b.brick(5.3, 11.05, y0, 0.25, 0.3, y1 - y0, BLACK, studs=False)

    # Lower frame (always upright while standing).
    frame(16, BREAK)
    b.brick(2.9, 2.9, 26, 8.2, 8.2, 1, TURQ, studs=False)
    braces(16, 26)
    braces(27, BREAK)
    ladder(16, BREAK)
    # Upper frame: in the Critical state it fails at the second ring and leans west.
    lean = 9 if dmg >= 3 else 0
    b.push(about(M(3, 7, BREAK), 'z', lean))
    frame(BREAK, DECK, missing=((9, 3),) if dmg >= 2 else ())
    b.brick(2.9, 2.9, BREAK, 8.2, 8.2, 1, TURQ, studs=False)
    braces(BREAK + 1, DECK)
    ladder(BREAK, DECK)
    b.brick(2, 2, DECK, 10, 10, 1, TURQ)
    b.tile(8.5, 2.6, DECK + 1, 2, 4, TEAM)  # team Identification Tile (game adaptation)
    if phase >= 1.0:
        if dmg < 2:
            for (x, z) in [(2.3, 2.3), (11.7, 2.3), (2.3, 11.7), (11.7, 11.7), (7, 2.3), (7, 11.7), (2.3, 7), (11.7, 7)]:
                b.vcyl(x, z, DECK + 1, 0.15, 3, BLACK)
            b.brick(2.1, 2.1, DECK + 4, 9.8, 0.3, 0.6, BLACK, studs=False)
            b.brick(2.1, 11.6, DECK + 4, 9.8, 0.3, 0.6, BLACK, studs=False)
            b.brick(2.1, 2.4, DECK + 4, 0.3, 9.2, 0.6, BLACK, studs=False)
            b.brick(11.6, 2.4, DECK + 4, 0.3, 9.2, 0.6, BLACK, studs=False)
        lamp_on = 0.0 if (state['brownout'] or dmg >= 2) else 0.95
        # Rotating head: the source's light-up Mining Laser (the same dark-grey 30346c01
        # 4 x 12 x 2 light assembly used by 4970 Chrome Crusher), presented as a
        # non-firing rotating work beam (explicit director decision).
        yaw = 0 if state['brownout'] else (-45 + (25 if dmg >= 2 else 0))
        h0 = DECK + 1
        b.push(about(M(7, 7, h0), 'y', yaw))
        b.vcyl(7, 7, h0, 2.0, 1, BLACK)                               # turntable
        b.brick(6, 6, h0 + 1, 2, 2, 3, BLACK, studs=False)            # short mount post
        b.brick(5, 1.5, h0 + 4, 4, 9, 6, DKGRAY, studs=False)         # body of the 4 x 12 x 2 light assembly
        b.brick(5.5, 10.5, h0 + 4.5, 3, 2.5, 5, DKGRAY, studs=False)  # lens housing (12 studs overall)
        b.brick(5.8, 13.0, h0 + 5.2, 2.4, 0.25, 3.6, LAMP if lamp_on else DIM, studs=False, emi=lamp_on)
        b.brick(5.5, 2.0, h0 + 10, 3, 2, 1, BLACK, studs=True)        # top switch block
        b.pop()
    b.pop()


def crane(b, state):
    """E arm: the official crane section (Upgrade Station), adapted as construction/receiving crane."""
    phase, dmg = state['construction'], state['damage']
    top = PLAT + 3
    b.brick(29, 16, PLAT, 8, 8, 3, DKGRAY, studs=phase < 0.5)
    if phase < 0.5:
        return
    if dmg >= 4:
        b.beam((27.2, 16, PLAT + 2.5), (27.2, 25, PLAT + 2.5), 2, YELLOW)  # fallen mast on the east arm
        b.beam((30.5, 14.8, PLAT), (34.5, 3.5, 2), 0.7, YELLOW)            # fallen boom into the receiving seat
        return
    b.vcyl(33, 20, top, 3.4, 1, DKGRAY)
    if dmg < 3:
        b.brick(33.6, 20.2, top + 1, 3, 4.4, 9, TURQ)
        b.brick(34.0, 24.62, top + 5, 2.2, 0.12, 3, TRANS_BLACK, studs=False)
        b.brick(30.8, 22.9, top + 1, 2.6, 1.1, 6, BLACK, studs=False)
        lamp_on = 0.0 if (state['brownout'] or dmg >= 2 or phase < 1.0) else 0.95
        b.brick(34.6, 20.5, top + 10, 1, 1, 1.5, LAMP if lamp_on else DIM, studs=False, emi=lamp_on)
    b.brick(32, 19, top + 1, 2, 2, 18, YELLOW)
    if phase < 1.0:
        return
    mast_top = top + 19
    if dmg >= 3:
        # Critical: the boom has detached and lies across the east arm into the receiving seat.
        b.beam((32.0, 15.2, PLAT + 1), (34.2, 3.6, 2), 0.7, YELLOW)
        b.beam((33.4, 15.4, PLAT + 1), (35.6, 3.8, 2), 0.7, YELLOW)
        return
    length = 13.24
    ang = math.radians(25 if dmg < 2 else -38)
    tip = (33, 20 - length * math.cos(ang), mast_top + length * math.sin(ang) * STUD / PLATE)
    for off in (-0.7, 0.7):
        b.beam((33 + off, 20, mast_top), (33 + off, tip[1], tip[2]), 0.6, YELLOW)
    n = 7
    for i in range(n):
        if hsh('brace-crane', i, state['name']) < [0, 0.1, 0.35][dmg]:
            continue
        t0, t1 = i / n, (i + 1) / n
        b.beam((32.4, 20 + (tip[1] - 20) * t0, mast_top + (tip[2] - mast_top) * t0),
               (33.6, 20 + (tip[1] - 20) * t1, mast_top + (tip[2] - mast_top) * t1), 0.22, BLACK)
    if dmg >= 2:
        return  # hook and boulder lost when the boom drops
    chain = 9
    for k in range(chain):
        b.brick(32.8, tip[1] - 0.2, tip[2] - 1 - k, 0.4, 0.4, 0.9, BLACK, studs=False)
    hy = tip[2] - 1 - chain
    b.brick(32.4, tip[1] - 0.6, hy - 2, 1.2, 1.2, 2, YELLOW, studs=False)
    b.rock(33, tip[1], hy - 2 - 4.4, 1.1, 77)


def energy(b, state):
    """SE seat: the official Power Station top: crystal energy store and boulder splitter."""
    phase, dmg = state['construction'], state['damage']
    hh = 12 if phase >= 0.5 else 6
    if dmg >= 4:
        hh = 5
    b.brick(28, 28, 1, 9, 8, hh, TURQ, studs=phase < 0.5 or dmg >= 4)
    if phase < 0.5:
        return
    if dmg >= 4:
        for i in range(5):
            b.crystal(29 + 1.6 * i, 31 + (i % 2), 6, 0.3, 3, CRYSTAL, 0.15)
        return
    b.brick(29, 36.02, 3, 7, 0.12, 7, LTGRAY, studs=False)
    b.brick(29, 29, 13, 4, 4, 3, LTGRAY)
    if phase >= 1.0:
        glow = 0.2 if state['brownout'] else [0.9, 0.8, 0.55, 0.3][min(dmg, 3)]
        b.crystal(31, 31, 16, 1.4, 14, CRYSTAL, glow)
    b.brick(34.5, 29, 13, 2, 2, 9, YELLOW)
    if phase >= 1.0 and dmg < 3:
        b.hcyl(35.5, 31, 20, 0.45, 2.6, 'z', BLACK)
        b.hcyl(35.5, 33.6, 20, 0.3, 0.2, 'z', LAMP, emi=0.0 if state['brownout'] else 0.6)
        b.brick(34, 33, 13, 3, 3, 1, LTGRAY)
        b.rock(34.9, 34.2, 14, 0.8, 31)
        b.rock(36.1, 34.8, 14, 0.75, 32)
        b.crystal(35.5, 34.5, 14, 0.33, 4, CRYSTAL, 0.0 if state['brownout'] else 0.7)


def dumper(b, state):
    """NE seat: the official boulder dumper, adapted as the emergency Ore receiving bay."""
    phase, dmg = state['construction'], state['damage']
    legs = [(28, 3), (36, 3), (28, 11), (36, 11)]
    lh = 18 if phase >= 0.5 else 6
    if dmg >= 4:
        lh = 5
    for (x, z) in legs:
        b.brick(x, z, 1, 2, 2, lh, DKGRAY, studs=lh < 18)
    # Receiving-lane hazard edges (decal on the lane floor; game adaptation).
    for i in range(12):
        c = YELLOW if i % 2 == 0 else BLACK
        b.tile(27 + i, 4.6, 1, 1, 0.4, c)
        b.tile(27 + i, 11.0, 1, 1, 0.4, c)
    if phase < 0.5:
        return
    if dmg >= 4:
        b.beam((27, 6, 2), (35, 9, 2), 2, TURQ)
        return
    b.brick(28, 3, 19, 10, 2, 3, TURQ)
    b.brick(28, 11, 19, 10, 2, 3, TURQ)
    b.brick(28, 5, 19, 2, 6, 2, DKGRAY, studs=False)
    b.brick(36, 5, 19, 2, 6, 2, DKGRAY, studs=False)
    if phase < 1.0:
        return
    tilt = {0: 0, 1: 0, 2: 22, 3: 64}[min(dmg, 3)]
    drop = -8 if dmg == 3 else 0
    b.push(mmul(trans(0, drop * PLATE, 0), about(M(30, 11, 22), 'x', tilt)))
    b.brick(30, 5, 21, 6, 6, 3, YELLOW, studs=False)
    b.brick(30, 5, 24, 6, 0.6, 7, YELLOW, studs=False)
    b.brick(30, 10.4, 24, 6, 0.6, 7, YELLOW, studs=False)
    b.brick(30, 5.6, 24, 0.6, 4.8, 7, YELLOW, studs=False)
    b.brick(35.4, 5.6, 24, 0.6, 4.8, 7, YELLOW, studs=False)
    b.tile(30.6, 5.6, 24, 4.8, 4.8, BLACK)
    if dmg == 0:
        for i, (x, z) in enumerate([(32, 7.2), (33.8, 8.4), (32.6, 9.0)]):
            b.rock(x, z, 24.4, 0.75, 51 + i)
    b.pop()


def crew_pad(b, state):
    """Hub: the official Teleport Pad section, adapted as the Crew production pad."""
    phase = state['construction']
    dmg = state['damage']
    b.brick(15, 15, PLAT, 10, 10, 1, TURQ)
    if phase >= 0.5 and dmg < 4:
        b.tile(16, 16, PLAT + 1, 8, 8, DKGRAY)
    b.tile(18, 24.1, PLAT + 1, 4, 0.8, TEAM)  # team Identification Tile (game adaptation)
    if phase < 1.0 or dmg >= 3:
        return
    lamp_on = 0.0 if state['brownout'] else 0.95
    for i, (x, z) in enumerate([(15.5, 24.5), (24.5, 24.5)]):
        if dmg == 2 and i == 0:
            continue
        b.vcyl(x, z, PLAT + 1, 0.25, 12, BLACK)
        on = lamp_on if not (dmg >= 1 and i == 1) else 0.0
        b.brick(x - 0.5, z - 0.5, PLAT + 13, 1, 1, 2, LAMP if on else DIM, studs=False, emi=on)


def open_pit(b, state):
    """SW seat: left as open rock; no invented module."""
    for i, (x, z, r) in enumerate([(5.5, 31.5, 1.6), (8.8, 35.2, 1.2), (4.2, 36.4, 0.9)]):
        b.rock(x, z, 1, r, 60 + i, ROCK if i != 1 else PRINT)


def construction_dressing(b, state):
    """Delivered sections waiting for assembly (presentation only)."""
    if state['construction'] >= 1.0:
        return
    stacks = [(15.2, 25.3, TURQ), (21.8, 25.3, DKGRAY)] if state['construction'] < 0.5 else [(21.8, 25.3, YELLOW)]
    for (x, z, col) in stacks:
        g = ground_y(x + 1, z + 1)
        for k in range(3):
            b.brick(x, z, g + k, 3, 2, 1, col)


def debris(b, state):
    count = [0, 7, 16, 26, 44][state['damage']]
    cols = [DKGRAY, TURQ, YELLOW, BLACK, LTGRAY]
    for i in range(count):
        a, r = hsh('deb-a', i) * math.tau, 6 + 15 * hsh('deb-r', i)
        x, z = 20 + math.cos(a) * r, 20 + math.sin(a) * r
        x, z = min(max(x, 1), 38), min(max(z, 1), 38)
        base = ground_y(x, z)
        b.push(about(M(x, z, base), 'y', hsh('deb-y', i) * 90))
        w, d = (2, 1) if i % 3 else (2, 2)
        b.brick(x, z, base, w, d, 3 if i % 4 == 0 else 1, cols[i % len(cols)])
        b.pop()
    if state['damage'] >= 4:
        # Collapsed tower sections lying across the north arm and west seat.
        for k, (p0, p1) in enumerate([((4, 14.5, PLAT + 2), (4, 26, PLAT + 2)), ((14.5, 4, PLAT + 2), (25, 6, PLAT + 2)), ((1.5, 26, 3), (10, 38, 3))]):
            b.beam(p0, p1, 2, DKGRAY)
        b.beam((15, 9, PLAT + 1), (24, 12, PLAT + 1), 1, TURQ)


def scale_figure(b, state):
    """~2 m minifigure-scale marker; not part of the building."""
    if not state.get('figure'):
        return
    x, z, y = 21.2, 27.0, PLAT
    b.brick(x, z, y, 1.6, 0.8, 4, FIGURE, studs=False)
    b.brick(x - 0.15, z - 0.05, y + 4, 1.9, 0.9, 4, FIGURE, studs=False)
    b.vcyl(x + 0.8, z + 0.4, y + 8, 0.6, 3, FIGURE)


def build(state):
    b = Builder(state['name'])
    plinth(b, state)
    tower(b, state)
    crane(b, state)
    energy(b, state)
    dumper(b, state)
    crew_pad(b, state)
    open_pit(b, state)
    construction_dressing(b, state)
    debris(b, state)
    scale_figure(b, state)
    return b.finish()


STATES = [
    dict(name='operational', construction=1.0, damage=0, brownout=False, figure=True,
         label='Operational (Healthy 70–100%)'),
    dict(name='construction_30', construction=0.3, damage=0, brownout=False, figure=False,
         label='Construction ~30%: foundation and module seats'),
    dict(name='construction_70', construction=0.7, damage=0, brownout=False, figure=False,
         label='Construction ~70%: tower frame, housings, crane mast'),
    dict(name='brownout', construction=1.0, damage=0, brownout=True, figure=False,
         label='Brownout: command silhouette kept, lamps and crystal dimmed'),
    dict(name='damaged', construction=1.0, damage=1, brownout=False, figure=False,
         label='Damaged 35–69%: displaced parts, one lamp out'),
    dict(name='heavily_damaged', construction=1.0, damage=2, brownout=False, figure=False,
         label='Heavily damaged 20–34%: crane boom drops, hopper tilts, head lamp dark'),
    dict(name='critical', construction=1.0, damage=3, brownout=False, figure=False,
         label='Critical <20%: upper tower leans, boom detached, hopper falls'),
    dict(name='destroyed', construction=1.0, damage=4, brownout=False, figure=False,
         label='Destroyed: readable non-blocking rubble on the plinth'),
    dict(name='operational_nofigure', construction=1.0, damage=0, brownout=False, figure=False,
         label='Operational without scale figure (silhouette/camera checks)'),
]

EXIT_DECAL = [-2.0, 8.0, 2.0, 12.0, '#EFD774']  # production exit: cells x3-4 beyond the south edge
GRID = []
for i in range(-6, 7):
    GRID.append([i * 2 - 0.02, -12.0, i * 2 + 0.02, 12.0, '#8E897F'])
    GRID.append([-12.0, i * 2 - 0.02, 12.0, i * 2 + 0.02, '#8E897F'])
FOOTPRINT = [[-8.06, -8.06, 8.06, -7.94, '#1F2A2C'], [-8.06, 7.94, 8.06, 8.06, '#1F2A2C'],
             [-8.06, -8.06, -7.94, 8.06, '#1F2A2C'], [7.94, -8.06, 8.06, 8.06, '#1F2A2C']]

VIEWS = {
    'hero': dict(type='persp', width=1600, height=1000, fovY=24, yaw=45, pitch=28, target=[0.4, 3.4, -0.2],
                 distance=52, ground='#d6d1c6', partEdges=True, outlinePx=1.0, glowPx=6),
    'plan': dict(type='ortho', width=1000, height=1000, center=[0.0, 2.0], halfWidth=12.0, ground='#e4e0d6',
                 partEdges=True, outlinePx=0.9, glowPx=3, decals=GRID + [EXIT_DECAL] + FOOTPRINT),
    'state': dict(type='persp', width=800, height=560, fovY=24, yaw=45, pitch=32, target=[0.4, 3.0, -0.2],
                  distance=54, ground='#d6d1c6', partEdges=True, outlinePx=0.8, glowPx=4),
}
for cells in (24, 44, 72):
    VIEWS['camera_%d' % cells] = dict(type='persp', width=1280, height=720, fovY=36, yaw=45, pitch=58,
                                      target=[0, 0, 0], zoomCells=cells, ground='#c9c3b4', partEdges=False,
                                      outlinePx=1.0, glowPx=3, decals=[EXIT_DECAL])
    VIEWS['silhouette_%d' % cells] = dict(type='persp', width=1280, height=720, fovY=36, yaw=45, pitch=58,
                                          target=[0, 0, 0], zoomCells=cells, mode='silhouette')
for yaw in (45, 135, 225, 315):
    VIEWS['rotation_%03d' % yaw] = dict(type='persp', width=640, height=360, fovY=36, yaw=yaw, pitch=58,
                                        target=[0, 2.0, 0], zoomCells=18, ground='#c9c3b4', partEdges=False,
                                        outlinePx=0.9, glowPx=3, decals=[EXIT_DECAL])

RENDER_PLAN = [
    ('hero', 'operational', 'rock_raiders_hq_composition_target_hero.png',
     'ADAPTED composition target: operational HQ, neutral three-quarter inspection view from the default camera diagonal (south-east).'),
    ('plan', 'operational', 'rock_raiders_hq_plan_top.png',
     'Top plan on the authoritative 8x8-cell footprint (2 m cells) with the 2x2 production exit south of centre.'),
] + [('state', s['name'], 'rock_raiders_hq_state_%s.png' % s['name'], s['label']) for s in STATES
     if s['name'] not in ('operational_nofigure',)] + [
    ('camera_%d' % c, 'operational', 'rock_raiders_hq_camera_%d.png' % c,
     'Canonical gameplay camera framing (FOV 36, pitch 58, yaw 45, 16:9) at %d horizontal cells; composition preview, not a production-model capture.' % c)
    for c in (24, 44, 72)] + [
    ('silhouette_%d' % c, 'operational_nofigure', 'rock_raiders_hq_silhouette_%d.png' % c,
     'Black silhouette at the %d-cell canonical camera framing.' % c) for c in (24, 44, 72)] + [
    ('rotation_%03d' % y, 'operational_nofigure', 'rock_raiders_hq_rotation_%03d.png' % y,
     'Gameplay pitch at camera yaw %d degrees (90-degree rotation steps).' % y) for y in (45, 135, 225, 315)]

LIGHT = [-0.45, 0.80, 0.40]


def scene_document():
    return {
        'schemaVersion': 1,
        'task': 'T085',
        'stableId': 'building.rock_raiders.hq',
        'authority': 'Project-authored ADAPTED composition target; NON-AUTHORITATIVE FOR OFFICIAL LEGO 4990 MODULE GEOMETRY.',
        'units': {'studMetres': STUD, 'plateMetres': PLATE, 'plinthStuds': EDGE, 'footprintCells': 8, 'cellMetres': 2},
        'palette': PALETTE,
        'light': LIGHT,
        'views': VIEWS,
        'states': {s['name']: build(s) for s in STATES},
    }


def canonical_sha(doc):
    return hashlib.sha256(json.dumps(doc, sort_keys=True, separators=(',', ':')).encode()).hexdigest()


def file_sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def find_browser():
    env = os.environ.get('CHROME_BIN')
    if env and Path(env).is_file():
        return env
    candidates = sorted(glob.glob('/opt/pw-browsers/chromium_headless_shell-*/chrome-linux/headless_shell'), reverse=True) + \
        sorted(glob.glob('/opt/pw-browsers/chromium-*/chrome-linux/chrome'), reverse=True) + [
        '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
        '/Applications/Chromium.app/Contents/MacOS/Chromium',
        shutil.which('chromium') or '', shutil.which('google-chrome') or '']
    for c in candidates:
        if c and Path(c).is_file():
            return c
    return None


def render(doc, only=None):
    browser = find_browser()
    if not browser:
        raise SystemExit('No Chromium/Chrome found; set CHROME_BIN to render (use --check to verify committed renders).')
    RENDERS.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix='t085-hq-render-') as tmp:
        tmp = Path(tmp)
        shutil.copy(RENDERER, tmp / 'composition_renderer.html')
        for view, state, fname, desc in RENDER_PLAN:
            if only and fname not in only:
                continue
            payload = {k: doc[k] for k in ('palette', 'light', 'views')}
            payload['states'] = {state: doc['states'][state]}
            (tmp / 'scene_data.js').write_text('window.HQ_SCENE = ' + json.dumps(payload, separators=(',', ':')) + ';\n')
            v = doc['views'][view]
            out = RENDERS / fname
            headless = [] if browser.endswith('headless_shell') else ['--headless=new']
            cmd = [browser] + headless + ['--no-sandbox', '--use-angle=swiftshader', '--enable-unsafe-swiftshader',
                   '--hide-scrollbars', '--force-device-scale-factor=1', '--virtual-time-budget=60000',
                   '--window-size=%d,%d' % (v['width'], v['height']), '--screenshot=%s' % out,
                   (tmp / 'composition_renderer.html').as_uri() + '#view=%s&state=%s' % (view, state)]
            subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=600)
            print('rendered', fname)


def write_manifest(doc):
    renders = []
    for view, state, fname, desc in RENDER_PLAN:
        path = RENDERS / fname
        renders.append({'file': str(path.relative_to(ROOT)), 'view': view, 'state': state,
                        'sha256': file_sha(path), 'description': desc})
    manifest = {
        'schemaVersion': 1,
        'task': 'T085',
        'stableId': 'building.rock_raiders.hq',
        'revision': '2026-09-26-v1',
        'kind': 'PROJECT_AUTHORED_COMPOSITION_RENDERS',
        'authority': 'Review aid for the ADAPTED Production Design Target proposal. Not official LEGO imagery and not source evidence. Module bricks, colours and connections remain SOURCE-LOCKED to the official LEGO 4990 instructions.',
        'generator': 'tools/generate-m85-t085-rock-raiders-hq-target.py',
        'renderer': str(RENDERER.relative_to(ROOT)),
        'sceneSha256': canonical_sha(doc),
        'units': doc['units'],
        'camera': 'Gameplay views reproduce RtsCameraController framing: perspective FOV 36 deg vertical, pitch 58 deg, yaw 45 deg (camera south-east of target), 16:9, width = zoom cells x 2 m.',
        'renders': renders,
    }
    MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + '\n')


def check(doc):
    errors = []
    if not MANIFEST.is_file():
        raise SystemExit('FAIL: missing %s' % MANIFEST.relative_to(ROOT))
    manifest = json.loads(MANIFEST.read_text())
    if manifest.get('sceneSha256') != canonical_sha(doc):
        errors.append('scene hash differs from manifest (generator output changed without re-render)')
    listed = {r['file'] for r in manifest.get('renders', [])}
    for view, state, fname, desc in RENDER_PLAN:
        rel = str((RENDERS / fname).relative_to(ROOT))
        if rel not in listed:
            errors.append('render missing from manifest: ' + rel)
    for r in manifest.get('renders', []):
        p = ROOT / r['file']
        if not p.is_file():
            errors.append('missing render file: ' + r['file'])
        elif file_sha(p) != r['sha256']:
            errors.append('render hash mismatch: ' + r['file'])
        if r['view'] not in doc['views'] or r['state'] not in doc['states']:
            errors.append('manifest references unknown view/state: ' + r['file'])
    if errors:
        print('FAIL: T085 Rock Raiders HQ composition renders')
        for e in errors:
            print(' -', e)
        raise SystemExit(1)
    print('PASS: T085 Rock Raiders HQ composition scene hash and %d render hashes' % len(manifest['renders']))


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument('--check', action='store_true', help='verify scene determinism and committed render hashes')
    ap.add_argument('--only', nargs='*', help='render only these output file names')
    args = ap.parse_args()
    doc = scene_document()
    if args.check:
        check(doc)
        return
    render(doc, set(args.only) if args.only else None)
    if not args.only:
        write_manifest(doc)
        print('wrote', MANIFEST.relative_to(ROOT))


if __name__ == '__main__':
    main()
