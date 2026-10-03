using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Crée les matériaux de base dans Assets/Resources/ForgeMats pour qu'ils soient inclus dans l'APK
// (sinon Unity peut retirer les shaders URP utilisés uniquement par code).
[InitializeOnLoad]
public static class ForgeMaterialsSetup
{
    const string Dir = "Assets/Resources/ForgeMats";

    static ForgeMaterialsSetup()
    {
        EditorApplication.delayCall += Ensure;
    }

    [MenuItem("Tools/Forge : recréer les matériaux")]
    public static void Ensure()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Resources", "ForgeMats");

        Make("LitEmissive", "Universal Render Pipeline/Lit", m =>
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        });
        Make("Unlit", "Universal Render Pipeline/Unlit", m => { });
        Make("ParticleAdd", "Universal Render Pipeline/Particles/Unlit", m =>
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 2f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = 3000;
        });
    }

    static void Make(string name, string shaderName, System.Action<Material> setup)
    {
        string path = Dir + "/" + name + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        var shader = Shader.Find(shaderName);
        if (shader == null) { Debug.LogWarning("Forge : shader introuvable " + shaderName); return; }
        var m = new Material(shader);
        setup(m);
        AssetDatabase.CreateAsset(m, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Forge : matériau créé " + path);
    }
}
