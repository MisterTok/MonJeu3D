# Héros en pièces détachées façon Forge Master (animé par code dans SpriteChar, équipement visible).
# Chaque pièce est dessinée avec son articulation au CENTRE de l'image ; échelle commune : 400 px = taille du héros.
# Sortie : Assets/Resources/ForgeUI/Hero/{head,hair,torso,arm,leg}.png
# Usage : python Tools/make_paper_hero.py
import os
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Hero")
S = 4
INK = (28, 16, 16, 255)
SKIN, SKIN_D = (250, 206, 166, 255), (226, 168, 128, 255)
HAIR, HAIR_D = (120, 66, 36, 255), (88, 46, 24, 255)
TUNIC, TUNIC_D = (196, 52, 46, 255), (150, 34, 32, 255)
PANTS = (70, 58, 76, 255)
BOOT, BOOT_D = (110, 66, 40, 255), (80, 46, 28, 255)
BELT, GOLD = (90, 56, 34, 255), (240, 190, 70, 255)

class Pen:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.im = Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0)); self.d = ImageDraw.Draw(self.im)
    def P(self, pts): return [(x * S, y * S) for x, y in pts]
    def poly(self, pts, c, ow=4): self.d.polygon(self.P(pts), fill=c, outline=INK if ow else None, width=max(1, ow * S))
    def ell(self, b, c, ow=4): self.d.ellipse([v * S for v in b], fill=c, outline=INK if ow else None, width=max(1, ow * S))
    def rect(self, b, c, r=0, ow=4): self.d.rounded_rectangle([v * S for v in b], r * S, fill=c, outline=INK if ow else None, width=max(1, ow * S))
    def line(self, pts, c, w): self.d.line(self.P(pts), fill=c, width=int(w * S), joint="curve")
    def save(self, name, outline=6):
        a = self.im.getchannel("A")
        ol = a.filter(ImageFilter.MaxFilter(outline * S + 1))
        base = Image.new("RGBA", self.im.size, INK); base.putalpha(ol)
        base.alpha_composite(self.im)
        base.resize((self.w, self.h), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

def head():
    # Toile 280 x 440, cou au centre (140, 220) ; tête vers la droite, grands yeux.
    p = Pen(280, 440)
    p.rect([122, 196, 158, 236], SKIN_D, 8)                 # cou
    p.ell([44, 28, 248, 226], SKIN)                          # crâne
    p.ell([52, 110, 86, 150], SKIN_D)                        # oreille
    p.ell([150, 104, 196, 160], (255, 255, 255, 255), 4)     # oeil avant
    p.ell([166, 116, 192, 148], (40, 30, 30, 255), 0)
    p.ell([174, 120, 184, 130], (255, 255, 255, 255), 0)
    p.ell([206, 106, 240, 158], (255, 255, 255, 255), 4)     # oeil arrière (coupé par le profil)
    p.ell([218, 118, 238, 146], (40, 30, 30, 255), 0)
    p.line([(148, 96), (196, 90)], HAIR_D, 7); p.line([(208, 90), (238, 94)], HAIR_D, 7)   # sourcils
    p.line([(186, 186), (206, 192), (224, 184)], (150, 60, 50, 255), 5)                      # bouche
    p.ell([136, 160, 162, 178], (255, 160, 150, 255), 0); p.ell([226, 160, 244, 176], (255, 160, 150, 255), 0)
    p.save("head")

def hair():
    p = Pen(280, 440)
    pts = [(40, 140), (36, 90), (60, 46), (104, 22), (160, 18), (210, 34), (244, 70), (250, 104), (226, 88), (210, 104), (196, 80),
           (176, 100), (160, 76), (140, 98), (122, 78), (100, 108), (84, 150), (60, 170)]
    p.poly(pts, HAIR)
    p.poly([(60, 46), (104, 22), (160, 18), (130, 40), (96, 54)], (156, 92, 52, 255), 0)
    p.save("hair")

def torso():
    # Toile 200 x 320, hanche au centre (100, 160).
    p = Pen(200, 320)
    p.poly([(52, 40), (148, 40), (160, 176), (40, 176)], TUNIC)
    p.poly([(52, 40), (78, 40), (66, 176), (40, 176)], TUNIC_D, 0)
    p.poly([(72, 40), (128, 40), (100, 76)], SKIN)                      # col
    p.rect([38, 140, 162, 162], BELT, 4); p.rect([88, 136, 112, 166], GOLD, 3)
    p.poly([(40, 176), (160, 176), (166, 196), (34, 196)], TUNIC_D)       # bas de tunique
    p.save("torso")

def arm():
    # Toile 100 x 260, épaule au centre (50, 130) ; bras tendu vers le bas, main en bas.
    p = Pen(100, 260)
    p.rect([30, 118, 70, 210], TUNIC, 18)
    p.rect([32, 196, 68, 226], SKIN_D, 10)
    p.ell([28, 214, 72, 252], SKIN)
    p.save("arm")

def leg():
    # Toile 120 x 200, hanche au centre (60, 100) ; botte vers l'avant (droite).
    p = Pen(120, 200)
    p.rect([42, 92, 78, 160], PANTS, 12)
    p.poly([(38, 150), (82, 150), (86, 176), (110, 182), (110, 196), (36, 196)], BOOT)
    p.rect([36, 146, 84, 160], BOOT_D, 4)
    p.save("leg")

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    head(); hair(); torso(); arm(); leg()
    print("ok")
