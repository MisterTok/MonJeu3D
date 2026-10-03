using System.Collections.Generic;
using UnityEngine;

// Chargement des modèles 3D Quaternius (Resources/ForgeModels) avec mise à l'échelle automatique.
public static class ModelLib
{
    static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

    public static GameObject Prefab(string path)
    {
        if (cache.TryGetValue(path, out var p)) return p;
        p = Resources.Load<GameObject>("ForgeModels/" + path);
        if (p == null) Debug.LogWarning("Forge : modèle introuvable " + path);
        cache[path] = p;
        return p;
    }

    public static Bounds WorldBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    // Instancie un modèle dans un conteneur. La taille (hauteur, ou plus grande dimension si byMaxDim) vaut « size ».
    // Le conteneur a son pivot aux pieds (ou au centre si centerPivot), centré en X/Z.
    public static GameObject Spawn(string path, float size, Transform parent = null, bool byMaxDim = false, bool centerPivot = false)
    {
        var root = new GameObject(path);
        var prefab = Prefab(path);
        if (prefab == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(cube.GetComponent<Collider>());
            cube.transform.SetParent(root.transform, false);
            cube.transform.localScale = Vector3.one * size;
            cube.transform.localPosition = new Vector3(0, centerPivot ? 0 : size / 2f, 0);
            if (parent != null) root.transform.SetParent(parent, false);
            return root;
        }
        var inst = Object.Instantiate(prefab, root.transform, false);
        inst.name = prefab.name;
        foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>(true)) smr.updateWhenOffscreen = true;
        var b = WorldBounds(inst);
        float dim = byMaxDim ? Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)) : b.size.y;
        float s = size / Mathf.Max(dim, 0.0001f);
        inst.transform.localScale *= s;
        float y = centerPivot ? -b.center.y * s : -b.min.y * s;
        inst.transform.localPosition = new Vector3(-b.center.x * s, y, -b.center.z * s);
        if (parent != null) root.transform.SetParent(parent, false);
        return root;
    }

    // Personnages KayKit : certains objets portés (armes, boucliers, casque) sont des maillages du modèle.
    // On cache ceux de la liste « hide » avant de mesurer, pour que la taille corresponde au corps seul.
    public static GameObject SpawnKit(string path, float height, Transform parent, params string[] hide)
    {
        var root = new GameObject(path);
        var prefab = Prefab(path);
        if (prefab == null) { if (parent != null) root.transform.SetParent(parent, false); return root; }
        var inst = Object.Instantiate(prefab, root.transform, false);
        inst.name = prefab.name;
        foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
        {
            if (System.Array.IndexOf(hide, r.name) >= 0) r.gameObject.SetActive(false);
            if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = true;
        }
        var b = new Bounds(); bool has = false;
        foreach (var r in inst.GetComponentsInChildren<Renderer>(false))
        {
            if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
        }
        float s = height / Mathf.Max(b.size.y, 0.0001f);
        inst.transform.localScale *= s;
        inst.transform.localPosition = new Vector3(-b.center.x * s, -b.min.y * s, -b.center.z * s);
        if (parent != null) root.transform.SetParent(parent, false);
        return root;
    }

    // Instancie un élément de décor à son échelle d'origine (le kit donjon est déjà en mètres).
    public static GameObject Raw(string path, Transform parent, Vector3 pos, float rotY = 0f, float scale = 1f)
    {
        var prefab = Prefab(path);
        if (prefab == null) return null;
        var root = new GameObject(path);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        root.transform.localRotation = Quaternion.Euler(0, rotY, 0);
        root.transform.localScale = Vector3.one * scale;
        var inst = Object.Instantiate(prefab, root.transform, false);
        inst.name = prefab.name;
        return root;
    }

    // Teinte toutes les couleurs du modèle vers une couleur (instancie les matériaux).
    public static void Tint(GameObject go, Color c, float amount)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer) continue;
            var mats = r.materials;
            foreach (var m in mats)
            {
                if (m == null || !m.HasProperty("_BaseColor")) continue;
                var bc = m.GetColor("_BaseColor");
                var t = Color.Lerp(bc, c, amount);
                t.a = bc.a;
                m.SetColor("_BaseColor", t);
            }
            r.materials = mats;
        }
    }

    public static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var r = FindDeep(t.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }
}

// Lecture des animations « Legacy » par mots-clés (les noms exacts varient selon les modèles).
public class AnimPlayer : MonoBehaviour
{
    Animation anim;
    readonly Dictionary<string, string> found = new Dictionary<string, string>();

    public static AnimPlayer Attach(GameObject root)
    {
        var a = root.GetComponentInChildren<Animation>();
        if (a == null) return null;
        var p = root.AddComponent<AnimPlayer>();
        p.anim = a;
        a.cullingType = AnimationCullingType.AlwaysAnimate;
        a.playAutomatically = false;
        return p;
    }

    public string Find(string[] keys)
    {
        string cacheKey = string.Join(",", keys);
        if (found.TryGetValue(cacheKey, out var n)) return n;
        n = null;
        foreach (var k in keys)
        {
            foreach (AnimationState s in anim)
                if (s.name.EndsWith("|" + k) || s.name.EndsWith("_" + k) || s.name == k) { n = s.name; break; }
            if (n != null) break;
        }
        if (n == null)
            foreach (var k in keys)
            {
                foreach (AnimationState s in anim)
                    if (s.name.ToLowerInvariant().Contains(k.ToLowerInvariant())) { n = s.name; break; }
                if (n != null) break;
            }
        if (n == null && System.Array.IndexOf(keys, "*") >= 0)
            foreach (AnimationState st in anim) { n = st.name; break; }
        found[cacheKey] = n;
        return n;
    }

    public bool Has(string[] keys) => Find(keys) != null;

    // Joue l'animation ; renvoie sa durée (0 si absente).
    public float Play(string[] keys, bool loop, float fade = 0.12f, float speed = 1f, bool restart = false)
    {
        string n = Find(keys);
        if (n == null) return 0f;
        var st = anim[n];
        st.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
        st.speed = speed;
        if (restart || !loop) st.time = 0f;
        if (fade > 0f) anim.CrossFade(n, fade);
        else anim.Play(n);
        return st.length / Mathf.Max(0.01f, speed);
    }
}
