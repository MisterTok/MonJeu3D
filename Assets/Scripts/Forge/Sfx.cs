using System.Collections.Generic;
using UnityEngine;

// Effets sonores synthétisés au lancement (aucun fichier audio, aucune licence) :
// enclume, coups, critiques, pièces, révélation d'une pièce, clics, fanfare.
public static class Sfx
{
    const int Rate = 44100;
    static AudioSource src;
    static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    static readonly Dictionary<string, float> lastPlay = new Dictionary<string, float>();
    static System.Random rng = new System.Random(7);

    public static bool Muted => GameState.Data != null && GameState.Data.muted;

    static void Ensure()
    {
        if (src != null) return;
        var go = new GameObject("Sons");
        Object.DontDestroyOnLoad(go);
        src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
    }

    static void Play(string key, System.Func<float[]> make, float vol, float pitchJitter = 0.06f, float minGap = 0.04f)
    {
        if (Muted) return;
        Ensure();
        float now = Time.unscaledTime;
        if (lastPlay.TryGetValue(key, out float t) && now - t < minGap) return;
        lastPlay[key] = now;
        if (!clips.TryGetValue(key, out var clip))
        {
            var data = make();
            clip = AudioClip.Create(key, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            clips[key] = clip;
        }
        src.pitch = 1f + ((float)rng.NextDouble() * 2f - 1f) * pitchJitter;
        src.PlayOneShot(clip, vol);
    }

    // ---------- Synthèse ----------
    static float[] Buf(float seconds) => new float[(int)(seconds * Rate)];
    static float Noise() => (float)rng.NextDouble() * 2f - 1f;

    // Métal frappé : partiels inharmoniques qui s'éteignent, plus un claquement de bruit.
    static float[] Clang(float baseHz, float dur, float bright)
    {
        var b = Buf(dur);
        float[] ratio = { 1f, 2.76f, 5.4f, 8.93f, 13.3f };
        float[] amp = { 1f, 0.6f, 0.4f * bright, 0.25f * bright, 0.12f * bright };
        for (int i = 0; i < b.Length; i++)
        {
            float t = i / (float)Rate, v = 0f;
            for (int k = 0; k < ratio.Length; k++)
                v += amp[k] * Mathf.Sin(2f * Mathf.PI * baseHz * ratio[k] * t) * Mathf.Exp(-t * (4f + k * 3f));
            v += Noise() * Mathf.Exp(-t * 60f) * 0.8f;
            b[i] = v * 0.35f;
        }
        return b;
    }

    // Coup sourd : bruit filtré + grave qui descend.
    static float[] Thud(float hz, float dur, float noise)
    {
        var b = Buf(dur);
        float lp = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float t = i / (float)Rate;
            lp += (Noise() - lp) * 0.15f;
            float f = hz * (1f - t / dur * 0.5f);
            b[i] = (Mathf.Sin(2f * Mathf.PI * f * t) * 0.8f + lp * noise) * Mathf.Exp(-t * 22f) * 0.6f;
        }
        return b;
    }

    // Petites notes (pièces, carillons, fanfare).
    static float[] Notes(float[] hz, float step, float decay, float vol, bool square = false)
    {
        var b = Buf(step * hz.Length + 0.5f);
        for (int n = 0; n < hz.Length; n++)
        {
            int start = (int)(n * step * Rate);
            for (int i = start; i < b.Length; i++)
            {
                float t = (i - start) / (float)Rate;
                float ph = 2f * Mathf.PI * hz[n] * t;
                float w = square ? Mathf.Sign(Mathf.Sin(ph)) * 0.35f + Mathf.Sin(ph) * 0.5f : Mathf.Sin(ph) + 0.3f * Mathf.Sin(ph * 2f);
                b[i] += w * Mathf.Exp(-t * decay) * vol;
            }
        }
        return b;
    }

    static float[] Whoosh(float dur)
    {
        var b = Buf(dur);
        float lp = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float t = i / (float)Rate, x = t / dur;
            float k = Mathf.Lerp(0.02f, 0.3f, x);
            lp += (Noise() - lp) * k;
            b[i] = lp * Mathf.Sin(Mathf.PI * x) * 0.7f;
        }
        return b;
    }

    // ---------- Sons du jeu ----------
    public static void Anvil(float strength = 1f) => Play("enclume", () => Clang(310f, 0.9f, 1f), 0.55f * strength, 0.05f, 0.05f);
    public static void Hit() => Play("coup", () => Thud(140f, 0.16f, 0.9f), 0.35f, 0.12f, 0.05f);
    public static void Crit() => Play("critique", () => Clang(620f, 0.35f, 0.6f), 0.4f, 0.08f, 0.06f);
    public static void Hurt() => Play("douleur", () => Thud(90f, 0.2f, 0.5f), 0.3f, 0.1f, 0.08f);
    public static void Block() => Play("blocage", () => Clang(900f, 0.25f, 0.3f), 0.25f, 0.05f, 0.08f);
    public static void Coin() => Play("piece", () => Notes(new[] { 1318f, 1760f }, 0.06f, 18f, 0.25f), 0.35f, 0.04f, 0.07f);
    public static void Click() => Play("clic", () => Notes(new[] { 1900f }, 0.02f, 90f, 0.25f), 0.25f, 0.02f, 0.03f);
    public static void Cast() => Play("sort", () => Whoosh(0.45f), 0.4f, 0.1f, 0.1f);
    public static void Equip() => Play("equiper", () => Notes(new[] { 523f, 784f, 1046f }, 0.07f, 10f, 0.22f), 0.45f, 0f, 0.1f);
    public static void Summon() => Play("invocation", () => Notes(new[] { 784f, 988f, 1175f, 1568f }, 0.06f, 9f, 0.2f), 0.45f, 0f, 0.1f);
    public static void Fanfare() => Play("fanfare", () => Notes(new[] { 523f, 659f, 784f, 1046f }, 0.11f, 5f, 0.2f, true), 0.45f, 0f, 0.5f);

    // Révélation d'une pièce : plus le cercle est haut, plus le carillon est long et aigu.
    public static void Reveal(int circle)
    {
        int c = Mathf.Clamp(circle, 0, 9);
        int n = 2 + c / 2;
        Play("revelation" + c, () =>
        {
            float[] scale = { 523f, 659f, 784f, 1046f, 1318f, 1568f, 2093f };
            var hz = new float[n];
            for (int i = 0; i < n; i++) hz[i] = scale[Mathf.Min(scale.Length - 1, i + c / 3)];
            return Notes(hz, 0.08f, 6f, 0.18f);
        }, 0.5f, 0f, 0.2f);
    }
}
