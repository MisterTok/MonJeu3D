using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Écran de l'arbre technologique : 3 arbres (Forge, Puissance, Compagnons & Tech), recherches payées en potions rouges.
public partial class ForgeUI
{
    GameObject techPanel;
    Text techHeader, techFinishText;
    Button techFinishBtn;
    ScrollRect techScroll;
    readonly RectTransform[] techContent = new RectTransform[3];
    readonly Image[] techTabImg = new Image[3];
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
        Label(P, "TECHNOLOGIE", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -76), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => techPanel.SetActive(false));

        techHeader = Label(P, "", 28, TextAnchor.MiddleLeft, TextMain, new Vector2(0, 1), new Vector2(0.72f, 1), new Vector2(24, -160), new Vector2(0, -88));
        techFinishBtn = MakeButton(P, "Finir recherche", new Vector2(0.72f, 1), new Vector2(1, 1), new Vector2(0, -156), new Vector2(-20, -92),
            new Color(0.15f, 0.45f, 0.65f), "", 24, out techFinishText, () => { if (!GameState.SpeedUpResearch()) Toast("Pas assez de gemmes"); });

        string[] tabs = { "Forge", "Puissance", "Compagnons" };
        for (int i = 0; i < 3; i++)
        {
            int t = i;
            Text tt;
            var b = MakeButton(P, "Onglet " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1), new Vector2(8, -240), new Vector2(-8, -172),
                new Color(0.3f, 0.12f, 0.08f), tabs[i], 28, out tt, () => SelectTechTree(t));
            techTabImg[i] = b.GetComponent<Image>();
        }

        // Zone défilante (toute la hauteur : la fiche d'une recherche s'ouvre au toucher)
        var view = MakeRect("Vue", P, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -250));
        view.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.03f, 0.9f);
        view.gameObject.AddComponent<RectMask2D>();
        techScroll = view.gameObject.AddComponent<ScrollRect>();
        techScroll.horizontal = false;
        techScroll.viewport = view;
        techScroll.movementType = ScrollRect.MovementType.Clamped;
        techScroll.scrollSensitivity = 60f;
        for (int tr = 0; tr < 3; tr++) BuildTree(view, tr);

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
                    new Vector2(8, y - 112), new Vector2(-8, y), Panel, "", 24, out t, () => SelectNode(id));
                b.GetComponent<Image>().sprite = Round(); b.GetComponent<Image>().type = Image.Type.Sliced;
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
        bool open = !techPanel.activeSelf;
        CloseAllPanels();
        techPanel.SetActive(open);
        if (open) SelectTechTree(techTree);
    }

    void SelectTechTree(int t)
    {
        techTree = t;
        selNode = -1;
        for (int i = 0; i < 3; i++) techContent[i].gameObject.SetActive(i == t);
        techScroll.content = techContent[t];
        techContent[t].anchoredPosition = Vector2.zero;
        RefreshTech();
    }

    // Fiche d'une recherche : effet actuel / suivant, coût, bouton.
    void SelectNode(int id)
    {
        selNode = id;
        RefreshTech();
        int tr = techTree;
        var d = GameState.Data;
        int st = GameState.NodeType(tr, id);
        int sl = GameState.TechLevel(tr, id), sm = GameState.NodeMax(tr, id);
        float per = TechData.Value[st];
        string body = "<size=26><color=#A89C94>Palier " + Roman[GameState.NodeTier(tr, id)] + " · niv. " + sl + "/" + sm + "</color></size>\n\n"
            + (sl > 0 ? TechText.Describe(st, per * sl) : "<color=#998877>Pas encore recherché</color>");
        if (sl < sm) body += "\n<color=#9FE0A0>▶ " + TechText.Describe(st, per * (sl + 1)) + "</color>";
        string action; bool can;
        if (sl >= sm) { action = "MAXIMUM"; can = false; }
        else if (!GameState.NodeUnlocked(tr, id)) { action = "VERROUILLÉ"; can = false; body += "\n\n<size=24><color=#998877>Recherche d'abord les précédentes</color></size>"; }
        else
        {
            long cost = GameState.ResearchCost(tr, id);
            body += "\n\n<color=#FF8A7A>" + GameState.Fmt(cost) + " potions</color>  ·  " + GameState.FmtTime(GameState.ResearchSeconds(tr, id));
            bool busy = d.researchTree >= 0;
            can = !busy && d.potions >= cost;
            action = busy ? "1 À LA FOIS" : d.potions >= cost ? "RECHERCHER" : "PAS ASSEZ";
        }
        ShowAction(TechText.Name(st), body, action, can, () => { string e = GameState.StartResearch(tr, id); if (e != null) Toast(e); RefreshTech(); });
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
        for (int i = 0; i < 3; i++)
            techTabImg[i].color = i == techTree ? new Color(0.75f, 0.32f, 0.1f) : new Color(0.3f, 0.12f, 0.08f);

        string res;
        if (d.researchTree >= 0)
        {
            int type = GameState.NodeType(d.researchTree, d.researchNode);
            res = "<b>" + TechText.Name(type) + "</b>  " + GameState.FmtTime(GameState.ResearchSecondsLeft());
            techFinishBtn.gameObject.SetActive(true);
            techFinishText.text = "Finir\n<size=20>" + GameState.ResearchSpeedUpCost() + " gemmes</size>";
        }
        else
        {
            res = "<color=#998877>Aucune recherche</color>";
            techFinishBtn.gameObject.SetActive(false);
        }
        techHeader.text = "<color=#FF8A7A>Potions  <b>" + GameState.Fmt(d.potions) + "</b></color>\n<size=24>" + res + "</size>";

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
            nodeText[tr][id].text = col + TechText.Name(type) + "\n<b><size=26>" + lvl + "/" + max + "</size></b>" + (researching ? "  <color=#7FC8FF>●</color>" : "") + (unlocked ? "" : "</color>");
        }

    }
}
