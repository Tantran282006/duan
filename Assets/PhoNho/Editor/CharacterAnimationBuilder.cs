using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.Character.Editor
{
    [InitializeOnLoad]
    public static class CharacterAnimationBuilder
    {
        private const string PrefKey = "PhoNho_CharacterCleanup_Automation_Ran_V1";
        private const string AnimDir = "Assets/PhoNho/Art/Animations";
        private const string ScenesDir = "Assets/PhoNho/Scenes";
        private const string CharDir = "Assets/PhoNho/Art/Characters";
        private const string LogPath = "Assets/PhoNho/Editor/character-cleanup-execution.log";

        static CharacterAnimationBuilder()
        {
            EditorApplication.delayCall += RunOnceOnLoad;
        }

        private static void RunOnceOnLoad()
        {
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            EditorPrefs.SetBool(PrefKey, true);
            ExecuteAll();
        }

        [MenuItem("Phố Nhỏ/Character/Tạo Animation Clips, Controller và Scene Preview")]
        public static void ExecuteAll()
        {
            var logMessages = new List<string>();
            logMessages.Add($"=== PHỐ NHỎ CHARACTER CLEANUP & ANIMATION AUTOMATION ===");
            logMessages.Add($"Thời gian chạy: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                // 1. Chạy CleanAll từ CharacterSheetTools để xử lý và slice 6 sheet
                logMessages.Add("\n[BƯỚC 1] Chạy PhoNho.Art.Editor.CharacterSheetTools.CleanAll...");
                PhoNho.Art.Editor.CharacterSheetTools.CleanAll();
                logMessages.Add("=> CleanAll hoàn tất thành công!");

                // 2. Kiểm tra các sprite đã được slice trong 6 file
                logMessages.Add("\n[BƯỚC 2] Kiểm tra 32 frame của 6 sheet:");
                string[] targetSheets = new[]
                {
                    "Nhân vật nam đi bộ tám khung-1.png",
                    "Chu kỳ đi bộ cô gái pastel-2.png",
                    "Bộ sprite đi bộ bốn khung-3.png",
                    "Bảng sprite đi bộ bốn khung-4.png",
                    "Male_A_Idle.png",
                    "Female_A_Idle.png"
                };

                int totalFrames = 0;
                var sheetSprites = new Dictionary<string, Sprite[]>();

                foreach (string sheetName in targetSheets)
                {
                    string assetPath = $"{CharDir}/{sheetName}";
                    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                        .OfType<Sprite>()
                        .OrderBy(s => s.name, StringComparer.Ordinal)
                        .ToArray();

                    sheetSprites[sheetName] = sprites;
                    totalFrames += sprites.Length;
                    logMessages.Add($" - {sheetName}: {sprites.Length} frames sliced.");

                    foreach (var sp in sprites)
                    {
                        var pivotNormalized = new Vector2(sp.pivot.x / sp.rect.width, sp.pivot.y / sp.rect.height);
                        if (Mathf.Abs(pivotNormalized.x - 0.5f) > 0.05f || Mathf.Abs(pivotNormalized.y - 0.1f) > 0.05f)
                        {
                            logMessages.Add($"   [Cảnh báo Pivot] {sp.name} pivot: {pivotNormalized}");
                        }
                    }
                }
                logMessages.Add($"=> Tổng số frame kiểm tra được: {totalFrames}/32");

                // 3. Tạo thư mục Animations
                if (!AssetDatabase.IsValidFolder(AnimDir))
                {
                    Directory.CreateDirectory(AnimDir);
                    AssetDatabase.Refresh();
                }

                // 4. Tạo Animation Clips
                logMessages.Add("\n[BƯỚC 3] Tạo Animation Clips (Idle 2 FPS, Walk 8 FPS, Loop Time bật):");

                // Male clips
                var maleIdleSprites = sheetSprites["Male_A_Idle.png"];
                var maleWalkSprites = sheetSprites["Bộ sprite đi bộ bốn khung-3.png"];
                AnimationClip maleIdleClip = CreateOrUpdateClip($"{AnimDir}/Male_A_Idle.anim", maleIdleSprites, 2);
                AnimationClip maleWalkClip = CreateOrUpdateClip($"{AnimDir}/Male_A_Walk.anim", maleWalkSprites, 8);
                logMessages.Add(" - Tạo thành công Male_A_Idle.anim (2 FPS) và Male_A_Walk.anim (8 FPS).");

                // Female clips
                var femaleIdleSprites = sheetSprites["Female_A_Idle.png"];
                var femaleWalkSprites = sheetSprites["Bảng sprite đi bộ bốn khung-4.png"];
                AnimationClip femaleIdleClip = CreateOrUpdateClip($"{AnimDir}/Female_A_Idle.anim", femaleIdleSprites, 2);
                AnimationClip femaleWalkClip = CreateOrUpdateClip($"{AnimDir}/Female_A_Walk.anim", femaleWalkSprites, 8);
                logMessages.Add(" - Tạo thành công Female_A_Idle.anim (2 FPS) và Female_A_Walk.anim (8 FPS).");

                // 5. Tạo Animator Controllers
                logMessages.Add("\n[BƯỚC 4] Tạo Animator Controllers:");
                AnimatorController maleController = CreateOrUpdateController($"{AnimDir}/Male_A_Controller.controller", maleIdleClip, maleWalkClip);
                AnimatorController femaleController = CreateOrUpdateController($"{AnimDir}/Female_A_Controller.controller", femaleIdleClip, femaleWalkClip);
                logMessages.Add(" - Tạo thành công Male_A_Controller.controller và Female_A_Controller.controller.");

                // 6. Tạo Scene Preview riêng
                logMessages.Add("\n[BƯỚC 5] Tạo Scene Preview riêng Assets/PhoNho/Scenes/Character_Preview.unity...");
                BuildPreviewScene(maleController, femaleController, maleIdleSprites.FirstOrDefault(), femaleIdleSprites.FirstOrDefault());
                logMessages.Add(" - Đã dựng Scene Preview với nền sáng và nền tối để kiểm tra quầng viền và độ ổn định của sprite!");

                logMessages.Add("\n=== HOÀN TẤT THÀNH CÔNG ===");
            }
            catch (Exception ex)
            {
                logMessages.Add($"\n[LỖI]: {ex.Message}\n{ex.StackTrace}");
                Debug.LogError($"[CharacterAnimationBuilder] Lỗi: {ex}");
            }
            finally
            {
                File.WriteAllLines(LogPath, logMessages);
                AssetDatabase.Refresh();
                Debug.Log($"[CharacterAnimationBuilder] Đã ghi log tại {LogPath}");
            }
        }

        private static AnimationClip CreateOrUpdateClip(string path, Sprite[] sprites, int fps)
        {
            if (sprites == null || sprites.Length == 0) return null;

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = fps;

            // Bật Loop Time
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / (float)fps,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            return clip;
        }

        private static AnimatorController CreateOrUpdateController(string path, AnimationClip idleClip, AnimationClip walkClip)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            // Đảm bảo có parameters
            if (!controller.parameters.Any(p => p.name == "IsMoving"))
                controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            if (!controller.parameters.Any(p => p.name == "Speed"))
                controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var rootStateMachine = controller.layers[0].stateMachine;

            // Xóa states cũ để dựng mới chuẩn
            var existingStates = rootStateMachine.states.Select(s => s.state).ToArray();
            foreach (var st in existingStates)
            {
                rootStateMachine.RemoveState(st);
            }

            var idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            var walkState = rootStateMachine.AddState("Walk");
            walkState.motion = walkClip;

            rootStateMachine.defaultState = idleState;

            // Idle -> Walk
            var toWalk = idleState.AddTransition(walkState);
            toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
            toWalk.hasExitTime = false;
            toWalk.duration = 0.05f;

            // Walk -> Idle
            var toIdle = walkState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
            toIdle.hasExitTime = false;
            toIdle.duration = 0.05f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void BuildPreviewScene(AnimatorController maleController, AnimatorController femaleController, Sprite defaultMale, Sprite defaultFemale)
        {
            string scenePath = $"{ScenesDir}/Character_Preview.unity";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera 2D Orthographic
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.2f, 0.22f);
            camGo.transform.position = new Vector3(0, 0, -10);

            // 2. Tạo nền kiểm tra (Light panel bên trái, Dark panel bên phải)
            var lightCard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            lightCard.name = "TestBackground_Light";
            lightCard.transform.position = new Vector3(-3.5f, 0, 1);
            lightCard.transform.localScale = new Vector3(6, 7, 1);
            var lightMat = new Material(Shader.Find("Sprites/Default"));
            lightMat.color = new Color(0.95f, 0.95f, 0.92f);
            lightCard.GetComponent<MeshRenderer>().material = lightMat;

            var darkCard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            darkCard.name = "TestBackground_Dark";
            darkCard.transform.position = new Vector3(3.5f, 0, 1);
            darkCard.transform.localScale = new Vector3(6, 7, 1);
            var darkMat = new Material(Shader.Find("Sprites/Default"));
            darkMat.color = new Color(0.12f, 0.12f, 0.14f);
            darkCard.GetComponent<MeshRenderer>().material = darkMat;

            // Mặt đất tham chiếu (Ground line)
            var groundLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            groundLine.name = "Ground_Reference";
            groundLine.transform.position = new Vector3(0, -1.5f, 0.5f);
            groundLine.transform.localScale = new Vector3(14, 0.08f, 1);
            var groundMat = new Material(Shader.Find("Sprites/Default"));
            groundMat.color = new Color(0.5f, 0.6f, 0.5f, 0.8f);
            groundLine.GetComponent<MeshRenderer>().material = groundMat;

            // 3. Nhân vật Nam (Male Preview)
            var maleGo = new GameObject("Male_Character_Preview");
            maleGo.transform.position = new Vector3(-2.5f, -1.5f, 0);
            var maleSr = maleGo.AddComponent<SpriteRenderer>();
            maleSr.sprite = defaultMale;
            var maleAnim = maleGo.AddComponent<Animator>();
            maleAnim.runtimeAnimatorController = maleController;
            maleGo.AddComponent<PhoNhoCharacterPreview>();

            // 4. Nhân vật Nữ (Female Preview)
            var femaleGo = new GameObject("Female_Character_Preview");
            femaleGo.transform.position = new Vector3(2.5f, -1.5f, 0);
            var femaleSr = femaleGo.AddComponent<SpriteRenderer>();
            femaleSr.sprite = defaultFemale;
            var femaleAnim = femaleGo.AddComponent<Animator>();
            femaleAnim.runtimeAnimatorController = femaleController;
            femaleGo.AddComponent<PhoNhoCharacterPreview>();

            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
