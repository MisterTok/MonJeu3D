# Icônes des pièces façon Forge Master : dessins plats à gros contours noirs, une par emplacement et par cercle.
# Forme selon le niveau du cercle (0-2 simple, 3-5 renforcée, 6-8 ornée, 9 légendaire), accent à la couleur du cercle.
# Sortie : Assets/Resources/ForgeUI/Items/s<emplacement>_c<cercle>.png (256 px, fond transparent)
# Usage : python Tools/make_item_icons.py
import math, os
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Items")
N, S = 256, 4
INK = (28, 16, 16, 255)
CIRCLE = [(158, 153, 148), (230, 89, 166), (128, 199, 64), (242, 191, 56), (235, 56, 38), (255, 128, 20), (179, 20, 64), (158, 82, 242), (89, 204, 255), (255, 242, 191)]
# Métal par niveau : clair, moyen, sombre
METAL = [((176, 160, 140), (138, 120, 102), (98, 84, 70)),      # fer rouillé
         ((206, 212, 226), (150, 158, 176), (102, 108, 128)),   # acier
         ((132, 128, 150), (92, 88, 110), (60, 56, 76)),        # acier noirci
         ((255, 230, 130), (230, 170, 50), (170, 110, 30))]     # or
GOLD = ((255, 222, 110), (214, 150, 40))
WOOD = ((176, 110, 60), (126, 72, 36))
LEATHER = ((150, 92, 56), (108, 62, 34))

def tier(c): return 0 if c < 3 else 1 if c < 6 else 2 if c < 9 else 3
def rgba(c): return tuple(c[:3]) + (255,)
def shade(c, k): return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (255,)

class Pen:
    def __init__(self):
        self.im = Image.new("RGBA", (N * S, N * S), (0, 0, 0, 0)); self.d = ImageDraw.Draw(self.im)
    def P(self, pts): return [(x * S, y * S) for x, y in pts]
    def poly(self, pts, c, ow=4):
        self.d.polygon(self.P(pts), fill=rgba(c), outline=INK if ow else None, width=ow * S if ow else 1)
    def ell(self, b, c, ow=4):
        self.d.ellipse([v * S for v in b], fill=rgba(c), outline=INK if ow else None, width=ow * S if ow else 1)
    def rect(self, b, c, r=0, ow=4):
        self.d.rounded_rectangle([v * S for v in b], r * S, fill=rgba(c), outline=INK if ow else None, width=ow * S if ow else 1)
    def line(self, pts, c, w):
        self.d.line(self.P(pts), fill=rgba(c), width=int(w * S), joint="curve")
    def gem(self, cx, cy, r, c):
        self.poly([(cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)], c, 3)
        self.poly([(cx, cy - r), (cx + r * 0.45, cy - r * 0.1), (cx, cy + r * 0.2), (cx - r * 0.45, cy - r * 0.1)], shade(c, 1.35), 0)
    def save(self, name, rot=0):
        im = self.im
        if rot: im = im.rotate(rot, resample=Image.BICUBIC, center=(N * S / 2, N * S / 2))
        a = im.getchannel("A")
        ol = a.filter(ImageFilter.MaxFilter(6 * S + 1))
        base = Image.new("RGBA", im.size, INK); base.putalpha(ol)
        base.alpha_composite(im)
        base.resize((N, N), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

# ---------------- Lame ----------------
def sword(p, t, m, acc):
    L, M, D = m
    if t == 0:   # glaive court
        p.poly([(128, 26), (146, 52), (142, 160), (114, 160), (110, 52)], L)
        p.poly([(128, 26), (128, 160), (114, 160), (110, 52)], M, 0)
        p.rect([84, 156, 172, 172], WOOD[1], 4); p.rect([118, 170, 138, 214], LEATHER[0], 3); p.ell([112, 208, 144, 236], M)
    elif t == 1:  # épée longue
        p.poly([(128, 12), (148, 40), (146, 166), (110, 166), (108, 40)], L)
        p.poly([(128, 12), (128, 166), (110, 166), (108, 40)], M, 0); p.line([(128, 40), (128, 150)], D, 4)
        p.poly([(70, 160), (186, 160), (176, 180), (80, 180)], GOLD[1]); p.rect([118, 178, 138, 220], LEATHER[1], 3)
        p.gem(128, 170, 9, acc); p.ell([110, 214, 146, 244], GOLD[1])
    elif t == 2:  # lame dentelée
        pts = [(128, 8), (156, 36), (150, 60), (162, 80), (152, 100), (162, 122), (150, 168), (106, 168), (100, 120), (108, 96), (98, 70), (108, 40)]
        p.poly(pts, L); p.poly([(128, 8), (128, 168), (106, 168), (100, 120), (108, 96), (98, 70), (108, 40)], M, 0)
        p.line([(128, 30), (128, 150)], acc, 6)
        p.poly([(62, 156), (100, 164), (156, 164), (194, 156), (172, 186), (84, 186)], D)
        p.rect([118, 184, 138, 222], LEATHER[1], 3); p.gem(128, 236, 12, acc)
    else:         # épée de lumière
        p.poly([(128, 4), (156, 36), (154, 168), (102, 168), (100, 36)], (255, 250, 220))
        p.poly([(128, 4), (128, 168), (102, 168), (100, 36)], GOLD[0], 0)
        p.line([(128, 30), (128, 156)], acc, 8)
        p.poly([(52, 150), (100, 166), (156, 166), (204, 150), (184, 190), (72, 190)], GOLD[1]); p.gem(128, 176, 13, acc)
        p.rect([116, 188, 140, 226], (120, 40, 40), 3); p.gem(128, 238, 13, acc)

# ---------------- Heaume ----------------
def helmet(p, t, m, acc):
    L, M, D = m
    if t == 0:   # calotte à nasal
        p.ell([52, 54, 204, 206], M); p.rect([48, 140, 208, 168], D, 6)
        p.rect([118, 140, 138, 212], D, 4); p.ell([70, 70, 130, 110], L, 0)
    elif t == 1:  # grand heaume
        p.rect([58, 52, 198, 214], M, 40); p.ell([72, 62, 140, 110], L, 0)
        p.rect([74, 116, 182, 132], INK, 4, 0); p.rect([122, 132, 134, 196], INK, 2, 0)
        for y in (150, 168, 186): p.ell([96, y, 104, y + 8], D, 0); p.ell([152, y, 160, y + 8], D, 0)
    elif t == 2:  # heaume à cornes
        for s in (-1, 1):
            p.poly([(128 + s * 56, 106), (128 + s * 104, 46), (128 + s * 112, 20), (128 + s * 84, 66), (128 + s * 70, 130)], (236, 226, 204))
        p.rect([62, 64, 194, 218], M, 46); p.ell([78, 74, 140, 118], L, 0)
        p.poly([(80, 128), (176, 128), (160, 150), (96, 150)], INK, 0); p.gem(128, 96, 12, acc)
    else:         # heaume couronné à plumet
        p.poly([(122, 60), (134, 60), (172, 8), (150, 50), (190, 30), (146, 76)], acc)
        p.rect([62, 66, 194, 218], GOLD[1], 46); p.ell([78, 76, 140, 120], GOLD[0], 0)
        p.poly([(64, 92), (88, 56), (106, 84), (128, 48), (150, 84), (168, 56), (192, 92)], GOLD[0])
        p.poly([(80, 132), (176, 132), (160, 154), (96, 154)], INK, 0); p.gem(128, 80, 10, acc)

# ---------------- Bouclier ----------------
def shield(p, t, m, acc):
    L, M, D = m
    if t == 0:   # rond en bois
        p.ell([36, 36, 220, 220], D); p.ell([52, 52, 204, 204], WOOD[0])
        for x in (96, 128, 160): p.line([(x, 56), (x, 200)], WOOD[1], 4)
        p.ell([104, 104, 152, 152], M)
    elif t == 1:  # écu
        pts = [(46, 40), (210, 40), (206, 130), (128, 226), (50, 130)]
        p.poly(pts, M); p.poly([(64, 56), (192, 56), (190, 128), (128, 204), (66, 128)], acc)
        p.poly([(118, 56), (138, 56), (138, 196), (128, 204), (118, 196)], L, 0); p.poly([(66, 104), (190, 104), (190, 122), (66, 122)], L, 0)
    elif t == 2:  # pavois à pointes et crâne
        for a in range(8):
            ang = a * math.pi / 4; cx, cy = 128 + math.cos(ang) * 98, 128 + math.sin(ang) * 98
            p.poly([(cx + math.cos(ang) * 24, cy + math.sin(ang) * 24), (cx + math.cos(ang + 1.6) * 12, cy + math.sin(ang + 1.6) * 12), (cx + math.cos(ang - 1.6) * 12, cy + math.sin(ang - 1.6) * 12)], L)
        p.ell([34, 34, 222, 222], D); p.ell([54, 54, 202, 202], M)
        p.ell([92, 80, 164, 150], (236, 226, 204)); p.rect([106, 136, 150, 168], (236, 226, 204), 6)
        p.ell([102, 102, 124, 124], acc, 0); p.ell([132, 102, 154, 124], acc, 0)
    else:         # bouclier d'or
        pts = [(40, 34), (216, 34), (212, 132), (128, 232), (44, 132)]
        p.poly(pts, GOLD[1]); p.poly([(58, 50), (198, 50), (196, 128), (128, 210), (60, 128)], (120, 30, 40))
        p.poly([(128, 64), (150, 110), (198, 112), (160, 140), (172, 188), (128, 160), (84, 188), (96, 140), (58, 112), (106, 110)], GOLD[0])
        p.gem(128, 126, 16, acc)

# ---------------- Gantelets ----------------
def gauntlet(p, t, m, acc):
    L, M, D = m
    cuff = LEATHER[0] if t == 0 else D
    p.rect([70, 150, 186, 230], cuff, 14)
    p.rect([62, 70, 194, 168], M, 34)
    for k, x in enumerate((70, 102, 134, 166)):
        p.rect([x, 34 + (k in (1, 2)) * -10 + 14, x + 30, 110], L if t != 3 else GOLD[0], 14)
    p.rect([34, 92, 82, 150], L if t != 3 else GOLD[0], 20)
    if t >= 1:
        for x in (82, 114, 146): p.line([(x + 6, 120), (x + 26, 120)], D, 6)
    if t >= 2:
        for x in (84, 116, 148, 176): p.poly([(x - 8, 70), (x, 40), (x + 8, 70)], (230, 230, 240))
    p.gem(128, 190, 14 if t >= 1 else 0.1, acc) if t >= 1 else p.rect([90, 180, 166, 196], LEATHER[1], 4, 3)

# ---------------- Bottes ----------------
def boots(p, t, m, acc):
    L, M, D = m
    col = LEATHER[0] if t == 0 else M if t < 3 else GOLD[1]
    p.poly([(84, 30), (162, 30), (164, 160), (222, 176), (226, 222), (64, 222), (78, 150)], col)
    p.rect([62, 210, 228, 232], D if t else LEATHER[1], 6)
    p.rect([76, 22, 170, 52], L if t else LEATHER[1], 8)
    if t >= 1: p.poly([(84, 92), (162, 92), (164, 112), (80, 112)], L)
    if t >= 2:
        p.poly([(164, 168), (196, 140), (184, 176)], (230, 230, 240)); p.gem(124, 70, 12, acc)
    if t == 3: p.poly([(86, 52), (60, 20), (100, 40)], acc); p.poly([(160, 52), (190, 18), (150, 40)], acc)

# ---------------- Ceinture ----------------
def belt(p, t, m, acc):
    L, M, D = m
    strap = LEATHER[0] if t < 2 else (90, 40, 40) if t == 2 else (120, 30, 40)
    p.poly([(14, 98), (242, 98), (242, 158), (14, 158)], strap)
    p.line([(14, 108), (242, 108)], shade(strap, 1.25), 4)
    for x in (34, 62, 194, 222): p.ell([x - 6, 122, x + 6, 134], D, 0)
    buckle = M if t < 3 else GOLD[1]
    if t == 0:
        p.rect([92, 82, 164, 174], buckle, 10); p.rect([108, 98, 148, 158], strap, 4); p.line([(128, 98), (128, 158)], L, 6)
    else:
        p.ell([78, 68, 178, 188], buckle); p.ell([96, 86, 160, 170], shade(buckle, 1.25), 0)
        p.gem(128, 128, 22 if t >= 2 else 16, acc)
        if t >= 2:
            for a in range(6):
                ang = a * math.pi / 3; p.ell([128 + math.cos(ang) * 40 - 5, 128 + math.sin(ang) * 48 - 5, 128 + math.cos(ang) * 40 + 5, 128 + math.sin(ang) * 48 + 5], GOLD[0], 2)

# ---------------- Amulette ----------------
def amulet(p, t, m, acc):
    L, M, D = m
    chain = GOLD[1] if t >= 2 else M
    for k in range(13):
        a = math.pi * 1.08 + k * (math.pi * 0.84 / 12)
        cx, cy = 128 + math.cos(a) * 86, 30 - math.sin(a) * 120
        p.ell([cx - 8, cy - 8, cx + 8, cy + 8], chain, 3)
    if t == 0:
        p.poly([(128, 120), (166, 160), (128, 228), (90, 160)], M); p.gem(128, 168, 18, acc)
    elif t == 1:
        p.ell([84, 124, 172, 212], M); p.gem(128, 168, 26, acc)
    elif t == 2:
        p.poly([(128, 112), (182, 150), (166, 214), (90, 214), (74, 150)], GOLD[1]); p.gem(128, 166, 30, acc)
    else:
        p.poly([(128, 104), (148, 140), (196, 146), (160, 178), (170, 226), (128, 202), (86, 226), (96, 178), (60, 146), (108, 140)], GOLD[0])
        p.gem(128, 166, 26, acc)

# ---------------- Anneau ----------------
def ring(p, t, m, acc):
    L, M, D = m
    band = M if t < 2 else GOLD[1] if t == 2 else GOLD[0]
    p.ell([50, 92, 206, 236], band); p.ell([78, 120, 178, 210], (0, 0, 0, 0), 4)
    p.d.ellipse([84 * S, 126 * S, 172 * S, 204 * S], fill=(0, 0, 0, 0))
    p.ell([66, 104, 110, 150], shade(band, 1.3), 0)
    if t == 0:
        p.rect([104, 70, 152, 108], D, 8); p.gem(128, 88, 14, acc)
    else:
        p.poly([(92, 98), (164, 98), (150, 64), (106, 64)], band)
        p.gem(128, 64, 26 if t < 3 else 32, acc)
        if t >= 2:
            for x in (88, 168): p.gem(x, 96, 9, acc)

SLOTS = [(sword, -45), (helmet, 0), (shield, 0), (gauntlet, 0), (boots, 0), (belt, 0), (amulet, 0), (ring, 0)]

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for s, (fn, rot) in enumerate(SLOTS):
        for c in range(10):
            p = Pen()
            fn(p, tier(c), METAL[tier(c)], CIRCLE[c])
            p.save("s%d_c%d" % (s, c), rot)
    print("ok")
