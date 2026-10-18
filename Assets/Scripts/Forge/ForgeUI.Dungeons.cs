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

    static readonly string[] DungeonIcon = { "i_hammer", "i_shell", "i_potion", "i_ticket" };
    static readonly string[] DungeonKeyIcon = { "i_key_hammer", "i_key_egg", "i_key_potion", "i_key_skill" };
    readonly RectTransform[] dungReward = new RectTransform[GameState.DungeonCount];
    readonly int[] dungShownLevel = { -1, -1, -1, -1 };

    void BuildDungeonPanel(Transform R)
    {
        dungPanel = MakeRect("Donjons", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(dungPanel);
        Transform P = dungPanel.transform;
        Label(P, "DONJONS", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.4f, 0.12f, 0.08f), "X", 40, out closeT, () => dungPanel.SetActive(false));

        for (int i = 0; i < GameState.DungeonCount; i++)
        {
            int type = i;
            float top = -100 - i * 335, bottom = top - 315;
            var c = GameState.DungeonColors[i];
            var frame = Box(P, "Donjon " + i, new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, bottom), new Vector2(-16, top), Color.Lerp(c, Color.white, 0.2f));
            UISkin.Sliced(frame, "frame", 2.4f);
            var card = Box(frame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(7, 7), new Vector2(-7, -7), SkinTint(new Color(c.r * 0.16f + 0.05f, c.g * 0.16f + 0.03f, c.b * 0.16f + 0.03f)));
            UISkin.Sliced(card, "tile", 1.8f);
            // Emblème du donjon sur un halo de sa couleur
            var glow = Box(card.transform, "Halo", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, -130), new Vector2(240, 110), new Color(c.r, c.g, c.b, 0.75f));
            glow.sprite = UISkin.Get("glow"); glow.raycastTarget = false;
            UISkin.Icon(card.transform, DungeonIcon[i], new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(40, -90), new Vector2(200, 70));
            Label(card.transform, GameState.DungeonNames[i], 40, TextAnchor.UpperLeft, Color.Lerp(c, Color.white, 0.35f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(236, -76), new Vector2(-200, -18));
            dungInfo[i] = Label(card.transform, "", 26, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(236, -116), new Vector2(-200, -80));
            dungReward[i] = MakeRect("Récompense", card.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(230, 18), new Vector2(230 + 2 * 140, 168));
            // Clés : icône + compte
            var keys = Box(card.transform, "Clés", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-196, -84), new Vector2(-20, -24), Color.white);
            UISkin.Sliced(keys, "pill"); keys.raycastTarget = false;
            UISkin.Icon(keys.transform, DungeonKeyIcon[i], new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-8, -32), new Vector2(56, 32));
            dungKeys[i] = Label(keys.transform, "", 30, TextAnchor.MiddleRight, new Color(1f, 0.88f, 0.5f), Vector2.zero, Vector2.one, new Vector2(56, 0), new Vector2(-18, 0));
            Text bt;
            dungBtn[i] = MakeButton(card.transform, "Entrer", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-260, 22), new Vector2(-20, 132),
                new Color(0.15f, 0.45f, 0.7f), "ENTRER", 36, out bt, () => OnEnterDungeon(type));
            dungBtnText[i] = bt;
        }
        dungFooter = Label(P, "", 26, TextAnchor.UpperCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -1490), new Vector2(-20, -1440));
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
            dungInfo[i].text = "Niv. " + (lvl + 1);
            dungKeys[i].text = d.dungeonKeys[i] + "/" + GameState.DungeonKeysPerDay;
            if (dungShownLevel[i] != lvl)
            {
                dungShownLevel[i] = lvl;
                for (int k = dungReward[i].childCount - 1; k >= 0; k--) Destroy(dungReward[i].GetChild(k).gameObject);
                var r = GameState.DungeonReward(i, lvl);
                string[] ic = i == GameState.DungeonHammer ? new[] { "i_hammer", "i_gold" } : new[] { DungeonIcon[i] };
                for (int k = 0; k < ic.Length; k++)
                    RewardCell(dungReward[i], ic[k], "+" + GameState.Fmt(r[k]), new Vector2(k * 0.5f, 0), new Vector2(k * 0.5f + 0.5f, 1), 10, 28);
            }
            dungBtn[i].interactable = GameState.CanEnterDungeon(i) && !battle.InDungeon;
            dungBtnText[i].text = !GameState.Reached(GameState.UnlockDungeon[i]) ? "<size=24>Étape\n" + (GameState.UnlockDungeon[i] / 10 + 1) + "-" + (GameState.UnlockDungeon[i] % 10 + 1) + "</size>"
                : d.dungeonKeys[i] > 0 ? "ENTRER" : "DEMAIN";
        }
        dungFooter.text = GameState.FmtTime((long)GameState.TimeToKeyRefresh().TotalSeconds);
    }
}
