using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Écran de l'arbre technologique : 3 arbres (Forge, Puissance, Compagnons & Tech), recherches payées en potions rouges.
public partial class ForgeUI
{
    GameObject techPanel;
    Text techHeader, detailTitle, detailDesc, detailCost, detailBtnText, techFinishText;
    Button detailBtn, techFinishBtn;
    ScrollRect techScroll;
    readonly RectTransform[] techContent = new RectTransform[3];
    readonly Image[] techTabImg = new Image[4];
    readonly List<Button>[] nodeBtn = { new List<Button>(), new List<Button>(), new List<Button>() };
    readonly List<Text>[] nodeText = { new List<Text>(), new List<Text>(), new List<Text>() };
    int techTree, selNode = -1;
    float techTimer;
    static readonly string[] Roman = { "I", "II", "III", "IV", "V" };

    void BuildTechPanel(Transform R)
    {
        techPanel = MakeRect("Arbre technologique", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        techPanel.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 1f);
        Transform P = techPanel.transform;
        Label(P, "ARBRE TECHNOLOGIQUE", 44, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -76), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => techPanel.SetActive(false));

        techHeader = Label(P, "", 28, TextAnchor.MiddleLeft, TextMain, new Vector2(0, 1), new Vector2(0.72f, 1), new Vector2(24, -160), new Vector2(0, -88));
        techFinishBtn = MakeButton(P, "Finir recherche", new Vector2(0.72f, 1), new Vector2(1, 1), new Vector2(0, -156), new Vector2(-20, -92),
            new Color(0.15f, 0.45f, 0.65f), "", 24, out techFinishText, () => { if (!GameState.SpeedUpResearch()) Toast("Pas assez de gemmes"); });

        string[] tabs = { "Forge", "Puissance", "Compagnons\n& Tech", "Clan" };
        for (int i = 0; i < 4; i++)
        {
            int t = i;
            Text tt;
            var b = MakeButton(P, "Onglet " + i, new Vector2(i / 4f, 1), new Vector2((i + 1) / 4f, 1), new Vector2(8, -240), new Vector2(-8, -168),
                new Color(0.3f, 0.12f, 0.08f), tabs[i], 26, out tt, () => SelectTechTree(t));
            techTabImg[i] = b.GetComponent<Image>();
        }

        // Zone défilante
        var view = MakeRect("Vue", P, Vector2.zero, Vector2.one, new Vector2(10, 300), new Vector2(-10, -250));
        view.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.03f, 0.9f);
        view.gameObject.AddComponent<RectMask2D>();
        techScroll = view.gameObject.AddComponent<ScrollRect>();
        techScroll.horizontal = false;
        techScroll.viewport = view;
        techScroll.movementType = ScrollRect.MovementType.Clamped;
        techScroll.scrollSensitivity = 60f;
        for (int tr = 0; tr < 3; tr++) BuildTree(view, tr);

        // Détail de la recherche sélectionnée
        var det = Box(P, "Détail", new Vector2(0, 0), new Vector2(1, 0), new Vector2(10, 10), new Vector2(-10, 290), Panel);
        det.gameObject.AddComponent<Outline>().effectColor = Ember;
        detailTitle = Label(det.transform, "Touche une recherche", 34, TextAnchor.UpperLeft, Ember, Vector2.zero, Vector2.one, new Vector2(24, 0), new Vector2(-24, -14));
        detailDesc = Label(det.transform, "", 26, TextAnchor.UpperLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(24, 0), new Vector2(-330, -64));
        detailCost = Label(det.transform, "", 26, TextAnchor.LowerLeft, new Color(1f, 0.6f, 0.55f), Vector2.zero, Vector2.one, new Vector2(24, 18), new Vector2(-330, 0));
        detailBtn = MakeButton(det.transform, "Rechercher", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-300, 24), new Vector2(-24, 150),
            new Color(0.2f, 0.5f, 0.25f), "RECHERCHER", 30, out detailBtnText, OnResearch);

        techPanel.SetActive(false);
        GameState.TechDone += Toast;
    }

    void BuildTree(RectTransform view, int tr)
    {
        var content = (RectTransform)new GameObject("Arbre " + tr, typeof(RectTransform)).transform;
        content.SetParent(view, false);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
        techContent[tr] = content;

        var nodes = TechData.Trees[tr];
        var byLayer = new SortedDictionary<int, List<int>>();
        for (int id = 0; id < nodes.Length; id++)
        {
            int layer = nodes[id][1];
            if (!byLayer.ContainsKey(layer)) byLayer[layer] = new List<int>();
            byLayer[layer].Add(id);
        }
        for (int id = 0; id < nodes.Length; id++) { nodeBtn[tr].Add(null); nodeText[tr].Add(null); }

        float y = -10f;
        int lastTier = -1;
        foreach (var kv in byLayer)
        {
            int tier = nodes[kv.Value[0]][0];
            if (tier != lastTier)
            {
                lastTier = tier;
                Label(content, "— Palier " + Roman[Mathf.Clamp(tier, 0, 4)] + " —", 30, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.5f),
                    new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, y - 56), new Vector2(0, y));
                y -= 60f;
            }
            int k = kv.Value.Count;
            for (int j = 0; j < k; j++)
            {
                int id = kv.Value[j];
                Text t;
                var b = MakeButton(content, "Nœud " + id, new Vector2((float)j / k, 1), new Vector2((float)(j + 1) / k, 1),
                    new Vector2(10, y - 118), new Vector2(-10, y), Panel, "", 28, out t, () => SelectNode(id));
                nodeBtn[tr][id] = b;
                nodeText[tr][id] = t;
            }
            y -= 128f;
        }
        content.sizeDelta = new Vector2(0, -y + 10f);
        content.gameObject.SetActive(false);
    }

    void ToggleTech()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (mountPanel != null) mountPanel.SetActive(false);
        if (compPanel != null) compPanel.SetActive(false);
        if (dungPanel != null) dungPanel.SetActive(false);
        techPanel.SetActive(!techPanel.activeSelf);
        if (techPanel.activeSelf) SelectTechTree(techTree);
    }

    void SelectTechTree(int t)
    {
        if (t == 3) { Toast("Branche du clan : avec la guerre de clans (bientôt)"); return; }
        techTree = t;
        selNode = -1;
        for (int i = 0; i < 3; i++) techContent[i].gameObject.SetActive(i == t);
        techScroll.content = techContent[t];
        techContent[t].anchoredPosition = Vector2.zero;
        RefreshTech();
    }

    void SelectNode(int id)
    {
        selNode = id;
        RefreshTech();
    }

    void OnResearch()
    {
        if (selNode < 0) { Toast("Choisis d'abord une recherche"); return; }
        string err = GameState.StartResearch(techTree, selNode);
        if (err != null) Toast(err);
        RefreshTech();
    }

    void UpdateTech()
    {
        if (techPanel == null || !techPanel.activeSelf) return;
        techTimer -= Time.deltaTime;
        if (techTimer > 0f) return;
        techTimer = 0.5f;
        RefreshTech();
    }

    void RefreshTech()
    {
        if (techPanel == null || !techPanel.activeSelf) return;
        var d = GameState.Data;
        for (int i = 0; i < 4; i++)
            techTabImg[i].color = i == techTree ? new Color(0.75f, 0.32f, 0.1f) : i == 3 ? new Color(0.18f, 0.12f, 0.12f) : new Color(0.3f, 0.12f, 0.08f);

        string res;
        if (d.researchTree >= 0)
        {
            int type = GameState.NodeType(d.researchTree, d.researchNode);
            res = "En cours : <b>" + TechText.Name(type) + "</b> (" + GameState.TreeNames[d.researchTree] + ") — " + GameState.FmtTime(GameState.ResearchSecondsLeft());
            techFinishBtn.gameObject.SetActive(true);
            techFinishText.text = "Finir\n<size=20>" + GameState.ResearchSpeedUpCost() + " gemmes</size>";
        }
        else
        {
            res = "Aucune recherche en cours";
            techFinishBtn.gameObject.SetActive(false);
        }
        techHeader.text = "<color=#FF8A7A>Potions rouges " + GameState.Fmt(d.potions) + "</color>\n<size=24>" + res + "</size>";

        int tr = techTree;
        for (int id = 0; id < nodeBtn[tr].Count; id++)
        {
            int type = GameState.NodeType(tr, id);
            int lvl = GameState.TechLevel(tr, id), max = GameState.NodeMax(tr, id);
            bool unlocked = GameState.NodeUnlocked(tr, id);
            bool researching = GameState.IsResearching(tr, id);
            var img = nodeBtn[tr][id].GetComponent<Image>();
            var ol = nodeBtn[tr][id].GetComponent<Outline>();
            Color bg, border;
            if (researching) { bg = new Color(0.12f, 0.32f, 0.52f); border = new Color(0.5f, 0.8f, 1f); }
            else if (lvl >= max) { bg = new Color(0.45f, 0.34f, 0.1f); border = new Color(1f, 0.85f, 0.35f); }
            else if (!unlocked) { bg = new Color(0.12f, 0.1f, 0.1f); border = new Color(0.25f, 0.2f, 0.2f); }
            else { bg = new Color(0.38f, 0.17f, 0.08f); border = Ember; }
            if (id == selNode) border = Color.white;
            img.color = bg;
            ol.effectColor = border;
            ol.effectDistance = id == selNode ? new Vector2(5, -5) : new Vector2(3, -3);
            string col = unlocked ? "" : "<color=#7A6A66>";
            nodeText[tr][id].text = col + "<b>" + TechText.Name(type) + "</b>\nniv. " + lvl + "/" + max + (researching ? " · en cours" : "") + (unlocked ? "" : "</color>");
        }

        if (selNode < 0)
        {
            detailTitle.text = "Touche une recherche";
            detailDesc.text = "Les recherches améliorent durablement ta forge, ton héros et tes compagnons. Une seule recherche à la fois.";
            detailCost.text = "";
            detailBtn.gameObject.SetActive(false);
            return;
        }
        int st = GameState.NodeType(tr, selNode);
        int sl = GameState.TechLevel(tr, selNode), sm = GameState.NodeMax(tr, selNode);
        float per = TechData.Value[st];
        detailTitle.text = TechText.Name(st) + "  <size=26>(palier " + Roman[GameState.NodeTier(tr, selNode)] + ", niv. " + sl + "/" + sm + ")</size>";
        string now = sl > 0 ? "Actuel : " + TechText.Describe(st, per * sl) : "Pas encore recherché";
        string next = sl < sm ? "\nNiveau suivant : " + TechText.Describe(st, per * (sl + 1)) : "\nNiveau maximum atteint";
        detailDesc.text = now + next;
        detailBtn.gameObject.SetActive(true);
        if (sl >= sm) { detailCost.text = ""; detailBtn.interactable = false; detailBtnText.text = "MAXIMUM"; }
        else if (!GameState.NodeUnlocked(tr, selNode)) { detailCost.text = "Verrouillée : recherche d'abord les nœuds précédents"; detailBtn.interactable = false; detailBtnText.text = "VERROUILLÉ"; }
        else
        {
            long cost = GameState.ResearchCost(tr, selNode);
            detailCost.text = "Coût : " + GameState.Fmt(cost) + " potions rouges · " + GameState.FmtTime(GameState.ResearchSeconds(tr, selNode));
            bool busy = d.researchTree >= 0;
            detailBtn.interactable = !busy && d.potions >= cost;
            detailBtnText.text = busy ? "1 À LA FOIS" : d.potions >= cost ? "RECHERCHER" : "POTIONS ?";
        }
    }
}
