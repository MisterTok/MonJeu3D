# Zone de la forge façon Forge Master : fond (mur, fournaise, torches, sol), enclume sur souche et marteau, en aplats à contours noirs.
# Sortie : Assets/Resources/ForgeUI/Forge/{forge_bg,anvil,hammer}.png
# Usage : python Tools/make_forge_flat.py
import math, os, random
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Forge")
INK = (28, 16, 16, 255)
S = 2

def sc(pts): return [(x * S, y * S) for x, y in pts]
def shade(c, k): return tuple(max(0, min(255, int(v * k))) for v in c[:3]) + (255,)

def outlined(base, layer, width):
    """Colle layer sur base avec un contour noir autour de sa silhouette."""
    a = layer.getchannel("A")
    ol = a.filter(ImageFilter.MaxFilter(width * 2 + 1))
    ink = Image.new("RGBA", layer.size, INK); ink.putalpha(ol)
    base.alpha_composite(ink); base.alpha_composite(layer)

def flame(d, cx, by, w, h, cols):
    for i, c in enumerate(cols):
        k = 1 - i * 0.28
        ww, hh = w * k, h * k
        pts = [(cx - ww / 2, by), (cx - ww * 0.45, by - hh * 0.45), (cx - ww * 0.18, by - hh * 0.6), (cx - ww * 0.1, by - hh * 0.85),
               (cx + ww * 0.05, by - hh), (cx + ww * 0.15, by - hh * 0.7), (cx + ww * 0.3, by - hh * 0.8), (cx + ww * 0.48, by - hh * 0.4), (cx + ww / 2, by)]
        d.polygon(sc(pts), fill=c)

# ---------------- Fond ----------------
def background():
    W, H = 1400, 1000
    im = Image.new("RGBA", (W * S, H * S), (74, 34, 30, 255))
    d = ImageDraw.Draw(im)
    rnd = random.Random(3)
    floor_y = 700
    # Briques
    bh, bw = 56, 120
    for row in range(0, floor_y // bh + 1):
        y = row * bh
        off = (bw // 2) if row % 2 else 0
        for x in range(-bw, W + bw, bw):
            x0 = x + off
            c = (74, 34, 30, 255) if rnd.random() > 0.25 else (84, 40, 34, 255)
            d.rectangle(sc([(x0 + 3, y + 3), (x0 + bw - 3, y + bh - 3)]), fill=c)
            d.rectangle(sc([(x0 + 3, y + 3), (x0 + bw - 3, y + 10)]), fill=shade(c, 1.15))
        d.line(sc([(0, y), (W, y)]), fill=(44, 18, 16, 255), width=5 * S)
        for x in range(-bw, W + bw, bw):
            d.line(sc([(x + off, y), (x + off, y + bh)]), fill=(44, 18, 16, 255), width=5 * S)
    # Fournaise en arche au centre
    cx, top, base = W / 2, 150, floor_y + 10
    arch = Image.new("RGBA", im.size, (0, 0, 0, 0)); ad = ImageDraw.Draw(arch)
    ad.rectangle(sc([(cx - 330, top + 180), (cx + 330, base)]), fill=(108, 78, 70, 255))
    ad.ellipse(sc([(cx - 330, top - 20), (cx + 330, top + 400)]), fill=(108, 78, 70, 255))
    outlined(im, arch, 8 * S)
    d = ImageDraw.Draw(im)
    # Pierres de l'arche
    for k in range(9):
        a = math.pi + k * math.pi / 8
        x0, y0 = cx + math.cos(a) * 250, top + 190 + math.sin(a) * 200
        x1, y1 = cx + math.cos(a) * 326, top + 190 + math.sin(a) * 206
        d.line(sc([(x0, y0), (x1, y1)]), fill=INK, width=6 * S)
    # Bouche de la fournaise : feu en aplats
    mouth = Image.new("RGBA", im.size, (0, 0, 0, 0)); md = ImageDraw.Draw(mouth)
    md.rectangle(sc([(cx - 240, top + 200), (cx + 240, base)]), fill=(255, 120, 30, 255))
    md.ellipse(sc([(cx - 240, top + 70), (cx + 240, top + 330)]), fill=(255, 120, 30, 255))
    outlined(im, mouth, 7 * S)
    d = ImageDraw.Draw(im)
    d.ellipse(sc([(cx - 200, top + 140), (cx + 200, base + 100)]), fill=(255, 170, 50, 255))
    for fx, fw, fh in [(cx - 140, 150, 260), (cx, 210, 380), (cx + 140, 160, 280)]:
        flame(d, fx, base, fw, fh, [(255, 90, 30, 255), (255, 190, 60, 255), (255, 245, 190, 255)])
    # Torches murales
    for tx in (190, W - 190):
        torch = Image.new("RGBA", im.size, (0, 0, 0, 0)); td = ImageDraw.Draw(torch)
        td.polygon(sc([(tx - 24, 360), (tx + 24, 360), (tx + 12, 450), (tx - 12, 450)]), fill=(90, 80, 86, 255))
        td.rectangle(sc([(tx - 8, 450), (tx + 8, 520)]), fill=(110, 64, 34, 255))
        outlined(im, torch, 6 * S)
        d = ImageDraw.Draw(im)
        flame(d, tx, 366, 80, 150, [(255, 90, 30, 255), (255, 190, 60, 255), (255, 245, 190, 255)])
    # Râtelier d'armes (gauche) et bouclier (droite)
    rack = Image.new("RGBA", im.size, (0, 0, 0, 0)); rd = ImageDraw.Draw(rack)
    for k, x in enumerate((330, 380)):
        rd.polygon(sc([(x - 9, 600), (x + 9, 600), (x + 9, 330), (x, 300), (x - 9, 330)]), fill=(196, 200, 214, 255))
        rd.rectangle(sc([(x - 28, 600), (x + 28, 612)]), fill=(214, 168, 64, 255))
        rd.rectangle(sc([(x - 7, 612), (x + 7, 660)]), fill=(110, 56, 30, 255))
    rd.ellipse(sc([(W - 420, 380), (W - 280, 540)]), fill=(150, 40, 34, 255))
    rd.ellipse(sc([(W - 380, 425), (W - 320, 495)]), fill=(214, 168, 64, 255))
    outlined(im, rack, 6 * S)
    d = ImageDraw.Draw(im)
    # Sol en dalles
    d.rectangle(sc([(0, floor_y), (W, H)]), fill=(96, 62, 54, 255))
    for k, y in enumerate((floor_y, 780, 880)):
        d.line(sc([(0, y), (W, y)]), fill=INK if k == 0 else (60, 36, 32, 255), width=(8 if k == 0 else 5) * S)
    for k in range(-8, 9):
        d.line(sc([(cx + k * 110, floor_y), (cx + k * 260, H)]), fill=(60, 36, 32, 255), width=5 * S)
    # Lueur de la fournaise sur le sol
    glow = Image.new("RGBA", im.size, (0, 0, 0, 0)); gd = ImageDraw.Draw(glow)
    gd.ellipse(sc([(cx - 420, floor_y - 30), (cx + 420, floor_y + 170)]), fill=(255, 140, 50, 90))
    im.alpha_composite(glow)
    im.resize((W, H), Image.LANCZOS).convert("RGB").save(os.path.join(OUT, "forge_bg.png"))

# ---------------- Enclume ----------------
def anvil():
    W, H = 560, 420
    S4 = 4
    def p(pts): return [(x * S4, y * S4) for x, y in pts]
    lay = Image.new("RGBA", (W * S4, H * S4), (0, 0, 0, 0)); d = ImageDraw.Draw(lay)
    # Souche
    wood, woodD = (150, 86, 46, 255), (112, 60, 30, 255)
    d.rectangle(p([(150, 250), (410, 400)]), fill=wood)
    d.ellipse(p([(150, 385), (410, 415)]), fill=woodD)
    d.ellipse(p([(150, 232), (410, 268)]), fill=(196, 132, 80, 255))
    for x in (190, 250, 330, 380): d.line(p([(x, 270), (x - 6, 395)]), fill=woodD, width=4 * S4)
    for y in (290, 360): d.rectangle(p([(146, y), (414, y + 14)]), fill=(80, 78, 88, 255))
    # Corps de l'enclume : corne à gauche, table, taille, pied
    steel, light, dark = (124, 128, 146, 255), (196, 200, 216, 255), (84, 86, 102, 255)
    body = [(40, 70), (430, 70), (500, 82), (500, 120), (420, 132), (360, 140), (330, 200), (400, 236), (160, 236), (230, 200), (200, 140), (150, 132), (80, 112)]
    d.polygon(p(body), fill=steel)
    d.polygon(p([(40, 70), (430, 70), (500, 82), (500, 96), (60, 92)]), fill=light)
    d.polygon(p([(330, 200), (400, 236), (160, 236), (230, 200)]), fill=dark)
    d.polygon(p([(360, 140), (330, 200), (300, 200), (330, 140)]), fill=dark)
    # Lingot chauffé
    d.rounded_rectangle(p([(250, 46), (390, 72)]), 6 * S4, fill=(255, 150, 40, 255))
    d.rounded_rectangle(p([(262, 50), (378, 60)]), 4 * S4, fill=(255, 240, 180, 255))
    out = Image.new("RGBA", lay.size, (0, 0, 0, 0))
    outlined(out, lay, 9 * S4 // 2 * 2)
    out.resize((W, H), Image.LANCZOS).save(os.path.join(OUT, "anvil.png"))

# ---------------- Marteau (pivot à droite, au bout du manche) ----------------
def hammer():
    W, H = 360, 170
    S4 = 4
    def p(pts): return [(x * S4, y * S4) for x, y in pts]
    lay = Image.new("RGBA", (W * S4, H * S4), (0, 0, 0, 0)); d = ImageDraw.Draw(lay)
    d.rounded_rectangle(p([(90, 72), (350, 98)]), 10 * S4, fill=(150, 88, 46, 255))
    d.rounded_rectangle(p([(90, 74), (350, 82)]), 4 * S4, fill=(196, 132, 80, 255))
    d.rounded_rectangle(p([(14, 14), (100, 156)]), 10 * S4, fill=(124, 128, 146, 255))
    d.rounded_rectangle(p([(14, 14), (40, 156)]), 8 * S4, fill=(196, 200, 216, 255))
    d.rectangle(p([(96, 64), (116, 106)]), fill=(214, 168, 64, 255))
    out = Image.new("RGBA", lay.size, (0, 0, 0, 0))
    outlined(out, lay, 8 * S4 // 2 * 2)
    out.resize((W, H), Image.LANCZOS).save(os.path.join(OUT, "hammer.png"))

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    background(); anvil(); hammer()
    print("ok")
