using UnityEngine;
using UnityEngine.UI;

// Fiche du héros façon Forge Master : le héros animé en grand (rendu en direct dans une texture), ses 8 pièces autour,
// la puissance, puis toutes les statistiques en cartes à icônes.
public partial class ForgeUI
{
    Camera heroCam;
    SpriteChar heroModel;
    RenderTexture heroRT;
    RawImage heroView;
    Text heroPower, heroHint;
    readonly Image[] hvSlotBg = new Image[GameState.SlotCount];
    readonly RawImage[] hvSlotIcon = new RawImage[GameState.SlotCount];
    readonly Text[] hvSlotLevel = new Text[GameState.SlotCount];
    Text[] hvStatValue;
    RectTransform hvRays;

    static readonly Vector3 HeroStudio = new Vector3(40f, -520f, 0f);

    // Statistiques affichées : icône (sprite ou glyphe de stat secondaire), nom.
    static readonly string[] HvStatNames = { "Attaque", "Points de vie", "Attaques / s", "Critique", "Dégâts critiques", "Double frappe", "Vitesse d'attaque",
                                             "Dégâts", "Dégâts en mêlée", "Santé", "Vol de vie", "Blocage", "Régénération" };
    static readonly int[] HvStatSub = { -1, -2, -3, 0, 1, 5, 8, 6, 7, 9, 4, 2, 3 };   // -1 épée, -2 cœur, -3 horloge, sinon stat secondaire

    void BuildHeroView(Transform P)
    {
        // Vitrine : cadre, rayons et halo derrière le héros
        var frame = Box(P, "Vitrine", new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -1110), new Vector2(-16, -100), Ember);
        UISkin.Sliced(frame, "frame", 2.4f);
        var inner = Box(frame.transform, "Fond", Vector2.zero, Vector2.one, new Vector2(7, 7), new Vector2(-7, -7), SkinTint(new Color(0.16f, 0.07f, 0.06f)));
        UISkin.Sliced(inner, "tile", 1.8f); inner.raycastTarget = false;
        var rays = Box(inner.transform, "Rayons", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(-420, -420), new Vector2(420, 420), new Color(1f, 0.7f, 0.3f, 0.22f));
        rays.sprite = UISkin.Get("rays"); rays.raycastTarget = false; hvRays = rays.rectTransform;
        var glow = Box(inner.transform, "Halo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-330, -300), new Vector2(330, 360), new Color(1f, 0.45f, 0.15f, 0.55f));
        glow.sprite = UISkin.Get("glow"); glow.raycastTarget = false;
        // Socle
        var plinth = Box(inner.transform, "Socle", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 100), new Vector2(210, 156), new Color(0, 0, 0, 0.45f));
        plinth.sprite = Circle(); plinth.raycastTarget = false;

        var hv = MakeRect("Héros", inner.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-300, 112), new Vector2(300, 1062));
        heroView = hv.gameObject.AddComponent<RawImage>();
        heroView.raycastTarget = false;

        // Puissance sous le héros
        var pw = Box(inner.transform, "Puissance", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 22), new Vector2(190, 94), Color.white);
        UISkin.Sliced(pw, "pill"); pw.raycastTarget = false;
        UISkin.Icon(pw.transform, "i_power", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-14, -42), new Vector2(70, 42));
        heroPower = Label(pw.transform, "", 40, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.3f), Vector2.zero, Vector2.one, new Vector2(60, 0), new Vector2(-16, 0));
        heroHint = Label(inner.transform, "", 22, TextAnchor.UpperCenter, TextDim, new Vector2(0.25f, 1), new Vector2(0.75f, 1), new Vector2(0, -50), new Vector2(0, -14));

        // 8 pièces : 4 à gauche, 4 à droite
        for (int i = 0; i < GameState.SlotCount; i++)
        {
            bool left = i < 4;
            int row = i % 4;
            float top = -30 - row * 240;
            var aMin = new Vector2(left ? 0 : 1, 1);
            var bg = Box(inner.transform, "Pièce " + i, aMin, aMin, new Vector2(left ? 18 : -186, top - 168), new Vector2(left ? 186 : -18, top), Panel);
            UISkin.Sliced(bg, "tile", 1.4f);
            hvSlotBg[i] = bg;
            var ic = MakeRect("Icône", bg.transform, Vector2.zero, Vector2.one, new Vector2(16, 14), new Vector2(-16, -26));
            hvSlotIcon[i] = ic.gameObject.AddComponent<RawImage>(); hvSlotIcon[i].raycastTarget = false;
            hvSlotLevel[i] = Label(bg.transform, "", 22, TextAnchor.UpperLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(10, 4), new Vector2(-4, -6));
        }

        // Attaque et vie en grand ; le détail s'ouvre avec « Statistiques »
        int n = 2;
        hvStatValue = new Text[n];
        for (int i = 0; i < n; i++)
        {
            int col = i % 2, row = i / 2;
            float top = -1130 - row * 84;
            var cell = Box(P, "Stat " + i, new Vector2(col * 0.5f, 1), new Vector2(col * 0.5f + 0.5f, 1), new Vector2(col == 0 ? 16 : 6, top - 100), new Vector2(col == 0 ? -6 : -16, top), Color.white);
            UISkin.Sliced(cell, "slot", 1.2f); cell.raycastTarget = false;
            int sub = HvStatSub[i];
            if (sub < 0) UISkin.Icon(cell.transform, sub == -1 ? "i_sword" : sub == -2 ? "i_heart" : "i_clock", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, -28), new Vector2(66, 28));
            else Label(cell.transform, "<color=" + Hex(SubColors[sub]) + ">" + SubGlyphs[sub] + "</color>", 34, TextAnchor.MiddleCenter, Color.white, new Vector2(0, 0), new Vector2(0, 1), new Vector2(10, 0), new Vector2(66, 0));
            hvStatValue[i] = Label(cell.transform, "", 40, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f), Vector2.zero, Vector2.one, new Vector2(66, 0), new Vector2(-10, 0));
        }
        Text st;
        MakeButton(P, "Statistiques", new Vector2(0.25f, 1), new Vector2(0.75f, 1), new Vector2(0, -1350), new Vector2(0, -1256),
            new Color(0.3f, 0.18f, 0.12f), "Statistiques", 30, out st, ShowAllStats);
    }

    // Studio caché : le héros Craftpix (ou en pièces détachées) filmé par une caméra orthographique dans une texture.
    void EnsureHeroStudio()
    {
        if (heroCam != null) return;
        var root = new GameObject("Studio du héros").transform;
        root.position = HeroStudio;
        heroModel = SpriteChar.Spawn(SpriteChar.CraftpixName, 1.7f, root, false, 0f) ?? SpriteChar.Spawn(SpriteChar.PaperName, 1.8f, root, false, 0f);
        heroRT = new RenderTexture(460, 730, 16, RenderTextureFormat.ARGB32) { name = "Héros (vitrine)" };
        var cg = new GameObject("Caméra vitrine");
        cg.transform.SetParent(root, false);
        cg.transform.localPosition = new Vector3(0f, 1.02f, -10f);
        heroCam = cg.AddComponent<Camera>();
        heroCam.orthographic = true;
        heroCam.orthographicSize = 1.22f;
        heroCam.nearClipPlane = 0.1f; heroCam.farClipPlane = 20f;
        heroCam.clearFlags = CameraClearFlags.SolidColor;
        heroCam.backgroundColor = new Color(0, 0, 0, 0);
        heroCam.targetTexture = heroRT;
        heroCam.enabled = false;
        heroView.texture = heroRT;
        heroView.color = heroModel != null ? Color.white : new Color(0, 0, 0, 0);
    }

    void UpdateHeroView()
    {
        bool on = statsPanel != null && statsPanel.activeSelf;
        if (heroCam != null) heroCam.enabled = on;
        if (!on) return;
        if (hvRays != null) hvRays.localRotation = Quaternion.Euler(0, 0, Time.time * 6f);
    }

    void RefreshHeroView()
    {
        EnsureHeroStudio();
        var d = GameState.Data;
        var eq = d.equipped;
        if (heroModel != null)
        {
            if (heroModel.Craft != null) heroModel.Craft.SetGear(eq[0], eq[1], eq[2], eq[3], eq[4], eq[5]);
            else if (heroModel.Paper != null) heroModel.Paper.SetGear(eq[0], eq[1], eq[2], eq[3], eq[4], eq[5]);
        }
        heroPower.text = GameState.Fmt(GameState.Power());
        var skin = HeroSkins.Find(HeroSkins.Shown);
        heroHint.text = skin != null ? "Skin : <color=#FFC07A>" + skin.name + "</color>" : "";
        for (int i = 0; i < GameState.SlotCount; i++)
        {
            var it = eq[i];
            if (it.valid)
            {
                hvSlotBg[i].color = Color.Lerp(GameState.CircleColors[it.circle], Color.white, 0.12f);
                hvSlotIcon[i].texture = ItemIcons.Get(it); hvSlotIcon[i].color = Color.white;
                hvSlotLevel[i].text = (it.stars > 0 ? "<color=#FFD24A>" + GameState.StarText(it.stars) + "</color> " : "") + "Niv. " + it.level;
            }
            else
            {
                hvSlotBg[i].color = new Color(0.32f, 0.22f, 0.2f);
                hvSlotIcon[i].texture = ItemIcons.Get(i, 0); hvSlotIcon[i].color = new Color(0, 0, 0, 0.55f);
                hvSlotLevel[i].text = "";
            }
        }
        var v = HvValues();
        for (int i = 0; i < hvStatValue.Length; i++) hvStatValue[i].text = v[i];
    }

    static string[] HvValues()
    {
        return new[]
        {
            GameState.Fmt(GameState.TotalAtk()), GameState.Fmt(GameState.TotalHp()), (1f / GameState.AttackInterval).ToString("0.00"),
            GameState.FmtPct(GameState.CritChance), "x" + GameState.CritMult.ToString("0.##"), GameState.FmtPct(GameState.DoubleChance),
            "+" + GameState.FmtPct(GameState.AttackSpeedBonus), "+" + GameState.FmtPct(GameState.SubTotal(GameState.SubDamage)),
            "+" + GameState.FmtPct(GameState.SubTotal(GameState.SubMelee)), "+" + GameState.FmtPct(GameState.SubTotal(GameState.SubHealth)),
            GameState.FmtPct(GameState.LifeSteal), GameState.FmtPct(GameState.BlockChance), GameState.FmtPct(GameState.RegenPerSecond) + "/s",
        };
    }

    void ShowAllStats()
    {
        var sb = new System.Text.StringBuilder();
        var v = HvValues();
        for (int i = 2; i < HvStatNames.Length; i++)
        {
            int sub = HvStatSub[i];
            string g = sub >= 0 ? "<color=" + Hex(SubColors[sub]) + ">" + SubGlyphs[sub] + "</color> " : "";
            sb.Append(g + HvStatNames[i] + "   <color=#FFC07A>" + v[i] + "</color>\n");
        }
        ShowInfo("Statistiques", sb.ToString());
    }
}
