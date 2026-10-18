using UnityEngine;
using UnityEngine.UI;

// Écran du « Pas japonais » (Stepping Stones de FM) : les 2 skins de l'édition, rivière de lave à 8 rangées de 2 pierres.
// On touche la pierre de gauche ou de droite de la rangée suivante ; l'une des deux casse.
public partial class ForgeUI
{
    GameObject ssPanel;
    Text ssSeasonText, ssInfo, ssAutoText, ssBuyText, ssTokensText;
    Tile[] ssSkinTile = new Tile[2];
    Text[] ssSkinText = new Text[2];
    Button ssAutoBtn;
    RectTransform ssBoard, ssMarker;
    Image ssMarkerImg;
    readonly RectTransform[,] ssStone = new RectTransform[SteppingStones.Rows + 1, 2];
    readonly Image[,] ssStoneImg = new Image[SteppingStones.Rows + 1, 2];
    float ssFall, ssJumpAnim, ssBackToShore;
    Vector2 ssMarkerFrom, ssTarget;
    int ssBrokenRow = -1, ssBrokenSide;

    const float SsRowStep = 0.112f;
    static Vector2 SsShore => new Vector2(0.5f, 0.055f);
    static Vector2 SsStonePos(int row, int side) => new Vector2(side == 0 ? 0.3f : 0.7f, 0.055f + row * SsRowStep);

    void BuildStonesPanel(Transform R)
    {
        ssPanel = MakeRect("Pas japonais", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(ssPanel);
        Transform P = ssPanel.transform;
        Label(P, "PAS JAPONAIS", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => ssPanel.SetActive(false));
        ssSeasonText = Label(P, "", 26, TextAnchor.MiddleCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -140), new Vector2(-20, -96));
        ssTokensText = Label(P, "", 26, TextAnchor.UpperLeft, new Color(0.7f, 0.9f, 1f), new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(26, -96), new Vector2(0, -60));

        // Les 2 skins de l'édition : le petit (1 % à la 6e pierre) et le grand (bout du chemin)
        for (int i = 0; i < 2; i++)
        {
            int k = i;
            float x0 = i == 0 ? 0.04f : 0.52f;
            ssSkinTile[i] = MakeTile(P, new Vector2(x0, 1), new Vector2(x0 + 0.17f, 1), new Vector2(0, -330), new Vector2(0, -150), () => OpenSeasonSkin(k));
            ssSkinText[i] = Label(P, "", 24, TextAnchor.MiddleLeft, TextMain, new Vector2(x0 + 0.18f, 1), new Vector2(x0 + 0.47f, 1), new Vector2(4, -330), new Vector2(0, -150));
        }

        // Plateau : lave, rive et 8 rangées de 2 pierres
        var boardImg = Box(P, "Plateau", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -1320), new Vector2(-24, -345), Color.white);
        boardImg.sprite = LoadSprite("ForgeUI/Event/lava");
        boardImg.raycastTarget = false;
        ssBoard = boardImg.rectTransform;
        var shore = MakeRect("Rive", ssBoard, SsShore, SsShore, new Vector2(-150, -50), new Vector2(150, 50));
        var shoreImg = shore.gameObject.AddComponent<Image>();
        shoreImg.sprite = LoadSprite("ForgeUI/Event/shore"); shoreImg.raycastTarget = false;
        for (int r = 1; r <= SteppingStones.Rows; r++)
        {
            bool last = r == SteppingStones.Rows, rare = r == SteppingStones.RareRow;
            for (int s = 0; s < 2; s++)
            {
                int row = r, side = s;
                var p = SsStonePos(r, s);
                var rt = MakeRect("Pierre " + r + (s == 0 ? "G" : "D"), ssBoard, p, p, new Vector2(-80, -42), new Vector2(80, 42));
                var img = rt.gameObject.AddComponent<Image>();
                img.sprite = LoadSprite(last ? "ForgeUI/Event/stone_gold" : "ForgeUI/Event/stone");
                rt.gameObject.AddComponent<Button>().onClick.AddListener(() => OnSsStone(row, side));
                ssStone[r, s] = rt; ssStoneImg[r, s] = img;
            }
            // Gains de la rangée, entre les deux pierres
            var mid = new Vector2(0.5f, 0.055f + r * SsRowStep);
            string txt = last ? "<color=#FFE070><b>SKIN</b></color>" : "+" + SteppingStones.Hammers[r - 1] + (rare ? "\n<size=18><color=#E8A0FF>1 % skin</color></size>" : "");
            var lab = Label(ssBoard, txt, 26, TextAnchor.MiddleCenter, Color.white, mid, mid, new Vector2(-55, -34), new Vector2(55, 34));
            lab.raycastTarget = false;
            if (!last) UISkin.Icon(ssBoard, "i_hammer", mid, mid, new Vector2(-62, rare ? 0 : -14), new Vector2(-34, rare ? 28 : 14)).raycastTarget = false;
        }
        ssMarker = MakeRect("Héros", ssBoard, SsShore, SsShore, new Vector2(-55, -18), new Vector2(55, 110));
        ssMarkerImg = ssMarker.gameObject.AddComponent<Image>();
        ssMarkerImg.preserveAspect = true; ssMarkerImg.raycastTarget = false;

        ssInfo = Label(P, "", 26, TextAnchor.MiddleCenter, TextMain, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -1400), new Vector2(-20, -1325));
        ssAutoBtn = MakeButton(P, "Auto", new Vector2(0.04f, 1), new Vector2(0.48f, 1), new Vector2(0, -1520), new Vector2(0, -1405),
            new Color(0.25f, 0.25f, 0.3f), "", 30, out ssAutoText, OnSsAuto);
        // Les jetons viennent de la boutique (8 offerts chaque jour de l'édition, packs payants)
        MakeButton(P, "Jetons", new Vector2(0.52f, 1), new Vector2(0.96f, 1), new Vector2(0, -1520), new Vector2(0, -1405),
            new Color(0.12f, 0.4f, 0.55f), "", 26, out ssBuyText, () => { ssPanel.SetActive(false); ToggleShop(); });
        ssPanel.SetActive(false);
    }

    static Sprite LoadSprite(string path)
    {
        var t = Resources.Load<Texture2D>(path);
        return t == null ? null : Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
    }

    void ToggleStones()
    {
        if (!SteppingStones.Unlocked) { Toast(GameState.UnlockText(SteppingStones.Unlock)); return; }
        CloseAllPanels();
        ssPanel.SetActive(true);
        SteppingStones.Tick();
        ssJumpAnim = ssFall = ssBackToShore = 0f; ssBrokenRow = -1;
        SetMarker(SsCurrent);
        RefreshStones();
    }

    // Position du héros : la rive, ou la pierre franchie de la rangée atteinte.
    Vector2 SsCurrent
    {
        get { var d = GameState.Data; return d.ssPos == 0 ? SsShore : SsStonePos(d.ssPos, (d.ssPending >> (d.ssPos - 1)) & 1); }
    }

    void SetMarker(Vector2 p) { ssMarker.anchorMin = ssMarker.anchorMax = p; }

    void OpenSeasonSkin(int k)
    {
        var l = SteppingStones.SeasonSkins;
        if (k < l.Count) OpenHeroSkin(l[k].id);
    }

    bool SsBusy => ssJumpAnim > 0f || ssFall > 0f || ssBackToShore > 0f;

    void OnSsStone(int row, int side)
    {
        if (SsBusy || row != GameState.Data.ssPos + 1) return;
        Vector2 from = SsCurrent;
        string msg;
        var st = SteppingStones.Jump(side, out msg);
        if (st.result == SteppingStones.Result.Error) { Toast(msg); RefreshStones(); return; }
        Sfx.Click();
        ssMarkerFrom = from;
        ssTarget = SsStonePos(row, side);
        ssJumpAnim = 0.35f;
        if (st.result == SteppingStones.Result.Fall)
        {
            ssBrokenRow = row; ssBrokenSide = side;
            ssFall = 1.1f;
            Toast("Plouf ! La pierre a cassé");
        }
        else
        {
            Sfx.Coin();
            ssInfo.text = "Pierre " + row + " : <color=#FFE2A0>+" + st.hammers + " marteaux  +" + GameState.Fmt(st.gold) + " or</color>";
            if (st.result == SteppingStones.Result.End) ssBackToShore = 1.6f;
            if (st.skin != null) { Sfx.Fanfare(); Toast("NOUVEAU SKIN : " + HeroSkins.Find(st.skin).name + " !"); }
            else if (st.result == SteppingStones.Result.End) { Sfx.Fanfare(); Toast("Bout du chemin ! +" + st.gems + " gemmes"); }
        }
        RefreshStones(false);
    }

    void TestStones(string c)
    {
        if (c == "tokens") { GameState.Data.ssTokens += 40; GameState.Data.ssPos = 0; RefreshStones(); }
        else if (c == "auto") OnSsAuto();
        else { SsFinishAnims(); OnSsStone(GameState.Data.ssPos + 1, c == "1" ? 1 : 0); }
        if (c == "auto" || c == "tokens") SsFinishAnims();
    }

    // Termine aussitôt les animations en cours (pierre brisée remise, héros à sa place).
    void SsFinishAnims()
    {
        ssJumpAnim = ssBackToShore = 0f;
        if (ssFall > 0f) { ssFall = 0f; ssMarker.localScale = Vector3.one; ssMarkerImg.color = Color.white; }
        if (ssBrokenRow > 0) ssStone[ssBrokenRow, ssBrokenSide].localScale = Vector3.one;
        ssBrokenRow = -1;
        SetMarker(SsCurrent);
        RefreshStones();
    }

    void OnSsAuto()
    {
        if (SsBusy) return;
        int before = GameState.Data.ssTokens;
        string msg = SteppingStones.AutoSweep();
        Toast(msg);
        if (GameState.Data.ssTokens == before) return;
        if (msg.Contains("SKIN")) Sfx.Fanfare(); else Sfx.Coin();
        SetMarker(SsShore);
        RefreshStones();
    }

    void UpdateStones()
    {
        if (ssPanel == null || !ssPanel.activeSelf) return;
        float dt = Time.deltaTime;
        if (ssJumpAnim > 0f)
        {
            ssJumpAnim = Mathf.Max(0f, ssJumpAnim - dt);
            float t = 1f - ssJumpAnim / 0.35f;
            var p = Vector2.Lerp(ssMarkerFrom, ssTarget, t);
            p.y += Mathf.Sin(t * Mathf.PI) * 0.05f;   // petit saut en cloche
            SetMarker(p);
        }
        else if (ssFall > 0f)
        {
            // La pierre se brise et le héros tombe dans la lave.
            ssFall = Mathf.Max(0f, ssFall - dt);
            float k = ssFall / 1.1f;
            ssMarker.localScale = Vector3.one * Mathf.Max(0.05f, k);
            ssMarkerImg.color = Color.Lerp(new Color(1f, 0.3f, 0.1f, 0f), Color.white, k);
            if (ssBrokenRow > 0)
            {
                ssStone[ssBrokenRow, ssBrokenSide].localScale = Vector3.one * Mathf.Max(0.05f, k);
                ssStoneImg[ssBrokenRow, ssBrokenSide].color = Color.Lerp(new Color(1f, 0.25f, 0.1f, 0f), new Color(0.6f, 0.3f, 0.25f), k);
            }
            if (ssFall <= 0f)
            {
                ssMarker.localScale = Vector3.one; ssMarkerImg.color = Color.white;
                ssBrokenRow = -1;
                SetMarker(SsShore);
                RefreshStones();
            }
        }
        else if (ssBackToShore > 0f)
        {
            ssBackToShore -= dt;
            if (ssBackToShore <= 0f) { SetMarker(SsShore); RefreshStones(); }
        }
        else
        {
            // La rangée à jouer respire doucement.
            int next = GameState.Data.ssPos + 1;
            float s = 1f + Mathf.Sin(Time.time * 5f) * 0.05f;
            if (next <= SteppingStones.Rows) for (int i = 0; i < 2; i++) ssStone[next, i].localScale = Vector3.one * s;
        }
    }

    void RefreshStones() => RefreshStones(true);

    void RefreshStones(bool clearInfo)
    {
        if (ssPanel == null || !ssPanel.activeSelf) return;
        var d = GameState.Data;
        var t = SteppingStones.TimeLeft;
        ssSeasonText.text = SteppingStones.Active
            ? "Édition : <color=#FFC07A>" + SteppingStones.SeasonPack + "</color>  ·  fin dans " + GameState.FmtTime((long)t.TotalSeconds)
            : "<color=#FF9A7A>En pause</color>  ·  prochaine édition (<color=#FFC07A>" + SteppingStones.SeasonPack + "</color>) dans " + GameState.FmtTime((long)t.TotalSeconds);
        ssTokensText.text = "Jetons <b>" + d.ssTokens + "</b>";
        var l = SteppingStones.SeasonSkins;
        for (int i = 0; i < 2; i++)
        {
            bool has = i < l.Count;
            ssSkinTile[i].frame.gameObject.SetActive(has); ssSkinText[i].gameObject.SetActive(has);
            if (!has) continue;
            var s = l[i];
            bool owned = HeroSkins.Owned(s.id);
            SetTile(ssSkinTile[i], s.rarity, SkinIcon(s.id), "?", ProgressionData.RarityColors[s.rarity], 1, 1, 1, owned && d.skinEquipped == s.id);
            ssSkinTile[i].barBg.gameObject.SetActive(false);
            string how = i == 0 && l.Count > 1 ? "1 % · pierre " + SteppingStones.RareRow : "Bout du chemin";
            ssSkinText[i].text = "<b>" + s.name + "</b>\n<size=22>" + (owned ? "<color=#8FE08F>✔</color>" : "<color=#FFC07A>" + how + "</color>") + "</size>";
        }

        // Pierres : rangées franchies (côté choisi en clair), rangée à jouer, rangées à venir.
        int pos = d.ssPos;
        for (int r = 1; r <= SteppingStones.Rows; r++)
            for (int s = 0; s < 2; s++)
            {
                if (r == ssBrokenRow && s == ssBrokenSide) continue;
                bool passed = r <= pos, chosen = passed && ((d.ssPending >> (r - 1)) & 1) == s;
                ssStoneImg[r, s].color = passed ? (chosen ? Color.white : new Color(1f, 1f, 1f, 0.3f)) : r == pos + 1 ? Color.white : new Color(0.85f, 0.8f, 0.78f);
                ssStone[r, s].localScale = Vector3.one;
            }

        ssMarkerImg.sprite = null;
        var shown = HeroSkins.Shown;
        var icon = !string.IsNullOrEmpty(shown) ? SkinIcon(shown) as Texture2D : null;
        if (icon != null) ssMarkerImg.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0f), 100f);
        else ssMarkerImg.sprite = UISkin.Get("i_hero");
        if (clearInfo) ssInfo.text = "";
        ssAutoBtn.interactable = SteppingStones.Active && pos == 0 && d.ssTokens >= SteppingStones.AutoMin;
        ssAutoText.text = "AUTO x" + SteppingStones.AutoRuns;
        ssBuyText.text = SteppingStones.FreeAvailable ? "+" + SteppingStones.FreeTokens + " GRATUIT" : "+ Jetons";
    }
}
