using UnityEngine;
using UnityEngine.UI;

// Boutique façon Forge Master : cadeau du jour, offres du jour, ressources contre des gemmes, jetons du Pas japonais.
// Tout le contenu est montré en cases (icône + quantité), les prix avec l'icône de la monnaie.
public partial class ForgeUI
{
    GameObject shopPanel;
    Text shopTimer;
    Button shopGiftBtn;
    Text shopGiftBtnText;
    RectTransform shopGiftBox;
    readonly Text[] dealTitle = new Text[ShopData.DealsPerDay];
    readonly Image[] dealBg = new Image[ShopData.DealsPerDay];
    readonly Image[] dealHead = new Image[ShopData.DealsPerDay];
    readonly RectTransform[] dealBox = new RectTransform[ShopData.DealsPerDay];
    readonly GameObject[] dealSold = new GameObject[ShopData.DealsPerDay];
    readonly Button[] dealBtn = new Button[ShopData.DealsPerDay];
    readonly Text[] dealBtnText = new Text[ShopData.DealsPerDay];
    readonly Button[] bundleBtn = new Button[ShopData.Bundles.Length];
    readonly Text[] bundleText = new Text[ShopData.Bundles.Length];
    readonly Text[] bundlePrice = new Text[ShopData.Bundles.Length];
    readonly Text[] bundleLeft = new Text[ShopData.Bundles.Length];
    int[] shopShownDeals;
    float shopTimerTick;

    Text ssShopTitle;
    readonly Button[] ssShopBtn = new Button[3];
    readonly Text[] ssShopText = new Text[3];
    readonly Text[] ssShopPrice = new Text[3];

    static readonly Color ShopCard = new Color(0.2f, 0.11f, 0.09f, 1f);

    // Carte de boutique : cadre teinté + fond sombre.
    Image ShopFrame(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color c, out Image inner)
    {
        var frame = Box(parent, name, aMin, aMax, offMin, offMax, c);
        UISkin.Sliced(frame, "frame", 2.4f);
        inner = Box(frame.transform, "Fond", Vector2.zero, Vector2.one, new Vector2(7, 7), new Vector2(-7, -7), SkinTint(ShopCard));
        UISkin.Sliced(inner, "tile", 1.8f);
        inner.raycastTarget = false;
        return frame;
    }

    Text SectionTitle(Transform P, string txt, float y)
    {
        var t = Label(P, txt, 32, TextAnchor.MiddleLeft, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(28, y - 40), new Vector2(-28, y));
        var line = Box(P, "Ligne", new Vector2(0, 1), new Vector2(1, 1), new Vector2(28, y - 44), new Vector2(-28, y - 41), new Color(0.75f, 0.5f, 0.28f, 0.6f));
        line.raycastTarget = false;
        return t;
    }

    void BuildShopPanel(Transform R)
    {
        shopPanel = MakeRect("Boutique", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(shopPanel);
        Transform P = shopPanel.transform;
        Label(P, "BOUTIQUE", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => shopPanel.SetActive(false));

        // Cadeau du jour
        Image giftIn;
        var gift = ShopFrame(P, "Cadeau", new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -290), new Vector2(-16, -104), new Color(1f, 0.78f, 0.3f), out giftIn);
        Label(gift.transform, "Cadeau du jour", 32, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.4f), Vector2.zero, Vector2.one, new Vector2(28, 0), new Vector2(-280, -18));
        shopTimer = Label(gift.transform, "", 22, TextAnchor.LowerLeft, TextDim, Vector2.zero, Vector2.one, new Vector2(28, 20), new Vector2(-280, 0));
        shopGiftBox = MakeRect("Contenu", gift.transform, new Vector2(0.37f, 0), new Vector2(0.73f, 1), new Vector2(0, 22), new Vector2(0, -22));
        shopGiftBtn = MakeButton(gift.transform, "Récupérer", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-250, 34), new Vector2(-26, -34),
            new Color(0.2f, 0.55f, 0.25f), "", 32, out shopGiftBtnText, () => ShopResult(GameState.TakeFreeGift()));

        // Offres du jour
        SectionTitle(P, "Offres du jour", -302);
        for (int i = 0; i < ShopData.DealsPerDay; i++)
        {
            int slot = i;
            Image inner;
            dealBg[i] = ShopFrame(P, "Offre " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1), new Vector2(i == 0 ? 16 : 8, -850), new Vector2(i == 2 ? -16 : -8, -356), Color.white, out inner);
            dealHead[i] = Box(dealBg[i].transform, "Titre", new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -82), new Vector2(-10, -10), Color.white);
            UISkin.Sliced(dealHead[i], "tab", 1.4f); dealHead[i].raycastTarget = false;
            dealTitle[i] = Label(dealHead[i].transform, "", 26, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -2));
            dealTitle[i].resizeTextForBestFit = true; dealTitle[i].resizeTextMaxSize = 26; dealTitle[i].resizeTextMinSize = 16;
            dealBox[i] = MakeRect("Contenu", dealBg[i].transform, Vector2.zero, Vector2.one, new Vector2(14, 112), new Vector2(-14, -92));
            dealBtn[i] = MakeButton(dealBg[i].transform, "Acheter", new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 18), new Vector2(-16, 100),
                new Color(0.15f, 0.42f, 0.62f), "", 30, out dealBtnText[i], () => ShopResult(GameState.BuyDeal(slot)));
            PriceIcon(dealBtn[i], dealBtnText[i], "i_gem", 42);
            var sold = Box(dealBg[i].transform, "Vendu", Vector2.zero, Vector2.one, new Vector2(7, 7), new Vector2(-7, -7), new Color(0, 0, 0, 0.55f));
            sold.sprite = Round(); sold.type = Image.Type.Sliced; sold.raycastTarget = false;
            Label(sold.transform, "ACHETÉ", 40, TextAnchor.MiddleCenter, new Color(0.7f, 1f, 0.7f));
            dealSold[i] = sold.gameObject;
        }

        // Ressources
        SectionTitle(P, "Ressources", -866);
        for (int i = 0; i < ShopData.Bundles.Length; i++)
        {
            int idx = i, row = i / 3, col = i % 3;
            float top = -920 - row * 200;
            var b = ShopData.Bundles[i];
            Image inner;
            var card = ShopFrame(P, "Ressource " + i, new Vector2(col / 3f, 1), new Vector2((col + 1) / 3f, 1),
                new Vector2(col == 0 ? 16 : 8, top - 188), new Vector2(col == 2 ? -16 : -8, top), Color.Lerp(ShopData.CurrencyColors[b.currency], Color.white, 0.2f), out inner);
            bundleBtn[i] = card.gameObject.AddComponent<Button>();
            bundleBtn[i].onClick.AddListener(Sfx.Click);
            bundleBtn[i].onClick.AddListener(() => ShopResult(GameState.BuyBundle(idx)));
            UISkin.Icon(card.transform, CurrencyIcon[b.currency], new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -112), new Vector2(112, -18));
            bundleText[i] = Label(card.transform, "", 36, TextAnchor.MiddleLeft, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(118, -112), new Vector2(-10, -18));
            bundleText[i].resizeTextForBestFit = true; bundleText[i].resizeTextMaxSize = 36; bundleText[i].resizeTextMinSize = 18;
            bundleLeft[i] = Label(card.transform, "", 20, TextAnchor.UpperRight, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -44), new Vector2(-16, -12));
            var price = Box(card.transform, "Prix", new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 16), new Vector2(-16, 72), new Color(0, 0, 0, 0.45f));
            UISkin.Sliced(price, "pill"); price.raycastTarget = false;
            UISkin.Icon(price.transform, "i_gem", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-70, -24), new Vector2(-22, 24));
            bundlePrice[i] = Label(price.transform, "", 30, TextAnchor.MiddleLeft, new Color(1f, 0.75f, 0.92f), new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(-16, 0), Vector2.zero);
        }

        // Pas japonais : 8 jetons offerts chaque jour de l'édition, packs (la section n'est utile que pendant la semaine de l'événement)
        ssShopTitle = SectionTitle(P, "Pas japonais", -1330);
        for (int i = 0; i < 3; i++)
        {
            int k = i;
            Image inner;
            var card = ShopFrame(P, "Jetons " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1),
                new Vector2(i == 0 ? 16 : 8, -1580), new Vector2(i == 2 ? -16 : -8, -1384), i == 0 ? new Color(0.5f, 1f, 0.55f) : new Color(1f, 0.6f, 0.3f), out inner);
            UISkin.Icon(card.transform, "i_token", new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -112), new Vector2(112, -18));
            ssShopText[i] = Label(card.transform, "", 36, TextAnchor.MiddleLeft, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(118, -112), new Vector2(-10, -18));
            UnityEngine.Events.UnityAction act = i == 0
                ? () => { string e = SteppingStones.ClaimFree(); Toast(e ?? "+" + SteppingStones.FreeTokens + " jetons du Pas japonais"); RefreshShop(); }
                : (UnityEngine.Events.UnityAction)(() => { string e = SteppingStones.BuyPack(k - 1); Toast(e ?? "+" + SteppingStones.PackTokens[k - 1] + " jetons du Pas japonais"); RefreshShop(); });
            ssShopBtn[i] = MakeButton(card.transform, "Jetons " + i, new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 16), new Vector2(-16, 80),
                i == 0 ? new Color(0.2f, 0.55f, 0.25f) : new Color(0.15f, 0.42f, 0.62f), "", 28, out ssShopPrice[i], act);
            if (i > 0) PriceIcon(ssShopBtn[i], ssShopPrice[i], "i_gem", 38);
        }
        shopPanel.SetActive(false);
        GameState.ShopMessage += Toast;
    }

    void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;
        CloseAllPanels();
        shopPanel.SetActive(open);
        shopShownDeals = null;
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
        shopTimer.text = "Renouvellement dans " + GameState.FmtTime((long)t.TotalSeconds);
    }

    void RefreshShop()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;
        var d = GameState.Data;
        shopTimerTick = 0f;

        bool ssOn = SteppingStones.Unlocked && SteppingStones.Active;
        ssShopTitle.text = ssOn ? "Pas japonais  <size=24><color=#FFC07A>" + SteppingStones.SeasonPack + "</color>  ·  jetons " + d.ssTokens + "</size>"
            : "Pas japonais  <size=24>" + (SteppingStones.Unlocked ? "revient dans " + GameState.FmtTime((long)SteppingStones.TimeLeft.TotalSeconds) : GameState.UnlockText(SteppingStones.Unlock)) + "</size>";
        ssShopBtn[0].interactable = SteppingStones.FreeAvailable;
        ssShopText[0].text = "x" + SteppingStones.FreeTokens;
        ssShopPrice[0].text = SteppingStones.FreeAvailable ? "GRATUIT" : ssOn ? "Demain" : "Hors édition";
        for (int i = 0; i < 2; i++)
        {
            ssShopBtn[i + 1].interactable = ssOn && d.gems >= SteppingStones.PackGems[i];
            ssShopText[i + 1].text = "x" + SteppingStones.PackTokens[i];
            ssShopPrice[i + 1].text = SteppingStones.PackGems[i].ToString();
        }

        // Le contenu (cases) n'est reconstruit que s'il a changé.
        bool rebuild = shopShownDeals == null || shopShownDeals.Length != ShopData.DealsPerDay;
        if (!rebuild) for (int i = 0; i < ShopData.DealsPerDay; i++) if (shopShownDeals[i] != d.shopDeals[i]) rebuild = true;
        if (rebuild)
        {
            shopShownDeals = (int[])d.shopDeals.Clone();
            FillRewards(shopGiftBox, ShopData.FreeGift, 3, 26);
            for (int i = 0; i < ShopData.DealsPerDay; i++) FillRewards(dealBox[i], GameState.DealContent(i), 2, 26);
        }
        shopGiftBtn.interactable = !d.shopFreeTaken;
        shopGiftBtnText.text = d.shopFreeTaken ? "Déjà pris" : "GRATUIT";

        int price = GameState.DealPrice;
        for (int i = 0; i < ShopData.DealsPerDay; i++)
        {
            var deal = ShopData.Deals[d.shopDeals[i]];
            bool bought = d.shopDealBought[i];
            dealBg[i].color = Color.Lerp(deal.color, Color.white, 0.15f);
            dealHead[i].color = UISkin.Vivid(new Color(deal.color.r * 0.55f, deal.color.g * 0.55f, deal.color.b * 0.55f));
            dealTitle[i].text = deal.name;
            dealBtn[i].interactable = !bought && d.gems >= price;
            dealBtnText[i].text = price.ToString();
            dealSold[i].SetActive(bought);
        }

        for (int i = 0; i < ShopData.Bundles.Length; i++)
        {
            var b = ShopData.Bundles[i];
            int left = b.perDay - d.shopBundleBuys[i];
            bundleBtn[i].interactable = left > 0 && d.gems >= b.price;
            bundleText[i].text = RewardAmount(b.currency, b.amount);
            bundleLeft[i].text = left + "/" + b.perDay;
            bundlePrice[i].text = b.price.ToString();
        }
    }
}
