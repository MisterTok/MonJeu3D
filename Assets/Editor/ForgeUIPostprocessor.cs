using UnityEditor;
using UnityEngine;

// Skin d'interface (Resources/ForgeUI) : textures nettes, sans mipmaps ni compression ni redimensionnement.
public class ForgeUIPostprocessor : AssetPostprocessor
{
    public override uint GetVersion() => 3;

    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/ForgeUI/")) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Default;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        // Décor du combat : compressé (beaucoup de grandes images), sol avec mipmaps (vu en biais).
        if (assetPath.Contains("/Battle/"))
        {
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.mipmapEnabled = assetPath.EndsWith("_ground.png");
        }
        // Compagnons et montures dessinés : compressés, lisibles (recadrage du sprite de combat), mipmaps (réduits à l'écran).
        if (assetPath.Contains("/Creatures/"))
        {
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.mipmapEnabled = true;
            ti.isReadable = true;
        }
    }
}
