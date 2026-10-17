# Héros en pièces détachées façon Forge Master (animé par code dans PaperHero) + équipement PORTÉ dessiné pour lui.
# Chaque pièce a son articulation au CENTRE de l'image ; échelle commune : 400 px = taille du héros.
# Base : head, hair, torso, arm, leg.
# Porté (un fichier par cercle et par modèle, mêmes modèles que les icônes de Tools/make_item_icons.py) :
#   helm_c<c>_v<m>  (toile 360 x 600, cou au centre : la tête de base y est décalée de +40, +80)
#   glove_c<c>_v<m> (toile du bras)   boot_c<c>_v<m> (toile de la jambe)   belt_c<c>_v<m> (toile du torse)
# Sortie : Assets/Resources/ForgeUI/Hero/
# Usage : python Tools/make_paper_hero.py
import os, sys, math
from PIL import Image, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(__file__))
from make_item_icons import MAT, SETS, BONE, GOLD   # mêmes modèles et palettes que les icônes

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Hero")
S = 4
INK = (28, 16, 16, 255)
SKIN, SKIN_D = (252, 210, 172, 255), (228, 172, 132, 255)
HAIR, HAIR_D = (120, 66, 36, 255), (88, 46, 24, 255)
TUNIC, TUNIC_D = (196, 52, 46, 255), (150, 34, 32, 255)
PANTS = (70, 58, 76, 255)
BOOT, BOOT_D = (110, 66, 40, 255), (80, 46, 28, 255)
CLEAR = (0, 0, 0, 0)

def rgba(c): return tuple(c[:3]) + (255,) if len(c) == 3 else c
def shade(c, k): return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (255,)

class Pen:
    def __init__(self, w, h, ox=0, oy=0):
        self.w, self.h, self.ox, self.oy = w, h, ox, oy
        self.im = Image.new("RGBA", (w * S, h * S), CLEAR); self.d = ImageDraw.Draw(self.im)
    def P(self, pts): return [((x + self.ox) * S, (y + self.oy) * S) for x, y in pts]
    def B(self, b): return [(b[0] + self.ox) * S, (b[1] + self.oy) * S, (b[2] + self.ox) * S, (b[3] + self.oy) * S]
    def poly(self, pts, c, ow=4): self.d.polygon(self.P(pts), fill=rgba(c), outline=INK if ow else None, width=max(1, ow * S))
    def ell(self, b, c, ow=4): self.d.ellipse(self.B(b), fill=rgba(c), outline=INK if ow else None, width=max(1, ow * S))
    def rect(self, b, c, r=0, ow=4): self.d.rounded_rectangle(self.B(b), r * S, fill=rgba(c), outline=INK if ow else None, width=max(1, ow * S))
    def line(self, pts, c, w): self.d.line(self.P(pts), fill=rgba(c), width=int(w * S), joint="curve")
    # Trou transparent (le visage de base reste visible dessous)
    def cut_ell(self, b): self.d.ellipse(self.B(b), fill=CLEAR)
    def cut_poly(self, pts): self.d.polygon(self.P(pts), fill=CLEAR)
    def gem(self, cx, cy, r, c):
        self.poly([(cx, cy - r), (cx + r, cy), (cx, cy + r), (cx - r, cy)], c, 3)
    def spike(self, x, y, ang, ln, w, c):
        a = math.radians(ang)
        self.poly([(x + math.cos(a + 1.57) * w, y - math.sin(a + 1.57) * w), (x + math.cos(a) * ln, y - math.sin(a) * ln),
                   (x + math.cos(a - 1.57) * w, y - math.sin(a - 1.57) * w)], c, 3)
    def save(self, name, outline=6):
        a = self.im.getchannel("A")
        ol = a.filter(ImageFilter.MaxFilter(outline * S + 1))
        base = Image.new("RGBA", self.im.size, INK); base.putalpha(ol)
        # le contour extérieur ne doit pas boucher les trous du visage : on ne garde l'encre que là où il y a de la matière
        # ou juste autour (le trou reste transparent grâce au masque du calque).
        base.alpha_composite(self.im)
        base.resize((self.w, self.h), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

# =============== Base (chibi façon FM) ===============
# Tête : toile 280 x 440, cou en (140, 220). Crâne : ellipse [40, 24, 252, 232], visage tourné vers la droite.
HEAD = (40, 24, 252, 232)

def head():
    p = Pen(280, 440)
    p.rect([122, 200, 158, 240], SKIN_D, 8)
    p.ell(HEAD, SKIN)
    p.ell([52, 116, 88, 156], SKIN_D)
    for x0 in (170, 214):   # grands yeux ovales noirs avec reflet
        p.ell([x0, 110, x0 + 24, 150], (34, 24, 26), 0)
        p.ell([x0 + 5, 116, x0 + 13, 126], (255, 255, 255), 0)
    p.line([(168, 98), (194, 94)], HAIR_D, 6); p.line([(212, 94), (236, 98)], HAIR_D, 6)
    p.line([(196, 180), (210, 186), (224, 180)], (150, 60, 50), 5)
    p.ell([150, 156, 172, 170], (255, 160, 150), 0); p.ell([234, 156, 250, 170], (255, 160, 150), 0)
    p.save("head")

def hair_shape(p, ox=0, oy=0):
    pts = [(38, 140), (34, 92), (58, 46), (104, 20), (160, 16), (210, 30), (246, 66), (254, 100), (232, 86), (214, 100), (198, 78),
           (178, 98), (162, 74), (142, 96), (124, 76), (104, 108), (88, 152), (60, 172)]
    p.poly(pts, HAIR)
    p.poly([(58, 46), (104, 20), (160, 16), (130, 38), (96, 52)], (156, 92, 52), 0)

def hair():
    p = Pen(280, 440); hair_shape(p); p.save("hair")

def torso():
    # Toile 200 x 320, hanche en (100, 160) : torse en tonneau.
    p = Pen(200, 320)
    p.rect([46, 44, 154, 188], TUNIC, 40)
    p.rect([46, 44, 86, 188], TUNIC_D, 30, 0)
    p.poly([(76, 46), (124, 46), (100, 74)], SKIN)
    p.rect([42, 140, 158, 160], (90, 56, 34), 4); p.rect([88, 136, 112, 164], (240, 190, 70), 3)
    p.save("torso")

def arm():
    # Toile 100 x 260, épaule en (50, 130) ; main ronde en bas.
    p = Pen(100, 260)
    p.rect([32, 116, 68, 196], TUNIC, 18)
    p.ell([28, 180, 72, 224], SKIN)
    p.save("arm")

def leg():
    # Toile 120 x 200, hanche en (60, 100) ; botte vers l'avant (droite).
    p = Pen(120, 200)
    p.rect([42, 92, 78, 156], PANTS, 14)
    p.poly([(38, 148), (82, 148), (86, 174), (108, 180), (108, 196), (36, 196)], BOOT)
    p.save("leg")

# =============== Heaumes portés (toile 360 x 600, décalage +40, +80) ===============
def dome(p, m, low=104):
    # Calotte qui couvre le haut et l'arrière du crâne, s'arrête au front.
    p.poly([(34, low + 50), (30, 90), (60, 36), (110, 12), (170, 10), (222, 30), (254, 70), (260, low)] + [(34, low + 50)], m["M"])
    p.poly([(60, 36), (110, 12), (170, 10), (140, 34), (96, 46)], m["L"], 0)
    p.rect([28, low - 10, 262, low + 8], m["D"], 6)

def wh_cap(p, m): dome(p, m); p.rect([206, 100, 220, 168], m["D"], 4)
def wh_great(p, m):
    p.rect([28, 10, 262, 240], m["M"], 60); p.rect([28, 10, 120, 240], m["L"], 50, 0)
    p.rect([140, 116, 262, 140], INK, 6, 0); p.ell([178, 120, 194, 136], m["acc"], 0); p.ell([222, 120, 238, 136], m["acc"], 0)
    for y in (166, 186, 206): p.ell([226, y, 236, y + 8], m["D"], 0)
def wh_barbute(p, m):
    p.rect([28, 10, 262, 244], m["M"], 60); p.rect([28, 10, 120, 244], m["L"], 50, 0)
    p.cut_poly([(160, 104), (262, 104), (262, 158), (232, 158), (232, 214), (196, 214), (196, 158), (160, 158)])
    p.rect([26, 6, 264, 24], m["trim"], 8)
def wh_horned(p, m):
    for side in (0, 1):
        bx = 70 if side == 0 else 200
        p.poly([(bx - 20, 50), (bx - 50 + side * 100, -40), (bx - 40 + side * 80, -70), (bx - 10 + side * 20, -10), (bx + 20, 40)], m["trim"])
    dome(p, m)
def wh_hood(p, m):
    c = m["cloth"]
    p.poly([(140, -6), (234, 24), (272, 120), (262, 250), (24, 250), (16, 120), (48, 34)], c)
    p.poly([(140, -6), (48, 34), (16, 120), (24, 250), (76, 250), (66, 140)], shade(c, 1.15), 0)
    p.cut_ell([140, 80, 258, 218])
def wh_crown(p, m):
    hair_shape(p)
    p.poly([(60, 40), (70, -10), (100, 22), (140, -26), (180, 22), (214, -10), (226, 40)], m["trim"]); p.rect([58, 30, 228, 50], m["trim"], 6)
    p.gem(140, 10, 12, m["acc"])
def wh_skull(p, m):
    bone = BONE if m["motif"] != "wing" else m["M"]
    p.poly([(30, 140), (26, 70), (70, 10), (160, 0), (230, 20), (268, 70), (270, 104), (40, 112)], bone)
    p.ell([150, 40, 190, 80], INK, 0); p.ell([206, 40, 246, 80], INK, 0); p.ell([162, 52, 178, 68], m["acc"], 0); p.ell([218, 52, 234, 68], m["acc"], 0)
    for x in range(150, 266, 18): p.poly([(x, 100), (x + 14, 100), (x + 7, 122)], bone, 3)
def wh_mitre(p, m):
    c = m["cloth"]
    p.poly([(44, 100), (60, -40), (140, -78), (220, -40), (244, 100)], c); p.poly([(60, -40), (140, -78), (130, 100), (44, 100)], shade(c, 1.2), 0)
    p.rect([40, 84, 248, 108], m["trim"], 6); p.rect([132, -60, 148, 90], m["trim"], 3)
def wh_jester(p, m):
    p.rect([34, 50, 256, 108], m["M"], 26)
    for s, c, tip in ((0, m["cloth"], (-30, 140)), (1, m["D"], (310, 120))):
        x = 80 if s == 0 else 200
        p.poly([(x - 40, 60), (x, -30), (tip[0], tip[1] - 140), (tip[0] + (20 if s == 0 else -20), tip[1] - 90), (x + 40, 60)], c)
        p.ell([tip[0] - 14, tip[1] - 154, tip[0] + 14, tip[1] - 126], m["trim"], 3)
def wh_winged(p, m):
    dome(p, m)
    p.poly([(60, 70), (-30, 10), (-10, 50), (-36, 70), (-4, 96), (20, 120), (60, 110)], m["trim"])
def wh_plumed(p, m):
    p.poly([(120, 20), (100, -40), (150, -76), (220, -70), (260, -30), (220, -30), (170, 20)], m["acc"])
    dome(p, m)
def wh_mask(p, m):
    hair_shape(p)
    p.poly([(150, 84), (262, 84), (262, 170), (236, 214), (180, 214), (150, 170)], (248, 240, 228) if m["motif"] != "wing" else m["M"])
    p.ell([174, 112, 198, 138], INK, 0); p.ell([218, 112, 242, 138], INK, 0); p.line([(186, 186), (206, 196), (230, 186)], INK, 4)
    p.rect([140, 80, 266, 96], m["cloth"], 6)

HELMS = {"Casque": wh_cap, "Heaume": wh_great, "Barbute": wh_barbute, "Heaume cornu": wh_horned, "Capuche": wh_hood,
         "Couronne": wh_crown, "Crâne": wh_skull, "Mitre": wh_mitre, "Bonnet de bouffon": wh_jester,
         "Heaume ailé": wh_winged, "Heaume à plumet": wh_plumed, "Masque": wh_mask}

# =============== Gants portés (toile du bras, main en [28,180,72,224]) ===============
def wg_gauntlet(p, m): p.rect([30, 168, 70, 196], m["D"], 8); p.ell([26, 178, 74, 226], m["M"]); p.ell([34, 184, 52, 200], m["L"], 0)
def wg_glove(p, m): p.rect([30, 166, 70, 194], m["trim"], 8); p.ell([26, 178, 74, 226], m["cloth"])
def wg_claw(p, m):
    p.ell([26, 178, 74, 226], m["M"])
    for x in (36, 50, 64): p.poly([(x - 6, 216), (x + 6, 216), (x, 246)], m["L"], 3)
def wg_bracer(p, m): p.rect([28, 150, 72, 196], m["M"], 10); p.rect([28, 166, 72, 176], m["trim"], 3); p.ell([28, 180, 72, 224], SKIN)
def wg_wraps(p, m):
    p.ell([28, 180, 72, 224], (236, 226, 204))
    for y in (186, 200, 214): p.line([(30, y), (70, y + 4)], (200, 186, 160), 3)
def wg_spiked(p, m):
    p.rect([30, 166, 70, 194], m["D"], 8); p.ell([26, 178, 74, 226], m["M"])
    for a in (200, 270, 340): p.spike(50 + math.cos(math.radians(a)) * 22, 202 - math.sin(math.radians(a)) * 22, a, 14, 6, m["L"])

GLOVES = {"Gantelets": wg_gauntlet, "Gants": wg_glove, "Griffes": wg_claw, "Brassards": wg_bracer, "Bandages": wg_wraps, "Poings cloutés": wg_spiked}

# =============== Bottes portées (toile de la jambe) ===============
def boot_base(p, c, top=134):
    p.poly([(36, top), (84, top), (86, 172), (110, 178), (112, 198), (34, 198)], c)
def wb_boot(p, m): boot_base(p, m["M"]); p.rect([34, 128, 86, 144], m["L"], 6)
def wb_sabaton(p, m):
    boot_base(p, m["M"], 120)
    for y in (132, 152): p.rect([34, y, 88, y + 10], m["L"], 3, 3)
def wb_curly(p, m):
    p.poly([(36, 134), (84, 134), (86, 170), (110, 168), (120, 150), (124, 176), (112, 198), (34, 198)], m["cloth"]); p.ell([116, 140, 130, 154], m["trim"], 3)
def wb_sandal(p, m):
    p.poly([(40, 160), (80, 160), (84, 178), (108, 184), (108, 196), (38, 196)], SKIN)
    p.rect([34, 190, 112, 200], m["grip"], 4)
    for y in (150, 168): p.rect([38, y, 86, y + 6], m["M"], 3, 2)
def wb_fur(p, m):
    boot_base(p, m["M"], 140)
    for k in range(4): p.ell([30 + k * 14, 124, 52 + k * 14, 148], (240, 232, 220) if m["motif"] != "flame" else (90, 80, 80), 3)
def wb_winged(p, m):
    boot_base(p, m["M"])
    p.poly([(36, 150), (4, 128), (10, 146), (0, 158), (14, 166), (36, 168)], m["trim"])

BOOTS = {"Bottes": wb_boot, "Solerets": wb_sabaton, "Poulaines": wb_curly, "Sandales": wb_sandal, "Bottes fourrées": wb_fur, "Bottes ailées": wb_winged}

# =============== Ceintures portées (toile du torse, ceinture en y 140-160) ===============
def belt_strap(p, c): p.rect([42, 140, 158, 160], c, 4)
def wbe_round(p, m): belt_strap(p, m["grip"]); p.ell([84, 132, 116, 168], m["M"]); p.gem(100, 150, 8, m["acc"])
def wbe_square(p, m): belt_strap(p, m["grip"]); p.rect([84, 134, 116, 166], m["M"], 6); p.rect([92, 142, 108, 158], m["grip"], 3, 3)
def wbe_skull(p, m): belt_strap(p, m["grip"]); p.ell([82, 128, 118, 160], BONE); p.ell([88, 138, 98, 148], m["acc"], 0); p.ell([102, 138, 112, 148], m["acc"], 0)
def wbe_rope(p, m):
    for k in range(9): p.ell([40 + k * 13, 140, 56 + k * 13, 160], (190, 146, 90) if k % 2 else (166, 120, 70), 3)
    p.line([(100, 158), (94, 196)], (176, 130, 80), 6)
def wbe_chain(p, m):
    for k in range(8): p.ell([40 + k * 15, 140 + (k % 2) * 3, 60 + k * 15, 158 + (k % 2) * 3], m["M"], 3)
def wbe_sash(p, m): belt_strap(p, m["cloth"]); p.poly([(100, 150), (130, 166), (138, 206), (116, 200), (108, 164)], shade(m["cloth"], 0.85))
def wbe_pouch(p, m): belt_strap(p, m["grip"]); p.rect([118, 150, 150, 184], m["grip"], 8); p.rect([118, 150, 150, 162], shade(m["grip"], 1.2), 6)

BELTS = {"Ceinture": wbe_round, "Ceinturon": wbe_square, "Sangle au crâne": wbe_skull, "Corde": wbe_rope, "Chaîne": wbe_chain,
         "Écharpe": wbe_sash, "Ceinture à bourses": wbe_pouch}

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    head(); hair(); torso(); arm(); leg()
    for c in range(10):
        m = MAT[c]
        for v, fam in enumerate(SETS[1][c]):
            p = Pen(360, 600, 40, 80); HELMS[fam](p, m); p.save("helm_c%d_v%d" % (c, v))
        for v, fam in enumerate(SETS[3][c]):
            p = Pen(100, 260); GLOVES[fam](p, m); p.save("glove_c%d_v%d" % (c, v), 5)
        for v, fam in enumerate(SETS[4][c]):
            p = Pen(120, 200); BOOTS[fam](p, m); p.save("boot_c%d_v%d" % (c, v), 5)
        for v, fam in enumerate(SETS[5][c]):
            p = Pen(200, 320); BELTS[fam](p, m); p.save("belt_c%d_v%d" % (c, v), 5)
    print("ok")
