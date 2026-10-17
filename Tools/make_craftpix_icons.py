# Icônes des pièces tirées des pièces Craftpix que porte le héros (CraftpixHero) : l'icône montre exactement la tête,
# l'arme, le corps, les bras, les jambes ou le bouclier choisis pour ce modèle de ce cercle.
# Emplacements 0-5 (Lame, Heaume, Bouclier, Gantelets, Bottes, Ceinture) ; amulettes et anneaux gardent les icônes plates.
# Lit les tables de correspondance directement dans Assets/Scripts/Forge/CraftpixHero.cs (une seule source de vérité).
# Entrée : Resources/ForgeSprites/Rig + Parts (Tools/make_craftpix_rig.py). Sortie (hors dépôt) : Resources/ForgeSprites/ItemIcons/
# Usage : python Tools/make_craftpix_icons.py
import json, math, os, re, sys
from PIL import Image, ImageFilter

HERE = os.path.dirname(__file__)
sys.path.insert(0, HERE)
from make_item_icons import SETS

ROOT = os.path.join(HERE, "..")
FS = os.path.join(ROOT, "Assets", "Resources", "ForgeSprites")
OUT = os.path.join(FS, "ItemIcons")
CS = open(os.path.join(ROOT, "Assets", "Scripts", "Forge", "CraftpixHero.cs"), encoding="utf-8").read()
N = 256

def table(name):
    blk = CS[CS.index(name + " = new"):]; blk = blk[:blk.index("};")]
    return {m.group(1): re.findall(r'"([^"]+)"', m.group(2)) for m in re.finditer(r'\{ "([^"]+)", new\[\] \{([^}]*)\} \}', blk)}

HEADS, WEAPONS = table("Heads"), table("Weapons")
THEME = [re.findall(r'"([^"]+)"', l) for l in CS[CS.index("Theme ="):CS.index("static readonly Dictionary<string, string[]> Heads")].split("\n") if "new[]" in l and '"' in l]
SHIELDS = re.findall(r'"([^"]+)"', CS[CS.index("Shields ="):].split(";")[0])
assert len(THEME) == 10 and HEADS and WEAPONS and SHIELDS

def pick(lst, c, m): return lst[abs(c * 7 + m * 3) % len(lst)]

rig = json.load(open(os.path.join(FS, "Rig", "rig.json")))
meta = json.load(open(os.path.join(FS, "Rig", "parts.json"), encoding="utf-8"))
H, SC = meta["height_px"], meta["scale"]

def render(choice, parts, frame=None):
    """Dessine les pièces demandées (pose de repos) et renvoie l'image recadrée."""
    U = 1400  # pixels par unité (grand, réduit ensuite)
    W = 1600
    s = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    fr = frame or rig["anims"]["idle"]["frames"][0]
    items = []
    for part in parts:
        if part not in fr: continue
        c = choice.get(part)
        loc = meta["chars"].get(c, {}).get(part)
        if loc is None: continue
        bx, by, ba, bsx, bsy, z, a = fr[part]
        lx, ly, la, lsx, lsy, px, py = loc
        pa = math.radians(ba)
        X = bx + (lx * bsx) * math.cos(pa) - (ly * bsy) * math.sin(pa); Y = by + (lx * bsx) * math.sin(pa) + (ly * bsy) * math.cos(pa)
        items.append((z, part, c, X, Y, ba + la, bsx * lsx, bsy * lsy, px, py))
    for z, part, c, X, Y, ang, sx, sy, px, py in sorted(items):
        im = Image.open(os.path.join(FS, "Parts", c, part + ".png")).convert("RGBA")
        k = U / (H * SC)
        im = im.resize((max(1, int(im.width * k * abs(sx))), max(1, int(im.height * k * abs(sy)))), Image.LANCZOS)
        if sx < 0: im = im.transpose(Image.FLIP_LEFT_RIGHT)
        if sy < 0: im = im.transpose(Image.FLIP_TOP_BOTTOM)
        pvx, pvy = px * im.width, (1 - py) * im.height
        big = Image.new("RGBA", (im.width * 3, im.height * 3)); big.alpha_composite(im, (im.width, im.height))
        big = big.rotate(ang, center=(im.width + pvx, im.height + pvy), resample=Image.BICUBIC)
        x0, y0 = int(W / 2 + X * U - (im.width + pvx)), int(W * 0.95 - Y * U - (im.height + pvy))
        layer = Image.new("RGBA", s.size); layer.paste(big, (x0, y0)); s = Image.alpha_composite(s, layer)
    bb = s.getbbox()
    return s.crop(bb) if bb else s

def fit(im, rot=0, margin=12):
    if rot: im = im.rotate(rot, expand=True, resample=Image.BICUBIC)
    bb = im.getbbox()
    if bb: im = im.crop(bb)
    k = (N - 2 * margin) / max(im.width, im.height)
    im = im.resize((max(1, int(im.width * k)), max(1, int(im.height * k))), Image.LANCZOS)
    out = Image.new("RGBA", (N, N), (0, 0, 0, 0))
    # légère ombre portée pour détacher l'objet du fond de la tuile
    sh = Image.new("RGBA", im.size, (0, 0, 0, 110)); sh.putalpha(im.getchannel("A").point(lambda v: v * 110 // 255))
    out.alpha_composite(sh.filter(ImageFilter.GaussianBlur(4)), ((N - im.width) // 2 + 3, (N - im.height) // 2 + 6))
    out.alpha_composite(im, ((N - im.width) // 2, (N - im.height) // 2))
    return out

def weapon_icon(ch):
    im = Image.open(os.path.join(FS, "Parts", ch, "Weapon.png")).convert("RGBA")
    return fit(im, 45)   # images horizontales (poignée à gauche) -> lame vers le haut à droite

def main():
    os.makedirs(OUT, exist_ok=True)
    n = 0
    for c in range(10):
        for m in range(len(SETS[0][c])):
            ch = pick(WEAPONS.get(SETS[0][c][m], ["Valkyrie_1"]), c, m)
            weapon_icon(ch).save(os.path.join(OUT, "s0_c%d_v%d.png" % (c, m))); n += 1
        for m in range(len(SETS[1][c])):
            ch = pick(HEADS.get(SETS[1][c][m], ["Valkyrie_1"]), c, m)
            fit(render({"Head": ch, "Face": ch}, ["Head", "Face"])).save(os.path.join(OUT, "s1_c%d_v%d.png" % (c, m))); n += 1
        for m in range(len(SETS[2][c])):
            ch = SHIELDS[abs(c * 3 + m) % len(SHIELDS)]
            fit(Image.open(os.path.join(FS, "Parts", ch, "Shield.png")).convert("RGBA")).save(os.path.join(OUT, "s2_c%d_v%d.png" % (c, m))); n += 1
        for m in range(len(SETS[3][c])):
            ch = pick(THEME[c], c, m)
            fit(render({"RArm": ch, "RHand": ch}, ["RArm", "RHand"]), -30).save(os.path.join(OUT, "s3_c%d_v%d.png" % (c, m))); n += 1
        for m in range(len(SETS[4][c])):
            ch = pick(THEME[c], c, m)
            fit(render({"RLeg": ch}, ["RLeg"])).save(os.path.join(OUT, "s4_c%d_v%d.png" % (c, m))); n += 1
        for m in range(len(SETS[5][c])):
            ch = pick(THEME[c], c, m)
            fit(render({"Body": ch}, ["Body"])).save(os.path.join(OUT, "s5_c%d_v%d.png" % (c, m))); n += 1
    print("icônes", n)

if __name__ == "__main__":
    main()
