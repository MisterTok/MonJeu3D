using UnityEngine;
using UnityEngine.UI;

// Écran des compétences : invocation avec les tickets, 3 compétences équipées et collection de 18.
public partial class ForgeUI
{
    GameObject skillPanel;
    Text skillTickets, skillSummonLvl, skillPassive;
    Button skSummonSmall, skSummonBig;
    Text skSummonSmallText, skSummonBigText;
    readonly Image[] eqSkillBg = new Image[3];
    readonly Text[] eqSkillText = new Text[3];
    readonly Image[] skillTileBg = new Image[18];
    readonly Text[] skillTileText = new Text[18];

    void BuildSkillPanel(Transform R)
    {
        skillPanel = MakeRect("Compétences", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        skillPanel.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.06f, 0.98f);
        Transform P = skillPanel.transform;
        Label(P, "COMPÉTENCES", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => skillPanel.SetActive(false));
        skillPassive = Label(P, "", 24, TextAnchor.UpperCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -134), new Vector2(-30, -86));

        // Invocation
        var srow = Box(P, "Invocation", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -240), new Vector2(-12, -142), Panel);
        skillTickets = Label(srow.transform, "", 30, TextAnchor.MiddleLeft, new Color(0.95f, 0.88f, 0.75f), new Vector2(0, 0), new Vector2(0.3f, 1), new Vector2(18, 0), Vector2.zero);
        skSummonSmall = MakeButton(srow.transform, "Invoquer petit", new Vector2(0.3f, 0), new Vector2(0.52f, 1), new Vector2(4, 10), new Vector2(-4, -10),
            new Color(0.2f, 0.5f, 0.3f), "", 24, out skSummonSmallText, () => OnSummonSkills(SkillData.SummonSmall));
        skSummonBig = MakeButton(srow.transform, "Invoquer grand", new Vector2(0.52f, 0), new Vector2(0.74f, 1), new Vector2(4, 10), new Vector2(-4, -10),
            new Color(0.2f, 0.5f, 0.3f), "", 24, out skSummonBigText, () => OnSummonSkills(SkillData.SummonBig));
        skillSummonLvl = Label(srow.transform, "", 22, TextAnchor.MiddleLeft, TextDim, new Vector2(0.74f, 0), new Vector2(1, 1), new Vector2(10, 0), new Vector2(-8, 0));

        // Équipées
        Label(P, "Équipées (se lancent seules en combat)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -295), new Vector2(0, -255));
        for (int i = 0; i < 3; i++)
        {
            int slot = i;
            Text t;
            var b = MakeButton(P, "Équipée " + i, new Vector2(i / 3f, 1), new Vector2((i + 1) / 3f, 1), new Vector2(12, -560), new Vector2(-12, -300),
                Panel, "", 23, out t, () => OnEquippedSkill(slot));
            eqSkillBg[i] = b.GetComponent<Image>();
            b.GetComponent<Outline>().effectDistance = new Vector2(4, -4);
            t.rectTransform.offsetMin = new Vector2(10, 8); t.rectTransform.offsetMax = new Vector2(-10, -8);
            eqSkillText[i] = t;
        }

        // Collection
        Label(P, "Collection (touche pour équiper ou retirer)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -615), new Vector2(0, -575));
        var grid = MakeRect("Collection", P, Vector2.zero, new Vector2(1, 1), new Vector2(12, 12), new Vector2(-12, -620));
        for (int i = 0; i < 18; i++)
        {
            int id = i, row = i / 3, col = i % 3;
            Text t;
            var b = MakeButton(grid, "Compétence " + i, new Vector2(col / 3f, 1f - (row + 1) / 6f), new Vector2((col + 1) / 3f, 1f - row / 6f),
                new Vector2(5, 5), new Vector2(-5, -5), Panel, "", 21, out t, () => OnSkillTile(id));
            skillTileBg[i] = b.GetComponent<Image>();
            b.GetComponent<Outline>().effectDistance = new Vector2(3, -3);
            skillTileText[i] = t;
        }
        skillPanel.SetActive(false);
        GameState.SkillMessage += Toast;
    }

    void ToggleSkills()
    {
        bool open = !skillPanel.activeSelf;
        CloseAllPanels();
        skillPanel.SetActive(open);
        RefreshSkills();
    }

    void OnSummonSkills(int count)
    {
        if (GameState.SummonSkills(count) == null) Toast("Pas assez de tickets (Crypte des grimoires, boss)");
        RefreshSkills();
    }

    void OnEquippedSkill(int slot)
    {
        int id = GameState.Data.equippedSkills[slot];
        if (id < 0) { Toast("Touche une compétence de la collection pour l'équiper"); return; }
        GameState.ToggleSkillEquip(id);
        RefreshSkills();
    }

    void OnSkillTile(int id)
    {
        string err = GameState.ToggleSkillEquip(id);
        if (err != null) Toast(err);
        RefreshSkills();
    }

    void RefreshSkills()
    {
        if (skillPanel == null || !skillPanel.activeSelf) return;
        var d = GameState.Data;
        long c = GameState.SkillSummonCost;
        skillPassive.text = "Toutes les compétences possédées donnent un bonus permanent :  <color=#FFC07A>+" + GameState.Fmt(GameState.SkillPassiveDamage())
            + " ATQ   +" + GameState.Fmt(GameState.SkillPassiveHealth()) + " PV</color>";
        skillTickets.text = "Tickets\n<b><size=36>" + GameState.Fmt(d.skillTickets) + "</size></b>";
        skSummonSmallText.text = "Invoquer x" + SkillData.SummonSmall + "\n<size=20>" + c * SkillData.SummonSmall + " tickets</size>";
        skSummonBigText.text = "Invoquer x" + SkillData.SummonBig + "\n<size=20>" + c * SkillData.SummonBig + " tickets</size>";
        skSummonSmall.interactable = d.skillTickets >= c * SkillData.SummonSmall;
        skSummonBig.interactable = d.skillTickets >= c * SkillData.SummonBig;
        var odds = GameState.SkillOdds;
        var ob = new System.Text.StringBuilder("Invocation niv. " + (d.skillSummonLevel + 1) + "  (" + d.skillSummonProgress + "/" + GameState.SkillSummonRequired + ")\n<size=18>");
        for (int r = 0; r < 6; r++)
            if (odds[r] > 0.0001f) ob.Append("<color=" + Hex(ProgressionData.RarityColors[r]) + ">" + odds[r].ToString("0.##") + "%</color> ");
        skillSummonLvl.text = ob.Append("</size>").ToString();

        for (int i = 0; i < 3; i++)
        {
            int id = d.equippedSkills[i];
            var s = GameState.FindSkill(id);
            var ol = eqSkillBg[i].GetComponent<Outline>();
            if (s == null)
            {
                eqSkillBg[i].color = Panel;
                ol.effectColor = new Color(0.3f, 0.15f, 0.1f);
                eqSkillText[i].text = "<color=#776655>emplacement libre</color>";
                continue;
            }
            var def = SkillData.Skills[id];
            var rc = ProgressionData.RarityColors[def.rarity];
            eqSkillBg[i].color = new Color(rc.r * 0.25f, rc.g * 0.25f, rc.b * 0.25f, 0.95f);
            ol.effectColor = rc;
            eqSkillText[i].text = "<b>" + def.name + "</b>\n<size=20><color=" + Hex(rc) + ">" + ProgressionData.Rarities[def.rarity] + "</color> · niv. " + s.level
                + " (" + s.copies + "/" + GameState.SkillCopiesForNext(s.level) + ")</size>\n<size=20>" + GameState.SkillEffectText(id, s)
                + "\n<color=#A89C94>Recharge " + def.cooldown.ToString("0") + "s</color></size>";
        }

        for (int i = 0; i < 18; i++)
        {
            var def = SkillData.Skills[i];
            var s = GameState.FindSkill(i);
            var rc = ProgressionData.RarityColors[def.rarity];
            var ol = skillTileBg[i].GetComponent<Outline>();
            if (s == null)
            {
                skillTileBg[i].color = new Color(0.08f, 0.05f, 0.06f);
                ol.effectColor = new Color(rc.r * 0.4f, rc.g * 0.4f, rc.b * 0.4f);
                skillTileText[i].text = "<color=#665555>???\n" + ProgressionData.Rarities[def.rarity] + "</color>";
                continue;
            }
            bool eq = GameState.IsSkillEquipped(i);
            skillTileBg[i].color = eq ? new Color(rc.r * 0.5f, rc.g * 0.5f, rc.b * 0.5f, 1f) : new Color(rc.r * 0.22f, rc.g * 0.22f, rc.b * 0.22f, 1f);
            ol.effectColor = eq ? Color.white : rc;
            skillTileText[i].text = "<b>" + def.name + "</b>\n<size=26>Niv. " + s.level + "</size>\n<size=18>" + s.copies + "/" + GameState.SkillCopiesForNext(s.level) + (eq ? "  ✔ équipée" : "") + "</size>";
        }
    }
}
