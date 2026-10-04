using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Icônes 3D des pièces : le vrai modèle est photographié une fois dans un petit studio caché,
// puis l'image (fond transparent) est réutilisée dans les tuiles. Une icône par emplacement et par cercle.
public static class ItemIcons
{
    const int Size = 256;
    static readonly Vector3 StudioPos = new Vector3(0f, -300f, 0f);
    static Camera cam;
    static Transform studio;
    static readonly Dictionary<int, RenderTexture> cache = new Dictionary<int, RenderTexture>();

    // Orientation de chaque type de pièce devant l'objectif (vue de trois quarts).
    static readonly Vector3[] SlotEuler =
    {
        new Vector3(0f, 0f, -45f),    // Lame : en diagonale, lame de face
        new Vector3(12f, 150f, 0f),   // Heaume
        new Vector3(5f, 160f, 0f),    // Bouclier
        new Vector3(15f, -25f, 0f),   // Gantelets
        new Vector3(10f, -35f, 0f),   // Bottes
        new Vector3(28f, -15f, 0f),   // Ceinture : vue d'un peu au-dessus, boucle devant
        new Vector3(8f, -20f, 0f),    // Amulette
        new Vector3(20f, -30f, 0f),   // Anneau
    };

    static void EnsureStudio()
    {
        if (cam != null) return;
        studio = new GameObject("Studio des icônes").transform;
        studio.position = StudioPos;
        Object.DontDestroyOnLoad(studio.gameObject);

        var cgo = new GameObject("Caméra des icônes");
        cgo.transform.SetParent(studio, false);
        cgo.transform.localPosition = new Vector3(0f, 0f, -4f);
        cam = cgo.AddComponent<Camera>();
        cam.enabled = false;                       // ne rend que sur demande
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        cam.nearClipPlane = 0.5f;
        cam.farClipPlane = 8f;                     // ne voit rien d'autre que le studio
        cam.allowHDR = false;
        cam.allowMSAA = true;
        var data = cam.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.renderShadows = false;

        // Éclairage du studio : lumière principale chaude + contre-jour froid (portée limitée au studio).
        AddLight(new Vector3(-1.6f, 1.8f, -2.4f), new Color(1f, 0.92f, 0.82f), 9f);
        AddLight(new Vector3(1.8f, 0.6f, -1.8f), new Color(1f, 0.55f, 0.3f), 5f);
        AddLight(new Vector3(0.5f, 1.5f, 2f), new Color(0.6f, 0.75f, 1f), 6f);
    }

    static void AddLight(Vector3 local, Color c, float intensity)
    {
        var l = new GameObject("Lumière studio").AddComponent<Light>();
        l.transform.SetParent(studio, false);
        l.transform.localPosition = local;
        l.type = LightType.Point;
        l.color = c;
        l.intensity = intensity;
        l.range = 7f;
        l.shadows = LightShadows.None;
    }

    // Icône de la pièce (emplacement + cercle). Renvoie une texture prête pour une RawImage.
    public static Texture Get(int slot, int circle)
    {
        int key = slot * 100 + circle;
        if (cache.TryGetValue(key, out var rt) && rt != null && rt.IsCreated()) return rt;
        rt = Render(slot, circle);
        cache[key] = rt;
        return rt;
    }

    static RenderTexture Render(int slot, int circle)
    {
        EnsureStudio();
        var it = new Item { valid = true, slot = slot, circle = circle, level = 1 };
        var model = ForgeWorld.BuildItemModel(it, 0.35f); // lueur réduite : les couleurs restent lisibles
        return Shoot(model, SlotEuler[Mathf.Clamp(slot, 0, SlotEuler.Length - 1)], "Icône " + slot + "-" + circle);
    }

    // ---------- Créatures (compagnons et montures) ----------
    static readonly string[] IdleKeys = { "Idle", "Flying", "Swim", "ArmatureAction", "Action", "*" };

    // Icône d'un modèle animé : posé dans son animation de repos, de trois quarts face à l'objectif.
    public static Texture GetCreature(string modelPath, Color tint, float tintAmount) => GetCreature(modelPath, tint, tintAmount, CreatureEuler);

    public static Texture GetCreature(string modelPath, Color tint, float tintAmount, Vector3 euler)
    {
        int key = ("créature:" + modelPath + ":" + tint + ":" + tintAmount + ":" + euler).GetHashCode();
        if (cache.TryGetValue(key, out var rt) && rt != null && rt.IsCreated()) return rt;
        EnsureStudio();
        var model = ModelLib.Spawn(modelPath, 1f, null, false, true);
        if (tintAmount > 0f) ModelLib.Tint(model, tint, tintAmount);
        var anim = model.GetComponentInChildren<Animation>();
        if (anim != null)
        {
            var p = AnimPlayer.Attach(model);
            string n = p != null ? p.Find(IdleKeys) : null;
            if (n != null)
            {
                var st = anim[n];
                st.enabled = true; st.weight = 1f; st.time = st.length * 0.25f;
                anim.Sample();
                st.enabled = false;
            }
        }
        rt = Shoot(model, euler, "Icône " + modelPath);
        cache[key] = rt;
        return rt;
    }

    // Version lisible (Texture2D) d'une icône de créature, recadrée sur la partie visible : pour en faire un sprite.
    static readonly Dictionary<int, Sprite> spriteCache = new Dictionary<int, Sprite>();
    public static Sprite CreatureSprite(string modelPath, Color tint, float tintAmount, Vector3 euler)
    {
        int key = ("sprite:" + modelPath + ":" + tint + ":" + tintAmount + ":" + euler).GetHashCode();
        if (spriteCache.TryGetValue(key, out var sp) && sp != null) return sp;
        var rt = GetCreature(modelPath, tint, tintAmount, euler) as RenderTexture;
        if (rt == null) return null;
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        // Boîte des pixels visibles ; pivot en bas au centre.
        var px = tex.GetPixels32();
        int x0 = rt.width, y0 = rt.height, x1 = -1, y1 = -1;
        for (int y = 0; y < rt.height; y++)
            for (int x = 0; x < rt.width; x++)
                if (px[y * rt.width + x].a > 20) { if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y; }
        if (x1 < 0) { x0 = y0 = 0; x1 = rt.width - 1; y1 = rt.height - 1; }
        var r = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        sp = Sprite.Create(tex, r, new Vector2(0.5f, 0f), r.height);   // 1 unité = hauteur visible
        spriteCache[key] = sp;
        return sp;
    }

    // Icône d'une pièce en sprite (lisible, recadrée, pivot au centre) : pour l'afficher à plat au-dessus de l'enclume.
    public static Sprite ItemSprite(int slot, int circle)
    {
        int key = ("pièce:" + slot + ":" + circle).GetHashCode();
        if (spriteCache.TryGetValue(key, out var sp) && sp != null) return sp;
        var rt = Get(slot, circle) as RenderTexture;
        if (rt == null) return null;
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        sp = Sprite.Create(tex, new Rect(0, 0, rt.width, rt.height), new Vector2(0.5f, 0.5f), rt.height);
        spriteCache[key] = sp;
        return sp;
    }

    // Les modèles regardent vers +Z : on les tourne vers l'objectif, de trois quarts.
    static readonly Vector3 CreatureEuler = new Vector3(8f, 215f, 0f);

    static RenderTexture Shoot(GameObject model, Vector3 euler, string name)
    {
        model.transform.SetParent(studio, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(euler);

        // Cadrage : on centre l'objet et on règle la taille de l'objectif sur sa plus grande dimension.
        var b = ModelLib.WorldBounds(model);
        model.transform.position += studio.position - b.center;
        float half = Mathf.Max(b.extents.x, b.extents.y, 0.05f);
        cam.orthographicSize = half * 1.18f;

        var rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32) { name = name, antiAliasing = 4 };
        rt.Create();
        cam.targetTexture = rt;
        var req = new UniversalRenderPipeline.SingleCameraRequest { destination = rt };
        if (RenderPipeline.SupportsRenderRequest(cam, req)) RenderPipeline.SubmitRenderRequest(cam, req);
        else cam.Render();
        cam.targetTexture = null;

        model.SetActive(false);
        Object.Destroy(model);
        return rt;
    }
}
