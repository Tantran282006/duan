using UnityEditor;
using UnityEngine;

namespace PhoNho.Art.Editor
{
    // Import cleanup only: source files and frame geometry stay unchanged.
    public sealed class CharacterTexturePostprocessor : AssetPostprocessor
    {
        private bool IsCharacter => assetPath.StartsWith("Assets/PhoNho/Art/Characters/", System.StringComparison.Ordinal)
            && assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);

        private void OnPreprocessTexture()
        {
            if (!IsCharacter) return;
            var importer = (TextureImporter)assetImporter;
            if (importer.importSettingsMissing) { importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 256; }
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            if (!IsCharacter) return;
            Color32[] pixels = texture.GetPixels32(); CharacterSheetPixels.ScrubLowAlpha(pixels);
            texture.SetPixels32(pixels); texture.Apply(false, false);
        }
    }
}
