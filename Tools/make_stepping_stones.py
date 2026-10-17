# Éléments du « Pas japonais » (Stepping Stones de Forge Master) : rivière de lave, pierres et rive, en aplats à contours noirs.
# Sortie : Assets/Resources/ForgeUI/Event/{lava,stone,stone_gold,shore}.png
# Usage : python Tools/make_stepping_stones.py
import math, os, random
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Event")
INK = (28, 16, 16, 255)
S = 2

def outline(layer, w):
    a = layer.getchannel("A").filter(ImageFilter.MaxFilter(w * 2 + 1))
    base = Image.new("RGBA", layer.size, INK); base.putalpha(a); base.alpha_composite(layer); return base

def lava():
    W, H = 720, 1000
    im = Image.new("RGBA", (W * S, H * S), (230, 80, 20, 255)); d = ImageDraw.Draw(im)
    rnd = random.Random(4)
    for _ in range(40):   # croûtes sombres
        cx, cy, r = rnd.uniform(0, W), rnd.uniform(0, H), rnd.uniform(30, 90)
        pts = [(cx + math.cos(a) * r * rnd.uniform(0.7, 1.2), cy + math.sin(a) * r * 0.6 * rnd.uniform(0.7, 1.2)) for a in [k * math.tau / 10 for k in range(10)]]
        d.polygon([(x * S, y * S) for x, y in pts], fill=(160, 40, 16, 255))
    for _ in range(60):   # veines claires
        x, y = rnd.uniform(0, W), rnd.uniform(0, H)
        d.line([(x * S, y * S), ((x + rnd.uniform(-60, 60)) * S, (y + rnd.uniform(-30, 30)) * S)], fill=(255, 180, 60, 255), width=5 * S)
    for _ in range(26):   # bulles
        x, y, r = rnd.uniform(0, W), rnd.uniform(0, H), rnd.uniform(6, 16)
        d.ellipse([(x - r) * S, (y - r) * S, (x + r) * S, (y + r) * S], fill=(255, 210, 90, 255), outline=INK, width=3 * S)
        d.ellipse([(x - r * 0.5) * S, (y - r * 0.6) * S, (x - r * 0.1) * S, (y - r * 0.2) * S], fill=(255, 250, 200, 255))
    im.resize((W, H), Image.LANCZOS).convert("RGB").save(os.path.join(OUT, "lava.png"))

def stone(name, top, side, w=220, h=150):
    lay = Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0)); d = ImageDraw.Draw(lay)
    def P(pts): return [(x * S, y * S) for x, y in pts]
    d.ellipse(P([(12, 40), (w - 12, h - 10)]), fill=side)                     # flanc
    d.ellipse(P([(12, 18), (w - 12, h - 34)]), fill=top)                      # dessus
    d.ellipse(P([(40, 30), (w * 0.55, h * 0.42)]), fill=tuple(min(255, int(v * 1.18)) for v in top[:3]) + (255,))
    d.line(P([(w * 0.6, h * 0.35), (w * 0.72, h * 0.5), (w * 0.66, h * 0.62)]), fill=INK, width=3 * S)
    lay = outline(lay, 5 * S)
    lay.resize((w, h), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    lava()
    stone("stone", (150, 136, 128, 255), (96, 82, 78, 255))
    stone("stone_gold", (255, 214, 90, 255), (190, 130, 40, 255))
    stone("shore", (120, 92, 76, 255), (78, 58, 48, 255), 320, 170)
    print("ok")
