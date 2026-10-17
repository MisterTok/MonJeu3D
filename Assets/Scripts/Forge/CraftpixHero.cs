using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Héros Craftpix à pièces interchangeables (Tools/make_craftpix_rig.py) : rejoue les animations Spriter de la Valkyrie
// (os de chaque pièce, image par image) et y greffe les pièces du personnage Craftpix choisi pour chaque emplacement :
// casque -> tête + visage, lame -> arme, ceinture -> corps, gantelets -> bras et mains, bottes -> jambes, bouclier.
// Données et images dans Resources/ForgeSprites (hors dépôt, licence Craftpix) ; sans elles, PaperHero prend le relais.
public class CraftpixHero
{
    [System.Serializable] class Frame { public float[] v; }
    [System.Serializable] class Anim { public string name; public float fps; public bool loop; public Frame[] frames; }
    [System.Serializable] class Rig { public string[] parts; public Anim[] anims; }
    [System.Serializable] class Char { public string name; public float[] v; }
    [System.Serializable] class Parts { public float scale, heightPx; public Char[] chars; }

    const int Stride = 8;   // présent, x, y, angle, échelle x, échelle y, (z | pivot x), (alpha | pivot y)
    enum P { Body, Head, Face, RArm, RHand, LArm, LHand, LLeg, RLeg, Weapon, Shield }
    const int Count = 11;

    static Rig rig;
    static Parts parts;
    static Dictionary<string, Anim> anims;
    static Dictionary<string, float[]> charParts;
    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public static bool Available
    {
        get
        {
            if (rig != null) return true;
            var r = Resources.Load<TextAsset>("ForgeSprites/Rig/rig_u");
            var p = Resources.Load<TextAsset>("ForgeSprites/Rig/parts_u");
            if (r == null || p == null) return false;
            rig = JsonUtility.FromJson<Rig>(r.text);
            parts = JsonUtility.FromJson<Parts>(p.text);
            anims = new Dictionary<string, Anim>();
            foreach (var a in rig.anims) anims[a.name] = a;
            charParts = new Dictionary<string, float[]>();
            foreach (var c in parts.chars) charParts[c.name] = c.v;
            return true;
        }
    }

    // ---------- Correspondance équipement -> personnage Craftpix ----------
    const string Base = "Valkyrie_1";
    // Personnages au thème de chaque cercle (corps, bras, jambes).
    static readonly string[][] Theme =
    {
        new[] { "Zombie_Villager_1", "Zombie_Villager_2", "Zombie_Villager_3", "Skeleton_Warrior_1", "Golem_01", "Satyr_03" },
        new[] { "Bloody_Alchemist_2", "Seer_3", "Valkyrie_3" },
        new[] { "Goblin", "Ogre", "Orc", "Golem_2", "Golem_02", "Minotaur_01", "Satyr_02" },
        new[] { "Seer_1", "Valkyrie_1", "Fallen_Angels_3", "Satyr_01" },
        new[] { "Golem_3", "Minotaur_1", "Bloody_Alchemist_1", "Golem_03", "Minotaur_03" },
        new[] { "Dark_Oracle_1", "Dark_Oracle_2", "Dark_Oracle_3", "Necromancer_of_the_Shadow_2" },
        new[] { "Minotaur_2", "Minotaur_3", "Skeleton_Warrior_3", "Skeleton_Crusader_1", "Minotaur_02" },
        new[] { "Necromancer_of_the_Shadow_3", "Reaper_Man_3", "Seer_2" },
        new[] { "Golem_1", "Necromancer_of_the_Shadow_1", "Valkyrie_2" },
        new[] { "Fallen_Angels_1", "Fallen_Angels_2", "Reaper_Man_1", "Reaper_Man_2" },
    };
    static readonly Dictionary<string, string[]> Heads = new Dictionary<string, string[]>
    {
        { "Casque", new[] { "Skeleton_Warrior_3", "Skeleton_Warrior_2", "Skeleton_Crusader_1" } },
        { "Heaume", new[] { "Valkyrie_1", "Skeleton_Crusader_2", "Skeleton_Warrior_2" } },
        { "Barbute", new[] { "Skeleton_Warrior_2", "Skeleton_Crusader_3" } },
        { "Heaume cornu", new[] { "Minotaur_1", "Minotaur_2", "Minotaur_3", "Forest_Ranger_3", "Orc", "Minotaur_01", "Minotaur_02", "Minotaur_03", "Satyr_01", "Satyr_03" } },
        { "Capuche", new[] { "Dark_Oracle_1", "Dark_Oracle_2", "Dark_Oracle_3", "Necromancer_of_the_Shadow_1", "Necromancer_of_the_Shadow_2", "Necromancer_of_the_Shadow_3", "Reaper_Man_1", "Reaper_Man_2", "Reaper_Man_3", "Seer_2", "Forest_Ranger_1", "Bloody_Alchemist_3", "Wraith_02", "Wraith_03" } },
        { "Couronne", new[] { "Fallen_Angels_3", "Forest_Ranger_2", "Fallen_Angels_1" } },
        { "Crâne", new[] { "Skeleton_Warrior_1", "Golem_1", "Golem_3", "Wraith_01" } },
        { "Mitre", new[] { "Seer_1" } },
        { "Bonnet de bouffon", new[] { "Bloody_Alchemist_2", "Zombie_Villager_2", "Satyr_02" } },
        { "Heaume ailé", new[] { "Valkyrie_2", "Valkyrie_3", "Fallen_Angels_2" } },
        { "Heaume à plumet", new[] { "Valkyrie_1", "Valkyrie_3" } },
        { "Masque", new[] { "Seer_3", "Bloody_Alchemist_1", "Zombie_Villager_2", "Golem_01", "Golem_02", "Golem_03" } },
    };
    static readonly Dictionary<string, string[]> Weapons = new Dictionary<string, string[]>
    {
        { "Épée", new[] { "Valkyrie_1", "Valkyrie_2", "Fallen_Angels_1", "Fallen_Angels_3", "Skeleton_Crusader_1" } },
        { "Espadon", new[] { "Skeleton_Warrior_1", "Golem_3", "Skeleton_Crusader_2" } },
        { "Dague", new[] { "Bloody_Alchemist_1", "Valkyrie_3" } },
        { "Rapière", new[] { "Fallen_Angels_3", "Skeleton_Warrior_2" } },
        { "Lame courbe", new[] { "Minotaur_2", "Golem_1" } },
        { "Hache", new[] { "Goblin", "Ogre", "Minotaur_3", "Golem_02" } },
        { "Couperet", new[] { "Minotaur_2", "Bloody_Alchemist_1", "Golem_01" } },
        { "Gourdin", new[] { "Golem_2", "Zombie_Villager_1", "Golem_03", "Minotaur_01" } },
        { "Masse", new[] { "Orc", "Golem_2", "Minotaur_02", "Satyr_03" } },
        { "Marteau de guerre", new[] { "Skeleton_Warrior_3", "Minotaur_1", "Minotaur_03" } },
        { "Fléau", new[] { "Orc" } },
        { "Lance", new[] { "Forest_Ranger_3", "Necromancer_of_the_Shadow_1", "Satyr_02" } },
        { "Hallebarde", new[] { "Minotaur_3" } },
        { "Faux", new[] { "Reaper_Man_1", "Reaper_Man_2", "Reaper_Man_3" } },
        { "Trident", new[] { "Necromancer_of_the_Shadow_1", "Forest_Ranger_3" } },
        { "Arc", new[] { "Zombie_Villager_3", "Zombie_Villager_2" } },
        { "Bâton", new[] { "Seer_1", "Seer_2", "Seer_3", "Dark_Oracle_1", "Dark_Oracle_2", "Dark_Oracle_3", "Fallen_Angels_2", "Necromancer_of_the_Shadow_2", "Necromancer_of_the_Shadow_3", "Bloody_Alchemist_2", "Bloody_Alchemist_3", "Satyr_01" } },
        { "Sceptre", new[] { "Seer_1", "Dark_Oracle_3", "Bloody_Alchemist_3" } },
    };
    static readonly string[] Shields = { "Valkyrie_1", "Skeleton_Crusader_1", "Valkyrie_2", "Skeleton_Crusader_2", "Valkyrie_3", "Skeleton_Crusader_3" };

    static string From(string[] list, Item it) => list[Mathf.Abs(it.circle * 7 + it.model * 3) % list.Length];
    static string Family(Item it, int slot) => GameState.ModelName(slot, it.circle, it.model);

    // ---------- Instance ----------
    readonly Transform root;
    readonly SpriteRenderer[] sr = new SpriteRenderer[Count];
    readonly string[] who = new string[Count];
    string key = "";
    float ppu;

    public CraftpixHero(Transform body)
    {
        root = new GameObject("Héros Craftpix").transform;
        root.SetParent(body, false);
        root.gameObject.AddComponent<SortingGroup>().sortingOrder = 1;
        for (int i = 0; i < Count; i++)
        {
            sr[i] = new GameObject(((P)i).ToString()).AddComponent<SpriteRenderer>();
            sr[i].transform.SetParent(root, false);
            who[i] = Base;
        }
        ppu = parts.heightPx * parts.scale;   // pixels d'image réduite par unité (1 unité = taille du héros)
        Apply();
    }

    Sprite Get(string ch, int part)
    {
        string k = ch + "/" + part;
        if (sprites.TryGetValue(k, out var s)) return s;
        s = null;
        if (charParts.TryGetValue(ch, out var v) && v[part * Stride] > 0.5f)
        {
            var tex = Resources.Load<Texture2D>("ForgeSprites/Parts/" + ch + "/" + (P)part);
            if (tex != null)
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(v[part * Stride + 6], v[part * Stride + 7]), ppu, 0, SpriteMeshType.FullRect);
        }
        sprites[k] = s;
        return s;
    }

    public void SetGear(Item weapon, Item helmet, Item shield, Item gloves, Item boots, Item belt)
    {
        string head = helmet != null && helmet.valid && Heads.TryGetValue(Family(helmet, 1), out var hl) ? From(hl, helmet) : Base;
        string wpn = weapon != null && weapon.valid && Weapons.TryGetValue(Family(weapon, 0), out var wl) ? From(wl, weapon) : Base;
        string body = belt != null && belt.valid ? From(Theme[Mathf.Clamp(belt.circle, 0, 9)], belt) : Base;
        string arms = gloves != null && gloves.valid ? From(Theme[Mathf.Clamp(gloves.circle, 0, 9)], gloves) : Base;
        string legs = boots != null && boots.valid ? From(Theme[Mathf.Clamp(boots.circle, 0, 9)], boots) : Base;
        string shd = shield != null && shield.valid ? Shields[Mathf.Abs(shield.circle * 3 + shield.model) % Shields.Length] : null;
        string k = head + wpn + body + arms + legs + shd;
        if (k == key) return;
        key = k;
        who[(int)P.Head] = who[(int)P.Face] = head;
        who[(int)P.Weapon] = wpn;
        who[(int)P.Body] = body;
        who[(int)P.RArm] = who[(int)P.RHand] = who[(int)P.LArm] = who[(int)P.LHand] = arms;
        who[(int)P.LLeg] = who[(int)P.RLeg] = legs;
        who[(int)P.Shield] = shd;
        Apply();
    }

    void Apply()
    {
        for (int i = 0; i < Count; i++) sr[i].sprite = who[i] != null ? Get(who[i], i) : null;
    }

    public float Length(string anim) => anims.TryGetValue(anim, out var a) ? a.frames.Length / a.fps : 0.5f;

    public void Animate(string anim, float t)
    {
        if (!anims.TryGetValue(anim, out var a) && !anims.TryGetValue("idle", out a)) return;
        int n = a.frames.Length;
        int f = (int)(t * a.fps);
        f = a.loop ? f % n : Mathf.Min(f, n - 1);
        var fv = a.frames[f].v;
        for (int i = 0; i < Count; i++)
        {
            int o = i * Stride;
            var s = sr[i];
            string ch = who[i];
            if (fv[o] < 0.5f || ch == null || s.sprite == null) { s.enabled = false; continue; }
            var lv = charParts[ch];
            int lo = i * Stride;
            // Os (animation de référence) puis position de la pièce sur l'os (dans la pose de son propre personnage).
            float bx = fv[o + 1], by = fv[o + 2], ba = fv[o + 3], bsx = fv[o + 4], bsy = fv[o + 5];
            float lx = lv[lo + 1] * bsx, ly = lv[lo + 2] * bsy;
            float r = ba * Mathf.Deg2Rad, c = Mathf.Cos(r), sn = Mathf.Sin(r);
            s.enabled = true;
            s.transform.localPosition = new Vector3(bx + lx * c - ly * sn, by + lx * sn + ly * c, 0f);
            s.transform.localRotation = Quaternion.Euler(0f, 0f, ba + lv[lo + 3]);
            s.transform.localScale = new Vector3(bsx * lv[lo + 4], bsy * lv[lo + 5], 1f);
            s.sortingOrder = (int)fv[o + 6];
        }
    }

    public void SetColor(Color c)
    {
        foreach (var s in sr) s.color = c;
    }
}
