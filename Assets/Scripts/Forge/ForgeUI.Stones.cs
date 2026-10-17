using UnityEngine;
using UnityEngine.UI;

// Écran du « Pas japonais » (Stepping Stones de FM) : saison et ses 2 skins, rivière de lave à 6 pierres, sauts, butin.
public partial class ForgeUI
{
    GameObject ssPanel;
    Text ssSeasonText, ssInfo, ssJumpText, ssCashText, ssAutoText, ssBuyText, ssEclatsText;
    Tile[] ssSkinTile = new Tile[2];
    Text[] ssSkinText = new Text[2];
    Button[] ssClaimBtn = new Button[2];
    Text[] ssClaimText = new Text[2];
    Button ssJumpBtn, ssCashBtn, ssAutoBtn;
    RectTransform ssBoard, ssMarker;
    Image ssMarkerImg;
    readonly RectTransform[] ssStone = new RectTransform[SteppingStones.Stones + 1];
    bool ssAuto;
    float ssAutoTimer, ssFall, ssJumpAnim;
    Vector2 ssMarkerFrom;

    // Positions des pierres sur le plateau (0 = rive), en zigzag du bas vers le haut.
    static readonly Vector2[] SsPos = { new Vector2(0.5f, 0.07f), new Vector2(0.28f, 0.22f), new Vector2(0.7f, 0.35f), new Vector2(0.3f, 0.49f),
                                        new Vector2(0.68f, 0.62f), new Vector2(0.32f, 0.76f), new Vector2(0.55f, 0.91f) };

    void BuildStonesPanel(Transform R)
    {
        ssPanel = MakeRect("Pas japonais", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(ssPanel);
        Transform P = ssPanel.transform;
        Label(P, "PAS JAPONAIS", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => { ssAuto = false; ssPanel.SetActive(false); });
        ssSeasonText = Label(P, "", 26, TextAnchor.MiddleCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -140), new Vector2(-20, -96));

        // Les 2 skins de la saison et leur progression
        for (int i = 0; i < 2; i++)
        {
            int k = i;
            float x0 = i == 0 ? 0.04f : 0.52f;
            ssSkinTile[i] = MakeTile(P, new Vector2(x0, 1), new Vector2(x0 + 0.17f, 1), new Vector2(0, -330), new Vector2(0, -150), () => OpenSeasonSkin(k));
            ssSkinText[i] = Label(P, "", 24, TextAnchor.UpperLeft, TextMain, new Vector2(x0 + 0.18f, 1), new Vector2(x0 + 0.46f, 1), new Vector2(4, -240), new Vector2(0, -150));
            ssClaimBtn[i] = MakeButton(P, "Obtenir " + i, new Vector2(x0 + 0.18f, 1), new Vector2(x0 + 0.45f, 1), new Vector2(4, -330), new Vector2(0, -250),
                new Color(0.55f, 0.2f, 0.5f), "", 24, out ssClaimText[i], () => ClaimSeasonSkin(k));
        }

        // Plateau : lave, rive et pierres
        var boardImg = Box(P, "Plateau", new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -1130), new Vector2(-24, -350), Color.white);
        boardImg.sprite = LoadSprite("ForgeUI/Event/lava");
        boardImg.raycastTarget = false;
        ssBoard = boardImg.rectTransform;
        for (int i = 0; i <= SteppingStones.Stones; i++)
        {
            bool shore = i == 0, last = i == SteppingStones.Stones;
            var rt = MakeRect("Pierre " + i, ssBoard, SsPos[i], SsPos[i], shore ? new Vector2(-150, -60) : new Vector2(-95, -62), shore ? new Vector2(150, 60) : new Vector2(95, 62));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = LoadSprite(shore ? "ForgeUI/Event/shore" : last ? "ForgeUI/Event/stone_gold" : "ForgeUI/Event/stone");
            img.raycastTarget = false;
            if (!shore)
                Label(rt, "<size=22>" + Mathf.RoundToInt(SteppingStones.Chance[i - 1] * 100) + " %</size>\n<color=#FFE2A0>+" + SteppingStones.Loot[i - 1] + "</color>", 26,
                    TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(0, 14), new Vector2(0, 0));
            ssStone[i] = rt;
        }
        ssMarker = MakeRect("Héros", ssBoard, SsPos[0], SsPos[0], new Vector2(-70, -10), new Vector2(70, 150));
        ssMarkerImg = ssMarker.gameObject.AddComponent<Image>();
        ssMarkerImg.preserveAspect = true; ssMarkerImg.raycastTarget = false;

        ssInfo = Label(P, "", 30, TextAnchor.MiddleCenter, TextMain, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -1200), new Vector2(-20, -1135));
        ssJumpBtn = MakeButton(P, "Sauter", new Vector2(0.03f, 1), new Vector2(0.37f, 1), new Vector2(0, -1330), new Vector2(0, -1205),
            new Color(0.15f, 0.5f, 0.22f), "", 34, out ssJumpText, OnSsJump);
        ssCashBtn = MakeButton(P, "Encaisser", new Vector2(0.39f, 1), new Vector2(0.73f, 1), new Vector2(0, -1330), new Vector2(0, -1205),
            new Color(0.62f, 0.32f, 0.06f), "", 32, out ssCashText, OnSsCash);
        ssAutoBtn = MakeButton(P, "Auto", new Vector2(0.75f, 1), new Vector2(0.97f, 1), new Vector2(0, -1330), new Vector2(0, -1205),
            new Color(0.25f, 0.25f, 0.3f), "", 28, out ssAutoText, () => { ssAuto = !ssAuto; RefreshStones(); });
        // Les jetons viennent de la boutique (8 offerts par édition, packs payants)
        MakeButton(P, "Jetons", new Vector2(0.2f, 1), new Vector2(0.8f, 1), new Vector2(0, -1440), new Vector2(0, -1350),
            new Color(0.12f, 0.4f, 0.55f), "", 26, out ssBuyText, () => { ssAuto = false; ssPanel.SetActive(false); ToggleShop(); });
        ssEclatsText = Label(P, "", 26, TextAnchor.UpperLeft, new Color(1f, 0.8f, 0.95f), new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(26, -96), new Vector2(0, -60));
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
        ssMarker.anchorMin = ssMarker.anchorMax = SsPos[GameState.Data.ssPos];
        RefreshStones();
    }

    void OpenSeasonSkin(int k)
    {
        var l = SteppingStones.SeasonSkins;
        if (k < l.Count) OpenHeroSkin(l[k].id);
    }

    void ClaimSeasonSkin(int k)
    {
        var l = SteppingStones.SeasonSkins;
        if (k >= l.Count) return;
        string e = SteppingStones.Claim(l[k].id);
        if (e != null) Toast(e); else { Sfx.Fanfare(); Toast("Nouveau skin : " + l[k].name + " !"); }
        RefreshStones();
    }

    void OnSsJump()
    {
        if (ssJumpAnim > 0f || ssFall > 0f) return;
        int from = GameState.Data.ssPos;
        string msg;
        var r = SteppingStones.Jump(out msg);
        if (r == SteppingStones.Result.Error) { ssAuto = false; Toast(msg); RefreshStones(); return; }
        Sfx.Click();
        ssMarkerFrom = SsPos[from];
        if (r == SteppingStones.Result.Fall)
        {
            // Saut vers la pierre suivante puis chute dans la lave.
            ssTarget = Vector2.Lerp(SsPos[from], SsPos[from + 1], 0.6f);
            ssJumpAnim = 0.35f; ssFall = 1.1f;
            Toast(msg);
        }
        else
        {
            ssTarget = r == SteppingStones.Result.Jackpot ? SsPos[SteppingStones.Stones] : SsPos[GameState.Data.ssPos];
            ssJumpAnim = 0.35f;
            if (r == SteppingStones.Result.Jackpot) { Sfx.Fanfare(); Toast(msg); ssBackToShore = 1.2f; }
        }
        RefreshStones();
    }

    Vector2 ssTarget;
    float ssBackToShore;

    void OnSsCash()
    {
        if (GameState.Data.ssPending <= 0) return;
        int n = SteppingStones.CashOut();
        Sfx.Coin();
        Toast("+" + n + " éclats");
        ssMarker.anchorMin = ssMarker.anchorMax = SsPos[0];
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
            p.y += Mathf.Sin(t * Mathf.PI) * 0.06f;   // petit saut en cloche
            ssMarker.anchorMin = ssMarker.anchorMax = p;
        }
        else if (ssFall > 0f)
        {
            ssFall = Mathf.Max(0f, ssFall - dt);
            float k = ssFall / 1.1f;
            ssMarker.localScale = Vector3.one * Mathf.Max(0.05f, k);
            ssMarkerImg.color = Color.Lerp(new Color(1f, 0.3f, 0.1f, 0f), Color.white, k);
            if (ssFall <= 0f) { ssMarker.localScale = Vector3.one; ssMarkerImg.color = Color.white; ssMarker.anchorMin = ssMarker.anchorMax = SsPos[0]; }
        }
        else if (ssBackToShore > 0f)
        {
            ssBackToShore -= dt;
            if (ssBackToShore <= 0f) ssMarker.anchorMin = ssMarker.anchorMax = SsPos[0];
        }
        else if (ssAuto)
        {
            ssAutoTimer -= dt;
            if (ssAutoTimer <= 0f)
            {
                ssAutoTimer = 0.5f;
                if (GameState.Data.ssPos >= SteppingStones.AutoCashAt) OnSsCash();
                else if (GameState.Data.ssTokens > 0) OnSsJump();
                else { ssAuto = false; RefreshStones(); }
            }
        }
    }

    void RefreshStones()
    {
        if (ssPanel == null || !ssPanel.activeSelf) return;
        var d = GameState.Data;
        var t = SteppingStones.TimeLeft;
        ssSeasonText.text = SteppingStones.Active
            ? "Édition : <color=#FFC07A>" + SteppingStones.SeasonPack + "</color>  ·  fin dans " + GameState.FmtTime((long)t.TotalSeconds)
            : "<color=#FF9A7A>En pause</color>  ·  prochaine édition (<color=#FFC07A>" + SteppingStones.SeasonPack + "</color>) dans " + GameState.FmtTime((long)t.TotalSeconds);
        ssEclatsText.text = "Éclats <b>" + d.ssEclats + "</b>";
        var l = SteppingStones.SeasonSkins;
        for (int i = 0; i < 2; i++)
        {
            bool has = i < l.Count;
            ssSkinTile[i].frame.gameObject.SetActive(has); ssSkinText[i].gameObject.SetActive(has); ssClaimBtn[i].gameObject.SetActive(has);
            if (!has) continue;
            var s = l[i];
            bool owned = HeroSkins.Owned(s.id);
            int cost = SteppingStones.SkinCost(s);
            SetTile(ssSkinTile[i], s.rarity, SkinIcon(s.id), "?", ProgressionData.RarityColors[s.rarity], 1, 1, 1, owned && d.skinEquipped == s.id);
            ssSkinTile[i].barBg.gameObject.SetActive(false);
            ssSkinText[i].text = "<b>" + s.name + "</b>\n<size=22>+" + GameState.FmtPct(HeroSkins.Bonus(s.rarity)) + " dégâts et vie</size>";
            ssClaimBtn[i].interactable = !owned && SteppingStones.Active && d.ssEclats >= cost;
            ssClaimText[i].text = owned ? "Obtenu" : "OBTENIR\n<size=20>" + Mathf.Min(d.ssEclats, cost) + " / " + cost + " éclats</size>";
        }
        ssMarkerImg.sprite = null;
        var shown = HeroSkins.Shown;
        var icon = !string.IsNullOrEmpty(shown) ? SkinIcon(shown) as Texture2D : null;
        if (icon == null) icon = Resources.Load<Texture2D>("ForgeSprites/SkinIcons/Valkyrie_1");
        if (icon != null) ssMarkerImg.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0f), 100f);
        else ssMarkerImg.sprite = UISkin.Get("i_hero");
        ssInfo.text = "Jetons <b>" + d.ssTokens + "</b>     Butin en jeu <color=#FFE2A0><b>" + d.ssPending + "</b> éclats</color>";
        ssJumpBtn.interactable = d.ssTokens > 0 && SteppingStones.Active;
        ssJumpText.text = "SAUTER\n<size=22>1 jeton · " + (d.ssPos < SteppingStones.Stones ? Mathf.RoundToInt(SteppingStones.Chance[d.ssPos] * 100) + " %" : "") + "</size>";
        ssCashBtn.interactable = d.ssPending > 0;
        ssCashText.text = "ENCAISSER\n<size=22>+" + d.ssPending + " éclats</size>";
        ssAutoText.text = ssAuto ? "AUTO\n<size=20>en cours</size>" : "AUTO\n<size=20>pierre " + SteppingStones.AutoCashAt + "</size>";
        ssAutoBtn.GetComponent<Image>().color = UISkin.Vivid(ssAuto ? new Color(0.2f, 0.55f, 0.25f) : new Color(0.25f, 0.25f, 0.3f));
        ssBuyText.text = SteppingStones.FreeAvailable ? "<b>" + SteppingStones.FreeTokens + " jetons offerts</b> dans la boutique" : "Plus de jetons dans la boutique";
    }
}
