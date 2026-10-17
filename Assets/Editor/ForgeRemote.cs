using System.IO;
using UnityEditor;
using UnityEngine;

// Pilotage de l'éditeur sans écran (Claude, téléphone) : fichiers déposés dans ../_dl
//   refresh_request -> Assets > Refresh (recompile)    play_request -> lance Play    stop_request -> arrête Play
[InitializeOnLoad]
public static class ForgeRemote
{
    static double next;
    static string DlDir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "_dl"));

    static ForgeRemote() { EditorApplication.update += Poll; }

    static bool Take(string name)
    {
        string p = Path.Combine(DlDir, name);
        if (!File.Exists(p)) return false;
        File.Delete(p);
        return true;
    }

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup < next || EditorApplication.isCompiling) return;
        next = EditorApplication.timeSinceStartup + 1.0;
        if (Take("stop_request") && EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (Take("refresh_request")) { AssetDatabase.Refresh(); return; }
        if (Take("play_request")) EditorApplication.isPlaying = true;
    }
}
