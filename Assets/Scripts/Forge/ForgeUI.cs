using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Interface 2D construite par code, par-dessus la scène 3D.
public partial class ForgeUI : MonoBehaviour
{
    ForgeWorld world;
    Button autoBtn;
    Text autoText;
    float autoTimer;
    BattleWorld battle;
    RectTransform statsRect, equipRect;
    Vector2Int lastScreen;
    int layoutFrames = 3;
    Font font;
    RectTransform safeRoot;

    Text goldText, gemText, hammerText, statsText, forgeLevelText, oddsText, upgradeText, speedUpText, forgeBtnText;
    Button forgeBtn, upgradeBtn, speedUpBtn;
    Image forgeBtnImg;
    readonly Image[] slotBg = new Image[GameState.SlotCount];
    readonly Text[] slotText = new Text[GameState.SlotCount];
    readonly Text[] slotLevel = new Text[GameState.SlotCount];
    readonly RawImage[] slotIcon = new RawImage[GameState.SlotCount];

    GameObject popup;
    Text popName, popInfo, popStat, popSubs, popCompare, popSellText;
    GameObject statsPanel;
    Image popFrame;
    Text toast;
    float toastTime;
    CanvasGroup toastGroup;
    float refreshTimer;

    static readonly Color Panel = new Color(0.16f, 0.09f, 0.08f, 0.94f);
    static readonly Color Ember = new Color(0.95f, 0.38f, 0.1f);
    static readonly Color TextMain = new Color(0.96f, 0.9f, 0.84f);
    static readonly Color TextDim = new Color(0.88f, 0.8f, 0.74f);

    public static ForgeUI Build(ForgeWorld world, BattleWorld battle)
    {
        var go = new GameObject("ForgeUI");
        var ui = go.AddComponent<ForgeUI>();
        ui.world = world;
        ui.battle = battle;
        ui.Create();
        return ui;
    }

    // ---------- Aides de construction ----------
    static RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        return rt;
    }

    static Image Box(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color c)
    {
        var img = MakeRect(name, parent, aMin, aMax, offMin, offMax).gameObject.AddComponent<Image>();
        img.color = c;
        return img;
    }

    Text Label(Transform parent, string txt, int size, TextAnchor anchor, Color c, Vector2? aMin = null, Vector2? aMax = null, Vector2? offMin = null, Vector2? offMax = null)
    {
        var rt = MakeRect("Texte", parent, aMin ?? Vector2.zero, aMax ?? Vector2.one, offMin ?? Vector2.zero, offMax ?? Vector2.zero);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = txt;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = c;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var sh = rt.gameObject.AddComponent<Outline>();
        sh.effectColor = new Color(0.06f, 0.02f, 0.02f, 0.95f);
        sh.effectDistance = new Vector2(2.2f, -2.2f);
        return t;
    }

    Button MakeButton(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color c, string txt, int size, out Text label, UnityEngine.Events.UnityAction onClick)
    {
        var img = Box(parent, name, aMin, aMax, offMin, offMax, UISkin.Vivid(c));
        bool close = name == "Fermer" && UISkin.Get("i_close") != null;
        if (close) { img.sprite = UISkin.Get("i_close"); img.preserveAspect = true; img.color = Color.white; txt = ""; }
        else UISkin.Sliced(img, "btn", 1.3f);
        var b = img.gameObject.AddComponent<Button>();
        var colors = b.colors;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        b.colors = colors;
        b.onClick.AddListener(Sfx.Click);
        b.onClick.AddListener(onClick);
        // Liseré coloré optionnel (transparent par défaut ; certains écrans le colorent pour marquer un état)
        var outline = img.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0);
        outline.effectDistance = new Vector2(3, -3);
        label = Label(img.transform, txt, size, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(6, 8), new Vector2(-6, -2));
        return b;
    }

    // ---------- Construction de l'écran (disposition inspirée de Forge Master) ----------
    RectTransform anvilRect;
    Text anvilText;

    void Create()
    {
        font = UISkin.Font;

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        safeRoot = MakeRect("Zone sûre", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        ApplySafeArea();
        Transform R = safeRoot;

        // ----- Barre du haut : puissance à gauche, or et gemmes à droite -----
        var top = Box(R, "Haut", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -130), Vector2.zero, new Color(0.05f, 0.02f, 0.02f, 0.92f));
        if (UISkin.Get("topbar") != null) { UISkin.Sliced(top, "topbar"); top.color = Color.white; }
        statsRect = top.rectTransform;
        var avatar = Box(top.transform, "Portrait", new Vector2(0, 0), new Vector2(0, 1), new Vector2(16, 12), new Vector2(120, -12), Ember);
        UISkin.Sliced(avatar, "frame", 2.2f);
        var avIn = Box(avatar.transform, "Fond", Vector2.zero, Vector2.one, new Vector2(7, 7), new Vector2(-7, -7), new Color(0.35f, 0.08f, 0.06f));
        UISkin.Sliced(avIn, "tile", 2.4f); avIn.raycastTarget = false;
        UISkin.Icon(avatar.transform, "i_hero", Vector2.zero, Vector2.one, new Vector2(12, 12), new Vector2(-12, -12));
        UISkin.Icon(top.transform, "i_power", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(126, 2), new Vector2(176, 52));
        statsText = Label(top.transform, "", 30, TextAnchor.MiddleLeft, TextMain, new Vector2(0, 0), new Vector2(0.52f, 1), new Vector2(132, 6), new Vector2(0, -6));
        goldText = CurrencyPill(top.transform, "i_gold", new Vector2(0.52f, 0.5f), new Vector2(0.76f, 0.5f), new Vector2(4, -32), new Vector2(-6, 32), new Color(1f, 0.86f, 0.4f));
        gemText = CurrencyPill(top.transform, "i_gem", new Vector2(0.76f, 0.5f), new Vector2(1f, 0.5f), new Vector2(6, -32), new Vector2(-16, 32), new Color(1f, 0.7f, 0.9f));

        BuildNav(R);

        // ----- Rangée de la forge : enclume tactile au centre, niveau de forge à droite -----
        var forgeRow = MakeRect("Rangée forge", R, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 165), new Vector2(0, 520));
        forgeRowGroup = forgeRow.gameObject.AddComponent<CanvasGroup>();
        anvilRect = MakeRect("Enclume", forgeRow, new Vector2(0.27f, 0f), new Vector2(0.73f, 1f), Vector2.zero, Vector2.zero);
        var anvilImg = anvilRect.gameObject.AddComponent<Image>();
        anvilImg.color = new Color(0, 0, 0, 0); // transparent : la 3D est derrière
        forgeBtn = anvilRect.gameObject.AddComponent<Button>();
        forgeBtn.transition = Selectable.Transition.None;
        forgeBtn.onClick.AddListener(OnForge);
        forgeBtnImg = anvilImg;
        var hammerPlate = Box(anvilRect, "Marteaux", new Vector2(0.2f, 0f), new Vector2(0.8f, 0f), new Vector2(0, 8), new Vector2(0, 72), new Color(1f, 1f, 1f, 0.95f));
        UISkin.Sliced(hammerPlate, "pill");
        hammerPlate.raycastTarget = false;
        UISkin.Icon(hammerPlate.transform, "i_hammer", new Vector2(0, 0), new Vector2(0, 1), new Vector2(-14, -6), new Vector2(66, 10));
        hammerText = Label(hammerPlate.transform, "", 34, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(52, 0), new Vector2(-8, 0));
        anvilText = Label(anvilRect, "Touche l'enclume !", 26, TextAnchor.UpperCenter, new Color(1f, 0.75f, 0.5f), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -6));
        forgeBtnText = anvilText;
        BuildQueueBadge(anvilRect);

        // Niveau de forge (droite)
        upgradeBtn = MakeButton(forgeRow, "Forge niveau", new Vector2(0.74f, 0.42f), new Vector2(1f, 0.82f), new Vector2(6, 0), new Vector2(-18, 0),
            new Color(0.62f, 0.22f, 0.06f), "", 30, out upgradeText, OnUpgrade);
        speedUpBtn = MakeButton(forgeRow, "Accélérer", new Vector2(0.74f, 0.12f), new Vector2(1f, 0.38f), new Vector2(6, 0), new Vector2(-18, 0),
            new Color(0.12f, 0.4f, 0.55f), "", 26, out speedUpText, OnSpeedUp);
        forgeLevelText = Label(forgeRow, "", 34, TextAnchor.LowerCenter, Ember, new Vector2(0.74f, 0.82f), new Vector2(1f, 1f), new Vector2(6, 0), new Vector2(-18, 0));

        // Gauche : chances (le détail s'ouvre au toucher) et forge automatique
        MakeButton(forgeRow, "Chances", new Vector2(0f, 0.5f), new Vector2(0.26f, 0.82f), new Vector2(18, 0), new Vector2(-6, 0),
            new Color(0.2f, 0.1f, 0.08f), "", 24, out oddsText, ShowOdds);
        autoBtn = MakeButton(forgeRow, "Auto", new Vector2(0f, 0.12f), new Vector2(0.26f, 0.44f), new Vector2(18, 0), new Vector2(-6, 0),
            new Color(0.25f, 0.25f, 0.3f), "Auto", 28, out autoText, OnAutoToggle);

        // ----- Équipement : 2 rangées de 4, tuiles compactes -----
        // Bandeau de pierre derrière les tuiles (entre le chemin et la forge)
        var eqBg = Box(R, "Fond équipement", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 518), new Vector2(0, 862), new Color(0.12f, 0.06f, 0.05f, 1f));
        if (UISkin.Get("navbar") != null) { UISkin.Sliced(eqBg, "navbar"); eqBg.color = new Color(1f, 0.92f, 0.88f); }
        eqBg.raycastTarget = false;
        var eq = MakeRect("Equipement", R, new Vector2(0, 0), new Vector2(1, 0), new Vector2(18, 530), new Vector2(-18, 850));
        equipRect = eq;
        equipGroup = eq.gameObject.AddComponent<CanvasGroup>();
        for (int i = 0; i < GameState.SlotCount; i++)
        {
            int row = i / 4, col = i % 4;
            var aMin = new Vector2(col / 4f, row == 0 ? 0.5f : 0f);
            var aMax = new Vector2((col + 1) / 4f, row == 0 ? 1f : 0.5f);
            slotBg[i] = Box(eq, "Emplacement " + i, aMin, aMax, new Vector2(6, 6), new Vector2(-6, -6), Panel);
            UISkin.Sliced(slotBg[i], "tile", 1.2f);
            // Icône 3D de la pièce au centre, niveau en haut, statistique en bas.
            var iconRt = MakeRect("Icône", slotBg[i].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-62, -54), new Vector2(62, 70));
            slotIcon[i] = iconRt.gameObject.AddComponent<RawImage>();
            slotIcon[i].raycastTarget = false;
            slotLevel[i] = Label(slotBg[i].transform, "", 24, TextAnchor.UpperLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(10, 6), new Vector2(-6, -6));
            slotText[i] = Label(slotBg[i].transform, "", 25, TextAnchor.LowerCenter, TextMain, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6));
        }

        BuildPassButton(R);
        BuildPopup(R);
        BuildStatsPanel(R);
        BuildCompanionPanel(R);
        BuildDungeonPanel(R);
        BuildTechPanel(R);
        BuildMountPanel(R);
        BuildSkillPanel(R);
        BuildHeroSkinsPanel(R);
        BuildStonesPanel(R);
        BuildShopPanel(R);
        BuildMissionPanel(R);
        BuildPassPanel(R);
        BuildSubTabs(R);
        BuildDetailPopup(R);

        // Messages : bandeau sombre sous le titre de l'étape (au-dessus de tout, texte sur plusieurs lignes si besoin).
        var tb = MakeRect("Bandeau message", R, new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -500), new Vector2(-40, -330));
        var tbImg = tb.gameObject.AddComponent<Image>();
        tbImg.sprite = Round(); tbImg.type = Image.Type.Sliced; tbImg.color = new Color(0.05f, 0.02f, 0.02f, 0.82f); tbImg.raycastTarget = false;
        if (UISkin.Get("card") != null) { UISkin.Sliced(tbImg, "card"); tbImg.color = new Color(1f, 1f, 1f, 0.95f); }
        toastGroup = tb.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f; toastGroup.blocksRaycasts = false;
        toast = Label(tb, "", 34, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f), Vector2.zero, Vector2.one, new Vector2(20, 8), new Vector2(-20, -8));
        toast.horizontalOverflow = HorizontalWrapMode.Wrap;
        toast.verticalOverflow = VerticalWrapMode.Truncate;
        toast.resizeTextForBestFit = true; toast.resizeTextMinSize = 20; toast.resizeTextMaxSize = 34;

        GameState.Changed += Refresh;
        GameState.ForgeLeveledUp += OnLevelUp;
        Refresh();

        if (GameState.Data.pending.valid) ShowPopup(GameState.Data.pending);
        if (!string.IsNullOrEmpty(GameState.OfflineMessage)) { Toast(GameState.OfflineMessage); toastTime = 5f; }
    }

    void OnDestroy()
    {
        GameState.PetMessage -= Toast;
        GameState.TechDone -= Toast;
        GameState.MountMessage -= Toast;
        GameState.SkillMessage -= Toast;
        GameState.ShopMessage -= Toast;
        GameState.MissionMessage -= Toast;
        if (battle != null) battle.DungeonEnded -= Toast;
        GameState.Changed -= Refresh;
        GameState.ForgeLeveledUp -= OnLevelUp;
    }

    void ApplySafeArea()
    {
        var sa = Screen.safeArea;
        if (Screen.width <= 0 || Screen.height <= 0) return;
        safeRoot.anchorMin = new Vector2(sa.xMin / Screen.width, sa.yMin / Screen.height);
        safeRoot.anchorMax = new Vector2(sa.xMax / Screen.width, sa.yMax / Screen.height);
        safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
    }

    // ---------- Fiche du héros (toutes les statistiques) ----------
    void ShowOdds()
    {
        var odds = GameState.CurrentOdds();
        var sb = new System.Text.StringBuilder("<size=26><color=#A89C94>Forge niv. " + GameState.Data.forgeLevel + "</color></size>\n\n");
        for (int i = 0; i < odds.Length; i++)
            if (odds[i] > 0f) sb.Append("<color=" + Hex(GameState.CircleColors[i]) + ">" + GameState.CircleNames[i] + "</color>   " + odds[i].ToString("0.##") + " %\n");
        ShowInfo("Chances de la forge", sb.ToString());
    }

    void BuildStatsPanel(Transform R)
    {
        // Panneau plein comme les autres (sous-onglet « Héros ») : statistiques détaillées.
        statsPanel = MakeRect("Fiche du héros", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(statsPanel);
        Transform P = statsPanel.transform;
        Label(P, "HÉROS", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => statsPanel.SetActive(false));
        BuildHeroView(P);
        MakeButton(P, "Son", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260, 24), new Vector2(-24, 84),
            new Color(0.25f, 0.12f, 0.1f), "", 26, out soundText, () => { GameState.Data.muted = !GameState.Data.muted; GameState.Save(); RefreshSoundButton(); });
        RefreshSoundButton();
        MakeButton(P, "Apparence", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150, 24), new Vector2(150, 84),
            new Color(0.3f, 0.18f, 0.4f), "", 26, out skinText, () => { GameState.Data.heroSkin = 1 - GameState.Data.heroSkin; GameState.Save(); RefreshSkinButton(); battle.RespawnHero(); });
        RefreshSkinButton();
        Text resetT;
        MakeButton(P, "Reset", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24, 24), new Vector2(200, 84),
            new Color(0.2f, 0.08f, 0.08f, 0.9f), "Reset (test)", 20, out resetT, () => { GameState.ResetAll(); world.DismissItem(false); HidePopup(); statsPanel.SetActive(false); });
        statsPanel.SetActive(false);
    }

    Text soundText, skinText;
    void RefreshSkinButton() { if (skinText != null) skinText.text = "Héros : " + (GameState.Data.heroSkin == 0 ? "Chevalier" : "Valkyrie"); }
    void RefreshSoundButton() { if (soundText != null) soundText.text = GameState.Data.muted ? "Son : coupé" : "Son : activé"; }

    void CloseAllPanels()
    {
        foreach (var p in new[] { statsPanel, compPanel, dungPanel, techPanel, mountPanel, skillPanel, shopPanel, missionPanel, passPanel, skinsPanel, ssPanel, detail })
            if (p != null) p.SetActive(false);
    }

    void ToggleStats()
    {
        bool open = !statsPanel.activeSelf;
        CloseAllPanels();
        statsPanel.SetActive(open);
        if (statsPanel.activeSelf) RefreshStatsPanel();
    }

    void RefreshStatsPanel() => RefreshHeroView();

    // Tuile de comparaison de la fiche « nouvelle pièce » : fond teinté au cercle, icône, niveau, statistique dessous.
    class PopTile { public Image bg, halo; public RawImage icon; public Text level, stat, caption; }

    PopTile MakePopTile(Transform c, float cx, float size, string caption)
    {
        var t = new PopTile();
        var a = new Vector2(cx, 1);
        t.halo = Box(c, "Halo", a, a, new Vector2(-size * 0.85f, -150 - size * 1.2f), new Vector2(size * 0.85f, -150 + size * 0.2f), Color.white);
        t.halo.sprite = UISkin.Get("glow") ?? Circle(); t.halo.raycastTarget = false;
        t.caption = Label(c, caption, 26, TextAnchor.LowerCenter, TextDim, a, a, new Vector2(-size, -150), new Vector2(size, -112));
        t.bg = Box(c, "Tuile", a, a, new Vector2(-size * 0.5f, -158 - size), new Vector2(size * 0.5f, -158), Color.white);
        UISkin.Sliced(t.bg, "tile", 1.3f); t.bg.raycastTarget = false;
        t.icon = MakeRect("Icône", t.bg.transform, Vector2.zero, Vector2.one, new Vector2(size * 0.1f, size * 0.08f), new Vector2(-size * 0.1f, -size * 0.12f)).gameObject.AddComponent<RawImage>();
        t.icon.raycastTarget = false;
        t.level = Label(t.bg.transform, "", size > 220 ? 28 : 24, TextAnchor.UpperLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(12, 6), new Vector2(-6, -8));
        t.stat = Label(c, "", size > 220 ? 40 : 32, TextAnchor.UpperCenter, Color.white, a, a, new Vector2(-size, -230 - size), new Vector2(size, -166 - size));
        return t;
    }

    void SetPopTile(PopTile t, Item it, Color statColor)
    {
        bool ok = it != null && it.valid;
        var c = ok ? GameState.CircleColors[it.circle] : new Color(0.4f, 0.32f, 0.3f);
        t.bg.color = Color.Lerp(c, Color.white, 0.12f);
        t.halo.color = new Color(c.r, c.g, c.b, ok ? 0.5f : 0f);
        t.icon.texture = ok ? ItemIcons.Get(it) : null;
        t.icon.color = ok ? Color.white : new Color(0, 0, 0, 0);
        t.level.text = ok ? (it.stars > 0 ? "<color=#FFD24A>" + GameState.StarText(it.stars) + "</color> " : "") + "Niv. " + it.level : "";
        t.stat.text = ok ? it.StatLabel + " " + GameState.Fmt(it.MainStat) : "<color=#776655>vide</color>";
        t.stat.color = statColor;
    }

    PopTile popCur, popNew;
    Text popArrow;

    void BuildPopup(Transform R)
    {
        popup = MakeRect("Nouvelle pièce", R, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        // Voile sous la barre du haut (elle reste claire) ; la barre d'onglets passe par-dessus.
        var shade = Box(popup.transform, "Voile", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, new Vector2(0, -130), new Color(0, 0, 0, 0.65f));
        shade.raycastTarget = true;
        popFrame = Box(popup.transform, "Cadre", new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.6f), Vector2.zero, Vector2.zero, Color.white);
        UISkin.Sliced(popFrame, "frame", 1.6f);
        var inner = Box(popFrame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(14, 14), new Vector2(-14, -14), new Color(0.07f, 0.035f, 0.035f, 0.97f));
        if (UISkin.Get("panel") != null) { UISkin.Sliced(inner, "panel"); inner.color = Color.white; }
        Transform c = inner.transform;
        popRemain = Label(c, "", 26, TextAnchor.UpperRight, TextDim, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-28, -18));
        popName = Label(c, "", 52, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -24));
        popInfo = Label(c, "", 28, TextAnchor.UpperCenter, TextDim, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -88));
        // Comparaison : pièce portée (à gauche, plus petite) -> nouvelle pièce (à droite)
        popCur = MakePopTile(c, 0.25f, 200f, "Équipé");
        popNew = MakePopTile(c, 0.7f, 270f, "Nouveau");
        popArrow = Label(c, "➜", 64, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f), new Vector2(0.47f, 1), new Vector2(0.47f, 1), new Vector2(-50, -330), new Vector2(50, -230));
        popStat = Label(c, "", 34, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -502));
        popPills = MakePills(c, new Vector2(20, -720), new Vector2(-20, -556));
        popSubs = Label(c, "", 24, TextAnchor.UpperCenter, TextDim, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -730));
        popCompare = popSubs;
        Text eqText;
        popEquipBtn = MakeButton(c, "Equiper", new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(30, 30), new Vector2(-15, 170),
            new Color(0.15f, 0.5f, 0.2f), "ÉQUIPER", 48, out eqText, OnEquip);
        MakeButton(c, "Vendre", new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(15, 30), new Vector2(-30, 170),
            new Color(0.5f, 0.14f, 0.1f), "", 40, out popSellText, OnSell);
        popup.SetActive(false);
    }

    // ---------- Actions ----------
    // Toucher l'enclume forge une pièce sur place (pas d'autre écran) ; elle rejoint la pile à examiner.
    void OnForge()
    {
        if (popup.activeSelf) return;
        if (!GameState.CanForge)
        {
            if (GameState.QueueCount > 0) ReviewNext();
            else Toast(GameState.Data.hammers <= 0 ? "Plus de marteaux" : "Pile pleine : examine tes pièces");
            return;
        }
        if (world.Busy) return;
        var item = GameState.Forge();
        if (item == null) return;
        world.QuickStrike(GameState.CircleColors[item.circle]);
        if (item.circle >= 2) Sfx.Reveal(item.circle);
        queuePulse = 1f;
        if (GameState.LastForgeFree) Toast("Frappe gratuite !");
    }

    // Ouvre la fiche de la pièce suivante de la pile (ou ferme la fiche quand la pile est vide).
    void ReviewNext()
    {
        var it = GameState.TakeNext();
        if (it == null) { HidePopup(); return; }
        ShowPopup(it);
    }

    void OnEquip()
    {
        Sfx.Equip();
        long g = GameState.EquipPending();
        world.DismissItem(true);
        ReviewNext();
        if (g > 0) Toast("Ancienne pièce revendue : +" + GameState.Fmt(g) + " or");
    }

    void OnSell()
    {
        Sfx.Coin();
        long g = GameState.SellPending();
        world.DismissItem(false);
        ReviewNext();
        Toast("+" + GameState.Fmt(g) + " or");
    }

    void OnUpgrade()
    {
        if (GameState.IsMaxLevel) { ShowAscend(GameState.AscForge); return; }
        if (!GameState.PayNode()) Toast("Pas assez d'or");
    }

    void OnSpeedUp()
    {
        if (!GameState.SpeedUp()) Toast("Pas assez de gemmes");
    }

    void OnLevelUp()
    {
        world.CelebrateLevelUp();
        Toast("La forge passe au niveau " + GameState.Data.forgeLevel + " !");
    }

    void Toast(string msg)
    {
        toast.text = msg;
        toastTime = 2.2f;
        toastGroup.transform.SetAsLastSibling();   // au-dessus des panneaux
    }

    // ---------- Fenêtre de la nouvelle pièce ----------
    void ShowPopup(Item it)
    {
        Color c = GameState.CircleColors[it.circle];
        popFrame.color = c;
        popName.text = it.Name;
        popName.color = Color.Lerp(c, Color.white, 0.25f);
        int left = GameState.Data.forged.Count;
        popRemain.text = left > 0 ? "encore " + left : "";
        popInfo.text = "Cercle " + (it.circle + 1) + " : " + GameState.CircleNames[it.circle];
        var cur = GameState.Data.equipped[it.slot];
        int cmp = !cur.valid ? 1 : it.MainStat.CompareTo(cur.MainStat);
        SetPopTile(popCur, cur, TextDim);
        SetPopTile(popNew, it, cmp > 0 ? new Color(0.45f, 1f, 0.5f) : cmp < 0 ? new Color(1f, 0.5f, 0.42f) : Color.white);
        popNew.stat.text += cmp > 0 ? " ▲" : cmp < 0 ? " ▼" : "";

        long pDiff = GameState.PowerIfEquipped(it) - GameState.Power();
        popBetter = pDiff > 0 || !cur.valid;
        string pCol = pDiff > 0 ? "#6EE07A" : pDiff < 0 ? "#FF6A5A" : "#CCCCCC";
        popStat.text = "Puissance  <color=" + pCol + ">" + (pDiff > 0 ? "▲ +" : pDiff < 0 ? "▼ " : "= ") + GameState.Fmt(pDiff) + "</color>";
        SetPills(popPills, it.subs, it.subVals);
        int rows = (it.SubCount + 1) / 2;
        popSubs.rectTransform.offsetMax = new Vector2(-20, rows == 0 ? -560 : -566 - rows * 72);
        popSubs.text = cur.valid && cur.SubCount > 0 ? "Équipé : " + SubsRich(cur, "   ") : "";
        popSellText.text = "VENDRE\n+" + GameState.Fmt(it.SellValue) + " or";
        popup.SetActive(true);
        Refresh();
    }

    Pills popPills;
    Text popRemain;
    Button popEquipBtn;
    bool popBetter;

    static string SubsRich(Item it, string sep)
    {
        if (it.SubCount == 0) return "";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < it.SubCount; i++) { if (i > 0) sb.Append(sep); sb.Append(SubRich(it.subs[i], it.subVals[i])); }
        return sb.ToString();
    }

    static string SubsText(Item it, string sep)
    {
        if (it.SubCount == 0) return "";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < it.SubCount; i++) { if (i > 0) sb.Append(sep); sb.Append(it.SubLine(i)); }
        return sb.ToString();
    }

    void HidePopup()
    {
        popup.SetActive(false);
        Refresh();
    }

    // ---------- Mise à jour ----------
    // Calcule les bandes d'écran libres entre les menus : forge en bas, combat en haut.
    void LateUpdate()
    {
        var size = new Vector2Int(Screen.width, Screen.height);
        if (size != lastScreen) { lastScreen = size; layoutFrames = 3; ApplySafeArea(); }
        if (layoutFrames > 0)
        {
            layoutFrames--;
            Canvas.ForceUpdateCanvases();
            var c = new Vector3[4];
            float W = Mathf.Max(1, Screen.width), H = Mathf.Max(1, Screen.height);
            equipRect.GetWorldCorners(c);
            float battleBottom = c[1].y / H + 0.004f;
            statsRect.GetWorldCorners(c);
            float battleTop = c[0].y / H;
            anvilRect.GetWorldCorners(c);
            var anvil = new Rect(c[0].x / W, c[0].y / H, (c[2].x - c[0].x) / W, (c[2].y - c[0].y) / H);
            var expanded = new Rect(0f, anvil.y, 1f, battleTop - anvil.y);
            world.SetBand(new Rect(0f, anvil.y, 1f, anvil.height), expanded);   // fond de la forge sur toute la largeur
            battle.SetBand(battleBottom, battleTop);
        }
        battle.SetVisible(world.ExpandAmount < 0.98f);

        // Pendant la révélation d'une pièce, la forge s'agrandit : on efface l'équipement et les boutons qui la recouvraient.
        float a = 1f - Mathf.Clamp01(world.ExpandAmount * 2f);
        foreach (var g in new[] { forgeRowGroup, equipGroup })
        {
            if (g == null) continue;
            g.alpha = a;
            g.blocksRaycasts = a > 0.99f;
        }
    }

    CanvasGroup forgeRowGroup, equipGroup;

    void OnAutoToggle()
    {
        if (!GameState.AutoForgeUnlocked) { Toast("Forge automatique : à débloquer dans l'arbre technologique (Forge)"); return; }
        GameState.Data.autoForge = !GameState.Data.autoForge;
        GameState.Save();
        RefreshTimers();
    }

    void UpdateAuto()
    {
        bool on = GameState.AutoForgeUnlocked && GameState.Data.autoForge;
        if (!on || popup.activeSelf || world.Busy || world.ExpandAmount > 0.01f) return;
        autoTimer -= Time.deltaTime;
        if (autoTimer > 0f) return;
        autoTimer = GameState.AutoForgeInterval;
        if (GameState.TakeNext() != null) { Toast(GameState.AutoResolvePending()); return; }
        var it = GameState.Forge();
        if (it == null) return;
        world.QuickStrike(GameState.CircleColors[it.circle]);
        GameState.TakeNext();
        string msg = GameState.AutoResolvePending();
        if (GameState.LastForgeFree) msg += "  (frappe gratuite !)";
        Toast(msg);
    }

    // Vrai si un écran plein (compagnons, boutique…) est ouvert : on masque alors ce qui dépasse (bouton Pass).
    bool AnyPanelOpen()
    {
        foreach (var p in new[] { statsPanel, compPanel, dungPanel, techPanel, mountPanel, skillPanel, shopPanel, missionPanel, passPanel, skinsPanel, ssPanel })
            if (p != null && p.activeSelf) return true;
        return false;
    }

    void Update()
    {
        if (passBtn != null) passBtn.gameObject.SetActive(!AnyPanelOpen() && !popup.activeSelf);
        UpdateAuto();
        UpdateNav();
        UpdateQueueBadge();
        UpdateTech();
        UpdateCompanions();
        UpdateDungeons();
        UpdateShop();
        UpdateMissions();
        UpdateStones();
        UpdateHeroView();
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.25f;
            GameState.Tick();
            RefreshTimers();
        }
        if (toastTime > 0f)
        {
            toastTime -= Time.deltaTime;
            toastGroup.alpha = Mathf.Clamp01(toastTime / 0.5f);
        }
    }

    void Refresh()
    {
        RefreshQueueBadge();
        if (statsPanel != null && statsPanel.activeSelf) RefreshStatsPanel();
        RefreshCompanions();
        RefreshDungeons();
        RefreshTech();
        RefreshMounts();
        RefreshSkills();
        RefreshHeroSkins();
        RefreshStones();
        RefreshShop();
        RefreshMissions();
        var d = GameState.Data;
        goldText.text = GameState.Fmt(d.gold);
        gemText.text = GameState.Fmt(d.gems);
        statsText.text = "<size=40><color=#FFC54A>      " + GameState.Fmt(GameState.Power()) + "</color></size>\n<size=26>ATQ " + GameState.Fmt(GameState.TotalAtk()) + "   PV " + GameState.Fmt(GameState.TotalHp()) + "</size>";

        forgeLevelText.text = (d.forgeStars > 0 ? "<color=#FFD24A>" + GameState.StarText(d.forgeStars) + "</color> " : "") + "Forge niv. " + d.forgeLevel + (GameState.IsMaxLevel ? " (max)" : "");
        // Bouton des chances : seulement le meilleur cercle possible
        var odds = GameState.CurrentOdds();
        int best = 0;
        for (int i = 0; i < odds.Length; i++) if (odds[i] > 0f) best = i;
        oddsText.text = "Chances <color=#9FC8FF>(?)</color>\n<color=" + Hex(GameState.CircleColors[best]) + ">" + GameState.CircleNames[best] + " " + odds[best].ToString("0.##") + "%</color>";

        for (int i = 0; i < GameState.SlotCount; i++)
        {
            var it = d.equipped[i];
            if (it.valid)
            {
                Color c = GameState.CircleColors[it.circle];
                slotBg[i].color = Color.Lerp(c, Color.white, 0.12f);
                slotIcon[i].texture = ItemIcons.Get(it);
                slotIcon[i].color = Color.white;
                slotLevel[i].text = (it.stars > 0 ? "<color=#FFD24A>" + GameState.StarText(it.stars) + "</color> " : "") + "<b>Niv. " + it.level + "</b>" + (it.SubCount > 0 ? "\n<size=30>" + SubDots(it.subs) + "</size>" : "");
                slotText[i].text = "<size=23>" + it.StatLabel + " " + GameState.Fmt(it.MainStat) + "</size>";
            }
            else
            {
                slotBg[i].color = new Color(0.32f, 0.22f, 0.2f);
                // Emplacement vide : silhouette sombre de la pièce.
                slotIcon[i].texture = ItemIcons.Get(i, 0);
                slotIcon[i].color = new Color(0f, 0f, 0f, 0.55f);
                slotLevel[i].text = "";
                slotText[i].text = "<b>" + GameState.SlotNames[i] + "</b>  <color=#776655>vide</color>";
            }
        }
        RefreshTimers();
    }

    void RefreshTimers()
    {
        var d = GameState.Data;
        int next = GameState.SecondsToNextHammer();
        hammerText.text = d.hammers + (next > 0 && d.hammers < GameState.HammerCap ? "  <size=22>+1 " + next + "s</size>" : "");

        bool autoOn = GameState.AutoForgeUnlocked && GameState.Data.autoForge;
        autoText.text = !GameState.AutoForgeUnlocked ? "Auto\n<size=18>verrouillé</size>" : autoOn ? "Auto\n<size=20>ACTIVÉ</size>" : "Auto\n<size=20>arrêté</size>";
        autoBtn.GetComponent<Image>().color = UISkin.Vivid(autoOn ? new Color(0.2f, 0.55f, 0.25f) : new Color(0.25f, 0.25f, 0.3f));
        bool canForge = GameState.CanForge && !world.Busy;
        forgeBtn.interactable = canForge;
        forgeBtnText.text = d.hammers > 0 ? (d.totalForged < 3 ? "Touche l'enclume !" : "") : "Plus de marteaux";

        if (GameState.IsMaxLevel)
        {
            bool canAsc = GameState.Stars(GameState.AscForge) < GameState.MaxStars;
            upgradeBtn.interactable = canAsc;
            upgradeText.text = canAsc ? "ASCENSION ★\n<size=24>" + GameState.Fmt(GameState.ForgeAscendCost) + " or</size>" : "Ascension max";
            speedUpBtn.gameObject.SetActive(false);
        }
        else if (d.upgrading)
        {
            upgradeBtn.interactable = false;
            upgradeText.text = "En cours\n<size=24>" + GameState.FmtTime(GameState.UpgradeSecondsLeft()) + "</size>";
            speedUpBtn.gameObject.SetActive(true);
            speedUpBtn.interactable = d.gems >= GameState.SpeedUpGemCost();
            speedUpText.text = "Finir\n<size=22>" + GameState.SpeedUpGemCost() + " gemmes</size>";
        }
        else
        {
            upgradeBtn.interactable = d.gold >= GameState.NodeCost;
            string nodes = GameState.NodesNeeded > 1 ? " (" + (d.nodesPaid + 1) + "/" + GameState.NodesNeeded + ")" : "";
            upgradeText.text = "Améliorer" + nodes + "\n<size=26>" + GameState.Fmt(GameState.NodeCost) + " or</size>";
            speedUpBtn.gameObject.SetActive(false);
        }
    }
}
