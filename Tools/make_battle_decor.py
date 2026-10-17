# Décor du chemin de combat façon Forge Master : aplats de couleur, gros contours noirs, une palette par cercle.
# Sortie : Assets/Resources/ForgeUI/Battle/c<cercle>_ground.png (sol répétable en X) et c<cercle>_<élément>.png (base en bas au centre).
# Usage : python Tools/make_battle_decor.py
import math, os, random
from PIL import Image, ImageDraw, ImageFilter, ImageChops

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Battle")
INK = (28, 16, 16, 255)

# ground, patch, road, roadDark, rock, tree, accent, tuft, liquid (rivière/flaques : 0 = aucune, 1 = flaques, 2 = rivière)
P = [
    dict(name="Limbes",      ground=(118, 122, 138), patch=(104, 108, 126), road=(178, 170, 154), rock=(150, 152, 166), tree=(88, 84, 94),  accent=(176, 196, 226), tuft=(92, 96, 114),  liquid=1),
    dict(name="Luxure",      ground=(150, 82, 128),  patch=(132, 66, 112),  road=(220, 166, 176), rock=(176, 114, 164), tree=(84, 40, 74),  accent=(255, 122, 196), tuft=(186, 92, 150), liquid=1),
    dict(name="Gourmandise", ground=(102, 122, 58),  patch=(86, 104, 46),   road=(156, 124, 84),  rock=(124, 134, 92),  tree=(72, 60, 40),  accent=(160, 204, 64),  tuft=(132, 164, 62), liquid=1),
    dict(name="Avarice",     ground=(172, 140, 70),  patch=(152, 120, 56),  road=(214, 194, 136), rock=(192, 162, 92),  tree=(104, 72, 42), accent=(255, 214, 64),  tuft=(196, 166, 76), liquid=1),
    dict(name="Colere",      ground=(150, 62, 46),   patch=(128, 48, 36),   road=(194, 126, 94),  rock=(122, 62, 52),  tree=(70, 30, 24),  accent=(255, 128, 32),  tuft=(176, 82, 52),  liquid=2),
    dict(name="Heresie",     ground=(122, 82, 62),   patch=(104, 66, 50),   road=(184, 154, 124), rock=(154, 136, 124), tree=(62, 46, 38),  accent=(255, 156, 44),  tuft=(142, 94, 62),  liquid=1),
    dict(name="Violence",    ground=(112, 42, 42),   patch=(92, 30, 32),    road=(164, 114, 104), rock=(134, 84, 82),  tree=(62, 26, 26),  accent=(214, 26, 36),   tuft=(142, 52, 52),  liquid=2),
    dict(name="Fraude",      ground=(84, 62, 114),   patch=(68, 48, 96),    road=(154, 134, 174), rock=(114, 92, 144), tree=(52, 38, 72),  accent=(170, 96, 255),  tuft=(114, 82, 154), liquid=1),
    dict(name="Trahison",    ground=(168, 200, 226), patch=(146, 180, 212), road=(224, 236, 246), rock=(132, 172, 212), tree=(92, 112, 142), accent=(126, 206, 255), tuft=(204, 228, 246), liquid=2),
    dict(name="Lucifer",     ground=(64, 42, 46),    patch=(48, 30, 34),    road=(124, 94, 92),   rock=(84, 62, 66),   tree=(32, 22, 24),  accent=(255, 66, 32),   tuft=(94, 52, 52),   liquid=2),
]

def shade(c, k): return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (255,)
def rgba(c): return tuple(c[:3]) + (255,)

# ---------------- Sol ----------------
# Texture W x H = 12 x 25 unités du monde (x répété ; z de +16 en haut à -9 en bas). Route de z = -1.8 à +1.8.
GW, GH, SS_G = 1024, 1536, 2
ZTOP, ZSPAN = 16.0, 25.0
def zy(z): return (ZTOP - z) / ZSPAN * GH * SS_G

def ground(ci, p):
    rnd = random.Random(100 + ci)
    W, H = GW * SS_G, GH * SS_G
    im = Image.new("RGBA", (W, H), rgba(p["ground"]))
    d = ImageDraw.Draw(im)
    def wrap(fn):
        for off in (-W, 0, W): fn(off)
    # Grandes taches plus sombres (formes molles, sans contour)
    for _ in range(26):
        cx, cy = rnd.uniform(0, W), rnd.uniform(0, H); rx, ry = rnd.uniform(60, 190) * SS_G, rnd.uniform(30, 80) * SS_G
        pts = [(cx + math.cos(a) * rx * rnd.uniform(0.75, 1.15), cy + math.sin(a) * ry * rnd.uniform(0.75, 1.15)) for a in [k * math.tau / 14 for k in range(14)]]
        wrap(lambda o, pts=pts: d.polygon([(x + o, y) for x, y in pts], fill=rgba(p["patch"])))
    # Rivière (lave, sang, glace) ou flaques dans la bande du fond
    acc = rgba(p["accent"])
    if p["liquid"] == 2:
        y0 = zy(9.0); amp = 18 * SS_G
        top = [(x, y0 + math.sin(x / W * math.tau * 2) * amp) for x in range(0, W + 1, 16)]
        bot = [(x, y0 + 120 * SS_G + math.sin(x / W * math.tau * 3 + 1) * amp) for x in range(W, -1, -16)]
        d.polygon(top + bot, fill=acc)
        d.line(top, fill=INK, width=6 * SS_G); d.line(bot, fill=INK, width=6 * SS_G)
        # reflets
        for _ in range(14):
            x, y = rnd.uniform(0, W), y0 + rnd.uniform(30, 90) * SS_G
            wrap(lambda o, x=x, y=y: d.rounded_rectangle([x + o, y, x + o + rnd.uniform(40, 110) * SS_G, y + 7 * SS_G], 4 * SS_G, fill=shade(acc, 1.35)))
    else:
        for _ in range(4):
            cx, cy = rnd.uniform(0, W), zy(rnd.uniform(4.5, 11)); rx, ry = rnd.uniform(70, 130) * SS_G, rnd.uniform(26, 42) * SS_G
            def puddle(o, cx=cx, cy=cy, rx=rx, ry=ry):
                d.ellipse([cx - rx + o - 6 * SS_G, cy - ry - 6 * SS_G, cx + rx + o + 6 * SS_G, cy + ry + 6 * SS_G], fill=INK)
                d.ellipse([cx - rx + o, cy - ry, cx + rx + o, cy + ry], fill=shade(acc, 0.85))
                d.ellipse([cx - rx * 0.6 + o, cy - ry * 0.7, cx + rx * 0.2 + o, cy - ry * 0.2], fill=shade(acc, 1.25))
            wrap(puddle)
    # Route : bords ondulés, liseré noir, ombre intérieure, ornières et pierres
    ra, rb = zy(1.8), zy(-1.8)
    top = [(x, ra + math.sin(x / W * math.tau * 3) * 8 * SS_G + math.sin(x / W * math.tau * 7) * 4 * SS_G) for x in range(0, W + 1, 8)]
    bot = [(x, rb + math.sin(x / W * math.tau * 2 + 2) * 8 * SS_G + math.sin(x / W * math.tau * 5) * 4 * SS_G) for x in range(W, -1, -8)]
    road = rgba(p["road"]); rdark = shade(p["road"], 0.84)
    d.polygon(top + bot, fill=road)
    d.polygon(top + [(x, y + 22 * SS_G) for x, y in reversed(top)], fill=rdark)
    for k in (0.38, 0.62):
        y = ra + (rb - ra) * k
        d.line([(x, y + math.sin(x / W * math.tau * 4 + k * 9) * 5 * SS_G) for x in range(0, W + 1, 8)], fill=shade(p["road"], 0.9), width=10 * SS_G)
    d.line(top, fill=INK, width=7 * SS_G); d.line(bot, fill=INK, width=7 * SS_G)
    for _ in range(26):
        x, y = rnd.uniform(0, W), rnd.uniform(ra + 30 * SS_G, rb - 20 * SS_G); r = rnd.uniform(6, 13) * SS_G
        def stone(o, x=x, y=y, r=r):
            d.ellipse([x - r * 1.4 + o - 3 * SS_G, y - r - 3 * SS_G, x + r * 1.4 + o + 3 * SS_G, y + r + 3 * SS_G], fill=INK)
            d.ellipse([x - r * 1.4 + o, y - r, x + r * 1.4 + o, y + r], fill=shade(p["road"], 0.78))
        wrap(stone)
    # Touffes (petits « w » sombres) et cailloux sur les bas-côtés
    for _ in range(170):
        x, y = rnd.uniform(0, W), rnd.uniform(0, H)
        if ra - 20 * SS_G < y < rb + 20 * SS_G: continue
        s = rnd.uniform(9, 15) * SS_G
        def tuft(o, x=x, y=y, s=s):
            d.line([(x - s + o, y - s * 0.5), (x - s * 0.5 + o, y + s * 0.3), (x + o, y - s * 0.6), (x + s * 0.5 + o, y + s * 0.3), (x + s + o, y - s * 0.5)],
                   fill=shade(p["ground"], 0.62), width=4 * SS_G, joint="curve")
        wrap(tuft)
    for _ in range(60):
        x, y = rnd.uniform(0, W), rnd.uniform(0, H)
        if ra - 20 * SS_G < y < rb + 20 * SS_G: continue
        r = rnd.uniform(5, 10) * SS_G
        def peb(o, x=x, y=y, r=r):
            d.ellipse([x - r * 1.4 + o - 3 * SS_G, y - r - 3 * SS_G, x + r * 1.4 + o + 3 * SS_G, y + r + 3 * SS_G], fill=INK)
            d.ellipse([x - r * 1.4 + o, y - r, x + r * 1.4 + o, y + r], fill=rgba(p["rock"]))
        wrap(peb)
    im = im.resize((GW, GH), Image.LANCZOS)
    im.convert("RGB").save(os.path.join(OUT, "c%d_ground.png" % ci))

# ---------------- Éléments de décor (sprites, base en bas au centre) ----------------
S = 4
def canvas(w, h): return Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0))
def finish(im, name, w, h, outline=12):
    # Contour noir épais autour de toute la silhouette, puis réduction.
    a = im.getchannel("A")
    ol = a.filter(ImageFilter.MaxFilter(outline * S // 2 * 2 + 1))
    base = Image.new("RGBA", im.size, INK); base.putalpha(ol)
    base.alpha_composite(im)
    base.resize((w, h), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

def poly(d, pts, c): d.polygon([(x * S, y * S) for x, y in pts], fill=c)
def ell(d, b, c): d.ellipse([v * S for v in b], fill=c)
def line(d, pts, c, w): d.line([(x * S, y * S) for x, y in pts], fill=c, width=int(w * S), joint="curve")

def p_tree(ci, p, rnd):
    w, h = 256, 320; im = canvas(w, h); d = ImageDraw.Draw(im)
    t = rgba(p["tree"]); tl = shade(p["tree"], 1.35)
    poly(d, [(110, 312), (146, 312), (138, 200), (134, 150), (122, 150), (118, 200)], t)
    def branch(x0, y0, ang, ln, wd, depth):
        x1, y1 = x0 + math.cos(ang) * ln, y0 - math.sin(ang) * ln
        line(d, [(x0, y0), (x1, y1)], t, wd)
        if depth > 0:
            for da in (-0.5, 0.45):
                branch(x1, y1, ang + da + rnd.uniform(-0.15, 0.15), ln * 0.66, wd * 0.62, depth - 1)
    branch(128, 160, math.pi / 2 + 0.05, 70, 18, 3)
    branch(126, 210, math.pi * 0.82, 55, 12, 2)
    branch(132, 190, math.pi * 0.2, 60, 12, 2)
    line(d, [(124, 300), (122, 210)], tl, 5)
    finish(im, "c%d_tree" % ci, w // 2, h // 2)

def p_rock(ci, p, rnd):
    w, h = 220, 160; im = canvas(w, h); d = ImageDraw.Draw(im)
    c = rgba(p["rock"])
    pts = [(20, 152), (34, 92), (78, 48), (138, 40), (184, 74), (204, 152)]
    poly(d, pts, shade(c, 0.78))
    poly(d, [(34, 92), (78, 48), (138, 40), (184, 74), (150, 98), (88, 104)], c)
    poly(d, [(78, 48), (138, 40), (120, 62), (90, 66)], shade(c, 1.25))
    finish(im, "c%d_rock" % ci, w // 2, h // 2)

def p_skull(ci, p, rnd):
    w, h = 160, 150; im = canvas(w, h); d = ImageDraw.Draw(im)
    bone = (238, 226, 204, 255)
    ell(d, [24, 14, 136, 112], bone); poly(d, [(50, 96), (110, 96), (106, 140), (54, 140)], bone)
    ell(d, [42, 52, 74, 86], INK); ell(d, [86, 52, 118, 86], INK); poly(d, [(80, 92), (72, 108), (88, 108)], INK)
    for x in (62, 76, 90): line(d, [(x, 120), (x, 138)], INK, 4)
    poly(d, [(40, 30), (70, 18), (60, 40)], (255, 255, 240, 255))
    finish(im, "c%d_skull" % ci, w // 2, h // 2)

def p_bones(ci, p, rnd):
    w, h = 200, 110; im = canvas(w, h); d = ImageDraw.Draw(im)
    bone = (232, 220, 196, 255)
    for (a, b) in [((30, 90), (170, 40)), ((40, 40), (165, 95))]:
        line(d, [a, b], bone, 18)
        for q in (a, b): ell(d, [q[0] - 14, q[1] - 14, q[0] + 2, q[1] + 2], bone); ell(d, [q[0] - 2, q[1] - 2, q[0] + 14, q[1] + 14], bone)
    finish(im, "c%d_bones" % ci, w // 2, h // 2)

def p_tomb(ci, p, rnd):
    w, h = 170, 220; im = canvas(w, h); d = ImageDraw.Draw(im)
    c = rgba(p["rock"]); c2 = shade(c, 1.2)
    poly(d, [(28, 212), (142, 212), (142, 80), (28, 80)], c)
    ell(d, [28, 22, 142, 136], c)
    poly(d, [(40, 200), (60, 200), (60, 84), (40, 84)], c2)
    line(d, [(85, 60), (85, 150)], shade(c, 0.6), 12); line(d, [(58, 92), (112, 92)], shade(c, 0.6), 12)
    poly(d, [(14, 212), (156, 212), (148, 200), (22, 200)], shade(rgba(p["ground"]), 0.7))
    finish(im, "c%d_tomb" % ci, w // 2, h // 2)

def p_brazier(ci, p, rnd):
    w, h = 170, 260; im = canvas(w, h); d = ImageDraw.Draw(im)
    metal = (84, 78, 86, 255); acc = rgba(p["accent"])
    poly(d, [(70, 252), (100, 252), (92, 150), (78, 150)], metal)
    poly(d, [(40, 252), (130, 252), (118, 236), (52, 236)], metal)
    poly(d, [(26, 120), (144, 120), (122, 160), (48, 160)], (110, 102, 112, 255))
    # flamme
    poly(d, [(36, 124), (50, 70), (68, 92), (84, 20), (104, 84), (120, 60), (134, 124)], acc)
    poly(d, [(58, 124), (72, 88), (86, 56), (98, 92), (112, 124)], (255, 240, 170, 255))
    finish(im, "c%d_brazier" % ci, w // 2, h // 2)

def p_crystal(ci, p, rnd):
    w, h = 180, 220; im = canvas(w, h); d = ImageDraw.Draw(im)
    acc = rgba(p["accent"])
    for (x, top, wd) in [(60, 70, 30), (100, 20, 38), (136, 90, 26)]:
        poly(d, [(x - wd, 210), (x - wd * 0.7, top + 30), (x, top), (x + wd * 0.7, top + 30), (x + wd, 210)], acc)
        poly(d, [(x - wd * 0.7, top + 30), (x, top), (x, 210), (x - wd, 210)], shade(acc, 1.3))
    finish(im, "c%d_crystal" % ci, w // 2, h // 2)

def p_spikes(ci, p, rnd):
    w, h = 220, 180; im = canvas(w, h); d = ImageDraw.Draw(im)
    c = rgba(p["rock"])
    for (x, top, wd) in [(56, 60, 34), (112, 14, 44), (166, 80, 30)]:
        poly(d, [(x - wd, 172), (x, top), (x + wd, 172)], shade(c, 0.85))
        poly(d, [(x - wd, 172), (x, top), (x - wd * 0.2, 172)], shade(c, 1.15))
    finish(im, "c%d_spikes" % ci, w // 2, h // 2)

def p_bush(ci, p, rnd):
    w, h = 200, 130; im = canvas(w, h); d = ImageDraw.Draw(im)
    c = rgba(p["tuft"])
    for k in range(9):
        x = 30 + k * 18; top = 20 + (k % 3) * 18 + rnd.uniform(0, 14)
        poly(d, [(x - 14, 124), (x + rnd.uniform(-10, 10), top), (x + 14, 124)], c if k % 2 else shade(c, 1.2))
    finish(im, "c%d_bush" % ci, w // 2, h // 2)

PROPS = [p_tree, p_rock, p_skull, p_bones, p_tomb, p_brazier, p_crystal, p_spikes, p_bush]

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for ci, p in enumerate(P):
        ground(ci, p)
        rnd = random.Random(7 + ci)
        for fn in PROPS: fn(ci, p, rnd)
        print("cercle", ci, p["name"])
