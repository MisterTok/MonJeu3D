# Génère les compagnons et montures en 2D (chibi, contour noir épais, aplats) avec ComfyUI + SDXL sur le PC,
# puis détoure le fond blanc. Sortie : Assets/Resources/ForgeUI/Creatures/<modèle>.png (dans le dépôt : images à nous).
# Usage : lancer ComfyUI (port 8188) puis  python Tools/make_creatures.py [--seeds 2] [noms...]
#   --pick fichier.json : { "Rat": 1, ... } choisit la variante gardée (sinon la 0).
import json, os, sys, time, urllib.request, random
from collections import deque
from PIL import Image, ImageFilter

COMFY = "http://127.0.0.1:8188"
COMFY_OUT = r"C:\Users\pierr\Downloads\ComfyUI_windows_portable_nvidia\ComfyUI_windows_portable\ComfyUI\output"
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "ForgeUI", "Creatures")
RAW = os.path.join(os.path.dirname(__file__), "..", "..", "_dl", "creatures_raw2")

# modèle (nom du fichier, comme ProgressionData) -> description (thème des Enfers)
CREATURES = {
    "Rat": "small rat with glowing ember fur and red eyes",
    "Frog": "yellow sulfur toad with warts",
    "Slime": "orange lava slime blob with big eyes",
    "bird": "black crow with red eyes",
    "Cat": "black cat with glowing yellow eyes and small devil horns",
    "Chick": "baby chick made of fire, orange and yellow",
    "Snake": "grey ash snake with red belly",
    "Dog": "shadow dog, dark purple fur, glowing eyes",
    "Spider": "abyss spider, dark purple with red markings",
    "Wasp": "infernal wasp, red and black stripes",
    "Bat": "vampire bat, dark red wings, fangs",
    "Triceratops": "lava triceratops, dark rock skin with glowing orange cracks",
    "Velociraptor": "shadow velociraptor, dark blue scales",
    "RedFox": "flame fox, fur made of fire, fiery tail",
    "Eagle": "ember eagle with burning wings",
    "Skeleton": "small skeleton servant with a rusty sword",
    "Wolf": "cerberus, three headed hellhound, black fur, fiery eyes",
    "Robot": "iron golem, riveted metal body, glowing chest",
    "Trex": "ash tyrannosaurus, grey scales with ember spots",
    "Dragon": "red dragon with wings and horns",
    "Stegosaurus": "magma stegosaurus, glowing lava plates",
    "Alien": "astral demon, purple cosmic skin with stars, horns",
    "Apatosaurus": "bone titan brontosaurus, skeletal, bone armor",
    "Mantaray": "spectral manta ray, translucent ghostly blue",
    "Whale": "leviathan sea monster whale, dark teal, spikes",
    "Pig": "pit pig, pink with soot spots and small tusks",
    "Sheep": "black sheep with curly dark wool and red eyes",
    "Llama": "ash goat with curved horns, grey fur",
    "Horse": "skeleton horse with burning mane",
    "Zebra": "infernal zebra, black and red stripes, fiery mane",
}

# Style calé sur les persos Craftpix du jeu : grosse tête ronde, petit corps trapu, couleurs sourdes et mates, contour sombre épais.
STYLE = ("tiny super deformed chibi {d}, oversized round head, very small stubby body, short legs, side view facing right, "
         "2d mobile game sprite like craftpix chibi monsters, thick dark outline, matte soft shading, muted earthy colors, "
         "simple shapes, low detail, centered, isolated on plain white background")
NEG = ("realistic, photo, 3d render, anime, manga, highly detailed, intricate, shiny, glossy, sparkles, thin lines, "
       "realistic proportions, long legs, oversaturated, neon, blurry, text, watermark, signature, multiple characters, "
       "cropped, cut off, background scenery, ground, ground shadow, drop shadow, extra objects, frame, border, grayscale")

def craftpix_look(im):
    """Rapproche le rendu des persos Craftpix : moins de détails, couleurs moins vives, contour sombre épais et doux."""
    from PIL import ImageEnhance
    a = im.getchannel("A")
    rgb = im.convert("RGB").filter(ImageFilter.MedianFilter(5))
    rgb = ImageEnhance.Color(rgb).enhance(0.78)
    rgb = ImageEnhance.Brightness(rgb).enhance(0.93)
    body = rgb.convert("RGBA"); body.putalpha(a)
    ring = a.point(lambda v: 255 if v > 40 else 0).filter(ImageFilter.MaxFilter(9)).filter(ImageFilter.GaussianBlur(1.2))
    out = Image.new("RGBA", im.size, (38, 24, 22, 0))
    edge = Image.new("RGBA", im.size, (38, 24, 22, 255)); edge.putalpha(ring)
    out.alpha_composite(edge); out.alpha_composite(body)
    return out

def workflow(prompt, seed, prefix):
    return {
        "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": "sd_xl_base_1.0.safetensors"}},
        "5": {"class_type": "EmptyLatentImage", "inputs": {"width": 1024, "height": 1024, "batch_size": 1}},
        "6": {"class_type": "CLIPTextEncode", "inputs": {"text": prompt, "clip": ["4", 1]}},
        "7": {"class_type": "CLIPTextEncode", "inputs": {"text": NEG, "clip": ["4", 1]}},
        "3": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": 30, "cfg": 7.0, "sampler_name": "dpmpp_2m", "scheduler": "karras",
                                                    "denoise": 1.0, "model": ["4", 0], "positive": ["6", 0], "negative": ["7", 0], "latent_image": ["5", 0]}},
        "8": {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}},
        "9": {"class_type": "SaveImage", "inputs": {"filename_prefix": prefix, "images": ["8", 0]}},
    }

def post(path, data):
    req = urllib.request.Request(COMFY + path, data=json.dumps(data).encode(), headers={"Content-Type": "application/json"})
    return json.loads(urllib.request.urlopen(req).read())

def get(path): return json.loads(urllib.request.urlopen(COMFY + path).read())

def generate(name, seed):
    prefix = "forge_creature_%s_%d" % (name, seed)
    pid = post("/prompt", {"prompt": workflow(STYLE.format(d=CREATURES[name]), seed, prefix)})["prompt_id"]
    while True:
        h = get("/history/" + pid)
        if pid in h and h[pid].get("outputs"):
            img = h[pid]["outputs"]["9"]["images"][0]
            return os.path.join(COMFY_OUT, img.get("subfolder", ""), img["filename"])
        time.sleep(1)

def cutout(path, size=512):
    """Détoure le fond clair relié aux bords (le contour noir épais arrête le remplissage), recadre et réduit."""
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    seen = bytearray(w * h)
    q = deque([(x, y) for x in range(w) for y in (0, h - 1)] + [(x, y) for y in range(h) for x in (0, w - 1)])
    # couleur du fond = médiane des bords ; on retire ce qui lui ressemble (fond blanc ou gris, ombre au sol claire)
    border = sorted(px[x, y][:3] for x, y in q)
    bg = border[len(border) // 2]
    def light(c): return sum(abs(c[k] - bg[k]) for k in range(3)) < 60 or (c[0] > 200 and c[1] > 200 and c[2] > 200)
    while q:
        x, y = q.popleft()
        i = y * w + x
        if seen[i]: continue
        seen[i] = 1
        if not light(px[x, y]): continue
        px[x, y] = (255, 255, 255, 0)
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if 0 <= nx < w and 0 <= ny < h and not seen[ny * w + nx]: q.append((nx, ny))
    # Bord adouci
    a = im.getchannel("A").filter(ImageFilter.MinFilter(3)).filter(ImageFilter.GaussianBlur(1))
    im.putalpha(a)
    bb = im.getbbox()
    if bb: im = im.crop(bb)
    s = size / max(im.size)
    im = im.resize((max(1, int(im.width * s)), max(1, int(im.height * s))), Image.LANCZOS)
    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    out.alpha_composite(im, ((size - im.width) // 2, size - im.height))   # posé en bas (pieds au sol)
    return out

def remove_shadow(im):
    """Retire l'ombre grise au sol : depuis le vide, on efface les gris clairs peu saturés (le contour noir protège le dessin)."""
    im = im.copy(); w, h = im.size; px = im.load()
    def greyish(c): return c[3] > 0 and max(c[:3]) - min(c[:3]) < 30 and max(c[:3]) > 120
    q = deque((x, y) for y in range(h) for x in range(w) if px[x, y][3] == 0 and
              any(0 <= x + dx < w and 0 <= y + dy < h and greyish(px[x + dx, y + dy]) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))))
    while q:
        x, y = q.popleft()
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if 0 <= nx < w and 0 <= ny < h and greyish(px[nx, ny]):
                px[nx, ny] = (0, 0, 0, 0); q.append((nx, ny))
    # recadre à nouveau, posé en bas
    bb = im.getbbox()
    if not bb: return im
    c = im.crop(bb); s = min(1.0, w / max(c.size))
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    out.alpha_composite(c, ((w - c.width) // 2, h - c.height))
    return out

# Variante gardée pour chaque créature (planche de contrôle : _dl/creatures_raw).
PICK = {"Rat": 0, "Frog": 0, "Slime": 0, "bird": 0, "Cat": 1, "Chick": 1, "Snake": 1, "Dog": 0, "Spider": 0, "Wasp": 0, "Bat": 1,
        "Triceratops": 1, "Velociraptor": 1, "RedFox": 2, "Eagle": 2, "Skeleton": 1, "Wolf": 1, "Robot": 1, "Trex": 2, "Dragon": 1,
        "Stegosaurus": 0, "Alien": 0, "Apatosaurus": 0, "Mantaray": 2, "Whale": 1, "Pig": 2, "Sheep": 1, "Llama": 1, "Horse": 0, "Zebra": 2}

if __name__ == "__main__":
    args = sys.argv[1:]
    seeds = 2
    pick = dict(PICK)
    if "--seeds" in args: i = args.index("--seeds"); seeds = int(args[i + 1]); del args[i:i + 2]
    if "--pick" in args: i = args.index("--pick"); pick = json.load(open(args[i + 1])); del args[i:i + 2]
    names = args or list(CREATURES)
    os.makedirs(OUT, exist_ok=True); os.makedirs(RAW, exist_ok=True)
    for n in names:
        for v in range(seeds):
            raw = os.path.join(RAW, "%s_%d.png" % (n, v))
            if not os.path.exists(raw):
                src = generate(n, 2000 + v * 7919 + sum(map(ord, n)))
                craftpix_look(cutout(src, 480)).save(raw)
                print("ok", n, v, flush=True)
        keep = pick.get(n, 0)
        remove_shadow(Image.open(os.path.join(RAW, "%s_%d.png" % (n, keep))).convert("RGBA")).resize((256, 256), Image.LANCZOS).save(os.path.join(OUT, n + ".png"))
