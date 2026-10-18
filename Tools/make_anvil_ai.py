# Enclume et marteau de la forge dans le style des persos Craftpix (ComfyUI + SDXL, même rendu que make_creatures.py).
# Variantes dans _dl/anvil_raw ; --pick N et --hpick N choisissent celles gardées -> Resources/ForgeUI/Forge/anvil.png / hammer.png
# (mêmes toiles que make_forge_flat.py : enclume 560x420, dessus de la table ~70 px du haut, pied en bas ; marteau 360x170, tête à gauche).
import os, sys, importlib.util
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("mc", os.path.join(HERE, "make_creatures.py"))
mc = importlib.util.module_from_spec(spec); spec.loader.exec_module(mc)
RAW = os.path.join(HERE, "..", "..", "_dl", "anvil_raw")
DENOISE = (0.72, 0.82)
OUT = os.path.join(HERE, "..", "Assets", "Resources", "ForgeUI", "Forge")

ANVIL = ("game asset of a big chunky blacksmith anvil, side view, horn pointing left, dark iron with a glowing hot orange ingot on top, "
         "sitting on a wooden stump with iron bands, 2d mobile rpg like craftpix, thick dark outline, matte soft shading, "
         "muted colors, simple shapes, centered, isolated on plain white background")
HAMMER = ("game asset of a blacksmith hammer lying horizontal, heavy iron head on the left, wooden handle to the right, side view, "
          "2d mobile rpg like craftpix, thick dark outline, matte soft shading, muted colors, simple shapes, isolated on plain white background")

def gen(prompt, name, seed):
    pid = mc.post("/prompt", {"prompt": mc.workflow(prompt, seed, "forge_" + name)})["prompt_id"]
    while True:
        h = mc.get("/history/" + pid)
        if pid in h and h[pid].get("outputs"):
            img = h[pid]["outputs"]["9"]["images"][0]
            return os.path.join(mc.COMFY_OUT, img.get("subfolder", ""), img["filename"])
        mc.time.sleep(1)

def fit(im, W, H, margin_bottom=4):
    bb = im.getbbox(); im = im.crop(bb)
    k = min((W - 20) / im.width, (H - margin_bottom - 10) / im.height)
    im = im.resize((int(im.width * k), int(im.height * k)), Image.LANCZOS)
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    out.alpha_composite(im, ((W - im.width) // 2, H - margin_bottom - im.height))
    return out

if __name__ == "__main__":
    a = sys.argv[1:]
    os.makedirs(RAW, exist_ok=True)
    if "--pick" in a or "--hpick" in a:
        if "--pick" in a:
            fit(Image.open(os.path.join(RAW, a[a.index("--pick") + 1] + ".png")), 560, 420).save(os.path.join(OUT, "anvil.png"))
        if "--hpick" in a:
            fit(Image.open(os.path.join(RAW, a[a.index("--hpick") + 1] + ".png")), 360, 170, 3).save(os.path.join(OUT, "hammer.png"))
        sys.exit(0)
    if "--i2i" in a:
        # img2img depuis les dessins de make_forge_flat.py (forme gardée, rendu Craftpix)
        src = {"anvil": os.path.join(RAW, "src_anvil.png"), "hammer": os.path.join(RAW, "src_hammer.png")}
        for kind, png in (("anvil", "anvil.png"), ("hammer", "hammer.png")):
            if not os.path.exists(src[kind]):
                Image.open(os.path.join(OUT, png)).save(src[kind])
            im = Image.open(src[kind]).convert("RGBA")
            bg = Image.new("RGBA", (1024, 1024), (255, 255, 255, 255))
            k = 900 / max(im.size); im2 = im.resize((int(im.width * k), int(im.height * k)), Image.LANCZOS)
            bg.alpha_composite(im2, ((1024 - im2.width) // 2, (1024 - im2.height) // 2))
            fn = "forge_%s_src.png" % kind
            bg.convert("RGB").save(os.path.join(mc.COMFY_IN, fn))
            for v in range(4):
                for dn in DENOISE:
                    prompt = (ANVIL if kind == "anvil" else HAMMER)
                    pid = mc.post("/prompt", {"prompt": mc.workflow_i2i(fn, prompt, 6100 + v * 331, "forge_i2i_%s" % kind, dn)})["prompt_id"]
                    while True:
                        h = mc.get("/history/" + pid)
                        if pid in h and h[pid].get("outputs"):
                            img = h[pid]["outputs"]["9"]["images"][0]
                            out = os.path.join(mc.COMFY_OUT, img.get("subfolder", ""), img["filename"]); break
                        mc.time.sleep(1)
                    mc.craftpix_look(mc.cutout(out, 600)).save(os.path.join(RAW, "%s_i%d_%d.png" % (kind, v, int(dn * 100))))
                    print("ok", kind, v, dn, flush=True)
        sys.exit(0)
    for v in range(6):
        for kind, prompt in (("anvil", ANVIL), ("hammer", HAMMER)):
            f = os.path.join(RAW, "%s_%d.png" % (kind, v))
            if os.path.exists(f): continue
            mc.craftpix_look(mc.cutout(gen(prompt, kind, 4100 + v * 977), 600)).save(f)
            print("ok", kind, v, flush=True)
