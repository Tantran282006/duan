using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Generates high-resolution Vietnamese cozy chibi/pixel art styled textures
    /// for the Honey Breakfast Shop Interior (walls, floor, counter, dining table, exit door, menu board).
    /// </summary>
    public static class HoneyBreakfastTextureGenerator
    {
        public const string ArtFolder = "Assets/PhoNho/Art/Backgrounds";

        public static readonly string WallPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Wall.png";
        public static readonly string FloorPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Floor.png";
        public static readonly string CounterPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Counter.png";
        public static readonly string DiningTablePath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_DiningTable.png";
        public static readonly string ExitDoorPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_ExitDoor.png";
        public static readonly string MenuBoardPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_MenuBoard.png";

        [MenuItem("Phố Nhỏ/Art/Tạo Texture Nội Thất Quán Ăn Sáng")]
        public static void GenerateAllTextures()
        {
            Directory.CreateDirectory(ArtFolder);

            GenerateWallTexture();
            GenerateFloorTexture();
            GenerateCounterTexture();
            GenerateDiningTableTexture();
            GenerateExitDoorTexture();
            GenerateMenuBoardTexture();

            AssetDatabase.Refresh();
            ConfigureImporter(WallPath, 100);
            ConfigureImporter(FloorPath, 100);
            ConfigureImporter(CounterPath, 100);
            ConfigureImporter(DiningTablePath, 100);
            ConfigureImporter(ExitDoorPath, 100);
            ConfigureImporter(MenuBoardPath, 100);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HoneyBreakfastTextureGenerator] Hoàn thành tạo bộ Texture Nội Thất Quán Ăn Sáng!");
        }

        private static void ConfigureImporter(string path, float ppu)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        private static void GenerateWallTexture()
        {
            int w = 1920, h = 1080;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color wallBase = new Color(0.98f, 0.93f, 0.82f, 1f); // Warm honey cream
            Color wallShade = new Color(0.95f, 0.88f, 0.74f, 1f);
            Color woodBrown = new Color(0.42f, 0.25f, 0.15f, 1f); // Indochine dark wood
            Color woodLight = new Color(0.55f, 0.35f, 0.20f, 1f);
            Color windowGlass = new Color(0.88f, 0.94f, 0.98f, 1f);
            Color windowFrame = new Color(0.72f, 0.50f, 0.30f, 1f);
            Color lanternRed = new Color(0.82f, 0.22f, 0.18f, 1f);
            Color lanternGold = new Color(0.95f, 0.78f, 0.25f, 1f);

            Color[] colors = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float tY = (float)y / h;
                for (int x = 0; x < w; x++)
                {
                    Color c = Color.Lerp(wallShade, wallBase, tY);

                    // Top ceiling wooden molding (y > 1020)
                    if (y >= 1020)
                    {
                        c = (y % 12 < 3) ? woodBrown * 0.85f : woodBrown;
                    }
                    // Bottom wooden wainscoting (y < 240)
                    else if (y < 240)
                    {
                        if (y >= 225)
                        {
                            c = woodLight; // Baseboard trim
                        }
                        else
                        {
                            // Vertical wood paneling stripes
                            bool isGroove = (x % 32 < 3);
                            c = isGroove ? woodBrown * 0.8f : woodBrown;
                        }
                    }
                    else
                    {
                        // Large central arched sun window (x: 820..1100, y: 480..880)
                        if (x >= 820 && x <= 1100 && y >= 480 && y <= 880)
                        {
                            bool isFrame = (x <= 835 || x >= 1085 || y <= 495 || y >= 865 || Mathf.Abs(x - 960) <= 6 || Mathf.Abs(y - 680) <= 6);
                            if (isFrame)
                            {
                                c = windowFrame;
                            }
                            else
                            {
                                // Sunlight gradient through window
                                float distToCenter = Mathf.Abs(x - 960) / 140f;
                                c = Color.Lerp(new Color(1f, 0.98f, 0.90f, 1f), windowGlass, distToCenter);
                            }
                        }

                        // Left Lantern (x: 240..300, y: 880..980)
                        if (x >= 240 && x <= 300 && y >= 880 && y <= 980)
                        {
                            float dx = (x - 270) / 30f;
                            float dy = (y - 930) / 50f;
                            if (dx * dx + dy * dy <= 1f)
                            {
                                c = lanternRed;
                                if (Mathf.Abs(dy) < 0.2f || Mathf.Abs(dx) < 0.15f) c = lanternGold;
                            }
                        }

                        // Right Lantern (x: 1620..1680, y: 880..980)
                        if (x >= 1620 && x <= 1680 && y >= 880 && y <= 980)
                        {
                            float dx = (x - 1650) / 30f;
                            float dy = (y - 930) / 50f;
                            if (dx * dx + dy * dy <= 1f)
                            {
                                c = lanternRed;
                                if (Mathf.Abs(dy) < 0.2f || Mathf.Abs(dx) < 0.15f) c = lanternGold;
                            }
                        }

                        // Wall art frames: Left Frame (x: 440..620, y: 560..760)
                        if (x >= 440 && x <= 620 && y >= 560 && y <= 760)
                        {
                            bool isBorder = (x <= 450 || x >= 610 || y <= 570 || y >= 750);
                            c = isBorder ? woodLight : new Color(0.96f, 0.90f, 0.78f, 1f);
                        }

                        // Right Frame (x: 1300..1480, y: 560..760)
                        if (x >= 1300 && x <= 1480 && y >= 560 && y <= 760)
                        {
                            bool isBorder = (x <= 1310 || x >= 1470 || y <= 570 || y >= 750);
                            c = isBorder ? woodLight : new Color(0.96f, 0.90f, 0.78f, 1f);
                        }
                    }

                    colors[y * w + x] = c;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(WallPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateFloorTexture()
        {
            int w = 1024, h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color tileCream = new Color(0.96f, 0.94f, 0.88f, 1f);
            Color tileBlue = new Color(0.24f, 0.38f, 0.50f, 1f);
            Color tileOchre = new Color(0.82f, 0.62f, 0.32f, 1f);
            Color grout = new Color(0.30f, 0.28f, 0.25f, 1f);

            Color[] colors = new Color[w * h];
            int tileSize = 64;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int localX = x % tileSize;
                    int localY = y % tileSize;

                    if (localX == 0 || localX == tileSize - 1 || localY == 0 || localY == tileSize - 1)
                    {
                        colors[y * w + x] = grout;
                        continue;
                    }

                    // Symmetrical Indochine geometric lotus/flower pattern
                    int cx = Mathf.Abs(localX - 32);
                    int cy = Mathf.Abs(localY - 32);
                    int dist = cx + cy;

                    if (dist < 12)
                    {
                        colors[y * w + x] = tileOchre; // Lotus center
                    }
                    else if (dist >= 16 && dist <= 24)
                    {
                        colors[y * w + x] = tileBlue; // Petal rim
                    }
                    else
                    {
                        colors[y * w + x] = tileCream; // Background tile
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(FloorPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateCounterTexture()
        {
            int w = 600, h = 400;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color woodDark = new Color(0.48f, 0.26f, 0.12f, 1f);
            Color woodMid = new Color(0.62f, 0.36f, 0.18f, 1f);
            Color marble = new Color(0.92f, 0.92f, 0.90f, 1f);
            Color stoveMetal = new Color(0.25f, 0.25f, 0.26f, 1f);
            Color panMetal = new Color(0.18f, 0.18f, 0.18f, 1f);
            Color eggWhite = new Color(0.98f, 0.98f, 0.95f, 1f);
            Color eggYolk = new Color(0.98f, 0.65f, 0.10f, 1f);
            Color potMetal = new Color(0.75f, 0.76f, 0.78f, 1f);
            Color breadGold = new Color(0.88f, 0.60f, 0.22f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 1. Counter base body (y: 20..220, x: 20..580)
                    if (y >= 20 && y <= 220 && x >= 20 && x <= 580)
                    {
                        bool panelBorder = (x <= 30 || x >= 570 || y <= 30 || y >= 210 || x == 180 || x == 300 || x == 440);
                        colors[y * w + x] = panelBorder ? woodDark : woodMid;
                    }
                    // 2. Counter Top Marble Slab (y: 220..250, x: 10..590)
                    else if (y >= 220 && y <= 250 && x >= 10 && x <= 590)
                    {
                        colors[y * w + x] = (y >= 246) ? marble * 1.05f : marble;
                    }
                    // 3. Stove & Frying Pan with Sunny-Side Up Eggs (Left side: x: 70..210, y: 250..320)
                    else if (y >= 250 && y <= 265 && x >= 70 && x <= 210)
                    {
                        colors[y * w + x] = stoveMetal; // Stove burner base
                    }
                    else if (y >= 265 && y <= 315 && x >= 85 && x <= 195)
                    {
                        float dx = (x - 140) / 55f;
                        float dy = (y - 290) / 25f;
                        if (dx * dx + dy * dy <= 1f)
                        {
                            // Inside frying pan
                            float inner = dx * dx + dy * dy;
                            if (inner >= 0.85f)
                            {
                                colors[y * w + x] = panMetal; // Rim
                            }
                            else
                            {
                                // Eggs! Two sunny side up eggs
                                float dEgg1 = Mathf.Sqrt(Mathf.Pow((x - 120) / 20f, 2) + Mathf.Pow((y - 290) / 14f, 2));
                                float dEgg2 = Mathf.Sqrt(Mathf.Pow((x - 160) / 20f, 2) + Mathf.Pow((y - 290) / 14f, 2));

                                if (dEgg1 < 0.45f || dEgg2 < 0.45f)
                                {
                                    colors[y * w + x] = eggYolk; // Yolks!
                                }
                                else if (dEgg1 < 1f || dEgg2 < 1f)
                                {
                                    colors[y * w + x] = eggWhite; // Whites
                                }
                                else
                                {
                                    colors[y * w + x] = panMetal * 0.7f; // Dark pan surface
                                }
                            }
                        }
                    }
                    // 4. Large Soup/Pho Pot (Center: x: 260..360, y: 250..370)
                    else if (y >= 250 && y <= 370 && x >= 260 && x <= 360)
                    {
                        bool isHandle = (y >= 300 && y <= 330 && (x <= 268 || x >= 352));
                        bool isLid = (y >= 355);
                        if (x >= 270 && x <= 350 || isHandle)
                        {
                            colors[y * w + x] = isLid ? potMetal * 1.1f : potMetal;
                        }
                    }
                    // 5. Bread Basket with golden baguettes (Right: x: 420..540, y: 250..340)
                    else if (y >= 250 && y <= 280 && x >= 420 && x <= 540)
                    {
                        colors[y * w + x] = woodDark; // Woven basket
                    }
                    else if (y >= 280 && y <= 340 && x >= 430 && x <= 530)
                    {
                        // Baguettes sticking out
                        int subBaguette = (x - 430) / 25;
                        colors[y * w + x] = (subBaguette % 2 == 0) ? breadGold : breadGold * 0.9f;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(CounterPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateDiningTableTexture()
        {
            int w = 400, h = 320;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color woodDark = new Color(0.44f, 0.24f, 0.12f, 1f);
            Color woodMid = new Color(0.60f, 0.36f, 0.18f, 1f);
            Color chairWood = new Color(0.50f, 0.28f, 0.14f, 1f);
            Color condimentRed = new Color(0.85f, 0.20f, 0.15f, 1f);
            Color plateWhite = new Color(0.95f, 0.95f, 0.92f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Left Chair (x: 30..90, y: 20..220)
                    if (x >= 30 && x <= 90)
                    {
                        bool isSeat = (y >= 110 && y <= 130);
                        bool isLeg = (y < 110 && (x <= 45 || x >= 75));
                        bool isBack = (y >= 130 && y <= 210 && x <= 45);
                        if (isSeat || isLeg || isBack) colors[y * w + x] = chairWood;
                    }

                    // Right Chair (x: 310..370, y: 20..220)
                    if (x >= 310 && x <= 370)
                    {
                        bool isSeat = (y >= 110 && y <= 130);
                        bool isLeg = (y < 110 && (x <= 325 || x >= 355));
                        bool isBack = (y >= 130 && y <= 210 && x >= 355);
                        if (isSeat || isLeg || isBack) colors[y * w + x] = chairWood;
                    }

                    // Table Legs (x: 120..140, 260..280, y: 20..150)
                    if (y >= 20 && y <= 150 && ((x >= 120 && x <= 140) || (x >= 260 && x <= 280)))
                    {
                        colors[y * w + x] = woodDark;
                    }

                    // Table Top Slab (x: 95..305, y: 150..185)
                    if (y >= 150 && y <= 185 && x >= 95 && x <= 305)
                    {
                        colors[y * w + x] = (y >= 180) ? woodMid * 1.1f : woodMid;
                    }

                    // Plate with food on table (x: 160..240, y: 185..215)
                    if (y >= 185 && y <= 215 && x >= 160 && x <= 240)
                    {
                        float dx = (x - 200) / 40f;
                        float dy = (y - 200) / 15f;
                        if (dx * dx + dy * dy <= 1f)
                        {
                            colors[y * w + x] = (dx * dx + dy * dy < 0.5f) ? new Color(0.95f, 0.70f, 0.25f, 1f) : plateWhite;
                        }
                    }

                    // Chopsticks holder & Chili sauce bottle (x: 255..285, y: 185..245)
                    if (y >= 185 && y <= 245 && x >= 255 && x <= 285)
                    {
                        colors[y * w + x] = (x <= 268) ? woodDark : condimentRed;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(DiningTablePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateExitDoorTexture()
        {
            int w = 360, h = 580;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color woodDark = new Color(0.40f, 0.22f, 0.12f, 1f);
            Color woodMid = new Color(0.55f, 0.32f, 0.18f, 1f);
            Color glass = new Color(0.85f, 0.92f, 0.98f, 0.75f); // Transparent street view glass
            Color brassHandle = new Color(0.88f, 0.72f, 0.28f, 1f);
            Color welcomeMat = new Color(0.75f, 0.20f, 0.20f, 1f);
            Color matGold = new Color(0.95f, 0.80f, 0.30f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Welcome Mat (y: 0..40, x: 20..340)
                    if (y <= 40 && x >= 20 && x <= 340)
                    {
                        bool isBorder = (x <= 35 || x >= 325 || y <= 8 || y >= 32);
                        colors[y * w + x] = isBorder ? matGold : welcomeMat;
                        continue;
                    }

                    // Door Outer Frame (y: 40..540, x: 30..330)
                    if (y >= 40 && y <= 540 && x >= 30 && x <= 330)
                    {
                        bool isFrame = (x <= 55 || x >= 305 || y <= 65 || y >= 515 || Mathf.Abs(y - 280) <= 10);
                        if (isFrame)
                        {
                            colors[y * w + x] = woodDark;
                        }
                        else
                        {
                            // Upper Glass Pane (y: 290..515)
                            if (y > 290)
                            {
                                colors[y * w + x] = glass;
                            }
                            // Lower Wooden Panel (y: 65..270)
                            else
                            {
                                colors[y * w + x] = woodMid;
                            }
                        }

                        // Brass Handle (x: 270..285, y: 260..300)
                        if (x >= 270 && x <= 285 && y >= 260 && y <= 300)
                        {
                            colors[y * w + x] = brassHandle;
                        }
                    }

                    // Signboard "LỐI RA" above door (y: 540..575, x: 60..300)
                    if (y >= 540 && y <= 575 && x >= 60 && x <= 300)
                    {
                        bool isSignBorder = (x <= 68 || x >= 292 || y <= 544 || y >= 571);
                        colors[y * w + x] = isSignBorder ? woodDark : matGold;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(ExitDoorPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateMenuBoardTexture()
        {
            int w = 320, h = 240;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color woodFrame = new Color(0.50f, 0.30f, 0.16f, 1f);
            Color blackboard = new Color(0.14f, 0.16f, 0.15f, 1f);
            Color chalkWhite = new Color(0.92f, 0.94f, 0.92f, 1f);
            Color chalkGold = new Color(0.96f, 0.82f, 0.32f, 1f);

            Color[] colors = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isBorder = (x <= 16 || x >= 304 || y <= 16 || y >= 224);
                    if (isBorder)
                    {
                        colors[y * w + x] = woodFrame;
                    }
                    else
                    {
                        Color c = blackboard;

                        // Title chalk line (y: 190..196, x: 50..270)
                        if (y >= 190 && y <= 196 && x >= 50 && x <= 270) c = chalkGold;

                        // Menu line 1: Bánh mì ốp la (y: 145..151, x: 40..280)
                        if (y >= 145 && y <= 151 && x >= 40 && x <= 280) c = chalkWhite;

                        // Menu line 2: Phở bò (y: 105..111, x: 40..280)
                        if (y >= 105 && y <= 111 && x >= 40 && x <= 280) c = chalkWhite;

                        // Bottom slogan (y: 50..54, x: 70..250)
                        if (y >= 50 && y <= 54 && x >= 70 && x <= 250) c = chalkGold;

                        colors[y * w + x] = c;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(MenuBoardPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
