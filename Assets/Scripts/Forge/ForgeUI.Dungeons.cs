using UnityEngine;
using UnityEngine.UI;

// Écran des donjons, épuré : 4 cartes (pastille, nom, niveau, récompense, clés, bouton).
public partial class ForgeUI
{
    GameObject dungPanel;
    Text dungFooter;
    readonly Text[] dungInfo = new Text[GameState.DungeonCount];
    readonly Text[] dungKeys = new Text[GameState.DungeonCount];
    readonly Button[] dungBtn = new Button[GameState.DungeonCount];
    readonly Text[] dungBtnText = new Text[GameState.DungeonCount];
    float dungTimer;

    static readonly string[] DungeonGlyph = { "⚒", "●", "✚", "✦" };

    void BuildDungeonPanel(Transform R)
    {
        dungPanel = MakeRect("Donjons", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        dungPanel.AddComponent<Image>().color = new Color(0.05f, 0.025f, 0.03f, 1f);
        Transform P = dungPanel.transform;
        Label(P, "DONJONS", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.4f, 0.12f, 0.08f), "X", 40, out closeT, () => dungPanel.SetActive(false));

        for (int i = 0; i < GameState.DungeonCount; i++)
        {
            int type = i;
            float top = -100 - i * 270, bottom = top - 250;
            var c = GameState.DungeonColors[i];
            var frame = Box(P, "Donjon " + i, new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, bottom), new Vector2(-16, top), c);
            frame.sprite = Round(); frame.type = Image.Type.Sliced;
            var card = Box(frame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(5, 5), new Vector2(-5, -5), new Color(c.r * 0.18f + 0.04f, c.g * 0.18f + 0.03f, c.b * 0.18f + 0.03f));
            card.sprite = Round(); card.type = Image.Type.Sliced;
            // Pastille du donjon
            var dot = Box(card.transform, "Pastille", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, -80), new Vector2(184, 80), new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f));
            dot.sprite = Circle();
            Label(dot.transform, DungeonGlyph[i], 76, TextAnchor.MiddleCenter, Color.white);
            Label(card.transform, GameState.DungeonNames[i], 38, TextAnchor.UpperLeft, Color.Lerp(c, Color.white, 0.3f), Vector2.zero, Vector2.one, new Vector2(206, 0), new Vector2(-270, -22));
            dungInfo[i] = Label(card.transform, "", 28, TextAnchor.LowerLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(206, 26), new Vector2(-270, 0));
            dungKeys[i] = Label(card.transform, "", 26, TextAnchor.UpperCenter, new Color(1f, 0.85f, 0.4f), new Vector2(1, 0), new Vector2(1, 1), new Vector2(-250, 0), new Vector2(-20, -22));
            Text bt;
            dungBtn[i] = MakeButton(card.transform, "Entrer", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-250, 26), new Vector2(-20, 136),
                new Color(0.15f, 0.45f, 0.7f), "ENTRER", 36, out bt, () => OnEnterDungeon(type));
            dungBtnText[i] = bt;
        }
        dungFooter = Label(P, "", 26, TextAnchor.UpperCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -1220), new Vector2(-20, -1180));
        dungPanel.SetActive(false);
        battle.DungeonEnded += Toast;
    }

    void ToggleDungeons()
    {
        bool open = !dungPanel.activeSelf;
        CloseAllPanels();
        dungPanel.SetActive(open);
        RefreshDungeons();
    }

    void OnEnterDungeon(int type)
    {
        if (battle.InDungeon) { Toast("Termine d'abord le combat en cours"); return; }
        if (!battle.StartDungeon(type)) { Toast("Plus de clé aujourd'hui"); return; }
        dungPanel.SetActive(false);
        Toast(GameState.DungeonNames[type]);
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
            dungInfo[i].text = "Niv. " + (lvl + 1) + "\n<color=#FFD27A>" + GameState.DungeonRewardText(i, lvl) + "</color>";
            dungKeys[i].text = "Clés  <b>" + d.dungeonKeys[i] + "/" + GameState.DungeonKeysPerDay + "</b>";
            dungBtn[i].interactable = GameState.CanEnterDungeon(i) && !battle.InDungeon;
            dungBtnText[i].text = d.dungeonKeys[i] > 0 ? "ENTRER" : "DEMAIN";
        }
        dungFooter.text = "Nouvelles clés dans " + GameState.FmtTime((long)GameState.TimeToKeyRefresh().TotalSeconds);
    }
}
