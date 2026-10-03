using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Outil de diagnostic : si le fichier ../_dl/report_request existe, écrit un rapport sur les modèles importés.
[InitializeOnLoad]
public static class ForgeModelReport
{
    static ForgeModelReport() { EditorApplication.delayCall += Check; }

    static string DlDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../_dl"));

    [MenuItem("Tools/Forge : rapport des modèles")]
    public static void Write()
    {
        var sb = new StringBuilder();
        foreach (var guid in AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Resources/ForgeModels" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var b = new Bounds();
            bool has = false;
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
            sb.Append(path.Replace("Assets/Resources/ForgeModels/", "")).Append(" | root rot ").Append(go.transform.localEulerAngles)
              .Append(" scale ").Append(go.transform.localScale).Append(" | bounds c ").Append(b.center).Append(" s ").Append(b.size);
            var anim = go.GetComponent<Animation>();
            if (anim != null)
            {
                sb.Append(" | clips:");
                foreach (AnimationState s in anim) sb.Append(' ').Append(s.name).Append('(').Append(s.length.ToString("0.00")).Append(')');
            }
            var mats = new System.Collections.Generic.List<string>();
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials) if (m != null) mats.Add(m.name + ":" + (m.shader != null ? m.shader.name : "?"));
            sb.Append(" | mats: ").Append(string.Join(", ", mats.ToArray()));
            sb.AppendLine();
        }
        Directory.CreateDirectory(DlDir);
        File.WriteAllText(Path.Combine(DlDir, "models_report.txt"), sb.ToString());
        Debug.Log("Forge : rapport des modèles écrit.");
    }

    static void Check()
    {
        string req = Path.Combine(DlDir, "report_request");
        if (!File.Exists(req)) return;
        File.Delete(req);
        Write();
    }
}
