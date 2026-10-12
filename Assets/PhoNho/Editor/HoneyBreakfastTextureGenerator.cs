using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Generates high-resolution Vietnamese cozy Chibi & Pixel Art styled textures
    /// for Honey Breakfast Shop Interior and the Cooking station mini-game.
    /// Produces rich, warm textures without relying on basic primitives.
    /// </summary>
    public static class HoneyBreakfastTextureGenerator
    {
        public const string ArtFolder = "Assets/PhoNho/Art/Backgrounds";

        // Interior scene assets
        public static readonly string WallPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Wall.png";
        public static readonly string FloorPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Floor.png";
        public static readonly string CounterPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Counter.png";
        public static readonly string DiningTablePath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_DiningTable.png";
        public static readonly string ExitDoorPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_ExitDoor.png";
        public static readonly string MenuBoardPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_MenuBoard.png";
        public static readonly string IngredientShelfPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_IngredientShelf.png";
        public static readonly string DecorationsPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_Decorations.png";
        public static readonly string NPCCustomersPath = $"{ArtFolder}/ShopInterior_HoneyBreakfast_NPC_Customers.png";

        // Cooking scene assets
        public static readonly string CookingStationBackgroundPath = $"{ArtFolder}/Cooking_Station_Background.png";
        public static readonly string CookingStoveFlamePath = $"{ArtFolder}/Cooking_Stove_Flame.png";
        public static readonly string CookingPanStatesPath = $"{ArtFolder}/Cooking_Pan_States.png";
        public static readonly string CookingIngredientsTrayPath = $"{ArtFolder}/Cooking_Ingredients_Tray.png";
        public static readonly string CookingFinishedDishPath = $"{ArtFolder}/Cooking_Finished_Dish.png";

        [MenuItem("Phố Nhỏ/Art/Tạo Toàn Bộ Texture Quán & Khu Nấu Ăn")]
        public static void GenerateAllTextures()
        {
            Directory.CreateDirectory(ArtFolder);

            // 1. Interior Textures
            GenerateWallTexture();
            GenerateFloorTexture();
            GenerateCounterTexture();
            GenerateDiningTableTexture();
            GenerateExitDoorTexture();
            GenerateMenuBoardTexture();
            GenerateIngredientShelfTexture();
            GenerateDecorationsTexture();
            GenerateNPCCustomersTexture();

            // 2. Cooking Station Textures
            GenerateCookingStationBackgroundTexture();
            GenerateCookingStoveFlameTexture();
            GenerateCookingPanStatesTexture();
            GenerateCookingIngredientsTrayTexture();
            GenerateCookingFinishedDishTexture();

            AssetDatabase.Refresh();

            // Configure Sprite Importers
            ConfigureImporter(WallPath, 100);
            ConfigureImporter(FloorPath, 100);
            ConfigureImporter(CounterPath, 100);
            ConfigureImporter(DiningTablePath, 100);
            ConfigureImporter(ExitDoorPath, 100);
            ConfigureImporter(MenuBoardPath, 100);
            ConfigureImporter(IngredientShelfPath, 100);
            ConfigureImporter(DecorationsPath, 100);
            ConfigureImporter(NPCCustomersPath, 100);

            ConfigureImporter(CookingStationBackgroundPath, 100);
            ConfigureImporter(CookingStoveFlamePath, 100);
            ConfigureImporter(CookingPanStatesPath, 100);
            ConfigureImporter(CookingIngredientsTrayPath, 100);
            ConfigureImporter(CookingFinishedDishPath, 100);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HoneyBreakfastTextureGenerator] Hoàn thành tạo toàn bộ Texture Nội Thất & Nấu Ăn!");
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

        #region Interior Textures Generation

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
                            if (isBorder)
                            {
                                c = woodBrown;
                            }
                            else
                            {
                                // Ancient town scenery painting
                                float paintingY = (float)(y - 570) / 180f;
                                c = Color.Lerp(new Color(0.95f, 0.85f, 0.70f, 1f), new Color(0.70f, 0.82f, 0.90f, 1f), paintingY);
                            }
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
            int w = 1920, h = 320;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color tileLight = new Color(0.94f, 0.92f, 0.88f, 1f); // Ivory
            Color tileDark = new Color(0.24f, 0.22f, 0.20f, 1f);  // Black ink
            Color tileGold = new Color(0.85f, 0.68f, 0.35f, 1f);  // Mustard gold
            Color grout = new Color(0.50f, 0.48f, 0.45f, 1f);

            Color[] colors = new Color[w * h];
            int tileSize = 64;

            for (int y = 0; y < h; y++)
            {
                int tileY = y / tileSize;
                int localY = y % tileSize;

                for (int x = 0; x < w; x++)
                {
                    int tileX = x / tileSize;
                    int localX = x % tileSize;

                    if (localX == 0 || localY == 0 || localX == tileSize - 1 || localY == tileSize - 1)
                    {
                        colors[y * w + x] = grout;
                        continue;
                    }

                    bool isEvenTile = (tileX + tileY) % 2 == 0;
                    Color baseCol = isEvenTile ? tileLight : tileDark;

                    float cx = localX - tileSize / 2f;
                    float cy = localY - tileSize / 2f;
                    float r2 = cx * cx + cy * cy;

                    // Indochine 4-petal flower rosette in center
                    if (r2 < (tileSize * 0.38f) * (tileSize * 0.38f))
                    {
                        float angle = Mathf.Atan2(cy, cx);
                        float petal = Mathf.Abs(Mathf.Sin(angle * 2f));
                        if (r2 < (tileSize * 0.32f * petal) * (tileSize * 0.32f * petal) + 16f)
                        {
                            baseCol = tileGold;
                        }
                    }

                    // Perspective shading: darker towards back
                    float perspective = 0.70f + 0.30f * (1.0f - (float)y / h);
                    baseCol = new Color(baseCol.r * perspective, baseCol.g * perspective, baseCol.b * perspective, 1f);

                    colors[y * w + x] = baseCol;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(FloorPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateCounterTexture()
        {
            int w = 560, h = 420;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color woodDark = new Color(0.38f, 0.22f, 0.12f, 1f);
            Color woodMid = new Color(0.52f, 0.32f, 0.18f, 1f);
            Color marbleTop = new Color(0.95f, 0.94f, 0.92f, 1f);
            Color marbleShade = new Color(0.85f, 0.84f, 0.82f, 1f);
            Color potMetal = new Color(0.70f, 0.72f, 0.75f, 1f);
            Color brothGold = new Color(0.92f, 0.65f, 0.22f, 1f);
            Color panBlack = new Color(0.20f, 0.20f, 0.22f, 1f);
            Color eggYellow = new Color(0.98f, 0.78f, 0.15f, 1f);
            Color eggWhite = new Color(0.98f, 0.98f, 0.96f, 1f);
            Color breadCrust = new Color(0.85f, 0.52f, 0.20f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 1. Counter Body (y: 20..260, x: 20..540)
                    if (y >= 20 && y <= 260 && x >= 20 && x <= 540)
                    {
                        bool isBorder = (x <= 30 || x >= 530 || y <= 30 || Mathf.Abs(x - 280) <= 5);
                        colors[y * w + x] = isBorder ? woodDark : woodMid;
                    }

                    // 2. Marble Countertop Slab (y: 260..295, x: 10..550)
                    if (y >= 260 && y <= 295 && x >= 10 && x <= 550)
                    {
                        colors[y * w + x] = (y >= 285) ? marbleTop : marbleShade;
                    }

                    // 3. Pho Broth Pot on Left (x: 70..190, y: 295..385)
                    if (x >= 70 && x <= 190 && y >= 295 && y <= 385)
                    {
                        float dx = (x - 130) / 60f;
                        float dy = (y - 340) / 45f;
                        if (dx * dx + dy * dy <= 1f)
                        {
                            if (dy > 0.4f) colors[y * w + x] = brothGold;
                            else colors[y * w + x] = potMetal;
                        }
                    }

                    // 4. Skillet with Sunny-Side Egg in Middle (x: 230..330, y: 295..345)
                    if (x >= 230 && x <= 330 && y >= 295 && y <= 345)
                    {
                        float dx = (x - 280) / 50f;
                        float dy = (y - 320) / 25f;
                        float r2 = dx * dx + dy * dy;
                        if (r2 <= 1.0f)
                        {
                            if (r2 <= 0.65f)
                            {
                                float rEgg = Mathf.Sqrt(dx * dx + dy * dy);
                                colors[y * w + x] = (rEgg < 0.28f) ? eggYellow : eggWhite;
                            }
                            else
                            {
                                colors[y * w + x] = panBlack;
                            }
                        }
                    }

                    // 5. Baguette Basket on Right (x: 380..490, y: 295..365)
                    if (x >= 380 && x <= 490 && y >= 295 && y <= 365)
                    {
                        float dx = (x - 435) / 55f;
                        float dy = (y - 330) / 35f;
                        if (dx * dx + dy * dy <= 1.0f)
                        {
                            colors[y * w + x] = breadCrust;
                        }
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
            int w = 400, h = 300;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color woodDark = new Color(0.38f, 0.22f, 0.12f, 1f);
            Color woodMid = new Color(0.55f, 0.32f, 0.18f, 1f);
            Color chairWood = new Color(0.48f, 0.28f, 0.15f, 1f);
            Color plateWhite = new Color(0.96f, 0.96f, 0.94f, 1f);
            Color condimentRed = new Color(0.85f, 0.20f, 0.15f, 1f);

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
            Color glass = new Color(0.85f, 0.92f, 0.98f, 0.75f);
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
                            if (y > 290) colors[y * w + x] = glass;
                            else colors[y * w + x] = woodMid;
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
                        if (y >= 190 && y <= 196 && x >= 50 && x <= 270) c = chalkGold;
                        if (y >= 145 && y <= 151 && x >= 40 && x <= 280) c = chalkWhite;
                        if (y >= 105 && y <= 111 && x >= 40 && x <= 280) c = chalkWhite;
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

        private static void GenerateIngredientShelfTexture()
        {
            int w = 480, h = 360;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color shelfWood = new Color(0.45f, 0.26f, 0.14f, 1f);
            Color ironBracket = new Color(0.20f, 0.18f, 0.16f, 1f);
            Color glassJar = new Color(0.85f, 0.92f, 0.96f, 0.85f);
            Color spiceRed = new Color(0.88f, 0.24f, 0.16f, 1f);
            Color spiceBlack = new Color(0.22f, 0.20f, 0.18f, 1f);
            Color eggBrown = new Color(0.88f, 0.65f, 0.42f, 1f);
            Color sauceBottle = new Color(0.78f, 0.32f, 0.12f, 1f);
            Color butterYellow = new Color(0.96f, 0.85f, 0.35f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 3 Horizontal wooden shelves at y = 80, 180, 280
                    bool isShelf1 = (y >= 75 && y <= 95 && x >= 30 && x <= 450);
                    bool isShelf2 = (y >= 175 && y <= 195 && x >= 30 && x <= 450);
                    bool isShelf3 = (y >= 275 && y <= 295 && x >= 30 && x <= 450);

                    if (isShelf1 || isShelf2 || isShelf3)
                    {
                        colors[y * w + x] = shelfWood;
                        continue;
                    }

                    // Side Iron Wall Brackets (x = 50..65 and 415..430)
                    bool isBracketL = (x >= 50 && x <= 62 && y >= 60 && y <= 310);
                    bool isBracketR = (x >= 418 && x <= 430 && y >= 60 && y <= 310);
                    if (isBracketL || isBracketR)
                    {
                        colors[y * w + x] = ironBracket;
                        continue;
                    }

                    // Top shelf: 4 Glass Jars (y: 295..345)
                    if (y >= 295 && y <= 345)
                    {
                        int jarIndex = (x - 80) / 80;
                        int localX = (x - 80) % 80;
                        if (jarIndex >= 0 && jarIndex <= 3 && localX >= 15 && localX <= 65)
                        {
                            Color spice = (jarIndex % 2 == 0) ? spiceRed : spiceBlack;
                            colors[y * w + x] = (y <= 325) ? spice : glassJar;
                        }
                    }

                    // Middle shelf: Bottles & Butter (y: 195..265)
                    if (y >= 195 && y <= 265)
                    {
                        // Sauce bottles on left (x: 90..180)
                        if (x >= 90 && x <= 180)
                        {
                            int bIndex = (x - 90) / 45;
                            int lx = (x - 90) % 45;
                            if (lx >= 10 && lx <= 35) colors[y * w + x] = sauceBottle;
                        }
                        // Butter block on right (x: 280..370, y: 195..235)
                        if (x >= 280 && x <= 370 && y <= 235)
                        {
                            colors[y * w + x] = butterYellow;
                        }
                    }

                    // Bottom shelf: Basket of eggs (x: 140..340, y: 95..155)
                    if (y >= 95 && y <= 155 && x >= 140 && x <= 340)
                    {
                        float dx = (x - 240) / 90f;
                        float dy = (y - 120) / 25f;
                        if (dx * dx + dy * dy <= 1.0f)
                        {
                            colors[y * w + x] = (dy > 0.1f) ? eggBrown : shelfWood;
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(IngredientShelfPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateDecorationsTexture()
        {
            int w = 400, h = 600;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color potCeramic = new Color(0.92f, 0.94f, 0.95f, 1f);
            Color leafGreen = new Color(0.20f, 0.58f, 0.28f, 1f);
            Color leafLight = new Color(0.38f, 0.75f, 0.35f, 1f);
            Color calendarRed = new Color(0.85f, 0.18f, 0.15f, 1f);
            Color goldAccent = new Color(0.96f, 0.82f, 0.25f, 1f);
            Color paperWhite = new Color(0.98f, 0.97f, 0.95f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 1. Vietnamese Lucky Money Tree in Ceramic Pot (Bottom: x: 80..320, y: 20..380)
                    if (y >= 20 && y <= 160 && x >= 120 && x <= 280)
                    {
                        // Ceramic Pot
                        float dx = (x - 200) / 75f;
                        float dy = (y - 90) / 70f;
                        if (dx * dx + dy * dy <= 1.0f)
                        {
                            colors[y * w + x] = (Mathf.Abs(dx) < 0.2f && Mathf.Abs(dy) < 0.2f) ? goldAccent : potCeramic;
                        }
                    }
                    else if (y >= 150 && y <= 380 && x >= 60 && x <= 340)
                    {
                        // Plant Leaves & Bush
                        float dx = (x - 200) / 120f;
                        float dy = (y - 260) / 100f;
                        float r2 = dx * dx + dy * dy;
                        if (r2 <= 1.0f)
                        {
                            float noise = Mathf.Sin(x * 0.15f) * Mathf.Cos(y * 0.15f);
                            colors[y * w + x] = (noise > 0f) ? leafLight : leafGreen;
                        }
                    }

                    // 2. Traditional Lunar Calendar Bloc (Top: x: 100..300, y: 410..580)
                    if (y >= 410 && y <= 580 && x >= 100 && x <= 300)
                    {
                        // Red Header Plaque (y: 510..580)
                        if (y >= 510)
                        {
                            bool isPlaqueBorder = (x <= 110 || x >= 290 || y <= 516 || y >= 574);
                            colors[y * w + x] = isPlaqueBorder ? goldAccent : calendarRed;
                        }
                        // White Paper Pad (y: 415..505, x: 120..280)
                        else if (x >= 120 && x <= 280)
                        {
                            // Bold red date in middle
                            bool isDate = (x >= 180 && x <= 220 && y >= 445 && y <= 475);
                            colors[y * w + x] = isDate ? calendarRed : paperWhite;
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(DecorationsPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateNPCCustomersTexture()
        {
            int w = 560, h = 400;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color skinTone = new Color(0.98f, 0.88f, 0.80f, 1f);
            Color hairBlack = new Color(0.18f, 0.16f, 0.15f, 1f);
            Color aoDaiPink = new Color(0.95f, 0.65f, 0.72f, 1f);
            Color shirtWhite = new Color(0.92f, 0.94f, 0.96f, 1f);
            Color heartPink = new Color(0.95f, 0.25f, 0.45f, 1f);
            Color newspaperGrey = new Color(0.85f, 0.88f, 0.85f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // NPC 1: Chibi Girl eating breakfast on left (x: 40..240, y: 40..340)
                    if (x >= 40 && x <= 240)
                    {
                        // Body / Ao Dai (x: 80..180, y: 40..180)
                        if (y >= 40 && y <= 180 && x >= 80 && x <= 180)
                        {
                            float dx = (x - 130) / 45f;
                            float dy = (y - 110) / 70f;
                            if (dx * dx + dy * dy <= 1.0f) colors[y * w + x] = aoDaiPink;
                        }
                        // Chibi Head (x: 85..175, y: 170..270)
                        if (y >= 170 && y <= 270 && x >= 85 && x <= 175)
                        {
                            float dx = (x - 130) / 42f;
                            float dy = (y - 220) / 45f;
                            if (dx * dx + dy * dy <= 1.0f)
                            {
                                colors[y * w + x] = (dy > 0.2f) ? hairBlack : skinTone;
                            }
                        }
                        // Happy Heart Emotion Bubble above head (x: 105..155, y: 285..335)
                        if (y >= 285 && y <= 335 && x >= 105 && x <= 155)
                        {
                            float dx = (x - 130) / 20f;
                            float dy = (y - 310) / 20f;
                            if (dx * dx + dy * dy <= 1.0f) colors[y * w + x] = heartPink;
                        }
                    }

                    // NPC 2: Chibi Boy reading newspaper on right (x: 320..520, y: 40..340)
                    if (x >= 320 && x <= 520)
                    {
                        // Newspaper (x: 350..470, y: 80..200)
                        if (x >= 350 && x <= 470 && y >= 80 && y <= 200)
                        {
                            colors[y * w + x] = newspaperGrey;
                        }
                        // Body / Shirt (x: 380..460, y: 40..170)
                        else if (x >= 380 && x <= 460 && y >= 40 && y <= 170)
                        {
                            colors[y * w + x] = shirtWhite;
                        }
                        // Head (x: 385..475, y: 180..280)
                        if (y >= 180 && y <= 280 && x >= 385 && x <= 475)
                        {
                            float dx = (x - 430) / 40f;
                            float dy = (y - 230) / 45f;
                            if (dx * dx + dy * dy <= 1.0f)
                            {
                                colors[y * w + x] = (dy > 0.25f) ? hairBlack : skinTone;
                            }
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(NPCCustomersPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        #endregion

        #region Cooking Station Textures Generation

        private static void GenerateCookingStationBackgroundTexture()
        {
            int w = 1920, h = 1080;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            Color tileWhite = new Color(0.96f, 0.97f, 0.98f, 1f);
            Color tileShade = new Color(0.88f, 0.90f, 0.92f, 1f);
            Color groutColor = new Color(0.75f, 0.78f, 0.80f, 1f);
            Color marbleBase = new Color(0.82f, 0.84f, 0.85f, 1f);
            Color marbleVein = new Color(0.92f, 0.93f, 0.94f, 1f);
            Color steelRack = new Color(0.70f, 0.72f, 0.75f, 1f);

            Color[] colors = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Lower part: Marble Kitchen Countertop (y: 0..420)
                    if (y < 420)
                    {
                        float veinNoise = Mathf.Sin(x * 0.02f + y * 0.015f) * Mathf.Cos(x * 0.008f - y * 0.02f);
                        Color marble = (veinNoise > 0.45f) ? marbleVein : marbleBase;
                        // Light rim on top of counter
                        if (y >= 405) marble = Color.Lerp(marble, Color.white, 0.4f);
                        colors[y * w + x] = marble;
                    }
                    // Upper part: Retro Subway Tile Wall (y >= 420)
                    else
                    {
                        int tileH = 48;
                        int tileW = 120;
                        int row = (y - 420) / tileH;
                        int colX = x + ((row % 2 == 0) ? 0 : tileW / 2);
                        int localY = (y - 420) % tileH;
                        int localX = colX % tileW;

                        if (localX <= 2 || localY <= 2)
                        {
                            colors[y * w + x] = groutColor;
                        }
                        else
                        {
                            float tY = (float)localY / tileH;
                            colors[y * w + x] = Color.Lerp(tileShade, tileWhite, tY);
                        }

                        // Hanging utensil rack (y: 840..855)
                        if (y >= 840 && y <= 855 && x >= 200 && x <= 1720)
                        {
                            colors[y * w + x] = steelRack;
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(CookingStationBackgroundPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateCookingStoveFlameTexture()
        {
            int w = 500, h = 380;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color stoveMetal = new Color(0.80f, 0.82f, 0.85f, 1f);
            Color stoveDark = new Color(0.35f, 0.36f, 0.38f, 1f);
            Color grateCastIron = new Color(0.20f, 0.20f, 0.22f, 1f);
            Color flameBlue = new Color(0.18f, 0.65f, 0.98f, 0.95f);
            Color flameCore = new Color(0.98f, 0.95f, 0.70f, 0.98f);
            Color knobRed = new Color(0.88f, 0.20f, 0.18f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Stove Base Body (x: 40..460, y: 30..180)
                    if (y >= 30 && y <= 180 && x >= 40 && x <= 460)
                    {
                        colors[y * w + x] = (y <= 50) ? stoveDark : stoveMetal;

                        // Red Dial Knob in front (x: 230..270, y: 40..80)
                        float kdx = (x - 250) / 20f;
                        float kdy = (y - 60) / 20f;
                        if (kdx * kdx + kdy * kdy <= 1.0f) colors[y * w + x] = knobRed;
                    }

                    // Burner Ring (x: 150..350, y: 180..220)
                    if (y >= 180 && y <= 220 && x >= 150 && x <= 350)
                    {
                        float bdx = (x - 250) / 90f;
                        float bdy = (y - 200) / 20f;
                        if (bdx * bdx + bdy * bdy <= 1.0f) colors[y * w + x] = grateCastIron;
                    }

                    // Gas Flame Jets (x: 160..340, y: 220..310)
                    if (y >= 220 && y <= 310 && x >= 160 && x <= 340)
                    {
                        float fdx = (x - 250) / 80f;
                        float fdy = (y - 255) / 45f;
                        float fr2 = fdx * fdx + fdy * fdy;
                        if (fr2 <= 1.0f)
                        {
                            float flameWave = Mathf.Abs(Mathf.Sin(x * 0.25f));
                            if (fr2 < 0.4f * flameWave) colors[y * w + x] = flameCore;
                            else if (fr2 < 0.85f * flameWave) colors[y * w + x] = flameBlue;
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(CookingStoveFlamePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateCookingPanStatesTexture()
        {
            // 3 frames side-by-side (1200 x 400): Frame 1: Empty hot pan | Frame 2: Melting butter | Frame 3: Sunny egg sizzling
            int w = 1200, h = 400;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color panRim = new Color(0.72f, 0.74f, 0.76f, 1f);
            Color panBase = new Color(0.18f, 0.18f, 0.20f, 1f);
            Color panHandle = new Color(0.45f, 0.28f, 0.16f, 1f);
            Color butterGold = new Color(0.96f, 0.86f, 0.25f, 1f);
            Color eggWhite = new Color(0.98f, 0.98f, 0.95f, 1f);
            Color eggYolk = new Color(0.98f, 0.65f, 0.12f, 1f);
            Color yolkGlow = new Color(1.0f, 0.80f, 0.25f, 1f);
            Color pepperDot = new Color(0.20f, 0.18f, 0.16f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int f = 0; f < 3; f++)
            {
                int offsetX = f * 400;
                int centerX = offsetX + 200;
                int centerY = 200;

                for (int y = 0; y < h; y++)
                {
                    for (int x = offsetX; x < offsetX + 400; x++)
                    {
                        // Handle sticking out right (x: centerX + 110..centerX + 185, y: centerY - 15..centerY + 15)
                        if (x >= centerX + 110 && x <= centerX + 185 && Mathf.Abs(y - centerY) <= 16)
                        {
                            colors[y * w + x] = panHandle;
                            continue;
                        }

                        // Round Skillet
                        float dx = (x - centerX) / 125f;
                        float dy = (y - centerY) / 125f;
                        float r2 = dx * dx + dy * dy;

                        if (r2 <= 1.05f)
                        {
                            if (r2 > 0.88f)
                            {
                                colors[y * w + x] = panRim;
                            }
                            else
                            {
                                Color panInner = panBase;

                                // Frame 1: Empty hot pan (slight thermal glow)
                                if (f == 0)
                                {
                                    if (r2 < 0.6f) panInner = Color.Lerp(panBase, new Color(0.28f, 0.22f, 0.20f, 1f), 0.5f);
                                }
                                // Frame 2: Melting butter sizzle
                                else if (f == 1)
                                {
                                    float bdx = (x - centerX) / 65f;
                                    float bdy = (y - centerY) / 50f;
                                    if (bdx * bdx + bdy * bdy <= 1.0f)
                                    {
                                        panInner = butterGold;
                                    }
                                }
                                // Frame 3: Sunny-side up eggs frying
                                else if (f == 2)
                                {
                                    // Egg White spreading
                                    float wdx = (x - centerX) / 85f;
                                    float wdy = (y - centerY) / 75f;
                                    if (wdx * wdx + wdy * wdy <= 1.0f)
                                    {
                                        panInner = eggWhite;

                                        // Left Yolk (x = centerX - 30, y = centerY + 5)
                                        float y1dx = (x - (centerX - 30)) / 26f;
                                        float y1dy = (y - (centerY + 5)) / 26f;
                                        if (y1dx * y1dx + y1dy * y1dy <= 1.0f)
                                        {
                                            panInner = (y1dx * y1dx + y1dy * y1dy < 0.35f) ? yolkGlow : eggYolk;
                                        }

                                        // Right Yolk (x = centerX + 32, y = centerY - 8)
                                        float y2dx = (x - (centerX + 32)) / 26f;
                                        float y2dy = (y - (centerY - 8)) / 26f;
                                        if (y2dx * y2dx + y2dy * y2dy <= 1.0f)
                                        {
                                            panInner = (y2dx * y2dx + y2dy * y2dy < 0.35f) ? yolkGlow : eggYolk;
                                        }

                                        // Pepper flakes
                                        if ((x * 17 + y * 23) % 47 == 0) panInner = pepperDot;
                                    }
                                }

                                colors[y * w + x] = panInner;
                            }
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(CookingPanStatesPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateCookingIngredientsTrayTexture()
        {
            int w = 600, h = 400;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color trayWood = new Color(0.50f, 0.32f, 0.18f, 1f);
            Color eggShell = new Color(0.92f, 0.70f, 0.48f, 1f);
            Color butterYellow = new Color(0.98f, 0.88f, 0.30f, 1f);
            Color pateBrown = new Color(0.60f, 0.38f, 0.28f, 1f);
            Color scallionGreen = new Color(0.25f, 0.75f, 0.30f, 1f);
            Color chiliRed = new Color(0.90f, 0.18f, 0.15f, 1f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Wooden Tray Outer Rim (x: 20..580, y: 30..370)
                    if (x >= 20 && x <= 580 && y >= 30 && y <= 370)
                    {
                        bool isRim = (x <= 35 || x >= 565 || y <= 45 || y >= 355 || Mathf.Abs(x - 300) <= 6 || Mathf.Abs(y - 200) <= 6);
                        if (isRim)
                        {
                            colors[y * w + x] = trayWood * 0.8f;
                        }
                        else
                        {
                            colors[y * w + x] = trayWood;

                            // Compartment 1 (Top-Left): 3 Farm Eggs
                            if (x < 300 && y > 200)
                            {
                                float edx = (x - 160) / 45f;
                                float edy = (y - 280) / 35f;
                                if (edx * edx + edy * edy <= 1.0f) colors[y * w + x] = eggShell;
                            }
                            // Compartment 2 (Top-Right): Rich Butter Block
                            else if (x > 300 && y > 200)
                            {
                                if (x >= 370 && x <= 500 && y >= 240 && y <= 330) colors[y * w + x] = butterYellow;
                            }
                            // Compartment 3 (Bottom-Left): Fresh Liver Pate
                            else if (x < 300 && y < 200)
                            {
                                float pdx = (x - 160) / 50f;
                                float pdy = (y - 120) / 35f;
                                if (pdx * pdx + pdy * pdy <= 1.0f) colors[y * w + x] = pateBrown;
                            }
                            // Compartment 4 (Bottom-Right): Chopped Scallions & Chili slices
                            else if (x > 300 && y < 200)
                            {
                                float cdx = (x - 440) / 55f;
                                float cdy = (y - 120) / 40f;
                                if (cdx * cdx + cdy * cdy <= 1.0f)
                                {
                                    colors[y * w + x] = ((x + y) % 11 < 3) ? chiliRed : scallionGreen;
                                }
                            }
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(CookingIngredientsTrayPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateCookingFinishedDishTexture()
        {
            int w = 640, h = 500;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color porcelainWhite = new Color(0.96f, 0.97f, 0.98f, 1f);
            Color lotusBlue = new Color(0.20f, 0.45f, 0.75f, 1f);
            Color eggWhite = new Color(0.98f, 0.98f, 0.95f, 1f);
            Color eggYolk = new Color(0.98f, 0.68f, 0.12f, 1f);
            Color crispyCrust = new Color(0.85f, 0.52f, 0.18f, 1f);
            Color herbsGreen = new Color(0.22f, 0.72f, 0.28f, 1f);
            Color chiliRed = new Color(0.95f, 0.18f, 0.12f, 1f);
            Color steamWhite = new Color(1.0f, 1.0f, 1.0f, 0.45f);

            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;

            int cx = 320, cy = 230;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Big Porcelain Serving Plate
                    float pdx = (x - cx) / 260f;
                    float pdy = (y - cy) / 180f;
                    float pr2 = pdx * pdx + pdy * pdy;

                    if (pr2 <= 1.0f)
                    {
                        // Lotus Blue Pattern Rim
                        if (pr2 > 0.88f)
                        {
                            colors[y * w + x] = lotusBlue;
                        }
                        else
                        {
                            Color food = porcelainWhite;

                            // Left Side: 2 Sunny-side Eggs
                            float edx = (x - (cx - 85)) / 110f;
                            float edy = (y - cy) / 95f;
                            if (edx * edx + edy * edy <= 1.0f)
                            {
                                food = eggWhite;

                                // Yolk 1
                                float y1dx = (x - (cx - 120)) / 30f;
                                float y1dy = (y - (cy + 20)) / 30f;
                                if (y1dx * y1dx + y1dy * y1dy <= 1.0f) food = eggYolk;

                                // Yolk 2
                                float y2dx = (x - (cx - 60)) / 30f;
                                float y2dy = (y - (cy - 20)) / 30f;
                                if (y2dx * y2dx + y2dy * y2dy <= 1.0f) food = eggYolk;
                            }

                            // Right Side: Crispy Baguette sliced open
                            float bdx = (x - (cx + 100)) / 85f;
                            float bdy = (y - cy) / 60f;
                            if (bdx * bdx + bdy * bdy <= 1.0f)
                            {
                                food = crispyCrust;
                            }

                            // Fresh Herbs & Chili Garnish
                            if (x >= cx - 20 && x <= cx + 40 && y >= cy - 50 && y <= cy + 50)
                            {
                                if ((x * 7 + y * 13) % 17 < 5) food = herbsGreen;
                                else if ((x * 11 + y * 19) % 29 < 4) food = chiliRed;
                            }

                            colors[y * w + x] = food;
                        }
                    }

                    // Aromatic Steam rising above plate (y: 380..480)
                    if (y >= 380 && pr2 <= 1.6f)
                    {
                        float wave = Mathf.Sin(x * 0.08f + y * 0.05f);
                        if (wave > 0.75f && Mathf.Abs(x - cx) < 160)
                        {
                            colors[y * w + x] = steamWhite;
                        }
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(CookingFinishedDishPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        #endregion
    }
}
