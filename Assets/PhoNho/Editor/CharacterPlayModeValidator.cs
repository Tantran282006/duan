using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhoNho.Character;

namespace PhoNho.Character.Editor
{
    public static class CharacterPlayModeValidator
    {
        public const string ResultPath = "Assets/PhoNho/Editor/character-validation-results.json";

        [Serializable]
        public class ValidationReport
        {
            public bool success;
            public string timestamp;
            public int oneRowStripsChecked;
            public int totalSpritesCount;
            public List<string> stripDetails = new List<string>();
            public List<string> clipDetails = new List<string>();
            public List<string> controllerDetails = new List<string>();
            public List<string> sceneValidation = new List<string>();
            public List<string> errors = new List<string>();
        }

        [MenuItem("Phố Nhỏ/Character/Kiểm tra toàn diện Character Preview")]
        public static void ValidateAll()
        {
            var report = new ValidationReport
            {
                timestamp = DateTime.UtcNow.ToString("o")
            };

            try
            {
                // 1. Kiểm tra 4 sprite strip 1 hàng ngang (1x4, 2048x640)
                string[] mainStrips = new[]
                {
                    "Male_A_Idle_Strip.png",
                    "Male_A_Movement_Strip.png",
                    "Female_A_Idle_Strip.png",
                    "Female_A_Movement_Strip.png"
                };

                report.oneRowStripsChecked = mainStrips.Length;
                int totalSprites = 0;

                foreach (string strip in mainStrips)
                {
                    string path = "Assets/PhoNho/Art/Characters/" + strip;
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.rect.x).ToArray();
                    totalSprites += sprites.Length;

                    if (sprites.Length != 4)
                    {
                        report.errors.Add($"Strip {strip}: kỳ vọng 4 frames nhưng có {sprites.Length} frames.");
                    }

                    bool pivotValid = true;
                    bool oneRowValid = true;

                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sp = sprites[i];
                        // 1 hàng duy nhất: y phải bằng 0
                        if (Mathf.Abs(sp.rect.y) > 0.01f)
                        {
                            oneRowValid = false;
                        }

                        // Frame width/height chuẩn 512x640
                        if (Mathf.Abs(sp.rect.width - 512f) > 0.01f || Mathf.Abs(sp.rect.height - 640f) > 0.01f)
                        {
                            report.errors.Add($"Strip {strip} frame {sp.name} size {sp.rect.width}x{sp.rect.height} khác 512x640.");
                        }

                        var normPivot = new Vector2(sp.pivot.x / sp.rect.width, sp.pivot.y / sp.rect.height);
                        if (Mathf.Abs(normPivot.x - 0.5f) > 0.01f || Mathf.Abs(normPivot.y - 0.1f) > 0.01f)
                        {
                            pivotValid = false;
                        }
                    }

                    if (!oneRowValid)
                    {
                        report.errors.Add($"Strip {strip} vi phạm quy chuẩn 1 hàng ngang (phát hiện rect.y khác 0).");
                    }

                    report.stripDetails.Add($"{strip}: {sprites.Length} frames (kỳ vọng 4), 1-row valid: {oneRowValid}, frame size 512x640, pivot (0.5, 0.1) valid: {pivotValid}, PPU: 256");
                }
                report.totalSpritesCount = totalSprites;

                // 2. Kiểm tra 4 Animation Clips trỏ tới 1-row strips
                string[] clips = new[]
                {
                    "Assets/PhoNho/Art/Animations/Male_A_Idle.anim",
                    "Assets/PhoNho/Art/Animations/Male_A_Walk.anim",
                    "Assets/PhoNho/Art/Animations/Female_A_Idle.anim",
                    "Assets/PhoNho/Art/Animations/Female_A_Walk.anim"
                };
                int[] expectedFps = new[] { 2, 8, 2, 8 };
                string[] expectedStripNames = new[]
                {
                    "Male_A_Idle_Strip",
                    "Male_A_Movement_Strip",
                    "Female_A_Idle_Strip",
                    "Female_A_Movement_Strip"
                };

                for (int c = 0; c < clips.Length; c++)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clips[c]);
                    if (clip == null)
                    {
                        report.errors.Add($"Không tìm thấy clip: {clips[c]}");
                        continue;
                    }
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    int keyframeCount = 0;
                    bool referencesCorrectStrip = true;

                    if (bindings.Length > 0)
                    {
                        var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
                        keyframeCount = keyframes != null ? keyframes.Length : 0;
                        if (keyframes != null)
                        {
                            foreach (var kf in keyframes)
                            {
                                var sp = kf.value as Sprite;
                                if (sp == null || !sp.name.StartsWith(expectedStripNames[c]))
                                {
                                    referencesCorrectStrip = false;
                                    report.errors.Add($"Clip {clips[c]} keyframe tham chiếu sprite {sp?.name ?? "null"} không thuộc strip 1 hàng {expectedStripNames[c]}.");
                                }
                            }
                        }
                    }

                    report.clipDetails.Add($"{Path.GetFileName(clips[c])}: fps={clip.frameRate} (kỳ vọng {expectedFps[c]}), keyframes={keyframeCount}, loopTime={settings.loopTime}, stripRefValid={referencesCorrectStrip}");
                    if (Mathf.Abs(clip.frameRate - expectedFps[c]) > 0.1f)
                        report.errors.Add($"Clip {clips[c]}: fps {clip.frameRate} khác {expectedFps[c]}");
                    if (!settings.loopTime)
                        report.errors.Add($"Clip {clips[c]}: chưa bật loopTime");
                    if (keyframeCount != 4)
                        report.errors.Add($"Clip {clips[c]}: có {keyframeCount} keyframes (kỳ vọng 4)");
                }

                // 3. Kiểm tra Animator Controllers
                string[] controllers = new[]
                {
                    "Assets/PhoNho/Art/Animations/Male_A_Controller.controller",
                    "Assets/PhoNho/Art/Animations/Female_A_Controller.controller"
                };
                foreach (var ctrlPath in controllers)
                {
                    var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                    if (ctrl == null)
                    {
                        report.errors.Add($"Không tìm thấy controller: {ctrlPath}");
                        continue;
                    }
                    bool hasIsMoving = ctrl.parameters.Any(p => p.name == "IsMoving" && p.type == AnimatorControllerParameterType.Bool);
                    bool hasSpeed = ctrl.parameters.Any(p => p.name == "Speed" && p.type == AnimatorControllerParameterType.Float);
                    var states = ctrl.layers[0].stateMachine.states.Select(s => s.state.name).ToList();
                    bool hasIdle = states.Contains("Idle");
                    bool hasWalk = states.Contains("Walk");
                    report.controllerDetails.Add($"{Path.GetFileName(ctrlPath)}: states=[{string.Join(", ", states)}], hasIsMoving={hasIsMoving}, hasSpeed={hasSpeed}");
                    if (!hasIdle || !hasWalk) report.errors.Add($"Controller {ctrlPath} thiếu state Idle hoặc Walk.");
                }

                // 4. Kiểm tra Scene Character_Preview.unity
                string scenePath = "Assets/PhoNho/Scenes/Character_Preview.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    report.errors.Add("Scene Character_Preview.unity không hợp lệ.");
                }
                else
                {
                    var rootGos = scene.GetRootGameObjects();
                    var rootNames = rootGos.Select(g => g.name).ToList();
                    report.sceneValidation.Add($"Scene roots: [{string.Join(", ", rootNames)}]");

                    var camGo = rootGos.FirstOrDefault(g => g.name == "Main Camera");
                    if (camGo != null)
                    {
                        var cam = camGo.GetComponent<Camera>();
                        report.sceneValidation.Add($"Main Camera: orthographic={cam.orthographic}, size={cam.orthographicSize}");
                    }
                    else report.errors.Add("Thiếu Main Camera trong preview scene");

                    var lightCard = rootGos.FirstOrDefault(g => g.name == "TestBackground_Light");
                    var darkCard = rootGos.FirstOrDefault(g => g.name == "TestBackground_Dark");
                    var groundLine = rootGos.FirstOrDefault(g => g.name == "Ground_Reference");
                    report.sceneValidation.Add($"TestBackground_Light: {(lightCard != null ? "Found" : "Missing")}");
                    report.sceneValidation.Add($"TestBackground_Dark: {(darkCard != null ? "Found" : "Missing")}");
                    report.sceneValidation.Add($"Ground_Reference: {(groundLine != null ? "Found" : "Missing")}");

                    var maleGo = rootGos.FirstOrDefault(g => g.name == "Male_Character_Preview");
                    var femaleGo = rootGos.FirstOrDefault(g => g.name == "Female_Character_Preview");

                    if (maleGo != null)
                    {
                        var sr = maleGo.GetComponent<SpriteRenderer>();
                        var anim = maleGo.GetComponent<Animator>();
                        var preview = maleGo.GetComponent<PhoNhoCharacterPreview>();
                        bool spriteFromStrip = sr != null && sr.sprite != null && sr.sprite.name.StartsWith("Male_A_Idle_Strip");
                        report.sceneValidation.Add($"Male Preview Go: SpriteRenderer={(sr != null && sr.sprite != null)}, from1RowStrip={spriteFromStrip}, Animator={(anim != null && anim.runtimeAnimatorController != null)}, PhoNhoCharacterPreview={(preview != null)}");
                        if (!spriteFromStrip) report.errors.Add("Male Preview SpriteRenderer không dùng sprite từ Male_A_Idle_Strip.");
                    }
                    else report.errors.Add("Thiếu Male_Character_Preview");

                    if (femaleGo != null)
                    {
                        var sr = femaleGo.GetComponent<SpriteRenderer>();
                        var anim = femaleGo.GetComponent<Animator>();
                        var preview = femaleGo.GetComponent<PhoNhoCharacterPreview>();
                        bool spriteFromStrip = sr != null && sr.sprite != null && sr.sprite.name.StartsWith("Female_A_Idle_Strip");
                        report.sceneValidation.Add($"Female Preview Go: SpriteRenderer={(sr != null && sr.sprite != null)}, from1RowStrip={spriteFromStrip}, Animator={(anim != null && anim.runtimeAnimatorController != null)}, PhoNhoCharacterPreview={(preview != null)}");
                        if (!spriteFromStrip) report.errors.Add("Female Preview SpriteRenderer không dùng sprite từ Female_A_Idle_Strip.");
                    }
                    else report.errors.Add("Thiếu Female_Character_Preview");
                }

                report.success = report.errors.Count == 0;
            }
            catch (Exception ex)
            {
                report.errors.Add($"Exception: {ex.Message}\n{ex.StackTrace}");
                report.success = false;
            }
            finally
            {
                string json = JsonUtility.ToJson(report, true);
                File.WriteAllText(ResultPath, json);
                Debug.Log($"[CharacterPlayModeValidator] Kết quả: success={report.success}, errors={report.errors.Count}. Đã ghi tại {ResultPath}");
            }

            if (!report.success)
            {
                throw new InvalidOperationException($"Character validation failed: {string.Join("; ", report.errors)}");
            }
        }
    }
}
