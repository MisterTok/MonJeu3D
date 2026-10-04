using UnityEngine;
using UnityEngine.UI;

// Navigation façon Forge Master : 5 onglets en bas (Boutique, Héros, Forge, Aventure, Techno)
// et des sous-onglets en haut des panneaux regroupés (Héros : stats, compétences, compagnons, montures ;
// Aventure : donjons, missions).
public partial class ForgeUI
{
    static readonly string[] NavNames = { "Boutique", "Héros", "FORGE", "Aventure", "Techno" };
    readonly Image[] navImg = new Image[5];
    readonly Text[] navText = new Text[5];
    readonly GameObject[] navDot = new GameObject[5];
    static readonly Color NavOff = new Color(0.2f, 0.09f, 0.08f), NavOn = new Color(0.78f, 0.3f, 0.08f);

    class SubTabs { public RectTransform root; public Image[] img; public Text[] txt; public GameObject[] dot; public GameObject[] panels; public System.Action[] open; public int last; }
    SubTabs heroTabs, advTabs;
    Transform navRoot;

    void BuildNav(Transform R)
    {
        var nav = Box(R, "Navigation", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 150), new Color(0.07f, 0.04f, 0.05f, 0.98f));
        // La forge (accueil) au centre, un peu plus large.
        navRoot = nav.transform;
        float[] edges = { 0f, 0.19f, 0.38f, 0.62f, 0.81f, 1f };
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            var b = MakeButton(nav.transform, "Onglet " + NavNames[i], new Vector2(edges[i], 0), new Vector2(edges[i + 1], 1), new Vector2(4, 12), new Vector2(-4, -12),
                NavOff, NavNames[i], i == 2 ? 32 : 26, out navText[i], () => OnNav(idx));
            navImg[i] = b.GetComponent<Image>();
            navDot[i] = MakeDot(b.transform);
            if (i == 2) navText[i].fontStyle = FontStyle.Bold;
        }
    }

    void OnNav(int idx)
    {
        if (passPanel != null) passPanel.SetActive(false);
        switch (idx)
        {
            case 0: if (shopPanel.activeSelf) CloseAllPanels(); else ToggleShop(); break;
            case 1: OpenGroup(heroTabs); break;
            case 2: CloseAllPanels(); break;
            case 3: OpenGroup(advTabs); break;
            case 4: if (techPanel.activeSelf) CloseAllPanels(); else ToggleTech(); break;
        }
    }

    // Toucher l'onglet d'un groupe déjà ouvert le referme ; sinon rouvre le dernier sous-onglet utilisé.
    void OpenGroup(SubTabs g)
    {
        if (GroupOpen(g) >= 0) { CloseAllPanels(); return; }
        OpenTab(g, g.last);
    }

    void OpenTab(SubTabs g, int i)
    {
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
    SubTabs MakeSubTabs(Transform R, string[] names, GameObject[] panels, System.Action[] open)
    {
        var g = new SubTabs { panels = panels, open = open, img = new Image[names.Length], txt = new Text[names.Length], dot = new GameObject[names.Length] };
        g.root = MakeRect("Sous-onglets", R, new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -216), new Vector2(-114, -140));
        g.root.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.025f, 0.03f, 1f);
        for (int i = 0; i < names.Length; i++)
        {
            int idx = i;
            var b = MakeButton(g.root, names[i], new Vector2(i / (float)names.Length, 0), new Vector2((i + 1) / (float)names.Length, 1), new Vector2(4, 6), new Vector2(-4, -6),
                NavOff, names[i], 26, out g.txt[i], () => OpenTab(g, idx));
            g.img[i] = b.GetComponent<Image>();
            g.dot[i] = MakeDot(b.transform);
        }
        g.root.gameObject.SetActive(false);
        return g;
    }

    void BuildSubTabs(Transform R)
    {
        heroTabs = MakeSubTabs(R, new[] { "Héros", "Compétences", "Compagnons", "Montures" },
            new[] { statsPanel, skillPanel, compPanel, mountPanel },
            new System.Action[] { ToggleStats, ToggleSkills, ToggleCompanions, ToggleMounts });
        advTabs = MakeSubTabs(R, new[] { "Donjons", "Missions" },
            new[] { dungPanel, missionPanel },
            new System.Action[] { ToggleDungeons, ToggleMissions });
        // Barre du bas et sous-onglets au-dessus des panneaux (la fiche de détail reste par-dessus).
        navRoot.SetAsLastSibling();
        heroTabs.root.SetAsLastSibling();
        advTabs.root.SetAsLastSibling();
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
        bool gift = !d.shopFreeTaken, dung = AnyDungeonKey(), miss = GameState.CanStartMission;
        bool skills = d.skillTickets >= GameState.SkillSummonCost * SkillData.SummonSmall;
        if (navDot[0] != null)
        {
            navDot[0].SetActive(gift);
            navDot[1].SetActive(skills);
            navDot[3].SetActive(dung || miss);
        }
        if (heroTabs != null) heroTabs.dot[1].SetActive(skills);
        if (advTabs != null) { advTabs.dot[0].SetActive(dung); advTabs.dot[1].SetActive(miss); }

        // Le bouton Améliorer la forge pulse quand on peut payer.
        if (upgradeBtn != null)
        {
            var img = upgradeBtn.GetComponent<Image>();
            bool ready = upgradeBtn.interactable && !d.upgrading && !GameState.IsMaxLevel;
            float k = ready ? 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f) : 0f;
            img.color = Color.Lerp(new Color(0.62f, 0.22f, 0.06f), new Color(1f, 0.55f, 0.12f), k);
            upgradeBtn.transform.localScale = Vector3.one * (1f + 0.04f * k);
        }

        int hero = GroupOpen(heroTabs), adv = GroupOpen(advTabs);
        bool detailOpen = detail != null && detail.activeSelf;
        foreach (var (g, cur) in new[] { (heroTabs, hero), (advTabs, adv) })
        {
            if (g == null) continue;
            g.root.gameObject.SetActive(cur >= 0);
            for (int i = 0; i < g.img.Length; i++)
            {
                g.img[i].color = i == cur ? NavOn : NavOff;
                g.txt[i].color = i == cur ? Color.white : TextDim;
            }
        }
        int active = shopPanel != null && shopPanel.activeSelf ? 0 : hero >= 0 ? 1 : adv >= 0 ? 3 : techPanel != null && techPanel.activeSelf ? 4 : 2;
        for (int i = 0; i < 5; i++)
        {
            if (navImg[i] == null) continue;
            navImg[i].color = i == active ? NavOn : NavOff;
            navText[i].color = i == active ? Color.white : TextDim;
        }
    }
}
