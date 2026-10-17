using UnityEngine;
using UnityEngine.Rendering;

// Héros en pièces détachées façon Forge Master (Tools/make_paper_hero.py) : tête, cheveux, torse, bras, jambes,
// animés par code ; l'arme, le bouclier et le heaume équipés sont les icônes des pièces (ItemIcons.Flat).
// Les pièces ont leur articulation au centre de l'image ; 400 px = 1 unité = taille du héros.
public class PaperHero
{
    public static bool Available => Resources.Load<Texture2D>("ForgeUI/Hero/head") != null;

    readonly Transform rig, hips, neck, shoulderB, shoulderF, legB, legF, torso;
    readonly SpriteRenderer hair, helmet, weapon, shield, belt;
    readonly SpriteRenderer[] gloves = new SpriteRenderer[2], boots = new SpriteRenderer[2];
    int keyG = -99, keyB = -99, keyBe = -99;
    readonly SpriteRenderer[] all;
    int keyW = -99, keyH = -99, keyS = -99;
    static int Key(Item it) => it != null && it.valid ? it.circle * 10 + it.model : -1;

    static Sprite Part(string name)
    {
        var t = Resources.Load<Texture2D>("ForgeUI/Hero/" + name);
        return t == null ? null : Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 400f, 0, SpriteMeshType.FullRect);
    }

    static Transform Node(string name, Transform parent, float x, float y)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = new Vector3(x, y, 0f);
        return t;
    }

    static SpriteRenderer Sr(string name, Transform parent, Sprite s, int order, float x = 0f, float y = 0f)
    {
        var sr = Node(name, parent, x, y).gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingOrder = order * 10 + (s == null ? 5 : 0);   // équipement porté (sans image au départ) juste devant sa pièce
        return sr;
    }

    static readonly System.Collections.Generic.Dictionary<string, Sprite> worn = new System.Collections.Generic.Dictionary<string, Sprite>();
    // Équipement porté dessiné pour le héros (Tools/make_paper_hero.py) : même articulation que la pièce de base.
    static Sprite Worn(string kind, Item it)
    {
        if (it == null || !it.valid) return null;
        string key = kind + "_c" + Mathf.Clamp(it.circle, 0, 9) + "_v" + it.model;
        if (!worn.TryGetValue(key, out var s)) worn[key] = s = Part(key);
        return s;
    }

    public PaperHero(Transform body)
    {
        rig = Node("Héros en pièces", body, 0f, 0f);
        var sg = rig.gameObject.AddComponent<SortingGroup>();
        sg.sortingOrder = 1;
        var arm = Part("arm");
        hips = Node("Hanches", rig, 0f, 0.24f);
        shoulderB = Node("Épaule arrière", rig, -0.05f, 0.5f);
        Sr("Bras arrière", shoulderB, arm, 1).color = new Color(0.82f, 0.78f, 0.78f);
        gloves[0] = Sr("Gant arrière", shoulderB, null, 1);
        legB = Node("Jambe arrière", hips, -0.03f, 0f);
        Sr("Jambe", legB, Part("leg"), 2).color = new Color(0.85f, 0.82f, 0.82f);
        boots[0] = Sr("Botte", legB, null, 2);
        torso = Node("Torse", hips, 0f, 0f);
        Sr("Torse", torso, Part("torso"), 3);
        belt = Sr("Ceinture", torso, null, 3);
        legF = Node("Jambe avant", hips, 0.04f, 0f);
        Sr("Jambe", legF, Part("leg"), 4);
        boots[1] = Sr("Botte", legF, null, 4);
        shield = Sr("Bouclier", shoulderB, null, 5, 0.13f, -0.15f);
        neck = Node("Cou", rig, 0.01f, 0.53f);
        Sr("Tête", neck, Part("head"), 6);
        hair = Sr("Cheveux", neck, Part("hair"), 7);
        helmet = Sr("Heaume", neck, null, 8);   // heaume porté : même articulation que la tête
        shoulderF = Node("Épaule avant", rig, 0.06f, 0.5f);
        weapon = Sr("Arme", shoulderF, null, 9, 0f, -0.18f);
        Sr("Bras avant", shoulderF, arm, 10);
        gloves[1] = Sr("Gant avant", shoulderF, null, 10);
        all = rig.GetComponentsInChildren<SpriteRenderer>(true);
    }

    static Sprite Icon(int slot, int circle, int model, Vector2 pivot, float size)
    {
        var t = ItemIcons.Flat(slot, circle, model);
        return t == null ? null : Sprite.Create(t, new Rect(0, 0, t.width, t.height), pivot, t.width / size, 0, SpriteMeshType.FullRect);
    }

    // Pièces portées (arme, heaume, bouclier) : leur modèle et leur cercle donnent l'image.
    public void SetGear(Item w, Item h, Item s, Item g = null, Item b = null, Item be = null)
    {
        int kw = Key(w), kh = Key(h), ks = Key(s), kg = Key(g), kb = Key(b), kbe = Key(be);
        if (kg != keyG) { keyG = kg; foreach (var sr in gloves) sr.sprite = Worn("glove", g); }
        if (kb != keyB) { keyB = kb; foreach (var sr in boots) sr.sprite = Worn("boot", b); }
        if (kbe != keyBe) { keyBe = kbe; belt.sprite = Worn("belt", be); }
        if (kw != keyW)
        {
            keyW = kw;
            // Prise de l'arme dans l'icône (lame vers le haut à droite) : 81 px du bord, 31,6 % du bas, pour tous les modèles.
            weapon.sprite = kw >= 0 ? Icon(0, w.circle, w.model, new Vector2(81f / 256f, 0.316f), 0.62f) : Icon(0, 0, 0, new Vector2(81f / 256f, 0.316f), 0.62f);
        }
        if (kh != keyH)
        {
            keyH = kh;
            helmet.sprite = Worn("helm", h);
            hair.enabled = kh < 0;
        }
        if (ks != keyS)
        {
            keyS = ks;
            shield.sprite = ks >= 0 ? Icon(2, s.circle, s.model, new Vector2(0.5f, 0.5f), 0.4f) : null;
        }
    }

    static float Key(float t, float t0, float t1, float a, float b) => Mathf.Lerp(a, b, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(t0, t1, t)));

    // t = temps depuis le début de l'animation.
    public void Animate(string anim, float t)
    {
        float now = Time.time;
        float bob = 0f, legA = 0f, armB = 0f, armF = 0f, lean = 0f, fall = 0f;
        switch (anim)
        {
            case "move":
                float ph = now * 11f;
                legA = Mathf.Sin(ph) * 26f;
                armB = Mathf.Sin(ph) * 16f;
                armF = -Mathf.Sin(ph) * 16f;
                bob = Mathf.Abs(Mathf.Sin(ph)) * 0.025f;
                lean = -5f;
                break;
            case "attack":
                // Armer en arrière, frapper vers l'avant, revenir.
                armF = t < 0.12f ? Key(t, 0f, 0.12f, 0f, -150f) : t < 0.2f ? Key(t, 0.12f, 0.2f, -150f, 70f) : Key(t, 0.2f, 0.4f, 70f, 0f);
                lean = t < 0.12f ? Key(t, 0f, 0.12f, 0f, 6f) : Key(t, 0.12f, 0.3f, 6f, -8f);
                armB = -10f;
                break;
            case "death":
                fall = Key(t, 0f, 0.35f, 0f, 85f);
                break;
            default:
                bob = Mathf.Sin(now * 3f) * 0.008f;
                armB = Mathf.Sin(now * 2.2f) * 4f;
                armF = -Mathf.Sin(now * 2.2f) * 4f;
                break;
        }
        rig.localRotation = Quaternion.Euler(0f, 0f, fall);
        rig.localPosition = new Vector3(-fall / 85f * 0.15f, bob, 0f);
        legB.localRotation = Quaternion.Euler(0f, 0f, legA);
        legF.localRotation = Quaternion.Euler(0f, 0f, -legA);
        torso.localRotation = Quaternion.Euler(0f, 0f, lean);
        neck.localRotation = Quaternion.Euler(0f, 0f, lean * 0.6f);
        shoulderB.localRotation = Quaternion.Euler(0f, 0f, armB);
        shoulderF.localRotation = Quaternion.Euler(0f, 0f, armF);
    }

    public void SetColor(Color c)
    {
        foreach (var sr in all)
        {
            // Les pièces arrière gardent leur ombrage.
            float k = sr.transform.parent == shoulderB && sr != shield ? 0.82f : sr.transform.parent == legB ? 0.85f : 1f;
            sr.color = new Color(c.r * k, c.g * k, c.b * k, c.a);
        }
    }
}
