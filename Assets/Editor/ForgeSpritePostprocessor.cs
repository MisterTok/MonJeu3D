using UnityEditor;
using UnityEngine;

// Planches de personnages 2D (Resources/ForgeSprites) : importées en sprites, sans mipmaps ni redimensionnement.
public class ForgeSpritePostprocessor : AssetPostprocessor
{
    public override uint GetVersion() => 1;

    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/ForgeSprites/")) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.maxTextureSize = 2048;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Compressed;
    }
}
