using System.Collections.Generic;
using UnityEngine;

// Personnage 2D (planches générées dans Resources/ForgeSprites, sprites Craftpix de MyBrute) posé face à la caméra
// dans la scène 3D du chemin. Animations : idle, move (si présente), attack, death.
// Les planches ne sont pas dans le dépôt GitHub (licence) : sans elles, le combat garde les modèles 3D.
public class SpriteChar : MonoBehaviour
{
    [System.Serializable] class AnimMeta { public string name; public int frames; public int cols; }
    [System.Serializable] class Meta { public int cellW, cellH; public float pivotX, pivotY, idleW, idleH; public AnimMeta[] anims; }
    class Sheet { public Meta meta; public readonly Dictionary<string, Sprite[]> anims = new Dictionary<string, Sprite[]>(); }

    static readonly Dictionary<string, Sheet> cache = new Dictionary<string, Sheet>();
    static Sprite shadowSprite;

    public static bool Has(string name) => Load(name) != null;

    static Sheet Load(string name)
    {
        if (cache.TryGetValue(name, out var s)) return s;
        var ta = Resources.Load<TextAsset>("ForgeSprites/" + name + "/meta");
        if (ta == null) { cache[name] = null; return null; }
        s = new Sheet { meta = JsonUtility.FromJson<Meta>(ta.text) };
        var m = s.meta;
        foreach (var a in m.anims)
        {
            var tex = Resources.Load<Texture2D>("ForgeSprites/" + name + "/" + a.name);
            if (tex == null) continue;
            tex.wrapMode = TextureWrapMode.Clamp;
            int rows = Mathf.CeilToInt(a.frames / (float)a.cols);
            // Si l'import a réduit la planche, on garde les proportions.
            float k = tex.height / (float)(rows * m.cellH);
            float cw = m.cellW * k, ch = m.cellH * k;
            var arr = new Sprite[a.frames];
            for (int i = 0; i < a.frames; i++)
            {
                int c = i % a.cols, r = i / a.cols;
                var rect = new Rect(c * cw, tex.height - (r + 1) * ch, cw, ch);
                // 1 unité = hauteur du personnage au repos : l'échelle du transform donne directement sa taille.
                arr[i] = Sprite.Create(tex, rect, new Vector2(m.pivotX / m.cellW, m.pivotY / m.cellH), m.idleH * k, 0, SpriteMeshType.FullRect);
                arr[i].name = name + "_" + a.name + "_" + i;
            }
            s.anims[a.name] = arr;
        }
        cache[name] = s;
        return s;
    }

    static Sprite Shadow()
    {
        if (shadowSprite != null) return shadowSprite;
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                px[y * n + x] = new Color32(0, 0, 0, (byte)(Mathf.SmoothStep(0f, 1f, a) * 255));
            }
        tex.SetPixels32(px);
        tex.Apply();
        shadowSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        return shadowSprite;
    }

    Sheet sheet;
    SpriteRenderer sr, shadow;
    Transform body;
    Sprite[] cur;
    string curName = "";
    bool loop, moving;
    float t, fps, flash, height, pitch;
    Color baseColor = Color.white;
    public float Alpha = 1f;

    public bool HasMove => sheet.anims.ContainsKey("move");
    public float HalfWidth => sheet.meta.idleW / sheet.meta.idleH * height * 0.5f;
    public string Current => curName;

    // faceLeft : les planches regardent vers la droite, on retourne les ennemis.
    public static SpriteChar Spawn(string name, float height, Transform parent, bool faceLeft, float camPitch)
    {
        var s = Load(name);
        if (s == null || !s.anims.ContainsKey("idle")) return null;
        var go = new GameObject("Perso " + name);
        go.transform.SetParent(parent, false);
        var sc = go.AddComponent<SpriteChar>();
        sc.sheet = s;
        sc.height = height;
        sc.pitch = camPitch;

        sc.body = new GameObject("Sprite").transform;
        sc.body.SetParent(go.transform, false);
        sc.body.localRotation = Quaternion.Euler(camPitch, 0f, 0f);   // face à la caméra
        sc.body.localScale = Vector3.one * height;
        sc.sr = sc.body.gameObject.AddComponent<SpriteRenderer>();
        sc.sr.flipX = faceLeft;
        sc.sr.sortingOrder = 1;

        var sh = new GameObject("Ombre").transform;
        sh.SetParent(go.transform, false);
        sh.localPosition = new Vector3(0f, 0.02f, 0f);
        sh.localRotation = Quaternion.Euler(90f, 0f, 0f);
        float w = sc.HalfWidth * 2.1f;
        sh.localScale = new Vector3(w, w * 0.38f, 1f);
        sc.shadow = sh.gameObject.AddComponent<SpriteRenderer>();
        sc.shadow.sprite = Shadow();
        sc.shadow.color = new Color(0f, 0f, 0f, 0.5f);
        sc.shadow.sortingOrder = 0;

        sc.Play("idle", true);
        return sc;
    }

    public void SetTint(Color c) { baseColor = c; }

    // Renvoie la durée de l'animation (0 si absente).
    public float Play(string anim, bool loop, float fps = 14f)
    {
        string a = anim == "move" && !HasMove ? "idle" : anim;
        if (!sheet.anims.TryGetValue(a, out var arr)) return 0f;
        cur = arr; curName = anim; this.loop = loop; this.fps = fps; t = 0f;
        moving = anim == "move";
        sr.sprite = cur[0];
        return arr.Length / fps;
    }

    public void Flash() { flash = 0.18f; }

    void Update()
    {
        if (cur == null) return;
        float dt = Time.deltaTime;
        t += dt;
        int f = (int)(t * fps);
        f = loop ? f % cur.Length : Mathf.Min(f, cur.Length - 1);
        sr.sprite = cur[f];

        // Sans animation de marche : petits bonds et légère inclinaison vers l'avant.
        float hop = 0f, lean = 0f;
        if (moving && !HasMove)
        {
            hop = Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.06f * height;
            lean = sr.flipX ? 6f : -6f;
        }
        body.localPosition = new Vector3(0f, hop, 0f);
        body.localRotation = Quaternion.Euler(pitch, 0f, lean);

        var c = baseColor;
        if (flash > 0f)
        {
            flash -= dt;
            c = Color.Lerp(c, new Color(1f, 0.25f, 0.2f), Mathf.Clamp01(flash / 0.18f));
        }
        c.a = Alpha;
        sr.color = c;
        shadow.color = new Color(0f, 0f, 0f, 0.5f * Alpha);
    }
}
