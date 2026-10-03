using UnityEngine;
using UnityEngine.UI;

// Missions (escouades à affronter, 3 énergies par jour) et pass de progression (paliers du chemin).
public partial class ForgeUI
{
    GameObject missionPanel, passPanel;
    Text missionEnergy, missionFooter;
    readonly Text[] missionTitle = new Text[MissionData.OfferCount];
    readonly Text[] missionInfo = new Text[MissionData.OfferCount];
    readonly Button[] missionBtn = new Button[MissionData.OfferCount];
    readonly Text[] missionBtnText = new Text[MissionData.OfferCount];
    Button refreshBtn;
    Text refreshText;
    float missionTimer;

    Button passBtn;
    Text passBtnText;
    readonly Image[] passRowBg = new Image[MissionData.PassStage.Length];
    readonly Text[] passRowText = new Text[MissionData.PassStage.Length];
    readonly Button[] passRowBtn = new Button[MissionData.PassStage.Length];
    readonly Text[] passRowBtnText = new Text[MissionData.PassStage.Length];
    Text passHeader;

    static readonly Color MissionPurple = new Color(0.75f, 0.45f, 1f);

    void BuildMissionPanel(Transform R)
    {
        missionPanel = MakeRect("Missions", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        missionPanel.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.07f, 1f);
        Transform P = missionPanel.transform;
        Label(P, "MISSIONS", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => missionPanel.SetActive(false));
        missionEnergy = Label(P, "", 32, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.4f), new Vector2(0, 1), new Vector2(0.6f, 1), new Vector2(30, -170), new Vector2(0, -96));
        refreshBtn = MakeButton(P, "Nouvelle liste", new Vector2(0.6f, 1), new Vector2(1, 1), new Vector2(0, -168), new Vector2(-20, -98),
            new Color(0.35f, 0.2f, 0.5f), "", 24, out refreshText, () => { string e = GameState.RefreshMissionList(); if (e != null) Toast(e); RefreshMissions(); });

        for (int i = 0; i < MissionData.OfferCount; i++)
        {
            int slot = i;
            float top = -190 - i * 205, bottom = top - 190;
            var frame = Box(P, "Mission " + i, new Vector2(0, 1), new Vector2(1, 1), new Vector2(18, bottom), new Vector2(-18, top), MissionPurple);
            frame.sprite = Round(); frame.type = Image.Type.Sliced;
            var card = Box(frame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(5, 5), new Vector2(-5, -5), new Color(0.13f, 0.07f, 0.15f, 1f));
            card.sprite = Round(); card.type = Image.Type.Sliced;
            var cardBtn = frame.gameObject.AddComponent<Button>();
            cardBtn.onClick.AddListener(() => OpenMission(slot));
            missionTitle[i] = Label(card.transform, "", 32, TextAnchor.UpperLeft, Color.Lerp(MissionPurple, Color.white, 0.35f), Vector2.zero, Vector2.one, new Vector2(28, 0), new Vector2(-250, -20));
            missionInfo[i] = Label(card.transform, "", 28, TextAnchor.LowerLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(28, 22), new Vector2(-250, 0));
            missionBtn[i] = MakeButton(card.transform, "Lancer", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-230, 24), new Vector2(-20, -24),
                new Color(0.45f, 0.22f, 0.65f), "LANCER", 34, out missionBtnText[i], () => OnStartMission(slot));
        }
        missionFooter = Label(P, "", 26, TextAnchor.UpperCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -1240), new Vector2(-20, -1200));
        missionPanel.SetActive(false);
        GameState.MissionMessage += Toast;
    }

    void ToggleMissions()
    {
        bool open = !missionPanel.activeSelf;
        CloseAllPanels();
        missionPanel.SetActive(open);
        RefreshMissions();
    }

    void OnStartMission(int slot)
    {
        if (battle.InDungeon) { Toast("Termine d'abord le combat en cours"); return; }
        if (!battle.StartMission(slot)) { Toast("Plus d'énergie aujourd'hui"); return; }
        missionPanel.SetActive(false);
        Toast("Mission : " + MissionData.SquadNames[GameState.Data.missionSquad[slot]]);
    }

    // Difficulté estimée : combien de temps le héros tient face à l'escouade, comparé au temps pour la vaincre.
    static string MissionDifficulty(int slot)
    {
        double heroDps = GameState.TotalAtk() / (double)GameState.AttackInterval * (1 + GameState.CritChance * (GameState.CritMult - 1));
        double kill = GameState.MissionUnits(slot) * GameState.MissionUnitHp(slot) / System.Math.Max(1, heroDps);
        double survive = GameState.TotalHp() / System.Math.Max(1, GameState.MissionUnitAtk(slot) / 1.3);
        double r = survive / System.Math.Max(0.01, kill);
        return r > 3 ? "<color=#7CFF8A>Facile</color>" : r > 1.2 ? "<color=#FFD24A>Moyen</color>" : "<color=#FF6A5A>Difficile</color>";
    }

    // Fiche d'une mission : ennemis et récompenses.
    void OpenMission(int slot)
    {
        var d = GameState.Data;
        int units = GameState.MissionUnits(slot);
        string body = units + " ennemi" + (units > 1 ? "s" : "") + "  ·  " + MissionDifficulty(slot)
            + "\n<size=26><color=#A89C94>Chacun : PV " + GameState.Fmt(GameState.MissionUnitHp(slot)) + "  ATQ " + GameState.Fmt(GameState.MissionUnitAtk(slot)) + "</color></size>"
            + "\n\n" + GameState.ContentText(GameState.MissionReward(d.missionLevel[slot]), "\n");
        bool can = GameState.CanStartMission && !battle.InDungeon;
        ShowAction(MissionData.SquadNames[d.missionSquad[slot]] + "  niv. " + d.missionLevel[slot], body, d.missionEnergy > 0 ? "LANCER" : "DEMAIN", can,
            () => OnStartMission(slot), MissionPurple);
    }

    void UpdateMissions()
    {
        if (missionPanel == null || !missionPanel.activeSelf) return;
        missionTimer -= Time.deltaTime;
        if (missionTimer > 0f) return;
        missionTimer = 1f;
        RefreshMissions();
    }

    void RefreshMissions()
    {
        RefreshPassButton();
        if (passPanel != null && passPanel.activeSelf) RefreshPass();
        if (missionPanel == null || !missionPanel.activeSelf) return;
        var d = GameState.Data;
        missionEnergy.text = "Énergie  <b>" + d.missionEnergy + "/" + MissionData.DailyEnergy + "</b>";
        refreshText.text = "Nouvelle liste\n<size=20>" + MissionData.RefreshGemCost + " gemmes</size>";
        refreshBtn.interactable = d.gems >= MissionData.RefreshGemCost;
        bool can = GameState.CanStartMission && !battle.InDungeon;
        for (int i = 0; i < MissionData.OfferCount; i++)
        {
            int squad = d.missionSquad[i], lvl = d.missionLevel[i];
            int units = GameState.MissionUnits(i);
            missionTitle[i].text = MissionData.SquadNames[squad] + "  <size=26><color=#FFD27A>niv. " + lvl + "</color></size>";
            missionInfo[i].text = units + " ennemi" + (units > 1 ? "s" : "") + "  ·  " + MissionDifficulty(i);
            missionBtn[i].interactable = can;
            missionBtnText[i].text = d.missionEnergy > 0 ? "LANCER" : "DEMAIN";
        }
        var t = GameState.TimeToKeyRefresh();
        missionFooter.text = "Énergie et nouvelle liste dans " + GameState.FmtTime((long)t.TotalSeconds);
    }

    // ---------- Pass de progression ----------
    void BuildPassButton(Transform R)
    {
        passBtn = MakeButton(R, "Pass", new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, -232), new Vector2(190, -142),
            new Color(0.55f, 0.35f, 0.08f), "", 26, out passBtnText, TogglePass);
        passBtn.GetComponent<Outline>().effectColor = new Color(1f, 0.8f, 0.3f);
    }

    void RefreshPassButton()
    {
        if (passBtn == null) return;
        int n = GameState.PassReadyCount();
        passBtnText.text = "Pass ★" + (n > 0 ? "\n<color=#7CFF8A><size=22>" + n + " à prendre</size></color>" : "");
        passBtn.GetComponent<Image>().color = n > 0 ? new Color(0.25f, 0.5f, 0.15f) : new Color(0.45f, 0.28f, 0.07f, 0.9f);
    }

    void BuildPassPanel(Transform R)
    {
        passPanel = MakeRect("Pass de progression", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        passPanel.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.02f, 1f);
        Transform P = passPanel.transform;
        Label(P, "PASS", 48, TextAnchor.UpperCenter, new Color(1f, 0.8f, 0.3f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => passPanel.SetActive(false));
        passHeader = Label(P, "", 24, TextAnchor.MiddleLeft, TextDim, new Vector2(0, 1), new Vector2(0.62f, 1), new Vector2(30, -175), new Vector2(0, -90));
        Text allT;
        MakeButton(P, "Tout récupérer", new Vector2(0.62f, 1), new Vector2(1, 1), new Vector2(0, -172), new Vector2(-20, -96),
            new Color(0.2f, 0.5f, 0.22f), "Tout récupérer", 28, out allT, () => { string e = GameState.ClaimAllPass(); if (e != null) Toast(e); RefreshPass(); RefreshPassButton(); });

        // Liste défilante des paliers
        var view = MakeRect("Vue", P, Vector2.zero, Vector2.one, new Vector2(12, 12), new Vector2(-12, -185));
        view.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);
        view.gameObject.AddComponent<RectMask2D>();
        const float rowH = 130f;
        int n = MissionData.PassStage.Length;
        var content = MakeRect("Paliers", view, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -n * rowH), Vector2.zero);
        content.pivot = new Vector2(0.5f, 1f);
        var sr = view.gameObject.AddComponent<ScrollRect>();
        sr.content = content; sr.viewport = view; sr.horizontal = false; sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 40f;
        for (int i = 0; i < n; i++)
        {
            int idx = i;
            float top = -i * rowH;
            passRowBg[i] = Box(content, "Palier " + i, new Vector2(0, 1), new Vector2(1, 1), new Vector2(6, top - rowH + 6), new Vector2(-6, top - 6), Panel);
            passRowBg[i].gameObject.AddComponent<Outline>().effectDistance = new Vector2(3, -3);
            passRowText[i] = Label(passRowBg[i].transform, "", 24, TextAnchor.MiddleLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(20, 6), new Vector2(-250, -6));
            passRowBtn[i] = MakeButton(passRowBg[i].transform, "Récupérer", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-230, 16), new Vector2(-16, -16),
                new Color(0.2f, 0.5f, 0.22f), "", 26, out passRowBtnText[i], () => { string e = GameState.ClaimPass(idx); if (e != null) Toast(e); RefreshPass(); RefreshPassButton(); });
        }
        passPanel.SetActive(false);
    }

    void TogglePass()
    {
        bool open = !passPanel.activeSelf;
        CloseAllPanels();
        passPanel.SetActive(open);
        RefreshPass();
    }

    void RefreshPass()
    {
        if (passPanel == null || !passPanel.activeSelf) return;
        var d = GameState.Data;
        int done = 0;
        for (int i = 0; i < MissionData.PassStage.Length; i++) if (d.passClaimed[i]) done++;
        passHeader.text = "<color=#FFD27A>" + done + " / " + MissionData.PassStage.Length + "</color> paliers";
        for (int i = 0; i < MissionData.PassStage.Length; i++)
        {
            bool reached = GameState.PassReached(i), claimed = d.passClaimed[i];
            var ol = passRowBg[i].GetComponent<Outline>();
            passRowBg[i].color = claimed ? new Color(0.07f, 0.07f, 0.06f) : reached ? new Color(0.12f, 0.22f, 0.08f) : new Color(0.13f, 0.08f, 0.05f);
            ol.effectColor = claimed ? new Color(0.3f, 0.3f, 0.3f) : reached ? new Color(0.5f, 1f, 0.4f) : new Color(0.5f, 0.35f, 0.15f);
            passRowText[i].text = "<b>" + GameState.StageName(MissionData.PassStage[i]) + "</b>\n<size=21>" + GameState.ContentText(GameState.PassContent(i), "  ") + "</size>";
            passRowBtn[i].interactable = reached && !claimed;
            passRowBtnText[i].text = claimed ? "Pris ✔" : reached ? "RÉCUPÉRER" : "Verrouillé";
        }
    }
}
