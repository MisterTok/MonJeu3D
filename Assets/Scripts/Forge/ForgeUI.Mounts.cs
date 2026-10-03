using UnityEngine;
using UnityEngine.UI;

// Écran des montures : invocation avec les remontoirs, monture équipée et collection.
public partial class ForgeUI
{
    GameObject mountPanel;
    Text mountWinders, mountSummonLvl, mountEquipped;
    Button mSummon1, mSummon15;
    Text mSummon1Text, mSummon15Text;
    Image mountEqBg;
    readonly Image[] mountTileBg = new Image[15];
    readonly Text[] mountTileText = new Text[15];

    void BuildMountPanel(Transform R)
    {
        mountPanel = MakeRect("Montures", R, Vector2.zero, Vector2.one, new Vector2(0, 150), new Vector2(0, -130)).gameObject;
        mountPanel.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.98f);
        Transform P = mountPanel.transform;
        Label(P, "MONTURES", 48, TextAnchor.UpperCenter, Ember, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -16));
        Text closeT;
        MakeButton(P, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -86), new Vector2(-20, -16),
            new Color(0.45f, 0.14f, 0.1f), "X", 40, out closeT, () => mountPanel.SetActive(false));
        Label(P, "Ton héros chevauche sa monture : elle augmente ses dégâts et sa vie en pourcentage.", 24, TextAnchor.UpperCenter, TextDim,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -130), new Vector2(-30, -88));

        var srow = Box(P, "Invocation", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -235), new Vector2(-12, -138), Panel);
        mountWinders = Label(srow.transform, "", 30, TextAnchor.MiddleLeft, new Color(0.95f, 0.88f, 0.75f), new Vector2(0, 0), new Vector2(0.3f, 1), new Vector2(18, 0), Vector2.zero);
        mSummon1 = MakeButton(srow.transform, "Invoquer x1", new Vector2(0.3f, 0), new Vector2(0.52f, 1), new Vector2(4, 10), new Vector2(-4, -10),
            new Color(0.5f, 0.25f, 0.65f), "", 24, out mSummon1Text, () => OnSummonMounts(1));
        mSummon15 = MakeButton(srow.transform, "Invoquer x15", new Vector2(0.52f, 0), new Vector2(0.74f, 1), new Vector2(4, 10), new Vector2(-4, -10),
            new Color(0.5f, 0.25f, 0.65f), "", 24, out mSummon15Text, () => OnSummonMounts(15));
        mountSummonLvl = Label(srow.transform, "", 22, TextAnchor.MiddleLeft, TextDim, new Vector2(0.74f, 0), new Vector2(1, 1), new Vector2(10, 0), new Vector2(-8, 0));

        Label(P, "Monture chevauchée", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -290), new Vector2(0, -250));
        mountEqBg = Box(P, "Équipée", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -470), new Vector2(-12, -295), Panel);
        mountEqBg.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5, -5);
        mountEquipped = Label(mountEqBg.transform, "", 32, TextAnchor.MiddleCenter, TextMain, Vector2.zero, Vector2.one, new Vector2(12, 6), new Vector2(-12, -6));

        Label(P, "Collection (touche pour chevaucher)", 30, TextAnchor.UpperLeft, TextDim, new Vector2(0, 1), new Vector2(1, 1), new Vector2(30, -530), new Vector2(0, -490));
        var grid = MakeRect("Collection", P, Vector2.zero, new Vector2(1, 1), new Vector2(12, 12), new Vector2(-12, -540));
        for (int i = 0; i < 15; i++)
        {
            int id = i, row = i / 5, col = i % 5;
            Text t;
            var b = MakeButton(grid, "Monture " + i, new Vector2(col / 5f, 1f - (row + 1) / 3f), new Vector2((col + 1) / 5f, 1f - row / 3f),
                new Vector2(6, 6), new Vector2(-6, -6), Panel, "", 24, out t, () => { GameState.EquipMount(id); RefreshMounts(); });
            mountTileBg[i] = b.GetComponent<Image>();
            mountTileText[i] = t;
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
        if (GameState.SummonMounts(count) == null) Toast("Pas assez de remontoirs (boss, et bientôt ligue et guerre de clans)");
        RefreshMounts();
    }

    void RefreshMounts()
    {
        if (mountPanel == null || !mountPanel.activeSelf) return;
        var d = GameState.Data;
        long c1 = GameState.MountSummonCost;
        mountWinders.text = "Remontoirs\n<b><size=36>" + GameState.Fmt(d.winders) + "</size></b>";
        mSummon1Text.text = "Invoquer x1\n<size=20>" + c1 + " remontoirs</size>";
        mSummon15Text.text = "Invoquer x15\n<size=20>" + c1 * 15 + " remontoirs</size>";
        mSummon1.interactable = d.winders >= c1;
        mSummon15.interactable = d.winders >= c1 * 15;
        var odds = GameState.MountOdds;
        var ob = new System.Text.StringBuilder("Invocation niv. " + (d.mountSummonLevel + 1) + "  (" + d.mountSummonProgress + "/" + GameState.MountSummonRequired + ")\n<size=18>");
        for (int r = 0; r < 6; r++)
            if (odds[r] > 0.0001f) ob.Append("<color=" + Hex(ProgressionData.RarityColors[r]) + ">" + odds[r].ToString("0.##") + "%</color> ");
        mountSummonLvl.text = ob.Append("</size>").ToString();

        var eq = GameState.FindMount(d.equippedMount);
        var ol = mountEqBg.GetComponent<Outline>();
        if (eq == null)
        {
            mountEqBg.color = Panel;
            ol.effectColor = new Color(0.4f, 0.25f, 0.2f);
            mountEquipped.text = "<color=#998877>Aucune monture : invoque-en avec des remontoirs</color>";
        }
        else
        {
            var def = ProgressionData.Mounts[eq.id];
            var c = ProgressionData.RarityColors[def.rarity];
            mountEqBg.color = new Color(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f, 1f);
            ol.effectColor = c;
            mountEquipped.text = "<b>" + def.name + "</b>  <size=26><color=" + Hex(c) + ">" + ProgressionData.Rarities[def.rarity] + "</color> · niv. " + eq.level
                + " (" + eq.copies + "/" + eq.level + ")</size>\n+" + GameState.FmtPct((float)GameState.MountDamageBonus()) + " dégâts   +" + GameState.FmtPct((float)GameState.MountHealthBonus()) + " vie";
        }

        for (int i = 0; i < 15; i++)
        {
            var def = ProgressionData.Mounts[i];
            var m = GameState.FindMount(i);
            var c = ProgressionData.RarityColors[def.rarity];
            var tol = mountTileBg[i].GetComponent<Outline>();
            if (m == null)
            {
                mountTileBg[i].color = new Color(0.12f, 0.09f, 0.09f);
                tol.effectColor = new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.45f);
                mountTileText[i].text = "<color=#776666>???\n" + ProgressionData.Rarities[def.rarity] + "</color>";
                continue;
            }
            bool on = d.equippedMount == i;
            mountTileBg[i].color = on ? new Color(c.r * 0.55f, c.g * 0.55f, c.b * 0.55f) : new Color(c.r * 0.25f, c.g * 0.25f, c.b * 0.25f);
            tol.effectColor = on ? Color.white : c;
            mountTileText[i].text = def.name + "\n<b><size=30>Niv. " + m.level + "</size></b>\n<size=20>+" + GameState.FmtPct((float)GameState.MountBonus(m)) + "</size>";
        }
    }
}
