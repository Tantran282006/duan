using System;
using System.IO;
using System.Linq;
using PhoNho.Character;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using PhoNho.Gameplay.Interaction;
using PhoNho.Map;
using PhoNho.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Builds the 2D Player Character prefab and the playable street scene with 2D physics.
    /// </summary>
    public static class PhoNhoGameplaySceneBuilder
    {
        public const string PrefabFolder = "Assets/PhoNho/Prefabs";
        public const string PrefabPath = PrefabFolder + "/Player_Character.prefab";
        public const string SceneFolder = "Assets/PhoNho/Scenes";
        public const string ScenePath = SceneFolder + "/CityOverworld_PlayerMovement.unity";

        private const string BackgroundFolder = "Assets/PhoNho/Art/Backgrounds/";
        public const float GroundBaseline = -1.80f;

        [MenuItem("Phố Nhỏ/Gameplay/Tạo scene Gameplay Player Movement")]
        public static void BuildAll()
        {
            BuildPlayerPrefab();
            BuildPlayableScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PhoNhoGameplaySceneBuilder] Tạo Prefab và Scene Gameplay thành công!");
        }

        public static void BuildAllFromCommandLine()
        {
            BuildAll();
        }

        public static GameObject BuildPlayerPrefab()
        {
            Directory.CreateDirectory(PrefabFolder);

            // Load Sprite and Animator Controller
            string stripPath = "Assets/PhoNho/Art/Characters/Male_A_Idle_Strip.png";
            Sprite initialSprite = AssetDatabase.LoadAllAssetsAtPath(stripPath)
                .OfType<Sprite>()
                .OrderBy(s => s.rect.x)
                .FirstOrDefault();

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/PhoNho/Art/Animations/Male_A_Controller.controller");

            // Create temporary instance
            GameObject playerGo = new GameObject("Player_Character");
            playerGo.tag = "Player";

            // SpriteRenderer
            var sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = initialSprite;
            sr.sortingOrder = 25;

            // Animator
            var anim = playerGo.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            // Rigidbody2D
            var rb = playerGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 1f;
            rb.gravityScale = 2.0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // CapsuleCollider2D
            // Chibi sprite: 512x640 at 256 PPU. Pivot is (0.5, 0.1) -> feet at Y=0.
            // Size (0.7, 1.8), offset Y=0.90 -> bottom bounds exactly at Y=0, aligning feet with ground surface!
            var col = playerGo.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.7f, 1.8f);
            col.offset = new Vector2(0f, 0.90f);

            // Movement Controller (synchronized with 8 FPS walk stride ~1.5 units to prevent foot sliding)
            var movement = playerGo.AddComponent<PhoNhoPlayerMovement>();
            movement.MoveSpeed = 3.0f;

            // Save as Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(playerGo, PrefabPath);
            UnityEngine.Object.DestroyImmediate(playerGo);

            Debug.Log($"[PhoNhoGameplaySceneBuilder] Đã lưu Player Prefab tại: {PrefabPath}");
            return prefab;
        }

        public static Scene BuildPlayableScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // Proceed anyway for headless/batchmode
            }

            Directory.CreateDirectory(SceneFolder);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new GameObject("PhoNho_CityGameplay");

            // 1. Camera
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.95f, 0.94f, 0.91f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();

            var cameraFollow = cameraObject.AddComponent<PhoNhoCameraFollow>();

            // 2. Parallax & Environment Layers
            Transform skyLayer = CreateParallaxLayer("01_Sky", root.transform, camera.transform, Vector2.one);
            Transform distantLayer = CreateParallaxLayer("02_DistantTown", root.transform, camera.transform, new Vector2(0.78f, 0.15f));
            Transform middleLayer = CreateLayer("03_Buildings", root.transform);
            Transform groundLayer = CreateLayer("04_Ground", root.transform);
            Transform propLayer = CreateLayer("05_StreetProps", root.transform);
            Transform foregroundLayer = CreateParallaxLayer("06_Foreground", root.transform, camera.transform, new Vector2(-0.12f, 0f));

            // Infinite Background: Sky & Distant Town
            CreateInfiniteSky(skyLayer, camera.transform);
            CreateInfiniteDistantTown(distantLayer, camera.transform);

            // Buildings: Giữ cố định tại khu phố hiện tại, TUYỆT ĐỐI KHÔNG recycle/nhân bản nhà
            CreateGroundedBuilding("Shade_Tree", "Cây nhiệt đới rợp bóng-1.png", middleLayer, 0.1f, GroundBaseline, 5.6f, 32f / 1297f, -10, new Color(0.92f, 0.96f, 0.9f, 1f));

            var ingShopSr = CreateGroundedBuilding("Ingredient_Shop", "Cửa hàng nguyên liệu Việt-5.png", middleLayer, -7.05f, GroundBaseline, 4.25f, 54f / 1199f, 0, Color.white);
            var ingTrigger = ingShopSr.gameObject.AddComponent<ShopInteractionTrigger>();
            ingTrigger.Configure(ShopType.IngredientShop, "Chợ Nguyên Liệu Việt", "Nhấn [E] Mua sỉ nguyên liệu", 2.5f);

            var bobaShopSr = CreateGroundedBuilding("Boba_Shop", "Cửa hàng trà sữa hồng đáng yêu-3.png", middleLayer, -2.4f, GroundBaseline, 4.25f, 44f / 1199f, 0, Color.white);
            var bobaTrigger = bobaShopSr.gameObject.AddComponent<ShopInteractionTrigger>();
            bobaTrigger.Configure(ShopType.BobaShop, "Tiệm Trà Sữa Hồng Đáng Yêu", "Nhấn [E] Pha chế trà sữa", 2.5f);

            var houseSr = CreateGroundedBuilding("Player_House", "Nhà phố hai tầng hoa giấy ấm áp-5.png", middleLayer, 2.15f, GroundBaseline, 5.45f, 116f / 1536f, 1, Color.white);
            var houseTrigger = houseSr.gameObject.AddComponent<ShopInteractionTrigger>();
            houseTrigger.Configure(ShopType.PlayerHouse, "Nhà Phố Của Bạn", "Nhấn [E] Vào nhà xem tủ đồ", 2.8f);

            var breakfastSr = CreateGroundedBuilding("Breakfast_Shop", "Quán ăn sáng Việt màu mật ong-4.png", middleLayer, 6.75f, GroundBaseline, 4.25f, 49f / 1199f, 0, Color.white);
            var breakfastTrigger = breakfastSr.gameObject.AddComponent<ShopInteractionTrigger>();
            breakfastTrigger.Configure(ShopType.BreakfastShop, "Quán Ăn Sáng Mật Ong", "Nhấn [E] Nấu đồ ăn sáng", 2.5f);

            // Infinite Road & Ground: Sidewalk & Road cuộn vô tận
            CreateInfiniteSidewalk(groundLayer, camera.transform);
            CreateInfiniteRoad(groundLayer, camera.transform);

            // Props & Foregrounds: Giữ cố định tại khu phố hiện tại, TUYỆT ĐỐI KHÔNG recycle/nhân bản prop
            CreateGroundedBuilding("Street_Lamp", "Đèn đường cổ điển màu xanh sage-4.png", propLayer, -4.75f, GroundBaseline, 2.8f, 38f / 1536f, 20, Color.white);
            CreateGroundedBuilding("Bench_Left", "Ghế băng gỗ nền trong suốt-3.png", propLayer, 0.05f, GroundBaseline, 1.05f, 93f / 1024f, 21, Color.white);
            CreateGroundedBuilding("Bench_Right", "Ghế băng gỗ nền trong suốt-3.png", propLayer, 4.45f, GroundBaseline, 1.05f, 93f / 1024f, 21, Color.white);

            SpriteRenderer leftBush = CreateWidthSprite("Foreground_Bush_Left", "Cụm bụi lá xanh hoa phấn-6.png", foregroundLayer, new Vector2(-7.6f, -4.75f), 5.2f, 100, new Color(0.86f, 0.92f, 0.8f, 1f));
            SpriteRenderer rightBush = CreateWidthSprite("Foreground_Bush_Right", "Cụm bụi lá xanh hoa phấn-6.png", foregroundLayer, new Vector2(7.6f, -4.75f), 5.2f, 100, new Color(0.86f, 0.92f, 0.8f, 1f));
            rightBush.flipX = true;

            // 3. Ground Platform with Infinite Collider2D (Top surface exactly at GroundBaseline = -1.80f)
            GameObject groundPlatform = new GameObject("Ground_Platform");
            groundPlatform.transform.SetParent(groundLayer, false);
            groundPlatform.transform.position = new Vector3(0f, GroundBaseline, 0f);

            var groundCol = groundPlatform.AddComponent<BoxCollider2D>();
            // Collider surface top aligns with GroundBaseline (-1.80f), wide coverage with dynamic follower
            groundCol.size = new Vector2(100f, 1.0f);
            groundCol.offset = new Vector2(0f, -0.5f);

            var infiniteCol = groundPlatform.AddComponent<PhoNhoInfiniteGroundCollider>();
            infiniteCol.RecenterThreshold = 20f;

            // 4. Instantiate Player Character from Prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (playerPrefab == null)
            {
                playerPrefab = BuildPlayerPrefab();
            }

            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, root.transform);
            playerInstance.name = "Player_Character";
            // Spawn position just slightly above ground baseline: Y = -1.0f
            playerInstance.transform.position = new Vector3(0f, -1.0f, 0f);

            // Set infinite ground collider target to player
            infiniteCol.SetTarget(playerInstance.transform);

            // Hook up Camera Follow with infinite tracking (no boundary clamping)
            cameraFollow.SetTarget(playerInstance.transform);
            cameraFollow.DisableClamping();

            // 5. Game Systems & UI
            GameObject gameSystemGo = new GameObject("PhoNho_GameSystem");
            gameSystemGo.transform.SetParent(root.transform, false);

            var wallet = gameSystemGo.AddComponent<PlayerWallet>();
            var inventory = gameSystemGo.AddComponent<PlayerInventory>();
            var cookingService = gameSystemGo.AddComponent<CookingService>();
            var uiManager = gameSystemGo.AddComponent<ShopUIManager>();

            var playerMovement = playerInstance.GetComponent<PhoNhoPlayerMovement>();
            cookingService.Configure(wallet, inventory);
            uiManager.Configure(wallet, inventory, cookingService, playerMovement);

            // 6. EventSystem for UI interactions
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.transform.SetParent(root.transform, false);
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log($"[PhoNhoGameplaySceneBuilder] Đã lưu Scene Gameplay tại: {ScenePath}");
            return scene;
        }

        private static Transform CreateLayer(string name, Transform parent)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(parent, false);
            return layer.transform;
        }

        private static Transform CreateParallaxLayer(
            string name,
            Transform parent,
            Transform cameraTransform,
            Vector2 movementMultiplier)
        {
            Transform layer = CreateLayer(name, parent);
            ParallaxLayer parallax = layer.gameObject.AddComponent<ParallaxLayer>();
            parallax.Configure(cameraTransform, movementMultiplier);
            return layer;
        }

        private static Sprite LoadSprite(string fileName)
        {
            string path = BackgroundFolder + fileName;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[PhoNhoGameplaySceneBuilder] Không tìm thấy sprite: {path}");
            }
            return sprite;
        }

        private static SpriteRenderer CreateRenderer(
            string objectName,
            Sprite sprite,
            Transform parent,
            int sortingOrder,
            Color tint)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(parent, false);

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = tint;

            return renderer;
        }

        private static SpriteRenderer CreateStretchedSprite(
            string objectName,
            string fileName,
            Transform parent,
            Vector2 position,
            Vector2 targetSize,
            int sortingOrder,
            Color tint)
        {
            Sprite sprite = LoadSprite(fileName);
            if (sprite == null) return null;

            SpriteRenderer renderer = CreateRenderer(objectName, sprite, parent, sortingOrder, tint);
            renderer.transform.position = new Vector3(position.x, position.y, 0f);
            renderer.transform.localScale = new Vector3(
                targetSize.x / sprite.bounds.size.x,
                targetSize.y / sprite.bounds.size.y,
                1f);
            return renderer;
        }

        private static SpriteRenderer CreateWidthSprite(
            string objectName,
            string fileName,
            Transform parent,
            Vector2 position,
            float targetWidth,
            int sortingOrder,
            Color tint)
        {
            Sprite sprite = LoadSprite(fileName);
            if (sprite == null) return null;

            SpriteRenderer renderer = CreateRenderer(objectName, sprite, parent, sortingOrder, tint);
            float scale = targetWidth / sprite.bounds.size.x;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.position = new Vector3(position.x, position.y, 0f);
            return renderer;
        }

        private static SpriteRenderer CreateBottomAlignedSprite(
            string objectName,
            string fileName,
            Transform parent,
            float x,
            float bottom,
            float targetHeight,
            int sortingOrder,
            Color tint)
        {
            Sprite sprite = LoadSprite(fileName);
            if (sprite == null) return null;

            SpriteRenderer renderer = CreateRenderer(objectName, sprite, parent, sortingOrder, tint);
            float scale = targetHeight / sprite.bounds.size.y;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.position = new Vector3(x, bottom - sprite.bounds.min.y * scale, 0f);
            return renderer;
        }

        private static SpriteRenderer CreateGroundedBuilding(
            string objectName,
            string fileName,
            Transform parent,
            float x,
            float groundBaseline,
            float targetHeight,
            float bottomPadFraction,
            int sortingOrder,
            Color tint)
        {
            Sprite sprite = LoadSprite(fileName);
            if (sprite == null) return null;

            SpriteRenderer renderer = CreateRenderer(objectName, sprite, parent, sortingOrder, tint);
            float scale = targetHeight / sprite.bounds.size.y;
            renderer.transform.localScale = Vector3.one * scale;

            // Compensate for transparent padding at the bottom of the sprite artwork
            // so visual contact points (feet, walls, poles) sit precisely on groundBaseline.
            float bottomPadWorld = bottomPadFraction * targetHeight;
            float bottomY = groundBaseline - bottomPadWorld;
            renderer.transform.position = new Vector3(x, bottomY - sprite.bounds.min.y * scale, 0f);
            return renderer;
        }

        private static void CreateInfiniteSky(Transform parent, Transform cameraTransform)
        {
            Sprite sprite = LoadSprite("Bầu trời pastel ngày trong-1.png");
            if (sprite == null) return;

            Vector2 targetSize = new Vector2(19.2f, 10.8f);
            Transform[] tiles = new Transform[3];

            for (int i = -1; i <= 1; i++)
            {
                float x = i * targetSize.x;
                SpriteRenderer renderer = CreateRenderer($"Sky_{i + 2:00}", sprite, parent, -100, Color.white);
                renderer.transform.localScale = new Vector3(
                    targetSize.x / sprite.bounds.size.x,
                    targetSize.y / sprite.bounds.size.y,
                    1f);
                renderer.transform.localPosition = new Vector3(x, 0f, 0f);
                tiles[i + 1] = renderer.transform;
            }

            var infinite = parent.gameObject.AddComponent<PhoNhoInfiniteLayer>();
            infinite.Configure(cameraTransform, targetSize.x, tiles);
        }

        private static void CreateInfiniteDistantTown(Transform parent, Transform cameraTransform)
        {
            Sprite sprite = LoadSprite("Dải chân trời khu phố Việt pastel-2.png");
            if (sprite == null) return;

            float targetWidth = 20.2f;
            float scale = targetWidth / sprite.bounds.size.x;
            float segmentSpacing = targetWidth * 0.998f;
            Transform[] tiles = new Transform[3];

            for (int i = -1; i <= 1; i++)
            {
                float x = i * segmentSpacing;
                SpriteRenderer renderer = CreateRenderer($"Distant_Town_{i + 2:00}", sprite, parent, -80, new Color(0.86f, 0.93f, 1f, 0.62f));
                renderer.transform.localScale = Vector3.one * scale;
                renderer.transform.localPosition = new Vector3(x, -0.1f, 0f);
                tiles[i + 1] = renderer.transform;
            }

            var infinite = parent.gameObject.AddComponent<PhoNhoInfiniteLayer>();
            infinite.Configure(cameraTransform, segmentSpacing, tiles);
        }

        private static void CreateInfiniteSidewalk(Transform parent, Transform cameraTransform)
        {
            Sprite sprite = LoadSprite("Dải vỉa hè gạch hoa-1.png");
            if (sprite == null) return;

            GameObject container = new GameObject("Sidewalk_Container");
            container.transform.SetParent(parent, false);

            // Texture: 2172x724. Visual brick surface occupies y=213..521 (height = 309 px, top pad = 213 px, bottom pad = 202 px).
            // We want visual brick top surface to align precisely at GroundBaseline (-1.80f).
            float targetHeight = 2.226f;
            float scale = targetHeight / sprite.bounds.size.y;
            float segmentWidth = sprite.bounds.size.x * scale;
            float spacing = segmentWidth * 0.985f;

            // The top of the sprite is placed such that visual brick top sits on GroundBaseline
            float bottom = GroundBaseline - (511f / 724f) * targetHeight;

            int minIdx = -3;
            int maxIdx = 3;
            int count = maxIdx - minIdx + 1;
            Transform[] tiles = new Transform[count];

            for (int index = minIdx; index <= maxIdx; index++)
            {
                float x = index * spacing;
                int tileNum = index - minIdx + 1;
                GameObject gameObject = new GameObject($"Sidewalk_{tileNum:00}");
                gameObject.transform.SetParent(container.transform, false);

                SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 10;
                renderer.color = Color.white;
                renderer.transform.localScale = Vector3.one * scale;
                renderer.transform.localPosition = new Vector3(x, bottom - sprite.bounds.min.y * scale, 0f);

                tiles[index - minIdx] = gameObject.transform;
            }

            var infinite = container.AddComponent<PhoNhoInfiniteLayer>();
            infinite.Configure(cameraTransform, spacing, tiles);
        }

        private static void CreateInfiniteRoad(Transform parent, Transform cameraTransform)
        {
            Sprite sprite = LoadSprite("Mặt đường nhựa be ấm-2.png");
            if (sprite == null) return;

            GameObject container = new GameObject("Road_Container");
            container.transform.SetParent(parent, false);

            Vector2 targetSize = new Vector2(20.4f, 2.80f);
            float spacing = targetSize.x * 0.998f;
            Transform[] tiles = new Transform[3];

            for (int i = -1; i <= 1; i++)
            {
                float x = i * spacing;
                SpriteRenderer renderer = CreateRenderer($"Road_{i + 2:00}", sprite, container.transform, 5, new Color(0.88f, 0.88f, 0.86f, 1f));
                renderer.transform.localScale = new Vector3(
                    targetSize.x / sprite.bounds.size.x,
                    targetSize.y / sprite.bounds.size.y,
                    1f);
                renderer.transform.localPosition = new Vector3(x, -4.15f, 0f);
                tiles[i + 1] = renderer.transform;
            }

            var infinite = container.AddComponent<PhoNhoInfiniteLayer>();
            infinite.Configure(cameraTransform, spacing, tiles);
        }
    }
}
