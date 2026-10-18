# Icônes des amulettes et anneaux dans le style Craftpix (même procédé que make_creatures.py : ComfyUI + SDXL du PC,
# détourage, rendu mat à contour sombre). Noms des modèles lus dans Assets/Scripts/Forge/ItemModels.cs.
# Sortie : Assets/Resources/ForgeUI/Jewels/s<6|7>_c<cercle>_v<modèle>.png (256 px, centrées, dans le dépôt).
# Usage : lancer ComfyUI (port 8188) puis  python Tools/make_jewel_icons.py [--redo s6_c0_v1 ...] [--seed N]
import os, re, sys, importlib.util
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("mc", os.path.join(HERE, "make_creatures.py"))
mc = importlib.util.module_from_spec(spec); spec.loader.exec_module(mc)

OUT = os.path.join(HERE, "..", "Assets", "Resources", "ForgeUI", "Jewels")
RAW = os.path.join(HERE, "..", "..", "_dl", "jewels_raw")

THEME = ["carved bone and old rope, grey stone", "pink gold with heart motifs", "brass dripping with green slime",
         "shiny gold with coins and rubies", "black iron with fire and ember gems", "bronze with purple hellfire gem",
         "blood red metal with thorns", "violet metal with a creepy eye gem", "silver and ice crystals",
         "black metal and gold with small demon wings"]
FAMILY = {"Pendentif": "teardrop gemstone pendant necklace", "Médaillon": "round medallion necklace", "Croc": "fang tooth necklace",
          "Crâne": "small skull pendant necklace", "Œil": "eye amulet necklace", "Cœur": "heart pendant necklace", "Croix ansée": "ankh amulet necklace",
          "Anneau": "gemstone ring", "Chevalière": "signet ring", "Bague griffue": "claw ring holding a gem", "Bague au crâne": "skull ring",
          "Anneau torsadé": "one single twisted band ring", "Anneau à pointes": "one single spiked ring"}
STYLE = ("game item icon of a {f}, {t}, single object centered, front view, 2d mobile rpg icon like craftpix icons, "
         "thick dark outline, matte soft shading, muted colors, simple shapes, isolated on plain white background")

def names():
    src = open(os.path.join(HERE, "..", "Assets", "Scripts", "Forge", "ItemModels.cs"), encoding="utf-8").read()
    circles = re.findall(r"new\[\] \{ // .*?\n((?:\s+new\[\] \{.*?\},\n)+)", src)
    out = []
    for block in circles:
        slots = re.findall(r'new\[\] \{ (.*?) \},', block)
        out.append([re.findall(r'"(.*?)"', s) for s in slots])
    return out

def clear_white(im):
    """Retire les grandes zones blanches/grises fermées (trou d'une bague, disque ou carré de fond)."""
    from collections import deque
    im = im.copy(); w, h = im.size; px = im.load(); seen = bytearray(w * h)
    def pale(c): return c[3] > 0 and min(c[:3]) > 185 and max(c[:3]) - min(c[:3]) < 34
    for y0 in range(h):
        for x0 in range(w):
            if seen[y0 * w + x0] or not pale(px[x0, y0]): continue
            comp = []; q = deque([(x0, y0)]); seen[y0 * w + x0] = 1
            while q:
                x, y = q.popleft(); comp.append((x, y))
                for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                    if 0 <= nx < w and 0 <= ny < h and not seen[ny * w + nx] and pale(px[nx, ny]):
                        seen[ny * w + nx] = 1; q.append((nx, ny))
            if len(comp) > w * h * 0.012:
                for x, y in comp: px[x, y] = (0, 0, 0, 0)
    return im

def centered(im, size=256):
    im = clear_white(im)
    bb = im.getbbox()
    if bb: im = im.crop(bb)
    k = size * 0.92 / max(im.size)
    im = im.resize((max(1, int(im.width * k)), max(1, int(im.height * k))), Image.LANCZOS)
    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    out.alpha_composite(im, ((size - im.width) // 2, (size - im.height) // 2))
    return out

if __name__ == "__main__":
    args = sys.argv[1:]
    redo = set(); seed0 = 7000
    if "--seed" in args: i = args.index("--seed"); seed0 = int(args[i + 1]); del args[i:i + 2]
    if "--redo" in args: i = args.index("--redo"); redo = set(args[i + 1:]); del args[i:]
    os.makedirs(OUT, exist_ok=True); os.makedirs(RAW, exist_ok=True)
    if "--refinish" in args:   # retraite les images déjà générées (sans regénérer)
        for f in os.listdir(RAW):
            centered(Image.open(os.path.join(RAW, f)).convert("RGBA")).save(os.path.join(OUT, f))
        sys.exit(0)
    N = names()
    for c, slots in enumerate(N):
        for slot in (6, 7):
            for m, nm in enumerate(slots[slot]):
                key = "s%d_c%d_v%d" % (slot, c, m)
                raw = os.path.join(RAW, key + ".png")
                if os.path.exists(raw) and key not in redo: continue
                prompt = STYLE.format(f=FAMILY.get(nm, nm), t=THEME[c])
                seed = seed0 + c * 97 + m * 13 + slot
                pid = mc.post("/prompt", {"prompt": mc.workflow(prompt, seed, "forge_jewel_" + key)})["prompt_id"]
                while True:
                    h = mc.get("/history/" + pid)
                    if pid in h and h[pid].get("outputs"):
                        img = h[pid]["outputs"]["9"]["images"][0]
                        src = os.path.join(mc.COMFY_OUT, img.get("subfolder", ""), img["filename"]); break
                    mc.time.sleep(1)
                im = mc.craftpix_look(mc.cutout(src, 480))
                im.save(raw)
                centered(im).save(os.path.join(OUT, key + ".png"))
                print("ok", key, nm, flush=True)
