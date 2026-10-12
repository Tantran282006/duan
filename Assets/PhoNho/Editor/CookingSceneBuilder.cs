using System;
using System.IO;
using PhoNho.Domain;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using PhoNho.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PhoNho.Gameplay.Editor
{
    /// <summary>
    /// Builds the close-up cooking station scene: Cooking_HoneyBreakfast.unity.
    /// Features warm cozy 2D aesthetics with marble countertop, subway tiles, gas stove,
    /// pan states, ingredients tray, finished dish presentation, and interactive UI controls.
    /// </summary>
    public static class CookingSceneBuilder
    {
        public const string ScenePath = "Assets/PhoNho/Scenes/Cooking_HoneyBreakfast.unity";

        [MenuItem("Phố Nhỏ/Gameplay/Tạo Scene Nấu Ăn Cận Cảnh (Cooking_HoneyBreakfast)")]
        public static void BuildCookingScene()
        {
            Directory.CreateDirectory("Assets/PhoNho/Scenes");

            // Ensure textures exist
            HoneyBreakfastTextureGenerator.GenerateAllTextures();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("PhoNho_CookingStation");

            // 1. Camera
            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.tag = "MainCamera";

            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.96f, 0.94f, 0.90f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.AddComponent<AudioListener>();

            // 2. Kitchen Station Background
            GameObject bgGo = new GameObject("Cooking_Station_Background");
            bgGo.transform.SetParent(root.transform, false);
            bgGo.transform.position = new Vector3(0f, 0f, 0f);
            var bgSr = bgGo.AddComponent<SpriteRenderer>();
            bgSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.CookingStationBackgroundPath);
            bgSr.sortingOrder = -50;

            // 3. Ingredients Tray on Left (X = -3.4f, Y = -1.2f)
            GameObject trayGo = new GameObject("Cooking_Ingredients_Tray");
            trayGo.transform.SetParent(root.transform, false);
            trayGo.transform.position = new Vector3(-3.4f, -1.2f, 0f);
            var traySr = trayGo.AddComponent<SpriteRenderer>();
            traySr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.CookingIngredientsTrayPath);
            traySr.sortingOrder = -20;
            trayGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            // 4. Gas Stove with Blue Flame in Center (X = 0.2f, Y = -1.5f)
            GameObject stoveGo = new GameObject("Cooking_Stove_Flame");
            stoveGo.transform.SetParent(root.transform, false);
            stoveGo.transform.position = new Vector3(0.2f, -1.5f, 0f);
            var stoveSr = stoveGo.AddComponent<SpriteRenderer>();
            stoveSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.CookingStoveFlamePath);
            stoveSr.sortingOrder = -15;
            stoveGo.transform.localScale = new Vector3(0.92f, 0.92f, 1f);

            // 5. Cooking Skillet / Pan (X = 0.2f, Y = -0.1f)
            GameObject panGo = new GameObject("Cooking_Pan_Station");
            panGo.transform.SetParent(root.transform, false);
            panGo.transform.position = new Vector3(0.2f, -0.1f, 0f);
            var panSr = panGo.AddComponent<SpriteRenderer>();
            panSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.CookingPanStatesPath);
            panSr.sortingOrder = -10;
            panGo.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            // 6. Finished Dish Platter on Right (X = 3.6f, Y = -1.1f)
            GameObject dishGo = new GameObject("Cooking_Finished_Dish");
            dishGo.transform.SetParent(root.transform, false);
            dishGo.transform.position = new Vector3(3.6f, -1.1f, 0f);
            var dishSr = dishGo.AddComponent<SpriteRenderer>();
            dishSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HoneyBreakfastTextureGenerator.CookingFinishedDishPath);
            dishSr.sortingOrder = -5;
            dishGo.transform.localScale = new Vector3(0.90f, 0.90f, 1f);
            dishGo.SetActive(false); // Activates after step 4!

            // 7. Domain Systems
            GameObject gameSystemGo = new GameObject("PhoNho_GameSystem");
            gameSystemGo.transform.SetParent(root.transform, false);

            var wallet = gameSystemGo.AddComponent<PlayerWallet>();
            var inventory = gameSystemGo.AddComponent<PlayerInventory>();
            var miniGame = gameSystemGo.AddComponent<CookingMiniGameController>();

            // 8. Canvas UI for Cooking Station
            GameObject canvasGo = new GameObject("Cooking_Canvas");
            canvasGo.transform.SetParent(root.transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Header Banner
            GameObject headerGo = new GameObject("Header_Panel");
            headerGo.transform.SetParent(canvasGo.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.1f, 0.88f);
            headerRect.anchorMax = new Vector2(0.9f, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            var headerImg = headerGo.AddComponent<Image>();
            headerImg.color = new Color(0.24f, 0.16f, 0.10f, 0.85f);

            GameObject titleTextGo = new GameObject("Title_Text");
            titleTextGo.transform.SetParent(headerGo.transform, false);
            var titleTextRect = titleTextGo.AddComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = Vector2.zero;
            titleTextRect.offsetMax = Vector2.zero;

            var titleText = titleTextGo.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.98f, 0.85f, 0.35f, 1f);
            titleText.text = "BẾP QUÁN ĂN SÁNG MẬT ONG — BÁNH MÌ ỐP LA GIÒN RỤM";

            // Step Instruction Panel (y: 0.76..0.86)
            GameObject stepPanelGo = new GameObject("Step_Panel");
            stepPanelGo.transform.SetParent(canvasGo.transform, false);
            var stepRect = stepPanelGo.AddComponent<RectTransform>();
            stepRect.anchorMin = new Vector2(0.15f, 0.77f);
            stepRect.anchorMax = new Vector2(0.85f, 0.86f);
            stepRect.offsetMin = Vector2.zero;
            stepRect.offsetMax = Vector2.zero;

            var stepImg = stepPanelGo.AddComponent<Image>();
            stepImg.color = new Color(0.98f, 0.95f, 0.88f, 0.90f);

            GameObject stepTextGo = new GameObject("Step_Text");
            stepTextGo.transform.SetParent(stepPanelGo.transform, false);
            var stepTextRect = stepTextGo.AddComponent<RectTransform>();
            stepTextRect.anchorMin = Vector2.zero;
            stepTextRect.anchorMax = Vector2.one;
            stepTextRect.offsetMin = new Vector2(10, 0);
            stepTextRect.offsetMax = new Vector2(-10, 0);

            var stepText = stepTextGo.AddComponent<Text>();
            stepText.font = defaultFont;
            stepText.fontSize = 26;
            stepText.fontStyle = FontStyle.Bold;
            stepText.alignment = TextAnchor.MiddleCenter;
            stepText.color = new Color(0.35f, 0.20f, 0.10f, 1f);
            stepText.text = "BƯỚC 1: Nhấn [1] hoặc click nút để Thả Bơ Làm Nóng Chảo";

            // Bottom Control Bar (y: 0.04..0.16)
            GameObject bottomPanelGo = new GameObject("Bottom_Panel");
            bottomPanelGo.transform.SetParent(canvasGo.transform, false);
            var bottomRect = bottomPanelGo.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0.1f, 0.04f);
            bottomRect.anchorMax = new Vector2(0.9f, 0.16f);
            bottomRect.offsetMin = Vector2.zero;
            bottomRect.offsetMax = Vector2.zero;

            var bottomImg = bottomPanelGo.AddComponent<Image>();
            bottomImg.color = new Color(0.18f, 0.14f, 0.10f, 0.90f);

            // Action Button inside Bottom Bar
            GameObject actionBtnGo = new GameObject("Action_Button");
            actionBtnGo.transform.SetParent(bottomPanelGo.transform, false);
            var actionBtnRect = actionBtnGo.AddComponent<RectTransform>();
            actionBtnRect.anchorMin = new Vector2(0.55f, 0.15f);
            actionBtnRect.anchorMax = new Vector2(0.95f, 0.85f);
            actionBtnRect.offsetMin = Vector2.zero;
            actionBtnRect.offsetMax = Vector2.zero;

            var actionBtnImg = actionBtnGo.AddComponent<Image>();
            actionBtnImg.color = new Color(0.95f, 0.55f, 0.15f, 1f); // Warm Orange
            var actionBtn = actionBtnGo.AddComponent<Button>();

            GameObject actionBtnTextGo = new GameObject("Text");
            actionBtnTextGo.transform.SetParent(actionBtnGo.transform, false);
            var actionBtnTextRect = actionBtnTextGo.AddComponent<RectTransform>();
            actionBtnTextRect.anchorMin = Vector2.zero;
            actionBtnTextRect.anchorMax = Vector2.one;
            actionBtnTextRect.offsetMin = Vector2.zero;
            actionBtnTextRect.offsetMax = Vector2.zero;

            var actionBtnText = actionBtnTextGo.AddComponent<Text>();
            actionBtnText.font = defaultFont;
            actionBtnText.fontSize = 24;
            actionBtnText.fontStyle = FontStyle.Bold;
            actionBtnText.alignment = TextAnchor.MiddleCenter;
            actionBtnText.color = Color.white;
            actionBtnText.text = "1. Thả Bơ Vào Chảo [1]";

            // Return / Cancel Button inside Bottom Bar
            GameObject returnBtnGo = new GameObject("Return_Button");
            returnBtnGo.transform.SetParent(bottomPanelGo.transform, false);
            var returnBtnRect = returnBtnGo.AddComponent<RectTransform>();
            returnBtnRect.anchorMin = new Vector2(0.05f, 0.15f);
            returnBtnRect.anchorMax = new Vector2(0.35f, 0.85f);
            returnBtnRect.offsetMin = Vector2.zero;
            returnBtnRect.offsetMax = Vector2.zero;

            var returnBtnImg = returnBtnGo.AddComponent<Image>();
            returnBtnImg.color = new Color(0.40f, 0.35f, 0.32f, 1f);
            var returnBtn = returnBtnGo.AddComponent<Button>();

            GameObject returnBtnTextGo = new GameObject("Text");
            returnBtnTextGo.transform.SetParent(returnBtnGo.transform, false);
            var returnBtnTextRect = returnBtnTextGo.AddComponent<RectTransform>();
            returnBtnTextRect.anchorMin = Vector2.zero;
            returnBtnTextRect.anchorMax = Vector2.one;
            returnBtnTextRect.offsetMin = Vector2.zero;
            returnBtnTextRect.offsetMax = Vector2.zero;

            var returnBtnText = returnBtnTextGo.AddComponent<Text>();
            returnBtnText.font = defaultFont;
            returnBtnText.fontSize = 22;
            returnBtnText.fontStyle = FontStyle.Bold;
            returnBtnText.alignment = TextAnchor.MiddleCenter;
            returnBtnText.color = Color.white;
            returnBtnText.text = "Quay Lại Quán [ESC]";

            // Status Notice Text
            GameObject statusTextGo = new GameObject("Status_Text");
            statusTextGo.transform.SetParent(bottomPanelGo.transform, false);
            var statusRect = statusTextGo.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.36f, 0.15f);
            statusRect.anchorMax = new Vector2(0.54f, 0.85f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;

            var statusText = statusTextGo.AddComponent<Text>();
            statusText.font = defaultFont;
            statusText.fontSize = 20;
            statusText.fontStyle = FontStyle.Bold;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = new Color(0.95f, 0.85f, 0.35f, 1f);
            statusText.text = "Tiến độ: 0/4 bước";

            // Configure MiniGame Controller
            miniGame.Configure(
                panSr,
                dishSr,
                stoveSr,
                stepText,
                statusText,
                actionBtn,
                actionBtnText,
                returnBtn,
                wallet,
                inventory
            );

            // 9. EventSystem
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.transform.SetParent(root.transform, false);
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            HoneyBreakfastSceneBuilder.UpdateBuildSettings();

            Debug.Log($"[CookingSceneBuilder] Đã tạo thành công Scene Nấu Ăn Cận Cảnh tại: {ScenePath}");
        }
    }
}
