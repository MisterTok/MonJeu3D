using UnityEngine;
using UnityEngine.UI;

// Écran des skins payants (sous-onglet du Héros) : skin porté en grand, collection des 36 skins, fiche au toucher.
public partial class ForgeUI
{
    GameObject skinsPanel;
    Tile skinEqTile;
    Text skinEqText;
    Tile[] skinTile;
    Text[] skinPrice;
    Text skinEventText;

    static Texture SkinIcon(string id) => Resources.Load<Texture2D>("ForgeSprites/SkinIcons/" + id);

    void BuildHeroSkinsPanel(Transform R)
    {
        skinsPanel = MakeRect("Skins", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        SkinPanel(skinsPanel);
        Transform P = skinsPanel.transform;
        Label(P, "SKINS", 48, TextAnchor.UpperCenter, TitleColor, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => skinsPanel.SetActive(false));

        // Bouton de l'événement qui fait gagner les skins (Stepping Stones de FM)
        MakeButton(P, "Pas japonais", new Vector2(0.38f, 1), new Vector2(0.95f, 1), new Vector2(0, -460), new Vector2(0, -235),
            new Color(0.6f, 0.22f, 0.08f), "", 34, out skinEventText, ToggleStones);
        skinEqTile = MakeTile(P, new Vector2(0.06f, 1), new Vector2(0.06f, 1), new Vector2(0, -470), new Vector2(260, -220),
            () => { if (HeroSkins.Find(GameState.Data.skinEquipped) != null) OpenHeroSkin(GameState.Data.skinEquipped); }, true);
        skinEqText = Label(P, "", 28, TextAnchor.UpperCenter, TextMain, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -545), new Vector2(-20, -478));

        int n = HeroSkins.All.Length;
        skinTile = new Tile[n]; skinPrice = new Text[n];
        for (int i = 0; i < n; i++)
        {
            int idx = i, row = i / 6, col = i % 6;
            float top = -555 - row * 175;
            skinTile[i] = MakeTile(P, new Vector2(col / 6f, 1), new Vector2((col + 1) / 6f, 1),
                new Vector2(col == 0 ? 14 : 4, top - 168), new Vector2(col == 5 ? -14 : -4, top), () => OpenHeroSkin(HeroSkins.All[idx].id));
            skinPrice[i] = Label(skinTile[i].bg.transform, "", 20, TextAnchor.LowerCenter, new Color(1f, 0.75f, 0.9f), Vector2.zero, Vector2.one, new Vector2(2, 6), new Vector2(-2, 0));
        }
        skinsPanel.SetActive(false);
    }

    void ToggleHeroSkins()
    {
        CloseAllPanels();
        skinsPanel.SetActive(true);
        RefreshHeroSkins();
    }

    void OpenHeroSkin(string id)
    {
        var d = HeroSkins.Find(id);
        if (d == null) return;
        bool owned = HeroSkins.Owned(id), on = GameState.Data.skinEquipped == id;
        var c = ProgressionData.RarityColors[d.rarity];
        bool season = SteppingStones.InSeason(id);
        string body = "<color=#A89C94>" + d.pack + "</color>\n\nDégâts et vie <color=#FFC07A>+" + GameState.FmtPct(HeroSkins.Bonus(d.rarity)) + "</color> quand il est porté"
            + (owned ? "" : season ? "\n\nÀ gagner cette semaine au <color=#FFC07A>Pas japonais</color> : " + SteppingStones.SkinCost(d) + " éclats"
                                   : "\n\n<color=#998877>Reviendra lors d'une prochaine saison du Pas japonais</color>");
        string action = owned ? (on ? "RETIRER" : "PORTER") : season ? "PAS JAPONAIS" : null;
        ShowDetail(d.name, d.rarity, SkinIcon(id), "?", c, 1, 1, 1, body, action, () =>
        {
            if (owned) { HeroSkins.Toggle(id); RefreshHeroSkins(); }
            else ToggleStones();
        }, on ? new Color(0.5f, 0.18f, 0.12f) : owned ? (Color?)null : new Color(0.6f, 0.22f, 0.08f));
        detailBarRoot.SetActive(false);
        if (!owned && !season) dpBtn.gameObject.SetActive(false);
    }

    void RefreshHeroSkins()
    {
        if (skinsPanel == null || !skinsPanel.activeSelf) return;
        var gd = GameState.Data;
        skinEventText.text = SteppingStones.Unlocked
            ? "PAS JAPONAIS\n<size=24>" + SteppingStones.SeasonPack + "</size>\n<size=22>Jetons " + gd.ssTokens + "  ·  Éclats " + gd.ssEclats + "</size>"
            : "PAS JAPONAIS\n<size=22>" + GameState.UnlockText(SteppingStones.Unlock) + "</size>";
        var eq = HeroSkins.Find(GameState.Data.skinEquipped);
        if (eq == null || !HeroSkins.Owned(eq.id))
        {
            SetEmptySlot(skinEqTile);
            skinEqText.text = "<color=#776655>Aucun skin : le héros montre son équipement</color>";
        }
        else
        {
            SetTile(skinEqTile, eq.rarity, SkinIcon(eq.id), "?", ProgressionData.RarityColors[eq.rarity], 1, 1, 1, false);
            skinEqTile.barBg.gameObject.SetActive(false);
            skinEqText.text = "<b>" + eq.name + "</b>\n<size=24><color=#FFC07A>+" + GameState.FmtPct(HeroSkins.Bonus(eq.rarity)) + "</color> dégâts et vie</size>";
        }
        for (int i = 0; i < HeroSkins.All.Length; i++)
        {
            var d = HeroSkins.All[i];
            bool owned = HeroSkins.Owned(d.id);
            var c = ProgressionData.RarityColors[d.rarity];
            // Les skins non achetés restent visibles en couleur (vitrine), avec leur prix.
            SetTile(skinTile[i], d.rarity, SkinIcon(d.id), "?", c, 1, 1, 1, GameState.Data.skinEquipped == d.id && owned);
            skinTile[i].barBg.gameObject.SetActive(false);
            skinPrice[i].text = owned ? "" : SteppingStones.InSeason(d.id) ? "<b>Cette semaine</b>" : "";
        }
    }
}
