using UnityEngine;
using UnityEngine.UI;

// Boutique : cadeau du jour, offres du jour, ressources contre des gemmes, packs de gemmes (à venir).
public partial class ForgeUI
{
    GameObject shopPanel;
    Text shopGems, shopTimer, shopGiftText;
    Button shopGiftBtn;
    Text shopGiftBtnText;
    readonly Text[] dealTitle = new Text[ShopData.DealsPerDay];
    readonly Text[] dealBody = new Text[ShopData.DealsPerDay];
    readonly Image[] dealBg = new Image[ShopData.DealsPerDay];
    readonly Button[] dealBtn = new Button[ShopData.DealsPerDay];
    readonly Text[] dealBtnText = new Text[ShopData.DealsPerDay];
    readonly Button[] bundleBtn = new Button[ShopData.Bundles.Length];
    readonly Text[] bundleText = new Text[ShopData.Bundles.Length];
    float shopTimerTick;

    Text ssShopTitle;
    readonly Button[] ssShopBtn = new Button[3];
    readonly Text[] ssShopText = new Text[3];

    void BuildShopPanel(Transform R)
    {
        shopPanel = MakeRect("Boutique", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(shopPanel);
        Transform P = shopPanel.transform;
        Label(P, "BOUTIQUE", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => shopPanel.SetActive(false));
        shopGems = Label(P, "", 30, TextAnchor.UpperLeft, new Color(0.5f, 0.9f, 1f), new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(30, -130), new Vector2(0, -88));
        shopTimer = Label(P, "", 24, TextAnchor.UpperRight, TextDim, new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(0, -130), new Vector2(-30, -92));

        // Cadeau du jour
        var gift = Box(P, "Cadeau", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -290), new Vector2(-12, -140), new Color(0.13f, 0.08f, 0.03f, 1f));
        UISkin.Sliced(gift, "tile", 1.6f); gift.color = SkinTint(gift.color);
        gift.gameObject.AddComponent<Outline>().effectColor = new Color(1f, 0.8f, 0.3f);
        Label(gift.transform, "Cadeau du jour", 36, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.35f), Vector2.zero, Vector2.one, new Vector2(24, 0), new Vector2(-300, -14));
        shopGiftText = Label(gift.transform, "", 26, TextAnchor.LowerLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(24, 18), new Vector2(-300, 0));
        shopGiftBtn = MakeButton(gift.transform, "Récupérer", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-270, 22), new Vector2(-22, -22),
            new Color(0.2f, 0.55f, 0.25f), "", 32, out shopGiftBtnText, () => ShopResult(GameState.TakeFreeGift()));

        // Offres du jour
        Label(P, "Offres du jour", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -345), new Vector2(0, -305));
        for (int i = 0; i < ShopData.DealsPerDay; i++)
        {
            int slot = i;
            dealBg[i] = Box(P, "Offre " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1), new Vector2(12, -800), new Vector2(-12, -350), Panel);
            dealBg[i].gameObject.AddComponent<Outline>().effectDistance = new Vector2(4, -4);
            UISkin.Sliced(dealBg[i], "tile", 1.6f);
            dealTitle[i] = Label(dealBg[i].transform, "", 28, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, -14));
            dealBody[i] = Label(dealBg[i].transform, "", 27, TextAnchor.MiddleCenter, TextMain, Vector2.zero, Vector2.one, new Vector2(10, 110), new Vector2(-10, -60));
            dealBody[i].lineSpacing = 1.25f;
            dealBtn[i] = MakeButton(dealBg[i].transform, "Acheter", new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 16), new Vector2(-16, 96),
                new Color(0.15f, 0.42f, 0.62f), "", 28, out dealBtnText[i], () => ShopResult(GameState.BuyDeal(slot)));
        }

        // Ressources
        Label(P, "Ressources", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -855), new Vector2(0, -815));
        for (int i = 0; i < ShopData.Bundles.Length; i++)
        {
            int idx = i, row = i / 3, col = i % 3;
            float top = -860 - row * 190;
            var b = ShopData.Bundles[i];
            var c = ShopData.CurrencyColors[b.currency];
            bundleBtn[i] = MakeButton(P, "Ressource " + i, new Vector2(col / 3f, 1), new Vector2((col + 1) / 3f, 1), new Vector2(12, top - 175), new Vector2(-12, top),
                new Color(c.r * 0.25f, c.g * 0.25f, c.b * 0.25f), "", 24, out bundleText[i], () => ShopResult(GameState.BuyBundle(idx)));
            bundleBtn[i].GetComponent<Outline>().effectColor = c;
        }

        // Pas japonais : 8 jetons offerts par édition, packs (la section n'est utile que pendant la semaine de l'événement)
        ssShopTitle = Label(P, "Pas japonais", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -1285), new Vector2(0, -1245));
        ssShopBtn[0] = MakeButton(P, "Jetons offerts", new Vector2(0f, 1), new Vector2(1 / 3f, 1), new Vector2(12, -1460), new Vector2(-12, -1290),
            new Color(0.15f, 0.5f, 0.22f), "", 26, out ssShopText[0], () => { string e = SteppingStones.ClaimFree(); Toast(e ?? "+" + SteppingStones.FreeTokens + " jetons du Pas japonais"); RefreshShop(); });
        for (int i = 0; i < 2; i++)
        {
            int k = i;
            ssShopBtn[i + 1] = MakeButton(P, "Pack de jetons " + i, new Vector2((i + 1) / 3f, 1), new Vector2((i + 2) / 3f, 1), new Vector2(12, -1460), new Vector2(-12, -1290),
                new Color(0.55f, 0.22f, 0.08f), "", 26, out ssShopText[i + 1], () => { string e = SteppingStones.BuyPack(k); Toast(e ?? "+" + SteppingStones.PackTokens[k] + " jetons du Pas japonais"); RefreshShop(); });
        }
        shopPanel.SetActive(false);
        GameState.ShopMessage += Toast;
    }

    void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;
        CloseAllPanels();
        shopPanel.SetActive(open);
        RefreshShop();
    }

    void ShopResult(string err)
    {
        if (err != null) Toast(err);
        RefreshShop();
    }

    void UpdateShop()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;
        shopTimerTick -= Time.deltaTime;
        if (shopTimerTick > 0f) return;
        shopTimerTick = 1f;
        var t = GameState.TimeToShopRefresh();
        shopTimer.text = "Renouvellement " + GameState.FmtTime((long)t.TotalSeconds);
    }

    void RefreshShop()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;
        var d = GameState.Data;
        shopTimerTick = 0f;
        shopGems.text = "<size=24>gemmes</size>  <b>" + GameState.Fmt(d.gems) + "</b>";
        bool ssOn = SteppingStones.Unlocked && SteppingStones.Active;
        ssShopTitle.text = ssOn ? "Pas japonais  <size=22><color=#FFC07A>" + SteppingStones.SeasonPack + "</color> · jetons " + d.ssTokens + "</size>"
            : "Pas japonais  <size=22>" + (SteppingStones.Unlocked ? "revient dans " + GameState.FmtTime((long)SteppingStones.TimeLeft.TotalSeconds) : GameState.UnlockText(SteppingStones.Unlock)) + "</size>";
        ssShopBtn[0].interactable = SteppingStones.FreeAvailable;
        ssShopText[0].text = "<b>" + SteppingStones.FreeTokens + " jetons</b>\n<size=22>" + (SteppingStones.FreeAvailable ? "GRATUIT" : ssOn ? "pris" : "hors édition") + "</size>";
        for (int i = 0; i < 2; i++)
        {
            ssShopBtn[i + 1].interactable = ssOn && d.gems >= SteppingStones.PackGems[i];
            ssShopText[i + 1].text = "<b>" + SteppingStones.PackTokens[i] + " jetons</b>\n<size=22>" + SteppingStones.PackGems[i] + " gemmes</size>";
        }

        shopGiftText.text = GameState.ContentText(ShopData.FreeGift, "   ");
        shopGiftBtn.interactable = !d.shopFreeTaken;
        shopGiftBtnText.text = d.shopFreeTaken ? "Déjà pris" : "GRATUIT";

        int price = GameState.DealPrice;
        for (int i = 0; i < ShopData.DealsPerDay; i++)
        {
            var deal = ShopData.Deals[d.shopDeals[i]];
            var c = deal.color;
            bool bought = d.shopDealBought[i];
            dealBg[i].color = SkinTint(bought ? new Color(0.08f, 0.07f, 0.07f, 1f) : new Color(c.r * 0.2f, c.g * 0.2f, c.b * 0.2f, 1f));
            dealBg[i].GetComponent<Outline>().effectColor = bought ? new Color(0.3f, 0.3f, 0.3f) : c;
            dealTitle[i].text = "<b>" + deal.name + "</b>";
            dealTitle[i].color = Color.Lerp(c, Color.white, 0.3f);
            dealBody[i].text = GameState.ContentText(GameState.DealContent(i), "\n");
            dealBtn[i].interactable = !bought && d.gems >= price;
            dealBtnText[i].text = bought ? "Acheté ✔" : price + " gemmes";
        }

        for (int i = 0; i < ShopData.Bundles.Length; i++)
        {
            var b = ShopData.Bundles[i];
            int left = b.perDay - d.shopBundleBuys[i];
            bundleBtn[i].interactable = left > 0 && d.gems >= b.price;
            bundleText[i].text = "<b><size=30>+" + GameState.Fmt(GameState.ShopAmount(b.currency, b.amount)) + "</size></b>\n" + ShopData.CurrencyNames[b.currency]
                + "\n<color=#80E0FF>" + b.price + " gemmes</color>  <size=18><color=#A89C94>(" + left + "/" + b.perDay + ")</color></size>";
        }
    }
}
