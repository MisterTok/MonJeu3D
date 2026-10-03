using System;
using UnityEngine;

// Effets visuels des compétences : projectiles lumineux, explosions, auras (tout en particules, construit par code).
public static class SkillFx
{
    // Matériaux mis en cache par couleur (évite d'en créer un à chaque tir).
    static readonly System.Collections.Generic.Dictionary<Color, Material> particleCache = new System.Collections.Generic.Dictionary<Color, Material>();
    static readonly System.Collections.Generic.Dictionary<Color, Material> headCache = new System.Collections.Generic.Dictionary<Color, Material>();

    static Material Mat(Color c)
    {
        if (!particleCache.TryGetValue(c, out var m)) particleCache[c] = m = ForgeWorld.Particle(new Color(c.r * 2.4f, c.g * 2.4f, c.b * 2.4f, 1f));
        return m;
    }

    static Material HeadMat(Color c)
    {
        if (!headCache.TryGetValue(c, out var m)) headCache[c] = m = ForgeWorld.Unlit(new Color(c.r * 3f, c.g * 3f, c.b * 3f));
        return m;
    }

    static ParticleSystem NewSystem(string name, Transform parent, Vector3 worldPos, Color c, bool world = true)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.position = worldPos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.simulationSpace = world ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        m.startColor = new Color(c.r, c.g, c.b, 1f);
        m.playOnAwake = false;
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(c, 0.3f), new GradientColorKey(c * 0.5f, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = Mat(c);
        return ps;
    }

    // Explosion ponctuelle (s'autodétruit).
    public static void Burst(Vector3 pos, Color c, float size = 1f, int count = 40)
    {
        var ps = NewSystem("Explosion", null, pos, c);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f * size, 4.5f * size);
        m.startSize = new ParticleSystem.MinMaxCurve(0.12f * size, 0.35f * size);
        m.gravityModifier = 0.4f;
        m.maxParticles = count + 10;
        var e = ps.emission; e.rateOverTime = 0f; e.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        var s = ps.shape; s.shapeType = ParticleSystemShapeType.Sphere; s.radius = 0.15f * size;
        var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.1f));
        ps.Play();
        // Éclair de lumière
        var l = ps.gameObject.AddComponent<Light>();
        l.type = LightType.Point; l.color = c; l.range = 4f * size; l.intensity = 8f;
        ps.gameObject.AddComponent<FxLife>().Init(1.2f, l);
    }

    // Projectile qui vole jusqu'à la cible puis explose. target est relu chaque image (la cible peut avancer).
    public static void Projectile(Vector3 from, Func<Vector3> target, Color c, float speed, float arc, float size, Action onHit)
    {
        var ps = NewSystem("Projectile", null, from, c);
        var m = ps.main;
        m.startLifetime = 0.35f;
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(0.18f * size, 0.32f * size);
        m.maxParticles = 120;
        var e = ps.emission; e.rateOverTime = 0f; e.rateOverDistance = 18f;
        var s = ps.shape; s.enabled = false;
        var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0f));
        // Tête du projectile : une boule lumineuse
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        UnityEngine.Object.Destroy(head.GetComponent<Collider>());
        head.transform.SetParent(ps.transform, false);
        head.transform.localScale = Vector3.one * 0.22f * size;
        head.GetComponent<Renderer>().sharedMaterial = HeadMat(c);
        ps.Play();
        ps.gameObject.AddComponent<FxProjectile>().Init(from, target, speed, arc, c, size, onHit);
    }

    // Aura qui suit un objet pendant « seconds ».
    public static GameObject Aura(Transform follow, Color c, float seconds, float radius = 0.6f)
    {
        var ps = NewSystem("Aura", follow, follow.position, c);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        m.maxParticles = 120;
        m.gravityModifier = -0.15f;
        var e = ps.emission; e.rateOverTime = 45f;
        var s = ps.shape; s.shapeType = ParticleSystemShapeType.Circle; s.radius = radius; s.rotation = new Vector3(-90f, 0f, 0f);
        ps.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        ps.Play();
        ps.gameObject.AddComponent<FxLife>().Init(seconds, null, true);
        return ps.gameObject;
    }

    // Onde de choc horizontale (anneau qui s'élargit).
    public static void Shockwave(Vector3 pos, Color c, float radius)
    {
        var ps = NewSystem("Onde", null, pos, c);
        var m = ps.main;
        m.startLifetime = 0.5f;
        m.startSpeed = radius * 2f;
        m.startSize = 0.25f;
        m.maxParticles = 80;
        var e = ps.emission; e.rateOverTime = 0f; e.SetBursts(new[] { new ParticleSystem.Burst(0f, 70) });
        var s = ps.shape; s.shapeType = ParticleSystemShapeType.Circle; s.radius = 0.1f; s.radiusThickness = 0f; s.rotation = new Vector3(-90f, 0f, 0f);
        ps.Play();
        ps.gameObject.AddComponent<FxLife>().Init(1f, null);
    }
}

// Durée de vie d'un effet : arrête l'émission puis détruit l'objet.
public class FxLife : MonoBehaviour
{
    float life, t; Light glow; bool fadeEmit; float lightStart;
    public void Init(float seconds, Light l, bool stopEmitting = false) { life = seconds; glow = l; fadeEmit = stopEmitting; if (l != null) lightStart = l.intensity; }
    void Update()
    {
        t += Time.deltaTime;
        if (glow != null) glow.intensity = Mathf.Lerp(lightStart, 0f, t / 0.4f);
        if (fadeEmit && t >= life)
        {
            fadeEmit = false;
            var ps = GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            transform.SetParent(null, true);
            life += 1.2f;
        }
        if (!fadeEmit && t >= life) Destroy(gameObject);
    }
}

public class FxProjectile : MonoBehaviour
{
    Vector3 from; Func<Vector3> target; float speed, arc, size, t, dur; Color col; Action onHit; bool done;
    public void Init(Vector3 f, Func<Vector3> tg, float sp, float a, Color c, float s, Action hit)
    {
        from = f; target = tg; speed = sp; arc = a; col = c; size = s; onHit = hit;
        dur = Mathf.Max(0.15f, Vector3.Distance(f, tg()) / Mathf.Max(0.1f, sp));
    }
    void Update()
    {
        if (done) { t += Time.deltaTime; if (t > 0.5f) Destroy(gameObject); return; }
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / dur);
        var to = target();
        var p = Vector3.Lerp(from, to, k) + Vector3.up * arc * 4f * k * (1f - k);
        transform.position = p;
        if (k >= 1f)
        {
            done = true; t = 0f;
            foreach (Transform c in transform) Destroy(c.gameObject);
            var ps = GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            SkillFx.Burst(to, col, size);
            onHit?.Invoke();
        }
    }
}
