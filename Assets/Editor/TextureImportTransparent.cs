#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Ensures PNGs in Art/ and Sprites/ are imported with transparency enabled
/// so transparent backgrounds display correctly in the scene.
/// </summary>
public class TextureImportTransparent : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (string.IsNullOrEmpty(assetPath)) return;
        if (!assetPath.StartsWith("Assets/Art/") && !assetPath.StartsWith("Assets/Sprites/"))
            return;
        if (!assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.alphaIsTransparency = true;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;
    }
}
#endif
