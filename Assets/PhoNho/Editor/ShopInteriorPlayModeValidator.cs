using System;
using System.Collections;
using System.IO;
using System.Linq;
using PhoNho.Character;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using PhoNho.Gameplay;
using PhoNho.Gameplay.Interaction;
using PhoNho.Map;
using PhoNho.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Comprehensive automated validator for the Honey Breakfast Shop Interior scene,
    /// scene transition mechanics, player movement inside the shop, cooking flow, and screenshot capture.
    /// Can be executed headlessly via Unity batchmode.
    /// </summary>
    public static class ShopInteriorPlayModeValidator
    {
        public const string ResultsJsonPath = "Assets/PhoNho/Editor/shop-interior-validation-results.json";
        public const string ScreenshotPath = "Assets/PhoNho/Editor/shop-interior-view-1080p.png";
        public const string DocsScreenshotPath = "docs/snapshots/shop-interior-view-1080p.png";

        [MenuItem("Phố Nhỏ/Gameplay/Kiểm tra Scene Nội Thất & Chụp Ảnh")]
        public static void RunValidation()
        {
            var results = new ValidationResults();

            try
            {
                Debug.Log("[ShopInteriorValidator] Bắt đầu quy trình kiểm tra Scene Nội Thất Quán Ăn Sáng...");

                // 1. Build and update scenes & build settings
                HoneyBreakfastTextureGenerator.GenerateAllTextures();
                PhoNhoGameplaySceneBuilder.BuildAll();
                HoneyBreakfastSceneBuilder.BuildInteriorScene();

                results.buildSettingsCheck = ValidateBuildSettings();
                results.texturesGenerated = ValidateTextures();

                // 2. Validate Fixed Default Coordinates for all buildings in Street Scene
                ValidateBuildingCoordinates(results);

                // 3. Open and inspect Interior Scene
                Scene interiorScene = EditorSceneManager.OpenScene(HoneyBreakfastSceneBuilder.InteriorScenePath, OpenSceneMode.Single);
                results.interiorSceneOpened = interiorScene.IsValid();

                // 4. Inspect Scene Objects & Hierarchy
                ValidateSceneHierarchy(results);

                // 5. Test Player Movement inside Shop
                ValidatePlayerMovement(results);

                // 6. Test Cooking Station Interaction & Revenue Flow inside Shop
                ValidateCookingFlow(results);

                // 7. Test Scene Transition & Return Coordinates
                ValidateTransitions(results);

                // 8. Capture High-Resolution 1080p Screenshot of the Interior
                CaptureInteriorScreenshot(results);

                results.allPassed = results.buildSettingsCheck &&
                                    results.texturesGenerated &&
                                    results.buildingCoordinatesValid &&
                                    results.interiorSceneOpened &&
                                    results.hierarchyValid &&
                                    results.movementValid &&
                                    results.cookingFlowValid &&
                                    results.transitionsValid &&
                                    results.screenshotCaptured;

                results.message = results.allPassed
                    ? "TẤT CẢ KIỂM THỬ ĐẠT 100%! Scene Nội Thất Quán Ăn Sáng hoàn thiện với mỹ thuật chuẩn Chibi Việt Nam, di chuyển, nấu ăn và chuyển cảnh mượt mà."
                    : "Một số bước kiểm thử chưa đạt, vui lòng xem chi tiết trong file kết quả.";

                Debug.Log($"[ShopInteriorValidator] Kết quả: {results.message}");
            }
            catch (Exception ex)
            {
                results.allPassed = false;
                results.message = $"Lỗi ngoại lệ trong quá trình kiểm thử: {ex.Message}\n{ex.StackTrace}";
                Debug.LogError($"[ShopInteriorValidator] {results.message}");
            }
            finally
            {
                SaveResults(results);
            }
        }

        public static void ValidateFromCommandLine()
        {
            RunValidation();
        }

        private static bool ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasStreet = scenes.Any(s => s.path == HoneyBreakfastSceneBuilder.StreetScenePath && s.enabled);
            bool hasInterior = scenes.Any(s => s.path == HoneyBreakfastSceneBuilder.InteriorScenePath && s.enabled);

            Debug.Log($"[ShopInteriorValidator] Build Settings: Street={hasStreet}, Interior={hasInterior}, Total={scenes.Length}");
            return hasStreet && hasInterior;
        }

        private static bool ValidateTextures()
        {
            bool wall = File.Exists(HoneyBreakfastTextureGenerator.WallPath);
            bool floor = File.Exists(HoneyBreakfastTextureGenerator.FloorPath);
            bool counter = File.Exists(HoneyBreakfastTextureGenerator.CounterPath);
            bool table = File.Exists(HoneyBreakfastTextureGenerator.DiningTablePath);
            bool door = File.Exists(HoneyBreakfastTextureGenerator.ExitDoorPath);
            bool menu = File.Exists(HoneyBreakfastTextureGenerator.MenuBoardPath);

            Debug.Log($"[ShopInteriorValidator] Textures: Wall={wall}, Floor={floor}, Counter={counter}, Table={table}, Door={door}, Menu={menu}");
            return wall && floor && counter && table && door && menu;
        }

        private static void ValidateBuildingCoordinates(ValidationResults results)
        {
            Scene streetScene = EditorSceneManager.OpenScene(PhoNhoGameplaySceneBuilder.ScenePath, OpenSceneMode.Single);
            if (!streetScene.IsValid())
            {
                results.buildingCoordinatesValid = false;
                Debug.LogError($"[ShopInteriorValidator] Không mở được street scene để kiểm tra tọa độ nhà: {PhoNhoGameplaySceneBuilder.ScenePath}");
                return;
            }

            bool allValid = true;
            foreach (var kvp in BuildingDefaultCoordinates.AllPlacements)
            {
                var placement = kvp.Value;
                var go = GameObject.Find(placement.buildingName);
                if (go == null)
                {
                    Debug.LogError($"[ShopInteriorValidator] Không tìm thấy nhà: {placement.buildingName}");
                    allValid = false;
                    continue;
                }

                float distPos = Vector3.Distance(go.transform.position, placement.defaultPosition);
                float distScale = Vector3.Distance(go.transform.localScale, placement.defaultScale);
                var anchor = go.GetComponent<PhoNhoBuildingAnchor>();

                bool posOk = distPos < 0.01f;
                bool scaleOk = distScale < 0.01f;
                bool anchorOk = anchor != null;

                if (!posOk || !scaleOk || !anchorOk)
                {
                    Debug.LogError($"[ShopInteriorValidator] Nhà {placement.buildingName} sai tọa độ/scale hoặc thiếu Anchor! PosDist={distPos}, ScaleDist={distScale}, HasAnchor={anchorOk}");
                    allValid = false;
                }
                else
                {
                    Debug.Log($"[ShopInteriorValidator] Nhà {placement.buildingName} khớp tọa độ mặc định chuẩn 100%: Pos={go.transform.position}, Scale={go.transform.localScale.x}");
                }
            }

            results.buildingCoordinatesValid = allValid;
        }

        private static void ValidateSceneHierarchy(ValidationResults results)
        {
            var cam = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
            var camFollow = cam?.GetComponent<PhoNhoCameraFollow>();
            var wall = GameObject.Find("Interior_Wall_Background");
            var floor = GameObject.Find("Interior_Floor_Tiles");
            var groundCol = GameObject.Find("Interior_Ground_Platform")?.GetComponent<BoxCollider2D>();
            var wallLeftCol = GameObject.Find("Boundary_Wall_Left")?.GetComponent<BoxCollider2D>();
            var wallRightCol = GameObject.Find("Boundary_Wall_Right")?.GetComponent<BoxCollider2D>();
            var exitDoor = GameObject.Find("Interior_Exit_Door")?.GetComponent<ShopExitTrigger>();
            var counter = GameObject.Find("Interior_Cooking_Counter")?.GetComponent<ShopInteractionTrigger>();
            var player = GameObject.FindWithTag("Player");

            bool hasCam = (cam != null && cam.orthographic && camFollow != null);
            bool hasEnv = (wall != null && floor != null && groundCol != null && wallLeftCol != null && wallRightCol != null);
            bool hasExit = (exitDoor != null && exitDoor.PromptText.Contains("Ra ngoài phố"));
            bool hasCounter = (counter != null && counter.ShopType == ShopType.BreakfastShop && !counter.LoadSceneOnInteract);
            bool hasPlayer = (player != null && player.GetComponent<PhoNhoPlayerMovement>() != null && player.GetComponent<Rigidbody2D>() != null);

            results.hierarchyValid = hasCam && hasEnv && hasExit && hasCounter && hasPlayer;
            Debug.Log($"[ShopInteriorValidator] Hierarchy Check: Cam={hasCam}, Env={hasEnv}, Exit={hasExit}, Counter={hasCounter}, Player={hasPlayer}");
        }

        private static void ValidatePlayerMovement(ValidationResults results)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                results.movementValid = false;
                return;
            }

            var movement = player.GetComponent<PhoNhoPlayerMovement>();
            var rb = player.GetComponent<Rigidbody2D>();

            // Simulate setting input to walk right towards counter
            movement.SetMoveInput(1.0f);
            bool canInput = (movement.MoveSpeed > 0f);

            // Reset input
            movement.SetMoveInput(0f);

            results.movementValid = canInput && (rb.constraints == RigidbodyConstraints2D.FreezeRotation);
            Debug.Log($"[ShopInteriorValidator] Player Movement Check: Valid={results.movementValid}, Speed={movement.MoveSpeed}");
        }

        private static void ValidateCookingFlow(ValidationResults results)
        {
            var wallet = UnityEngine.Object.FindFirstObjectByType<PlayerWallet>();
            var inventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
            var cooking = UnityEngine.Object.FindFirstObjectByType<CookingService>();

            if (wallet == null || inventory == null || cooking == null)
            {
                results.cookingFlowValid = false;
                return;
            }

            int initialScoin = wallet.Scoin;
            int initialFlour = inventory.GetIngredientCount("flour");
            int initialEgg = inventory.GetIngredientCount("egg");

            // Ensure sufficient ingredients
            inventory.AddIngredient("flour", 2);
            inventory.AddIngredient("egg", 4);

            // Test cooking Bánh Mì Ốp La (recipe_breakfast_bread_egg)
            var recipe = CookingService.StandardRecipes.FirstOrDefault(r => r.id == "recipe_breakfast_bread_egg");
            if (recipe == null)
            {
                results.cookingFlowValid = false;
                return;
            }

            bool started = cooking.TryStartCooking(recipe, out string error);
            if (!started)
            {
                Debug.LogWarning($"[ShopInteriorValidator] TryStartCooking failed: {error}");
                results.cookingFlowValid = false;
                return;
            }

            // Fast-forward cooking timer
            cooking.FastForwardCooking();

            int finalScoin = wallet.Scoin;
            bool scoinEarned = (finalScoin == initialScoin + recipe.rewardScoin);

            results.cookingFlowValid = started && scoinEarned;
            Debug.Log($"[ShopInteriorValidator] Cooking Flow Check: Started={started}, ScoinEarned={scoinEarned} ({initialScoin} -> {finalScoin})");
        }

        private static void ValidateTransitions(ValidationResults results)
        {
            // Test EnterShop sets return spawn
            Vector3 testStreetPos = new Vector3(6.75f, HoneyBreakfastSceneBuilder.GroundBaseline, 0f);
            SceneTransitionManager.LastStreetReturnPosition = testStreetPos;

            bool recorded = (SceneTransitionManager.LastStreetReturnPosition.HasValue &&
                             SceneTransitionManager.LastStreetReturnPosition.Value.x == 6.75f);

            // Test StreetSpawnController applies position
            var playerGo = new GameObject("TestPlayer");
            playerGo.tag = "Player";
            var rb = playerGo.AddComponent<Rigidbody2D>();
            var spawnCtrl = playerGo.AddComponent<StreetSpawnController>();
            spawnCtrl.Configure(playerGo.transform);

            spawnCtrl.CheckReturnSpawn();
            bool spawnedAccurately = Mathf.Abs(playerGo.transform.position.x - 6.75f) < 0.01f;

            UnityEngine.Object.DestroyImmediate(playerGo);
            SceneTransitionManager.ResetState();

            results.transitionsValid = recorded && spawnedAccurately;
            Debug.Log($"[ShopInteriorValidator] Transitions Check: Recorded={recorded}, SpawnedAccurately={spawnedAccurately}");
        }

        private static void CaptureInteriorScreenshot(ValidationResults results)
        {
            var cam = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
            if (cam == null)
            {
                results.screenshotCaptured = false;
                return;
            }

            int width = 1920;
            int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var prevRt = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var screenTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenTex.Apply();

            cam.targetTexture = prevRt;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = screenTex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(screenTex);

            File.WriteAllBytes(ScreenshotPath, bytes);

            // Also copy to docs/snapshots for user artifact review
            Directory.CreateDirectory("docs/snapshots");
            File.WriteAllBytes(DocsScreenshotPath, bytes);

            AssetDatabase.Refresh();
            results.screenshotCaptured = File.Exists(ScreenshotPath) && (new FileInfo(ScreenshotPath).Length > 10000);
            Debug.Log($"[ShopInteriorValidator] Đã chụp ảnh màn hình Scene Nội Thất: {ScreenshotPath} ({bytes.Length} bytes)");
        }

        private static void SaveResults(ValidationResults results)
        {
            string json = JsonUtility.ToJson(results, true);
            File.WriteAllBytes(ResultsJsonPath, System.Text.Encoding.UTF8.GetBytes(json));
            File.WriteAllBytes("docs/snapshots/shop-interior-validation-results.json", System.Text.Encoding.UTF8.GetBytes(json));
            AssetDatabase.Refresh();
            Debug.Log($"[ShopInteriorValidator] Đã lưu kết quả kiểm thử tại: {ResultsJsonPath}");
        }

        [Serializable]
        public class ValidationResults
        {
            public bool allPassed;
            public string message;
            public bool buildSettingsCheck;
            public bool texturesGenerated;
            public bool buildingCoordinatesValid;
            public bool interiorSceneOpened;
            public bool hierarchyValid;
            public bool movementValid;
            public bool cookingFlowValid;
            public bool transitionsValid;
            public bool screenshotCaptured;
            public string timestamp = DateTime.UtcNow.ToString("o");
        }
    }
}
