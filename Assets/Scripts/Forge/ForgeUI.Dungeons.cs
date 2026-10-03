using UnityEngine;
using UnityEngine.UI;

// Écran des donjons : 4 donjons, 2 clés par jour, récompenses en monnaies.
public partial class ForgeUI
{
    GameObject dungPanel;
    Text dungFooter;
    readonly Text[] dungInfo = new Text[GameState.DungeonCount];
    readonly Text[] dungKeys = new Text[GameState.DungeonCount];
    readonly Button[] dungBtn = new Button[GameState.DungeonCount];
    readonly Text[] dungBtnText = new Text[GameState.DungeonCount];
    float dungTimer;

    void BuildDungeonPanel(Transform R)
    {
        dungPanel = MakeRect("Donjons", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        dungPanel.AddComponent<Image>().color = new Color(0.05f, 0.025f, 0.03f, 0.98f);
        Transform P = dungPanel.transform;
        Label(P, "DONJONS", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.4f, 0.12f, 0.08f), "X", 40, out closeT, () => dungPanel.SetActive(false));
        Label(P, "Les clés se rechargent chaque jour à 22:00. Elles ne sont consommées que si tu termines le donjon.", 24,
            TextAnchor.UpperCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -150), new Vector2(-30, -90));

        for (int i = 0; i < GameState.DungeonCount; i++)
        {
            int type = i;
            float top = -160 - i * 330, bottom = top - 310;
            var card = Box(P, "Donjon " + i, new Vector2(0, 1), new Vector2(1, 1), new Vector2(18, bottom), new Vector2(-18, top), new Color(0.11f, 0.05f, 0.05f, 1f));
            var c = GameState.DungeonColors[i];
            card.gameObject.AddComponent<Outline>().effectColor = c;
            Box(card.transform, "Bande", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(16, 0), c);
            Label(card.transform, GameState.DungeonNames[i], 42, TextAnchor.UpperLeft, c, Vector2.zero, Vector2.one, new Vector2(40, 0), new Vector2(-20, -16));
            Label(card.transform, GameState.DungeonDesc[i], 24, TextAnchor.UpperLeft, TextDim, Vector2.zero, Vector2.one, new Vector2(40, 0), new Vector2(-300, -72));
            dungInfo[i] = Label(card.transform, "", 28, TextAnchor.LowerLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(40, 22), new Vector2(-300, 0));
            dungKeys[i] = Label(card.transform, "", 34, TextAnchor.UpperRight, new Color(1f, 0.85f, 0.4f), Vector2.zero, Vector2.one, new Vector2(0, 0), new Vector2(-24, -18));
            Text bt;
            dungBtn[i] = MakeButton(card.transform, "Entrer", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-270, 24), new Vector2(-24, 130),
                new Color(0.15f, 0.45f, 0.7f), "ENTRER", 38, out bt, () => OnEnterDungeon(type));
            dungBtnText[i] = bt;
        }
        dungFooter = Label(P, "", 26, TextAnchor.LowerCenter, TextMain, new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 20), new Vector2(-20, 110));
        dungPanel.SetActive(false);
        battle.DungeonEnded += Toast;
    }

    void ToggleDungeons()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (mountPanel != null) mountPanel.SetActive(false);
        if (techPanel != null) techPanel.SetActive(false);
        if (compPanel != null) compPanel.SetActive(false);
        dungPanel.SetActive(!dungPanel.activeSelf);
        if (dungPanel.activeSelf) RefreshDungeons();
    }

    void OnEnterDungeon(int type)
    {
        if (battle.InDungeon) { Toast("Termine d'abord le donjon en cours"); return; }
        if (!battle.StartDungeon(type)) { Toast("Plus de clé aujourd'hui pour ce donjon"); return; }
        dungPanel.SetActive(false);
        Toast("Entrée dans : " + GameState.DungeonNames[type]);
    }

    void UpdateDungeons()
    {
        if (dungPanel == null || !dungPanel.activeSelf) return;
        dungTimer -= Time.deltaTime;
        if (dungTimer > 0f) return;
        dungTimer = 1f;
        RefreshDungeons();
    }

    void RefreshDungeons()
    {
        if (dungPanel == null || !dungPanel.activeSelf) return;
        var d = GameState.Data;
        for (int i = 0; i < GameState.DungeonCount; i++)
        {
            int lvl = d.dungeonLevel[i];
            dungInfo[i].text = "Niveau " + (lvl + 1) + "\n<color=#FFD27A>" + GameState.DungeonRewardText(i, lvl) + "</color>";
            dungKeys[i].text = "Clés " + d.dungeonKeys[i] + "/" + GameState.DungeonKeysPerDay;
            bool can = GameState.CanEnterDungeon(i) && !battle.InDungeon;
            dungBtn[i].interactable = can;
            dungBtnText[i].text = d.dungeonKeys[i] > 0 ? "ENTRER" : "DEMAIN";
        }
        var t = GameState.TimeToKeyRefresh();
        dungFooter.text = "Recharge des clés dans " + GameState.FmtTime((long)t.TotalSeconds)
            + "\n<color=#BFD8FF>Coquilles " + GameState.Fmt(d.eggshells) + "  ·  Potions rouges " + GameState.Fmt(d.potions)
            + "  ·  Tickets " + GameState.Fmt(d.skillTickets) + "</color>";
    }
}
