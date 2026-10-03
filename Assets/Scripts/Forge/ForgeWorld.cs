using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Construit et anime la scène 3D : la forge infernale, l'enclume, le marteau, les braises et les pièces forgées.
public class ForgeWorld : MonoBehaviour
{
    Camera cam;
    Vector3 camBasePos;
    Light furnaceLight, itemLight, flashLight;
    Transform hammerPivot;
    ParticleSystem sparks, burst;
    GameObject currentItem;
    static Texture2D softDot;
    float shake;
    Rect normalRect = new Rect(0, 0, 1, 1), expandedRect = new Rect(0, 0, 1, 1);
    float expandT, expandTarget;
    public float ExpandAmount => expandT;

    public static readonly string[] WeaponPaths =
    {
        "Weapons/Dagger", "Weapons/Sword", "Weapons/Axe_Small", "Weapons/Sword_2", "Weapons/Axe",
        "Weapons/Hammer_Small", "Weapons/Sword_Big", "Weapons/Claymore", "Weapons/Scythe", "Weapons/Sword_Golden"
    };
    public static string WeaponPath(int circle) => WeaponPaths[Mathf.Clamp(circle, 0, 9)];
    public static string ShieldPath(int circle) =>
        circle < 2 ? "Weapons/Shield_Round" : circle < 4 ? "Weapons/Shield_Round_2" : circle < 6 ? "Weapons/Shield_Heater" : circle < 8 ? "Weapons/Shield_Heater_2" : "Weapons/Shield_Celtic_Golden";
    public static string HelmetPath(int circle) =>
        circle < 3 ? "Characters/Helmet1" : circle < 6 ? "Characters/Helmet2" : circle < 9 ? "Characters/Helmet3" : "Props/KnightHelmet";

    // normal = zone de l'enclume tactile ; expanded = grande zone utilisée pendant la révélation d'une pièce.
    public void SetBand(Rect normal, Rect expanded)
    {
        normalRect = normal;
        expandedRect = expanded;
    }

    static Rect LerpRect(Rect a, Rect b, float t) =>
        new Rect(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t), Mathf.Lerp(a.width, b.width, t), Mathf.Lerp(a.height, b.height, t));
    static readonly Vector3 AnvilTop = new Vector3(0f, 1.45f, 0f);
    static readonly Vector3 ItemShowPos = new Vector3(0f, 2.75f, 0f);

    // ---------- Matériaux ----------
    static Material baseLit, baseUnlit, baseParticle;

    static Material Load(string res, string shader)
    {
        var m = Resources.Load<Material>("ForgeMats/" + res);
        if (m != null) return m;
        var s = Shader.Find(shader);
        return s != null ? new Material(s) : new Material(Shader.Find("Sprites/Default"));
    }

    public static void InitMaterials()
    {
        if (baseLit != null) return;
        baseLit = Load("LitEmissive", "Universal Render Pipeline/Lit");
        baseUnlit = Load("Unlit", "Universal Render Pipeline/Unlit");
        baseParticle = Load("ParticleAdd", "Universal Render Pipeline/Particles/Unlit");
    }

    public static Material Lit(Color c, float metal = 0f, float smooth = 0.3f, Color? emission = null)
    {
        var m = new Material(baseLit);
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Metallic", metal);
        m.SetFloat("_Smoothness", smooth);
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", emission ?? Color.black);
        return m;
    }

    // Textures procédurales partagées (sol volcanique, pavés, lave)
    static Texture2D groundAlbedo, groundEmission, cobbleTex, lavaTex;

    static void EnsureTextures()
    {
        if (groundAlbedo != null) return;
        ProcGen.VolcanicGround(256, out groundAlbedo, out groundEmission);
        cobbleTex = ProcGen.Cobbles(256);
        lavaTex = ProcGen.Lava(256);
    }

    public static Material GroundMaterial(float tiling)
    {
        EnsureTextures();
        var m = Lit(Color.white, 0f, 0.2f, new Color(3.2f, 3.2f, 3.2f));
        m.SetTexture("_BaseMap", groundAlbedo);
        m.SetTexture("_EmissionMap", groundEmission);
        m.mainTextureScale = new Vector2(tiling, tiling);
        return m;
    }

    public static Material CobbleMaterial()
    {
        EnsureTextures();
        var m = Lit(Color.white, 0f, 0.3f);
        m.SetTexture("_BaseMap", cobbleTex);
        return m;
    }

    public static Material LavaMaterial(Color hdr)
    {
        EnsureTextures();
        var m = Unlit(hdr);
        m.SetTexture("_BaseMap", lavaTex);
        return m;
    }

    public static Material Unlit(Color c)
    {
        var m = new Material(baseUnlit);
        m.SetColor("_BaseColor", c);
        return m;
    }

    public static Material Particle(Color c)
    {
        InitMaterials();
        if (softDot == null) softDot = MakeSoftDot();
        var m = new Material(baseParticle);
        m.SetColor("_BaseColor", c);
        m.SetTexture("_BaseMap", softDot);
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 2f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.One);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = 3000;
        return m;
    }

    static GameObject Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3? euler = null)
    {
        var g = GameObject.CreatePrimitive(t);
        var col = g.GetComponent<Collider>();
        if (col != null) Destroy(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        if (euler.HasValue) g.transform.localEulerAngles = euler.Value;
        g.GetComponent<Renderer>().sharedMaterial = mat;
        return g;
    }

    // ---------- Construction ----------
    public static ForgeWorld Build()
    {
        InitMaterials();
        var root = new GameObject("ForgeWorld");
        var w = root.AddComponent<ForgeWorld>();
        if (softDot == null) softDot = MakeSoftDot();
        w.BuildEnvironment();
        return w;
    }

    static Texture2D MakeSoftDot()
    {
        const int n = 64;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - n / 2f + 0.5f) / (n / 2f), dy = (y - n / 2f + 0.5f) / (n / 2f);
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                a = a * a;
                t.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        t.Apply();
        t.wrapMode = TextureWrapMode.Clamp;
        return t;
    }

    void BuildEnvironment()
    {
        Transform r = transform;

        // Caméra
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.025f, 0.008f, 0.006f);
        cam.fieldOfView = 52f;
        cam.allowHDR = true;
        // Caméra basse et proche : l'enclume et la pièce restent visibles entre le haut de l'écran et les menus du bas.
        camBasePos = new Vector3(0f, 1.55f, -5.4f);
        camGo.transform.position = camBasePos;
        camGo.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
        cam.depth = 1f;
        var camData = cam.GetUniversalAdditionalCameraData();
        camData.renderPostProcessing = true;
        camGo.AddComponent<AudioListener>();

        // Ambiance
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.36f, 0.32f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.fogColor = new Color(0.22f, 0.08f, 0.05f);

        var sun = new GameObject("Lumière infernale").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.88f, 0.76f);
        sun.intensity = 1.6f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Post-traitement : halo lumineux, vignette, tonemapping
        var vol = new GameObject("PostFX").AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 50;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.9f);
        bloom.threshold.Override(1.0f);
        bloom.scatter.Override(0.7f);
        var vig = profile.Add<Vignette>(true);
        vig.intensity.Override(0.18f);
        vig.color.Override(new Color(0.12f, 0f, 0f));
        var tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.Neutral);
        var ca = profile.Add<ColorAdjustments>(true);
        ca.postExposure.Override(0.45f);
        ca.contrast.Override(8f);
        ca.saturation.Override(6f);
        vol.profile = profile;

        // Sol en pierre et fissures de lave
        var stone = Lit(new Color(0.09f, 0.07f, 0.07f), 0f, 0.15f);
        Prim(PrimitiveType.Cube, r, new Vector3(0, -0.25f, 2f), new Vector3(30f, 0.5f, 30f), GroundMaterial(10f));
        var lava = Unlit(new Color(4f, 1.1f, 0.15f));
        Prim(PrimitiveType.Cube, r, new Vector3(-2.2f, 0.005f, 1.2f), new Vector3(0.08f, 0.02f, 3.5f), lava, new Vector3(0, 25f, 0));
        Prim(PrimitiveType.Cube, r, new Vector3(2.6f, 0.005f, 0.6f), new Vector3(0.06f, 0.02f, 2.8f), lava, new Vector3(0, -35f, 0));
        Prim(PrimitiveType.Cube, r, new Vector3(1.2f, 0.005f, -1.6f), new Vector3(0.05f, 0.02f, 1.6f), lava, new Vector3(0, 70f, 0));
        Prim(PrimitiveType.Cylinder, r, new Vector3(0, 0.01f, 0), new Vector3(2.6f, 0.01f, 2.6f), Lit(new Color(0.05f, 0.03f, 0.03f), 0f, 0.4f));

        // Fournaise derrière l'enclume
        var rock = Lit(new Color(0.12f, 0.08f, 0.07f), 0f, 0.1f);
        Prim(PrimitiveType.Cube, r, new Vector3(0, 1.6f, 4.2f), new Vector3(4.2f, 3.2f, 1.6f), rock);
        Prim(PrimitiveType.Cube, r, new Vector3(0, 3.4f, 4.0f), new Vector3(5f, 0.5f, 2f), rock);
        Prim(PrimitiveType.Cube, r, new Vector3(0, 1.3f, 3.38f), new Vector3(2.2f, 1.7f, 0.05f), Unlit(new Color(1.1f, 0.28f, 0.05f)));
        MakeFire(new Vector3(0f, 0.55f, 3.25f));
        MakeFire(new Vector3(-0.6f, 0.55f, 3.25f));
        MakeFire(new Vector3(0.6f, 0.55f, 3.25f));
        Prim(PrimitiveType.Cube, r, new Vector3(0, 2.35f, 3.36f), new Vector3(2.5f, 0.25f, 0.1f), rock);
        // Piliers et cornes
        for (int side = -1; side <= 1; side += 2)
        {
            Prim(PrimitiveType.Cube, r, new Vector3(side * 2.4f, 2.2f, 3.4f), new Vector3(0.7f, 4.4f, 0.7f), rock);
            Prim(PrimitiveType.Cube, r, new Vector3(side * 2.4f, 4.6f, 3.4f), new Vector3(0.25f, 1.1f, 0.25f), Lit(new Color(0.2f, 0.05f, 0.04f), 0.3f, 0.5f), new Vector3(0, 0, side * -25f));
            Prim(PrimitiveType.Cube, r, new Vector3(side * 2.9f, 0.7f, 1.0f), new Vector3(0.5f, 1.4f, 0.5f), rock, new Vector3(0, 45f, side * 10f));
            var brazier = Prim(PrimitiveType.Cylinder, r, new Vector3(side * 1.7f, 0.45f, 2.2f), new Vector3(0.5f, 0.45f, 0.5f), Lit(new Color(0.15f, 0.1f, 0.08f), 0.8f, 0.5f));
            Prim(PrimitiveType.Sphere, brazier.transform, new Vector3(0, 1f, 0), new Vector3(0.9f, 0.4f, 0.9f), Unlit(new Color(5f, 1.4f, 0.2f)));
            var bl = new GameObject("Brasero").AddComponent<Light>();
            bl.type = LightType.Point; bl.color = new Color(1f, 0.45f, 0.15f); bl.range = 4f; bl.intensity = 2.5f;
            bl.transform.SetParent(r, false); bl.transform.localPosition = new Vector3(side * 1.7f, 1.3f, 2.2f);
            var fl = bl.gameObject.AddComponent<Flicker>(); fl.baseIntensity = 2.5f; fl.amount = 0.8f; fl.speed = 9f;
            MakeFire(new Vector3(side * 1.7f, 0.95f, 2.2f));
        }

        // Décor du kit donjon (modèles Quaternius)
        ModelLib.Raw("Dungeon/Barrel", r, new Vector3(-2.05f, 0f, 0.9f), 20f, 0.75f);
        ModelLib.Raw("Dungeon/Barrel2", r, new Vector3(-2.6f, 0f, 1.6f), 70f, 0.75f);
        ModelLib.Raw("Dungeon/Crate", r, new Vector3(2.25f, 0f, 1.0f), 15f, 0.6f);
        ModelLib.Raw("Dungeon/Chest_Gold", r, new Vector3(-1.3f, 0f, -0.55f), 160f, 0.55f);
        ModelLib.Raw("Dungeon/Coin_Pile", r, new Vector3(-0.85f, 0f, -0.9f), 0f, 3f);
        ModelLib.Raw("Dungeon/Bag_Coins", r, new Vector3(1.15f, 0f, -0.75f), 40f, 1.4f);
        ModelLib.Raw("Dungeon/Skull", r, new Vector3(0.75f, 0f, -1.1f), 200f, 0.5f);
        ModelLib.Raw("Dungeon/Sword_WallMount", r, new Vector3(0f, 2.75f, 3.35f), 180f, 1.2f);
        ModelLib.Raw("Dungeon/Column", r, new Vector3(-2.4f, 0f, 2.6f), 0f, 0.9f);
        ModelLib.Raw("Dungeon/Column", r, new Vector3(2.4f, 0f, 2.6f), 0f, 0.9f);

        // Éclairage de l'enclume (lumière chaude venant de face)
        var keyLight = new GameObject("Lumière enclume").AddComponent<Light>();
        keyLight.type = LightType.Spot;
        keyLight.color = new Color(1f, 0.75f, 0.55f);
        keyLight.range = 9f;
        keyLight.spotAngle = 55f;
        keyLight.intensity = 26f;
        keyLight.transform.position = new Vector3(-1.2f, 3.4f, -2.6f);
        keyLight.transform.LookAt(new Vector3(0f, 1.0f, 0f));
        var fill = new GameObject("Lumière d'appoint").AddComponent<Light>();
        fill.type = LightType.Spot;
        fill.color = new Color(1f, 0.55f, 0.3f);
        fill.range = 7f;
        fill.spotAngle = 60f;
        fill.intensity = 8f;
        fill.transform.position = new Vector3(1.4f, 1.1f, -2.4f);
        fill.transform.LookAt(new Vector3(0f, 0.9f, 0f));

        furnaceLight = new GameObject("Fournaise").AddComponent<Light>();
        furnaceLight.type = LightType.Point;
        furnaceLight.color = new Color(1f, 0.42f, 0.12f);
        furnaceLight.range = 9f;
        furnaceLight.intensity = 6f;
        furnaceLight.transform.position = new Vector3(0, 1.5f, 2.6f);
        var f2 = furnaceLight.gameObject.AddComponent<Flicker>(); f2.baseIntensity = 6f; f2.amount = 1.5f; f2.speed = 5f;

        // Enclume : souche cerclée de fer, enclume au profil classique (maillage procédural), lingot chauffé à blanc.
        var anvil = new GameObject("Enclume").transform;
        anvil.SetParent(r, false);
        var stumpWood = Lit(new Color(0.16f, 0.08f, 0.05f), 0f, 0.25f);
        var band = Lit(new Color(0.12f, 0.11f, 0.12f), 0.9f, 0.5f);
        Prim(PrimitiveType.Cylinder, anvil, new Vector3(0, 0.21f, 0), new Vector3(1.05f, 0.21f, 0.95f), stumpWood);
        Prim(PrimitiveType.Cylinder, anvil, new Vector3(0, 0.08f, 0), new Vector3(1.09f, 0.03f, 0.99f), band);
        Prim(PrimitiveType.Cylinder, anvil, new Vector3(0, 0.34f, 0), new Vector3(1.09f, 0.03f, 0.99f), band);
        var body = new GameObject("Corps de l'enclume");
        body.transform.SetParent(anvil, false);
        body.transform.localPosition = new Vector3(0, 0.42f, 0);
        body.AddComponent<MeshFilter>().sharedMesh = ProcGen.Anvil();
        body.AddComponent<MeshRenderer>().sharedMaterial = Lit(new Color(0.3f, 0.28f, 0.3f), 0.75f, 0.55f, new Color(0.06f, 0.015f, 0f));
        // Arête supérieure polie qui accroche la lumière de la fournaise
        Prim(PrimitiveType.Cube, anvil, new Vector3(0.12f, 1.392f, 0), new Vector3(1.3f, 0.012f, 0.43f), Lit(new Color(0.45f, 0.42f, 0.42f), 1f, 0.85f));
        // Lingot chauffé et runes infernales gravées
        Prim(PrimitiveType.Cube, anvil, new Vector3(0.25f, 1.43f, 0f), new Vector3(0.38f, 0.06f, 0.12f), Unlit(new Color(4f, 1.4f, 0.3f)));
        var rune = Unlit(new Color(3f, 0.55f, 0.08f));
        Prim(PrimitiveType.Cube, anvil, new Vector3(0, 0.62f, -0.225f), new Vector3(0.3f, 0.025f, 0.01f), rune);
        Prim(PrimitiveType.Cube, anvil, new Vector3(0, 0.68f, -0.225f), new Vector3(0.025f, 0.14f, 0.01f), rune);
        Prim(PrimitiveType.Cube, anvil, new Vector3(-0.07f, 0.7f, -0.225f), new Vector3(0.025f, 0.09f, 0.01f), rune, new Vector3(0, 0, 35f));
        Prim(PrimitiveType.Cube, anvil, new Vector3(0.07f, 0.7f, -0.225f), new Vector3(0.025f, 0.09f, 0.01f), rune, new Vector3(0, 0, -35f));
        // Seau de trempe à côté de la souche
        ModelLib.Raw("Dungeon/Bucket", r, new Vector3(-0.85f, 0f, -0.45f), 30f, 0.9f);

        // Marteau (pivot au manche)
        hammerPivot = new GameObject("Marteau").transform;
        hammerPivot.SetParent(r, false);
        hammerPivot.localPosition = new Vector3(1.25f, 1.67f, -0.1f);
        var wood = Lit(new Color(0.18f, 0.08f, 0.04f), 0f, 0.2f);
        Prim(PrimitiveType.Cylinder, hammerPivot, new Vector3(-0.45f, 0, 0), new Vector3(0.07f, 0.45f, 0.07f), wood, new Vector3(0, 0, 90f));
        Prim(PrimitiveType.Cube, hammerPivot, new Vector3(-0.9f, 0, 0), new Vector3(0.22f, 0.4f, 0.22f), Lit(new Color(0.2f, 0.18f, 0.2f), 0.9f, 0.6f, new Color(0.25f, 0.05f, 0f)));
        hammerPivot.localEulerAngles = new Vector3(0, 0, -55f);

        // Braises qui montent
        var embers = MakeParticles("Braises", new Vector3(0, 0.2f, 1.5f), new Color(1f, 0.45f, 0.1f, 1f), 400);
        var em = embers.main;
        em.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        em.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.7f);
        em.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        em.gravityModifier = -0.05f;
        var emE = embers.emission; emE.rateOverTime = 35f;
        var emS = embers.shape; emS.shapeType = ParticleSystemShapeType.Box; emS.scale = new Vector3(9f, 0.2f, 6f);
        var noise = embers.noise; noise.enabled = true; noise.strength = 0.4f; noise.frequency = 0.5f;
        embers.Play();

        // Étincelles (frappe) et explosion (révélation de la pièce)
        sparks = MakeParticles("Etincelles", new Vector3(0.35f, 1.42f, -0.1f), new Color(1f, 0.7f, 0.25f, 1f), 300);
        var sm = sparks.main;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        sm.gravityModifier = 1.2f;
        var se = sparks.emission; se.rateOverTime = 0f;
        var ss = sparks.shape; ss.shapeType = ParticleSystemShapeType.Hemisphere; ss.radius = 0.1f;
        sparks.Play();

        burst = MakeParticles("Explosion", ItemShowPos, Color.white, 400);
        var bm = burst.main;
        bm.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.4f);
        bm.startSpeed = new ParticleSystem.MinMaxCurve(1f, 4f);
        bm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
        bm.gravityModifier = 0.1f;
        var be = burst.emission; be.rateOverTime = 0f;
        var bs = burst.shape; bs.shapeType = ParticleSystemShapeType.Sphere; bs.radius = 0.2f;
        burst.Play();

        itemLight = new GameObject("Lumière pièce").AddComponent<Light>();
        itemLight.type = LightType.Point; itemLight.range = 4f; itemLight.intensity = 0f;
        itemLight.transform.position = ItemShowPos + new Vector3(0, 0.2f, -0.8f);

        flashLight = new GameObject("Flash").AddComponent<Light>();
        flashLight.type = LightType.Point; flashLight.range = 5f; flashLight.intensity = 0f;
        flashLight.color = new Color(1f, 0.6f, 0.2f);
        flashLight.transform.position = AnvilTop + new Vector3(0, 0.3f, -0.4f);
    }

    void MakeFire(Vector3 pos)
    {
        var fire = MakeParticles("Feu", pos, new Color(1f, 0.4f, 0.08f, 1f), 120);
        var m = fire.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        var e = fire.emission; e.rateOverTime = 40f;
        var s = fire.shape; s.shapeType = ParticleSystemShapeType.Circle; s.radius = 0.2f; s.rotation = new Vector3(-90f, 0, 0);
        var sz = fire.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.1f));
        fire.Play();
    }

    ParticleSystem MakeParticles(string name, Vector3 pos, Color color, int max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.maxParticles = max;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.5f, 0.3f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var rend = go.GetComponent<ParticleSystemRenderer>();
        rend.sharedMaterial = Particle(new Color(2.5f, 2.5f, 2.5f, 1f));
        return ps;
    }

    // ---------- Animations ----------
    void Update()
    {
        expandT = Mathf.MoveTowards(expandT, expandTarget, Time.deltaTime / 0.3f);
        float ex = Mathf.SmoothStep(0f, 1f, expandT);
        cam.rect = LerpRect(normalRect, expandedRect, ex);
        cam.fieldOfView = Mathf.Lerp(30f, 36f, ex);
        camBasePos = Vector3.Lerp(new Vector3(0.15f, 2.0f, -4.6f), new Vector3(0f, 2.2f, -6.4f), ex);
        cam.transform.rotation = Quaternion.Euler(Mathf.Lerp(14f, 3f, ex), 0f, 0f);

        if (shake > 0f)
        {
            shake = Mathf.Max(0f, shake - Time.deltaTime * 3f);
            cam.transform.position = camBasePos + Random.insideUnitSphere * shake * 0.12f;
        }
        else cam.transform.position = Vector3.Lerp(cam.transform.position, camBasePos, Time.deltaTime * 10f);

        if (currentItem != null)
        {
            currentItem.transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);
            var p = currentItem.transform.position;
            p.y = ItemShowPos.y + Mathf.Sin(Time.time * 2f) * 0.06f;
            currentItem.transform.position = p;
        }
        flashLight.intensity = Mathf.MoveTowards(flashLight.intensity, 0f, Time.deltaTime * 60f);
    }

    public bool Busy { get; private set; }

    // Frappe du marteau puis apparition de la pièce. onRevealed est appelé quand la pièce est visible.
    public void PlayForge(Item item, System.Action onRevealed)
    {
        StartCoroutine(ForgeRoutine(item, onRevealed));
    }

    // Frappe rapide sans révélation (forge automatique).
    public void QuickStrike(Color c)
    {
        if (!Busy) StartCoroutine(QuickRoutine(c));
    }

    IEnumerator QuickRoutine(Color c)
    {
        Busy = true;
        yield return Rotate(hammerPivot, -55f, -75f, 0.1f);
        yield return Rotate(hammerPivot, -75f, 10f, 0.06f);
        var sm = sparks.main;
        sm.startColor = new Color(c.r * 2f, c.g * 2f, c.b * 2f, 1f);
        sparks.Emit(30);
        flashLight.intensity = 12f;
        shake = 0.3f;
        yield return Rotate(hammerPivot, 10f, -55f, 0.15f);
        sm.startColor = new Color(1f, 0.7f, 0.25f, 1f);
        Busy = false;
    }

    IEnumerator ForgeRoutine(Item item, System.Action onRevealed)
    {
        Busy = true;
        ClearItem();
        expandTarget = 1f;
        while (expandT < 1f) yield return null;
        Color c = GameState.CircleColors[item.circle];
        bool rare = item.circle >= 2;

        int strikes = rare ? 2 : 1;
        for (int i = 0; i < strikes; i++)
        {
            yield return Rotate(hammerPivot, -55f, -80f, 0.18f);
            yield return Rotate(hammerPivot, -80f, 10f, 0.08f);
            sparks.Emit(rare ? 60 : 35);
            flashLight.intensity = 18f;
            shake = rare ? 0.9f : 0.5f;
            yield return Rotate(hammerPivot, 10f, -55f, 0.2f);
        }

        currentItem = BuildItemModel(item);
        currentItem.transform.position = AnvilTop;
        currentItem.transform.localScale = Vector3.zero;
        itemLight.color = c;

        var bmain = burst.main;
        bmain.startColor = new Color(c.r * 2f, c.g * 2f, c.b * 2f, 1f);
        burst.Emit(30 + item.circle * 25);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.45f;
            float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            currentItem.transform.position = Vector3.Lerp(AnvilTop, ItemShowPos, e);
            currentItem.transform.localScale = Vector3.one * e * 1.15f;
            itemLight.intensity = Mathf.Lerp(0f, 1.5f + item.circle * 0.8f, e);
            yield return null;
        }
        Busy = false;
        onRevealed?.Invoke();
    }

    IEnumerator Rotate(Transform tr, float from, float to, float dur)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            tr.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, t)));
            yield return null;
        }
    }

    // equipped = true : la pièce part vers le héros ; false : elle est fondue (vendue).
    public void DismissItem(bool equipped)
    {
        if (currentItem == null) return;
        StartCoroutine(DismissRoutine(currentItem, equipped));
        currentItem = null;
    }

    IEnumerator DismissRoutine(GameObject go, bool equipped)
    {
        Vector3 start = go.transform.position;
        Vector3 end = equipped ? new Vector3(0f, 0.6f, -3f) : new Vector3(0f, 1.3f, 3.4f);
        float t = 0f;
        float startLight = itemLight.intensity;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.35f;
            go.transform.position = Vector3.Lerp(start, end, t * t);
            go.transform.localScale = Vector3.one * 1.15f * (1f - t);
            itemLight.intensity = Mathf.Lerp(startLight, 0f, t);
            yield return null;
        }
        if (!equipped) { flashLight.intensity = 10f; }
        Destroy(go);
        if (currentItem == null) expandTarget = 0f;
    }

    void ClearItem()
    {
        if (currentItem != null) Destroy(currentItem);
        currentItem = null;
        itemLight.intensity = 0f;
    }

    // Montre directement une pièce déjà forgée (reprise d'une partie avec une pièce en attente).
    public void ShowItemInstant(Item item)
    {
        ClearItem();
        currentItem = BuildItemModel(item);
        currentItem.transform.position = ItemShowPos;
        currentItem.transform.localScale = Vector3.one * 1.15f;
        expandTarget = expandT = 1f;
        itemLight.color = GameState.CircleColors[item.circle];
        itemLight.intensity = 1.5f + item.circle * 0.8f;
    }

    public void Collapse() { if (currentItem == null) expandTarget = 0f; }

    public void CelebrateLevelUp()
    {
        var bmain = burst.main;
        bmain.startColor = new Color(3f, 1.4f, 0.3f, 1f);
        burst.Emit(150);
        furnaceLight.intensity = 20f;
        shake = 1f;
    }

    // ---------- Modèles des pièces (formes simples en attendant les vrais modèles 3D) ----------
    static void AddGlowGem(Transform t, Material gem, Vector3 pos)
    {
        Prim(PrimitiveType.Sphere, t, pos, Vector3.one * 0.08f, gem);
    }

    public static GameObject BuildItemModel(Item item, float glowScale = 1f)
    {
        var root = new GameObject("Pièce " + item.Name);
        var t = root.transform;
        Color c = GameState.CircleColors[item.circle];
        float glow = (0.08f + item.circle * 0.12f) * glowScale;
        var metal = Lit(Color.Lerp(new Color(0.25f, 0.24f, 0.25f), c, 0.55f), 0.85f, 0.7f, c * glow);
        var dark = Lit(new Color(0.08f, 0.05f, 0.05f), 0.5f, 0.4f);
        var gem = Unlit(c * 3f);

        switch (item.slot)
        {
            case 0: // Lame : vraie arme selon le cercle
            {
                var w = ModelLib.Spawn(WeaponPath(item.circle), 1.5f, t, true, true);
                ModelLib.Tint(w, c, 0.3f);
                AddGlowGem(t, gem, new Vector3(0, -0.2f, -0.08f));
                break;
            }
            case 100:
                Prim(PrimitiveType.Cube, t, new Vector3(0, 0.25f, 0), new Vector3(0.14f, 1.1f, 0.04f), metal);
                Prim(PrimitiveType.Cube, t, new Vector3(0, 0.82f, 0), new Vector3(0.1f, 0.1f, 0.04f), metal, new Vector3(0, 0, 45f));
                Prim(PrimitiveType.Cube, t, new Vector3(0, -0.32f, 0), new Vector3(0.5f, 0.07f, 0.1f), metal);
                Prim(PrimitiveType.Cylinder, t, new Vector3(0, -0.52f, 0), new Vector3(0.06f, 0.17f, 0.06f), dark);
                Prim(PrimitiveType.Sphere, t, new Vector3(0, -0.32f, -0.05f), Vector3.one * 0.09f, gem);
                break;
            case 1: // Heaume : vrai casque selon le cercle
            {
                var h = ModelLib.Spawn(HelmetPath(item.circle), 0.8f, t, true, true);
                ModelLib.Tint(h, c, 0.35f);
                break;
            }
            case 101:
                Prim(PrimitiveType.Sphere, t, Vector3.zero, new Vector3(0.6f, 0.62f, 0.62f), metal);
                Prim(PrimitiveType.Cube, t, new Vector3(0, -0.05f, -0.29f), new Vector3(0.4f, 0.06f, 0.06f), Unlit(Color.black));
                Prim(PrimitiveType.Cube, t, new Vector3(-0.32f, 0.3f, 0), new Vector3(0.08f, 0.45f, 0.08f), dark, new Vector3(0, 0, 30f));
                Prim(PrimitiveType.Cube, t, new Vector3(0.32f, 0.3f, 0), new Vector3(0.08f, 0.45f, 0.08f), dark, new Vector3(0, 0, -30f));
                Prim(PrimitiveType.Sphere, t, new Vector3(0, 0.18f, -0.28f), Vector3.one * 0.08f, gem);
                break;
            case 2: // Bouclier : vrai bouclier selon le cercle
            {
                var sh = ModelLib.Spawn(ShieldPath(item.circle), 1.0f, t, true, true);
                ModelLib.Tint(sh, c, 0.3f);
                break;
            }
            case 102:
                Prim(PrimitiveType.Cube, t, Vector3.zero, new Vector3(0.65f, 0.75f, 0.3f), metal);
                Prim(PrimitiveType.Sphere, t, new Vector3(-0.4f, 0.3f, 0), new Vector3(0.3f, 0.22f, 0.32f), metal);
                Prim(PrimitiveType.Sphere, t, new Vector3(0.4f, 0.3f, 0), new Vector3(0.3f, 0.22f, 0.32f), metal);
                Prim(PrimitiveType.Cube, t, new Vector3(0, -0.1f, -0.16f), new Vector3(0.06f, 0.5f, 0.02f), dark);
                Prim(PrimitiveType.Sphere, t, new Vector3(0, 0.15f, -0.17f), Vector3.one * 0.1f, gem);
                break;
            case 3: // Gantelets
                for (int s = -1; s <= 1; s += 2)
                {
                    Prim(PrimitiveType.Cube, t, new Vector3(s * 0.22f, 0, 0), new Vector3(0.2f, 0.28f, 0.14f), metal);
                    Prim(PrimitiveType.Cylinder, t, new Vector3(s * 0.22f, -0.25f, 0), new Vector3(0.18f, 0.1f, 0.15f), dark);
                    Prim(PrimitiveType.Cube, t, new Vector3(s * 0.22f, 0.2f, 0), new Vector3(0.18f, 0.12f, 0.12f), metal);
                    Prim(PrimitiveType.Sphere, t, new Vector3(s * 0.22f, 0f, -0.08f), Vector3.one * 0.06f, gem);
                }
                break;
            case 4: // Bottes
                for (int s = -1; s <= 1; s += 2)
                {
                    Prim(PrimitiveType.Cube, t, new Vector3(s * 0.18f, 0.05f, 0.05f), new Vector3(0.16f, 0.45f, 0.18f), metal);
                    Prim(PrimitiveType.Cube, t, new Vector3(s * 0.18f, -0.2f, -0.06f), new Vector3(0.16f, 0.12f, 0.36f), metal);
                    Prim(PrimitiveType.Cube, t, new Vector3(s * 0.18f, 0.25f, 0.05f), new Vector3(0.19f, 0.06f, 0.21f), dark);
                }
                break;
            case 5: // Ceinture : bande de cuir en anneau, plaques de métal, grosse boucle devant
            {
                var leather = Lit(new Color(0.22f, 0.12f, 0.07f), 0.1f, 0.35f);
                const int segs = 18;
                for (int k = 0; k < segs; k++)
                {
                    float a = k * Mathf.PI * 2f / segs;
                    var pos = new Vector3(Mathf.Sin(a) * 0.38f, 0f, -Mathf.Cos(a) * 0.38f);
                    Prim(PrimitiveType.Cube, t, pos, new Vector3(0.15f, 0.14f, 0.04f), k % 3 == 0 ? metal : leather, new Vector3(0, -a * Mathf.Rad2Deg, 0));
                }
                Prim(PrimitiveType.Cube, t, new Vector3(0, 0, -0.41f), new Vector3(0.24f, 0.2f, 0.05f), metal);
                Prim(PrimitiveType.Cube, t, new Vector3(0, 0, -0.43f), new Vector3(0.14f, 0.11f, 0.03f), dark);
                Prim(PrimitiveType.Sphere, t, new Vector3(0, 0, -0.45f), Vector3.one * 0.08f, gem);
                break;
            }
            case 6: // Amulette : chaîne de perles et pendentif
                for (int k = 0; k < 16; k++)
                {
                    float a = k * Mathf.PI * 2f / 16;
                    Prim(PrimitiveType.Sphere, t, new Vector3(Mathf.Sin(a) * 0.26f, 0.32f + Mathf.Cos(a) * 0.26f, 0), Vector3.one * 0.06f, metal);
                }
                Prim(PrimitiveType.Cube, t, new Vector3(0, -0.05f, 0), new Vector3(0.28f, 0.28f, 0.05f), metal, new Vector3(0, 0, 45f));
                Prim(PrimitiveType.Sphere, t, new Vector3(0, -0.05f, -0.04f), Vector3.one * 0.14f, gem);
                break;
            default: // Anneau
                Prim(PrimitiveType.Cylinder, t, Vector3.zero, new Vector3(0.45f, 0.05f, 0.45f), metal, new Vector3(90f, 0, 0));
                Prim(PrimitiveType.Cylinder, t, Vector3.zero, new Vector3(0.33f, 0.06f, 0.33f), Unlit(new Color(0.02f, 0.01f, 0.01f)), new Vector3(90f, 0, 0));
                Prim(PrimitiveType.Sphere, t, new Vector3(0, 0.25f, 0), Vector3.one * 0.16f, gem);
                break;
        }
        return root;
    }
}

// Fait vaciller une lumière comme une flamme.
public class Flicker : MonoBehaviour
{
    public float baseIntensity = 2f, amount = 0.5f, speed = 6f;
    Light l;
    float seed;
    void Awake() { l = GetComponent<Light>(); seed = Random.value * 100f; }
    void Update()
    {
        if (l.intensity > baseIntensity + amount * 1.5f) { l.intensity = Mathf.MoveTowards(l.intensity, baseIntensity, Time.deltaTime * 15f); return; }
        l.intensity = baseIntensity + (Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f) * 2f * amount;
    }
}
