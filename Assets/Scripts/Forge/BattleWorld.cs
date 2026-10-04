using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Le chemin des Enfers : le héros avance, affronte des vagues de démons et des boss, en automatique.
public class BattleWorld : MonoBehaviour
{
    // ---------- Réglages visuels (orientation des modèles) ----------
    public static float HeroYaw = 90f;      // le héros regarde vers +X
    public static float EnemyYaw = -90f;    // les ennemis regardent vers -X
    public static Vector3 WeaponLocalPos = new Vector3(0f, 0f, 0f);
    public static Vector3 WeaponLocalEuler = new Vector3(0f, 0f, 0f);
    public static float HelmetUp = 0.12f, HelmetYaw = 0f;

    class EnemyDef
    {
        public string path; public float height; public float fly;
        public string[] idle, move, attack, death;
        public string weapon;   // arme KayKit tenue en main droite (squelettes)
        public string sprite;   // personnage 2D (Resources/ForgeSprites) à la place du modèle 3D
        public EnemyDef(string p, float h, float f, string[] i, string[] m, string[] a, string[] d, string w = null) { path = p; height = h; fly = f; idle = i; move = m; attack = a; death = d; weapon = w; }
        public bool Kit => path.StartsWith("KayKit/");
    }

    // Squelettes KayKit (CC0) : animations nommées comme celles du héros KayKit.
    static readonly string[] SkelIdle = { "Idle_Combat", "Idle" };
    static readonly string[] SkelMove = { "Walking_D_Skeletons", "Walking_A" };
    static readonly string[] SkelAttack = { "1H_Melee_Attack_Chop", "1H_Melee_Attack_Slice_Diagonal" };
    static readonly string[] SkelDeath = { "Death_C_Skeletons", "Death_A" };

    static readonly string[] AttackK = { "Attack", "Bite" };
    static readonly string[] DeathK = { "Death" };
    static readonly Dictionary<string, EnemyDef> Defs = new Dictionary<string, EnemyDef>
    {
        { "Rat", new EnemyDef("Characters/Rat", 0.55f, 0f, new[]{"Idle"}, new[]{"Run","Walk"}, AttackK, DeathK) },
        { "Spider", new EnemyDef("Characters/Spider", 0.6f, 0f, new[]{"Idle"}, new[]{"Walk"}, AttackK, DeathK) },
        { "Snake", new EnemyDef("Characters/Snake", 0.8f, 0f, new[]{"Idle"}, new[]{"Walk"}, AttackK, DeathK) },
        { "SnakeAngry", new EnemyDef("Characters/Snake_angry", 0.8f, 0f, new[]{"Idle"}, new[]{"Walk"}, AttackK, DeathK) },
        { "Wasp", new EnemyDef("Characters/Wasp", 0.75f, 0.7f, new[]{"Flying"}, new[]{"Flying"}, AttackK, DeathK) },
        { "Bat", new EnemyDef("Characters/Bat", 0.75f, 0.8f, new[]{"Flying"}, new[]{"Flying"}, AttackK, DeathK) },
        { "Frog", new EnemyDef("Characters/Frog", 0.6f, 0f, new[]{"Idle"}, new[]{"Jump"}, AttackK, DeathK) },
        { "Slime", new EnemyDef("Characters/Slime", 0.75f, 0f, new[]{"Idle"}, new[]{"Walk"}, AttackK, DeathK) },
        { "Skeleton", new EnemyDef("KayKit/Characters/Skeleton_Warrior", 1.75f, 0f, SkelIdle, SkelMove, SkelAttack, SkelDeath, "KayKit/Items/Skeleton_Blade") },
        { "Zombie", new EnemyDef("KayKit/Characters/Skeleton_Minion", 1.6f, 0f, SkelIdle, SkelMove, SkelAttack, SkelDeath, "KayKit/Items/Skeleton_Axe") },
        { "SkelRogue", new EnemyDef("KayKit/Characters/Skeleton_Rogue", 1.65f, 0f, SkelIdle, SkelMove, SkelAttack, SkelDeath, "KayKit/Items/Skeleton_Blade") },
        { "SkelMage", new EnemyDef("KayKit/Characters/Skeleton_Mage", 1.8f, 0f, SkelIdle, SkelMove, new[]{ "Spellcast_Shoot", "1H_Melee_Attack_Chop" }, SkelDeath, "KayKit/Items/Skeleton_Staff") },
        { "Dragon", new EnemyDef("Characters/Dragon", 1.6f, 0.5f, new[]{"Flying"}, new[]{"Flying"}, AttackK, DeathK) },
    };

    // ---------- Personnages 2D (sprites Craftpix, style Forge Master) ----------
    public const float CamPitch = 37f;
    const string HeroSprite = "Valkyrie_1";
    static readonly string[] SprIdle = { "idle" }, SprMove = { "move" }, SprAttack = { "attack" }, SprDeath = { "death" };
    static readonly Dictionary<string, EnemyDef> SpriteDefs = new Dictionary<string, EnemyDef>();
    // Anciens noms (missions, donjons) -> personnage 2D.
    static readonly Dictionary<string, string> SpriteAlias = new Dictionary<string, string>
    {
        { "Rat", "Forest_Ranger_1" }, { "Spider", "Seer_1" }, { "Snake", "Bloody_Alchemist_1" }, { "SnakeAngry", "Bloody_Alchemist_3" },
        { "Wasp", "Dark_Oracle_1" }, { "Bat", "Dark_Oracle_2" }, { "Frog", "Forest_Ranger_2" }, { "Slime", "Seer_2" },
        { "Skeleton", "Skeleton_Crusader_1" }, { "Zombie", "Skeleton_Crusader_2" }, { "SkelRogue", "Reaper_Man_1" },
        { "SkelMage", "Necromancer_of_the_Shadow_1" }, { "Dragon", "Reaper_Man_3" },
    };
    static readonly string[][] SpriteCircleEnemies =
    {
        new[]{ "Skeleton_Crusader_1", "Forest_Ranger_1", "Bloody_Alchemist_1", "Valkyrie_2" },
        new[]{ "Skeleton_Crusader_1", "Seer_1", "Forest_Ranger_2", "Dark_Oracle_1" },
        new[]{ "Skeleton_Crusader_2", "Bloody_Alchemist_2", "Seer_2", "Necromancer_of_the_Shadow_1" },
        new[]{ "Skeleton_Crusader_2", "Forest_Ranger_3", "Dark_Oracle_2", "Reaper_Man_1" },
        new[]{ "Skeleton_Crusader_3", "Seer_3", "Bloody_Alchemist_3", "Valkyrie_3" },
        new[]{ "Skeleton_Crusader_3", "Necromancer_of_the_Shadow_2", "Dark_Oracle_3", "Reaper_Man_2" },
        new[]{ "Skeleton_Crusader_1", "Reaper_Man_1", "Necromancer_of_the_Shadow_1", "Necromancer_of_the_Shadow_3" },
        new[]{ "Skeleton_Crusader_2", "Reaper_Man_2", "Dark_Oracle_3", "Seer_3" },
        new[]{ "Skeleton_Crusader_3", "Necromancer_of_the_Shadow_3", "Reaper_Man_2", "Reaper_Man_3" },
        new[]{ "Skeleton_Crusader_3", "Reaper_Man_3", "Necromancer_of_the_Shadow_2", "Dark_Oracle_3" },
    };
    static bool? spritesOn;
    static bool SpritesOn => spritesOn ?? (spritesOn = SpriteChar.Has(HeroSprite)).Value;

    static EnemyDef GetDef(string key)
    {
        if (SpritesOn)
        {
            string sk = SpriteAlias.TryGetValue(key, out var a) ? a : key;
            if (SpriteChar.Has(sk))
            {
                if (!SpriteDefs.TryGetValue(sk, out var d))
                    SpriteDefs[sk] = d = new EnemyDef("", 1.45f, 0f, SprIdle, SprMove, SprAttack, SprDeath) { sprite = sk };
                return d;
            }
        }
        return Defs.TryGetValue(key, out var def) ? def : Defs["Skeleton"];
    }

    // Nom logique d'animation pour un personnage 2D.
    static string Logical(string[] keys)
    {
        if (keys == HeroIdle) return "idle";
        if (keys == HeroRun) return "move";
        if (keys == HeroAttack) return "attack";
        if (keys == HeroDeath) return "death";
        return keys[0];
    }

    static float SpriteFps(string k) => k == "attack" ? 22f : k == "death" ? 20f : k == "move" ? 22f : 14f;

    // Ennemis par cercle : trois communs puis le boss.
    static readonly string[][] CircleEnemies =
    {
        new[]{ "Rat", "Spider", "Snake", "Skeleton" },
        new[]{ "Wasp", "Bat", "SnakeAngry", "Slime" },
        new[]{ "Slime", "Frog", "Rat", "Zombie" },
        new[]{ "Skeleton", "Spider", "Wasp", "Dragon" },
        new[]{ "Zombie", "SnakeAngry", "SkelRogue", "Skeleton" },
        new[]{ "Skeleton", "Slime", "Frog", "Dragon" },
        new[]{ "Zombie", "SkelMage", "Wasp", "Zombie" },
        new[]{ "SnakeAngry", "SkelRogue", "Skeleton", "Dragon" },
        new[]{ "Skeleton", "SkelMage", "SkelRogue", "Dragon" },
        new[]{ "Zombie", "Skeleton", "SkelMage", "Dragon" },
    };

    class Fighter
    {
        public GameObject go;
        public AnimPlayer anim;
        public SpriteChar spr;
        public EnemyDef def;
        public bool boss, dead;
        public double hp, maxHp, atk;
        public float cooldown, hitTimer, deathTimer, halfLen = 0.3f;
        public string state = "";
        public float X { get => go.transform.localPosition.x; set { var p = go.transform.localPosition; p.x = value; go.transform.localPosition = p; } }
    }

    static readonly string[] HeroIdle = { "Idle_swordRight", "Idle" };
    static readonly string[] HeroRun = { "Run_swordRight", "Run" };
    static readonly string[] HeroAttack = { "1H_Melee_Attack_Chop", "1H_Melee_Attack_Slice_Diagonal", "Run_swordAttack" };
    // Objets portés par le chevalier KayKit, remplacés par l'équipement du joueur.
    static readonly string[] KnightHide = { "1H_Sword_Offhand", "Badge_Shield", "Rectangle_Shield", "Round_Shield", "Spike_Shield", "1H_Sword", "2H_Sword" };
    static readonly string[] HeroDeath = { "Death" };

    const float HeroSpeed = 2.4f, EnemySpeed = 1.8f, Engage = 0.75f, Spacing = 1.25f;
    const float HeroAttackInterval = 0.85f, EnemyAttackInterval = 1.3f;

    Camera cam;
    Transform world;
    Fighter hero;
    readonly List<Fighter> enemies = new List<Fighter>();
    readonly List<GameObject> chunks = new List<GameObject>();
    float builtUntil = -12f;
    int wave, waveCount;
    bool waveSpawned;
    float pauseTimer;
    string phase = "walk"; // walk, dead, clear
    int heroWeaponKey = -999, heroHelmetKey = -999;
    Material heroHelmBase;
    bool heroEngaged;
    int dungeon = -1;          // -1 = chemin principal, sinon type de donjon
    int missionSlot = -1;     // mission en cours (index dans la liste), -1 sinon
    float missionMult = 1f;   // ennemis regroupés quand l'escouade est trop nombreuse
    public bool InDungeon => dungeon >= 0 || missionSlot >= 0;
    static readonly Color MissionColor = new Color(0.75f, 0.45f, 1f);
    public System.Action<string> DungeonEnded;
    string petKey = "";
    int mountKey = -999;
    GameObject mountObj;
    AnimPlayer mountAnim;
    ProgressionData.MountDef mountDef;
    string mountState = "";
    readonly List<GameObject> petObjs = new List<GameObject>();
    readonly List<AnimPlayer> petAnims = new List<AnimPlayer>();
    readonly List<ProgressionData.PetDef> petDefs = new List<ProgressionData.PetDef>();
    Light rigLight;
    Color circleTint;

    // Interface du combat
    RectTransform overlayRoot, canvasRect;
    Text stageText, waveText, banner;
    float bannerTime;
    Font font;
    readonly List<Bar> bars = new List<Bar>();
    readonly List<Popup> popups = new List<Popup>();

    class Bar { public RectTransform root, fill; public Image fillImg; }

    // ---------- Compétences en combat ----------
    class SkillSlot
    {
        public int id = -1;
        public float cd, active, droneTimer;
        public GameObject fx;
        public Image bg, fill; public Text label; public Button btn; public RawImage icon;
    }
    readonly SkillSlot[] skillSlots = { new SkillSlot(), new SkillSlot(), new SkillSlot() };
    string skillKey = "";
    double buffAtk, buffHp;
    Material droneMat;
    class Popup { public Text t; public Vector3 world; public float time; }

    public static BattleWorld Build()
    {
        var go = new GameObject("BattleWorld");
        go.transform.position = new Vector3(1000f, 0f, 0f);
        var b = go.AddComponent<BattleWorld>();
        b.Create();
        return b;
    }

    void Create()
    {
        ForgeWorld.InitMaterials();
        world = transform;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var camGo = new GameObject("Caméra combat");
        cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.16f, 0.05f, 0.04f);
        cam.fieldOfView = 38f;
        cam.depth = 0f;
        cam.allowHDR = true;
        var data = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam);
        data.renderPostProcessing = true;

        rigLight = new GameObject("Lumière chemin").AddComponent<Light>();
        rigLight.type = LightType.Point;
        rigLight.color = new Color(1f, 0.45f, 0.2f);
        rigLight.range = 22f;
        rigLight.intensity = 14f;
        var fl = rigLight.gameObject.AddComponent<Flicker>(); fl.baseIntensity = 14f; fl.amount = 1.5f; fl.speed = 4f;

        BuildOverlay();
        BuildTerrain();
        BuildArena();
        SpawnHero();
        StartStage();
    }

    // ---------- Disposition à l'écran ----------
    public void SetBand(float y0, float y1)
    {
        cam.rect = new Rect(0f, y0, 1f, Mathf.Max(0.01f, y1 - y0));
        overlayRoot.anchorMin = new Vector2(0f, y0);
        overlayRoot.anchorMax = new Vector2(1f, y1);
        overlayRoot.offsetMin = overlayRoot.offsetMax = Vector2.zero;
    }

    public void SetVisible(bool v)
    {
        cam.enabled = v;
        overlayRoot.gameObject.SetActive(v);
    }

    // ---------- Décor ----------
    // Terrain continu : sol volcanique, chemin pavé et rivière de lave (des quads qui suivent le héros, textures défilantes).
    Transform ground, path, river;
    Material groundMat, pathMat, riverMat;
    Light riverLight;
    static readonly Color Basalt = new Color(0.3f, 0.18f, 0.15f);

    // ---------- Fond peint 2D (arènes Craftpix, hors dépôt) ----------
    static readonly string[] ArenaNames = { "chateau", "terrasse", "foret", "trone" };
    // Décor et teinte par cercle (Limbes -> Lucifer).
    static readonly int[] CircleArena = { 0, 1, 2, 3, 0, 1, 2, 3, 0, 3 };
    static readonly Color[] CircleArenaTint =
    {
        new Color(0.85f, 0.85f, 0.95f), new Color(1f, 0.85f, 0.95f), new Color(0.9f, 0.85f, 0.8f), new Color(1f, 0.9f, 0.85f),
        new Color(1f, 0.7f, 0.65f), new Color(1f, 0.75f, 0.55f), new Color(0.95f, 0.65f, 0.6f), new Color(0.85f, 0.7f, 1f),
        new Color(0.9f, 0.55f, 0.5f), new Color(1f, 0.55f, 0.45f),
    };
    readonly Texture2D[] arenaTex = new Texture2D[4];
    Transform bgQuad;
    Material bgMat;
    bool UseArena => bgQuad != null;

    void BuildArena()
    {
        if (!SpritesOn) return;
        for (int i = 0; i < ArenaNames.Length; i++)
        {
            arenaTex[i] = Resources.Load<Texture2D>("ForgeSprites/Arenas/" + ArenaNames[i]);
            if (arenaTex[i] == null) return;
            arenaTex[i].wrapModeU = TextureWrapMode.Mirror;
            arenaTex[i].wrapModeV = TextureWrapMode.Clamp;
        }
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(q.GetComponent<Collider>());
        q.name = "Fond peint";
        bgQuad = q.transform;
        bgQuad.SetParent(cam.transform, false);
        bgMat = ForgeWorld.Unlit(Color.white);
        bgMat.mainTexture = arenaTex[0];
        q.GetComponent<Renderer>().sharedMaterial = bgMat;
        q.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // Le décor 3D n'est plus affiché.
        foreach (var t in new[] { ground, path, river }) t.gameObject.SetActive(false);
        riverLight.enabled = false;
    }

    void SetArena(int index, Color tint)
    {
        if (!UseArena) return;
        bgMat.mainTexture = arenaTex[Mathf.Clamp(index, 0, arenaTex.Length - 1)];
        bgMat.SetColor("_BaseColor", tint);
        bgMat.color = tint;
    }

    // Le fond suit la caméra (plein cadre, loin derrière) et défile avec le héros.
    void UpdateArena(float heroX)
    {
        if (!UseArena) return;
        const float dist = 40f;
        float h = 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float w = h * Mathf.Max(0.5f, cam.aspect);
        float tex = arenaTex[0] != null ? arenaTex[0].width / (float)arenaTex[0].height : 16f / 9f;
        // Hauteur du cadre remplie ; la largeur visible = w / (h * tex) d'une image.
        bgQuad.localPosition = new Vector3(0f, 0f, dist);
        bgQuad.localRotation = Quaternion.identity;
        bgQuad.localScale = new Vector3(w, h, 1f);
        float span = w / (h * tex);
        bgMat.mainTextureScale = new Vector2(span, 1f);
        bgMat.mainTextureOffset = new Vector2(heroX / 15f, 0f);
    }

    void BuildTerrain()
    {
        ground = Quad("Sol volcanique", new Vector3(0, -0.02f, -2f), new Vector2(90f, 40f), groundMat = ForgeWorld.GroundMaterial(1f));
        path = Quad("Chemin", new Vector3(0, 0.005f, 0f), new Vector2(90f, 2.4f), pathMat = ForgeWorld.CobbleMaterial());
        river = Quad("Rivière de lave", new Vector3(0, 0.012f, 4.6f), new Vector2(90f, 4.2f), riverMat = ForgeWorld.LavaMaterial(new Color(2.6f, 1.3f, 0.7f)));
        riverLight = new GameObject("Lueur de la lave").AddComponent<Light>();
        riverLight.type = LightType.Point;
        riverLight.color = new Color(1f, 0.4f, 0.1f);
        riverLight.range = 9f;
        riverLight.intensity = 4f;
    }

    Transform Quad(string name, Vector3 pos, Vector2 size, Material m)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(q.GetComponent<Collider>());
        q.name = name;
        q.transform.SetParent(world, false);
        q.transform.localPosition = pos;
        q.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        q.transform.localScale = new Vector3(size.x, size.y, 1f);
        q.GetComponent<Renderer>().sharedMaterial = m;
        return q.transform;
    }

    void UpdateTerrain(float heroX)
    {
        float x = heroX + 2f;
        foreach (var t in new[] { ground, path, river })
        {
            var p = t.localPosition; p.x = x; t.localPosition = p;
        }
        groundMat.mainTextureScale = new Vector2(90f / 3.5f, 40f / 3.5f);
        groundMat.mainTextureOffset = new Vector2(Frac(x / 3.5f), 0f);
        pathMat.mainTextureScale = new Vector2(90f / 1.6f, 1.5f);
        pathMat.mainTextureOffset = new Vector2(Frac(x / 1.6f), 0f);
        riverMat.mainTextureScale = new Vector2(90f / 7f, 0.6f);
        riverMat.mainTextureOffset = new Vector2(Frac(x / 7f + Time.time * 0.015f), Frac(Time.time * 0.03f));
        riverLight.transform.position = world.TransformPoint(new Vector3(heroX + 2f, 1.2f, 3.4f));
        riverLight.intensity = 4f + Mathf.PerlinNoise(Time.time * 0.7f, 3.1f) * 2f;
    }

    static float Frac(float v) => v - Mathf.Floor(v);

    GameObject Rock(Transform c, Vector3 pos, float scale, float rot, float r)
    {
        var g = ModelLib.Raw("Props/Rock" + (1 + (int)(r * 97f) % 4), c, pos, rot, scale);
        if (g != null) ModelLib.Tint(g, Basalt, 0.75f);
        return g;
    }

    void BuildChunk(float x)
    {
        var c = new GameObject("Tronçon " + x).transform;
        c.SetParent(world, false);
        c.localPosition = new Vector3(x, 0f, 0f);
        int i = Mathf.RoundToInt(x / 2f);
        float r = Mathf.Abs(Mathf.Sin(i * 12.9898f) * 43758.5453f) % 1f;
        float r2 = Mathf.Abs(Mathf.Sin(i * 78.233f) * 12543.17f) % 1f;
        float r3 = Mathf.Abs(Mathf.Sin(i * 39.425f) * 24634.63f) % 1f;

        // Berge de la rivière et falaises au loin
        Rock(c, new Vector3(r * 1.5f - 0.7f, -0.1f, 2.45f + r2 * 0.3f), 1.1f + r3 * 0.9f, r * 360f, r);
        if (i % 2 == 0) Rock(c, new Vector3(r2 - 0.5f, -0.3f, 9f + r * 1.5f), 2.8f + r3 * 1.8f, r2 * 360f, r2);
        if (i % 3 == 0)
        {
            var sp = ModelLib.Raw("Props/Spike_Group", c, new Vector3(r3 - 0.5f, -0.2f, 8f), r * 360f, 1.2f + r * 0.8f);
            if (sp != null) ModelLib.Tint(sp, Basalt, 0.7f);
        }

        // Abords du chemin
        if (i % 4 == 2)
        {
            float side = (i % 8 == 2) ? 1.75f : -1.75f;
            ModelLib.Raw("Dungeon/Woodfire", c, new Vector3(0, 0, side), 0f, 0.8f);
            MakeFire(c, new Vector3(0, 0.35f, side));
        }
        if (i % 6 == 0) ModelLib.Raw("Dungeon/Column", c, new Vector3(0.3f, 0, 1.9f), r * 90f, 0.6f);
        if (r < 0.25f) ModelLib.Raw("Dungeon/Skull", c, new Vector3(0.5f, 0, -1.55f), r * 1440f);
        else if (r < 0.45f) Rock(c, new Vector3(-0.4f, 0, -1.7f), 0.7f, r * 900f, r);
        else if (r < 0.55f) ModelLib.Raw("Props/Spike_Single", c, new Vector3(0.3f, 0, -2.0f), 0f, 0.5f);
        else if (r < 0.62f) ModelLib.Raw("Dungeon/Bag_Coins", c, new Vector3(-0.5f, 0, 1.45f), r * 500f, 1.2f);

        // Premier plan
        if (r2 < 0.35f) Rock(c, new Vector3(0.2f, -0.05f, -4.0f - r3), 1.4f + r * 0.8f, r2 * 900f, r2);
        else if (r2 < 0.48f)
        {
            var sp = ModelLib.Raw("Props/Spike_Group", c, new Vector3(-0.3f, 0, -4.4f), r2 * 500f, 0.8f);
            if (sp != null) ModelLib.Tint(sp, Basalt, 0.7f);
        }
        else if (r2 < 0.56f) ModelLib.Raw("Dungeon/Trap_spikes", c, new Vector3(0f, -0.05f, -3.2f), 0f, 0.6f);
        chunks.Add(c.gameObject);
    }

    void MakeFire(Transform parent, Vector3 pos)
    {
        var go = new GameObject("Feu");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
        m.startColor = new Color(1f, 0.42f, 0.1f, 1f);
        m.maxParticles = 60;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var e = ps.emission; e.rateOverTime = 25f;
        var s = ps.shape; s.shapeType = ParticleSystemShapeType.Circle; s.radius = 0.15f; s.rotation = new Vector3(-90f, 0, 0);
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.3f, 0.1f), 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.1f));
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = ForgeWorld.Particle(new Color(2.5f, 2.5f, 2.5f, 1f));
        ps.Play();
    }

    void UpdateChunks(float heroX)
    {
        while (builtUntil < heroX + 26f) { BuildChunk(builtUntil); builtUntil += 2f; }
        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            if (chunks[i].transform.localPosition.x < heroX - 14f) { Destroy(chunks[i]); chunks.RemoveAt(i); }
        }
    }

    // ---------- Personnages ----------
    void SpawnHero()
    {
        hero = new Fighter();
        hero.spr = SpritesOn ? SpriteChar.Spawn(HeroSprite, 1.55f, world, false, CamPitch) : null;
        if (hero.spr != null) { hero.go = hero.spr.gameObject; return; }
        hero.go = ModelLib.SpawnKit("KayKit/Characters/Knight", 1.75f, world, KnightHide);
        hero.go.transform.localRotation = Quaternion.Euler(0, HeroYaw, 0);
        hero.anim = AnimPlayer.Attach(hero.go);
        RefreshHeroWeapon();
    }

    void RefreshHeroWeapon()
    {
        var eq = GameState.Data.equipped[0];
        int key = eq.valid ? eq.circle : -1;
        if (key == heroWeaponKey) return;
        heroWeaponKey = key;
        var palm = ModelLib.FindDeep(hero.go.transform, "handslot.r") ?? ModelLib.FindDeep(hero.go.transform, "Palm.R");
        if (palm == null) return;
        var old = palm.Find("Arme du héros");
        if (old != null) Destroy(old.gameObject);
        string path = eq.valid ? ForgeWorld.WeaponPath(eq.circle) : "Characters/Sword";
        var w = ModelLib.Spawn(path, 1.0f, null, true);
        w.name = "Arme du héros";
        w.transform.SetParent(palm, false);
        float ls = palm.lossyScale.x;
        w.transform.localScale = Vector3.one / Mathf.Max(0.0001f, ls);
        w.transform.localPosition = WeaponLocalPos / Mathf.Max(0.0001f, ls);
        w.transform.localRotation = Quaternion.Euler(WeaponLocalEuler);
        if (eq.valid) ModelLib.Tint(w, GameState.CircleColors[eq.circle], 0.35f);
    }

    // ---------- Monture : le héros se tient dessus ----------
    static readonly string[] MountMove = { "Run", "Walk", "Running", "Walking", "Flying", "Swim", "Jump", "*" };
    static readonly string[] MountIdle = { "Idle", "Flying", "Swim", "*" };

    void UpdateMount(bool moving)
    {
        // Héros en 2D : la monture 3D n'est pas affichée en combat (ses bonus restent actifs).
        if (hero.spr != null)
        {
            if (mountObj != null) { Destroy(mountObj); mountObj = null; mountAnim = null; mountKey = -999; }
            var p0 = hero.go.transform.localPosition; p0.y = 0f; hero.go.transform.localPosition = p0;
            return;
        }
        int id = GameState.Data.equippedMount;
        if (id != mountKey)
        {
            mountKey = id;
            if (mountObj != null) Destroy(mountObj);
            mountObj = null; mountAnim = null; mountDef = null; mountState = "";
            if (id >= 0 && GameState.FindMount(id) != null)
            {
                mountDef = ProgressionData.Mounts[id];
                mountObj = ModelLib.Spawn(mountDef.model, mountDef.size, world);
                mountObj.transform.localRotation = Quaternion.Euler(0f, HeroYaw, 0f);
                ModelLib.Tint(mountObj, ProgressionData.RarityColors[mountDef.rarity], 0.15f);
                mountAnim = AnimPlayer.Attach(mountObj);
            }
        }
        float ride = 0f;
        if (mountObj != null)
        {
            float bob = mountDef.fly > 0 ? Mathf.Sin(Time.time * 2f) * 0.08f : 0f;
            mountObj.transform.localPosition = new Vector3(hero.X - 0.1f, mountDef.fly + bob, 0f);
            ride = mountDef.fly + bob + mountDef.ride;
            string want = moving ? "move" : "idle";
            if (mountAnim != null && want != mountState) { mountState = want; mountAnim.Play(moving ? MountMove : MountIdle, true, 0.15f); }
        }
        var hp = hero.go.transform.localPosition;
        hp.y = hero.dead ? 0f : ride;
        hero.go.transform.localPosition = hp;
    }

    // ---------- Compagnons qui suivent le héros ----------
    static readonly Vector3[] PetOffsets = { new Vector3(-1.1f, 0f, 0.95f), new Vector3(-1.3f, 0f, -0.95f), new Vector3(-2.3f, 0f, 0.1f) };
    static readonly string[] PetMove = { "Walk", "Walking", "Run", "Running", "Flying", "Swim", "ArmatureAction", "Action", "*" };
    static readonly string[] PetIdle = { "Idle", "Flying", "Swim", "ArmatureAction", "Action", "*" };

    void RefreshPets()
    {
        string key = string.Join(",", GameState.Data.equippedPets);
        if (key == petKey) return;
        petKey = key;
        foreach (var g in petObjs) if (g != null) Destroy(g);
        petObjs.Clear(); petAnims.Clear(); petDefs.Clear();
        int slot = 0;
        foreach (int id in GameState.Data.equippedPets)
        {
            if (id < 0) continue;
            var def = ProgressionData.Pets[id];
            if (hero.spr != null)
            {
                // Héros en 2D : le compagnon devient une image plate face caméra, tournée vers la droite.
                var flat = FlatPet(def);
                if (flat != null)
                {
                    flat.transform.localPosition = new Vector3(hero.X + PetOffsets[slot].x, def.fly, PetOffsets[slot].z);
                    petObjs.Add(flat); petAnims.Add(null); petDefs.Add(def);
                    slot++;
                    continue;
                }
            }
            var go = ModelLib.Spawn(def.model, def.size * 1.5f, world);
            go.transform.localRotation = Quaternion.Euler(0f, HeroYaw, 0f);
            go.transform.localPosition = new Vector3(hero.X + PetOffsets[slot].x, def.fly, PetOffsets[slot].z);
            ModelLib.Tint(go, ProgressionData.RarityColors[def.rarity], 0.12f);
            petObjs.Add(go);
            petAnims.Add(AnimPlayer.Attach(go));
            petDefs.Add(def);
            slot++;
        }
    }

    GameObject FlatPet(ProgressionData.PetDef def)
    {
        var sp = ItemIcons.CreatureSprite(def.model, ProgressionData.RarityColors[def.rarity], 0.12f, new Vector3(6f, 125f, 0f));
        if (sp == null) return null;
        var go = new GameObject("Compagnon " + def.name);
        go.transform.SetParent(world, false);
        var body = new GameObject("Sprite").transform;
        body.SetParent(go.transform, false);
        body.localRotation = Quaternion.Euler(CamPitch, 0f, 0f);
        body.localScale = Vector3.one * Mathf.Clamp(def.size * 1.2f, 0.45f, 1.3f);
        var sr = body.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = 1;
        return go;
    }

    readonly Dictionary<AnimPlayer, string> petState = new Dictionary<AnimPlayer, string>();

    void UpdatePets(float dt, bool moving)
    {
        RefreshPets();
        for (int i = 0; i < petObjs.Count; i++)
        {
            var go = petObjs[i];
            var def = petDefs[i];
            var target = new Vector3(hero.X + PetOffsets[i].x, def.fly + (def.fly > 0 ? Mathf.Sin(Time.time * 2f + i) * 0.12f : 0f), PetOffsets[i].z);
            go.transform.localPosition = Vector3.Lerp(go.transform.localPosition, target, Mathf.Clamp01(dt * 6f));
            var a = petAnims[i];
            if (a == null && go.transform.childCount > 0)
            {
                // Compagnon plat : petits bonds quand il marche.
                var body = go.transform.GetChild(0);
                float hop = moving && def.fly <= 0f ? Mathf.Abs(Mathf.Sin(Time.time * 10f + i)) * 0.12f : 0f;
                body.localPosition = new Vector3(0f, hop, 0f);
            }
            if (a == null) continue;
            string want = moving ? "move" : "idle";
            petState.TryGetValue(a, out string cur);
            if (cur != want)
            {
                petState[a] = want;
                a.Play(moving ? PetMove : PetIdle, true, 0.15f);
            }
        }
    }

    void RefreshHeroHelmet()
    {
        var eq = GameState.Data.equipped[1];
        int key = eq.valid ? eq.circle : -1;
        if (key == heroHelmetKey) return;
        heroHelmetKey = key;
        // Chevalier KayKit : son propre heaume prend la couleur du cercle du casque équipé.
        var kitHelm = ModelLib.FindDeep(hero.go.transform, "Knight_Helmet");
        if (kitHelm != null)
        {
            var r = kitHelm.GetComponent<Renderer>();
            if (r != null)
            {
                if (heroHelmBase == null) heroHelmBase = r.sharedMaterial;
                var m = new Material(heroHelmBase);
                if (eq.valid) m.SetColor("_BaseColor", Color.Lerp(Color.white, GameState.CircleColors[eq.circle], 0.45f));
                r.sharedMaterial = m;
            }
            return;
        }
        var head = ModelLib.FindDeep(hero.go.transform, "Head");
        if (head == null) return;
        var old = head.Find("Casque du héros");
        if (old != null) Destroy(old.gameObject);
        var h = ModelLib.Spawn(ForgeWorld.HelmetPath(eq.valid ? eq.circle : 0), 0.42f, null, true, true);
        h.name = "Casque du héros";
        h.transform.position = head.position + Vector3.up * HelmetUp;
        h.transform.rotation = hero.go.transform.rotation * Quaternion.Euler(0f, HelmetYaw, 0f);
        h.transform.SetParent(head, true);
        if (eq.valid) ModelLib.Tint(h, GameState.CircleColors[eq.circle], 0.35f);
    }

    Fighter SpawnEnemy(string key, float x, bool boss)
    {
        var def = GetDef(key);
        var f = new Fighter { def = def, boss = boss };
        float h = def.height * (boss ? 1.7f : 1f);
        if (def.sprite != null) f.spr = SpriteChar.Spawn(def.sprite, h, world, true, CamPitch);
        if (f.spr != null)
        {
            f.go = f.spr.gameObject;
            f.go.transform.localPosition = new Vector3(x, 0f, 0f);
            f.halfLen = f.spr.HalfWidth * 0.85f;
            f.spr.SetTint(Color.Lerp(Color.white, circleTint, boss ? 0.3f : 0.12f));
        }
        else
        {
        f.go = def.Kit ? ModelLib.SpawnKit(def.path, h, world) : ModelLib.Spawn(def.path, h, world);
        f.go.transform.localPosition = new Vector3(x, def.fly, 0f);
        f.go.transform.localRotation = Quaternion.Euler(0, EnemyYaw, 0);
        if (def.Kit)
        {
            // Arme en main droite ; largeur fixe (le modèle est en T avant la première animation).
            var slot = ModelLib.FindDeep(f.go.transform, "handslot.r");
            var wp = def.weapon != null ? ModelLib.Prefab(def.weapon) : null;
            if (slot != null && wp != null) Instantiate(wp, slot, false);
            f.halfLen = 0.32f * (boss ? 1.7f : 1f);
            ModelLib.Tint(f.go, circleTint, boss ? 0.22f : 0.1f);
        }
        else
        {
            ModelLib.Tint(f.go, circleTint, boss ? 0.3f : 0.15f);
            var bb = ModelLib.WorldBounds(f.go);
            f.halfLen = Mathf.Clamp(bb.extents.x, 0.2f, 1.2f);
        }
        f.anim = AnimPlayer.Attach(f.go);
        }
        if (missionSlot >= 0)
        {
            f.maxHp = f.hp = GameState.MissionUnitHp(missionSlot) * missionMult;
            f.atk = GameState.MissionUnitAtk(missionSlot) * missionMult;
        }
        else if (dungeon >= 0)
        {
            double hm = dungeon == GameState.DungeonEgg ? 0.6 : dungeon == GameState.DungeonHammer ? 6.0 : 3.0;
            f.maxHp = f.hp = GameState.DungeonEnemyHp(dungeon, hm);
            f.atk = GameState.DungeonEnemyAtk(dungeon, dungeon == GameState.DungeonEgg ? 0.6 : 1.4);
        }
        else
        {
            f.maxHp = f.hp = GameState.EnemyHp(boss);
            f.atk = GameState.EnemyAtk(boss);
        }
        f.cooldown = 0.6f;
        Play(f, def.move, true);
        enemies.Add(f);
        return f;
    }

    void Play(Fighter f, string[] keys, bool loop, bool force = false)
    {
        if (f.spr != null)
        {
            string lk = Logical(keys);
            if (!force && loop && f.state == lk) return;
            f.state = lk;
            f.spr.Play(lk, loop, SpriteFps(lk));
            return;
        }
        if (f.anim == null) return;
        string k = keys[0];
        if (!force && loop && f.state == k) return;
        f.state = k;
        f.anim.Play(keys, loop, 0.12f, 1f, !loop);
    }

    // ---------- Donjons ----------
    public bool StartDungeon(int type)
    {
        if (InDungeon || !GameState.CanEnterDungeon(type)) return false;
        dungeon = type;
        pauseTimer = 0f;
        hero.go.SetActive(true);
        Play(hero, HeroIdle, true, true);
        StartStage();
        return true;
    }

    // ---------- Missions ----------
    public bool StartMission(int slot)
    {
        if (InDungeon || !GameState.CanStartMission) return false;
        missionSlot = slot;
        int units = GameState.MissionUnits(slot);
        missionMult = units / (float)Mathf.Min(units, MissionData.MaxShownUnits);
        pauseTimer = 0f;
        hero.go.SetActive(true);
        Play(hero, HeroIdle, true, true);
        StartStage();
        return true;
    }

    // ---------- Étapes et vagues ----------
    void StartStage()
    {
        foreach (var e in enemies) Destroy(e.go);
        enemies.Clear();
        int circle = GameState.StageCircle;
        circleTint = Color.Lerp(GameState.CircleColors[circle], new Color(0.6f, 0.05f, 0.02f), 0.4f);
        var lavaCol = Color.Lerp(new Color(1f, 0.3f, 0.05f), GameState.CircleColors[circle], 0.35f) * 1.8f;
        riverMat.SetColor("_BaseColor", Color.Lerp(new Color(2.6f, 1.3f, 0.7f), GameState.CircleColors[circle] * 2.6f, 0.3f));
        wave = 0;
        waveCount = missionSlot >= 0 ? 1 : dungeon == GameState.DungeonHammer ? 1 : dungeon >= 0 ? 3 : GameState.IsBossStage ? 2 : 3;
        waveSpawned = false;
        if (missionSlot >= 0)
        {
            circleTint = Color.Lerp(MissionColor, new Color(0.4f, 0.05f, 0.1f), 0.35f);
            riverMat.SetColor("_BaseColor", Color.Lerp(new Color(2.6f, 1.3f, 0.7f), MissionColor * 2.6f, 0.45f));
        }
        else if (dungeon >= 0)
        {
            var dc = GameState.DungeonColors[dungeon];
            circleTint = Color.Lerp(dc, new Color(0.5f, 0.05f, 0.02f), 0.3f);
            riverMat.SetColor("_BaseColor", Color.Lerp(new Color(2.6f, 1.3f, 0.7f), dc * 2.6f, 0.45f));
        }
        if (missionSlot >= 0) SetArena(3, new Color(0.85f, 0.7f, 1f));
        else if (dungeon >= 0) SetArena(dungeon % 4, Color.Lerp(Color.white, GameState.DungeonColors[dungeon], 0.35f));
        else SetArena(CircleArena[circle], CircleArenaTint[circle]);
        ResetSkills();
        hero.maxHp = hero.hp = GameState.TotalHp();
        hero.atk = GameState.TotalAtk();
        hero.dead = false;
        phase = "walk";
        if (missionSlot >= 0)
        {
            stageText.text = MissionData.SquadNames[GameState.Data.missionSquad[missionSlot]] + "  niv. " + GameState.Data.missionLevel[missionSlot];
            ShowBanner("MISSION !", MissionColor);
        }
        else if (dungeon >= 0)
        {
            stageText.text = GameState.DungeonNames[dungeon] + "  niv. " + (GameState.Data.dungeonLevel[dungeon] + 1);
            ShowBanner("DONJON !", GameState.DungeonColors[dungeon]);
        }
        else
        {
            stageText.text = GameState.StageLabel;
            if (GameState.IsBossStage) ShowBanner("BOSS !", new Color(1f, 0.3f, 0.2f));
        }
    }

    static readonly string[] EggDungeonPool = { "Rat", "Spider", "Frog" };

    void SpawnWave()
    {
        float x = hero.X + 9f;
        if (missionSlot >= 0)
        {
            int squad = GameState.Data.missionSquad[missionSlot];
            int shown = Mathf.Min(GameState.MissionUnits(missionSlot), MissionData.MaxShownUnits);
            bool big = shown <= 2;
            for (int i = 0; i < shown; i++) SpawnEnemy(MissionData.SquadModel[squad], x + i * 1.3f, big);
            waveSpawned = true;
            return;
        }
        if (dungeon >= 0)
        {
            if (dungeon == GameState.DungeonHammer) SpawnEnemy("Zombie", x, true);
            else if (dungeon == GameState.DungeonEgg)
                for (int i = 0; i < 4; i++) SpawnEnemy(EggDungeonPool[Random.Range(0, 3)], x + i * 1.4f, false);
            else SpawnEnemy(dungeon == GameState.DungeonPotion ? "Slime" : "Skeleton", x, true);
            waveSpawned = true;
            return;
        }
        var pool = (SpritesOn ? SpriteCircleEnemies : CircleEnemies)[GameState.StageCircle];
        bool bossWave = GameState.IsBossStage && wave == waveCount - 1;
        if (bossWave) { SpawnEnemy(pool[3], x, true); }
        else
        {
            int count = 2 + (wave == 2 ? 1 : 0);
            for (int i = 0; i < count; i++)
                SpawnEnemy(pool[Random.Range(0, 3)], x + i * Spacing * 1.3f, false);
        }
        waveSpawned = true;
    }

    // ---------- Boucle de combat ----------
    void Update()
    {
        float dt = Time.deltaTime;
        RefreshHeroWeapon();
        RefreshHeroHelmet();

        // Les stats suivent l'équipement en temps réel.
        double newMax = GameState.TotalHp() + buffHp;
        if (System.Math.Abs(newMax - hero.maxHp) > 0.5 && !hero.dead)
        {
            hero.hp = hero.hp / hero.maxHp * newMax;
            hero.maxHp = newMax;
        }
        hero.atk = GameState.TotalAtk() + buffAtk;

        // Régénération (statistique secondaire) : % de la vie max par seconde.
        if (!hero.dead && phase == "walk" && hero.hp < hero.maxHp)
            hero.hp = System.Math.Min(hero.maxHp, hero.hp + hero.maxHp * GameState.RegenPerSecond * dt);

        if (pauseTimer > 0f)
        {
            pauseTimer -= dt;
            if (pauseTimer <= 0f)
            {
                if (phase == "dead") { hero.go.SetActive(true); Play(hero, HeroIdle, true, true); StartStage(); }
                else if (phase == "clear") StartStage();
            }
        }

        Fighter front = null;
        foreach (var e in enemies) if (!e.dead) { front = e; break; }

        if (phase == "walk")
        {
            if (front == null)
            {
                if (waveSpawned && enemies.TrueForAll(x => x.dead))
                {
                    waveSpawned = false;
                    wave++;
                    if (!InDungeon) GameState.WaveCleared();
                    if (wave >= waveCount && missionSlot >= 0)
                    {
                        string reward = GameState.MissionWon(missionSlot);
                        ShowBanner("Mission réussie !", MissionColor);
                        DungeonEnded?.Invoke("Mission réussie : " + reward);
                        missionSlot = -1;
                        phase = "clear";
                        pauseTimer = 2.2f;
                    }
                    else if (wave >= waveCount && dungeon >= 0)
                    {
                        string reward = GameState.DungeonWon(dungeon);
                        ShowBanner("Donjon réussi !  " + reward, new Color(1f, 0.85f, 0.35f));
                        DungeonEnded?.Invoke("Donjon réussi : " + reward);
                        dungeon = -1;
                        phase = "clear";
                        pauseTimer = 2.2f;
                    }
                    else if (wave >= waveCount)
                    {
                        long shellsBefore = GameState.Data.eggshells;
                        int gems = GameState.StageCleared();
                        Sfx.Fanfare();
                        long shells = GameState.Data.eggshells - shellsBefore;
                        ShowBanner("Étape réussie !" + (gems > 0 ? "  +" + gems + " gemmes" : "") + (shells > 0 ? "  +" + shells + " coquilles" : ""), new Color(1f, 0.85f, 0.35f));
                        phase = "clear";
                        pauseTimer = 1.6f;
                    }
                }
                if (phase == "walk" && !waveSpawned) SpawnWave();
            }

            float gap = hero.spr != null ? Engage + 0.3f : Engage;
            bool engaged = front != null && front.X - hero.X <= gap + front.halfLen + 0.05f;
            heroEngaged = engaged;
            if (!engaged)
            {
                hero.X += HeroSpeed * dt;
                Play(hero, HeroRun, true);
            }
            else
            {
                hero.cooldown -= dt;
                if (hero.cooldown <= 0f)
                {
                    hero.cooldown = GameState.AttackInterval;
                    Play(hero, HeroAttack, false, true);
                    hero.hitTimer = 0.28f;
                }
                if (hero.hitTimer > 0f)
                {
                    hero.hitTimer -= dt;
                    if (hero.hitTimer <= 0f) HeroHits(front);
                }
                if (hero.cooldown < GameState.AttackInterval - 0.6f) Play(hero, HeroIdle, true);
            }
        }
        else if (phase == "clear")
        {
            hero.X += HeroSpeed * dt;
            Play(hero, HeroRun, true);
        }

        // Ennemis
        int idx = 0;
        float queueX = hero.X + (hero.spr != null ? Engage + 0.3f : Engage);
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e.dead)
            {
                e.deathTimer += dt;
                if (e.spr != null) e.spr.Alpha = Mathf.Clamp01(1f - (e.deathTimer - 1.2f));
                else if (e.deathTimer > 0.9f)
                {
                    var p = e.go.transform.localPosition;
                    p.y -= dt * 0.8f;
                    e.go.transform.localPosition = p;
                }
                if (e.deathTimer > 2.2f) { Destroy(e.go); enemies.RemoveAt(i); i--; }
                continue;
            }
            float target = queueX + e.halfLen;
            queueX = target + e.halfLen + (e.spr != null ? 0.55f : 0.35f);
            if (e.X > target + 0.02f)
            {
                e.X = Mathf.Max(target, e.X - EnemySpeed * dt);
                Play(e, e.def.move, true);
            }
            else if (idx == 0 && phase == "walk" && !hero.dead)
            {
                e.cooldown -= dt;
                if (e.cooldown <= 0f)
                {
                    e.cooldown = EnemyAttackInterval;
                    Play(e, e.def.attack, false, true);
                    e.hitTimer = 0.35f;
                }
                if (e.hitTimer > 0f)
                {
                    e.hitTimer -= dt;
                    if (e.hitTimer <= 0f) EnemyHits(e);
                }
                if (e.cooldown < EnemyAttackInterval - 0.8f) Play(e, e.def.idle, true);
            }
            else Play(e, e.def.idle, true);
            idx++;
        }

        UpdateSkills(dt);
        if (!UseArena) { UpdateChunks(hero.X); UpdateTerrain(hero.X); }
        UpdatePets(dt, phase == "walk" && !heroEngaged || phase == "clear");
        UpdateMount(phase == "walk" && !heroEngaged || phase == "clear");
        // Vue de trois quarts, comme dans Forge Master : le héros à gauche, les ennemis arrivent par la droite.
        var camPos = world.TransformPoint(new Vector3(hero.X + 2.4f, 8.2f, -9.4f));
        cam.transform.position = camPos;
        cam.transform.rotation = Quaternion.Euler(CamPitch, 0f, 0f);
        rigLight.transform.position = world.TransformPoint(new Vector3(hero.X + 1.5f, 3.2f, -2.4f));
        UpdateArena(hero.X);

        UpdateOverlay(dt);
    }

    void HeroHits(Fighter target)
    {
        if (target == null || target.dead) return;
        DealHit(target, false);
        // Double frappe : un second coup immédiat.
        if (!target.dead && Random.value < GameState.DoubleChance) DealHit(target, true);
    }

    void DealHit(Fighter target, bool isDouble)
    {
        bool crit = Random.value < GameState.CritChance;
        double dmg = hero.atk * Random.Range(0.9f, 1.1f) * (crit ? GameState.CritMult : 1.0);
        target.hp -= dmg;
        if (target.spr != null) target.spr.Flash();
        if (crit) Sfx.Crit(); else Sfx.Hit();
        float h = target.def.height * (target.boss ? 1.7f : 1f) + target.def.fly * 0.2f + (isDouble ? 0.35f : 0f);
        string label = (isDouble ? "DOUBLE " : "") + (crit ? "CRITIQUE " : "") + GameState.Fmt(dmg);
        ShowPopup(target.go.transform.position + Vector3.up * h, label,
            crit ? new Color(1f, 0.55f, 0.1f) : isDouble ? new Color(0.6f, 0.85f, 1f) : Color.white, crit ? 46 : 36);

        // Vol de vie
        float ls = GameState.LifeSteal;
        if (ls > 0f && !hero.dead)
        {
            double heal = System.Math.Min(hero.maxHp - hero.hp, dmg * ls);
            if (heal >= 1) { hero.hp += heal; ShowPopup(hero.go.transform.position + Vector3.up * 2.2f, "+" + GameState.Fmt(heal), new Color(0.4f, 1f, 0.45f), 28); }
        }

        if (target.hp <= 0) KillEnemy(target);
    }

    void KillEnemy(Fighter target)
    {
        target.dead = true;
        target.hp = 0;
        target.state = "death";
        float d = target.spr != null ? target.spr.Play("death", false, SpriteFps("death"))
                : target.anim != null ? target.anim.Play(target.def.death, false, 0.1f) : 0f;
        if (d <= 0f) target.go.transform.localRotation = Quaternion.Euler(0, EnemyYaw, 80f);
        Sfx.Coin();
        long gold = GameState.KillGold(target.boss);
        GameState.AddGold(gold);
        ShowPopup(target.go.transform.position + Vector3.up * 0.4f, "+" + GameState.Fmt(gold) + " or", new Color(1f, 0.82f, 0.3f), 34);
        hero.cooldown = Mathf.Min(hero.cooldown, 0.35f);
    }

    void EnemyHits(Fighter e)
    {
        if (hero.dead) return;
        if (Random.value < GameState.BlockChance)
        {
            ShowPopup(hero.go.transform.position + Vector3.up * 1.9f, "BLOQUÉ", new Color(0.7f, 0.85f, 1f), 30);
            Sfx.Block();
            return;
        }
        double dmg = e.atk * Random.Range(0.9f, 1.1f);
        hero.hp -= dmg;
        if (hero.spr != null) hero.spr.Flash();
        Sfx.Hurt();
        ShowPopup(hero.go.transform.position + Vector3.up * 1.9f, "-" + GameState.Fmt(dmg), new Color(1f, 0.35f, 0.3f), 32);
        if (hero.hp <= 0)
        {
            hero.hp = 0;
            hero.dead = true;
            if (hero.spr != null) hero.spr.Play("death", false, SpriteFps("death"));
            else if (hero.anim != null) hero.anim.Play(HeroDeath, false, 0.1f);
            hero.state = "dead";
            phase = "dead";
            ResetSkills();
            pauseTimer = 2.5f;
            if (missionSlot >= 0)
            {
                ShowBanner("Mission échouée… l'énergie est conservée", new Color(1f, 0.4f, 0.35f));
                DungeonEnded?.Invoke("Mission échouée : l'énergie est conservée");
                missionSlot = -1;
            }
            else if (dungeon >= 0)
            {
                ShowBanner("Échec… la clé n'est pas consommée", new Color(1f, 0.4f, 0.35f));
                DungeonEnded?.Invoke("Donjon échoué : la clé est conservée");
                dungeon = -1;
            }
            else ShowBanner("Défaite… forge un meilleur équipement !", new Color(1f, 0.4f, 0.35f));
        }
    }

    // ---------- Compétences ----------
    const float SkillRange = 7.5f;

    void ResetSkills()
    {
        foreach (var sl in skillSlots)
        {
            if (sl.fx != null) Destroy(sl.fx);
            sl.fx = null;
            sl.active = 0f;
            sl.droneTimer = 0f;
            if (sl.id >= 0) sl.cd = SkillData.Skills[sl.id].cooldown * 0.5f;
        }
        buffAtk = buffHp = 0;
    }

    void RefreshSkillSlots()
    {
        var eq = GameState.Data.equippedSkills;
        string key = string.Join(",", eq);
        if (key == skillKey) return;
        skillKey = key;
        for (int i = 0; i < 3; i++)
        {
            var sl = skillSlots[i];
            int id = GameState.FindSkill(eq[i]) != null ? eq[i] : -1;
            if (id == sl.id) continue;
            if (sl.fx != null) Destroy(sl.fx);
            sl.fx = null; sl.active = 0f;
            sl.id = id;
            if (id >= 0) sl.cd = SkillData.Skills[id].cooldown * 0.5f;
        }
    }

    bool InSkillRange(Fighter f) => f != null && !f.dead && f.X - hero.X < SkillRange;

    Fighter FrontEnemy()
    {
        foreach (var e in enemies) if (!e.dead && e.X - hero.X < SkillRange) return e;
        return null;
    }

    void UpdateSkills(float dt)
    {
        RefreshSkillSlots();
        bool fighting = phase == "walk" && !hero.dead && FrontEnemy() != null;
        double atk = 0, hp = 0;
        for (int i = 0; i < 3; i++)
        {
            var sl = skillSlots[i];
            if (sl.id < 0) continue;
            var def = SkillData.Skills[sl.id];
            var own = GameState.FindSkill(sl.id);
            if (own == null) continue;
            if (sl.active > 0f)
            {
                sl.active -= dt;
                if (def.kind == SkillData.Rage || def.kind == SkillData.Aura) atk += GameState.SkillDamage(own);
                if (def.kind == SkillData.Aura) hp += GameState.SkillHealth(own);
                if (def.kind == SkillData.Heal && !hero.dead)
                {
                    double heal = GameState.SkillHealth(own) / def.duration * dt;
                    hero.hp = System.Math.Min(hero.maxHp, hero.hp + heal);
                }
                if (def.kind == SkillData.Drone && sl.fx != null)
                {
                    sl.fx.transform.localPosition = new Vector3(hero.X - 0.4f, 2.4f + Mathf.Sin(Time.time * 3f) * 0.15f, -0.5f);
                    sl.droneTimer -= dt;
                    if (sl.droneTimer <= 0f && fighting)
                    {
                        sl.droneTimer = 2f;
                        FireAt(FrontEnemy(), sl.fx.transform.position, def.color, GameState.SkillDamage(own), 14f, 0.1f, 0.8f);
                    }
                }
                if (sl.active <= 0f && sl.fx != null) { if (def.kind == SkillData.Drone) Destroy(sl.fx); sl.fx = null; }
            }
            else if (fighting)
            {
                sl.cd -= dt;
                if (sl.cd <= 0f) Cast(sl, def, own);
            }
            // Affichage du bouton
            var c = def.color;
            bool ready = sl.active <= 0f && sl.cd <= 0.05f;
            var tex = SkillData.Icon(sl.id);
            sl.icon.texture = tex;
            sl.icon.gameObject.SetActive(tex != null);
            if (tex != null)
            {
                // Icône visible ; voile sombre circulaire pendant la recharge, voile coloré pendant l'effet.
                if (sl.active > 0f) { sl.fill.fillAmount = sl.active / Mathf.Max(0.1f, def.duration); sl.fill.color = new Color(c.r, c.g, c.b, 0.3f); }
                else { sl.fill.fillAmount = Mathf.Clamp01(sl.cd / def.cooldown); sl.fill.color = new Color(0f, 0f, 0f, 0.62f); }
                sl.bg.color = ready || sl.active > 0f ? Color.Lerp(c, Color.white, 0.25f) : new Color(0.08f, 0.04f, 0.04f, 0.85f);
            }
            else
            {
                if (sl.active > 0f) sl.fill.fillAmount = sl.active / Mathf.Max(0.1f, def.duration);
                else sl.fill.fillAmount = 1f - Mathf.Clamp01(sl.cd / def.cooldown);
                sl.fill.color = sl.active > 0f ? new Color(c.r, c.g, c.b, 0.95f) : new Color(c.r * 0.55f, c.g * 0.55f, c.b * 0.55f, ready ? 1f : 0.75f);
            }
        }
        buffAtk = atk;
        buffHp = hp;
        for (int i = 0; i < 3; i++)
        {
            var sl = skillSlots[i];
            bool has = sl.id >= 0;
            sl.bg.gameObject.SetActive(has);
            if (!has) continue;
            var def = SkillData.Skills[sl.id];
            string t = def.name;
            if (SkillData.Icon(sl.id) != null)
            {
                sl.label.fontSize = 38;
                sl.label.text = sl.active > 0f ? "<b>" + Mathf.CeilToInt(sl.active) + "</b>" : sl.cd > 0.05f ? Mathf.CeilToInt(sl.cd).ToString() : "";
                continue;
            }
            sl.label.fontSize = 20;
            sl.label.text = sl.active > 0f ? "<b>" + t + "</b>\n" + Mathf.CeilToInt(sl.active) + "s" : t + (sl.cd > 0.05f ? "\n<size=24>" + Mathf.CeilToInt(sl.cd) + "</size>" : "");
        }
    }

    void ManualCast(int i)
    {
        var sl = skillSlots[i];
        if (sl.id < 0 || sl.active > 0f || phase != "walk" || hero.dead || FrontEnemy() == null) return;
        var own = GameState.FindSkill(sl.id);
        if (own == null) return;
        if (sl.cd > 0.05f) return;
        Cast(sl, SkillData.Skills[sl.id], own);
    }

    void Cast(SkillSlot sl, SkillData.SkillDef def, OwnedSkill own)
    {
        sl.cd = def.cooldown;
        Sfx.Cast();
        Vector3 heroPos = hero.go.transform.position;
        ShowPopup(heroPos + Vector3.up * 2.7f, def.name, Color.Lerp(def.color, Color.white, 0.3f), 30);
        double dmg = GameState.SkillDamage(own);
        switch (def.kind)
        {
            case SkillData.Strike:
            {
                var t = FrontEnemy();
                bool sky = sl.id == 13 || sl.id == 14;
                Vector3 from = sky && t != null ? t.go.transform.position + new Vector3(-0.5f, 7f, 0f) : heroPos + Vector3.up * 1.2f;
                FireAt(t, from, def.color, dmg, sky ? 22f : 11f, sky ? 0f : 0.5f, sl.id == 13 ? 1.8f : 1.1f);
                break;
            }
            case SkillData.Volley:
            {
                if (sl.id == 2 || sl.id == 12)
                {
                    // Onde de choc depuis le héros : touche immédiatement tous les ennemis proches.
                    SkillFx.Shockwave(heroPos + Vector3.up * 0.3f, def.color, SkillRange * 0.8f);
                    foreach (var e in enemies.ToArray()) if (InSkillRange(e)) SkillHit(e, dmg, def.color);
                }
                else
                {
                    int n = 0;
                    foreach (var e in enemies.ToArray())
                    {
                        if (!InSkillRange(e)) continue;
                        var from = e.go.transform.position + new Vector3(-1.5f - n * 0.4f, 7f + n * 0.6f, Random.Range(-0.5f, 0.5f));
                        FireAt(e, from, def.color, dmg, 12f + n * 1.5f, 0f, sl.id == 11 ? 2.2f : 1.3f, false);
                        n++;
                    }
                }
                break;
            }
            case SkillData.Heal:
            case SkillData.Rage:
            case SkillData.Aura:
                sl.active = def.duration;
                if (sl.fx != null) Destroy(sl.fx);
                sl.fx = SkillFx.Aura(hero.go.transform, def.color, def.duration, def.kind == SkillData.Heal ? 0.45f : 0.65f);
                break;
            case SkillData.Drone:
            {
                sl.active = def.duration;
                sl.droneTimer = 0.3f;
                if (sl.fx != null) Destroy(sl.fx);
                var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(orb.GetComponent<Collider>());
                orb.name = "Œil de Lucifer";
                orb.transform.SetParent(world, false);
                orb.transform.localScale = Vector3.one * 0.35f;
                orb.transform.localPosition = new Vector3(hero.X - 0.4f, 2.4f, -0.5f);
                if (droneMat == null) droneMat = ForgeWorld.Unlit(new Color(def.color.r * 3f, def.color.g * 3f, def.color.b * 3f));
                orb.GetComponent<Renderer>().sharedMaterial = droneMat;
                SkillFx.Aura(orb.transform, def.color, def.duration, 0.12f);
                sl.fx = orb;
                break;
            }
        }
    }

    // Projectile vers un ennemi ; à l'impact, les dégâts vont à la cible (ou au nouvel ennemi de tête si elle est déjà morte).
    void FireAt(Fighter target, Vector3 from, Color c, double dmg, float speed, float arc, float size, bool retarget = true)
    {
        if (target == null) return;
        var t = target;
        float h = t.def.height * (t.boss ? 1.7f : 1f) * 0.5f;
        Vector3 last = t.go.transform.position + Vector3.up * (h + t.def.fly * 0.3f);
        SkillFx.Projectile(from, () =>
        {
            if (t.go != null && !t.dead) last = t.go.transform.position + Vector3.up * (h + t.def.fly * 0.3f);
            return last;
        }, c, speed, arc, size, () =>
        {
            var hit = t;
            if ((hit.dead || hit.go == null) && retarget) hit = FrontEnemy();
            if (hit != null && !hit.dead && hit.go != null) SkillHit(hit, dmg, c);
        });
    }

    void SkillHit(Fighter target, double dmg, Color c)
    {
        if (target == null || target.dead) return;
        dmg *= Random.Range(0.95f, 1.05f);
        target.hp -= dmg;
        if (target.spr != null) target.spr.Flash();
        float h = target.def.height * (target.boss ? 1.7f : 1f) + target.def.fly * 0.2f + 0.5f;
        ShowPopup(target.go.transform.position + Vector3.up * h, GameState.Fmt(dmg), Color.Lerp(c, Color.white, 0.2f), 42);
        if (target.hp <= 0) KillEnemy(target);
    }

    // ---------- Interface du combat ----------
    void BuildOverlay()
    {
        var cgo = new GameObject("Interface combat");
        cgo.transform.SetParent(transform, false);
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect = (RectTransform)cgo.transform;

        overlayRoot = NewRect("Zone combat", cgo.transform);
        stageText = NewText(overlayRoot, 46, TextAnchor.UpperCenter, new Color(1f, 0.85f, 0.6f));
        Anchor(stageText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -18));
        waveText = NewText(overlayRoot, 30, TextAnchor.UpperCenter, new Color(0.95f, 0.9f, 0.85f));
        Anchor(waveText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -130), new Vector2(0, -82));
        banner = NewText(overlayRoot, 50, TextAnchor.MiddleCenter, Color.white);
        Anchor(banner.rectTransform, new Vector2(0, 0.55f), new Vector2(1, 0.85f), Vector2.zero, Vector2.zero);

        // Les 3 compétences équipées (en bas à droite du chemin) : se lancent seules, ou d'une touche quand elles sont prêtes.
        cgo.AddComponent<GraphicRaycaster>();
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            var sl = skillSlots[i];
            var rt = NewRect("Compétence " + i, overlayRoot);
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(124, 124);
            rt.anchoredPosition = new Vector2(-24 - (2 - i) * 138, 18);
            sl.bg = rt.gameObject.AddComponent<Image>();
            sl.bg.sprite = ForgeUI.Circle();
            sl.bg.color = new Color(0.08f, 0.04f, 0.04f, 0.85f);
            sl.btn = rt.gameObject.AddComponent<Button>();
            sl.btn.transition = Selectable.Transition.None;
            sl.btn.onClick.AddListener(() => ManualCast(idx));
            // Icône peinte, découpée en rond (sous la recharge).
            var mrt = NewRect("Masque", rt);
            mrt.offsetMin = new Vector2(8, 8); mrt.offsetMax = new Vector2(-8, -8);
            var mimg = mrt.gameObject.AddComponent<Image>(); mimg.sprite = ForgeUI.Circle(); mimg.raycastTarget = false;
            mrt.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            sl.icon = NewRect("Icône", mrt).gameObject.AddComponent<RawImage>();
            sl.icon.raycastTarget = false;
            var frt = NewRect("Recharge", rt);
            frt.offsetMin = new Vector2(8, 8); frt.offsetMax = new Vector2(-8, -8);
            sl.fill = frt.gameObject.AddComponent<Image>();
            sl.fill.sprite = ForgeUI.Circle();
            sl.fill.type = Image.Type.Filled;
            sl.fill.fillMethod = Image.FillMethod.Radial360;
            sl.fill.fillOrigin = (int)Image.Origin360.Top;
            sl.fill.raycastTarget = false;
            sl.label = NewText(rt, 20, TextAnchor.MiddleCenter, Color.white);
            sl.label.horizontalOverflow = HorizontalWrapMode.Wrap;
            sl.label.rectTransform.offsetMin = new Vector2(6, 6); sl.label.rectTransform.offsetMax = new Vector2(-6, -6);
        }
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static void Anchor(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;
    }

    Text NewText(Transform parent, int size, TextAnchor a, Color c)
    {
        var rt = NewRect("Texte", parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.fontSize = size; t.alignment = a; t.color = c; t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        var sh = rt.gameObject.AddComponent<Outline>();
        sh.effectColor = new Color(0, 0, 0, 0.85f); sh.effectDistance = new Vector2(2, -2);
        return t;
    }

    Bar GetBar(int i)
    {
        while (bars.Count <= i)
        {
            var b = new Bar();
            b.root = NewRect("Barre", overlayRoot);
            b.root.anchorMin = b.root.anchorMax = new Vector2(0.5f, 0.5f);
            b.root.sizeDelta = new Vector2(130, 16);
            var bg = b.root.gameObject.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f); bg.raycastTarget = false;
            b.fill = NewRect("Vie", b.root);
            b.fill.offsetMin = new Vector2(2, 2); b.fill.offsetMax = new Vector2(-2, -2);
            b.fillImg = b.fill.gameObject.AddComponent<Image>();
            b.fillImg.raycastTarget = false;
            bars.Add(b);
        }
        return bars[i];
    }

    bool ToLocal(Vector3 worldPos, out Vector2 local)
    {
        local = Vector2.zero;
        var sp = cam.WorldToScreenPoint(worldPos);
        if (sp.z < 0) return false;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRoot, sp, null, out local);
    }

    void PlaceBar(Bar b, Fighter f, Color col, float width)
    {
        float h = f == hero ? (hero.spr != null ? 1.55f : 1.75f) : f.def.height * (f.boss ? 1.7f : 1f) + f.def.fly;
        Vector2 local;
        bool ok = ToLocal(f.go.transform.position + Vector3.up * (f == hero ? h : h - f.def.fly + 0.15f + f.def.fly), out local);
        b.root.gameObject.SetActive(ok);
        if (!ok) return;
        b.root.anchoredPosition = local + new Vector2(0, 14);
        b.root.sizeDelta = new Vector2(width, f.boss ? 20 : 16);
        float r = (float)System.Math.Max(0, System.Math.Min(1, f.hp / System.Math.Max(1, f.maxHp)));
        b.fill.anchorMax = new Vector2(r, 1);
        b.fillImg.color = col;
    }

    void UpdateOverlay(float dt)
    {
        var sbw = new System.Text.StringBuilder();
        for (int i = 0; i < waveCount; i++)
        {
            bool bossDot = GameState.IsBossStage && i == waveCount - 1;
            string col = i < wave ? "#5FB8FF" : i == wave ? "#FFFFFF" : "#665555";
            sbw.Append("<color=" + col + ">" + (bossDot ? "☠" : "●") + "</color>");
            if (i < waveCount - 1) sbw.Append(i < wave ? "<color=#5FB8FF> ━━ </color>" : "<color=#665555> ━━ </color>");
        }
        waveText.text = sbw.ToString();
        int bi = 0;
        PlaceBar(GetBar(bi++), hero, new Color(0.3f, 0.85f, 0.35f), 150);
        foreach (var e in enemies)
        {
            if (e.dead) continue;
            PlaceBar(GetBar(bi++), e, e.boss ? new Color(1f, 0.25f, 0.1f) : new Color(0.9f, 0.2f, 0.15f), e.boss ? 220 : 110);
        }
        for (int i = bi; i < bars.Count; i++) bars[i].root.gameObject.SetActive(false);

        for (int i = popups.Count - 1; i >= 0; i--)
        {
            var p = popups[i];
            p.time += dt;
            Vector2 local;
            if (ToLocal(p.world + Vector3.up * p.time * 0.8f, out local)) p.t.rectTransform.anchoredPosition = local;
            var c = p.t.color; c.a = Mathf.Clamp01(1.4f - p.time * 1.2f); p.t.color = c;
            if (p.time > 1.2f) { Destroy(p.t.gameObject); popups.RemoveAt(i); }
        }

        if (bannerTime > 0f)
        {
            bannerTime -= dt;
            var c = banner.color; c.a = Mathf.Clamp01(bannerTime / 0.5f); banner.color = c;
        }
    }

    void ShowPopup(Vector3 worldPos, string txt, Color col, int size)
    {
        var t = NewText(overlayRoot, size, TextAnchor.MiddleCenter, col);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 60);
        t.text = txt;
        popups.Add(new Popup { t = t, world = worldPos + new Vector3(Random.Range(-0.2f, 0.2f), 0, 0) });
    }

    void ShowBanner(string txt, Color col)
    {
        banner.text = txt;
        banner.color = col;
        bannerTime = 2f;
    }
}
