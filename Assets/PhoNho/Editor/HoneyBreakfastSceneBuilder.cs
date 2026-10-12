using System;
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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Builds the ShopInterior_HoneyBreakfast.unity scene with warm Vietnamese vintage Chibi aesthetic,
    /// complete with cooking counter, exit door transition, camera follow, and player character.
    /// Also registers both street and interior scenes in EditorBuildSettings.
    /// </summary>
    public static class HoneyBreakfastSceneBuilder
    {
        public const string SceneFolder = "Assets/PhoNho/Scenes";
        public const string InteriorScenePath = SceneFolder + "/ShopInterior_HoneyBreakfast.unity";
        public const string StreetScenePath = SceneFolder + "/CityOverworld_PlayerMovement.unity";

        public const float GroundBaseline = -1.80f;

        [MenuItem("Phố Nhỏ/Gameplay/Tạo Scene Nội Thất Quán Ăn Sáng")]
        public static void BuildInteriorScene()
        {
            Directory.CreateDirectory(SceneFolder);

            // Ensure textures exist
            HoneyBreakfastTextureGenerator.GenerateAllTextures();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("PhoNho_HoneyBreakfastInterior");

            // 1. Main Camera
            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.tag = "MainCamera";

            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.98f, 0.94f, 0.88f, 1f); // Warm cozy honey beige
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            cameraGo.transform.position = new Vector3(0f, 0.3f, -10f);
            cameraGo.AddComponent<AudioListener>();

            var cameraFollow = cameraGo.AddComponent<PhoNhoCameraFollow>();
            cameraFollow.SetBounds(-1.8f, 1.8f); // Clamped inside shop room
            cameraFollow.SnapToTarget();

            // 2. Wall Background
            GameObject wallGo = new GameObject("Interior_Wall_Background");
            wallGo.transform.SetParent(root.transform, false);
            wallGo.transform.position = new Vector3(0f, 0.5f, 0f);

            var wallSr = wallGo.AddComponent<SpriteRenderer>();
            wallSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.WallPath);
            wallSr.sortingOrder = -50;
            wallGo.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            // 3. Indochine Tile Floor
            GameObject floorGo = new GameObject("Interior_Floor_Tiles");
            floorGo.transform.SetParent(root.transform, false);
            floorGo.transform.position = new Vector3(0f, GroundBaseline - 0.9f, 0f);

            var floorSr = floorGo.AddComponent<SpriteRenderer>();
            floorSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.FloorPath);
            floorSr.sortingOrder = -20;
            floorGo.transform.localScale = new Vector3(1.6f, 1.0f, 1.0f);

            // 4. Ground Solid Collider (Top surface exactly at GroundBaseline = -1.80f)
            GameObject groundPlatform = new GameObject("Interior_Ground_Platform");
            groundPlatform.transform.SetParent(root.transform, false);
            groundPlatform.transform.position = new Vector3(0f, GroundBaseline, 0f);

            var groundCol = groundPlatform.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(16.0f, 1.0f);
            groundCol.offset = new Vector2(0f, -0.5f);

            // Left & Right Boundary Walls
            GameObject wallLeftColGo = new GameObject("Boundary_Wall_Left");
            wallLeftColGo.transform.SetParent(root.transform, false);
            wallLeftColGo.transform.position = new Vector3(-6.2f, 0f, 0f);
            var wallLeftCol = wallLeftColGo.AddComponent<BoxCollider2D>();
            wallLeftCol.size = new Vector2(1.0f, 12.0f);

            GameObject wallRightColGo = new GameObject("Boundary_Wall_Right");
            wallRightColGo.transform.SetParent(root.transform, false);
            wallRightColGo.transform.position = new Vector3(6.2f, 0f, 0f);
            var wallRightCol = wallRightColGo.AddComponent<BoxCollider2D>();
            wallRightCol.size = new Vector2(1.0f, 12.0f);

            // 5. Exit Door & Street Return Trigger (Left side: X = -4.3f)
            GameObject exitDoorGo = new GameObject("Interior_Exit_Door");
            exitDoorGo.transform.SetParent(root.transform, false);
            // Height 580 px, PPU 100 -> height 5.8 units. Center Y = GroundBaseline + 2.9f
            exitDoorGo.transform.position = new Vector3(-4.3f, GroundBaseline + 2.8f, 0f);

            var doorSr = exitDoorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.ExitDoorPath);
            doorSr.sortingOrder = -10;

            var exitTrigger = exitDoorGo.AddComponent<ShopExitTrigger>();
            exitTrigger.Configure("Nhấn [E] Ra ngoài phố", 2.2f, "CityOverworld_PlayerMovement");

            // 6. Dining Tables
            GameObject table1Go = new GameObject("Dining_Table_1");
            table1Go.transform.SetParent(root.transform, false);
            table1Go.transform.position = new Vector3(-1.3f, GroundBaseline + 1.45f, 0f);
            var table1Sr = table1Go.AddComponent<SpriteRenderer>();
            table1Sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.DiningTablePath);
            table1Sr.sortingOrder = -5;

            GameObject table2Go = new GameObject("Dining_Table_2");
            table2Go.transform.SetParent(root.transform, false);
            table2Go.transform.position = new Vector3(1.3f, GroundBaseline + 1.45f, 0f);
            var table2Sr = table2Go.AddComponent<SpriteRenderer>();
            table2Sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.DiningTablePath);
            table2Sr.sortingOrder = -5;

            // 7. Chalk Menu Board
            GameObject menuGo = new GameObject("Interior_Menu_Board");
            menuGo.transform.SetParent(root.transform, false);
            menuGo.transform.position = new Vector3(4.0f, 1.8f, 0f);
            var menuSr = menuGo.AddComponent<SpriteRenderer>();
            menuSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.MenuBoardPath);
            menuSr.sortingOrder = -15;

            // 8. Cooking Counter & Interaction Trigger (Right side: X = 4.0f)
            GameObject counterGo = new GameObject("Interior_Cooking_Counter");
            counterGo.transform.SetParent(root.transform, false);
            counterGo.transform.position = new Vector3(4.0f, GroundBaseline + 1.8f, 0f);

            var counterSr = counterGo.AddComponent<SpriteRenderer>();
            counterSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.CounterPath);
            counterSr.sortingOrder = -2;

            var counterTrigger = counterGo.AddComponent<ShopInteractionTrigger>();
            counterTrigger.Configure(ShopType.BreakfastShop, "Bếp Quán Ăn Sáng Mật Ong", "Nhấn [E] Nấu & Phục vụ khách", 2.5f);
            counterTrigger.ConfigureSceneTransition(false, ""); // Inside interior: opens cooking dialog, not scene load

            // 9. Instantiate Player Character from Prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PhoNhoGameplaySceneBuilder.PrefabPath);
            if (playerPrefab == null)
            {
                playerPrefab = PhoNhoGameplaySceneBuilder.BuildPlayerPrefab();
            }

            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, root.transform);
            playerInstance.name = "Player_Character";
            // Spawn inside near the entrance door: X = -3.2f, Y = -1.0f
            playerInstance.transform.position = new Vector3(-3.2f, -1.0f, 0f);

            var playerMovement = playerInstance.GetComponent<PhoNhoPlayerMovement>();
            cameraFollow.SetTarget(playerInstance.transform);
            cameraFollow.SnapToTarget();

            // 10. Game Systems & UI
            GameObject gameSystemGo = new GameObject("PhoNho_GameSystem");
            gameSystemGo.transform.SetParent(root.transform, false);

            var wallet = gameSystemGo.AddComponent<PlayerWallet>();
            var inventory = gameSystemGo.AddComponent<PlayerInventory>();
            var cookingService = gameSystemGo.AddComponent<CookingService>();
            var uiManager = gameSystemGo.AddComponent<ShopUIManager>();

            // Stock initial breakfast ingredients so the player can cook right away
            inventory.AddIngredient("flour", 6);
            inventory.AddIngredient("egg", 10);
            inventory.AddIngredient("beef", 6);
            inventory.AddIngredient("sugar", 6);

            cookingService.Configure(wallet, inventory);
            uiManager.Configure(wallet, inventory, cookingService, playerMovement);

            // 11. EventSystem
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.transform.SetParent(root.transform, false);
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, InteriorScenePath);

            // Register in Build Settings
            UpdateBuildSettings();

            Debug.Log($"[HoneyBreakfastSceneBuilder] Đã tạo thành công Scene Nội Thất tại: {InteriorScenePath}");
        }

        public static void UpdateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();

            void AddScene(string path)
            {
                if (!scenes.Any(s => s.path == path))
                {
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            AddScene(StreetScenePath);
            AddScene(InteriorScenePath);

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[HoneyBreakfastSceneBuilder] Đã cập nhật Build Settings: {scenes.Count} scenes.");
        }
    }
}
