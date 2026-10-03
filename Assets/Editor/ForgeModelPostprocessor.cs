using UnityEditor;

// Réglages d'import automatiques pour les modèles Quaternius du dossier Resources/ForgeModels.
public class ForgeModelPostprocessor : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!assetPath.Contains("Resources/ForgeModels")) return;
        var mi = (ModelImporter)assetImporter;
        mi.importCameras = false;
        mi.importLights = false;
        mi.isReadable = false;
        if (assetPath.Contains("/Characters/") || assetPath.Contains("/Pets/"))
        {
            // Animations « Legacy » : on les joue directement par code, sans Animator Controller.
            mi.animationType = ModelImporterAnimationType.Legacy;
            mi.importAnimation = true;
        }
        else
        {
            mi.animationType = ModelImporterAnimationType.None;
            mi.importAnimation = false;
        }
    }
}
