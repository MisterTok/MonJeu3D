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
        Label(P, "Affronte des escouades de démons. L'énergie n'est consommée qu'en cas de victoire. Le niveau des missions monte avec le Voleur de marteau.", 23,
            TextAnchor.UpperCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -150), new Vector2(-30, -88));
        missionEnergy = Label(P, "", 32, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.4f), new Vector2(0, 1), new Vector2(0.6f, 1), new Vector2(30, -230), new Vector2(0, -160));
        refreshBtn = MakeButton(P, "Nouvelle liste", new Vector2(0.6f, 1), new Vector2(1, 1), new Vector2(0, -228), new Vector2(-20, -162),
            new Color(0.35f, 0.2f, 0.5f), "", 24, out refreshText, () => { string e = GameState.RefreshMissionList(); if (e != null) Toast(e); RefreshMissions(); });

        for (int i = 0; i < MissionData.OfferCount; i++)
        {
            int slot = i;
            float top = -240 - i * 250, bottom = top - 235;
            var card = Box(P, "Mission " + i, new Vector2(0, 1), new Vector2(1, 1), new Vector2(18, bottom), new Vector2(-18, top), new Color(0.12f, 0.06f, 0.13f, 1f));
            card.gameObject.AddComponent<Outline>().effectColor = MissionPurple;
            Box(card.transform, "Bande", new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(14, 0), MissionPurple);
            missionTitle[i] = Label(card.transform, "", 34, TextAnchor.UpperLeft, Color.Lerp(MissionPurple, Color.white, 0.3f), Vector2.zero, Vector2.one, new Vector2(36, 0), new Vector2(-260, -14));
            missionInfo[i] = Label(card.transform, "", 23, TextAnchor.LowerLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(36, 16), new Vector2(-260, 0));
            missionBtn[i] = MakeButton(card.transform, "Lancer", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-240, 24), new Vector2(-20, -24),
                new Color(0.45f, 0.22f, 0.65f), "LANCER", 34, out missionBtnText[i], () => OnStartMission(slot));
        }
        missionFooter = Label(P, "", 24, TextAnchor.LowerCenter, TextDim, new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 16), new Vector2(-20, 70));
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
            missionInfo[i].text = units + " ennemi" + (units > 1 ? "s" : "") + " · chacun PV " + GameState.Fmt(GameState.MissionUnitHp(i)) + " / ATQ " + GameState.Fmt(GameState.MissionUnitAtk(i))
                + "\n" + GameState.ContentText(GameState.MissionReward(lvl), "  ");
            missionBtn[i].interactable = can;
            missionBtnText[i].text = d.missionEnergy > 0 ? "LANCER" : "DEMAIN";
        }
        var t = GameState.TimeToKeyRefresh();
        missionFooter.text = "Recharge de l'énergie et nouvelle liste dans " + GameState.FmtTime((long)t.TotalSeconds)
            + "\n<color=#BFD8FF>Ton héros : ATQ " + GameState.Fmt(GameState.TotalAtk()) + "  ·  PV " + GameState.Fmt(GameState.TotalHp()) + "</color>";
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
        Label(P, "PASS DE PROGRESSION", 46, TextAnchor.UpperCenter, new Color(1f, 0.8f, 0.3f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
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
        passHeader.text = "Franchis des étapes du chemin pour gagner des récompenses.\n<color=#FFD27A>" + done + "/" + MissionData.PassStage.Length + " paliers récupérés</color>";
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
