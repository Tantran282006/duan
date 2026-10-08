using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhoNho.Art.Editor;

namespace PhoNho.Character.Editor
{
    [InitializeOnLoad]
    public static class CharacterAnimationBuilder
    {
        private const string PrefKey = "PhoNho_CharacterCleanup_Automation_Ran_V2";
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
            logMessages.Add($"=== PHỐ NHỎ CHARACTER CLEANUP & 1-ROW STRIP AUTOMATION ===");
            logMessages.Add($"Thời gian chạy: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            try
            {
                // 1. Chạy CleanAll từ CharacterSheetTools
                logMessages.Add("\n[BƯỚC 1] Chạy PhoNho.Art.Editor.CharacterSheetTools.CleanAll...");
                CharacterSheetTools.CleanAll();
                logMessages.Add("=> CleanAll hoàn tất thành công!");

                // 2. Slice 4 sprite strip 1 hàng ngang (1x4, 2048x640)
                logMessages.Add("\n[BƯỚC 2] Slice 4 horizontal sprite strip (1 hàng ngang):");
                string[] stripFiles = new[]
                {
                    "Male_A_Idle_Strip.png",
                    "Male_A_Movement_Strip.png",
                    "Female_A_Idle_Strip.png",
                    "Female_A_Movement_Strip.png"
                };

                var stripSprites = new Dictionary<string, Sprite[]>();

                foreach (string strip in stripFiles)
                {
                    string assetPath = $"{CharDir}/{strip}";
                    CharacterSheetTools.Slice(assetPath, 4, 1);
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                        .OfType<Sprite>()
                        .OrderBy(s => s.rect.x)
                        .ToArray();

                    stripSprites[strip] = sprites;
                    logMessages.Add($" - {strip}: {sprites.Length} frames sliced (1 row x 4 cols, each 512x640).");
                }

                // 3. Tạo thư mục Animations
                if (!AssetDatabase.IsValidFolder(AnimDir))
                {
                    Directory.CreateDirectory(AnimDir);
                    AssetDatabase.Refresh();
                }

                // 4. Tạo Animation Clips từ 4 horizontal strips 1 hàng
                logMessages.Add("\n[BƯỚC 3] Tạo Animation Clips từ 1-row strips (Idle 2 FPS, Walk 8 FPS, Loop Time bật):");

                // Male clips
                var maleIdleSprites = stripSprites["Male_A_Idle_Strip.png"];
                var maleWalkSprites = stripSprites["Male_A_Movement_Strip.png"];
                AnimationClip maleIdleClip = CreateOrUpdateClip($"{AnimDir}/Male_A_Idle.anim", maleIdleSprites, 2);
                AnimationClip maleWalkClip = CreateOrUpdateClip($"{AnimDir}/Male_A_Walk.anim", maleWalkSprites, 8);
                logMessages.Add(" - Tạo thành công Male_A_Idle.anim (2 FPS, 1-row strip) và Male_A_Walk.anim (8 FPS, 1-row strip).");

                // Female clips
                var femaleIdleSprites = stripSprites["Female_A_Idle_Strip.png"];
                var femaleWalkSprites = stripSprites["Female_A_Movement_Strip.png"];
                AnimationClip femaleIdleClip = CreateOrUpdateClip($"{AnimDir}/Female_A_Idle.anim", femaleIdleSprites, 2);
                AnimationClip femaleWalkClip = CreateOrUpdateClip($"{AnimDir}/Female_A_Walk.anim", femaleWalkSprites, 8);
                logMessages.Add(" - Tạo thành công Female_A_Idle.anim (2 FPS, 1-row strip) và Female_A_Walk.anim (8 FPS, 1-row strip).");

                // 5. Tạo Animator Controllers
                logMessages.Add("\n[BƯỚC 4] Tạo Animator Controllers:");
                AnimatorController maleController = CreateOrUpdateController($"{AnimDir}/Male_A_Controller.controller", maleIdleClip, maleWalkClip);
                AnimatorController femaleController = CreateOrUpdateController($"{AnimDir}/Female_A_Controller.controller", femaleIdleClip, femaleWalkClip);
                logMessages.Add(" - Tạo thành công Male_A_Controller.controller và Female_A_Controller.controller.");

                // 6. Tạo Scene Preview riêng
                logMessages.Add("\n[BƯỚC 5] Cập nhật Scene Preview riêng Assets/PhoNho/Scenes/Character_Preview.unity...");
                BuildPreviewScene(maleController, femaleController, maleIdleSprites.FirstOrDefault(), femaleIdleSprites.FirstOrDefault());
                logMessages.Add(" - Đã dựng Scene Preview với nền sáng và nền tối sử dụng sprite từ 1-row horizontal strips!");

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

            if (!controller.parameters.Any(p => p.name == "IsMoving"))
                controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            if (!controller.parameters.Any(p => p.name == "Speed"))
                controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var rootStateMachine = controller.layers[0].stateMachine;

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

            var toWalk = idleState.AddTransition(walkState);
            toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
            toWalk.hasExitTime = false;
            toWalk.duration = 0f;
            toWalk.canTransitionToSelf = false;

            var toIdle = walkState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
            toIdle.hasExitTime = false;
            toIdle.duration = 0f;
            toIdle.canTransitionToSelf = false;

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
