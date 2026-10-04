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
    Text statsPanelText;
    Image popFrame;
    Text toast;
    float toastTime;
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
        var sh = rt.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.8f);
        sh.effectDistance = new Vector2(2, -2);
        return t;
    }

    Button MakeButton(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color c, string txt, int size, out Text label, UnityEngine.Events.UnityAction onClick)
    {
        var img = Box(parent, name, aMin, aMax, offMin, offMax, c);
        var b = img.gameObject.AddComponent<Button>();
        var colors = b.colors;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        b.colors = colors;
        b.onClick.AddListener(onClick);
        // Liseré
        var outline = img.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.6f);
        outline.effectDistance = new Vector2(3, -3);
        label = Label(img.transform, txt, size, TextAnchor.MiddleCenter, Color.white);
        return b;
    }

    // ---------- Construction de l'écran (disposition inspirée de Forge Master) ----------
    RectTransform anvilRect;
    Text anvilText;

    void Create()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
        statsRect = top.rectTransform;
        var avatar = Box(top.transform, "Portrait", new Vector2(0, 0), new Vector2(0, 1), new Vector2(18, 14), new Vector2(120, -14), new Color(0.35f, 0.08f, 0.05f));
        avatar.gameObject.AddComponent<Outline>().effectColor = Ember;
        Label(avatar.transform, "☠", 60, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.6f));
        statsText = Label(top.transform, "", 30, TextAnchor.MiddleLeft, TextMain, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(135, 0), Vector2.zero);
        var goldBox = Box(top.transform, "Or", new Vector2(0.52f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0, -30), new Vector2(-8, 30), new Color(0, 0, 0, 0.6f));
        goldText = Label(goldBox.transform, "", 36, TextAnchor.MiddleRight, new Color(1f, 0.82f, 0.3f), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-14, 0));
        var gemBox = Box(top.transform, "Gemmes", new Vector2(0.75f, 0.5f), new Vector2(1f, 0.5f), new Vector2(8, -30), new Vector2(-18, 30), new Color(0, 0, 0, 0.6f));
        gemText = Label(gemBox.transform, "", 36, TextAnchor.MiddleRight, new Color(0.5f, 0.9f, 1f), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-14, 0));

        // ----- Barre de navigation (emplacements pour la suite) -----
        var nav = Box(R, "Navigation", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 150), new Color(0.07f, 0.04f, 0.05f, 0.97f));
        string[] navNames = { "Héros", "Donjons", "Compa-\ngnons", "Montures", "Compé-\ntences", "Techno", "Missions", "Boutique" };
        int navCount = navNames.Length;
        for (int i = 0; i < navCount; i++)
        {
            Text nt;
            int idx = i;
            MakeButton(nav.transform, navNames[i], new Vector2(i / (float)navCount, 0), new Vector2((i + 1) / (float)navCount, 1), new Vector2(3, 12), new Vector2(-3, -12),
                new Color(0.32f, 0.13f, 0.1f), navNames[i], 21, out nt, () =>
                {
                    if (idx != 4 && skillPanel != null) skillPanel.SetActive(false);
                    if (idx != 7 && shopPanel != null) shopPanel.SetActive(false);
                    if (idx != 6 && missionPanel != null) missionPanel.SetActive(false);
                    if (passPanel != null) passPanel.SetActive(false);
                    if (idx == 0) ToggleStats();
                    else if (idx == 1) ToggleDungeons();
                    else if (idx == 2) ToggleCompanions();
                    else if (idx == 3) { if (mountPanel.activeSelf) mountPanel.SetActive(false); else ToggleMounts(); }
                    else if (idx == 4) ToggleSkills();
                    else if (idx == 5) ToggleTech();
                    else if (idx == 6) ToggleMissions();
                    else if (idx == 7) ToggleShop();
                    else Toast(navNames[idx].Replace("\n", "") + " : bientôt !");
                });
        }

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
        var hammerPlate = Box(anvilRect, "Marteaux", new Vector2(0.18f, 0f), new Vector2(0.82f, 0f), new Vector2(0, 8), new Vector2(0, 70), new Color(0.1f, 0.03f, 0.02f, 0.85f));
        hammerPlate.gameObject.AddComponent<Outline>().effectColor = Ember;
        hammerPlate.raycastTarget = false;
        hammerText = Label(hammerPlate.transform, "", 32, TextAnchor.MiddleCenter, Color.white);
        anvilText = Label(anvilRect, "Touche l'enclume !", 26, TextAnchor.UpperCenter, new Color(1f, 0.75f, 0.5f), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -6));
        forgeBtnText = anvilText;

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
        var eq = MakeRect("Equipement", R, new Vector2(0, 0), new Vector2(1, 0), new Vector2(18, 530), new Vector2(-18, 850));
        equipRect = eq;
        equipGroup = eq.gameObject.AddComponent<CanvasGroup>();
        for (int i = 0; i < GameState.SlotCount; i++)
        {
            int row = i / 4, col = i % 4;
            var aMin = new Vector2(col / 4f, row == 0 ? 0.5f : 0f);
            var aMax = new Vector2((col + 1) / 4f, row == 0 ? 1f : 0.5f);
            slotBg[i] = Box(eq, "Emplacement " + i, aMin, aMax, new Vector2(7, 7), new Vector2(-7, -7), Panel);
            var ol = slotBg[i].gameObject.AddComponent<Outline>();
            ol.effectDistance = new Vector2(4, -4);
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
        BuildShopPanel(R);
        BuildMissionPanel(R);
        BuildPassPanel(R);
        BuildDetailPopup(R);

        toast = Label(R, "", 40, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 860), new Vector2(0, 930));

        GameState.Changed += Refresh;
        GameState.ForgeLeveledUp += OnLevelUp;
        Refresh();

        if (GameState.Data.pending.valid)
        {
            world.ShowItemInstant(GameState.Data.pending);
            ShowPopup(GameState.Data.pending);
        }
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
        statsPanel = MakeRect("Fiche du héros", R, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        var shade = Box(statsPanel.transform, "Voile", Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130), new Color(0, 0, 0, 0.6f));
        shade.raycastTarget = true;
        var frame = Box(statsPanel.transform, "Cadre", new Vector2(0.07f, 0.2f), new Vector2(0.93f, 0.85f), Vector2.zero, Vector2.zero, Ember);
        var inner = Box(frame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6), new Color(0.07f, 0.035f, 0.035f, 0.98f));
        Label(inner.transform, "HÉROS", 50, TextAnchor.UpperCenter, Ember, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -24));
        statsPanelText = Label(inner.transform, "", 34, TextAnchor.UpperLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(50, 130), new Vector2(-50, -110));
        Text closeT;
        MakeButton(inner.transform, "Fermer", new Vector2(0.3f, 0f), new Vector2(0.7f, 0f), new Vector2(0, 24), new Vector2(0, 110),
            new Color(0.45f, 0.15f, 0.08f), "FERMER", 38, out closeT, ToggleStats);
        Text resetT;
        MakeButton(inner.transform, "Reset", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20, 30), new Vector2(190, 90),
            new Color(0.2f, 0.08f, 0.08f, 0.9f), "Reset (test)", 20, out resetT, () => { GameState.ResetAll(); world.DismissItem(false); HidePopup(); ToggleStats(); });
        statsPanel.SetActive(false);
    }

    void CloseAllPanels()
    {
        foreach (var p in new[] { statsPanel, compPanel, dungPanel, techPanel, mountPanel, skillPanel, shopPanel, missionPanel, passPanel, detail })
            if (p != null) p.SetActive(false);
    }

    void ToggleStats()
    {
        bool open = !statsPanel.activeSelf;
        CloseAllPanels();
        statsPanel.SetActive(open);
        if (statsPanel.activeSelf) RefreshStatsPanel();
    }

    void RefreshStatsPanel()
    {
        string L(string name, string val) => name + "<color=#FFC07A>  " + val + "</color>\n";
        var sb = new System.Text.StringBuilder();
        sb.Append(L("Puissance", GameState.Fmt(GameState.Power())));
        sb.Append(L("Attaque", GameState.Fmt(GameState.TotalAtk())));
        sb.Append(L("Points de vie", GameState.Fmt(GameState.TotalHp())));
        sb.Append(L("Attaques par seconde", (1f / GameState.AttackInterval).ToString("0.00")));
        sb.Append("\n");
        sb.Append(L("Chance de critique", GameState.FmtPct(GameState.CritChance)));
        sb.Append(L("Dégâts critiques", "x" + GameState.CritMult.ToString("0.##")));
        sb.Append(L("Double frappe", GameState.FmtPct(GameState.DoubleChance)));
        sb.Append(L("Vitesse d'attaque", "+" + GameState.FmtPct(GameState.AttackSpeedBonus)));
        sb.Append(L("Dégâts", "+" + GameState.FmtPct(GameState.SubTotal(GameState.SubDamage))));
        sb.Append(L("Dégâts en mêlée", "+" + GameState.FmtPct(GameState.SubTotal(GameState.SubMelee))));
        sb.Append(L("Santé", "+" + GameState.FmtPct(GameState.SubTotal(GameState.SubHealth))));
        sb.Append(L("Vol de vie", GameState.FmtPct(GameState.LifeSteal)));
        sb.Append(L("Blocage", GameState.FmtPct(GameState.BlockChance)));
        sb.Append(L("Régénération", GameState.FmtPct(GameState.RegenPerSecond) + " / s"));
        statsPanelText.text = sb.ToString();
    }

    void BuildPopup(Transform R)
    {
        popup = MakeRect("Nouvelle pièce", R, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        // Fond qui bloque les clics en bas seulement : la pièce 3D reste visible en haut.
        var shade = Box(popup.transform, "Voile", new Vector2(0, 0), new Vector2(1, 0.56f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.55f));
        shade.raycastTarget = true;
        popFrame = Box(popup.transform, "Cadre", new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
        var inner = Box(popFrame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6), new Color(0.07f, 0.035f, 0.035f, 0.97f));
        Transform c = inner.transform;
        popName = Label(c, "", 54, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -30));
        popInfo = Label(c, "", 34, TextAnchor.UpperCenter, TextDim, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -105));
        popStat = Label(c, "", 56, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -160));
        popSubs = Label(c, "", 34, TextAnchor.UpperCenter, new Color(0.75f, 0.85f, 1f), Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -228));
        popCompare = Label(c, "", 30, TextAnchor.UpperCenter, TextMain, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -322));
        Text eqText;
        MakeButton(c, "Equiper", new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(30, 30), new Vector2(-15, 170),
            new Color(0.15f, 0.5f, 0.2f), "ÉQUIPER", 48, out eqText, OnEquip);
        MakeButton(c, "Vendre", new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(15, 30), new Vector2(-30, 170),
            new Color(0.5f, 0.14f, 0.1f), "", 40, out popSellText, OnSell);
        popup.SetActive(false);
    }

    // ---------- Actions ----------
    void OnForge()
    {
        if (world.Busy || !GameState.CanForge) return;
        var item = GameState.Forge();
        if (item == null) return;
        world.PlayForge(item, () => ShowPopup(item));
    }

    void OnEquip()
    {
        long g = GameState.EquipPending();
        world.DismissItem(true);
        HidePopup();
        if (g > 0) Toast("Ancienne pièce revendue : +" + GameState.Fmt(g) + " or");
    }

    void OnSell()
    {
        long g = GameState.SellPending();
        world.DismissItem(false);
        HidePopup();
        Toast("+" + GameState.Fmt(g) + " or");
    }

    void OnUpgrade()
    {
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
    }

    // ---------- Fenêtre de la nouvelle pièce ----------
    void ShowPopup(Item it)
    {
        Color c = GameState.CircleColors[it.circle];
        popFrame.color = c;
        popName.text = it.Name;
        popName.color = Color.Lerp(c, Color.white, 0.25f);
        popInfo.text = "Cercle " + (it.circle + 1) + " : " + GameState.CircleNames[it.circle] + "  ·  Niveau " + it.level;
        popStat.text = it.StatLabel + " +" + GameState.Fmt(it.MainStat);
        popSubs.text = SubsText(it, "\n");

        var cur = GameState.Data.equipped[it.slot];
        long pDiff = GameState.PowerIfEquipped(it) - GameState.Power();
        string pCol = pDiff > 0 ? "#6EE07A" : pDiff < 0 ? "#FF6A5A" : "#CCCCCC";
        string pTxt = "Puissance <color=" + pCol + ">" + (pDiff > 0 ? "▲ +" : pDiff < 0 ? "▼ " : "= ") + GameState.Fmt(pDiff) + "</color>";
        if (!cur.valid)
            popCompare.text = pTxt + "\nEmplacement vide : <color=#6EE07A>équipe-la !</color>";
        else
        {
            string curSubs = cur.SubCount > 0 ? "\n<color=#8A8A9A>" + SubsText(cur, "  ·  ") + "</color>" : "";
            popCompare.text = pTxt + "\n<color=#A89C94>Équipé : " + cur.Name + " (niv. " + cur.level + ") · " + cur.StatLabel + " " + GameState.Fmt(cur.MainStat) + "</color>" + curSubs;
        }
        popSellText.text = "VENDRE\n+" + GameState.Fmt(it.SellValue) + " or";
        popup.SetActive(true);
        Refresh();
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
            world.SetBand(anvil, expanded);
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
        if (GameState.Data.pending.valid) { Toast(GameState.AutoResolvePending()); return; }
        var it = GameState.Forge();
        if (it == null) return;
        world.QuickStrike(GameState.CircleColors[it.circle]);
        string msg = GameState.AutoResolvePending();
        if (GameState.LastForgeFree) msg += "  (frappe gratuite !)";
        Toast(msg);
    }

    // Vrai si un écran plein (compagnons, boutique…) est ouvert : on masque alors ce qui dépasse (bouton Pass).
    bool AnyPanelOpen()
    {
        foreach (var p in new[] { statsPanel, compPanel, dungPanel, techPanel, mountPanel, skillPanel, shopPanel, missionPanel, passPanel })
            if (p != null && p.activeSelf) return true;
        return false;
    }

    void Update()
    {
        if (passBtn != null) passBtn.gameObject.SetActive(!AnyPanelOpen() && !popup.activeSelf);
        UpdateAuto();
        UpdateTech();
        UpdateCompanions();
        UpdateDungeons();
        UpdateShop();
        UpdateMissions();
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
            var col = toast.color;
            col.a = Mathf.Clamp01(toastTime / 0.5f);
            toast.color = col;
        }
    }

    void Refresh()
    {
        if (statsPanel != null && statsPanel.activeSelf) RefreshStatsPanel();
        RefreshCompanions();
        RefreshDungeons();
        RefreshTech();
        RefreshMounts();
        RefreshSkills();
        RefreshShop();
        RefreshMissions();
        var d = GameState.Data;
        goldText.text = "<size=26>or</size>  " + GameState.Fmt(d.gold);
        gemText.text = "<size=26>gemmes</size>  " + GameState.Fmt(d.gems);
        statsText.text = "<color=#FFB347><b>Puissance " + GameState.Fmt(GameState.Power()) + "</b></color>\nATQ " + GameState.Fmt(GameState.TotalAtk()) + "   PV " + GameState.Fmt(GameState.TotalHp());

        forgeLevelText.text = "Forge niv. " + d.forgeLevel + (GameState.IsMaxLevel ? " (max)" : "");
        // Bouton des chances : seulement le meilleur cercle possible
        var odds = GameState.CurrentOdds();
        int best = 0;
        for (int i = 0; i < odds.Length; i++) if (odds[i] > 0f) best = i;
        oddsText.text = "Chances <color=#9FC8FF>(?)</color>\n<color=" + Hex(GameState.CircleColors[best]) + ">" + GameState.CircleNames[best] + " " + odds[best].ToString("0.##") + "%</color>";

        for (int i = 0; i < GameState.SlotCount; i++)
        {
            var it = d.equipped[i];
            var ol = slotBg[i].GetComponent<Outline>();
            if (it.valid)
            {
                Color c = GameState.CircleColors[it.circle];
                slotBg[i].color = new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.45f, 0.95f);
                ol.effectColor = c;
                slotIcon[i].texture = ItemIcons.Get(i, it.circle);
                slotIcon[i].color = Color.white;
                slotLevel[i].text = "<b>Niv. " + it.level + "</b>" + (it.SubCount > 0 ? "\n<size=19><color=#BFD8FF>+" + it.SubCount + " bonus</color></size>" : "");
                slotText[i].text = "<size=23>" + it.StatLabel + " " + GameState.Fmt(it.MainStat) + "</size>";
            }
            else
            {
                slotBg[i].color = Panel;
                ol.effectColor = new Color(0.3f, 0.15f, 0.1f);
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
        hammerText.text = "⚒ " + d.hammers + (next > 0 && d.hammers < GameState.HammerCap ? "  <size=22>+1 " + next + "s</size>" : "");

        bool autoOn = GameState.AutoForgeUnlocked && GameState.Data.autoForge;
        autoText.text = !GameState.AutoForgeUnlocked ? "Auto\n<size=18>verrouillé</size>" : autoOn ? "Auto\n<size=20>ACTIVÉ</size>" : "Auto\n<size=20>arrêté</size>";
        autoBtn.GetComponent<Image>().color = autoOn ? new Color(0.2f, 0.55f, 0.25f) : new Color(0.25f, 0.25f, 0.3f);
        bool canForge = GameState.CanForge && !world.Busy;
        forgeBtn.interactable = canForge;
        forgeBtnText.text = d.hammers > 0 ? (d.totalForged < 3 ? "Touche l'enclume !" : "") : "Plus de marteaux";

        if (GameState.IsMaxLevel)
        {
            upgradeBtn.interactable = false;
            upgradeText.text = "Niveau maximum";
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
