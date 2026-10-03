using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Construit l'APK Android : menu Tools, ou fichier ../_dl/build_request (résultat dans ../_dl/ForgeEnfer.apk + build_report.txt).
[InitializeOnLoad]
public static class ForgeBuild
{
    // Le nom du produit sert aussi de clé à la sauvegarde PlayerPrefs de l'éditeur : il doit rester « MonJeu3D ».
    const string EditorName = "MonJeu3D", AppName = "Forge de l'Enfer";

    static ForgeBuild()
    {
        EditorApplication.update += Poll;
        if (PlayerSettings.productName != EditorName) PlayerSettings.productName = EditorName;
    }

    static string DlDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../_dl"));
    static double next;

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup < next || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        next = EditorApplication.timeSinceStartup + 2.0;
        string req = Path.Combine(DlDir, "build_request");
        if (!File.Exists(req)) return;
        File.Delete(req);
        EditorApplication.delayCall += BuildApk;
    }

    [MenuItem("Tools/Forge : construire l'APK")]
    public static void BuildApk()
    {
        string apk = Path.Combine(DlDir, "ForgeEnfer.apk");
        string log = Path.Combine(DlDir, "build_report.txt");
        File.WriteAllText(log, "EN COURS " + System.DateTime.Now + "\n");
        PlayerSettings.productName = AppName;
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = apk,
            target = BuildTarget.Android,
            options = BuildOptions.None,
        };
        EditorUserBuildSettings.buildAppBundle = false;
        BuildReport report;
        try { report = BuildPipeline.BuildPlayer(opts); }
        finally { PlayerSettings.productName = EditorName; AssetDatabase.SaveAssets(); }
        var s = report.summary;
        var txt = "RESULTAT " + s.result + "\nTaille " + (s.totalSize / 1048576f).ToString("0.0") + " Mo\nDurée " + s.totalTime + "\nErreurs " + s.totalErrors + "\n";
        foreach (var step in report.steps)
            foreach (var m in step.messages)
                if (m.type == LogType.Error || m.type == LogType.Exception) txt += m.content + "\n";
        File.WriteAllText(log, txt);
    }
}
