using System.Collections.Generic;
using UnityEngine;

// Modèles de gantelets, bottes et anneaux (Quaternius CC0) stockés en JSON dans Resources/ForgeModels/Gear :
// une liste de parties (couleur + sommets + triangles), reconstruites en maillages à facettes au lancement.
public static class GearModel
{
    [System.Serializable] class Part { public float[] color; public float[] v; public int[] t; }
    [System.Serializable] class Data { public Part[] parts; }

    static readonly Dictionary<string, Mesh[]> meshCache = new Dictionary<string, Mesh[]>();
    static readonly Dictionary<string, Color[]> colorCache = new Dictionary<string, Color[]>();

    // Bottes et anneaux changent de modèle selon le cercle.
    static readonly string[] BootByCircle = { "SK_AdventurerFeet", "SK_AdventurerFeet", "SK_AdventurerFeet", "SK_MedievalFeet", "SK_MedievalFeet",
                                              "SK_MedievalFeet", "SK_SoldierFeet", "SK_SoldierFeet", "SK_SoldierFeet", "SK_KingFeet" };
    static readonly string[] RingByCircle = { "S_Ring1", "S_Ring2", "S_Ring3", "S_Ring3", "S_Ring6", "S_Ring6", "S_Ring4", "S_Ring4", "S_Ring5", "S_Ring5" };

    static readonly string[] AmuletByCircle = { "S_Amulet2", "S_Amulet2", "S_Amulet2", "S_Amulet2", "S_Amulet3",
                                                "S_Amulet3", "S_Amulet3", "S_Amulet1", "S_Amulet1", "S_Amulet1" };
    public static string AmuletName(int circle) => AmuletByCircle[Mathf.Clamp(circle, 0, 9)];
    public static string GloveName(int circle) => "S_Glove";
    public static string BootName(int circle) => BootByCircle[Mathf.Clamp(circle, 0, 9)];
    public static string RingName(int circle) => RingByCircle[Mathf.Clamp(circle, 0, 9)];

    static bool Load(string name)
    {
        if (meshCache.ContainsKey(name)) return meshCache[name] != null;
        var ta = Resources.Load<TextAsset>("ForgeModels/Gear/" + name);
        if (ta == null) { Debug.LogWarning("Forge : modèle introuvable Gear/" + name); meshCache[name] = null; return false; }
        var data = JsonUtility.FromJson<Data>(ta.text);
        var meshes = new Mesh[data.parts.Length];
        var cols = new Color[data.parts.Length];
        for (int p = 0; p < data.parts.Length; p++)
        {
            var part = data.parts[p];
            // Sommets dédoublés par triangle : ombrage à facettes, comme les autres modèles low-poly.
            var verts = new Vector3[part.t.Length];
            var tris = new int[part.t.Length];
            for (int i = 0; i < part.t.Length; i++)
            {
                int k = part.t[i] * 3;
                verts[i] = new Vector3(part.v[k], part.v[k + 1], part.v[k + 2]);
                tris[i] = i;
            }
            var m = new Mesh { name = name + "_" + p };
            if (verts.Length > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = verts;
            m.triangles = tris;
            m.RecalculateNormals();
            m.RecalculateBounds();
            meshes[p] = m;
            cols[p] = part.color != null && part.color.Length >= 3 ? new Color(part.color[0], part.color[1], part.color[2]) : Color.gray;
        }
        meshCache[name] = meshes;
        colorCache[name] = cols;
        return true;
    }

    // Construit le modèle (sa plus grande dimension vaut « size », centré). Les parties de métal ou de cuir prennent
    // la teinte du cercle ; les pierres précieuses (couleurs vives) gardent leur couleur et brillent.
    // euler = orientation de présentation (appliquée autour du centre du modèle).
    public static GameObject Build(string name, float size, Color tint, float glow, Transform parent = null, Vector3 euler = default)
    {
        var root = new GameObject("Équipement " + name);
        if (parent != null) root.transform.SetParent(parent, false);
        if (!Load(name)) return root;
        var meshes = meshCache[name];
        var cols = colorCache[name];
        var pivot = new GameObject("Pivot").transform;
        pivot.SetParent(root.transform, false);
        pivot.localRotation = Quaternion.Euler(euler);
        var holder = new GameObject("Modèle").transform;
        holder.SetParent(pivot, false);
        var bounds = new Bounds();
        for (int p = 0; p < meshes.Length; p++)
        {
            var go = new GameObject("Partie " + p);
            go.transform.SetParent(holder, false);
            go.AddComponent<MeshFilter>().sharedMesh = meshes[p];
            var c = cols[p];
            Color.RGBToHSV(c, out _, out float sat, out float val);
            bool gem = sat > 0.6f && val > 0.5f;
            Material mat;
            if (gem) mat = ForgeWorld.Lit(c, 0.2f, 0.85f, c * (0.6f + glow));
            else
            {
                // Les couleurs d'origine sont très sombres (espace linéaire) : on les éclaircit avant de teinter.
                var baseCol = Color.Lerp(new Color(Mathf.Min(1f, c.r * 3f + 0.08f), Mathf.Min(1f, c.g * 3f + 0.08f), Mathf.Min(1f, c.b * 3f + 0.08f)), tint, 0.45f);
                mat = ForgeWorld.Lit(baseCol, 0.75f, 0.6f, tint * glow * 0.5f);
            }
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            if (p == 0) bounds = meshes[p].bounds; else bounds.Encapsulate(meshes[p].bounds);
        }
        float dim = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        float s = size / Mathf.Max(0.0001f, dim);
        holder.localScale = Vector3.one * s;
        holder.localPosition = -bounds.center * s;
        return root;
    }
}
