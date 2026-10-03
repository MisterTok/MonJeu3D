using System.Collections.Generic;
using UnityEngine;

// Génération procédurale : textures du terrain infernal (sol volcanique, pavés, lave) et maillage de l'enclume.
public static class ProcGen
{
    // ---------- Bruit raccordable (pour des textures qui se répètent sans couture) ----------
    static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + seed * 1442695041;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }

    static int Wrap(int v, int p) => ((v % p) + p) % p;

    static float ValueNoise(float x, float y, int period, int seed)
    {
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float fx = x - x0, fy = y - y0;
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
        float a = Hash(Wrap(x0, period), Wrap(y0, period), seed), b = Hash(Wrap(x0 + 1, period), Wrap(y0, period), seed);
        float c = Hash(Wrap(x0, period), Wrap(y0 + 1, period), seed), d = Hash(Wrap(x0 + 1, period), Wrap(y0 + 1, period), seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    // u, v dans [0,1[ ; renvoie un bruit fractal raccordable dans [0,1].
    static float Fbm(float u, float v, int basePeriod, int octaves, int seed)
    {
        float sum = 0, amp = 0.5f, norm = 0;
        int p = basePeriod;
        for (int o = 0; o < octaves; o++)
        {
            sum += ValueNoise(u * p, v * p, p, seed + o * 31) * amp;
            norm += amp; amp *= 0.5f; p *= 2;
        }
        return sum / norm;
    }

    // Voronoï raccordable : distances au 1er et 2e germe, et identifiant de cellule.
    static void Voronoi(float u, float v, int cells, int seed, out float f1, out float f2, out float id)
    {
        float x = u * cells, y = v * cells;
        int cx = Mathf.FloorToInt(x), cy = Mathf.FloorToInt(y);
        f1 = f2 = 99f; id = 0f;
        for (int j = -1; j <= 1; j++)
            for (int i = -1; i <= 1; i++)
            {
                int gx = cx + i, gy = cy + j;
                int wx = Wrap(gx, cells), wy = Wrap(gy, cells);
                float px = gx + Hash(wx, wy, seed), py = gy + Hash(wx, wy, seed + 7);
                float dx = px - x, dy = py - y;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < f1) { f2 = f1; f1 = d; id = Hash(wx, wy, seed + 13); }
                else if (d < f2) f2 = d;
            }
    }

    static Texture2D NewTex(int n, bool linear = false)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, true, linear);
        t.wrapMode = TextureWrapMode.Repeat;
        t.filterMode = FilterMode.Bilinear;
        t.anisoLevel = 4;
        return t;
    }

    // Sol volcanique : basalte sombre parcouru de fissures de lave (renvoie la couleur et la carte d'émission).
    public static void VolcanicGround(int n, out Texture2D albedo, out Texture2D emission)
    {
        albedo = NewTex(n); emission = NewTex(n);
        var ca = new Color[n * n]; var ce = new Color[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (float)x / n, v = (float)y / n;
                float f = Fbm(u, v, 4, 5, 11);
                float g = Fbm(u, v, 16, 3, 23);
                Voronoi(u, v, 9, 5, out float f1, out float f2, out float id);
                float edge = f2 - f1;
                float crack = Mathf.Clamp01(1f - edge / 0.11f);
                float crackMask = Mathf.SmoothStep(0.3f, 0.55f, Fbm(u, v, 3, 3, 77));
                float glow = crack * crack * crackMask;
                float shade = 0.55f + 0.7f * f + 0.25f * (g - 0.5f) + (id - 0.5f) * 0.15f;
                var baseCol = new Color(0.27f, 0.19f, 0.17f) * shade;
                baseCol = Color.Lerp(baseCol, new Color(0.05f, 0.03f, 0.03f), crack * 0.8f);
                baseCol = Color.Lerp(baseCol, new Color(0.9f, 0.35f, 0.08f), glow * 0.6f);
                baseCol.a = 1f;
                ca[y * n + x] = baseCol;
                float e = glow * (0.7f + 0.6f * g);
                ce[y * n + x] = new Color(e, e * 0.35f, e * 0.06f, 1f);
            }
        albedo.SetPixels(ca); albedo.Apply(true);
        emission.SetPixels(ce); emission.Apply(true);
    }

    // Pavés du chemin : pierres irrégulières jointoyées.
    public static Texture2D Cobbles(int n)
    {
        var t = NewTex(n);
        var c = new Color[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (float)x / n, v = (float)y / n;
                Voronoi(u, v, 5, 3, out float f1, out float f2, out float id);
                float edge = f2 - f1;
                float mortar = Mathf.Clamp01(1f - edge / 0.09f);
                float f = Fbm(u, v, 8, 4, 41);
                float bevel = Mathf.Clamp01(edge * 6f);
                var stone = new Color(0.5f, 0.41f, 0.36f) * (0.65f + id * 0.45f) * (0.75f + 0.5f * f) * (0.7f + 0.3f * bevel);
                var col = Color.Lerp(stone, new Color(0.12f, 0.08f, 0.06f), mortar);
                col.a = 1f;
                c[y * n + x] = col;
            }
        t.SetPixels(c); t.Apply(true);
        return t;
    }

    // Lave : remous brillants (à afficher avec une couleur HDR et à faire défiler).
    public static Texture2D Lava(int n)
    {
        var t = NewTex(n);
        var c = new Color[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (float)x / n, v = (float)y / n;
                float f = Fbm(u, v, 3, 5, 91);
                float w = Fbm(u + f * 0.3f, v + f * 0.2f, 4, 4, 55);
                float crust = Mathf.SmoothStep(0.45f, 0.62f, w);
                var hot = Color.Lerp(new Color(1f, 0.85f, 0.35f), new Color(1f, 0.35f, 0.05f), f);
                var col = Color.Lerp(hot, new Color(0.18f, 0.04f, 0.02f), crust);
                col.a = 1f;
                c[y * n + x] = col;
            }
        t.SetPixels(c); t.Apply(true);
        return t;
    }

    // ---------- Enclume : profil classique extrudé, épaisseur variable (corne effilée) ----------
    public static Mesh Anvil()
    {
        // Profil vu de côté (x = longueur, y = hauteur) et demi-épaisseur en z pour chaque point.
        var pts = new List<Vector2>
        {
            new Vector2(-0.95f, 0.86f), new Vector2(-0.55f, 0.97f), new Vector2(0.78f, 0.97f), new Vector2(0.80f, 0.80f),
            new Vector2(0.58f, 0.70f), new Vector2(0.30f, 0.62f), new Vector2(0.22f, 0.40f), new Vector2(0.42f, 0.20f),
            new Vector2(0.52f, 0.0f), new Vector2(-0.52f, 0.0f), new Vector2(-0.42f, 0.20f), new Vector2(-0.22f, 0.40f),
            new Vector2(-0.30f, 0.62f), new Vector2(-0.52f, 0.76f),
        };
        var half = new List<float> { 0.11f, 0.2f, 0.22f, 0.22f, 0.22f, 0.2f, 0.17f, 0.22f, 0.26f, 0.26f, 0.22f, 0.17f, 0.2f, 0.17f };

        var verts = new List<Vector3>();
        var tris = new List<int>();
        int n = pts.Count;
        // Faces latérales (une face plate par segment)
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            var a = new Vector3(pts[i].x, pts[i].y, -half[i]); var b = new Vector3(pts[j].x, pts[j].y, -half[j]);
            var c = new Vector3(pts[j].x, pts[j].y, half[j]); var d = new Vector3(pts[i].x, pts[i].y, half[i]);
            int s = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
            tris.Add(s); tris.Add(s + 2); tris.Add(s + 1);
            tris.Add(s); tris.Add(s + 3); tris.Add(s + 2);
        }
        // Faces avant et arrière (triangulation par oreilles du profil concave)
        var poly = Triangulate(pts);
        int front = verts.Count;
        for (int i = 0; i < n; i++) verts.Add(new Vector3(pts[i].x, pts[i].y, -half[i]));
        int back = verts.Count;
        for (int i = 0; i < n; i++) verts.Add(new Vector3(pts[i].x, pts[i].y, half[i]));
        for (int i = 0; i < poly.Count; i += 3)
        {
            tris.Add(front + poly[i]); tris.Add(front + poly[i + 1]); tris.Add(front + poly[i + 2]);
            tris.Add(back + poly[i]); tris.Add(back + poly[i + 2]); tris.Add(back + poly[i + 1]);
        }
        // Normales plates : on duplique chaque sommet par triangle.
        var fv = new List<Vector3>(); var ft = new List<int>();
        for (int i = 0; i < tris.Count; i++) { fv.Add(verts[tris[i]]); ft.Add(i); }
        var m = new Mesh { name = "Enclume" };
        m.SetVertices(fv);
        m.SetTriangles(ft, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    static List<int> Triangulate(List<Vector2> p)
    {
        var idx = new List<int>();
        for (int i = 0; i < p.Count; i++) idx.Add(i);
        float area = 0; for (int i = 0; i < p.Count; i++) { var a = p[i]; var b = p[(i + 1) % p.Count]; area += a.x * b.y - b.x * a.y; }
        bool ccw = area > 0;
        var res = new List<int>();
        int guard = 0;
        while (idx.Count > 3 && guard++ < 500)
        {
            bool cut = false;
            for (int k = 0; k < idx.Count; k++)
            {
                int i0 = idx[(k + idx.Count - 1) % idx.Count], i1 = idx[k], i2 = idx[(k + 1) % idx.Count];
                Vector2 a = p[i0], b = p[i1], c = p[i2];
                float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
                if (ccw ? cross <= 0 : cross >= 0) continue;
                bool inside = false;
                foreach (int o in idx)
                {
                    if (o == i0 || o == i1 || o == i2) continue;
                    if (InTri(p[o], a, b, c)) { inside = true; break; }
                }
                if (inside) continue;
                res.Add(i0); res.Add(i1); res.Add(i2);
                idx.RemoveAt(k);
                cut = true;
                break;
            }
            if (!cut) break;
        }
        if (idx.Count == 3) { res.Add(idx[0]); res.Add(idx[1]); res.Add(idx[2]); }
        return res;
    }

    static bool InTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
        float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
        float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
        bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(neg && pos);
    }
}
