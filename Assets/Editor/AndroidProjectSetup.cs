using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Applique la configuration Android du projet.
// S'exécute automatiquement une fois (tant que l'identifiant d'application est celui du template),
// et reste disponible via le menu Tools > Configurer Android.
[InitializeOnLoad]
public static class AndroidProjectSetup
{
    const string CompanyName = "MisterTok";
    const string ApplicationId = "com.mistertok.monjeu3d";

    static AndroidProjectSetup()
    {
        if (Application.isBatchMode)
            return;

        if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) != ApplicationId)
            EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Configurer Android")]
    public static void Apply()
    {
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);

        // IL2CPP + ARM64 : obligatoire pour publier sur le Google Play Store.
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        AssetDatabase.SaveAssets();

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        Debug.Log($"Projet configuré pour Android ({ApplicationId}).");
    }
}
