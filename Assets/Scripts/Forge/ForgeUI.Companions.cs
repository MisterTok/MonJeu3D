using UnityEngine;
using UnityEngine.UI;

// Écran des compagnons, épuré façon Forge Master : invocation, couveuses, 3 équipés, collection d'icônes.
// Toucher un compagnon ouvre sa fiche (statistiques, équiper / retirer).
public partial class ForgeUI
{
    GameObject compPanel;
    SummonBar compSummon;
    Text compEggs;
    readonly Image[] incBg = new Image[ProgressionData.IncubatorMax];
    readonly Image[] incEgg = new Image[ProgressionData.IncubatorMax];
    readonly Text[] incText = new Text[ProgressionData.IncubatorMax];
    readonly Tile[] eqPetTile = new Tile[3];
    readonly Tile[] petTile = new Tile[25];
    float compTimer;

    void BuildCompanionPanel(Transform R)
    {
        compPanel = MakeRect("Compagnons", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        compPanel.AddComponent<Image>().color = new Color(0.05f, 0.025f, 0.03f, 1f);
        Transform P = compPanel.transform;
        Label(P, "COMPAGNONS", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.4f, 0.12f, 0.08f), "X", 40, out closeT, () => compPanel.SetActive(false));

        compSummon = MakeSummonBar(P, -90, new Color(0.15f, 0.4f, 0.65f), () => OnSummonEggs(1), () => OnSummonEggs(15),
            () => ShowInfo("Chances des œufs", OddsText(GameState.EggOdds)));

        // Couveuses : l'œuf, et en dessous l'état (couver, temps, éclore)
        compEggs = Label(P, "", 24, TextAnchor.MiddleLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(24, -240), new Vector2(-20, -200));
        for (int i = 0; i < ProgressionData.IncubatorMax; i++)
        {
            int slot = i;
            Text t;
            var b = MakeButton(P, "Couveuse " + i, new Vector2(i / 4f, 1), new Vector2((i + 1) / 4f, 1), new Vector2(10, -440), new Vector2(-10, -245),
                new Color(0.12f, 0.06f, 0.06f), "", 24, out t, () => OnIncubator(slot));
            incBg[i] = b.GetComponent<Image>();
            incBg[i].sprite = Round(); incBg[i].type = Image.Type.Sliced;
            var egg = new GameObject("Œuf", typeof(RectTransform)).AddComponent<Image>();
            egg.transform.SetParent(b.transform, false);
            var ert = egg.rectTransform;
            ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0.6f);
            ert.sizeDelta = new Vector2(90, 115);
            egg.sprite = Circle();
            egg.raycastTarget = false;
            incEgg[i] = egg;
            t.alignment = TextAnchor.LowerCenter;
            t.rectTransform.offsetMin = new Vector2(4, 10);
            t.transform.SetAsLastSibling();
            incText[i] = t;
        }

        // Compagnons équipés : 3 grandes tuiles
        for (int i = 0; i < 3; i++)
        {
            int slot = i;
            eqPetTile[i] = MakeTile(P, new Vector2(0.1f + i * 0.27f, 1), new Vector2(0.1f + (i + 1) * 0.27f, 1), new Vector2(10, -720), new Vector2(-10, -460),
                () => { int id = GameState.Data.equippedPets[slot]; if (id >= 0) OpenPet(id); }, true);
        }

        // Collection : 5 × 5 icônes
        var grid = MakeRect("Collection", P, Vector2.zero, new Vector2(1, 1), new Vector2(12, 10), new Vector2(-12, -740));
        for (int i = 0; i < 25; i++)
        {
            int id = i, row = i / 5, col = i % 5;
            petTile[i] = MakeTile(grid, new Vector2(col / 5f, 1f - (row + 1) / 5f), new Vector2((col + 1) / 5f, 1f - row / 5f),
                new Vector2(5, 5), new Vector2(-5, -5), () => OpenPet(id));
        }

        compPanel.SetActive(false);
        GameState.PetMessage += Toast;
    }

    void ToggleCompanions()
    {
        bool open = !compPanel.activeSelf;
        CloseAllPanels();
        compPanel.SetActive(open);
        RefreshCompanions();
    }

    void OnSummonEggs(int count)
    {
        var got = GameState.SummonEggs(count);
        if (got == null) { Toast("Pas assez de coquilles"); return; }
        Sfx.Summon();
        var sb = new System.Text.StringBuilder("Œufs : ");
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

    // Fiche d'un compagnon : statistiques et bouton équiper / retirer.
    void OpenPet(int id)
    {
        var def = ProgressionData.Pets[id];
        var p = GameState.FindPet(id);
        var c = ProgressionData.RarityColors[def.rarity];
        var icon = ItemIcons.GetCreature(def.model, c, 0.12f);
        if (p == null) { ShowDetail(def.name, def.rarity, icon, null, c, -1, 0, 0, null, null, null); return; }
        string body = "ATQ <color=#FFC07A>+" + GameState.Fmt(GameState.PetDamage(p)) + "</color>     PV <color=#FFC07A>+" + GameState.Fmt(GameState.PetHealth(p)) + "</color>";
        if (p.subs != null)
            for (int i = 0; i < p.subs.Length; i++) body += "\n" + SubRich(p.subs[i], p.subVals[i]);
        bool eq = GameState.IsPetEquipped(id);
        ShowDetail(def.name, def.rarity, icon, null, c, p.level, p.copies, GameState.CopiesForNext(p.level), body,
            eq ? "RETIRER" : "ÉQUIPER", () => { string e = GameState.TogglePetEquip(id); if (e != null) Toast(e); RefreshCompanions(); },
            eq ? new Color(0.5f, 0.18f, 0.12f) : (Color?)null);
    }

    void UpdateCompanions()
    {
        if (compPanel == null || !compPanel.activeSelf) return;
        compTimer -= Time.deltaTime;
        if (compTimer > 0f) return;
        compTimer = 0.5f;
        RefreshCompanions();
    }

    void RefreshCompanions()
    {
        if (compPanel == null || !compPanel.activeSelf) return;
        var d = GameState.Data;
        SetSummonBar(compSummon, "Coquilles", d.eggshells, ProgressionData.EggSummonCost, 1, 15, d.eggSummonLevel, d.eggSummonProgress, GameState.EggSummonRequired);

        var sb = new System.Text.StringBuilder("Œufs ");
        bool any = false;
        for (int r = 0; r < 6; r++)
        {
            if (d.eggs[r] <= 0) continue;
            any = true;
            sb.Append("  <color=" + Hex(ProgressionData.RarityColors[r]) + ">● " + d.eggs[r] + "</color>");
        }
        compEggs.text = any ? sb.ToString() : "Œufs  <color=#776655>aucun</color>";

        for (int i = 0; i < ProgressionData.IncubatorMax; i++)
        {
            var inc = d.incubators[i];
            if (i >= d.incubatorSlots)
            {
                incBg[i].color = new Color(0.08f, 0.05f, 0.05f);
                incEgg[i].color = new Color(0.25f, 0.2f, 0.2f, 0.4f);
                incText[i].text = i == d.incubatorSlots ? "<size=22>" + ProgressionData.IncubatorCost[i] + " gemmes</size>" : "<color=#665555>—</color>";
            }
            else if (inc.rarity < 0)
            {
                incBg[i].color = new Color(0.12f, 0.06f, 0.06f);
                incEgg[i].color = new Color(0.3f, 0.25f, 0.25f, 0.3f);
                incText[i].text = GameState.TotalEggs() > 0 ? "<color=#9FE0A0>Couver</color>" : "";
            }
            else
            {
                var c = ProgressionData.RarityColors[inc.rarity];
                long left = GameState.IncubatorSecondsLeft(i);
                incBg[i].color = left <= 0 ? new Color(0.12f, 0.3f, 0.12f) : new Color(0.12f, 0.06f, 0.06f);
                float pulse = left <= 0 ? 0.85f + 0.15f * Mathf.Sin(Time.time * 6f) : 1f;
                incEgg[i].color = new Color(c.r * pulse, c.g * pulse, c.b * pulse, 1f);
                incText[i].text = left <= 0 ? "<b>Éclore !</b>" : GameState.FmtTime(left);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            var p = GameState.FindPet(d.equippedPets[i]);
            if (p == null) { SetEmptySlot(eqPetTile[i]); LockSlot(eqPetTile[i], GameState.UnlockPetSlot[i]); continue; }
            var def = ProgressionData.Pets[p.id];
            var c = ProgressionData.RarityColors[def.rarity];
            SetTile(eqPetTile[i], def.rarity, ItemIcons.GetCreature(def.model, c, 0.12f), null, c, p.level, p.copies, GameState.CopiesForNext(p.level), false);
        }

        for (int i = 0; i < 25; i++)
        {
            var def = ProgressionData.Pets[i];
            var p = GameState.FindPet(i);
            var c = ProgressionData.RarityColors[def.rarity];
            SetTile(petTile[i], def.rarity, ItemIcons.GetCreature(def.model, c, 0.12f), null, c,
                p == null ? -1 : p.level, p == null ? 0 : p.copies, p == null ? 0 : GameState.CopiesForNext(p.level), p != null && GameState.IsPetEquipped(i));
        }
    }
}
