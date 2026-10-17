# Génère le skin d'interface (style Forge Master, thème forge de l'enfer) dans Assets/Resources/ForgeUI.
# Tout est dessiné par code (PIL), suréchantillonné x4 puis réduit : boutons bombés, cadres, tuiles, bandeaux, icônes.
# Usage : python Tools/make_ui_skin.py
import math, os, random
from PIL import Image, ImageDraw, ImageFilter, ImageChops

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI")
SS = 4  # suréchantillonnage

def new(w, h, c=(0, 0, 0, 0)): return Image.new("RGBA", (w, h), c)
def L(w, h): return Image.new("L", (w, h), 0)

def rrect_mask(w, h, r, inset=0):
    m = L(w, h); ImageDraw.Draw(m).rounded_rectangle([inset, inset, w - 1 - inset, h - 1 - inset], max(1, r - inset), fill=255); return m

def vgrad(w, h, stops):
    """Dégradé vertical ; stops = [(t, (r,g,b,a)), ...]."""
    im = new(1, h); px = im.load()
    for y in range(h):
        t = y / max(1, h - 1)
        for i in range(len(stops) - 1):
            if stops[i][0] <= t <= stops[i + 1][0]:
                a, ca = stops[i]; b, cb = stops[i + 1]
                k = (t - a) / max(1e-6, b - a)
                c = tuple(int(ca[j] + (cb[j] - ca[j]) * k) for j in range(4)); break
        px[0, y] = c
    return im.resize((w, h), Image.NEAREST)

def rgrad(w, h, inner, outer, cx=0.5, cy=0.5, rad=0.7):
    im = new(w, h); px = im.load()
    for y in range(h):
        for x in range(w):
            d = math.hypot(x / w - cx, y / h - cy) / rad; k = min(1, d)
            px[x, y] = tuple(int(inner[j] + (outer[j] - inner[j]) * k) for j in range(4))
    return im

def masked(src, mask):
    out = src.copy(); a = ImageChops.multiply(out.getchannel("A"), mask); out.putalpha(a); return out

def over(dst, src): dst.alpha_composite(src); return dst

def dilate(mask, px):
    return mask.filter(ImageFilter.MaxFilter(px * 2 + 1)) if px > 0 else mask

def solid(w, h, c, mask): return masked(new(w, h, c), mask)

def noise(w, h, amp, seed=1):
    random.seed(seed); im = Image.effect_noise((w, h), amp).convert("L"); return im

def save(im, name, size=None):
    if size: im = im.resize(size, Image.LANCZOS)
    im.save(os.path.join(OUT, name + ".png")); print("ok", name, im.size)

# ---------------- Éléments 9-slice ----------------
def button():
    # Gris clair bombé (teinté par Image.color) : liseré sombre, lèvre inférieure (épaisseur), reflet en haut.
    n, r = 128 * SS, 30 * SS
    im = new(n, n)
    over(im, solid(n, n, (20, 12, 10, 255), rrect_mask(n, n, r)))                      # contour
    lip = rrect_mask(n, n, r, 4 * SS)
    over(im, solid(n, n, (120, 120, 120, 255), lip))                                     # épaisseur
    body = L(n, n); ImageDraw.Draw(body).rounded_rectangle([4 * SS, 4 * SS, n - 1 - 4 * SS, n - 1 - 14 * SS], r - 4 * SS, fill=255)
    over(im, masked(vgrad(n, n, [(0, (255, 255, 255, 255)), (0.55, (225, 225, 225, 255)), (1, (178, 178, 178, 255))]), body))
    hi = L(n, n); ImageDraw.Draw(hi).rounded_rectangle([12 * SS, 9 * SS, n - 1 - 12 * SS, 40 * SS], 16 * SS, fill=110)
    over(im, solid(n, n, (255, 255, 255, 255), hi.filter(ImageFilter.GaussianBlur(3 * SS))))
    save(im, "btn", (128, 128))

def tab():
    n, r = 96 * SS, 22 * SS
    im = new(n, n)
    over(im, solid(n, n, (16, 10, 9, 255), rrect_mask(n, n, r)))
    body = rrect_mask(n, n, r, 4 * SS)
    over(im, masked(vgrad(n, n, [(0, (255, 255, 255, 255)), (1, (170, 170, 170, 255))]), body))
    hi = L(n, n); ImageDraw.Draw(hi).rounded_rectangle([10 * SS, 8 * SS, n - 1 - 10 * SS, 26 * SS], 10 * SS, fill=90)
    over(im, solid(n, n, (255, 255, 255, 255), hi.filter(ImageFilter.GaussianBlur(2 * SS))))
    save(im, "tab", (96, 96))

def panel():
    # Fond de carte : pierre sombre, bord bronze biseauté. Non teinté.
    n, r = 128 * SS, 26 * SS
    im = new(n, n)
    over(im, solid(n, n, (14, 8, 7, 255), rrect_mask(n, n, r)))
    over(im, masked(vgrad(n, n, [(0, (226, 168, 92, 255)), (0.5, (150, 88, 40, 255)), (1, (96, 52, 26, 255))]), rrect_mask(n, n, r, 3 * SS)))
    inner = rrect_mask(n, n, r, 9 * SS)
    over(im, solid(n, n, (22, 12, 10, 255), inner))
    over(im, masked(vgrad(n, n, [(0, (62, 30, 24, 255)), (1, (34, 16, 14, 255))]), rrect_mask(n, n, r, 11 * SS)))
    save(im, "panel", (128, 128))

def card():
    # Carte intérieure plus sobre (listes, fiches).
    n, r = 96 * SS, 20 * SS
    im = new(n, n)
    over(im, solid(n, n, (10, 6, 5, 230), rrect_mask(n, n, r)))
    over(im, masked(vgrad(n, n, [(0, (70, 36, 28, 255)), (1, (40, 20, 17, 255))]), rrect_mask(n, n, r, 3 * SS)))
    hi = L(n, n); ImageDraw.Draw(hi).rounded_rectangle([3 * SS, 3 * SS, n - 1 - 3 * SS, n - 1 - 3 * SS], r - 3 * SS, outline=255, width=2 * SS)
    over(im, solid(n, n, (130, 72, 48, 160), hi))
    save(im, "card", (96, 96))

def frame():
    # Cadre de fenêtre (teinté par la couleur de rareté / du cercle) : double liseré clair biseauté.
    n, r = 160 * SS, 34 * SS
    im = new(n, n)
    over(im, solid(n, n, (18, 12, 10, 255), rrect_mask(n, n, r)))
    over(im, masked(vgrad(n, n, [(0, (255, 255, 255, 255)), (0.5, (205, 205, 205, 255)), (1, (150, 150, 150, 255))]), rrect_mask(n, n, r, 4 * SS)))
    ring = rrect_mask(n, n, r, 11 * SS)
    over(im, solid(n, n, (60, 60, 60, 255), ring))
    over(im, masked(vgrad(n, n, [(0, (235, 235, 235, 255)), (1, (175, 175, 175, 255))]), rrect_mask(n, n, r, 13 * SS)))
    save(im, "frame", (160, 160))

def tile():
    # Tuile d'objet teintée par la rareté : dégradé + vignette + lueur centrale, liseré clair.
    n, r = 128 * SS, 22 * SS
    im = new(n, n)
    over(im, solid(n, n, (12, 8, 8, 255), rrect_mask(n, n, r)))
    over(im, masked(vgrad(n, n, [(0, (255, 255, 255, 255)), (1, (190, 190, 190, 255))]), rrect_mask(n, n, r, 3 * SS)))
    inner = rrect_mask(n, n, r, 8 * SS)
    over(im, masked(rgrad(n, n, (215, 215, 215, 255), (95, 95, 95, 255), 0.5, 0.42, 0.75), inner))
    gl = L(n, n); ImageDraw.Draw(gl).ellipse([n * 0.18, n * 0.12, n * 0.82, n * 0.7], fill=80)
    over(im, masked(solid(n, n, (255, 255, 255, 255), gl.filter(ImageFilter.GaussianBlur(14 * SS))), inner))
    save(im, "tile", (128, 128))

def pill():
    n, r = 64 * SS, 30 * SS
    im = new(n, n)
    over(im, solid(n, n, (0, 0, 0, 255), rrect_mask(n, n, r)))
    over(im, masked(vgrad(n, n, [(0, (40, 22, 20, 255)), (1, (18, 9, 9, 255))]), rrect_mask(n, n, r, 3 * SS)))
    hi = L(n, n); ImageDraw.Draw(hi).rounded_rectangle([3 * SS, 3 * SS, n - 1 - 3 * SS, n - 1 - 3 * SS], r - 3 * SS, outline=255, width=2 * SS)
    over(im, solid(n, n, (140, 84, 50, 200), hi))
    save(im, "pill", (64, 64))

def bar():
    # Barre de progression (fond sombre creusé) et remplissage (gris bombé, teinté).
    n, r = 48 * SS, 22 * SS
    im = new(n, n)
    over(im, solid(n, n, (0, 0, 0, 255), rrect_mask(n, n, r)))
    over(im, masked(vgrad(n, n, [(0, (8, 4, 4, 255)), (1, (36, 22, 20, 255))]), rrect_mask(n, n, r, 3 * SS)))
    save(im, "bar_bg", (48, 48))
    im = new(n, n)
    over(im, masked(vgrad(n, n, [(0, (255, 255, 255, 255)), (0.45, (230, 230, 230, 255)), (1, (150, 150, 150, 255))]), rrect_mask(n, n, r)))
    save(im, "bar_fill", (48, 48))

def banner():
    # Bandeau de titre : ruban rouge sombre bordé d'or, pointes aux extrémités.
    w, h = 512 * SS, 110 * SS
    im = new(w, h)
    def ribbon(inset):
        m = L(w, h); d = ImageDraw.Draw(m)
        e = 46 * SS
        pts = [(inset + 0, h * 0.18 + inset), (e, h * 0.18 + inset), (e + 10 * SS, inset), (w - e - 10 * SS, inset), (w - e, h * 0.18 + inset),
               (w - inset, h * 0.18 + inset), (w - 26 * SS - inset, h / 2), (w - inset, h * 0.82 - inset), (w - e, h * 0.82 - inset),
               (w - e - 10 * SS, h - inset), (e + 10 * SS, h - inset), (e, h * 0.82 - inset), (inset, h * 0.82 - inset), (26 * SS + inset, h / 2)]
        d.polygon(pts, fill=255); return m
    over(im, solid(w, h, (16, 8, 6, 255), ribbon(0)))
    over(im, masked(vgrad(w, h, [(0, (255, 214, 120, 255)), (0.5, (196, 128, 48, 255)), (1, (120, 66, 24, 255))]), ribbon(4 * SS)))
    over(im, masked(vgrad(w, h, [(0, (150, 34, 24, 255)), (0.5, (104, 18, 14, 255)), (1, (60, 8, 8, 255))]), ribbon(11 * SS)))
    hi = L(w, h); ImageDraw.Draw(hi).rectangle([60 * SS, 16 * SS, w - 60 * SS, 34 * SS], fill=70)
    over(im, masked(solid(w, h, (255, 200, 160, 255), hi.filter(ImageFilter.GaussianBlur(5 * SS))), ribbon(11 * SS)))
    save(im, "banner", (512, 110))

def strip(name, top, bottom, line_top=True):
    # Fonds des barres du haut / du bas : dégradé sombre + filet bronze.
    w, h = 64, 64
    im = vgrad(w, h, [(0, top), (1, bottom)])
    d = ImageDraw.Draw(im)
    if line_top:
        d.rectangle([0, 0, w, 2], fill=(206, 140, 64, 255)); d.rectangle([0, 3, w, 4], fill=(60, 26, 14, 255))
    else:
        d.rectangle([0, h - 3, w, h], fill=(206, 140, 64, 255)); d.rectangle([0, h - 5, w, h - 4], fill=(60, 26, 14, 255))
    save(im, name)

def glow():
    n = 128; im = new(n, n); px = im.load()
    for y in range(n):
        for x in range(n):
            d = math.hypot((x + 0.5) / n - 0.5, (y + 0.5) / n - 0.5) * 2
            a = max(0.0, 1 - d) ** 1.8
            px[x, y] = (255, 255, 255, int(255 * a))
    save(im, "glow")

def rays():
    # Rayons de lumière tournants derrière une pièce rare.
    n = 256 * 2; im = new(n, n); d = ImageDraw.Draw(im); c = n / 2
    for i in range(16):
        a0 = i * math.tau / 16; a1 = a0 + math.tau / 40
        d.polygon([(c, c), (c + math.cos(a0) * n, c + math.sin(a0) * n), (c + math.cos(a1) * n, c + math.sin(a1) * n)], fill=(255, 255, 255, 150))
    im = im.filter(ImageFilter.GaussianBlur(3))
    m = Image.new("L", (n, n)); pm = m.load()
    for y in range(n):
        for x in range(n): pm[x, y] = int(255 * max(0, 1 - math.hypot(x - c, y - c) / c) ** 1.2)
    im.putalpha(ImageChops.multiply(im.getchannel("A"), m))
    save(im, "rays", (256, 256))

# ---------------- Icônes (128 px, contour sombre épais) ----------------
N = 128 * SS
DARK = (24, 12, 10, 255)

def icon(parts, outline=7, name=None, shadow=True):
    """parts = [(mask, fill RGBA image)] dessinés dans l'ordre ; contour sombre autour de l'union."""
    im = new(N, N)
    union = L(N, N)
    for m, _ in parts: union = ImageChops.lighter(union, m)
    ol = dilate(union, outline * SS // 2)
    if shadow:
        sh = ol.transform(ol.size, Image.AFFINE, (1, 0, 0, 0, 1, -4 * SS)).filter(ImageFilter.GaussianBlur(2 * SS))
        over(im, solid(N, N, (0, 0, 0, 120), sh))
    over(im, solid(N, N, DARK, ol))
    for m, f in parts: over(im, masked(f, m))
    save(im, name, (128, 128))

def poly(pts, scale=1.0):
    m = L(N, N); ImageDraw.Draw(m).polygon([(x * N, y * N) for x, y in pts], fill=255); return m
def ell(x0, y0, x1, y1):
    m = L(N, N); ImageDraw.Draw(m).ellipse([x0 * N, y0 * N, x1 * N, y1 * N], fill=255); return m
def rr(x0, y0, x1, y1, r):
    m = L(N, N); ImageDraw.Draw(m).rounded_rectangle([x0 * N, y0 * N, x1 * N, y1 * N], r * N, fill=255); return m
def rot(m, deg, cx=0.5, cy=0.5): return m.rotate(deg, resample=Image.BICUBIC, center=(cx * N, cy * N))
def sub(a, b): return ImageChops.subtract(a, b)
def g(top, bottom, mid=None):
    st = [(0, top), (1, bottom)] if mid is None else [(0, top), (0.5, mid), (1, bottom)]
    return vgrad(N, N, st)
def hl(m, amount=90, y1=0.45):
    """Reflet clair sur la moitié haute d'une forme."""
    cut = L(N, N); ImageDraw.Draw(cut).rectangle([0, 0, N, y1 * N], fill=amount)
    return (ImageChops.multiply(m, cut.filter(ImageFilter.GaussianBlur(6 * SS))), new(N, N, (255, 255, 255, 255)))

GOLD = ((255, 236, 140, 255), (214, 134, 30, 255), (255, 196, 60, 255))

def ic_coin():
    o = ell(0.1, 0.1, 0.9, 0.9); i = ell(0.22, 0.22, 0.78, 0.78)
    star = poly([(0.5 + 0.2 * math.cos(-math.pi / 2 + k * math.pi / 5) * (1 if k % 2 == 0 else 0.45),
                  0.5 + 0.2 * math.sin(-math.pi / 2 + k * math.pi / 5) * (1 if k % 2 == 0 else 0.45)) for k in range(10)])
    icon([(o, g(GOLD[0], GOLD[1], GOLD[2])), (i, g((200, 116, 24, 255), (255, 214, 96, 255))),
          (star, g((255, 246, 190, 255), (240, 170, 50, 255))), hl(o, 70)], name="i_gold")

def ic_gem():
    top = 0.2; mid = 0.42; bot = 0.88
    outer = poly([(0.28, top), (0.72, top), (0.92, mid), (0.5, bot), (0.08, mid)])
    tbl = poly([(0.36, top), (0.64, top), (0.7, mid), (0.3, mid)])
    left = poly([(0.08, mid), (0.3, mid), (0.5, bot)])
    right = poly([(0.92, mid), (0.7, mid), (0.5, bot)])
    lt = poly([(0.28, top), (0.36, top), (0.3, mid), (0.08, mid)])
    rt = poly([(0.72, top), (0.64, top), (0.7, mid), (0.92, mid)])
    icon([(outer, g((255, 120, 200, 255), (150, 20, 90, 255))), (tbl, g((255, 200, 240, 255), (255, 110, 200, 255))),
          (lt, g((255, 160, 220, 255), (230, 70, 160, 255))), (rt, g((210, 60, 140, 255), (170, 30, 110, 255))),
          (left, g((220, 60, 150, 255), (120, 10, 70, 255))), (right, g((170, 20, 100, 255), (90, 5, 50, 255))),
          (ell(0.38, 0.23, 0.5, 0.32), new(N, N, (255, 255, 255, 200)))], name="i_gem")

def ic_hammer():
    handle = rot(rr(0.45, 0.32, 0.56, 0.95, 0.04), -40, 0.5, 0.55)
    head = rot(rr(0.22, 0.12, 0.78, 0.38, 0.05), -40, 0.5, 0.55)
    band = rot(rr(0.42, 0.12, 0.58, 0.38, 0.0), -40, 0.5, 0.55)
    icon([(handle, g((190, 120, 70, 255), (100, 52, 24, 255))), (head, g((236, 236, 240, 255), (110, 110, 124, 255), (190, 190, 200, 255))),
          (band, g((255, 210, 100, 255), (170, 100, 30, 255))), hl(head, 80, 0.3)], name="i_hammer")

def flame(cx, cy, k):
    # Goutte de feu : base ronde, pointe en haut légèrement recourbée, deux langues sur les côtés.
    def P(x, y): return (cx + (x - 0.5) * k, cy + (y - 0.5) * k)
    body = ImageChops.lighter(ell(*P(0.2, 0.44), *P(0.8, 0.94)), poly([P(0.21, 0.66), P(0.3, 0.42), P(0.46, 0.24), P(0.56, 0.02),
                               P(0.62, 0.22), P(0.74, 0.36), P(0.79, 0.66)]))
    tongues = ImageChops.lighter(poly([P(0.22, 0.7), P(0.12, 0.36), P(0.34, 0.56)]), poly([P(0.78, 0.7), P(0.9, 0.42), P(0.68, 0.56)]))
    return ImageChops.lighter(body, tongues)

def ic_power():
    o = flame(0.5, 0.5, 1.0); i = flame(0.5, 0.64, 0.55)
    icon([(o, g((255, 200, 70, 255), (190, 24, 8, 255), (255, 100, 20, 255))), (i, g((255, 255, 220, 255), (255, 180, 40, 255)))], name="i_power")

def ic_shop():
    # Coffre au trésor.
    body = rr(0.12, 0.44, 0.88, 0.86, 0.04)
    lid = rr(0.12, 0.2, 0.88, 0.48, 0.14)
    b1 = rr(0.24, 0.2, 0.32, 0.86, 0.0); b2 = rr(0.68, 0.2, 0.76, 0.86, 0.0)
    band = rr(0.12, 0.44, 0.88, 0.5, 0.0)
    lock = rr(0.43, 0.4, 0.57, 0.6, 0.03)
    wood = g((196, 104, 52, 255), (96, 40, 18, 255))
    goldg = g((255, 230, 120, 255), (190, 110, 30, 255))
    icon([(body, wood), (lid, g((220, 128, 66, 255), (130, 58, 26, 255))), (sub(ImageChops.lighter(b1, b2), L(N, N)), goldg),
          (band, goldg), (lock, g((255, 245, 180, 255), (210, 130, 30, 255))), hl(lid, 70, 0.3)], name="i_shop")

def ic_hero():
    # Heaume.
    helm = ImageChops.lighter(ell(0.2, 0.12, 0.8, 0.72), rr(0.2, 0.42, 0.8, 0.88, 0.08))
    visor = rr(0.3, 0.46, 0.7, 0.54, 0.02); slit = rr(0.47, 0.54, 0.53, 0.78, 0.01)
    crest = poly([(0.46, 0.04), (0.54, 0.04), (0.56, 0.4), (0.44, 0.4)])
    icon([(crest, g((255, 90, 60, 255), (150, 20, 10, 255))), (helm, g((236, 238, 246, 255), (96, 100, 120, 255), (176, 180, 196, 255))),
          (ImageChops.lighter(visor, slit), new(N, N, (30, 20, 24, 255))), hl(helm, 90, 0.38)], name="i_hero")

def ic_forge():
    # Enclume + étincelle.
    top = poly([(0.06, 0.34), (0.74, 0.34), (0.94, 0.4), (0.74, 0.5), (0.26, 0.5), (0.14, 0.44)])
    waist = poly([(0.36, 0.5), (0.64, 0.5), (0.58, 0.68), (0.42, 0.68)])
    base = rr(0.24, 0.66, 0.76, 0.84, 0.03)
    spark = poly([(0.62, 0.06), (0.66, 0.2), (0.8, 0.18), (0.69, 0.26), (0.74, 0.38), (0.62, 0.29), (0.5, 0.36), (0.56, 0.24), (0.46, 0.16), (0.6, 0.18)])
    met = g((210, 214, 226, 255), (70, 72, 88, 255), (150, 152, 168, 255))
    icon([(base, met), (waist, g((120, 122, 138, 255), (60, 62, 76, 255))), (top, met),
          (rr(0.3, 0.34, 0.7, 0.37, 0.01), new(N, N, (255, 150, 50, 255))), (spark, g((255, 255, 200, 255), (255, 140, 30, 255))), hl(top, 90, 0.4)], name="i_forge")

def ic_adventure():
    # Épées croisées.
    def sword(deg):
        blade = poly([(0.5, 0.0), (0.545, 0.06), (0.55, 0.62), (0.45, 0.62), (0.455, 0.06)])
        guard = rr(0.32, 0.61, 0.68, 0.68, 0.025); grip = rr(0.465, 0.68, 0.535, 0.88, 0.01); pom = ell(0.44, 0.86, 0.56, 0.97)
        return [rot(blade, deg), rot(guard, deg), rot(grip, deg), rot(pom, deg)]
    a = sword(38); b = sword(-38)
    steel = g((248, 250, 255, 255), (130, 136, 156, 255)); goldg = g((255, 226, 110, 255), (180, 100, 24, 255)); grip = g((140, 60, 30, 255), (70, 26, 12, 255))
    parts = []
    for s in (a, b): parts += [(s[0], steel), (s[1], goldg), (s[2], grip), (s[3], goldg)]
    icon(parts, name="i_adventure")

def ic_tech():
    # Fiole de potion rouge.
    neck = rr(0.42, 0.1, 0.58, 0.42, 0.02); bulb = ell(0.16, 0.32, 0.84, 0.92); cork = rr(0.39, 0.06, 0.61, 0.16, 0.03)
    liquid = sub(bulb, rr(0, 0, 1, 0.56, 0))
    glass = g((200, 230, 255, 200), (120, 150, 190, 200))
    icon([(neck, glass), (bulb, glass), (liquid, g((255, 90, 70, 255), (150, 10, 20, 255))), (cork, g((210, 150, 90, 255), (120, 70, 30, 255))),
          (ell(0.26, 0.42, 0.42, 0.56), new(N, N, (255, 255, 255, 170))), (ell(0.44, 0.66, 0.52, 0.74), new(N, N, (255, 200, 180, 200)))], name="i_tech")

def ic_close():
    o = ell(0.08, 0.08, 0.92, 0.92)
    x1 = rot(rr(0.44, 0.22, 0.56, 0.78, 0.03), 45); x2 = rot(rr(0.44, 0.22, 0.56, 0.78, 0.03), -45)
    icon([(o, g((255, 110, 80, 255), (150, 20, 14, 255), (220, 50, 30, 255))), hl(o, 80, 0.4), (ImageChops.lighter(x1, x2), new(N, N, (255, 255, 255, 255)))], name="i_close")

def ic_lock():
    shackle = sub(rr(0.28, 0.1, 0.72, 0.6, 0.22), rr(0.38, 0.2, 0.62, 0.62, 0.12))
    body = rr(0.18, 0.42, 0.82, 0.9, 0.08); hole = ImageChops.lighter(ell(0.44, 0.54, 0.56, 0.66), rr(0.475, 0.6, 0.525, 0.76, 0.01))
    icon([(shackle, g((230, 230, 240, 255), (110, 110, 130, 255))), (body, g((255, 220, 110, 255), (170, 96, 24, 255))),
          (hole, new(N, N, (60, 30, 10, 255))), hl(body, 70, 0.55)], name="i_lock")

def ic_star():
    pts = [(0.5 + 0.42 * math.cos(-math.pi / 2 + k * math.pi / 5) * (1 if k % 2 == 0 else 0.45),
            0.54 + 0.42 * math.sin(-math.pi / 2 + k * math.pi / 5) * (1 if k % 2 == 0 else 0.45)) for k in range(10)]
    s = poly(pts)
    icon([(s, g((255, 250, 180, 255), (230, 130, 20, 255), (255, 210, 60, 255))), hl(s, 80, 0.45)], name="i_star")

def ic_check():
    o = ell(0.08, 0.08, 0.92, 0.92)
    c = L(N, N); ImageDraw.Draw(c).line([(0.27 * N, 0.52 * N), (0.43 * N, 0.68 * N), (0.74 * N, 0.33 * N)], fill=255, width=int(0.12 * N), joint="curve")
    icon([(o, g((140, 240, 110, 255), (30, 130, 40, 255))), hl(o, 70, 0.4), (c, new(N, N, (255, 255, 255, 255)))], name="i_check")

def ic_key():
    ring = sub(ell(0.1, 0.1, 0.5, 0.5), ell(0.21, 0.21, 0.39, 0.39))
    shaft = rot(rr(0.42, 0.26, 0.92, 0.36, 0.02), -45, 0.3, 0.3)
    t1 = rot(rr(0.7, 0.36, 0.78, 0.46, 0.0), -45, 0.3, 0.3); t2 = rot(rr(0.82, 0.36, 0.9, 0.5, 0.0), -45, 0.3, 0.3)
    m = ImageChops.lighter(ImageChops.lighter(shaft, t1), t2)
    icon([(ring, g((255, 236, 140, 255), (190, 110, 30, 255))), (m, g((255, 226, 120, 255), (170, 96, 24, 255)))], name="i_key")

def ic_skull():
    head = ImageChops.lighter(ell(0.18, 0.1, 0.82, 0.7), rr(0.32, 0.5, 0.68, 0.86, 0.06))
    eyes = ImageChops.lighter(ell(0.28, 0.36, 0.46, 0.54), ell(0.54, 0.36, 0.72, 0.54))
    nose = poly([(0.5, 0.56), (0.45, 0.66), (0.55, 0.66)])
    teeth = ImageChops.lighter(rr(0.41, 0.72, 0.44, 0.86, 0), ImageChops.lighter(rr(0.485, 0.72, 0.515, 0.86, 0), rr(0.56, 0.72, 0.59, 0.86, 0)))
    icon([(head, g((255, 246, 226, 255), (186, 160, 130, 255))), (eyes, g((255, 120, 30, 255), (190, 20, 10, 255))),
          (ImageChops.lighter(nose, teeth), new(N, N, (60, 30, 24, 255)))], name="i_skull")

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    button(); tab(); panel(); card(); frame(); tile(); pill(); bar(); banner(); glow(); rays()
    strip("topbar", (40, 16, 14, 250), (14, 6, 6, 250), line_top=False)
    strip("navbar", (36, 14, 12, 255), (10, 4, 4, 255), line_top=True)
    ic_coin(); ic_gem(); ic_hammer(); ic_power(); ic_shop(); ic_hero(); ic_forge(); ic_adventure(); ic_tech()
    ic_close(); ic_lock(); ic_star(); ic_check(); ic_key(); ic_skull()
