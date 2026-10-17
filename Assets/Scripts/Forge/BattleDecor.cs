using System.Collections.Generic;
using UnityEngine;

// Décor du chemin façon Forge Master (vue plongeante, aplats et contours noirs) : sol répétable + éléments posés de chaque côté
// de la route, une palette par cercle. Images générées par Tools/make_battle_decor.py dans Resources/ForgeUI/Battle.
public class BattleDecor
{
    const float TileW = 12f, ZTop = 16f, ZSpan = 25f, GroundW = 72f, Slot = 1.6f;
    static readonly string[] PropNames = { "tree", "rock", "skull", "bones", "tomb", "brazier", "crystal", "spikes", "bush" };
    static readonly float[] PropHeight = { 2.7f, 0.9f, 0.55f, 0.5f, 1.15f, 1.45f, 1.25f, 1.1f, 0.6f };
    // Fréquence de chaque élément (même ordre).
    static readonly float[] PropWeight = { 0.14f, 0.2f, 0.07f, 0.08f, 0.08f, 0.04f, 0.04f, 0.09f, 0.26f };

    static bool? available;
    public static bool Available => available ?? (available = Resources.Load<Texture2D>("ForgeUI/Battle/c0_ground") != null).Value;

    readonly Transform world, root;
    readonly float pitch;
    readonly Transform ground;
    readonly Material groundMat;
    readonly Dictionary<int, Sprite[]> props = new Dictionary<int, Sprite[]>();
    readonly List<GameObject> items = new List<GameObject>();
    int circle = -1, built = int.MinValue;

    public BattleDecor(Transform world, float pitch)
    {
        this.world = world;
        this.pitch = pitch;
        root = new GameObject("Décor plat").transform;
        root.SetParent(world, false);
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(q.GetComponent<Collider>());
        q.name = "Sol";
        ground = q.transform;
        ground.SetParent(root, false);
        ground.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ground.localScale = new Vector3(GroundW, ZSpan, 1f);
        groundMat = ForgeWorld.Unlit(Color.white);
        var r = q.GetComponent<Renderer>();
        r.sharedMaterial = groundMat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        SetCircle(0);
    }

    public void SetCircle(int c)
    {
        c = Mathf.Clamp(c, 0, 9);
        if (c == circle) return;
        circle = c;
        var tex = Resources.Load<Texture2D>("ForgeUI/Battle/c" + c + "_ground");
        if (tex != null)
        {
            tex.wrapModeU = TextureWrapMode.Repeat;
            tex.wrapModeV = TextureWrapMode.Clamp;
            groundMat.mainTexture = tex;
            groundMat.SetTexture("_BaseMap", tex);
        }
        foreach (var g in items) Object.Destroy(g);
        items.Clear();
        built = int.MinValue;
    }

    Sprite[] Props(int c)
    {
        if (props.TryGetValue(c, out var arr)) return arr;
        arr = new Sprite[PropNames.Length];
        for (int i = 0; i < PropNames.Length; i++)
        {
            var t = Resources.Load<Texture2D>("ForgeUI/Battle/c" + c + "_" + PropNames[i]);
            if (t == null) continue;
            // 1 unité = hauteur voulue de l'élément ; base posée au sol.
            arr[i] = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.03f), t.height / PropHeight[i], 0, SpriteMeshType.FullRect);
        }
        props[c] = arr;
        return arr;
    }

    static float Hash(int i, float k) => Mathf.Abs(Mathf.Sin(i * k) * 43758.5453f) % 1f;

    int PickProp(float r)
    {
        for (int i = 0; i < PropWeight.Length; i++) { if (r < PropWeight[i]) return i; r -= PropWeight[i]; }
        return PropWeight.Length - 1;
    }

    void Place(int slot, float zMin, float zMax, float seed, bool smallOnly = false)
    {
        float r = Hash(slot, 12.9898f + seed), r2 = Hash(slot, 78.233f + seed), r3 = Hash(slot, 39.425f + seed);
        if (r > 0.62f) return;   // un emplacement sur trois environ reste vide
        int k = PickProp(r2);
        if (smallOnly && PropHeight[k] > 1f) k = 8;   // premier plan : rien qui cache la route
        var spr = Props(circle)[k];
        if (spr == null) return;
        var go = new GameObject(PropNames[k]);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(slot * Slot + (r3 - 0.5f) * Slot * 0.8f, 0f, Mathf.Lerp(zMin, zMax, Hash(slot, 93.989f + seed)));
        go.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        float s = 0.85f + r3 * 0.35f;
        go.transform.localScale = Vector3.one * s;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.flipX = r3 > 0.5f;
        items.Add(go);
    }

    public void Update(float heroX)
    {
        // Sol : suit le héros, la texture défile pour rester fixe dans le monde.
        float x = heroX + 4f;
        ground.localPosition = new Vector3(x, -0.01f, ZTop - ZSpan * 0.5f);
        groundMat.mainTextureScale = new Vector2(GroundW / TileW, 1f);
        float off = (x - GroundW * 0.5f) / TileW;
        groundMat.mainTextureOffset = new Vector2(off - Mathf.Floor(off), 0f);

        // Éléments : créés devant, détruits derrière.
        int from = Mathf.FloorToInt((heroX - 14f) / Slot), to = Mathf.CeilToInt((heroX + 26f) / Slot);
        if (built < from) built = from - 1;
        while (built < to)
        {
            built++;
            Place(built, 3.0f, 10.5f, 0f);      // bande du fond
            Place(built, 2.6f, 6.5f, 5.3f);     // deuxième rangée du fond
            Place(built, -3.0f, -5.4f, 11.7f, true);  // premier plan
        }
        for (int i = items.Count - 1; i >= 0; i--)
            if (items[i].transform.localPosition.x < heroX - 16f) { Object.Destroy(items[i]); items.RemoveAt(i); }
    }
}
