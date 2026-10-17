# Héros Craftpix à pièces interchangeables : rejoue les animations Spriter (.scml) de la Valkyrie et y greffe les pièces
# (tête, visage, corps, bras, mains, jambes, arme, bouclier) de n'importe quel personnage Craftpix (même squelette).
# Entrée : C:\jeu\_ref\craftpix\<pack>\<Perso>\PNG\Vector Parts (zips Craftpix de Pierre, licence : usage dans le jeu, pas de redistribution)
# Sortie (hors dépôt, comme ForgeSprites) : Assets/Resources/ForgeSprites/Rig/rig.json (os de chaque pièce, image par image)
#         Assets/Resources/ForgeSprites/Rig/parts.json (position de chaque pièce sur son os, par personnage)
#         Assets/Resources/ForgeSprites/Parts/<Perso>/<pièce>.png (réduites)
# Usage : python Tools/make_craftpix_rig.py
import glob, json, math, os, sys
import xml.etree.ElementTree as ET
from PIL import Image

SRC = r"C:\jeu\_ref\craftpix"
EXTRA = r"C:\Users\pierr\MyBrute\MyBrute project\Assets\sprites\character\skeleton_crusader"
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeSprites")
REF = "Valkyrie_1"
FPS = 24
SCALE = 0.4          # réduction des images
ANIMS = {"idle": "Idle", "move": "Walking", "attack": "Slashing", "hurt": "Hurt", "death": "Dying"}
# Nom de fichier -> pièce logique
PARTS = {"Body.png": "Body", "Head.png": "Head", "Face 01.png": "Face", "Right Arm.png": "RArm", "Right Hand.png": "RHand",
         "Left Arm.png": "LArm", "Left Hand.png": "LHand", "Left Leg.png": "LLeg", "Right Leg.png": "RLeg",
         "Sword.png": "Weapon", "Bow.png": "Weapon", "Weapon.png": "Weapon", "Shield.png": "Shield"}
REF_HEAD = 395.0     # largeur opaque moyenne de la tête des packs classiques (378 à 418 px)
REF_BODY = 320.0     # largeur du corps des packs classiques ; les packs « Tiny » (256 ou 128) sont agrandis d'autant

def num(e, k, d=0.0):
    v = e.get(k); return float(v) if v is not None else d

class Scml:
    def __init__(self, path):
        r = ET.parse(path).getroot()
        self.files = {}
        for fo in r.iter("folder"):
            for fi in fo.iter("file"):
                self.files[(fo.get("id"), fi.get("id"))] = dict(name=fi.get("name"), w=num(fi, "width"), h=num(fi, "height"),
                                                                px=num(fi, "pivot_x", 0), py=num(fi, "pivot_y", 1))
        self.ent = r.find("entity")
        self.anims = {a.get("name"): a for a in self.ent.findall("animation")}

    def pose(self, anim_name, t):
        """Transformations monde des os et objets à l'instant t (ms) : liste d'objets (fichier, parent os monde, local, z)."""
        a = self.anims[anim_name]
        length = num(a, "length"); looping = a.get("looping", "true") != "false"
        keys = a.find("mainline").findall("key")
        mk = keys[0]
        for k in keys:
            if num(k, "time") <= t: mk = k
        tls = {tl.get("id"): tl for tl in a.findall("timeline")}

        def sample(tl_id, key_id, kind):
            tl = tls[tl_id]; tkeys = tl.findall("key")
            idx = [i for i, k in enumerate(tkeys) if k.get("id") == key_id][0]
            k0 = tkeys[idx]; v0 = k0.find(kind)
            if idx + 1 < len(tkeys): k1 = tkeys[idx + 1]; t1 = num(k1, "time")
            elif looping and len(tkeys) > 1: k1 = tkeys[0]; t1 = length + num(k1, "time")
            else: k1 = None
            t0 = num(k0, "time")
            def g(e, n, d): return num(e, n, d)
            vals = dict(x=g(v0, "x", 0), y=g(v0, "y", 0), angle=g(v0, "angle", 0), sx=g(v0, "scale_x", 1), sy=g(v0, "scale_y", 1),
                        a=g(v0, "a", 1), folder=v0.get("folder"), file=v0.get("file"),
                        px=v0.get("pivot_x"), py=v0.get("pivot_y"))
            if k1 is not None and t1 > t0 and k0.get("curve_type") != "instant":
                v1 = k1.find(kind); f = max(0.0, min(1.0, (t - t0) / (t1 - t0)))
                spin = int(k0.get("spin", "1"))
                a0, a1 = vals["angle"], g(v1, "angle", 0)
                if spin == 0: a1 = a0
                elif spin > 0 and a1 < a0: a1 += 360
                elif spin < 0 and a1 > a0: a1 -= 360
                for n, d in (("x", 0), ("y", 0), ("sx", 1), ("sy", 1), ("a", 1)):
                    key = {"sx": "scale_x", "sy": "scale_y"}.get(n, n)
                    vals[n] = vals[n] + (g(v1, key, d) - vals[n]) * f
                vals["angle"] = a0 + (a1 - a0) * f
            return vals

        def compose(p, l):
            pa = math.radians(p["angle"])
            lx, ly = l["x"] * p["sx"], l["y"] * p["sy"]
            return dict(x=p["x"] + lx * math.cos(pa) - ly * math.sin(pa), y=p["y"] + lx * math.sin(pa) + ly * math.cos(pa),
                        angle=p["angle"] + l["angle"], sx=p["sx"] * l["sx"], sy=p["sy"] * l["sy"])

        root = dict(x=0, y=0, angle=0, sx=1, sy=1)
        bones = {}
        for b in mk.findall("bone_ref"):
            v = sample(b.get("timeline"), b.get("key"), "bone")
            parent = bones[b.get("parent")] if b.get("parent") is not None else root
            bones[b.get("id")] = compose(parent, v)
        objs = []
        for o in mk.findall("object_ref"):
            v = sample(o.get("timeline"), o.get("key"), "object")
            fi = self.files.get((v["folder"], v["file"]))
            if fi is None: continue
            parent = bones[o.get("parent")] if o.get("parent") is not None else root
            px = float(v["px"]) if v["px"] is not None else fi["px"]
            py = float(v["py"]) if v["py"] is not None else fi["py"]
            objs.append(dict(file=fi["name"], bone=parent, local=dict(x=v["x"], y=v["y"], angle=v["angle"], sx=v["sx"], sy=v["sy"]),
                             z=int(o.get("z_index", "0")), a=v["a"], px=px, py=py, w=fi["w"], h=fi["h"]))
        return objs

def find_chars():
    out = {}
    for f in glob.glob(os.path.join(SRC, "*", "*", "PNG", "Vector Parts", "Animations.scml")):
        if "(1)" in f or "MACOSX" in f: continue
        out[os.path.normpath(f).split(os.sep)[-4]] = os.path.dirname(f)
    # Packs « Tiny Style » : <pack>/PNG/<Perso>/Vector Parts (les boss en vue de dessus n'ont pas de Body.png : ignorés)
    for f in glob.glob(os.path.join(SRC, "*", "PNG", "*", "Vector Parts", "Animations.scml")):
        if "(1)" in f or "MACOSX" in f: continue
        if os.path.exists(os.path.join(os.path.dirname(f), "Body.png")):
            out[os.path.normpath(f).split(os.sep)[-3]] = os.path.dirname(f)
    for f in glob.glob(os.path.join(EXTRA, "Skeleton_Crusader_*", "PNG", "Vector Parts", "Animations.scml")):
        out[os.path.normpath(f).split(os.sep)[-4]] = os.path.dirname(f)
    return out

def main():
    chars = find_chars()
    ref = Scml(os.path.join(chars[REF], "Animations.scml"))
    # Hauteur de référence : du bas des pieds au haut de la tête dans la pose de repos.
    pose0 = ref.pose("Idle", 0)
    def corners(o):
        b = o["bone"]; l = o["local"]; w = dict(b); w = {**w}
        pa = math.radians(b["angle"]); lx, ly = l["x"] * b["sx"], l["y"] * b["sy"]
        x0 = b["x"] + lx * math.cos(pa) - ly * math.sin(pa); y0 = b["y"] + lx * math.sin(pa) + ly * math.cos(pa)
        ang = math.radians(b["angle"] + l["angle"]); sx = b["sx"] * l["sx"]; sy = b["sy"] * l["sy"]
        pts = []
        for cx, cy in ((0, 0), (1, 0), (0, 1), (1, 1)):
            dx = (cx - o["px"]) * o["w"] * sx; dy = (cy - o["py"]) * o["h"] * sy
            pts.append((x0 + dx * math.cos(ang) - dy * math.sin(ang), y0 + dx * math.sin(ang) + dy * math.cos(ang)))
        return pts
    ys = [p[1] for o in pose0 if o["file"] in ("Head.png", "Left Leg.png", "Right Leg.png") for p in corners(o)]
    xs = [p[0] for o in pose0 if o["file"] in ("Body.png",) for p in corners(o)]
    foot, top = min(ys), max(ys); cx = (min(xs) + max(xs)) / 2
    H = top - foot
    print("hauteur de référence", round(H), "px ; pieds", round(foot))

    # Os de chaque pièce, image par image (unités : hauteur du perso = 1, pieds en 0).
    rig = {"height_px": H, "anims": {}}
    for key, name in ANIMS.items():
        a = ref.anims[name]; length = num(a, "length")
        n = max(1, int(round(length / 1000 * FPS)))
        frames = []
        for i in range(n):
            t = i * length / n
            fr = {}
            for o in ref.pose(name, t):
                p = PARTS.get(o["file"]) or ("Face" if o["file"].startswith("Face") else None)
                if p is None: continue
                b = o["bone"]
                fr[p] = [round((b["x"] - cx) / H, 5), round((b["y"] - foot) / H, 5), round(b["angle"], 3), round(b["sx"], 4), round(b["sy"], 4), o["z"], round(o["a"], 3)]
            frames.append(fr)
        rig["anims"][key] = dict(fps=FPS, loop=a.get("looping", "true") != "false" and key != "death", frames=frames)
    os.makedirs(os.path.join(OUT, "Rig"), exist_ok=True)
    json.dump(rig, open(os.path.join(OUT, "Rig", "rig.json"), "w"), separators=(",", ":"))

    # Pièces de chaque personnage : position sur l'os (dans SA pose de repos), pivot, images réduites.
    parts = {}
    for cname, d in sorted(chars.items()):
        try: sc = Scml(os.path.join(d, "Animations.scml"))
        except Exception as e: print("ignoré", cname, e); continue
        anim = "Idle" if "Idle" in sc.anims else list(sc.anims)[0]
        entry = {}
        pose = sc.pose(anim, 0)
        bw = [o["w"] * abs(o["bone"]["sx"] * o["local"]["sx"]) for o in pose if o["file"] == "Body.png"]
        k = REF_BODY / bw[0] if bw else 1.0     # mise à l'échelle du corps, des membres et de l'arme
        # La tête et le visage sont mis à l'échelle sur la taille réelle (pixels opaques) de la tête de référence.
        hw = [Image.open(os.path.join(d, o["file"])).getbbox() for o in pose if o["file"] == "Head.png"]
        hw = [(b[2] - b[0]) * abs(o["bone"]["sx"] * o["local"]["sx"]) for b, o in zip(hw, [o for o in pose if o["file"] == "Head.png"]) if b]
        kh = REF_HEAD / hw[0] if hw and abs(k - 1) > 0.01 else k   # seulement pour les packs « Tiny »
        for o in pose:
            p = PARTS.get(o["file"]) or ("Face" if o["file"].startswith("Face 01") else None)
            if p is None or p in entry: continue
            src = os.path.join(d, o["file"])
            if not os.path.exists(src): continue
            im = Image.open(src).convert("RGBA")
            w, h = max(1, int(im.width * SCALE)), max(1, int(im.height * SCALE))
            os.makedirs(os.path.join(OUT, "Parts", cname), exist_ok=True)
            im.resize((w, h), Image.LANCZOS).save(os.path.join(OUT, "Parts", cname, p + ".png"))
            l = o["local"]
            kk = kh if p in ("Head", "Face") else k
            entry[p] = [round(l["x"] * kk / H, 5), round(l["y"] * kk / H, 5), round(l["angle"], 3), round(l["sx"] * kk, 4), round(l["sy"] * kk, 4), round(o["px"], 4), round(o["py"], 4)]
        parts[cname] = entry
        print(cname, sorted(entry))
    meta = {"scale": SCALE, "height_px": H, "chars": parts}
    json.dump(meta, open(os.path.join(OUT, "Rig", "parts.json"), "w", encoding="utf-8"), separators=(",", ":"), ensure_ascii=False)

PART_ORDER = ["Body", "Head", "Face", "RArm", "RHand", "LArm", "LHand", "LLeg", "RLeg", "Weapon", "Shield"]

def unity_format():
    """Mêmes données en tableaux plats lisibles par JsonUtility : rig_u.json et parts_u.json."""
    rig = json.load(open(os.path.join(OUT, "Rig", "rig.json")))
    anims = []
    for name, a in rig["anims"].items():
        frames = []
        for fr in a["frames"]:
            v = []
            for p in PART_ORDER:
                e = fr.get(p)
                v += ([1] + e) if e else [0] * 8
            frames.append({"v": v})
        anims.append({"name": name, "fps": a["fps"], "loop": a["loop"], "frames": frames})
    json.dump({"parts": PART_ORDER, "anims": anims}, open(os.path.join(OUT, "Rig", "rig_u.json"), "w"), separators=(",", ":"))
    meta = json.load(open(os.path.join(OUT, "Rig", "parts.json"), encoding="utf-8"))
    chars = []
    for name, e in meta["chars"].items():
        v = []
        for p in PART_ORDER:
            v += ([1] + e[p]) if p in e else [0] * 8
        chars.append({"name": name, "v": v})
    json.dump({"scale": meta["scale"], "heightPx": meta["height_px"], "chars": chars}, open(os.path.join(OUT, "Rig", "parts_u.json"), "w"), separators=(",", ":"))

if __name__ == "__main__":
    main()
    unity_format()
