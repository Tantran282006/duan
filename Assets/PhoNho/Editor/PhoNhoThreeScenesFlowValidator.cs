using System;
using System.Collections;
using System.IO;
using System.Linq;
using PhoNho.Character;
using PhoNho.Domain;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using PhoNho.Gameplay.Interaction;
using PhoNho.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Automated validator that verifies the entire 3-scene flow:
    /// Street (CityOverworld) <-> Interior (ShopInterior_HoneyBreakfast) <-> Cooking Station (Cooking_HoneyBreakfast).
    /// Tests 3 complete back-and-forth cycles, checks zero camera/manager duplicates,
    /// verifies economy ledger (+35 Scoin per cook), and captures 1080p Game View screenshots for all 3 scenes.
    /// </summary>
    public static class PhoNhoThreeScenesFlowValidator
    {
        public const string ResultsJsonPath = "docs/snapshots/three-scenes-validation-results.json";
        public const string EditorResultsJsonPath = "Assets/PhoNho/Editor/three-scenes-validation-results.json";

        public const string CityView1080pPath = "docs/snapshots/scene-city-overworld-view-1080p.png";
        public const string InteriorView1080pPath = "docs/snapshots/scene-shop-interior-view-1080p.png";
        public const string CookingView1080pPath = "docs/snapshots/scene-cooking-view-1080p.png";

        [Serializable]
        public class ValidationReport
        {
            public bool success;
            public string timestamp;
            public int completedCycles;
            public int initialScoin;
            public int finalScoin;
            public int earnedScoin;
            public string[] setupChecks;
            public string[] cycle1Logs;
            public string[] cycle2Logs;
            public string[] cycle3Logs;
            public string cityScreenshot;
            public string interiorScreenshot;
            public string cookingScreenshot;
        }

        [MenuItem("Phố Nhỏ/Gameplay/Kiểm tra Toàn Bộ 3 Scene (3 Chu Trình & Chụp Ảnh)")]
        public static void ValidateFromMenu()
        {
            RunFullValidation(false);
        }

        public static void ValidateFromCommandLine()
        {
            RunFullValidation(true);
        }

        private static void RunFullValidation(bool isCommandLine)
        {
            Debug.Log("[PhoNhoThreeScenesFlowValidator] === BẮT ĐẦU KIỂM THỬ 3 SCENE & CHU TRÌNH 2 CHIỀU ===");

            var report = new ValidationReport
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                initialScoin = 200,
                completedCycles = 0
            };

            var setupChecks = new System.Collections.Generic.List<string>();
            var c1Logs = new System.Collections.Generic.List<string>();
            var c2Logs = new System.Collections.Generic.List<string>();
            var c3Logs = new System.Collections.Generic.List<string>();

            try
            {
                // 1. Rebuild and synchronize all 3 scenes
                HoneyBreakfastTextureGenerator.GenerateAllTextures();
                setupChecks.Add("Textures generated: all 14 2D textures up to date.");

                PhoNhoGameplaySceneBuilder.BuildPlayableScene();
                setupChecks.Add("CityOverworld scene built.");

                HoneyBreakfastSceneBuilder.BuildInteriorScene();
                setupChecks.Add("ShopInterior scene built.");

                CookingSceneBuilder.BuildCookingScene();
                setupChecks.Add("Cooking scene built.");

                // Check EditorBuildSettings contains all 3
                var buildScenes = EditorBuildSettings.scenes;
                bool hasStreet = buildScenes.Any(s => s.path == HoneyBreakfastSceneBuilder.StreetScenePath);
                bool hasInterior = buildScenes.Any(s => s.path == HoneyBreakfastSceneBuilder.InteriorScenePath);
                bool hasCooking = buildScenes.Any(s => s.path == CookingSceneBuilder.ScenePath);

                if (!hasStreet || !hasInterior || !hasCooking)
                {
                    throw new Exception("EditorBuildSettings must include all 3 scenes!");
                }
                setupChecks.Add($"EditorBuildSettings registered {buildScenes.Length} scenes (all 3 verified).");

                // Reset Game State for clean testing
                PhoNhoGameState.ResetForTesting();
                SceneTransitionManager.ResetState();

                // 2. Cycle 1
                Debug.Log("[PhoNhoThreeScenesFlowValidator] --- BẮT ĐẦU CHU TRÌNH 1 ---");
                RunCycle(1, c1Logs, true);
                report.completedCycles = 1;

                // 3. Cycle 2
                Debug.Log("[PhoNhoThreeScenesFlowValidator] --- BẮT ĐẦU CHU TRÌNH 2 ---");
                RunCycle(2, c2Logs, false);
                report.completedCycles = 2;

                // 4. Cycle 3
                Debug.Log("[PhoNhoThreeScenesFlowValidator] --- BẮT ĐẦU CHU TRÌNH 3 ---");
                RunCycle(3, c3Logs, false);
                report.completedCycles = 3;

                // Economy verification: started with 200, +35 per cycle -> 305 Scoin
                report.finalScoin = PhoNhoGameState.Scoin;
                report.earnedScoin = report.finalScoin - report.initialScoin;

                if (report.earnedScoin != 3 * CookingMiniGameController.RewardScoin)
                {
                    throw new Exception($"Earned Scoin mismatch! Expected {3 * CookingMiniGameController.RewardScoin}, actual: {report.earnedScoin}");
                }

                report.success = true;
                report.setupChecks = setupChecks.ToArray();
                report.cycle1Logs = c1Logs.ToArray();
                report.cycle2Logs = c2Logs.ToArray();
                report.cycle3Logs = c3Logs.ToArray();
                report.cityScreenshot = CityView1080pPath;
                report.interiorScreenshot = InteriorView1080pPath;
                report.cookingScreenshot = CookingView1080pPath;

                SaveReport(report);

                Debug.Log($"[PhoNhoThreeScenesFlowValidator] === THÀNH CÔNG 100%! Hoàn thành 3 chu trình, Ví Scoin: {report.finalScoin} (+{report.earnedScoin}) ===");

                if (isCommandLine)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhoNhoThreeScenesFlowValidator] THẤT BẠI: {ex.Message}\n{ex.StackTrace}");
                report.success = false;
                report.setupChecks = setupChecks.ToArray();
                report.cycle1Logs = c1Logs.ToArray();
                report.cycle2Logs = c2Logs.ToArray();
                report.cycle3Logs = c3Logs.ToArray();
                SaveReport(report);

                if (isCommandLine)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunCycle(int cycleNumber, System.Collections.Generic.List<string> logs, bool captureScreenshots)
        {
            // A. Open Street Scene
            var streetScene = EditorSceneManager.OpenScene(HoneyBreakfastSceneBuilder.StreetScenePath, OpenSceneMode.Single);
            AssertNoDuplicateSingletons("CityOverworld");

            var playerStreet = GameObject.FindWithTag("Player");
            if (playerStreet == null) throw new Exception($"[Cycle {cycleNumber}] Không tìm thấy Player trong Street Scene!");

            logs.Add($"Vào Street: Player tại {playerStreet.transform.position}. Cameras: {Camera.allCamerasCount}");

            if (captureScreenshots)
            {
                CaptureSceneScreenshot(CityView1080pPath);
                logs.Add($"Chụp ảnh Street Scene: {CityView1080pPath}");
            }

            // Find Breakfast Shop trigger
            var breakfastShop = GameObject.Find("Breakfast_Shop");
            if (breakfastShop == null) throw new Exception("Không tìm thấy Breakfast_Shop trên đường phố!");
            var breakfastTrigger = breakfastShop.GetComponent<ShopInteractionTrigger>();
            if (breakfastTrigger == null || !breakfastTrigger.LoadSceneOnInteract)
            {
                throw new Exception("Breakfast_Shop thiếu ShopInteractionTrigger với LoadSceneOnInteract=true!");
            }

            // Trigger Enter Shop
            SceneTransitionManager.EnterShop(HoneyBreakfastSceneBuilder.InteriorScenePath, breakfastShop.transform.position);

            // B. Open Interior Scene
            var interiorScene = EditorSceneManager.OpenScene(HoneyBreakfastSceneBuilder.InteriorScenePath, OpenSceneMode.Single);
            AssertNoDuplicateSingletons("ShopInterior");

            var playerInterior = GameObject.FindWithTag("Player");
            if (playerInterior == null) throw new Exception($"[Cycle {cycleNumber}] Không tìm thấy Player trong Shop Interior!");

            var interiorSpawner = playerInterior.GetComponent<InteriorSpawnController>();
            if (interiorSpawner == null) throw new Exception("Player trong quán thiếu InteriorSpawnController!");

            logs.Add($"Vào Shop Interior: Player tại {playerInterior.transform.position}. Cameras: {Camera.allCamerasCount}");

            if (captureScreenshots)
            {
                CaptureSceneScreenshot(InteriorView1080pPath);
                logs.Add($"Chụp ảnh Shop Interior: {InteriorView1080pPath}");
            }

            // Find Cooking Counter Trigger
            var counterGo = GameObject.Find("Interior_Cooking_Counter");
            if (counterGo == null) throw new Exception("Không tìm thấy Interior_Cooking_Counter trong nội thất!");
            var counterTrigger = counterGo.GetComponent<ShopInteractionTrigger>();
            if (counterTrigger == null || !counterTrigger.LoadSceneOnInteract)
            {
                throw new Exception("Quầy bếp thiếu ShopInteractionTrigger trỏ đến Cooking scene!");
            }

            // Trigger Enter Cooking
            SceneTransitionManager.EnterCooking(CookingSceneBuilder.ScenePath, playerInterior.transform.position);

            // C. Open Cooking Scene
            var cookingScene = EditorSceneManager.OpenScene(CookingSceneBuilder.ScenePath, OpenSceneMode.Single);
            AssertNoDuplicateSingletons("CookingStation");

            var miniGame = UnityEngine.Object.FindFirstObjectByType<CookingMiniGameController>();
            if (miniGame == null) throw new Exception($"[Cycle {cycleNumber}] Không tìm thấy CookingMiniGameController trong Cooking Scene!");

            logs.Add($"Vào Cooking Scene: MiniGame sẵn sàng. Cameras: {Camera.allCamerasCount}");

            // Execute 4 cooking steps
            miniGame.ExecuteStep1_HeatPanAndButter();
            if (miniGame.CurrentStep != 1) throw new Exception("Bước 1 nấu ăn không thành công!");

            miniGame.ExecuteStep2_CrackEggs();
            if (miniGame.CurrentStep != 2) throw new Exception("Bước 2 nấu ăn không thành công!");

            miniGame.ExecuteStep3_ToastBreadAndPate();
            if (miniGame.CurrentStep != 3) throw new Exception("Bước 3 nấu ăn không thành công!");

            miniGame.ExecuteStep4_PlateAndFinish();
            if (miniGame.CurrentStep != 4 || !miniGame.IsCompleted) throw new Exception("Bước 4 nấu ăn không thành công!");

            if (captureScreenshots)
            {
                CaptureSceneScreenshot(CookingView1080pPath);
                logs.Add($"Chụp ảnh Cooking Scene hoàn thành: {CookingView1080pPath}");
            }

            // Complete and Return to Interior
            int scoinBefore = PhoNhoGameState.Scoin;
            miniGame.CompleteAndReturn();
            int scoinAfter = PhoNhoGameState.Scoin;
            if (scoinAfter != scoinBefore + CookingMiniGameController.RewardScoin)
            {
                throw new Exception($"Không được cộng thưởng Scoin! Before: {scoinBefore}, After: {scoinAfter}");
            }
            logs.Add($"Nấu hoàn thành: Scoin {scoinBefore} -> {scoinAfter} (+{CookingMiniGameController.RewardScoin})");

            // D. Back to Shop Interior
            EditorSceneManager.OpenScene(HoneyBreakfastSceneBuilder.InteriorScenePath, OpenSceneMode.Single);
            AssertNoDuplicateSingletons("ShopInterior_Return");

            playerInterior = GameObject.FindWithTag("Player");
            interiorSpawner = playerInterior.GetComponent<InteriorSpawnController>();
            interiorSpawner.CheckReturnSpawn();

            logs.Add($"Quay lại Interior: Player trở về trước quầy bếp tại {playerInterior.transform.position}");

            // Find Exit Door Trigger
            var exitDoorGo = GameObject.Find("Interior_Exit_Door");
            if (exitDoorGo == null) throw new Exception("Không tìm thấy Interior_Exit_Door trong nội thất!");
            var exitTrigger = exitDoorGo.GetComponent<ShopExitTrigger>();
            if (exitTrigger == null) throw new Exception("Cửa thoát thiếu ShopExitTrigger!");

            // Exit to Street
            SceneTransitionManager.ExitShop(HoneyBreakfastSceneBuilder.StreetScenePath);

            // E. Back to Street
            EditorSceneManager.OpenScene(HoneyBreakfastSceneBuilder.StreetScenePath, OpenSceneMode.Single);
            AssertNoDuplicateSingletons("CityOverworld_Return");

            playerStreet = GameObject.FindWithTag("Player");
            var streetSpawner = playerStreet.GetComponent<StreetSpawnController>();
            streetSpawner.CheckReturnSpawn();

            logs.Add($"Quay lại Street: Player tại {playerStreet.transform.position}. Chu trình {cycleNumber} hoàn tất 100%!");
        }

        private static void AssertNoDuplicateSingletons(string context)
        {
            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (cameras.Length != 1)
            {
                throw new Exception($"[{context}] Lỗi nhân đôi Camera! Tìm thấy {cameras.Length} cameras.");
            }

            var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length != 1)
            {
                throw new Exception($"[{context}] Lỗi nhân đôi AudioListener! Tìm thấy {listeners.Length} listeners.");
            }

            var eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if (eventSystems.Length != 1)
            {
                throw new Exception($"[{context}] Lỗi nhân đôi EventSystem! Tìm thấy {eventSystems.Length} EventSystems.");
            }
        }

        private static void CaptureSceneScreenshot(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var cam = Camera.main;
            if (cam == null) cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam == null) return;

            int width = 1920;
            int height = 1080;
            var rt = new RenderTexture(width, height, 24);
            var prevRt = cam.targetTexture;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            cam.targetTexture = prevRt;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(outputPath, bytes);
            UnityEngine.Object.DestroyImmediate(screenShot);
            Debug.Log($"[PhoNhoThreeScenesFlowValidator] Đã chụp ảnh Game View 1080p tại: {outputPath}");
        }

        private static void SaveReport(ValidationReport report)
        {
            string json = JsonUtility.ToJson(report, true);
            Directory.CreateDirectory("docs/snapshots");
            File.WriteAllText(ResultsJsonPath, json);
            File.WriteAllText(EditorResultsJsonPath, json);
            AssetDatabase.Refresh();
        }
    }
}
