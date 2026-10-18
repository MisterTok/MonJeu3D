using UnityEngine;
using UnityEngine.UI;

// Navigation calquée sur Forge Master : 5 onglets en bas (Ligue, Donjons, Compétences/Animaux/Techno, Clan, Boutique),
// pas d'onglet Forge : l'accueil est l'écran sans panneau ; l'onglet ouvert devient une croix rouge qui le referme.
// Sous-onglets : centre = Compétences, Animaux, Arbre techno ; Clan = Membres, Missions, (verrou), Guerre.
// Hors barre (comme FM) : le portrait en haut à gauche ouvre le Héros (Héros, Skins) ; la monture est la 9e tuile.
public partial class ForgeUI
{
    static readonly string[] NavNames = { "Ligue", "Donjons", "Collection", "Clan", "Boutique" };
    static readonly string[] NavIcons = { "i_skull", "i_door", "i_paw", "i_banner", "i_shop" };
    readonly Image[] navIcon = new Image[5];
    readonly Image[] navImg = new Image[5];
    readonly Text[] navText = new Text[5];
    readonly GameObject[] navDot = new GameObject[5];
    static readonly Color NavOff = new Color(0.2f, 0.09f, 0.08f), NavOn = new Color(0.78f, 0.3f, 0.08f);
    const int Soon = 99999;   // fonction en ligne pas encore faite (ligue, clan) : « bientôt »

    class SubTabs { public RectTransform root; public Image[] img; public Text[] txt; public GameObject[] dot; public int[] unlock; public string[] names; public GameObject[] panels; public System.Action[] open; public int last; }
    SubTabs heroTabs, collTabs, clanTabs;
    Transform navRoot;
    GameObject heroDot;

    void BuildNav(Transform R)
    {
        var nav = Box(R, "Navigation", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 150), new Color(0.07f, 0.04f, 0.05f, 0.98f));
        if (UISkin.Get("navbar") != null) { UISkin.Sliced(nav, "navbar"); nav.color = Color.white; }
        navRoot = nav.transform;
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            var b = MakeButton(nav.transform, "Onglet " + NavNames[i], new Vector2(i / 5f, 0), new Vector2((i + 1) / 5f, 1), new Vector2(4, 10), new Vector2(-4, -14),
                NavOff, NavNames[i], 24, out navText[i], () => OnNav(idx));
            navImg[i] = b.GetComponent<Image>();
            UISkin.Sliced(navImg[i], "tab", 1.2f);
            navIcon[i] = UISkin.Icon(b.transform, NavIcons[i], new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-46, -88), new Vector2(46, 4));
            navText[i].alignment = TextAnchor.LowerCenter;
            navText[i].rectTransform.offsetMin = new Vector2(2, 6);
            navDot[i] = MakeDot(b.transform);
        }
    }

    // Le portrait de la barre du haut ouvre la fiche du héros (comme le profil de FM).
    void MakePortraitButton(Image avatar)
    {
        var b = avatar.gameObject.AddComponent<Button>();
        b.onClick.AddListener(Sfx.Click);
        b.onClick.AddListener(() => OpenGroup(heroTabs));
        heroDot = MakeDot(avatar.transform);
    }

    int NavActive()
    {
        if (shopPanel != null && shopPanel.activeSelf) return 4;
        if (dungPanel != null && dungPanel.activeSelf) return 1;
        if (GroupOpen(collTabs) >= 0) return 2;
        if (GroupOpen(clanTabs) >= 0) return 3;
        return -1;
    }

    static int NavUnlock(int i) => i == 0 ? Soon : i == 1 ? GameState.UnlockDungeons : i == 2 ? GameState.UnlockSkills : i == 4 ? GameState.UnlockShop : 0;

    void OnNav(int idx)
    {
        if (passPanel != null) passPanel.SetActive(false);
        if (idx == NavActive()) { CloseAllPanels(); return; }   // l'onglet ouvert (croix) referme
        if (!GameState.Reached(NavUnlock(idx))) { Toast(LockText(NavUnlock(idx))); return; }
        switch (idx)
        {
            case 1: ToggleDungeons(); break;
            case 2: OpenGroup(collTabs); break;
            case 3: OpenGroup(clanTabs); break;
            case 4: ToggleShop(); break;
        }
    }

    static string LockText(int unlock) => unlock >= Soon ? "Bientôt : demande le serveur en ligne" : GameState.UnlockText(unlock);

    // Toucher l'onglet d'un groupe déjà ouvert le referme ; sinon rouvre le dernier sous-onglet utilisé.
    void OpenGroup(SubTabs g)
    {
        if (GroupOpen(g) >= 0) { CloseAllPanels(); return; }
        int t = g.last;
        if (!GameState.Reached(g.unlock[t])) t = System.Array.FindIndex(g.unlock, u => GameState.Reached(u));
        if (t < 0) { Toast(LockText(g.unlock[0])); return; }
        OpenTab(g, t);
    }

    void OpenTab(SubTabs g, int i)
    {
        if (!GameState.Reached(g.unlock[i]) || g.panels[i] == null) { Toast(LockText(g.unlock[i])); return; }
        g.last = i;
        if (!g.panels[i].activeSelf) g.open[i]();
    }

    int GroupOpen(SubTabs g)
    {
        if (g == null) return -1;
        for (int i = 0; i < g.panels.Length; i++) if (g.panels[i] != null && g.panels[i].activeSelf) return i;
        return -1;
    }

    // Barre de sous-onglets posée sur le titre des panneaux (le bouton X reste visible à droite).
    SubTabs MakeSubTabs(Transform R, string[] names, GameObject[] panels, System.Action[] open, int[] unlock)
    {
        var g = new SubTabs { panels = panels, open = open, unlock = unlock, names = names, img = new Image[names.Length], txt = new Text[names.Length], dot = new GameObject[names.Length] };
        g.root = MakeRect("Sous-onglets", R, new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -216), new Vector2(-114, -140));
        var gimg = g.root.gameObject.AddComponent<Image>();
        gimg.color = new Color(0.05f, 0.025f, 0.03f, 1f);
        if (UISkin.Get("card") != null) { UISkin.Sliced(gimg, "card"); gimg.color = Color.white; }
        for (int i = 0; i < names.Length; i++)
        {
            int idx = i;
            var b = MakeButton(g.root, names[i], new Vector2(i / (float)names.Length, 0), new Vector2((i + 1) / (float)names.Length, 1), new Vector2(4, 6), new Vector2(-4, -6),
                NavOff, names[i], 26, out g.txt[i], () => OpenTab(g, idx));
            g.img[i] = b.GetComponent<Image>();
            UISkin.Sliced(g.img[i], "tab", 1.4f);
            if (names[i] == "") UISkin.Icon(b.transform, "i_lock", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-22, -22), new Vector2(22, 22));
            g.dot[i] = MakeDot(b.transform);
        }
        g.root.gameObject.SetActive(false);
        return g;
    }

    void BuildSubTabs(Transform R)
    {
        heroTabs = MakeSubTabs(R, new[] { "Héros", "Skins" },
            new[] { statsPanel, skinsPanel },
            new System.Action[] { ToggleStats, ToggleHeroSkins },
            new[] { 0, 0 });
        collTabs = MakeSubTabs(R, new[] { "Compétences", "Animaux", "Arbre techno" },
            new[] { skillPanel, compPanel, techPanel },
            new System.Action[] { ToggleSkills, ToggleCompanions, ToggleTech },
            new[] { GameState.UnlockSkills, GameState.UnlockPets, GameState.UnlockTech });
        clanTabs = MakeSubTabs(R, new[] { "Membres", "Missions", "", "Guerre" },
            new GameObject[] { null, missionPanel, null, null },
            new System.Action[] { null, ToggleMissions, null, null },
            new[] { Soon, 0, Soon, Soon });
        // Barre du bas et sous-onglets au-dessus des panneaux (la fiche de détail reste par-dessus).
        navRoot.SetAsLastSibling();
        heroTabs.root.SetAsLastSibling();
        collTabs.root.SetAsLastSibling();
        clanTabs.root.SetAsLastSibling();
    }

    // Outil de test : liste les éléments affichés sous un point de l'écran (x,y en pixels, origine en bas à gauche).
    void TestUiAt(string xy)
    {
        var parts = xy.Split(',');
        var pt = new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
        var sb = new System.Text.StringBuilder();
        foreach (var gr in GetComponentsInChildren<Graphic>())
            if (gr.isActiveAndEnabled && gr.canvasRenderer.GetAlpha() > 0 && RectTransformUtility.RectangleContainsScreenPoint(gr.rectTransform, pt, null))
                sb.AppendLine(gr.GetType().Name + "  " + GetPath(gr.transform) + "  col=" + gr.color + " depth=" + gr.depth);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "../../_dl/uiat.txt"), sb.ToString());
    }
    static string GetPath(Transform t) => t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;

    // Outil de test (commande de capture « open:<écran> »).
    void TestOpen(string w)
    {
        CloseAllPanels();
        switch (w)
        {
            case "shop": OnNav(4); break;
            case "hero": OpenTab(heroTabs, 0); break;
            case "skins": OpenTab(heroTabs, 1); break;
            case "skills": OpenTab(collTabs, 0); break;
            case "pets": OpenTab(collTabs, 1); break;
            case "tech": OpenTab(collTabs, 2); break;
            case "mounts": ToggleMounts(); break;
            case "stones": ToggleStones(); break;
            case "dungeons": OnNav(1); break;
            case "missions": OpenTab(clanTabs, 1); break;
            case "pass": TogglePass(); break;
            case "odds": ShowOdds(); break;
        }
    }

    // Pastille rouge « ! » en haut à droite d'un bouton : quelque chose à faire là-bas.
    GameObject MakeDot(Transform parent)
    {
        var rt = MakeRect("Pastille", parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(4, 4));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Circle(); img.color = new Color(0.9f, 0.12f, 0.1f); img.raycastTarget = false;
        rt.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.7f);
        var t = Label(rt, "!", 24, TextAnchor.MiddleCenter, Color.white);
        t.fontStyle = FontStyle.Bold;
        rt.gameObject.SetActive(false);
        return rt.gameObject;
    }

    static bool AnyDungeonKey()
    {
        for (int i = 0; i < GameState.Data.dungeonKeys.Length; i++) if (GameState.CanEnterDungeon(i)) return true;
        return false;
    }

    void UpdateNav()
    {
        // Notifications façon Forge Master.
        var d = GameState.Data;
        bool gift = !d.shopFreeTaken || SteppingStones.FreeAvailable, dung = AnyDungeonKey(), miss = GameState.CanStartMission;
        bool skills = GameState.Reached(GameState.UnlockSkills) && d.skillTickets >= GameState.SkillSummonCost * SkillData.SummonSmall;
        bool tokens = SteppingStones.Unlocked && SteppingStones.Active && d.ssTokens > 0;
        if (navDot[0] != null)
        {
            navDot[1].SetActive(dung);
            navDot[2].SetActive(skills);
            navDot[3].SetActive(miss);
            navDot[4].SetActive(gift);
        }
        if (heroDot != null) heroDot.SetActive(tokens);
        if (heroTabs != null) heroTabs.dot[1].SetActive(tokens);
        if (collTabs != null) collTabs.dot[0].SetActive(skills);
        if (clanTabs != null) clanTabs.dot[1].SetActive(miss);

        // Le bouton ÉQUIPER pulse quand la nouvelle pièce est meilleure.
        if (popEquipBtn != null)
        {
            float e = popup.activeSelf && popBetter ? 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f) : 0f;
            popEquipBtn.transform.localScale = Vector3.one * (1f + 0.05f * e);
            popEquipBtn.GetComponent<Image>().color = UISkin.Vivid(Color.Lerp(new Color(0.15f, 0.5f, 0.2f), new Color(0.3f, 0.85f, 0.35f), e));
        }

        // Le bouton Améliorer la forge pulse quand on peut payer.
        if (upgradeBtn != null)
        {
            var img = upgradeBtn.GetComponent<Image>();
            bool ready = upgradeBtn.interactable && !d.upgrading && !GameState.IsMaxLevel;
            float k = ready ? 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f) : 0f;
            img.color = UISkin.Vivid(Color.Lerp(new Color(0.62f, 0.22f, 0.06f), new Color(1f, 0.55f, 0.12f), k));
            upgradeBtn.transform.localScale = Vector3.one * (1f + 0.04f * k);
        }

        foreach (var g in new[] { heroTabs, collTabs, clanTabs })
        {
            if (g == null) continue;
            int cur = GroupOpen(g);
            g.root.gameObject.SetActive(cur >= 0);
            for (int i = 0; i < g.img.Length; i++)
            {
                bool locked = !GameState.Reached(g.unlock[i]);
                g.img[i].color = UISkin.Vivid(i == cur ? NavOn : locked ? new Color(0.1f, 0.07f, 0.07f) : NavOff);
                g.txt[i].color = i == cur ? Color.white : locked ? new Color(0.45f, 0.4f, 0.38f) : TextDim;
                g.txt[i].text = locked && g.unlock[i] < Soon && g.names[i] != "" ? g.names[i] + "\n<size=18>étape " + (g.unlock[i] / 10 + 1) + "-" + (g.unlock[i] % 10 + 1) + "</size>" : g.names[i];
                if (locked) g.dot[i].SetActive(false);
            }
        }
        int active = NavActive();
        for (int i = 0; i < 5; i++)
        {
            if (navImg[i] == null) continue;
            int need = NavUnlock(i);
            bool locked = !GameState.Reached(need), on = i == active;
            navImg[i].color = UISkin.Vivid(on ? NavOn : locked ? new Color(0.1f, 0.07f, 0.07f) : NavOff);
            // L'onglet ouvert devient une croix rouge (comme FM) : le retoucher ramène à l'accueil.
            navIcon[i].sprite = UISkin.Get(on ? "i_close" : NavIcons[i]);
            navIcon[i].color = locked ? new Color(0.3f, 0.3f, 0.3f, 0.7f) : Color.white;
            navIcon[i].rectTransform.localScale = Vector3.one * (on ? 1.15f : 1f);
            navText[i].color = on ? Color.white : locked ? new Color(0.45f, 0.4f, 0.38f) : TextDim;
            navText[i].text = locked && need < Soon ? NavNames[i] + "\n<size=18>étape " + (need / 10 + 1) + "-" + (need % 10 + 1) + "</size>" : NavNames[i];
            if (locked) navDot[i].SetActive(false);
        }
    }
}
