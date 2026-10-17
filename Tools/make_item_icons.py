# Icônes des pièces façon Forge Master : dessins plats à gros contours noirs.
# Comme les âges de FM, CHAQUE CERCLE a ses propres modèles, tirés d'un grand catalogue (18 armes, 12 casques,
# 7 boucliers, 6 gantelets, 6 bottes, 7 ceintures, 7 amulettes, 6 anneaux), avec ses matériaux et son motif
# (cordes et os, cœurs, bave, pièces d'or, flammes, feu violet, épines, yeux, glace, ailes).
# Sortie : Assets/Resources/ForgeUI/Items/s<emplacement>_c<cercle>_v<modèle>.png (256 px, fond transparent)
#          Assets/Scripts/Forge/ItemModels.cs (noms des modèles par cercle, généré)
# Les armes sont dessinées verticales, poignée en (128, 178-220), puis tournées de 45° : prise commune pour PaperHero.
# Usage : python Tools/make_item_icons.py
import math, os, glob
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.join(os.path.dirname(__file__), "..")
OUT = os.path.join(ROOT, "Assets", "Resources", "ForgeUI", "Items")
CS_OUT = os.path.join(ROOT, "Assets", "Scripts", "Forge", "ItemModels.cs")
N, S = 256, 4
INK = (28, 16, 16, 255)
BONE = (236, 226, 204)
GOLD = ((255, 222, 110), (214, 150, 40))

# ---------------- Matériaux et motif de chaque cercle ----------------
MAT = [
    dict(name="Limbes",      L=(204, 198, 190), M=(150, 146, 140), D=(100, 96, 92),  trim=BONE,            grip=(150, 100, 60), cloth=(120, 96, 70),  acc=(170, 196, 230), motif="rope"),
    dict(name="Luxure",      L=(255, 216, 220), M=(232, 150, 172), D=(170, 80, 112), trim=GOLD[0],         grip=(150, 40, 80),  cloth=(206, 60, 124), acc=(255, 90, 170),  motif="heart"),
    dict(name="Gourmandise", L=(216, 182, 112), M=(170, 130, 70),  D=(116, 84, 46),  trim=(130, 172, 70),  grip=(110, 70, 40),  cloth=(120, 140, 60), acc=(150, 222, 60),  motif="drip"),
    dict(name="Avarice",     L=(255, 238, 150), M=(240, 180, 60),  D=(180, 120, 30), trim=(255, 250, 225), grip=(124, 30, 44),  cloth=(140, 30, 50),  acc=(230, 40, 70),   motif="coin"),
    dict(name="Colere",      L=(112, 102, 118), M=(72, 64, 78),    D=(42, 36, 46),   trim=(255, 132, 40),  grip=(64, 32, 30),   cloth=(150, 40, 30),  acc=(255, 120, 30),  motif="flame"),
    dict(name="Heresie",     L=(198, 166, 122), M=(142, 110, 76),  D=(88, 66, 46),   trim=(172, 92, 232),  grip=(72, 52, 62),   cloth=(92, 40, 112),  acc=(200, 120, 255), motif="hellfire"),
    dict(name="Violence",    L=(232, 172, 172), M=(182, 82, 92),   D=(112, 32, 46),  trim=(62, 42, 42),    grip=(72, 30, 30),   cloth=(142, 20, 40),  acc=(222, 22, 42),   motif="thorn"),
    dict(name="Fraude",      L=(202, 182, 242), M=(132, 102, 192), D=(78, 58, 122),  trim=(82, 222, 142),  grip=(52, 42, 72),   cloth=(200, 60, 200), acc=(180, 100, 255), motif="eye"),
    dict(name="Trahison",    L=(236, 250, 255), M=(160, 216, 246), D=(92, 152, 206), trim=(255, 255, 255), grip=(72, 92, 132),  cloth=(120, 170, 222), acc=(90, 212, 255), motif="ice"),
    dict(name="Lucifer",     L=(112, 98, 112),  M=(62, 52, 64),    D=(32, 26, 34),   trim=(255, 214, 92),  grip=(42, 32, 42),   cloth=(172, 20, 30),  acc=(255, 64, 42),   motif="wing"),
]

def rgba(c): return tuple(c[:3]) + (255,)
def shade(c, k): return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (255,)

class Pen:
    def __init__(self, m):
        self.m = m
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
    def arc(self, b, a0, a1, c, w):
        self.d.arc([v * S for v in b], a0, a1, fill=rgba(c), width=int(w * S))
    def hole(self, b): self.d.ellipse([v * S for v in b], fill=(0, 0, 0, 0))
    def gem(self, cx, cy, r, c=None):
        c = c or self.m["acc"]
        self.poly([(cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)], c, 3)
        self.poly([(cx, cy - r), (cx + r * 0.45, cy - r * 0.1), (cx, cy + r * 0.2), (cx - r * 0.45, cy - r * 0.1)], shade(c, 1.35), 0)
    def spike(self, x, y, ang, ln, w, c):
        a = math.radians(ang)
        tip = (x + math.cos(a) * ln, y - math.sin(a) * ln)
        b1 = (x + math.cos(a + 1.57) * w, y - math.sin(a + 1.57) * w); b2 = (x + math.cos(a - 1.57) * w, y - math.sin(a - 1.57) * w)
        self.poly([b1, tip, b2], c, 3)
    def flame(self, x, y, r, outer, inner):
        def shp(k):
            return [(x - r * k, y), (x - r * 0.8 * k, y - r * 0.9 * k), (x - r * 0.25 * k, y - r * 1.2 * k), (x, y - r * 2 * k),
                    (x + r * 0.35 * k, y - r * 1.3 * k), (x + r * 0.8 * k, y - r * 1.5 * k), (x + r * 0.9 * k, y - r * 0.7 * k), (x + r * k, y)]
        self.poly(shp(1), outer, 3); self.poly(shp(0.55), inner, 0)
    # Motif du cercle, posé aux points d'accroche que chaque dessin propose.
    def deco(self, x, y, r=12):
        m = self.m; mo = m["motif"]
        if mo == "rope":
            for dy in (-r * 0.45, r * 0.45): self.rect([x - r * 1.3, y + dy - r * 0.3, x + r * 1.3, y + dy + r * 0.3], (176, 128, 72), 3, 3)
        elif mo == "heart":
            self.poly([(x, y + r), (x - r, y), (x - r * 0.9, y - r * 0.7), (x - r * 0.4, y - r * 0.9), (x, y - r * 0.5),
                       (x + r * 0.4, y - r * 0.9), (x + r * 0.9, y - r * 0.7), (x + r, y)], m["acc"], 3)
        elif mo == "drip":
            self.ell([x - r, y - r * 0.8, x + r, y + r * 0.6], m["acc"], 3)
            self.poly([(x - r * 0.35, y + r * 0.3), (x + r * 0.35, y + r * 0.3), (x, y + r * 1.6)], m["acc"], 3)
            self.ell([x - r * 0.5, y - r * 0.5, x - r * 0.05, y - r * 0.1], shade(m["acc"], 1.3), 0)
        elif mo == "coin":
            for k, dx in enumerate((-r * 0.6, r * 0.6)):
                self.ell([x + dx - r * 0.8, y - r * 0.8 + k * r * 0.4, x + dx + r * 0.8, y + r * 0.8 + k * r * 0.4], GOLD[0], 3)
                self.ell([x + dx - r * 0.4, y - r * 0.4 + k * r * 0.4, x + dx + r * 0.4, y + r * 0.4 + k * r * 0.4], GOLD[1], 0)
        elif mo == "flame":
            self.flame(x, y + r * 0.6, r, (255, 110, 30), (255, 230, 120))
        elif mo == "hellfire":
            self.flame(x, y + r * 0.6, r, (170, 80, 230), (240, 190, 255))
        elif mo == "thorn":
            for a in (60, 120, 0, 180): self.spike(x, y, a, r * 1.2, r * 0.35, (90, 30, 30))
            self.ell([x - r * 0.45, y - r * 0.45, x + r * 0.45, y + r * 0.45], m["acc"], 3)
        elif mo == "eye":
            self.poly([(x - r * 1.3, y), (x - r * 0.5, y - r * 0.7), (x + r * 0.5, y - r * 0.7), (x + r * 1.3, y), (x + r * 0.5, y + r * 0.7), (x - r * 0.5, y + r * 0.7)], (255, 255, 240), 3)
            self.ell([x - r * 0.5, y - r * 0.5, x + r * 0.5, y + r * 0.5], m["acc"], 0); self.ell([x - r * 0.2, y - r * 0.2, x + r * 0.2, y + r * 0.2], INK, 0)
        elif mo == "ice":
            for a, ln in ((90, 1.8), (55, 1.2), (125, 1.2)): self.spike(x, y + r * 0.4, a, r * ln, r * 0.35, (200, 240, 255))
        elif mo == "wing":
            for s in (-1, 1):
                self.poly([(x, y), (x + s * r * 1.8, y - r * 1.0), (x + s * r * 1.6, y - r * 0.2), (x + s * r * 1.9, y + r * 0.3), (x + s * r * 1.2, y + r * 0.4), (x + s * r * 0.9, y + r * 0.9)], m["D"], 3)
            self.gem(x, y, r * 0.5, m["trim"])
    def save(self, name, rot=0):
        im = self.im
        if rot: im = im.rotate(rot, resample=Image.BICUBIC, center=(N * S / 2, N * S / 2))
        a = im.getchannel("A")
        ol = a.filter(ImageFilter.MaxFilter(6 * S + 1))
        base = Image.new("RGBA", im.size, INK); base.putalpha(ol)
        base.alpha_composite(im)
        base.resize((N, N), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

# =============== ARMES (verticales, poignée 178-222) ===============
def hilt(p, guard="bar"):
    m = p.m
    if guard == "bar": p.poly([(76, 160), (180, 160), (172, 178), (84, 178)], m["trim"])
    elif guard == "cross": p.rect([96, 158, 160, 178], m["trim"], 8); p.gem(128, 168, 9)
    elif guard == "wing":
        for s in (-1, 1): p.poly([(128, 164), (128 + s * 60, 140), (128 + s * 52, 162), (128 + s * 34, 178)], m["trim"])
    p.rect([118, 176, 138, 222], m["grip"], 3)
    p.ell([112, 214, 144, 244], m["trim"])

def handle(p, top, bottom=228, w=8):
    m = p.m
    p.rect([128 - w, top, 128 + w, bottom], m["grip"], 4)
    p.rect([128 - w - 4, 178, 128 + w + 4, 186], m["D"], 2, 3); p.rect([128 - w - 4, 212, 128 + w + 4, 220], m["D"], 2, 3)
    p.ell([114, bottom - 8, 142, bottom + 18], m["trim"])

def w_sword(p):
    m = p.m
    p.poly([(128, 10), (150, 38), (148, 164), (108, 164), (106, 38)], m["L"]); p.poly([(128, 10), (128, 164), (108, 164), (106, 38)], m["M"], 0)
    p.line([(128, 40), (128, 150)], m["D"], 5); hilt(p, "bar"); p.deco(128, 168, 10)

def w_greatsword(p):
    m = p.m
    p.poly([(128, 4), (168, 34), (164, 160), (92, 160), (88, 34)], m["L"]); p.poly([(128, 4), (128, 160), (92, 160), (88, 34)], m["M"], 0)
    p.poly([(116, 40), (140, 40), (138, 140), (118, 140)], m["D"], 0); p.deco(128, 90, 14); hilt(p, "wing")

def w_dagger(p):
    m = p.m
    p.poly([(128, 70), (146, 96), (142, 162), (114, 162), (110, 96)], m["L"]); p.poly([(128, 70), (128, 162), (114, 162), (110, 96)], m["M"], 0)
    for s in (-1, 1): p.poly([(128, 160), (128 + s * 56, 138), (128 + s * 60, 156), (128 + s * 20, 180)], m["trim"])
    p.rect([118, 176, 138, 222], m["grip"], 3); p.deco(128, 234, 13)

def w_rapier(p):
    m = p.m
    p.poly([(128, 4), (134, 14), (134, 160), (122, 160), (122, 14)], m["L"])
    p.arc([86, 130, 170, 214], 180, 360, m["trim"], 9); p.ell([92, 150, 164, 186], m["trim"]); p.hole([108, 158, 148, 180])
    p.rect([120, 176, 136, 222], m["grip"], 3); p.ell([112, 214, 144, 244], m["trim"]); p.deco(128, 168, 9)

def w_katana(p):
    m = p.m
    p.poly([(140, 8), (152, 20), (140, 100), (138, 164), (118, 164), (122, 96)], m["L"]); p.poly([(140, 8), (122, 96), (118, 164), (128, 164), (132, 96)], m["M"], 0)
    p.ell([100, 158, 156, 182], m["trim"]); p.rect([120, 178, 136, 228], m["grip"], 3)
    for y in (186, 200, 214): p.poly([(120, y), (136, y + 8), (120, y + 8)], m["D"], 0)
    p.deco(128, 236, 11)

def w_axe(p):
    m = p.m
    handle(p, 30)
    p.poly([(132, 40), (196, 22), (214, 70), (200, 118), (132, 100)], m["M"]); p.poly([(196, 22), (214, 70), (200, 118), (186, 70)], m["L"], 0)
    p.poly([(124, 46), (80, 36), (72, 70), (124, 90)], m["D"]); p.deco(128, 70, 12)

def w_cleaver(p):
    m = p.m
    p.poly([(92, 20), (184, 20), (184, 132), (112, 150), (92, 132)], m["L"]); p.poly([(92, 20), (184, 20), (184, 40), (92, 40)], m["M"], 0)
    p.ell([152, 30, 172, 50], (0, 0, 0, 0), 4); p.hole([156, 34, 168, 46])
    p.rect([118, 146, 138, 226], m["grip"], 3); p.ell([112, 214, 144, 244], m["trim"]); p.deco(132, 96, 15)

def w_club(p):
    m = p.m
    p.poly([(116, 228), (140, 228), (170, 70), (160, 24), (128, 12), (96, 24), (86, 70)], m["grip"] if m["motif"] != "rope" else (176, 120, 70))
    p.poly([(96, 24), (128, 12), (128, 228), (116, 228), (86, 70)], shade(m["grip"] if m["motif"] != "rope" else (176, 120, 70), 1.2), 0)
    for (x, y, a) in ((166, 50, 0), (90, 60, 180), (150, 100, 10), (104, 120, 170), (128, 16, 90)): p.spike(x, y, a, 18, 7, m["trim"])
    p.deco(128, 190, 12)

def w_mace(p):
    m = p.m
    handle(p, 96)
    p.ell([84, 22, 172, 110], m["M"]); p.ell([96, 32, 136, 66], m["L"], 0)
    for k in range(6):
        a = 90 + k * 60
        p.spike(128 + math.cos(math.radians(a)) * 44, 66 - math.sin(math.radians(a)) * 44, a, 18, 8, m["L"])
    p.deco(128, 66, 13)

def w_warhammer(p):
    m = p.m
    handle(p, 40)
    p.rect([70, 26, 186, 90], m["M"], 10); p.rect([70, 26, 186, 44], m["L"], 8, 0)
    p.rect([60, 34, 76, 82], m["D"], 4); p.rect([180, 34, 196, 82], m["D"], 4)
    p.deco(128, 60, 14)

def w_flail(p):
    m = p.m
    p.rect([120, 120, 136, 228], m["grip"], 4); p.ell([114, 220, 142, 246], m["trim"])
    for k in range(5): p.ell([116 + k * 2, 100 - k * 16, 140 + k * 2, 118 - k * 16], m["D"], 3)
    p.ell([116, 8, 186, 78], m["M"]); p.ell([128, 18, 154, 40], m["L"], 0)
    for a in (0, 60, 120, 180, 240, 300): p.spike(151 + math.cos(math.radians(a)) * 35, 43 - math.sin(math.radians(a)) * 35, a, 16, 7, m["L"])
    p.deco(151, 43, 11)

def w_spear(p):
    m = p.m
    handle(p, 84, 232)
    p.poly([(128, 6), (154, 56), (138, 92), (118, 92), (102, 56)], m["L"]); p.poly([(128, 6), (128, 92), (118, 92), (102, 56)], m["M"], 0)
    p.rect([112, 88, 144, 104], m["trim"], 4); p.deco(128, 120, 11)

def w_halberd(p):
    m = p.m
    handle(p, 40, 232)
    p.poly([(128, 4), (142, 34), (128, 50), (114, 34)], m["L"])
    p.poly([(136, 44), (196, 30), (206, 80), (190, 112), (136, 96)], m["M"]); p.poly([(196, 30), (206, 80), (190, 112), (180, 72)], m["L"], 0)
    p.poly([(120, 56), (80, 70), (120, 84)], m["D"]); p.deco(128, 70, 11)

def w_scythe(p):
    m = p.m
    handle(p, 26, 232)
    p.poly([(128, 30), (70, 20), (28, 48), (14, 92), (40, 64), (82, 50), (128, 60)], m["M"]); p.poly([(70, 20), (28, 48), (14, 92), (40, 58)], m["L"], 0)
    p.rect([116, 22, 140, 66], m["D"], 4); p.deco(128, 44, 11)

def w_trident(p):
    m = p.m
    handle(p, 96, 232)
    p.rect([84, 84, 172, 104], m["M"], 8)
    for x, top in ((92, 30), (128, 10), (164, 30)):
        p.poly([(x - 9, 92), (x - 9, top + 22), (x, top), (x + 9, top + 22), (x + 9, 92)], m["L"])
    p.deco(128, 94, 10)

def w_bow(p):
    m = p.m
    p.line([(128, 26), (128, 232)], (240, 230, 210), 3)
    p.poly([(128, 20), (96, 50), (80, 110), (88, 150), (120, 190), (128, 196), (110, 160), (98, 110), (108, 60), (134, 30)], m["M"])
    p.poly([(128, 236), (100, 214), (92, 190), (120, 196), (138, 226)], m["M"])
    p.rect([116, 176, 136, 222], m["grip"], 4)
    p.deco(96, 110, 12)

def w_staff(p):
    m = p.m
    handle(p, 70, 232, 7)
    for s in (-1, 1): p.poly([(128, 76), (128 + s * 34, 50), (128 + s * 30, 20), (128 + s * 12, 40)], m["trim"])
    p.ell([100, 14, 156, 70], m["acc"]); p.ell([110, 22, 128, 40], shade(m["acc"], 1.4), 0)
    p.deco(128, 110, 11)

def w_scepter(p):
    m = p.m
    p.rect([120, 90, 136, 226], m["trim"], 4); p.ell([112, 214, 144, 244], m["M"])
    p.poly([(128, 8), (166, 46), (154, 92), (102, 92), (90, 46)], m["M"]); p.poly([(128, 8), (128, 92), (102, 92), (90, 46)], m["L"], 0)
    p.gem(128, 52, 18); p.rect([108, 88, 148, 102], m["D"], 4); p.deco(128, 150, 11)

WEAPON_FAMILIES = {
    "Épée": w_sword, "Espadon": w_greatsword, "Dague": w_dagger, "Rapière": w_rapier, "Lame courbe": w_katana,
    "Hache": w_axe, "Couperet": w_cleaver, "Gourdin": w_club, "Masse": w_mace, "Marteau de guerre": w_warhammer,
    "Fléau": w_flail, "Lance": w_spear, "Hallebarde": w_halberd, "Faux": w_scythe, "Trident": w_trident,
    "Arc": w_bow, "Bâton": w_staff, "Sceptre": w_scepter,
}

# =============== HEAUMES ===============
def h_cap(p):
    m = p.m
    p.ell([52, 54, 204, 206], m["M"]); p.rect([48, 140, 208, 168], m["D"], 6)
    p.rect([118, 140, 138, 212], m["D"], 4); p.ell([70, 70, 130, 110], m["L"], 0); p.deco(128, 96, 13)

def h_great(p):
    m = p.m
    p.rect([58, 52, 198, 214], m["M"], 40); p.ell([72, 62, 140, 110], m["L"], 0)
    p.rect([74, 116, 182, 132], INK, 4, 0); p.rect([122, 132, 134, 196], INK, 2, 0)
    for y in (150, 168, 186): p.ell([96, y, 104, y + 8], m["D"], 0); p.ell([152, y, 160, y + 8], m["D"], 0)
    p.deco(128, 40, 14)

def h_barbute(p):
    m = p.m
    p.poly([(60, 210), (62, 100), (90, 50), (166, 50), (194, 100), (196, 210)], m["M"]); p.poly([(90, 50), (128, 50), (100, 210), (60, 210), (62, 100)], m["L"], 0)
    p.poly([(112, 100), (144, 100), (144, 120), (136, 120), (136, 200), (120, 200), (120, 120), (112, 120)], INK, 0)
    p.rect([86, 46, 170, 60], m["trim"], 6); p.deco(128, 36, 12)

def h_horned(p):
    m = p.m
    for s in (-1, 1):
        p.poly([(128 + s * 56, 106), (128 + s * 104, 46), (128 + s * 112, 20), (128 + s * 84, 66), (128 + s * 70, 130)], m["trim"])
    p.rect([62, 64, 194, 218], m["M"], 46); p.ell([78, 74, 140, 118], m["L"], 0)
    p.poly([(80, 128), (176, 128), (160, 150), (96, 150)], INK, 0); p.deco(128, 96, 12)

def h_hood(p):
    m = p.m; cloth = m["cloth"]
    p.poly([(128, 20), (196, 84), (210, 226), (46, 226), (60, 84)], cloth); p.poly([(128, 20), (150, 40), (110, 120), (60, 84)], shade(cloth, 1.2), 0)
    p.ell([78, 96, 178, 206], (20, 12, 16)); p.ell([96, 132, 120, 150], m["acc"], 0); p.ell([136, 132, 160, 150], m["acc"], 0)
    p.deco(128, 218, 12)

def h_crown(p):
    m = p.m
    p.rect([62, 66, 194, 218], m["M"], 46); p.ell([78, 76, 140, 120], m["L"], 0)
    p.poly([(60, 96), (84, 52), (106, 84), (128, 40), (150, 84), (172, 52), (196, 96)], m["trim"])
    p.poly([(80, 132), (176, 132), (160, 154), (96, 154)], INK, 0); p.gem(128, 76, 10)
    for x in (84, 172): p.ell([x - 7, 46, x + 7, 60], m["acc"], 3)

def h_skull(p):
    m = p.m
    p.ell([50, 30, 206, 180], BONE if m["motif"] != "wing" else m["M"]); p.rect([84, 150, 172, 214], BONE if m["motif"] != "wing" else m["M"], 14)
    p.ell([76, 86, 120, 130], INK, 0); p.ell([136, 86, 180, 130], INK, 0)
    p.ell([90, 100, 106, 116], m["acc"], 0); p.ell([150, 100, 166, 116], m["acc"], 0)
    p.poly([(128, 136), (118, 156), (138, 156)], INK, 0)
    for x in (104, 120, 136, 152): p.line([(x, 176), (x, 206)], INK, 4)
    p.deco(128, 34, 13)

def h_mitre(p):
    m = p.m
    p.poly([(70, 214), (64, 90), (128, 10), (192, 90), (186, 214)], m["cloth"]); p.poly([(64, 90), (128, 10), (128, 214), (70, 214)], shade(m["cloth"], 1.2), 0)
    p.rect([62, 186, 194, 218], m["trim"], 6); p.rect([118, 40, 138, 190], m["trim"], 3)
    p.rect([96, 90, 160, 108], m["trim"], 3); p.deco(128, 150, 14)

def h_jester(p):
    m = p.m
    p.rect([62, 120, 194, 210], m["M"], 30)
    for s, c in ((-1, m["cloth"]), (1, m["D"])):
        p.poly([(128, 130), (128 + s * 20, 60), (128 + s * 90, 24), (128 + s * 70, 80), (128 + s * 60, 140)], c)
        p.ell([128 + s * 90 - 12, 12, 128 + s * 90 + 12, 36], m["trim"], 3)
    p.poly([(128, 130), (110, 40), (128, 6), (146, 40)], m["acc"]); p.ell([116, 0, 140, 20], m["trim"], 3)
    p.ell([90, 140, 166, 200], (250, 240, 230)); p.deco(128, 170, 12)

def h_winged(p):
    m = p.m
    for s in (-1, 1):
        p.poly([(128 + s * 60, 120), (128 + s * 120, 60), (128 + s * 110, 100), (128 + s * 124, 112), (128 + s * 100, 136), (128 + s * 72, 160)], m["trim"])
    p.rect([62, 64, 194, 218], m["M"], 46); p.ell([78, 74, 140, 118], m["L"], 0)
    p.poly([(80, 132), (176, 132), (160, 154), (96, 154)], INK, 0); p.deco(128, 96, 12)

def h_plumed(p):
    m = p.m
    p.poly([(124, 70), (110, 30), (130, 6), (170, 4), (200, 30), (176, 30), (150, 70)], m["acc"])
    p.rect([62, 62, 194, 218], m["M"], 40); p.ell([76, 70, 140, 114], m["L"], 0)
    for y in (120, 140, 160): p.rect([80, y, 176, y + 8], INK, 3, 0)
    p.deco(128, 196, 11)

def h_mask(p):
    m = p.m
    p.poly([(56, 40), (200, 40), (196, 130), (170, 200), (128, 226), (86, 200), (60, 130)], (248, 240, 228) if m["motif"] != "wing" else m["M"])
    p.poly([(56, 40), (128, 40), (128, 226), (86, 200), (60, 130)], m["cloth"], 0)
    p.poly([(78, 96), (118, 92), (110, 122), (82, 118)], INK, 0); p.poly([(138, 92), (178, 96), (174, 118), (146, 122)], INK, 0)
    p.line([(96, 170), (128, 184), (160, 170)], INK, 5); p.deco(128, 50, 13)

HELMET_FAMILIES = {"Casque": h_cap, "Heaume": h_great, "Barbute": h_barbute, "Heaume cornu": h_horned, "Capuche": h_hood,
                   "Couronne": h_crown, "Crâne": h_skull, "Mitre": h_mitre, "Bonnet de bouffon": h_jester,
                   "Heaume ailé": h_winged, "Heaume à plumet": h_plumed, "Masque": h_mask}

# =============== BOUCLIERS ===============
def s_round(p):
    m = p.m
    p.ell([36, 36, 220, 220], m["D"]); p.ell([52, 52, 204, 204], m["M"]); p.ell([70, 70, 186, 186], m["L"], 0)
    p.ell([100, 100, 156, 156], m["trim"]); p.deco(128, 128, 14)

def s_kite(p):
    m = p.m
    p.poly([(46, 40), (210, 40), (206, 130), (128, 226), (50, 130)], m["M"])
    p.poly([(64, 56), (192, 56), (190, 128), (128, 204), (66, 128)], m["cloth"])
    p.poly([(118, 56), (138, 56), (138, 196), (128, 204), (118, 196)], m["trim"], 0); p.poly([(66, 104), (190, 104), (190, 122), (66, 122)], m["trim"], 0)
    p.deco(128, 113, 15)

def s_tower(p):
    m = p.m
    p.rect([52, 26, 204, 230], m["D"], 20); p.rect([66, 40, 190, 216], m["M"], 14)
    for y in (70, 186): p.rect([66, y - 6, 190, y + 6], m["D"], 2, 0)
    for x, y in ((82, 54), (174, 54), (82, 202), (174, 202)): p.ell([x - 6, y - 6, x + 6, y + 6], m["trim"], 2)
    p.deco(128, 128, 22)

def s_buckler(p):
    m = p.m
    p.ell([56, 56, 200, 200], m["M"]); p.ell([66, 66, 150, 130], m["L"], 0)
    p.ell([100, 100, 156, 156], m["D"]); p.spike(128, 100, 90, 30, 12, m["L"])
    for a in range(0, 360, 60): p.spike(128 + math.cos(math.radians(a)) * 72, 128 - math.sin(math.radians(a)) * 72, a, 22, 9, m["trim"])
    p.deco(128, 170, 10)

def s_bone(p):
    m = p.m
    p.poly([(60, 30), (196, 30), (200, 150), (128, 230), (56, 150)], m["D"])
    for k in range(5):
        y = 52 + k * 30; w = 64 - k * 8
        p.rect([128 - w, y, 128 + w, y + 18], BONE, 9)
    p.rect([120, 40, 136, 210], BONE, 6); p.deco(128, 104, 14)

def s_wing(p):
    m = p.m
    for s in (-1, 1):
        p.poly([(128, 50), (128 + s * 96, 24), (128 + s * 110, 80), (128 + s * 90, 120), (128 + s * 104, 150), (128 + s * 70, 170), (128 + s * 76, 200), (128, 230)], m["M"])
        for k in range(3): p.line([(128, 90 + k * 40), (128 + s * (86 - k * 10), 70 + k * 46)], m["D"], 4)
    p.rect([116, 44, 140, 230], m["trim"], 6); p.deco(128, 120, 14)

def s_mirror(p):
    m = p.m
    p.ell([34, 34, 222, 222], m["trim"]); p.ell([50, 50, 206, 206], m["D"])
    p.ell([64, 64, 192, 192], m["L"]); p.poly([(80, 110), (120, 70), (140, 80), (92, 132)], (255, 255, 255), 0)
    for a in range(0, 360, 30): p.ell([128 + math.cos(math.radians(a)) * 100 - 5, 128 - math.sin(math.radians(a)) * 100 - 5, 128 + math.cos(math.radians(a)) * 100 + 5, 128 - math.sin(math.radians(a)) * 100 + 5], m["acc"], 2)
    p.deco(128, 140, 18)

SHIELD_FAMILIES = {"Rondache": s_round, "Écu": s_kite, "Pavois": s_tower, "Bouclier à pointes": s_buckler,
                   "Bouclier d'os": s_bone, "Bouclier ailé": s_wing, "Miroir": s_mirror}

# =============== GANTELETS ===============
def g_gauntlet(p):
    m = p.m
    p.rect([70, 150, 186, 230], m["D"], 14); p.rect([62, 70, 194, 168], m["M"], 34)
    for k, x in enumerate((70, 102, 134, 166)): p.rect([x, 48 - (k in (1, 2)) * 10, x + 30, 110], m["L"], 14)
    p.rect([34, 92, 82, 150], m["L"], 20); p.deco(128, 190, 14)

def g_glove(p):
    m = p.m; c = m["cloth"]
    p.poly([(60, 150), (196, 150), (220, 234), (36, 234)], shade(c, 0.8)); p.rect([66, 80, 190, 166], c, 30)
    for k, x in enumerate((72, 104, 136, 168)): p.rect([x, 44 + (k == 0) * 14, x + 28, 112], c, 14)
    p.rect([36, 100, 84, 150], c, 20); p.rect([66, 140, 190, 158], m["trim"], 4); p.deco(128, 118, 14)

def g_claw(p):
    m = p.m
    p.rect([70, 150, 186, 230], m["D"], 14); p.rect([62, 90, 194, 168], m["M"], 30)
    for x in (78, 112, 146, 180): p.poly([(x - 12, 100), (x + 12, 100), (x + 6, 40), (x - 4, 10), (x - 8, 44)], m["L"])
    p.rect([34, 102, 80, 150], m["M"], 20); p.deco(128, 130, 12)

def g_bracer(p):
    m = p.m
    p.rect([74, 20, 182, 70], (250, 206, 166), 20)
    p.poly([(66, 70), (190, 70), (200, 230), (56, 230)], m["M"]); p.poly([(66, 70), (128, 70), (128, 230), (56, 230)], m["L"], 0)
    for y in (100, 150, 200): p.rect([60, y, 196, y + 12], m["trim"], 4)
    p.deco(128, 126, 13)

def g_wraps(p):
    m = p.m
    p.rect([64, 50, 192, 170], (250, 206, 166), 34)
    for k in range(6): p.rect([60, 60 + k * 26, 196, 82 + k * 26], (236, 226, 204) if k % 2 == 0 else (210, 196, 170), 8, 3)
    p.rect([70, 166, 186, 232], m["grip"], 10); p.deco(128, 200, 12)

def g_spiked(p):
    m = p.m
    p.rect([70, 150, 186, 230], m["D"], 14); p.rect([60, 64, 196, 168], m["M"], 34)
    p.rect([60, 64, 196, 100], m["trim"], 16)
    for x in (80, 112, 144, 176): p.spike(x, 68, 90, 30, 10, m["L"])
    p.rect([32, 96, 80, 150], m["M"], 20); p.deco(128, 190, 12)

GLOVE_FAMILIES = {"Gantelets": g_gauntlet, "Gants": g_glove, "Griffes": g_claw, "Brassards": g_bracer, "Bandages": g_wraps, "Poings cloutés": g_spiked}

# =============== BOTTES ===============
def b_boot(p):
    m = p.m
    p.poly([(84, 30), (162, 30), (164, 160), (222, 176), (226, 222), (64, 222), (78, 150)], m["M"])
    p.rect([62, 210, 228, 232], m["D"], 6); p.rect([76, 22, 170, 52], m["L"], 8); p.deco(124, 100, 13)

def b_sabaton(p):
    m = p.m
    for k in range(5):
        y = 30 + k * 34
        p.rect([76 + k * 2, y, 168 - k * 2 + (k >= 3) * 30, y + 40], m["M"] if k % 2 == 0 else m["L"], 10)
    p.poly([(150, 160), (226, 186), (226, 224), (66, 224), (70, 168)], m["M"]); p.rect([62, 214, 230, 234], m["D"], 6)
    p.deco(122, 98, 12)

def b_curly(p):
    m = p.m; c = m["cloth"]
    p.poly([(88, 40), (158, 40), (160, 150), (200, 160), (232, 130), (238, 170), (214, 222), (70, 222), (82, 150)], c)
    p.ell([224, 112, 250, 138], m["trim"]); p.rect([80, 28, 166, 56], m["M"], 10); p.rect([66, 212, 220, 232], m["D"], 6)
    p.deco(122, 120, 12)

def b_sandal(p):
    m = p.m
    p.poly([(96, 150), (150, 150), (150, 170), (220, 186), (220, 214), (80, 214), (90, 170)], (250, 206, 166))
    p.rect([70, 206, 230, 230], m["grip"], 8)
    for y in (160, 186): p.rect([86, y, 200 if y > 170 else 156, y + 12], m["M"], 4, 3)
    for k in range(4): p.rect([100, 40 + k * 28, 146, 52 + k * 28], m["M"], 4, 3)
    p.line([(122, 40), (122, 150)], m["M"], 6); p.deco(122, 26, 12)

def b_fur(p):
    m = p.m
    p.poly([(84, 70), (162, 70), (164, 160), (222, 176), (226, 222), (64, 222), (78, 150)], m["M"])
    p.rect([62, 210, 228, 232], m["D"], 6)
    for k in range(7): p.ell([66 + k * 16, 30 + (k % 2) * 8, 102 + k * 16, 80 + (k % 2) * 8], (240, 232, 220) if m["motif"] != "flame" else (90, 80, 80), 3)
    p.deco(124, 130, 13)

def b_winged(p):
    m = p.m
    p.poly([(84, 30), (162, 30), (164, 160), (222, 176), (226, 222), (64, 222), (78, 150)], m["M"]); p.rect([62, 210, 228, 232], m["D"], 6)
    p.poly([(84, 70), (26, 36), (38, 66), (20, 84), (44, 100), (36, 120), (82, 120)], m["trim"])
    p.rect([76, 22, 170, 52], m["L"], 8); p.deco(124, 110, 12)

BOOT_FAMILIES = {"Bottes": b_boot, "Solerets": b_sabaton, "Poulaines": b_curly, "Sandales": b_sandal, "Bottes fourrées": b_fur, "Bottes ailées": b_winged}

# =============== CEINTURES ===============
def strap(p, col=None):
    m = p.m; col = col or m["grip"]
    p.poly([(14, 98), (242, 98), (242, 158), (14, 158)], col); p.line([(14, 108), (242, 108)], shade(col, 1.25), 4)
    for x in (34, 62, 194, 222): p.ell([x - 6, 122, x + 6, 134], m["D"], 0)

def belt_round(p):
    m = p.m; strap(p); p.ell([78, 68, 178, 188], m["M"]); p.ell([96, 86, 160, 170], m["L"], 0); p.gem(128, 128, 18)

def belt_square(p):
    m = p.m; strap(p); p.rect([86, 78, 170, 178], m["M"], 12); p.rect([104, 96, 152, 160], m["grip"], 4); p.line([(128, 96), (128, 160)], m["L"], 6)
    p.deco(196, 128, 12)

def belt_skull(p):
    m = p.m; strap(p)
    p.ell([84, 64, 172, 152], BONE); p.rect([104, 140, 152, 186], BONE, 8)
    p.ell([98, 92, 122, 116], m["acc"], 0); p.ell([134, 92, 158, 116], m["acc"], 0)
    for x in (114, 128, 142): p.line([(x, 152), (x, 180)], INK, 3)

def belt_rope(p):
    m = p.m
    for k in range(13): p.ell([8 + k * 19, 106, 34 + k * 19, 150], (190, 146, 90) if k % 2 else (166, 120, 70), 3)
    p.rect([150, 150, 206, 214], m["grip"], 12); p.rect([150, 150, 206, 170], shade(m["grip"], 1.2), 8); p.deco(178, 190, 12)
    p.line([(100, 140), (90, 200)], (176, 130, 80), 8); p.line([(116, 140), (124, 196)], (176, 130, 80), 8)

def belt_chain(p):
    m = p.m
    for k in range(10): p.ell([10 + k * 24, 106 + (k % 2) * 6, 40 + k * 24, 140 + (k % 2) * 6], m["M"], 4)
    p.ell([92, 76, 164, 172], m["trim"]); p.deco(128, 124, 16)

def belt_sash(p):
    m = p.m; c = m["cloth"]
    p.poly([(14, 96), (242, 96), (242, 160), (14, 160)], c); p.line([(14, 128), (242, 128)], shade(c, 1.2), 6)
    p.poly([(128, 128), (170, 150), (190, 236), (156, 226), (140, 160)], shade(c, 0.85)); p.poly([(128, 128), (96, 150), (80, 230), (110, 222), (120, 160)], shade(c, 0.95))
    p.ell([108, 108, 148, 148], c); p.deco(128, 128, 12)

def belt_pouch(p):
    m = p.m; strap(p)
    p.rect([46, 140, 112, 214], m["grip"], 14); p.rect([46, 140, 112, 164], shade(m["grip"], 1.2), 10)
    p.rect([144, 140, 210, 214], m["grip"], 14); p.rect([144, 140, 210, 164], shade(m["grip"], 1.2), 10)
    p.rect([106, 90, 150, 166], m["M"], 8); p.deco(128, 128, 11)

BELT_FAMILIES = {"Ceinture": belt_round, "Ceinturon": belt_square, "Sangle au crâne": belt_skull, "Corde": belt_rope,
                 "Chaîne": belt_chain, "Écharpe": belt_sash, "Ceinture à bourses": belt_pouch}

# =============== AMULETTES ===============
def chain(p):
    m = p.m
    for k in range(13):
        a = math.pi * 1.08 + k * (math.pi * 0.84 / 12)
        cx, cy = 128 + math.cos(a) * 86, 30 - math.sin(a) * 120
        p.ell([cx - 8, cy - 8, cx + 8, cy + 8], m["trim"] if m["motif"] != "rope" else (176, 128, 72), 3)

def a_drop(p): m = p.m; chain(p); p.poly([(128, 120), (170, 166), (128, 232), (86, 166)], m["M"]); p.gem(128, 172, 22)
def a_medal(p):
    m = p.m; chain(p); p.ell([78, 120, 178, 220], m["M"]); p.ell([92, 134, 164, 206], m["L"], 0); p.deco(128, 170, 18)
def a_fang(p):
    m = p.m; chain(p); p.rect([110, 116, 146, 136], m["M"], 6)
    p.poly([(108, 134), (148, 134), (140, 196), (128, 238), (116, 196)], BONE); p.poly([(108, 134), (128, 134), (128, 238), (116, 196)], shade(BONE, 0.85), 0)
def a_skull(p):
    m = p.m; chain(p); p.ell([86, 118, 170, 196], BONE); p.rect([104, 184, 152, 220], BONE, 8)
    p.ell([98, 144, 122, 168], m["acc"], 0); p.ell([134, 144, 158, 168], m["acc"], 0)
def a_eye(p):
    m = p.m; chain(p); p.ell([74, 124, 182, 216], m["M"])
    p.poly([(84, 170), (110, 146), (146, 146), (172, 170), (146, 194), (110, 194)], (255, 255, 240), 3)
    p.ell([110, 152, 146, 188], m["acc"], 0); p.ell([120, 162, 136, 178], INK, 0)
def a_heart(p):
    m = p.m; chain(p); x, y, r = 128, 176, 46
    p.poly([(x, y + r), (x - r, y), (x - r * 0.9, y - r * 0.7), (x - r * 0.4, y - r * 0.9), (x, y - r * 0.5),
            (x + r * 0.4, y - r * 0.9), (x + r * 0.9, y - r * 0.7), (x + r, y)], m["acc"])
    p.ell([96, 146, 116, 162], shade(m["acc"], 1.4), 0)
def a_ankh(p):
    m = p.m; chain(p); p.ell([102, 112, 154, 166], m["M"]); p.hole([114, 124, 142, 154])
    p.rect([120, 160, 136, 236], m["M"], 4); p.rect([88, 164, 168, 180], m["M"], 4); p.gem(128, 172, 9)

AMULET_FAMILIES = {"Pendentif": a_drop, "Médaillon": a_medal, "Croc": a_fang, "Crâne": a_skull, "Œil": a_eye, "Cœur": a_heart, "Croix ansée": a_ankh}

# =============== ANNEAUX ===============
def band(p, c=None):
    c = c or p.m["M"]
    p.ell([50, 92, 206, 236], c); p.ell([78, 120, 178, 210], (0, 0, 0, 0), 4); p.hole([84, 126, 172, 204])
    p.ell([66, 104, 110, 150], shade(c, 1.3), 0)
    return c

def r_gem(p): c = band(p); p.poly([(92, 98), (164, 98), (150, 64), (106, 64)], c); p.gem(128, 62, 26)
def r_signet(p):
    m = p.m; c = band(p); p.rect([82, 48, 174, 112], c, 18); p.rect([96, 60, 160, 100], shade(c, 0.8), 12); p.deco(128, 80, 13)
def r_claw(p):
    m = p.m; c = band(p)
    for s in (-1, 0, 1): p.poly([(128 + s * 26 - 8, 106), (128 + s * 26 + 8, 106), (128 + s * 34, 40)], c, 3)
    p.ell([100, 52, 156, 104], m["acc"]); p.ell([108, 58, 126, 74], shade(m["acc"], 1.4), 0)
def r_skull(p):
    m = p.m; band(p); p.ell([92, 30, 164, 96], BONE); p.rect([108, 84, 148, 112], BONE, 6)
    p.ell([102, 54, 122, 74], m["acc"], 0); p.ell([134, 54, 154, 74], m["acc"], 0)
def r_twisted(p):
    m = p.m
    for k in range(16):
        a = k * math.tau / 16; cx, cy = 128 + math.cos(a) * 64, 150 + math.sin(a) * 60
        p.ell([cx - 16, cy - 16, cx + 16, cy + 16], m["M"] if k % 2 else m["trim"], 3)
    p.deco(128, 84, 14)
def r_spiked(p):
    m = p.m; c = band(p)
    for a in range(0, 360, 45): p.spike(128 + math.cos(math.radians(a)) * 78, 164 - math.sin(math.radians(a)) * 72, a, 20, 8, m["L"])
    p.gem(128, 92, 16)

RING_FAMILIES = {"Anneau": r_gem, "Chevalière": r_signet, "Bague griffue": r_claw, "Bague au crâne": r_skull, "Anneau torsadé": r_twisted, "Anneau à pointes": r_spiked}

# =============== Modèles de chaque cercle ===============
SETS = {
    0: [  # Lame
        ["Gourdin", "Hache", "Lance", "Dague", "Arc", "Bâton"],
        ["Rapière", "Dague", "Arc", "Sceptre", "Lame courbe", "Lance"],
        ["Couperet", "Gourdin", "Marteau de guerre", "Trident", "Masse", "Hache"],
        ["Sceptre", "Masse", "Épée", "Marteau de guerre", "Bâton", "Hallebarde"],
        ["Espadon", "Hache", "Fléau", "Marteau de guerre", "Épée", "Lance"],
        ["Dague", "Bâton", "Faux", "Sceptre", "Fléau", "Épée"],
        ["Couperet", "Fléau", "Hallebarde", "Hache", "Espadon", "Gourdin"],
        ["Lame courbe", "Dague", "Rapière", "Faux", "Arc", "Trident"],
        ["Lance", "Épée", "Hallebarde", "Arc", "Bâton", "Espadon"],
        ["Espadon", "Faux", "Trident", "Hallebarde", "Lame courbe", "Sceptre"],
    ],
    1: [  # Heaume
        ["Crâne", "Capuche", "Casque", "Heaume cornu", "Barbute"],
        ["Couronne", "Heaume à plumet", "Masque", "Heaume ailé", "Capuche"],
        ["Casque", "Heaume", "Barbute", "Heaume cornu", "Crâne"],
        ["Couronne", "Heaume", "Heaume à plumet", "Heaume ailé", "Barbute"],
        ["Heaume cornu", "Heaume", "Crâne", "Couronne", "Barbute"],
        ["Mitre", "Capuche", "Crâne", "Heaume", "Couronne"],
        ["Heaume cornu", "Barbute", "Crâne", "Heaume", "Capuche"],
        ["Bonnet de bouffon", "Masque", "Capuche", "Heaume à plumet", "Couronne"],
        ["Couronne", "Heaume ailé", "Heaume", "Capuche", "Heaume cornu"],
        ["Heaume cornu", "Couronne", "Heaume ailé", "Masque", "Heaume"],
    ],
    2: [["Bouclier d'os", "Rondache", "Bouclier à pointes"], ["Miroir", "Écu", "Bouclier ailé"], ["Rondache", "Pavois", "Bouclier à pointes"],
        ["Écu", "Miroir", "Pavois"], ["Pavois", "Rondache", "Bouclier à pointes"], ["Bouclier d'os", "Pavois", "Miroir"],
        ["Bouclier à pointes", "Pavois", "Bouclier d'os"], ["Miroir", "Écu", "Bouclier à pointes"], ["Écu", "Bouclier ailé", "Rondache"],
        ["Bouclier ailé", "Pavois", "Miroir"]],
    3: [["Bandages", "Griffes", "Brassards"], ["Gants", "Brassards", "Gantelets"], ["Gants", "Bandages", "Poings cloutés"],
        ["Gantelets", "Gants", "Brassards"], ["Poings cloutés", "Gantelets", "Griffes"], ["Brassards", "Gants", "Griffes"],
        ["Poings cloutés", "Griffes", "Bandages"], ["Gants", "Griffes", "Brassards"], ["Gantelets", "Brassards", "Griffes"],
        ["Griffes", "Gantelets", "Poings cloutés"]],
    4: [["Sandales", "Bottes fourrées", "Bottes"], ["Poulaines", "Sandales", "Bottes ailées"], ["Bottes", "Bottes fourrées", "Solerets"],
        ["Solerets", "Poulaines", "Bottes ailées"], ["Solerets", "Bottes", "Bottes fourrées"], ["Bottes", "Poulaines", "Sandales"],
        ["Solerets", "Bottes fourrées", "Bottes"], ["Poulaines", "Bottes", "Bottes ailées"], ["Bottes fourrées", "Solerets", "Bottes ailées"],
        ["Bottes ailées", "Solerets", "Poulaines"]],
    5: [["Corde", "Sangle au crâne", "Ceinture à bourses"], ["Écharpe", "Ceinture", "Chaîne"], ["Ceinture à bourses", "Corde", "Ceinturon"],
        ["Ceinture", "Chaîne", "Ceinturon"], ["Sangle au crâne", "Chaîne", "Ceinturon"], ["Sangle au crâne", "Écharpe", "Ceinture"],
        ["Chaîne", "Sangle au crâne", "Ceinturon"], ["Écharpe", "Ceinture à bourses", "Ceinture"], ["Chaîne", "Ceinture", "Écharpe"],
        ["Sangle au crâne", "Ceinture", "Chaîne"]],
    6: [["Croc", "Crâne", "Pendentif"], ["Cœur", "Pendentif", "Médaillon"], ["Croc", "Médaillon", "Œil"],
        ["Médaillon", "Pendentif", "Cœur"], ["Crâne", "Œil", "Pendentif"], ["Croix ansée", "Crâne", "Œil"],
        ["Croc", "Cœur", "Crâne"], ["Œil", "Médaillon", "Croix ansée"], ["Pendentif", "Croix ansée", "Médaillon"],
        ["Œil", "Crâne", "Cœur"]],
    7: [["Anneau torsadé", "Bague au crâne", "Bague griffue"], ["Anneau", "Anneau torsadé", "Chevalière"], ["Chevalière", "Bague au crâne", "Anneau à pointes"],
        ["Anneau", "Chevalière", "Anneau torsadé"], ["Anneau à pointes", "Bague griffue", "Anneau"], ["Bague au crâne", "Chevalière", "Bague griffue"],
        ["Anneau à pointes", "Bague au crâne", "Anneau"], ["Anneau torsadé", "Bague griffue", "Chevalière"], ["Anneau", "Bague griffue", "Anneau à pointes"],
        ["Bague au crâne", "Anneau", "Anneau à pointes"]],
}
FAMILIES = [WEAPON_FAMILIES, HELMET_FAMILIES, SHIELD_FAMILIES, GLOVE_FAMILIES, BOOT_FAMILIES, BELT_FAMILIES, AMULET_FAMILIES, RING_FAMILIES]
ROT = [-45, 0, 0, 0, 0, 0, 0, 0]

def write_cs():
    lines = ["// Généré par Tools/make_item_icons.py : noms des modèles de pièces, par cercle puis par emplacement.",
             "// Chaque cercle a ses propres modèles (comme les âges de Forge Master) ; l'indice = Item.model.",
             "public static class ItemModels", "{", "    public static readonly string[][][] Names =", "    {"]
    for c in range(10):
        parts = []
        for s in range(8):
            parts.append("new[] { " + ", ".join('"' + n + '"' for n in SETS[s][c]) + " }")
        lines.append("        new[] { // " + MAT[c]["name"])
        for ptxt in parts: lines.append("            " + ptxt + ",")
        lines.append("        },")
    lines += ["    };", "}", ""]
    open(CS_OUT, "w", encoding="utf-8").write("\n".join(lines))

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for f in glob.glob(os.path.join(OUT, "s?_c?.png")): os.remove(f)
    for s in range(8):
        for c in range(10):
            for v, fam in enumerate(SETS[s][c]):
                p = Pen(MAT[c])
                FAMILIES[s][fam](p)
                p.save("s%d_c%d_v%d" % (s, c, v), ROT[s])
    write_cs()
    print("ok")
