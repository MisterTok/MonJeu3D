using UnityEngine;
using UnityEngine.UI;

// Bonus des objets (statistiques secondaires) mis en avant : une couleur et un symbole par type,
// en pastilles dans la fenêtre de la nouvelle pièce, en symboles colorés sur les tuiles.
public partial class ForgeUI
{
    static readonly Color[] SubColors =
    {
        new Color(1f, 0.82f, 0.25f),   // Chance de critique
        new Color(1f, 0.55f, 0.15f),   // Dégâts critiques
        new Color(0.45f, 0.7f, 1f),    // Blocage
        new Color(0.4f, 0.9f, 0.45f),  // Régénération
        new Color(1f, 0.3f, 0.45f),    // Vol de vie
        new Color(0.3f, 0.9f, 0.95f),  // Double frappe
        new Color(1f, 0.32f, 0.22f),   // Dégâts
        new Color(1f, 0.45f, 0.3f),    // Dégâts en mêlée
        new Color(0.75f, 0.5f, 1f),    // Vitesse d'attaque
        new Color(0.55f, 1f, 0.6f),    // Santé
    };
    static readonly string[] SubGlyphs = { "✦", "✸", "◆", "✚", "♥", "◉", "▲", "✪", "»", "●" };

    static string SubRich(int type, float val, bool withName = true) =>
        "<color=" + Hex(SubColors[type]) + ">" + SubGlyphs[type] + " <b>+" + GameState.FmtPct(val) + "</b>" + (withName ? " " + GameState.SubNames[type] : "") + "</color>";

    // Symboles colorés seulement (tuiles).
    static string SubDots(int[] subs)
    {
        if (subs == null || subs.Length == 0) return "";
        var sb = new System.Text.StringBuilder();
        foreach (int t in subs) sb.Append("<color=").Append(Hex(SubColors[t])).Append('>').Append(SubGlyphs[t]).Append("</color>");
        return sb.ToString();
    }

    // ---------- Pastilles ----------
    class Pills { public RectTransform root; public Image[] bg = new Image[4]; public Text[] txt = new Text[4]; }

    Pills MakePills(Transform parent, Vector2 offMin, Vector2 offMax)
    {
        var p = new Pills();
        p.root = MakeRect("Bonus", parent, new Vector2(0, 1), new Vector2(1, 1), offMin, offMax);
        for (int i = 0; i < 4; i++)
        {
            var rt = MakeRect("Pastille " + i, p.root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
            p.bg[i] = rt.gameObject.AddComponent<Image>();
            p.bg[i].sprite = Round();
            p.bg[i].type = Image.Type.Sliced;
            p.bg[i].raycastTarget = false;
            p.txt[i] = Label(rt, "", 30, TextAnchor.MiddleCenter, Color.white);
            p.txt[i].fontStyle = FontStyle.Bold;
            rt.gameObject.SetActive(false);
        }
        return p;
    }

    // Une pastille par bonus, deux par ligne, centrées.
    void SetPills(Pills p, int[] subs, float[] vals)
    {
        int n = subs == null ? 0 : Mathf.Min(subs.Length, 4);
        const float W = 440f, H = 58f, Gap = 14f;
        for (int i = 0; i < 4; i++)
        {
            var go = p.bg[i].gameObject;
            go.SetActive(i < n);
            if (i >= n) continue;
            int t = subs[i];
            var c = SubColors[t];
            p.bg[i].color = new Color(c.r * 0.32f, c.g * 0.32f, c.b * 0.32f, 1f);
            var ol = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
            ol.effectColor = c; ol.effectDistance = new Vector2(3, -3);
            p.txt[i].text = SubGlyphs[t] + "  +" + GameState.FmtPct(vals[i]) + "  " + GameState.SubNames[t];
            p.txt[i].color = Color.Lerp(c, Color.white, 0.35f);
            int row = i / 2, inRow = Mathf.Min(2, n - row * 2), col = i % 2;
            float x = inRow == 1 ? 0f : (col == 0 ? -(W + Gap) / 2f : (W + Gap) / 2f);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(W, H);
            rt.anchoredPosition = new Vector2(x, -H / 2f - row * (H + Gap));
        }
    }
}
