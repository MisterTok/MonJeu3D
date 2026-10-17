# Icônes des pièces façon Forge Master : dessins plats à gros contours noirs.
# Plusieurs MODÈLES par emplacement (comme FM : beaucoup d'armes et de casques, 2-3 modèles ailleurs) ;
# le cercle donne le matériau (0-2 fer rouillé, 3-5 acier, 6-8 acier noirci, 9 or) et les ornements (gemme, pointes),
# l'accent prend la couleur du cercle.
# Sortie : Assets/Resources/ForgeUI/Items/s<emplacement>_c<cercle>_v<modèle>.png (256 px, fond transparent)
# Les armes sont dessinées verticales, poignée centrée en (128, 199), puis tournées de 45° : la prise est au même
# endroit pour toutes (PaperHero s'en sert pour les tenir en main).
# Usage : python Tools/make_item_icons.py
import math, os, glob
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Items")
N, S = 256, 4
INK = (28, 16, 16, 255)
CIRCLE = [(158, 153, 148), (230, 89, 166), (128, 199, 64), (242, 191, 56), (235, 56, 38), (255, 128, 20), (179, 20, 64), (158, 82, 242), (89, 204, 255), (255, 242, 191)]
METAL = [((176, 160, 140), (138, 120, 102), (98, 84, 70)),      # fer rouillé
         ((210, 216, 230), (152, 160, 178), (104, 110, 130)),   # acier
         ((132, 128, 150), (92, 88, 110), (60, 56, 76)),        # acier noirci
         ((255, 230, 130), (230, 170, 50), (170, 110, 30))]     # or
GOLD = ((255, 222, 110), (214, 150, 40))
WOOD = ((176, 110, 60), (126, 72, 36))
LEATHER = ((150, 92, 56), (108, 62, 34))
BONE = (236, 226, 204)
# Nombre de modèles par emplacement (Lame, Heaume, Bouclier, Gantelets, Bottes, Ceinture, Amulette, Anneau).
MODELS = [6, 5, 3, 3, 3, 3, 3, 3]

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
    def hole(self, b):
        self.d.ellipse([v * S for v in b], fill=(0, 0, 0, 0))
    def gem(self, cx, cy, r, c):
        self.poly([(cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)], c, 3)
        self.poly([(cx, cy - r), (cx + r * 0.45, cy - r * 0.1), (cx, cy + r * 0.2), (cx - r * 0.45, cy - r * 0.1)], shade(c, 1.35), 0)
    def spike(self, x, y, ang, ln, w, c):
        a = math.radians(ang)
        tip = (x + math.cos(a) * ln, y - math.sin(a) * ln)
        b1 = (x + math.cos(a + 1.57) * w, y - math.sin(a + 1.57) * w); b2 = (x + math.cos(a - 1.57) * w, y - math.sin(a - 1.57) * w)
        self.poly([b1, tip, b2], c, 3)
    def save(self, name, rot=0):
        im = self.im
        if rot: im = im.rotate(rot, resample=Image.BICUBIC, center=(N * S / 2, N * S / 2))
        a = im.getchannel("A")
        ol = a.filter(ImageFilter.MaxFilter(6 * S + 1))
        base = Image.new("RGBA", im.size, INK); base.putalpha(ol)
        base.alpha_composite(im)
        base.resize((N, N), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

# ---------------- Armes (verticales, poignée en 128,178-220) ----------------
def hilt(p, t, acc, guard=True):
    m = METAL[t]
    if guard: p.poly([(76, 160), (180, 160), (172, 178), (84, 178)], GOLD[1] if t else WOOD[1])
    p.rect([118, 176, 138, 222], LEATHER[1] if t < 3 else (120, 40, 40), 3)
    if t >= 1: p.gem(128, 236, 11, acc)
    else: p.ell([112, 214, 144, 244], m[1])

def handle(p, t, top, bottom=228):
    p.rect([120, top, 136, bottom], WOOD[0] if t < 2 else (70, 50, 60) if t == 2 else GOLD[1], 4)
    for y in range(top + 30, bottom - 10, 34): p.rect([116, y, 140, y + 8], LEATHER[1], 2, 3)
    p.ell([114, bottom - 8, 142, bottom + 18], METAL[t][2] if t < 3 else GOLD[0])

def w_sword(p, t, acc):
    L, M, D = METAL[t]
    p.poly([(128, 10), (150, 38), (148, 164), (108, 164), (106, 38)], L)
    p.poly([(128, 10), (128, 164), (108, 164), (106, 38)], M, 0)
    p.line([(128, 40), (128, 150)], acc if t >= 2 else D, 5)
    if t >= 2:
        for y in (60, 96, 130): p.spike(150, y, 20, 18, 7, L); p.spike(106, y, 160, 18, 7, L)
    hilt(p, t, acc)

def w_axe(p, t, acc):
    L, M, D = METAL[t]
    handle(p, t, 30)
    blade = [(132, 40), (196, 22), (214, 70), (200, 118), (132, 100)]
    p.poly(blade, M); p.poly([(196, 22), (214, 70), (200, 118), (186, 70)], L, 0)
    if t >= 1: p.poly([(124, 46), (78, 36), (70, 70), (124, 90)], M)
    if t >= 2: p.spike(128, 30, 90, 30, 9, L)
    if t >= 1: p.gem(128, 70, 10, acc)

def w_mace(p, t, acc):
    L, M, D = METAL[t]
    handle(p, t, 96)
    p.ell([84, 22, 172, 110], M); p.ell([96, 32, 136, 66], L, 0)
    n = 6 if t >= 2 else 4
    for k in range(n):
        a = 90 + k * 360 / n
        cx, cy = 128 + math.cos(math.radians(a)) * 44, 66 - math.sin(math.radians(a)) * 44
        p.spike(cx, cy, a, 18 if t else 12, 8, L)
    if t >= 1: p.gem(128, 66, 13, acc)
    p.rect([108, 104, 148, 118], D, 4)

def w_spear(p, t, acc):
    L, M, D = METAL[t]
    handle(p, t, 84, 232)
    p.poly([(128, 6), (154, 56), (138, 92), (118, 92), (102, 56)], L)
    p.poly([(128, 6), (128, 92), (118, 92), (102, 56)], M, 0)
    p.rect([112, 88, 144, 104], GOLD[1] if t else D, 4)
    if t >= 1: p.poly([(144, 104), (168, 126), (144, 120)], acc, 3)
    if t >= 2: p.poly([(112, 104), (88, 126), (112, 120)], acc, 3)

def w_scythe(p, t, acc):
    L, M, D = METAL[t]
    handle(p, t, 26, 232)
    blade = [(128, 30), (70, 20), (28, 48), (14, 92), (40, 64), (82, 50), (128, 60)]
    p.poly(blade, M); p.poly([(70, 20), (28, 48), (14, 92), (40, 58)], L, 0)
    p.rect([116, 22, 140, 66], D, 4)
    if t >= 1: p.gem(128, 44, 9, acc)
    if t >= 2: p.spike(128, 26, 90, 26, 8, L)

def w_trident(p, t, acc):
    L, M, D = METAL[t]
    handle(p, t, 96, 232)
    p.rect([84, 84, 172, 104], M, 8)
    for x, top in ((92, 30), (128, 10), (164, 30)):
        p.poly([(x - 9, 92), (x - 9, top + 22), (x, top), (x + 9, top + 22), (x + 9, 92)], L)
        p.poly([(x + 9, top + 34), (x + 20, top + 24), (x + 9, top + 48)], L, 3) if x != 128 else None
    if t >= 1: p.gem(128, 94, 10, acc)

WEAPONS = [w_sword, w_axe, w_mace, w_spear, w_scythe, w_trident]

# ---------------- Heaumes ----------------
def h_cap(p, t, acc):
    L, M, D = METAL[t]
    p.ell([52, 54, 204, 206], M); p.rect([48, 140, 208, 168], D, 6)
    p.rect([118, 140, 138, 212], D, 4); p.ell([70, 70, 130, 110], L, 0)
    if t >= 1: p.gem(128, 102, 12, acc)
    if t >= 2: p.spike(128, 56, 90, 30, 10, L)

def h_great(p, t, acc):
    L, M, D = METAL[t]
    p.rect([58, 52, 198, 214], M, 40); p.ell([72, 62, 140, 110], L, 0)
    p.rect([74, 116, 182, 132], INK, 4, 0); p.rect([122, 132, 134, 196], INK, 2, 0)
    for y in (150, 168, 186): p.ell([96, y, 104, y + 8], D, 0); p.ell([152, y, 160, y + 8], D, 0)
    if t >= 1: p.poly([(120, 54), (136, 54), (150, 18), (128, 34), (110, 16)], acc)
    if t >= 2: p.rect([58, 88, 198, 100], GOLD[1], 2, 3)

def h_horned(p, t, acc):
    L, M, D = METAL[t]
    horn = BONE if t < 2 else (60, 40, 50) if t == 2 else GOLD[0]
    for s in (-1, 1):
        p.poly([(128 + s * 56, 106), (128 + s * 104, 46), (128 + s * 112, 20), (128 + s * 84, 66), (128 + s * 70, 130)], horn)
    p.rect([62, 64, 194, 218], M, 46); p.ell([78, 74, 140, 118], L, 0)
    p.poly([(80, 128), (176, 128), (160, 150), (96, 150)], INK, 0)
    if t >= 1: p.gem(128, 96, 12, acc)

def h_hood(p, t, acc):
    L, M, D = METAL[t]
    cloth = [(96, 70, 60), (70, 60, 90), (50, 40, 60), (120, 30, 40)][t]
    p.poly([(128, 20), (196, 84), (210, 226), (46, 226), (60, 84)], cloth)
    p.poly([(128, 20), (150, 40), (110, 120), (60, 84)], shade(cloth, 1.2), 0)
    p.ell([78, 96, 178, 206], (20, 12, 16))
    p.ell([96, 132, 120, 150], acc, 0); p.ell([136, 132, 160, 150], acc, 0)
    if t >= 1: p.rect([70, 210, 186, 230], M, 6)
    if t >= 2: p.gem(128, 218, 10, acc)

def h_crown(p, t, acc):
    L, M, D = METAL[t]
    p.rect([62, 66, 194, 218], M, 46); p.ell([78, 76, 140, 120], L, 0)
    crown = GOLD[1] if t >= 1 else D
    p.poly([(60, 96), (84, 52), (106, 84), (128, 40), (150, 84), (172, 52), (196, 96)], crown)
    p.poly([(80, 132), (176, 132), (160, 154), (96, 154)], INK, 0)
    p.gem(128, 76, 10, acc)
    if t >= 2: p.poly([(122, 60), (134, 60), (172, 8), (150, 50), (190, 30), (146, 76)], acc)

HELMETS = [h_cap, h_great, h_horned, h_hood, h_crown]

# ---------------- Boucliers ----------------
def s_round(p, t, acc):
    L, M, D = METAL[t]
    p.ell([36, 36, 220, 220], D)
    face = WOOD[0] if t == 0 else M
    p.ell([52, 52, 204, 204], face)
    if t == 0:
        for x in (96, 128, 160): p.line([(x, 56), (x, 200)], WOOD[1], 4)
    else:
        p.ell([70, 70, 186, 186], acc, 0); p.ell([86, 86, 170, 170], face, 0)
    p.ell([104, 104, 152, 152], L)
    if t >= 2:
        for a in range(0, 360, 45): p.spike(128 + math.cos(math.radians(a)) * 96, 128 - math.sin(math.radians(a)) * 96, a, 22, 9, L)

def s_kite(p, t, acc):
    L, M, D = METAL[t]
    pts = [(46, 40), (210, 40), (206, 130), (128, 226), (50, 130)]
    p.poly(pts, M if t < 3 else GOLD[1])
    p.poly([(64, 56), (192, 56), (190, 128), (128, 204), (66, 128)], acc if t >= 1 else WOOD[0])
    p.poly([(118, 56), (138, 56), (138, 196), (128, 204), (118, 196)], L, 0); p.poly([(66, 104), (190, 104), (190, 122), (66, 122)], L, 0)
    if t >= 2: p.gem(128, 113, 14, (255, 255, 255))

def s_tower(p, t, acc):
    L, M, D = METAL[t]
    p.rect([52, 26, 204, 230], D, 20); p.rect([66, 40, 190, 216], M, 14)
    for y in (70, 128, 186): p.rect([66, y - 6, 190, y + 6], D, 2, 0)
    for x, y in ((82, 54), (174, 54), (82, 202), (174, 202)): p.ell([x - 6, y - 6, x + 6, y + 6], L, 2)
    p.ell([92, 92, 164, 164], BONE); p.ell([106, 112, 124, 130], acc if t else INK, 0); p.ell([132, 112, 150, 130], acc if t else INK, 0)
    if t >= 2: p.spike(128, 26, 90, 22, 10, L)

SHIELDS = [s_round, s_kite, s_tower]

# ---------------- Gantelets ----------------
def g_gauntlet(p, t, acc):
    L, M, D = METAL[t]
    p.rect([70, 150, 186, 230], LEATHER[0] if t == 0 else D, 14)
    p.rect([62, 70, 194, 168], M, 34)
    for k, x in enumerate((70, 102, 134, 166)): p.rect([x, 48 - (k in (1, 2)) * 10, x + 30, 110], L, 14)
    p.rect([34, 92, 82, 150], L, 20)
    if t >= 1: p.gem(128, 190, 14, acc)
    if t >= 2:
        for x in (84, 116, 148, 176): p.spike(x, 72, 90, 26, 8, (230, 230, 240))

def g_glove(p, t, acc):
    L, M, D = METAL[t]
    leather = LEATHER if t < 3 else (GOLD[1], GOLD[1])
    p.poly([(60, 150), (196, 150), (220, 234), (36, 234)], leather[1])
    p.rect([66, 80, 190, 166], leather[0], 30)
    for k, x in enumerate((72, 104, 136, 168)): p.rect([x, 44 + (k == 0) * 14, x + 28, 112], leather[0], 14)
    p.rect([36, 100, 84, 150], leather[0], 20)
    p.rect([66, 140, 190, 158], M, 4)
    if t >= 1: p.rect([108, 96, 148, 130], M, 8); p.gem(128, 113, 10, acc)
    if t >= 2:
        for x in (86, 118, 150, 182): p.ell([x - 6, 50, x + 6, 62], L, 2)

def g_claw(p, t, acc):
    L, M, D = METAL[t]
    p.rect([70, 150, 186, 230], D, 14)
    p.rect([62, 90, 194, 168], M, 30)
    for x in (78, 112, 146, 180):
        p.poly([(x - 12, 100), (x + 12, 100), (x + 6, 40), (x - 4, 10), (x - 8, 44)], L)
    p.rect([34, 102, 80, 150], M, 20)
    p.gem(128, 128, 12 if t else 7, acc if t else D)

GLOVES = [g_gauntlet, g_glove, g_claw]

# ---------------- Bottes ----------------
def b_boot(p, t, acc):
    L, M, D = METAL[t]
    col = LEATHER[0] if t == 0 else M if t < 3 else GOLD[1]
    p.poly([(84, 30), (162, 30), (164, 160), (222, 176), (226, 222), (64, 222), (78, 150)], col)
    p.rect([62, 210, 228, 232], D if t else LEATHER[1], 6)
    p.rect([76, 22, 170, 52], L if t else LEATHER[1], 8)
    if t >= 1: p.poly([(84, 92), (162, 92), (164, 112), (80, 112)], L)
    if t >= 2: p.spike(186, 168, 30, 30, 9, (230, 230, 240)); p.gem(124, 70, 12, acc)

def b_sabaton(p, t, acc):
    L, M, D = METAL[t]
    for k in range(5):
        y = 30 + k * 34
        p.rect([76 + k * 2, y, 168 - k * 2 + (k >= 3) * 30, y + 40], M if k % 2 == 0 else L, 10)
    p.poly([(150, 160), (226, 186), (226, 224), (66, 224), (70, 168)], M)
    p.rect([62, 214, 230, 234], D, 6)
    if t >= 1: p.gem(122, 98, 12, acc)
    if t >= 2:
        for x in (96, 144): p.spike(x, 30, 90, 24, 8, L)

def b_curly(p, t, acc):
    L, M, D = METAL[t]
    cloth = [(120, 70, 50), (90, 60, 110), (60, 40, 70), (150, 40, 50)][t]
    p.poly([(88, 40), (158, 40), (160, 150), (200, 160), (232, 130), (238, 170), (214, 222), (70, 222), (82, 150)], cloth)
    p.ell([224, 112, 250, 138], acc if t else cloth)
    p.rect([80, 28, 166, 56], M, 10)
    p.rect([66, 212, 220, 232], D, 6)
    if t >= 1:
        for y in (80, 112): p.rect([88, y, 158, y + 10], GOLD[1], 2, 3)
    if t >= 2: p.gem(122, 140, 12, acc)

BOOTS = [b_boot, b_sabaton, b_curly]

# ---------------- Ceintures ----------------
def strap(p, t, acc):
    col = LEATHER[0] if t < 2 else (90, 40, 40) if t == 2 else (120, 30, 40)
    p.poly([(14, 98), (242, 98), (242, 158), (14, 158)], col)
    p.line([(14, 108), (242, 108)], shade(col, 1.25), 4)
    for x in (34, 62, 194, 222): p.ell([x - 6, 122, x + 6, 134], METAL[t][2], 0)
    return col

def belt_round(p, t, acc):
    col = strap(p, t, acc); m = METAL[t]
    p.ell([78, 68, 178, 188], m[1]); p.ell([96, 86, 160, 170], m[0], 0)
    p.gem(128, 128, 22 if t >= 2 else 16, acc if t else m[2])

def belt_square(p, t, acc):
    col = strap(p, t, acc); m = METAL[t]
    p.rect([86, 78, 170, 178], m[1], 12); p.rect([104, 96, 152, 160], col, 4); p.line([(128, 96), (128, 160)], m[0], 6)
    if t >= 1:
        for x, y in ((92, 84), (164, 84), (92, 172), (164, 172)): p.gem(x, y, 8, acc)

def belt_skull(p, t, acc):
    strap(p, t, acc); m = METAL[t]
    p.ell([84, 64, 172, 152], BONE if t < 3 else GOLD[0]); p.rect([104, 140, 152, 186], BONE if t < 3 else GOLD[0], 8)
    p.ell([98, 92, 122, 116], acc if t else INK, 0); p.ell([134, 92, 158, 116], acc if t else INK, 0)
    for x in (114, 128, 142): p.line([(x, 152), (x, 180)], INK, 3)
    if t >= 2:
        for s in (-1, 1): p.spike(128 + s * 40, 76, 90 + s * 40, 26, 8, m[0])

BELTS = [belt_round, belt_square, belt_skull]

# ---------------- Amulettes ----------------
def chain(p, t):
    c = GOLD[1] if t >= 2 else METAL[t][1]
    for k in range(13):
        a = math.pi * 1.08 + k * (math.pi * 0.84 / 12)
        cx, cy = 128 + math.cos(a) * 86, 30 - math.sin(a) * 120
        p.ell([cx - 8, cy - 8, cx + 8, cy + 8], c, 3)

def a_drop(p, t, acc):
    chain(p, t); m = METAL[t]
    p.poly([(128, 120), (170, 166), (128, 232), (86, 166)], m[1]); p.gem(128, 172, 22, acc)

def a_medal(p, t, acc):
    chain(p, t); m = METAL[t]
    p.ell([78, 120, 178, 220], m[1]); p.ell([92, 134, 164, 206], m[0], 0)
    p.poly([(128, 140), (140, 164), (166, 168), (146, 184), (152, 210), (128, 196), (104, 210), (110, 184), (90, 168), (116, 164)], acc)

def a_fang(p, t, acc):
    chain(p, t); m = METAL[t]
    p.rect([110, 116, 146, 136], m[1], 6)
    p.poly([(108, 134), (148, 134), (140, 196), (128, 238), (116, 196)], BONE if t < 3 else GOLD[0])
    p.poly([(108, 134), (128, 134), (128, 238), (116, 196)], shade(BONE, 0.85) if t < 3 else GOLD[1], 0)
    if t >= 1:
        for s in (-1, 1): p.gem(128 + s * 36, 140, 10, acc)

AMULETS = [a_drop, a_medal, a_fang]

# ---------------- Anneaux ----------------
def band(p, t):
    c = METAL[t][1] if t < 2 else GOLD[1] if t == 2 else GOLD[0]
    p.ell([50, 92, 206, 236], c); p.ell([78, 120, 178, 210], (0, 0, 0, 0), 4); p.hole([84, 126, 172, 204])
    p.ell([66, 104, 110, 150], shade(c, 1.3), 0)
    return c

def r_gem(p, t, acc):
    c = band(p, t)
    p.poly([(92, 98), (164, 98), (150, 64), (106, 64)], c)
    p.gem(128, 62, 24 if t < 3 else 32, acc)
    if t >= 2:
        for x in (88, 168): p.gem(x, 96, 9, acc)

def r_signet(p, t, acc):
    c = band(p, t)
    p.rect([82, 48, 174, 112], c, 18); p.rect([96, 60, 160, 100], shade(c, 0.8), 12)
    p.poly([(128, 64), (138, 80), (156, 82), (142, 92), (146, 98), (128, 92), (110, 98), (114, 92), (100, 82), (118, 80)], acc if t else shade(c, 1.2), 3)

def r_claw(p, t, acc):
    c = band(p, t)
    for s in (-1, 0, 1):
        p.poly([(128 + s * 26 - 8, 106), (128 + s * 26 + 8, 106), (128 + s * 34, 40)], c, 3)
    p.ell([100, 52, 156, 104], acc if t else METAL[0][2]); p.ell([108, 58, 126, 74], shade(acc, 1.4) if t else METAL[0][0], 0)

RINGS = [r_gem, r_signet, r_claw]

SLOTS = [(WEAPONS, -45), (HELMETS, 0), (SHIELDS, 0), (GLOVES, 0), (BOOTS, 0), (BELTS, 0), (AMULETS, 0), (RINGS, 0)]

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for f in glob.glob(os.path.join(OUT, "s?_c?.png")): os.remove(f)   # ancien format sans modèle
    for s, (fns, rot) in enumerate(SLOTS):
        assert len(fns) == MODELS[s]
        for c in range(10):
            for v, fn in enumerate(fns):
                p = Pen()
                fn(p, tier(c), CIRCLE[c])
                p.save("s%d_c%d_v%d" % (s, c, v), rot)
    print("ok")
