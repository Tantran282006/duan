using System;
using System.IO;
using System.Linq;
using PhoNho.Art.Editor;
using PhoNho.Character.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    public static class GroundWalkArtRunner
    {
        public const string GameplayCapturePath = "Assets/PhoNho/Editor/gameplay-view-1080p.png";
        public const string GameplayCaptureRightPath = "Assets/PhoNho/Editor/gameplay-view-right-1080p.png";
        public const string GameplayCaptureLeftPath = "Assets/PhoNho/Editor/gameplay-view-left-1080p.png";
        public const string ShopDialogCapturePath = "Assets/PhoNho/Editor/shop-dialog-view-1080p.png";
        public const string CharacterCapturePath = "Assets/PhoNho/Editor/character-preview-1080p.png";

        [MenuItem("Phố Nhỏ/Gameplay/Chạy toàn bộ sửa Ground và Walk Animation")]
        public static void RunAll()
        {
            Debug.Log("[GroundWalkArtRunner] === BẮT ĐẦU CẬP NHẬT GROUND & WALK ANIMATION ===");

            // 1. Re-slice 4 strips (đảm bảo pivot 0.5, 0.1 và 4 frames chuẩn)
            string[] strips = new[]
            {
                "Assets/PhoNho/Art/Characters/Male_A_Idle_Strip.png",
                "Assets/PhoNho/Art/Characters/Male_A_Movement_Strip.png",
                "Assets/PhoNho/Art/Characters/Female_A_Idle_Strip.png",
                "Assets/PhoNho/Art/Characters/Female_A_Movement_Strip.png"
            };

            foreach (string strip in strips)
            {
                AssetDatabase.ImportAsset(strip, ImportAssetOptions.ForceUpdate);
                CharacterSheetTools.Slice(strip, 4, 1);
                AssetDatabase.ImportAsset(strip, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[GroundWalkArtRunner] Sliced strip: {strip}");
            }

            // 2. Chạy CharacterAnimationBuilder để update Clips & Controllers & Character Preview Scene
            CharacterAnimationBuilder.ExecuteAll();
            Debug.Log("[GroundWalkArtRunner] Đã cập nhật Animation Clips và Controller.");

            // 3. Chạy PhoNhoGameplaySceneBuilder để update Prefab & Gameplay Scene
            PhoNhoGameplaySceneBuilder.BuildAll();
            Debug.Log("[GroundWalkArtRunner] Đã cập nhật Prefab và Gameplay Scene.");

            // 4. Capture Game View 1920x1080 của Gameplay Scene tại Center (X = 0), Far Right (X = +45), Far Left (X = -45)
            CaptureSceneView(PhoNhoGameplaySceneBuilder.ScenePath, GameplayCapturePath, 0f);
            CaptureSceneView(PhoNhoGameplaySceneBuilder.ScenePath, GameplayCaptureRightPath, 45f);
            CaptureSceneView(PhoNhoGameplaySceneBuilder.ScenePath, GameplayCaptureLeftPath, -45f);

            // 5. Capture Game View khi mở giao diện Quán Trà Sữa (Boba Shop)
            CaptureShopDialogView(PhoNhoGameplaySceneBuilder.ScenePath, ShopDialogCapturePath);

            // 6. Capture Game View 1920x1080 của Character Preview Scene
            CaptureSceneView("Assets/PhoNho/Scenes/Character_Preview.unity", CharacterCapturePath);

            // 7. Chạy các validator tự động
            CharacterPlayModeValidator.ValidateAll();
            PlayerMovementValidator.ValidateAll();
            ShopInteractionCookingValidator.ValidateAll();

            Debug.Log("[GroundWalkArtRunner] === HOÀN TẤT THÀNH CÔNG 100%! ===");
        }

        public static void RunAllFromCommandLine()
        {
            RunAll();
        }

        private static void CaptureSceneView(string scenePath, string outputPath, float overrideX = float.NaN)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[GroundWalkArtRunner] Không mở được scene: {scenePath}");
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            if (cam == null)
            {
                Debug.LogError($"[GroundWalkArtRunner] Không tìm thấy Camera trong scene: {scenePath}");
                return;
            }

            if (!float.IsNaN(overrideX))
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Vector3 pPos = player.transform.position;
                    pPos.x = overrideX;
                    player.transform.position = pPos;
                }

                Vector3 cPos = cam.transform.position;
                cPos.x = overrideX;
                cam.transform.position = cPos;

                var groundCol = UnityEngine.Object.FindFirstObjectByType<PhoNho.Map.PhoNhoInfiniteGroundCollider>();
                if (groundCol != null)
                {
                    groundCol.ForceRecenter(overrideX);
                }

                var layers = UnityEngine.Object.FindObjectsByType<PhoNho.Map.PhoNhoInfiniteLayer>(FindObjectsSortMode.None);
                foreach (var layer in layers)
                {
                    layer.UpdateTilesPositions(overrideX);
                }
            }

            int width = 1920;
            int height = 1080;
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture prev = cam.targetTexture;
            RenderTexture activePrev = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            cam.targetTexture = prev;
            RenderTexture.active = activePrev;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(screenShot);

            File.WriteAllBytes(outputPath, bytes);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[GroundWalkArtRunner] Đã xuất ảnh Game view 1920x1080: {outputPath}");
        }

        private static void CaptureShopDialogView(string scenePath, string outputPath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            Camera cam = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam == null) return;

            var uiManager = UnityEngine.Object.FindFirstObjectByType<PhoNho.UI.ShopUIManager>();
            if (uiManager != null)
            {
                uiManager.EnsureUIBuilt();
                uiManager.OpenShopDialog(PhoNho.Gameplay.Interaction.ShopType.BobaShop, "Tiệm Trà Sữa Hồng Đáng Yêu");
            }

            int width = 1920;
            int height = 1080;
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture prev = cam.targetTexture;
            RenderTexture activePrev = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            cam.targetTexture = prev;
            RenderTexture.active = activePrev;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(screenShot);

            File.WriteAllBytes(outputPath, bytes);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[GroundWalkArtRunner] Đã xuất ảnh Shop Dialog view 1920x1080: {outputPath}");

            if (uiManager != null)
            {
                uiManager.CloseShopDialog();
            }
        }
    }
}
