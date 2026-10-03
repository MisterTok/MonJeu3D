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

    void BuildShopPanel(Transform R)
    {
        shopPanel = MakeRect("Boutique", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        shopPanel.AddComponent<Image>().color = new Color(0.07f, 0.035f, 0.03f, 0.98f);
        Transform P = shopPanel.transform;
        Label(P, "BOUTIQUE", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => shopPanel.SetActive(false));
        shopGems = Label(P, "", 30, TextAnchor.UpperLeft, new Color(0.5f, 0.9f, 1f), new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(30, -130), new Vector2(0, -88));
        shopTimer = Label(P, "", 24, TextAnchor.UpperRight, TextDim, new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(0, -130), new Vector2(-30, -92));

        // Cadeau du jour
        var gift = Box(P, "Cadeau", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -290), new Vector2(-12, -140), new Color(0.13f, 0.08f, 0.03f, 1f));
        gift.gameObject.AddComponent<Outline>().effectColor = new Color(1f, 0.8f, 0.3f);
        Label(gift.transform, "Cadeau du jour", 36, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.35f), Vector2.zero, Vector2.one, new Vector2(24, 0), new Vector2(-300, -14));
        shopGiftText = Label(gift.transform, "", 26, TextAnchor.LowerLeft, TextMain, Vector2.zero, Vector2.one, new Vector2(24, 18), new Vector2(-300, 0));
        shopGiftBtn = MakeButton(gift.transform, "Récupérer", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-270, 22), new Vector2(-22, -22),
            new Color(0.2f, 0.55f, 0.25f), "", 32, out shopGiftBtnText, () => ShopResult(GameState.TakeFreeGift()));

        // Offres du jour
        Label(P, "Offres du jour (1 achat chacune)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -345), new Vector2(0, -305));
        for (int i = 0; i < ShopData.DealsPerDay; i++)
        {
            int slot = i;
            dealBg[i] = Box(P, "Offre " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1), new Vector2(12, -800), new Vector2(-12, -350), Panel);
            dealBg[i].gameObject.AddComponent<Outline>().effectDistance = new Vector2(4, -4);
            dealTitle[i] = Label(dealBg[i].transform, "", 28, TextAnchor.UpperCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, -14));
            dealBody[i] = Label(dealBg[i].transform, "", 27, TextAnchor.MiddleCenter, TextMain, Vector2.zero, Vector2.one, new Vector2(10, 110), new Vector2(-10, -60));
            dealBody[i].lineSpacing = 1.25f;
            dealBtn[i] = MakeButton(dealBg[i].transform, "Acheter", new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 16), new Vector2(-16, 96),
                new Color(0.15f, 0.42f, 0.62f), "", 28, out dealBtnText[i], () => ShopResult(GameState.BuyDeal(slot)));
        }

        // Ressources
        Label(P, "Ressources contre des gemmes", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -855), new Vector2(0, -815));
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

        // Packs de gemmes : affichés pour plus tard.
        Label(P, "Packs de gemmes (achats réels : bientôt)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -1290), new Vector2(0, -1250));
        for (int i = 0; i < ShopData.GemPacks.Length; i++)
        {
            var g = Box(P, "Pack " + i, new Vector2(i / 4f, 1), new Vector2((i + 1) / 4f, 1), new Vector2(10, -1440), new Vector2(-10, -1295), new Color(0.06f, 0.1f, 0.13f, 1f));
            g.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.5f, 0.6f);
            Label(g.transform, "<b>" + ShopData.GemPacks[i] + "</b>\n<size=20>gemmes</size>\n<size=20><color=#7F98A6>" + ShopData.GemPackPrices[i] + "</color></size>", 30,
                TextAnchor.MiddleCenter, new Color(0.5f, 0.9f, 1f));
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
        shopTimer.text = "Nouvelles offres dans " + GameState.FmtTime((long)t.TotalSeconds);
    }

    void RefreshShop()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;
        var d = GameState.Data;
        shopTimerTick = 0f;
        shopGems.text = "<size=24>gemmes</size>  <b>" + GameState.Fmt(d.gems) + "</b>";

        shopGiftText.text = GameState.ContentText(ShopData.FreeGift, "   ");
        shopGiftBtn.interactable = !d.shopFreeTaken;
        shopGiftBtnText.text = d.shopFreeTaken ? "Déjà pris" : "GRATUIT";

        int price = GameState.DealPrice;
        for (int i = 0; i < ShopData.DealsPerDay; i++)
        {
            var deal = ShopData.Deals[d.shopDeals[i]];
            var c = deal.color;
            bool bought = d.shopDealBought[i];
            dealBg[i].color = bought ? new Color(0.08f, 0.07f, 0.07f, 1f) : new Color(c.r * 0.2f, c.g * 0.2f, c.b * 0.2f, 1f);
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
