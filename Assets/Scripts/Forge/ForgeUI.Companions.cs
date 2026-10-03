using UnityEngine;
using UnityEngine.UI;

// Écran des compagnons : œufs, couveuses, compagnons équipés et collection.
public partial class ForgeUI
{
    GameObject compPanel;
    Text compEggs, compShells, compSummonLvl;
    Button summon1Btn, summon15Btn;
    readonly Image[] incBg = new Image[ProgressionData.IncubatorMax];
    readonly Image[] incEgg = new Image[ProgressionData.IncubatorMax];
    readonly Text[] incText = new Text[ProgressionData.IncubatorMax];
    readonly Image[] eqPetBg = new Image[3];
    readonly Text[] eqPetText = new Text[3];
    readonly Image[] petTileBg = new Image[25];
    readonly Text[] petTileText = new Text[25];
    float compTimer;
    static Sprite circleSprite;

    internal static Sprite Circle()
    {
        if (circleSprite != null) return circleSprite;
        const int n = 64;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f - n / 2f) / (n / 2f), dy = (y + 0.5f - n / 2f) / (n / 2f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((1f - d) * 20f);
                float shade = 0.75f + 0.25f * Mathf.Clamp01(1f - Mathf.Sqrt((dx + 0.35f) * (dx + 0.35f) + (dy - 0.35f) * (dy - 0.35f)));
                t.SetPixel(x, y, new Color(shade, shade, shade, a));
            }
        t.Apply();
        circleSprite = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        return circleSprite;
    }

    void BuildCompanionPanel(Transform R)
    {
        compPanel = MakeRect("Compagnons", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        var bg = compPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.025f, 0.03f, 0.98f);
        Transform P = compPanel.transform;

        Label(P, "COMPAGNONS", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.4f, 0.12f, 0.08f), "X", 40, out closeT, () => compPanel.SetActive(false));
        compEggs = Label(P, "", 28, TextAnchor.UpperCenter, TextMain, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -130), new Vector2(-20, -88));

        // Invocation d'œufs avec les coquilles
        var srow = Box(P, "Invocation", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -228), new Vector2(-12, -136), new Color(0.1f, 0.05f, 0.05f, 0.95f));
        compShells = Label(srow.transform, "", 30, TextAnchor.MiddleLeft, new Color(0.95f, 0.88f, 0.75f), new Vector2(0, 0), new Vector2(0.3f, 1), new Vector2(18, 0), Vector2.zero);
        Text s1t, s15t;
        summon1Btn = MakeButton(srow.transform, "Invoquer x1", new Vector2(0.3f, 0), new Vector2(0.52f, 1), new Vector2(4, 10), new Vector2(-4, -10),
            new Color(0.15f, 0.4f, 0.65f), "Invoquer x1\n<size=20>" + ProgressionData.EggSummonCost + " coquilles</size>", 24, out s1t, () => OnSummonEggs(1));
        summon15Btn = MakeButton(srow.transform, "Invoquer x15", new Vector2(0.52f, 0), new Vector2(0.74f, 1), new Vector2(4, 10), new Vector2(-4, -10),
            new Color(0.15f, 0.4f, 0.65f), "Invoquer x15\n<size=20>" + ProgressionData.EggSummonCost * 15 + " coquilles</size>", 24, out s15t, () => OnSummonEggs(15));
        compSummonLvl = Label(srow.transform, "", 22, TextAnchor.MiddleLeft, TextDim, new Vector2(0.74f, 0), new Vector2(1, 1), new Vector2(10, 0), new Vector2(-8, 0));

        // Couveuses
        Label(P, "Couveuses", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -270), new Vector2(0, -233));
        for (int i = 0; i < ProgressionData.IncubatorMax; i++)
        {
            int slot = i;
            Text t;
            var b = MakeButton(P, "Couveuse " + i, new Vector2(i / 4f, 1), new Vector2((i + 1) / 4f, 1), new Vector2(12, -535), new Vector2(-12, -275),
                new Color(0.12f, 0.06f, 0.06f), "", 26, out t, () => OnIncubator(slot));
            incBg[i] = b.GetComponent<Image>();
            var egg = new GameObject("Œuf", typeof(RectTransform)).AddComponent<Image>();
            egg.transform.SetParent(b.transform, false);
            var ert = egg.rectTransform;
            ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0.62f);
            ert.sizeDelta = new Vector2(110, 140);
            egg.sprite = Circle();
            egg.raycastTarget = false;
            incEgg[i] = egg;
            t.alignment = TextAnchor.LowerCenter;
            t.rectTransform.offsetMin = new Vector2(6, 10);
            t.transform.SetAsLastSibling();
            incText[i] = t;
        }

        // Compagnons équipés
        Label(P, "Équipés (bonus de dégâts et de vie)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -590), new Vector2(0, -550));
        for (int i = 0; i < 3; i++)
        {
            eqPetBg[i] = Box(P, "Équipé " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1), new Vector2(12, -745), new Vector2(-12, -595), Panel);
            eqPetBg[i].gameObject.AddComponent<Outline>().effectDistance = new Vector2(4, -4);
            eqPetText[i] = Label(eqPetBg[i].transform, "", 26, TextAnchor.MiddleCenter, TextMain, Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4));
        }

        // Collection
        Label(P, "Collection (touche pour équiper)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -795), new Vector2(0, -757));
        var grid = MakeRect("Collection", P, Vector2.zero, new Vector2(1, 1), new Vector2(12, 12), new Vector2(-12, -800));
        for (int i = 0; i < 25; i++)
        {
            int id = i, row = i / 5, col = i % 5;
            Text t;
            var b = MakeButton(grid, "Compagnon " + i, new Vector2(col / 5f, 1f - (row + 1) / 5f), new Vector2((col + 1) / 5f, 1f - row / 5f),
                new Vector2(5, 5), new Vector2(-5, -5), Panel, "", 21, out t, () => OnPetTile(id));
            petTileBg[i] = b.GetComponent<Image>();
            var ol = b.gameObject.GetComponent<Outline>();
            ol.effectDistance = new Vector2(3, -3);
            petTileText[i] = t;
        }

        compPanel.SetActive(false);
        GameState.PetMessage += Toast;
    }

    void ToggleCompanions()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (mountPanel != null) mountPanel.SetActive(false);
        if (techPanel != null) techPanel.SetActive(false);
        if (dungPanel != null) dungPanel.SetActive(false);
        compPanel.SetActive(!compPanel.activeSelf);
        if (compPanel.activeSelf) RefreshCompanions();
    }

    void OnSummonEggs(int count)
    {
        var got = GameState.SummonEggs(count);
        if (got == null) { Toast("Pas assez de coquilles (donjon des œufs, ligue, guerre de clan, boss)"); return; }
        var sb = new System.Text.StringBuilder("Œufs obtenus : ");
        bool first = true;
        for (int r = 5; r >= 0; r--)
        {
            if (got[r] == 0) continue;
            if (!first) sb.Append(", ");
            first = false;
            sb.Append(got[r] + " " + ProgressionData.Rarities[r]);
        }
        Toast(sb.ToString());
        RefreshCompanions();
    }

    void OnIncubator(int slot)
    {
        var d = GameState.Data;
        if (slot >= d.incubatorSlots)
        {
            if (slot != d.incubatorSlots) { Toast("Débloque d'abord la couveuse précédente"); return; }
            if (!GameState.BuyIncubator()) Toast("Pas assez de gemmes");
        }
        else if (d.incubators[slot].rarity < 0)
        {
            if (!GameState.StartIncubation(slot)) Toast("Aucun œuf : invoque-en avec des coquilles");
        }
        else if (GameState.IncubatorSecondsLeft(slot) <= 0) GameState.Hatch(slot);
        else if (!GameState.SpeedUpIncubator(slot)) Toast("Pas assez de gemmes");
        RefreshCompanions();
    }

    void OnPetTile(int id)
    {
        string err = GameState.TogglePetEquip(id);
        if (err != null) Toast(err);
        RefreshCompanions();
    }

    void UpdateCompanions()
    {
        if (compPanel == null || !compPanel.activeSelf) return;
        compTimer -= Time.deltaTime;
        if (compTimer > 0f) return;
        compTimer = 0.5f;
        RefreshCompanions();
    }

    static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);

    void RefreshCompanions()
    {
        if (compPanel == null || !compPanel.activeSelf) return;
        var d = GameState.Data;

        var sb = new System.Text.StringBuilder("Œufs en réserve :  ");
        bool any = false;
        for (int r = 0; r < 6; r++)
        {
            if (d.eggs[r] <= 0) continue;
            any = true;
            sb.Append("<color=" + Hex(ProgressionData.RarityColors[r]) + ">" + ProgressionData.Rarities[r] + " ×" + d.eggs[r] + "</color>   ");
        }
        if (!any) sb.Append("aucun : invoque-en avec des coquilles");
        compEggs.text = sb.ToString();

        compShells.text = "Coquilles\n<b><size=36>" + GameState.Fmt(d.eggshells) + "</size></b>";
        summon1Btn.interactable = d.eggshells >= ProgressionData.EggSummonCost;
        summon15Btn.interactable = d.eggshells >= ProgressionData.EggSummonCost * 15;
        var odds = GameState.EggOdds;
        var ob = new System.Text.StringBuilder("Invocation niv. " + (d.eggSummonLevel + 1) + "  (" + d.eggSummonProgress + "/" + GameState.EggSummonRequired + ")\n<size=18>");
        for (int r = 0; r < 6; r++)
            if (odds[r] > 0f) ob.Append("<color=" + Hex(ProgressionData.RarityColors[r]) + ">" + odds[r].ToString("0.#") + "%</color> ");
        compSummonLvl.text = ob.Append("</size>").ToString();

        for (int i = 0; i < ProgressionData.IncubatorMax; i++)
        {
            var inc = d.incubators[i];
            if (i >= d.incubatorSlots)
            {
                incBg[i].color = new Color(0.08f, 0.05f, 0.05f);
                incEgg[i].color = new Color(0.25f, 0.2f, 0.2f, 0.5f);
                incText[i].text = i == d.incubatorSlots ? "Débloquer\n<size=22>" + ProgressionData.IncubatorCost[i] + " gemmes</size>" : "Verrouillée";
            }
            else if (inc.rarity < 0)
            {
                incBg[i].color = new Color(0.12f, 0.06f, 0.06f);
                incEgg[i].color = new Color(0.3f, 0.25f, 0.25f, 0.35f);
                incText[i].text = GameState.TotalEggs() > 0 ? "Couver un œuf" : "Vide";
            }
            else
            {
                var c = ProgressionData.RarityColors[inc.rarity];
                long left = GameState.IncubatorSecondsLeft(i);
                incBg[i].color = left <= 0 ? new Color(0.12f, 0.3f, 0.12f) : new Color(0.12f, 0.06f, 0.06f);
                float pulse = left <= 0 ? 0.85f + 0.15f * Mathf.Sin(Time.time * 6f) : 1f;
                incEgg[i].color = new Color(c.r * pulse, c.g * pulse, c.b * pulse, 1f);
                incText[i].text = left <= 0 ? "<b>Éclore !</b>" : GameState.FmtTime(left) + "\n<size=20>Finir : " + GameState.IncubatorSpeedUpCost(i) + " gemmes</size>";
            }
        }

        for (int i = 0; i < 3; i++)
        {
            var p = GameState.FindPet(d.equippedPets[i]);
            var ol = eqPetBg[i].GetComponent<Outline>();
            if (p == null)
            {
                eqPetBg[i].color = Panel;
                ol.effectColor = new Color(0.3f, 0.15f, 0.1f);
                eqPetText[i].text = "<color=#776655>emplacement libre</color>";
                continue;
            }
            var def = ProgressionData.Pets[p.id];
            var c = ProgressionData.RarityColors[def.rarity];
            eqPetBg[i].color = new Color(c.r * 0.25f, c.g * 0.25f, c.b * 0.25f, 0.95f);
            ol.effectColor = c;
            eqPetText[i].text = "<b>" + def.name + "</b>\nNiv. " + p.level + "\n<size=22>ATQ +" + GameState.Fmt(GameState.PetDamage(p)) + "  PV +" + GameState.Fmt(GameState.PetHealth(p)) + "</size>"
                + PetSubsText(p);
        }

        for (int i = 0; i < 25; i++)
        {
            var def = ProgressionData.Pets[i];
            var p = GameState.FindPet(i);
            var c = ProgressionData.RarityColors[def.rarity];
            var ol = petTileBg[i].GetComponent<Outline>();
            if (p == null)
            {
                petTileBg[i].color = new Color(0.07f, 0.05f, 0.05f);
                ol.effectColor = new Color(c.r * 0.4f, c.g * 0.4f, c.b * 0.4f);
                petTileText[i].text = "<color=#665555>???\n" + ProgressionData.Rarities[def.rarity] + "</color>";
                continue;
            }
            bool eq = GameState.IsPetEquipped(i);
            petTileBg[i].color = eq ? new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, 1f) : new Color(c.r * 0.22f, c.g * 0.22f, c.b * 0.22f, 1f);
            ol.effectColor = c;
            petTileText[i].text = def.name + "\n<b><size=28>Niv. " + p.level + "</size></b>\n<size=18>" + p.copies + "/" + GameState.CopiesForNext(p.level) + (eq ? "  ✔" : "") + "</size>";
        }
    }

    static string PetSubsText(OwnedPet p)
    {
        if (p.subs == null || p.subs.Length == 0) return "";
        var sb = new System.Text.StringBuilder("\n<size=19><color=#BFD8FF>");
        for (int i = 0; i < p.subs.Length; i++)
        {
            if (i > 0) sb.Append(" · ");
            sb.Append("+" + GameState.FmtPct(p.subVals[i]) + " " + GameState.SubNames[p.subs[i]]);
        }
        return sb.Append("</color></size>").ToString();
    }
}
