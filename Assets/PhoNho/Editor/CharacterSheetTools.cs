using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace PhoNho.Art.Editor
{
    public static class CharacterSheetTools
    {
        private const string Root = "Assets/PhoNho/Art/Characters/";
        private const string ConfigPath = "Assets/PhoNho/Editor/CharacterSheets.json";
        [Serializable] private sealed class Profile { public string file; public int columns = 2; public int rows = 2; }
        [Serializable] private sealed class Profiles { public List<Profile> sheets = new List<Profile>(); }

        [MenuItem("Phố Nhỏ/Character/Làm sạch và căn toàn bộ")]
        public static void CleanAll()
        {
            Profiles config = LoadConfig(); int success = 0, failed = 0;
            try
            {
                for (int i = 0; i < config.sheets.Count; i++)
                {
                    Profile profile = config.sheets[i];
                    EditorUtility.DisplayProgressBar("Character", profile.file, i / (float)config.sheets.Count);
                    try { Process(profile); success++; }
                    catch (Exception e) { failed++; Debug.LogError($"Character: {profile.file}: {e.Message}"); }
                }
                string[] unknown = Directory.GetFiles(Root, "*.png", SearchOption.AllDirectories)
                    .Select(p => p.Replace('\\', '/').Substring(Root.Length))
                    .Where(p => !config.sheets.Any(s => s.file == p)).ToArray();
                if (unknown.Length > 0) Debug.LogWarning("Character: select grid for new sheets: " + string.Join(", ", unknown));
                Debug.Log($"Character: {success} processed, {failed} failed. Originals: img/character.");
            }
            finally { EditorUtility.ClearProgressBar(); AssetDatabase.Refresh(); }
            if (failed > 0) throw new InvalidOperationException($"Character cleanup failed for {failed} sheet(s).");
        }

        [MenuItem("Phố Nhỏ/Character/Căn ảnh chọn - 4 khung (2x2)")]
        public static void SelectedFour() => ProcessSelected(2, 2);
        [MenuItem("Phố Nhỏ/Character/Căn ảnh chọn - 8 khung (4x2)")]
        public static void SelectedEight() => ProcessSelected(4, 2);
        [MenuItem("Phố Nhỏ/Character/Căn ảnh chọn - 1 khung")]
        public static void SelectedSingle() => ProcessSelected(1, 1);

        private static Profiles LoadConfig() => File.Exists(ConfigPath)
            ? JsonUtility.FromJson<Profiles>(File.ReadAllText(ConfigPath)) : new Profiles();

        private static void ProcessSelected(int columns, int rows)
        {
            Profiles config = LoadConfig();
            foreach (UnityEngine.Object selected in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(selected).Replace('\\', '/');
                if (!assetPath.StartsWith(Root, StringComparison.Ordinal) ||
                    !assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                string file = assetPath.Substring(Root.Length);
                Profile profile = config.sheets.FirstOrDefault(p => p.file == file);
                if (profile == null) { profile = new Profile { file = file }; config.sheets.Add(profile); }
                profile.columns = columns; profile.rows = rows;
                Process(profile);
            }
            File.WriteAllText(ConfigPath, JsonUtility.ToJson(config, true)); AssetDatabase.Refresh();
        }

        private static void Process(Profile profile)
        {
            if (string.IsNullOrEmpty(profile.file) || profile.file.Contains("..") || Path.IsPathRooted(profile.file))
                throw new ArgumentException("Invalid character asset path.");
            string assetPath = Root + profile.file, sourcePath = "img/character/" + profile.file;
            if (!File.Exists(assetPath)) throw new FileNotFoundException(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
            if (!File.Exists(sourcePath)) File.Copy(assetPath, sourcePath);
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Texture2D output = null;
            try
            {
                if (!ImageConversion.LoadImage(source, File.ReadAllBytes(sourcePath), false)) throw new InvalidDataException(sourcePath);
                Color32[] topDown = FlipRows(source.GetPixels32(), source.width, source.height);
                Color32[] clean = CharacterSheetPixels.Clean(topDown, source.width, source.height,
                    profile.columns, profile.rows, out List<CharacterSheetPixels.Frame> frames);
                Color32[] normalized = CharacterSheetPixels.Normalize(clean, source.width, frames, profile.columns, profile.rows);
                int width = profile.columns * CharacterSheetPixels.CellWidth, height = profile.rows * CharacterSheetPixels.CellHeight;
                output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(FlipRows(normalized, width, height)); output.Apply(false, false);
                File.WriteAllBytes(assetPath, ImageConversion.EncodeToPNG(output));
            }
            finally { UnityEngine.Object.DestroyImmediate(source); if (output != null) UnityEngine.Object.DestroyImmediate(output); }
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Slice(assetPath, profile.columns, profile.rows);
        }

        private static Color32[] FlipRows(Color32[] pixels, int width, int height)
        {
            var output = new Color32[pixels.Length];
            for (int y = 0; y < height; y++) Array.Copy(pixels, y * width, output, (height - 1 - y) * width, width);
            return output;
        }

        private static void Slice(string path, int columns, int rows)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("2D Sprite data provider unavailable.");
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] previous = provider.GetSpriteRects(), rects = new SpriteRect[columns * rows];
            // Keep names and IDs when this sheet has already been sliced by this tool.
            for (int i = 0; i < rects.Length; i++)
            {
                SpriteRect old = previous.FirstOrDefault(p => p.name == Path.GetFileNameWithoutExtension(path) + "_" + i);
                if (old == null && previous.Length == rects.Length)
                    old = previous.OrderByDescending(p => p.rect.y).ThenBy(p => p.rect.x).ElementAt(i);
                rects[i] = new SpriteRect { name = old != null ? old.name : Path.GetFileNameWithoutExtension(path) + "_" + i,
                    spriteID = old != null ? old.spriteID : GUID.Generate(), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, .1f), rect = new Rect((i % columns) * CharacterSheetPixels.CellWidth,
                        (rows - 1 - i / columns) * CharacterSheetPixels.CellHeight, CharacterSheetPixels.CellWidth, CharacterSheetPixels.CellHeight) };
            }
            provider.SetSpriteRects(rects);
            var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
    }
}
