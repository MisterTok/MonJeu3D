using UnityEngine;
using UnityEngine.UI;

// Écran des compétences, épuré : invocation, 3 équipées, collection de 18 pastilles. Toucher = fiche.
public partial class ForgeUI
{
    GameObject skillPanel;
    SummonBar skillSummon;
    Text skillPassive;
    readonly Tile[] eqSkillTile = new Tile[3];
    readonly Tile[] skillTile = new Tile[18];

    // Symbole de chaque type de compétence.
    static readonly string[] KindGlyph = { "✦", "✸", "✚", "▲", "✪", "◉" };
    static Color SkillColor(SkillData.SkillDef d) => new Color(d.color.r * 0.6f, d.color.g * 0.6f, d.color.b * 0.6f, 1f);

    void BuildSkillPanel(Transform R)
    {
        skillPanel = MakeRect("Compétences", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        skillPanel.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.06f, 1f);
        Transform P = skillPanel.transform;
        Label(P, "COMPÉTENCES", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => skillPanel.SetActive(false));

        skillSummon = MakeSummonBar(P, -90, new Color(0.2f, 0.5f, 0.3f), () => OnSummonSkills(SkillData.SummonSmall), () => OnSummonSkills(SkillData.SummonBig),
            () => { if (GameState.AscendReady(GameState.AscSkills)) ShowAscend(GameState.AscSkills); else ShowInfo("Chances des compétences", OddsText(GameState.SkillOdds)); });

        for (int i = 0; i < 3; i++)
        {
            int slot = i;
            eqSkillTile[i] = MakeTile(P, new Vector2(0.1f + i * 0.27f, 1), new Vector2(0.1f + (i + 1) * 0.27f, 1), new Vector2(10, -470), new Vector2(-10, -215),
                () => { int id = GameState.Data.equippedSkills[slot]; if (id >= 0) OpenSkill(id); }, true);
        }
        skillPassive = Label(P, "", 26, TextAnchor.MiddleCenter, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -530), new Vector2(-20, -480));

        // Collection : 6 × 3 pastilles (une rangée par paire de raretés)
        for (int i = 0; i < 18; i++)
        {
            int id = i, row = i / 6, col = i % 6;
            float top = -550 - row * 200;
            skillTile[i] = MakeTile(P, new Vector2(col / 6f, 1), new Vector2((col + 1) / 6f, 1),
                new Vector2(col == 0 ? 14 : 5, top - 192), new Vector2(col == 5 ? -14 : -5, top), () => OpenSkill(id));
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
        if (GameState.SummonSkills(count) == null) Toast("Pas assez de tickets"); else Sfx.Summon();
        RefreshSkills();
    }

    void OpenSkill(int id)
    {
        var def = SkillData.Skills[id];
        var s = GameState.FindSkill(id);
        string glyph = KindGlyph[def.kind];
        if (s == null) { ShowDetail(def.name, def.rarity, SkillData.Icon(id), glyph, SkillColor(def), -1, 0, 0, null, null, null); return; }
        string body = GameState.SkillEffectText(id, s) + "\n<color=#A89C94>Recharge " + def.cooldown.ToString("0") + " s</color>"
            + "\n\n<size=26>Bonus permanent : <color=#FFC07A>+" + GameState.Fmt(GameState.SkillPassiveDamage(s)) + " ATQ  +" + GameState.Fmt(GameState.SkillPassiveHealth(s)) + " PV</color></size>";
        bool eq = GameState.IsSkillEquipped(id);
        ShowDetail(def.name, def.rarity, SkillData.Icon(id), glyph, SkillColor(def), s.level, s.copies, GameState.SkillCopiesForNext(s.level), body,
            eq ? "RETIRER" : "ÉQUIPER", () => { string e = GameState.ToggleSkillEquip(id); if (e != null) Toast(e); RefreshSkills(); },
            eq ? new Color(0.5f, 0.18f, 0.12f) : (Color?)null);
    }

    void RefreshSkills()
    {
        if (skillPanel == null || !skillPanel.activeSelf) return;
        var d = GameState.Data;
        SetSummonBar(skillSummon, "Tickets", d.skillTickets, GameState.SkillSummonCost, SkillData.SummonSmall, SkillData.SummonBig,
            d.skillSummonLevel, d.skillSummonProgress, GameState.SkillSummonRequired, GameState.AscSkills);
        skillPassive.text = "Bonus de collection  <color=#FFC07A>+" + GameState.Fmt(GameState.SkillPassiveDamage()) + " ATQ   +" + GameState.Fmt(GameState.SkillPassiveHealth()) + " PV</color>";

        for (int i = 0; i < 3; i++)
        {
            int id = d.equippedSkills[i];
            var s = GameState.FindSkill(id);
            if (s == null) { SetEmptySlot(eqSkillTile[i]); LockSlot(eqSkillTile[i], GameState.UnlockSkillSlot[i]); continue; }
            var def = SkillData.Skills[id];
            SetTile(eqSkillTile[i], def.rarity, SkillData.Icon(id), KindGlyph[def.kind], SkillColor(def), s.level, s.copies, GameState.SkillCopiesForNext(s.level), false);
        }
        for (int i = 0; i < 18; i++)
        {
            var def = SkillData.Skills[i];
            var s = GameState.FindSkill(i);
            SetTile(skillTile[i], def.rarity, SkillData.Icon(i), KindGlyph[def.kind], SkillColor(def),
                s == null ? -1 : s.level, s == null ? 0 : s.copies, s == null ? 0 : GameState.SkillCopiesForNext(s.level), s != null && GameState.IsSkillEquipped(i));
        }
    }
}
