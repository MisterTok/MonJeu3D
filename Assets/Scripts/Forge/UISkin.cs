using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Skin d'interface façon Forge Master : sprites générés par Tools/make_ui_skin.py (Resources/ForgeUI) et police Lilita One.
// Sans les fichiers, retombe sur les sprites procéduraux et la police intégrée.
public static class UISkin
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    static Font font;

    // Bords 9-slice (en pixels de la texture) de chaque élément étirable.
    static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
    {
        { "btn", new Vector4(34, 40, 34, 34) }, { "tab", new Vector4(26, 26, 26, 26) }, { "panel", new Vector4(40, 40, 40, 40) },
        { "card", new Vector4(26, 26, 26, 26) }, { "frame", new Vector4(48, 48, 48, 48) }, { "tile", new Vector4(30, 30, 30, 30) },
        { "pill", new Vector4(31, 31, 31, 31) }, { "slot", new Vector4(22, 22, 22, 22) }, { "bar_bg", new Vector4(22, 22, 22, 22) }, { "bar_fill", new Vector4(22, 22, 22, 22) },
        { "banner", new Vector4(80, 0, 80, 0) }, { "topbar", new Vector4(0, 8, 0, 8) }, { "navbar", new Vector4(0, 8, 0, 8) },
    };

    public static Font Font
    {
        get
        {
            if (font == null) font = Resources.Load<Font>("ForgeUI/LilitaOne");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    public static Sprite Get(string name)
    {
        if (cache.TryGetValue(name, out var s)) return s;
        var tex = Resources.Load<Texture2D>("ForgeUI/" + name);
        if (tex != null)
        {
            Borders.TryGetValue(name, out var b);
            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, b);
        }
        cache[name] = s;
        return s;
    }

    // Applique un sprite étirable (9-slice) ; scale > 1 affine les bords.
    public static void Sliced(Image img, string name, float scale = 1f)
    {
        var s = Get(name);
        if (s == null) return;
        img.sprite = s;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = scale;
    }

    // Icône (garde ses proportions).
    public static Image Icon(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
    {
        var go = new GameObject("Icône " + name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = go.AddComponent<Image>();
        img.sprite = Get(name);
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (img.sprite == null) img.color = new Color(0, 0, 0, 0);
        return img;
    }

    // Couleurs de bouton plus franches (le sprite est clair en haut, ombré en bas).
    public static Color Vivid(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        var o = Color.HSVToRGB(h, Mathf.Min(1f, s * 1.1f), Mathf.Min(1f, v * 1.45f + 0.08f));
        o.a = c.a;
        return o;
    }
}
