using System;
using UnityEngine;
using UnityEngine.UI;

// Éléments d'interface partagés, dans l'esprit de Forge Master : tuiles « icône + niveau » sans texte superflu,
// barre d'invocation compacte, fenêtre de détail qui s'ouvre au toucher.
public partial class ForgeUI
{
    static Sprite circleSprite, roundSprite;

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

    // Carré aux coins arrondis (9-slice) pour les tuiles.
    static Sprite Round()
    {
        if (roundSprite != null) return roundSprite;
        const int n = 48, r = 14;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, r, n - r), cy = Mathf.Clamp(y + 0.5f, r, n - r);
                float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                t.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - d)));
            }
        t.Apply();
        roundSprite = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        return roundSprite;
    }

    static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
    static readonly Color Silhouette = new Color(0f, 0f, 0f, 0.55f);
    static readonly Color TileLocked = new Color(0.1f, 0.08f, 0.08f, 1f);

    // ---------- Tuile de collection ----------
    class Tile
    {
        public Image bg, frame, glyphBg, barBg, barFill, check;
        public RawImage icon;
        public Text glyph, level;
        public Button btn;
    }

    Tile MakeTile(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Action onClick, bool big = false)
    {
        var tl = new Tile();
        // Cadre de rareté puis fond
        tl.frame = Box(parent, "Tuile", aMin, aMax, offMin, offMax, Color.gray);
        tl.frame.sprite = Round(); tl.frame.type = Image.Type.Sliced;
        tl.btn = tl.frame.gameObject.AddComponent<Button>();
        tl.btn.transition = Selectable.Transition.ColorTint;
        tl.btn.onClick.AddListener(() => onClick());
        tl.bg = Box(tl.frame.transform, "Fond", Vector2.zero, Vector2.one, new Vector2(5, 5), new Vector2(-5, -5), TileLocked);
        tl.bg.sprite = Round(); tl.bg.type = Image.Type.Sliced; tl.bg.raycastTarget = false;
        // Icône carrée centrée
        var holder = MakeRect("Zone icône", tl.bg.transform, Vector2.zero, Vector2.one, new Vector2(8, big ? 30 : 22), new Vector2(-8, -8));
        var irt = MakeRect("Icône", holder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var fit = irt.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = 1f;
        tl.icon = irt.gameObject.AddComponent<RawImage>(); tl.icon.raycastTarget = false;
        // Variante sans modèle 3D (compétences) : pastille colorée + symbole
        var grt = MakeRect("Pastille", holder, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6));
        var gfit = grt.gameObject.AddComponent<AspectRatioFitter>();
        gfit.aspectMode = AspectRatioFitter.AspectMode.FitInParent; gfit.aspectRatio = 1f;
        tl.glyphBg = grt.gameObject.AddComponent<Image>(); tl.glyphBg.sprite = Circle(); tl.glyphBg.raycastTarget = false;
        tl.glyph = Label(grt, "", big ? 64 : 48, TextAnchor.MiddleCenter, Color.white);
        tl.glyphBg.gameObject.SetActive(false);
        // Barre de doublons en bas, niveau par-dessus
        tl.barBg = Box(tl.bg.transform, "Barre", new Vector2(0, 0), new Vector2(1, 0), new Vector2(10, 8), new Vector2(-10, big ? 34 : 26), new Color(0, 0, 0, 0.7f));
        tl.barBg.raycastTarget = false;
        tl.barFill = Box(tl.barBg.transform, "Remplissage", Vector2.zero, new Vector2(0.5f, 1), Vector2.zero, Vector2.zero, new Color(0.35f, 0.85f, 0.4f));
        tl.barFill.raycastTarget = false;
        tl.level = Label(tl.barBg.transform, "", big ? 24 : 19, TextAnchor.MiddleCenter, Color.white);
        // Coche « équipé »
        tl.check = Box(tl.bg.transform, "Équipé", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-34, -34), new Vector2(-4, -4), new Color(0.25f, 0.75f, 0.3f));
        tl.check.sprite = Circle(); tl.check.raycastTarget = false;
        Label(tl.check.transform, "✔", 20, TextAnchor.MiddleCenter, Color.white);
        return tl;
    }

    // Met à jour une tuile. icon = rendu 3D (ou null avec glyph pour une pastille). level < 0 = pas encore obtenu.
    void SetTile(Tile tl, int rarity, Texture icon, string glyph, Color glyphColor, int level, int copies, int need, bool equipped)
    {
        var rc = ProgressionData.RarityColors[rarity];
        bool owned = level >= 0;
        tl.frame.color = owned ? rc : new Color(rc.r * 0.35f, rc.g * 0.35f, rc.b * 0.35f);
        tl.bg.color = owned ? new Color(rc.r * 0.32f + 0.05f, rc.g * 0.32f + 0.04f, rc.b * 0.32f + 0.04f) : TileLocked;
        bool useGlyph = icon == null;
        tl.icon.gameObject.SetActive(!useGlyph);
        tl.glyphBg.gameObject.SetActive(useGlyph);
        if (useGlyph)
        {
            tl.glyphBg.color = owned ? glyphColor : new Color(0.2f, 0.17f, 0.17f);
            tl.glyph.text = owned ? glyph : "?";
            tl.glyph.color = owned ? Color.white : new Color(0.45f, 0.4f, 0.4f);
        }
        else
        {
            tl.icon.texture = icon;
            tl.icon.color = owned ? Color.white : Silhouette;
        }
        tl.barBg.gameObject.SetActive(owned);
        if (owned)
        {
            float f = need > 0 ? Mathf.Clamp01(copies / (float)need) : 1f;
            tl.barFill.rectTransform.anchorMax = new Vector2(f, 1);
            tl.barFill.color = copies >= need && need > 0 ? new Color(1f, 0.8f, 0.25f) : new Color(0.3f, 0.75f, 0.35f);
            tl.level.text = "<b>Niv. " + level + "</b>";
        }
        tl.check.gameObject.SetActive(equipped);
    }

    void SetEmptySlot(Tile tl)
    {
        tl.frame.color = new Color(0.3f, 0.18f, 0.14f);
        tl.bg.color = new Color(0.08f, 0.05f, 0.05f);
        tl.icon.gameObject.SetActive(false);
        tl.glyphBg.gameObject.SetActive(true);
        tl.glyphBg.color = new Color(0.14f, 0.1f, 0.1f);
        tl.glyph.text = "+";
        tl.glyph.color = new Color(0.45f, 0.35f, 0.3f);
        tl.barBg.gameObject.SetActive(false);
        tl.check.gameObject.SetActive(false);
    }

    // ---------- Barre d'invocation compacte ----------
    class SummonBar { public Text currency, level; public Image progFill; public Button b1, b2; public Text t1, t2; }

    SummonBar MakeSummonBar(Transform P, float top, Color btnColor, Action s1, Action s2, Action info)
    {
        var sb = new SummonBar();
        var row = Box(P, "Invocation", new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, top - 104), new Vector2(-12, top), new Color(0.1f, 0.06f, 0.06f, 0.95f));
        row.sprite = Round(); row.type = Image.Type.Sliced;
        sb.currency = Label(row.transform, "", 30, TextAnchor.MiddleLeft, new Color(0.96f, 0.9f, 0.78f), new Vector2(0, 0), new Vector2(0.3f, 1), new Vector2(20, 0), Vector2.zero);
        sb.b1 = MakeButton(row.transform, "Invoquer 1", new Vector2(0.3f, 0), new Vector2(0.53f, 1), new Vector2(4, 12), new Vector2(-4, -12), btnColor, "", 24, out sb.t1, () => s1());
        sb.b2 = MakeButton(row.transform, "Invoquer 2", new Vector2(0.53f, 0), new Vector2(0.76f, 1), new Vector2(4, 12), new Vector2(-4, -12), btnColor, "", 24, out sb.t2, () => s2());
        // Niveau d'invocation : badge + barre ; le « ? » ouvre les chances
        var lv = MakeButton(row.transform, "Niveau", new Vector2(0.76f, 0), new Vector2(1f, 1), new Vector2(6, 12), new Vector2(-12, -12), new Color(0.18f, 0.12f, 0.12f), "", 22, out sb.level, () => info());
        sb.level.alignment = TextAnchor.UpperCenter;
        sb.level.rectTransform.offsetMax = new Vector2(0, -6);
        var pbg = Box(lv.transform, "Progression", new Vector2(0, 0), new Vector2(1, 0), new Vector2(12, 10), new Vector2(-12, 22), new Color(0, 0, 0, 0.7f));
        pbg.raycastTarget = false;
        sb.progFill = Box(pbg.transform, "Remplissage", Vector2.zero, new Vector2(0.3f, 1), Vector2.zero, Vector2.zero, new Color(0.55f, 0.75f, 1f));
        sb.progFill.raycastTarget = false;
        return sb;
    }

    void SetSummonBar(SummonBar sb, string currencyName, long have, long cost1, int n1, int n2, int level, int prog, int need)
    {
        sb.currency.text = "<size=22>" + currencyName + "</size>\n<b>" + GameState.Fmt(have) + "</b>";
        sb.t1.text = "x" + n1 + "\n<size=20>" + GameState.Fmt(cost1 * n1) + "</size>";
        sb.t2.text = "x" + n2 + "\n<size=20>" + GameState.Fmt(cost1 * n2) + "</size>";
        sb.b1.interactable = have >= cost1 * n1;
        sb.b2.interactable = have >= cost1 * n2;
        sb.level.text = "Niv. " + (level + 1) + "  <color=#9FC8FF>(?)</color>";
        sb.progFill.rectTransform.anchorMax = new Vector2(need > 0 ? Mathf.Clamp01(prog / (float)need) : 1f, 1);
    }

    static string OddsText(float[] odds)
    {
        var sb = new System.Text.StringBuilder();
        for (int r = 0; r < 6; r++)
            if (odds[r] > 0.0001f) sb.Append("<color=" + Hex(ProgressionData.RarityColors[r]) + ">" + ProgressionData.Rarities[r] + "</color>   " + odds[r].ToString("0.###") + " %\n");
        return sb.ToString();
    }

    // ---------- Fenêtre de détail ----------
    GameObject detail;
    Image detailFrame, detailGlyphBg;
    RawImage detailIcon;
    Text detailGlyph, detailName, detailRarity, detailLevel, detailBody, dpBtnText;
    Image detailBarFill;
    GameObject detailBarRoot;
    Button dpBtn;
    Action detailAction;

    void BuildDetailPopup(Transform R)
    {
        detail = MakeRect("Détail", R, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        var shade = Box(detail.transform, "Voile", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
        var closeBg = shade.gameObject.AddComponent<Button>();
        closeBg.transition = Selectable.Transition.None;
        closeBg.onClick.AddListener(() => detail.SetActive(false));
        detailFrame = Box(detail.transform, "Cadre", new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.73f), Vector2.zero, Vector2.zero, Color.white);
        detailFrame.sprite = Round(); detailFrame.type = Image.Type.Sliced;
        var card = Box(detailFrame.transform, "Carte", Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6), new Color(0.08f, 0.04f, 0.04f, 1f));
        card.sprite = Round(); card.type = Image.Type.Sliced;
        Transform C = card.transform;
        Text x;
        MakeButton(C, "Fermer", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-90, -90), new Vector2(-16, -16), new Color(0.4f, 0.13f, 0.1f), "X", 36, out x, () => detail.SetActive(false));

        var holder = MakeRect("Zone icône", C, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-150, -330), new Vector2(150, -30));
        detailIcon = MakeRect("Icône", holder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject.AddComponent<RawImage>();
        detailIcon.raycastTarget = false;
        var grt = MakeRect("Pastille", holder, Vector2.zero, Vector2.one, new Vector2(40, 40), new Vector2(-40, -40));
        detailGlyphBg = grt.gameObject.AddComponent<Image>(); detailGlyphBg.sprite = Circle(); detailGlyphBg.raycastTarget = false;
        detailGlyph = Label(grt, "", 110, TextAnchor.MiddleCenter, Color.white);

        detailName = Label(C, "", 46, TextAnchor.UpperCenter, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -400), new Vector2(-20, -335));
        detailRarity = Label(C, "", 28, TextAnchor.UpperCenter, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -445), new Vector2(-20, -400));
        detailBarRoot = Box(C, "Barre", new Vector2(0.15f, 1), new Vector2(0.85f, 1), new Vector2(0, -505), new Vector2(0, -460), new Color(0, 0, 0, 0.7f)).gameObject;
        detailBarFill = Box(detailBarRoot.transform, "Remplissage", Vector2.zero, new Vector2(0.5f, 1), Vector2.zero, Vector2.zero, new Color(0.3f, 0.75f, 0.35f));
        detailLevel = Label(detailBarRoot.transform, "", 26, TextAnchor.MiddleCenter, Color.white);
        detailBody = Label(C, "", 32, TextAnchor.UpperCenter, TextMain, new Vector2(0, 0), new Vector2(1, 1), new Vector2(30, 150), new Vector2(-30, -525));
        dpBtn = MakeButton(C, "Action", new Vector2(0.2f, 0), new Vector2(0.8f, 0), new Vector2(0, 30), new Vector2(0, 130),
            new Color(0.15f, 0.5f, 0.22f), "", 40, out dpBtnText, () => { detailAction?.Invoke(); detail.SetActive(false); });
        detail.SetActive(false);
    }

    // Ouvre la fiche d'un élément. level < 0 = pas encore obtenu (pas de bouton).
    void ShowDetail(string name, int rarity, Texture icon, string glyph, Color glyphColor, int level, int copies, int need, string body, string action, Action onAction, Color? actionColor = null)
    {
        var rc = ProgressionData.RarityColors[rarity];
        bool owned = level >= 0;
        detailFrame.color = rc;
        detailIcon.gameObject.SetActive(icon != null);
        detailGlyphBg.gameObject.SetActive(icon == null);
        if (icon != null) { detailIcon.texture = icon; detailIcon.color = owned ? Color.white : Silhouette; }
        else { detailGlyphBg.color = owned ? glyphColor : new Color(0.2f, 0.17f, 0.17f); detailGlyph.text = owned ? glyph : "?"; }
        detailName.rectTransform.offsetMin = new Vector2(20, -400); detailName.rectTransform.offsetMax = new Vector2(-20, -335);
        detailBody.rectTransform.offsetMax = new Vector2(-30, -525);
        detailName.text = owned ? name : "???";
        detailName.color = Color.Lerp(rc, Color.white, 0.35f);
        detailRarity.text = "<color=" + Hex(rc) + ">" + ProgressionData.Rarities[rarity] + "</color>";
        detailBarRoot.SetActive(owned);
        if (owned)
        {
            detailBarFill.rectTransform.anchorMax = new Vector2(need > 0 ? Mathf.Clamp01(copies / (float)need) : 1f, 1);
            detailLevel.text = "<b>Niv. " + level + "</b>   <size=22>" + copies + " / " + need + "</size>";
        }
        detailBody.text = owned ? body : "<color=#998877>Pas encore obtenu</color>";
        dpBtn.gameObject.SetActive(owned && action != null);
        dpBtnText.text = action ?? "";
        dpBtn.GetComponent<Image>().color = actionColor ?? new Color(0.15f, 0.5f, 0.22f);
        detailAction = onAction;
        detail.transform.SetAsLastSibling();
        detail.SetActive(true);
    }

    // Fiche texte simple (chances d'invocation…).
    void ShowInfo(string title, string body)
    {
        detailFrame.color = Ember;
        detailIcon.gameObject.SetActive(false);
        detailGlyphBg.gameObject.SetActive(false);
        detailName.rectTransform.offsetMin = new Vector2(20, -110); detailName.rectTransform.offsetMax = new Vector2(-20, -40);
        detailBody.rectTransform.offsetMax = new Vector2(-30, -140);
        detailName.text = title; detailName.color = Ember;
        detailRarity.text = "";
        detailBarRoot.SetActive(false);
        detailBody.text = body;
        dpBtn.gameObject.SetActive(false);
        detail.transform.SetAsLastSibling();
        detail.SetActive(true);
    }
}
