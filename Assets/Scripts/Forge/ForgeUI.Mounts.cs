using UnityEngine;
using UnityEngine.UI;

// Écran des montures, épuré : invocation, grande tuile de la monture chevauchée, collection d'icônes.
public partial class ForgeUI
{
    GameObject mountPanel;
    SummonBar mountSummon;
    Tile mountEqTile;
    Text mountEqText;
    readonly Tile[] mountTile = new Tile[15];

    void BuildMountPanel(Transform R)
    {
        mountPanel = MakeRect("Montures", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        mountPanel.AddComponent<Image>().color = new Color(0.07f, 0.04f, 0.06f, 1f);
        Transform P = mountPanel.transform;
        Label(P, "MONTURES", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-104, -84), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => mountPanel.SetActive(false));

        mountSummon = MakeSummonBar(P, -90, new Color(0.5f, 0.25f, 0.65f), () => OnSummonMounts(1), () => OnSummonMounts(15),
            () => ShowInfo("Chances des montures", OddsText(GameState.MountOdds)));

        // Monture chevauchée : grande tuile + son bonus
        mountEqTile = MakeTile(P, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-150, -520), new Vector2(150, -215),
            () => { if (GameState.FindMount(GameState.Data.equippedMount) != null) OpenMount(GameState.Data.equippedMount); }, true);
        mountEqText = Label(P, "", 30, TextAnchor.UpperCenter, TextMain, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -610), new Vector2(-20, -530));

        // Collection : 5 × 3 icônes
        for (int i = 0; i < 15; i++)
        {
            int id = i, row = i / 5, col = i % 5;
            float top = -630 - row * 225;
            mountTile[i] = MakeTile(P, new Vector2(col / 5f, 1), new Vector2((col + 1) / 5f, 1),
                new Vector2(col == 0 ? 14 : 5, top - 215), new Vector2(col == 4 ? -14 : -5, top), () => OpenMount(id));
        }
        mountPanel.SetActive(false);
        GameState.MountMessage += Toast;
    }

    void ToggleMounts()
    {
        CloseAllPanels();
        mountPanel.SetActive(true);
        RefreshMounts();
    }

    void OnSummonMounts(int count)
    {
        if (GameState.SummonMounts(count) == null) Toast("Pas assez de remontoirs"); else Sfx.Summon();
        RefreshMounts();
    }

    void OpenMount(int id)
    {
        var def = ProgressionData.Mounts[id];
        var m = GameState.FindMount(id);
        var c = ProgressionData.RarityColors[def.rarity];
        var icon = ItemIcons.GetCreature(def.model, c, 0.15f);
        if (m == null) { ShowDetail(def.name, def.rarity, icon, null, c, -1, 0, 0, null, null, null); return; }
        bool on = GameState.Data.equippedMount == id;
        string body = "Dégâts et vie <color=#FFC07A>+" + GameState.FmtPct((float)GameState.MountBonus(m)) + "</color>";
        ShowDetail(def.name, def.rarity, icon, null, c, m.level, m.copies, m.level, body,
            on ? "DESCENDRE" : "CHEVAUCHER", () => { GameState.EquipMount(id); RefreshMounts(); },
            on ? new Color(0.5f, 0.18f, 0.12f) : (Color?)null);
    }

    void RefreshMounts()
    {
        if (mountPanel == null || !mountPanel.activeSelf) return;
        var d = GameState.Data;
        SetSummonBar(mountSummon, "Remontoirs", d.winders, GameState.MountSummonCost, 1, 15, d.mountSummonLevel, d.mountSummonProgress, GameState.MountSummonRequired);

        var eq = GameState.FindMount(d.equippedMount);
        if (eq == null)
        {
            SetEmptySlot(mountEqTile);
            mountEqText.text = "<color=#776655>Aucune monture</color>";
        }
        else
        {
            var def = ProgressionData.Mounts[eq.id];
            var c = ProgressionData.RarityColors[def.rarity];
            SetTile(mountEqTile, def.rarity, ItemIcons.GetCreature(def.model, c, 0.15f), null, c, eq.level, eq.copies, eq.level, false);
            mountEqText.text = "<b>" + def.name + "</b>\n<size=26><color=#FFC07A>+" + GameState.FmtPct((float)GameState.MountDamageBonus()) + "</color> dégâts et vie</size>";
        }

        for (int i = 0; i < 15; i++)
        {
            var def = ProgressionData.Mounts[i];
            var m = GameState.FindMount(i);
            var c = ProgressionData.RarityColors[def.rarity];
            SetTile(mountTile[i], def.rarity, ItemIcons.GetCreature(def.model, c, 0.15f), null, c,
                m == null ? -1 : m.level, m == null ? 0 : m.copies, m == null ? 0 : m.level, d.equippedMount == i && m != null);
        }
    }
}
