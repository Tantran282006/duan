using System.IO;
using System.Linq;
using PhoNho.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.EditorTools
{
    /// <summary>
    /// Builds a balanced reference street from the current Phố Nhỏ art assets.
    /// The command creates a new scene and never edits the user's open scene in place.
    /// </summary>
    public static class PhoNhoStreetSceneBuilder
    {
        private const string BackgroundFolder = "Assets/PhoNho/Art/Backgrounds/";
        private const string SceneFolder = "Assets/PhoNho/Scenes";
        private const string ScenePath = SceneFolder + "/CityOverworld_ArtLayout.unity";

        private const float BuildingBaseline = -1.78f;
        private const float PropBaseline = -2.58f;

        [MenuItem("Phố Nhỏ/Bản đồ/Tạo phố mẫu cân chỉnh")]
        public static void BuildBalancedStreetScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Directory.CreateDirectory(SceneFolder);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new GameObject("PhoNho_CityOverworld");
            Camera camera = CreateCamera(root.transform);

            Transform skyLayer = CreateParallaxLayer(
                "01_Sky",
                root.transform,
                camera.transform,
                Vector2.one);

            Transform distantLayer = CreateParallaxLayer(
                "02_DistantTown",
                root.transform,
                camera.transform,
                new Vector2(0.78f, 0.15f));

            Transform middleLayer = CreateLayer("03_Buildings", root.transform);
            Transform groundLayer = CreateLayer("04_Ground", root.transform);
            Transform propLayer = CreateLayer("05_StreetProps", root.transform);

            Transform foregroundLayer = CreateParallaxLayer(
                "06_Foreground",
                root.transform,
                camera.transform,
                new Vector2(-0.12f, 0f));

            CreateStretchedSprite(
                "Sky_Day",
                "Bầu trời pastel ngày trong-1.png",
                skyLayer,
                Vector2.zero,
                new Vector2(19.2f, 10.8f),
                -100,
                Color.white);

            CreateWidthSprite(
                "Distant_Town",
                "Dải chân trời khu phố Việt pastel-2.png",
                distantLayer,
                new Vector2(0f, -0.1f),
                20.2f,
                -80,
                new Color(0.86f, 0.93f, 1f, 0.62f));

            CreateBottomAlignedSprite(
                "Shade_Tree",
                "Cây nhiệt đới rợp bóng-1.png",
                middleLayer,
                0.1f,
                BuildingBaseline + 0.05f,
                5.6f,
                -10,
                new Color(0.92f, 0.96f, 0.9f, 1f));

            CreateBottomAlignedSprite(
                "Ingredient_Shop",
                "Cửa hàng nguyên liệu Việt-5.png",
                middleLayer,
                -7.05f,
                BuildingBaseline,
                4.25f,
                0,
                Color.white);

            CreateBottomAlignedSprite(
                "Boba_Shop",
                "Cửa hàng trà sữa hồng đáng yêu-3.png",
                middleLayer,
                -2.4f,
                BuildingBaseline,
                4.25f,
                0,
                Color.white);

            CreateBottomAlignedSprite(
                "Player_House",
                "Nhà phố hai tầng hoa giấy ấm áp-5.png",
                middleLayer,
                2.15f,
                BuildingBaseline,
                5.45f,
                1,
                Color.white);

            CreateBottomAlignedSprite(
                "Breakfast_Shop",
                "Quán ăn sáng Việt màu mật ong-4.png",
                middleLayer,
                6.75f,
                BuildingBaseline,
                4.25f,
                0,
                Color.white);

            CreateStretchedSprite(
                "Road",
                "Mặt đường nhựa be ấm-2.png",
                groundLayer,
                new Vector2(0f, -4.15f),
                new Vector2(20.4f, 3.05f),
                5,
                new Color(0.88f, 0.88f, 0.86f, 1f));

            CreateSidewalk(groundLayer);

            CreateBottomAlignedSprite(
                "Street_Lamp",
                "Đèn đường cổ điển màu xanh sage-4.png",
                propLayer,
                -4.75f,
                PropBaseline,
                2.8f,
                20,
                Color.white);

            CreateBottomAlignedSprite(
                "Bench_Left",
                "Ghế băng gỗ nền trong suốt-3.png",
                propLayer,
                0.05f,
                PropBaseline,
                1.05f,
                21,
                Color.white);

            CreateBottomAlignedSprite(
                "Bench_Right",
                "Ghế băng gỗ nền trong suốt-3.png",
                propLayer,
                4.45f,
                PropBaseline,
                1.05f,
                21,
                Color.white);

            SpriteRenderer leftBush = CreateWidthSprite(
                "Foreground_Bush_Left",
                "Cụm bụi lá xanh hoa phấn-6.png",
                foregroundLayer,
                new Vector2(-7.6f, -4.75f),
                5.2f,
                100,
                new Color(0.86f, 0.92f, 0.8f, 1f));

            SpriteRenderer rightBush = CreateWidthSprite(
                "Foreground_Bush_Right",
                "Cụm bụi lá xanh hoa phấn-6.png",
                foregroundLayer,
                new Vector2(7.6f, -4.75f),
                5.2f,
                100,
                new Color(0.86f, 0.92f, 0.8f, 1f));

            rightBush.flipX = true;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();

            Debug.Log(
                $"Đã tạo scene phố mẫu: {ScenePath}. " +
                "Mở Game view 16:9 để kiểm tra bố cục.");
        }

        private static Camera CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.79f, 0.91f, 0.98f, 1f);

            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static Transform CreateLayer(string name, Transform parent)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(parent);
            return layer.transform;
        }

        private static Transform CreateParallaxLayer(
            string name,
            Transform parent,
            Transform cameraTransform,
            Vector2 multiplier)
        {
            Transform layer = CreateLayer(name, parent);
            layer.gameObject
                .AddComponent<ParallaxLayer>()
                .Configure(cameraTransform, multiplier);

            return layer;
        }

        private static void CreateSidewalk(Transform parent)
        {
            Sprite sprite = LoadSprite("Dải vỉa hè gạch hoa-1.png");
            const float targetHeight = 1.18f;
            float scale = targetHeight / sprite.bounds.size.y;
            float segmentWidth = sprite.bounds.size.x * scale;
            float bottom = -3.03f;

            for (int index = -1; index <= 1; index++)
            {
                float x = index * segmentWidth * 0.985f;
                CreateBottomAlignedSprite(
                    $"Sidewalk_{index + 2:00}",
                    sprite,
                    parent,
                    x,
                    bottom,
                    targetHeight,
                    10,
                    Color.white);
            }
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
            return CreateBottomAlignedSprite(
                objectName,
                LoadSprite(fileName),
                parent,
                x,
                bottom,
                targetHeight,
                sortingOrder,
                tint);
        }

        private static SpriteRenderer CreateBottomAlignedSprite(
            string objectName,
            Sprite sprite,
            Transform parent,
            float x,
            float bottom,
            float targetHeight,
            int sortingOrder,
            Color tint)
        {
            SpriteRenderer renderer = CreateRenderer(
                objectName,
                sprite,
                parent,
                sortingOrder,
                tint);

            float scale = targetHeight / sprite.bounds.size.y;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.position = new Vector3(
                x,
                bottom - sprite.bounds.min.y * scale,
                0f);

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
            SpriteRenderer renderer = CreateRenderer(
                objectName,
                sprite,
                parent,
                sortingOrder,
                tint);

            float scale = targetWidth / sprite.bounds.size.x;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.position = position;

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
            SpriteRenderer renderer = CreateRenderer(
                objectName,
                sprite,
                parent,
                sortingOrder,
                tint);

            renderer.transform.localScale = new Vector3(
                targetSize.x / sprite.bounds.size.x,
                targetSize.y / sprite.bounds.size.y,
                1f);

            renderer.transform.position = position;
            return renderer;
        }

        private static SpriteRenderer CreateRenderer(
            string objectName,
            Sprite sprite,
            Transform parent,
            int sortingOrder,
            Color tint)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(parent);

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.color = tint;

            return renderer;
        }

        private static Sprite LoadSprite(string fileName)
        {
            string path = BackgroundFolder + fileName;
            Sprite sprite = AssetDatabase
                .LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(asset => asset.name)
                .FirstOrDefault();

            if (sprite == null)
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy Sprite trong asset: {path}",
                    path);
            }

            return sprite;
        }
    }
}
