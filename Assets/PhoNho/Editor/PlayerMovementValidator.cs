using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhoNho.Character;
using PhoNho.Gameplay.Editor;
using PhoNho.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    public static class PlayerMovementValidator
    {
        public const string ResultPath = "Assets/PhoNho/Editor/player-movement-validation-results.json";

        [Serializable]
        public class ValidationReport
        {
            public bool success;
            public string timestamp;
            public List<string> prefabChecks = new List<string>();
            public List<string> sceneChecks = new List<string>();
            public List<string> physicsSimulation = new List<string>();
            public List<string> infiniteWorldChecks = new List<string>();
            public List<string> regressionChecks = new List<string>();
            public List<string> errors = new List<string>();
        }

        [MenuItem("Phố Nhỏ/Gameplay/Kiểm tra toàn diện Player Movement 2D")]
        public static void ValidateAll()
        {
            var report = new ValidationReport
            {
                timestamp = DateTime.UtcNow.ToString("o")
            };

            try
            {
                // Ensure Prefab and Scene exist
                if (!File.Exists(PhoNhoGameplaySceneBuilder.PrefabPath) || !File.Exists(PhoNhoGameplaySceneBuilder.ScenePath))
                {
                    Debug.Log("[PlayerMovementValidator] Prefab hoặc Scene chưa tồn tại. Đang tự động xây dựng...");
                    PhoNhoGameplaySceneBuilder.BuildAll();
                }

                // 1. Kiểm tra Player Prefab
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PhoNhoGameplaySceneBuilder.PrefabPath);
                if (prefab == null)
                {
                    report.errors.Add($"Không thể tải Prefab tại {PhoNhoGameplaySceneBuilder.PrefabPath}");
                }
                else
                {
                    var rb = prefab.GetComponent<Rigidbody2D>();
                    var col = prefab.GetComponent<Collider2D>();
                    var movement = prefab.GetComponent<PhoNhoPlayerMovement>();
                    var sr = prefab.GetComponent<SpriteRenderer>();
                    var anim = prefab.GetComponent<Animator>();

                    if (rb == null) report.errors.Add("Player Prefab thiếu Rigidbody2D.");
                    else
                    {
                        bool freezeRot = (rb.constraints & RigidbodyConstraints2D.FreezeRotation) != 0;
                        bool dynamicType = rb.bodyType == RigidbodyType2D.Dynamic;
                        bool hasGravity = rb.gravityScale > 0.01f;

                        report.prefabChecks.Add($"Rigidbody2D: dynamic={dynamicType}, freezeRotation={freezeRot}, gravityScale={rb.gravityScale}, collisionMode={rb.collisionDetectionMode}");

                        if (!freezeRot) report.errors.Add("Player Rigidbody2D chưa khóa FreezeRotation Z.");
                        if (!dynamicType) report.errors.Add("Player Rigidbody2D phải có bodyType = Dynamic.");
                        if (!hasGravity) report.errors.Add("Player Rigidbody2D phải có gravityScale > 0.");
                    }

                    if (col == null) report.errors.Add("Player Prefab thiếu Collider2D.");
                    else
                    {
                        report.prefabChecks.Add($"Collider2D: type={col.GetType().Name}, isTrigger={col.isTrigger}, boundsSize={col.bounds.size}");
                        if (col.isTrigger) report.errors.Add("Player Collider2D không được là Trigger để tương tác vật lý với mặt đất.");
                    }

                    if (movement == null) report.errors.Add("Player Prefab thiếu PhoNhoPlayerMovement component.");
                    else report.prefabChecks.Add($"PhoNhoPlayerMovement: moveSpeed={movement.MoveSpeed}");

                    if (sr == null || sr.sprite == null) report.errors.Add("Player Prefab thiếu SpriteRenderer hoặc sprite.");
                    else report.prefabChecks.Add($"SpriteRenderer: sprite={sr.sprite.name}, sortingOrder={sr.sortingOrder}");

                    if (anim == null || anim.runtimeAnimatorController == null) report.errors.Add("Player Prefab thiếu Animator hoặc runtimeAnimatorController.");
                    else report.prefabChecks.Add($"Animator: controller={anim.runtimeAnimatorController.name}");
                }

                // 2. Kiểm tra Scene Gameplay
                Scene scene = EditorSceneManager.OpenScene(PhoNhoGameplaySceneBuilder.ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    report.errors.Add($"Scene {PhoNhoGameplaySceneBuilder.ScenePath} không mở được hoặc không hợp lệ.");
                }
                else
                {
                    var roots = scene.GetRootGameObjects();
                    var cameraGo = GameObject.FindWithTag("MainCamera");
                    if (cameraGo == null) report.errors.Add("Thiếu Main Camera trong Gameplay Scene.");
                    else
                    {
                        var cam = cameraGo.GetComponent<Camera>();
                        var follow = cameraGo.GetComponent<PhoNhoCameraFollow>();
                        report.sceneChecks.Add($"Camera: orthographic={cam.orthographic}, size={cam.orthographicSize}, hasCameraFollow={(follow != null)}");
                        if (!cam.orthographic) report.errors.Add("Camera phải là 2D Orthographic.");
                    }

                    // Ground Platform
                    var groundGo = GameObject.Find("Ground_Platform");
                    if (groundGo == null) report.errors.Add("Thiếu GameObject Ground_Platform trong Scene.");
                    else
                    {
                        var groundCol = groundGo.GetComponent<Collider2D>();
                        if (groundCol == null) report.errors.Add("Ground_Platform thiếu Collider2D.");
                        else
                        {
                            report.sceneChecks.Add($"Ground_Platform: position={groundGo.transform.position}, collider={groundCol.GetType().Name}, isTrigger={groundCol.isTrigger}, bounds={groundCol.bounds}");
                            if (groundCol.isTrigger) report.errors.Add("Ground Collider2D không được là Trigger.");
                            
                            // Check Ground surface aligns with GroundBaseline
                            float groundTop = groundCol.bounds.max.y;
                            if (Mathf.Abs(groundTop - PhoNhoGameplaySceneBuilder.GroundBaseline) > 0.05f)
                            {
                                report.errors.Add($"Ground Collider mặt trên ({groundTop:F2}) không khớp với GroundBaseline ({PhoNhoGameplaySceneBuilder.GroundBaseline:F2}).");
                            }
                        }
                    }

                    // Sidewalk checks
                    var sidewalkGo = GameObject.Find("Sidewalk_01");
                    if (sidewalkGo == null) report.errors.Add("Thiếu Sidewalk trong Scene. Vỉa hè chưa được tạo.");
                    else report.sceneChecks.Add("Sidewalk_01 visual surface found.");

                    // Player Instance
                    var playerInstance = GameObject.Find("Player_Character");
                    if (playerInstance == null) report.errors.Add("Thiếu Player_Character trong Scene.");
                    else
                    {
                        report.sceneChecks.Add($"Player_Character instance found at position={playerInstance.transform.position}");
                    }
                }

                // 3. Mô phỏng Physics 2D thực tế (Gravity fall, Ground collision, Left/Right Movement)
                var testPlayer = GameObject.Find("Player_Character");
                var testGround = GameObject.Find("Ground_Platform");

                if (testPlayer != null && testGround != null)
                {
                    var rb = testPlayer.GetComponent<Rigidbody2D>();
                    var movement = testPlayer.GetComponent<PhoNhoPlayerMovement>();
                    var sr = testPlayer.GetComponent<SpriteRenderer>();

                    // Spawn above ground
                    testPlayer.transform.position = new Vector3(0f, -0.6f, 0f);
#if UNITY_6000_0_OR_NEWER
                    rb.linearVelocity = Vector2.zero;
#else
                    rb.velocity = Vector2.zero;
#endif
                    rb.angularVelocity = 0f;
                    movement.SetUseInputManager(false); // Drive directly for deterministic test

                    float spawnY = testPlayer.transform.position.y;
                    float groundSurfaceY = testGround.GetComponent<Collider2D>().bounds.max.y;

                    report.physicsSimulation.Add($"Spawn: Player Y={spawnY:F2}, Ground Surface Y={groundSurfaceY:F2}");

                    // Simulate 1.5 seconds of gravity fall (75 steps of 0.02s)
                    Physics2D.simulationMode = SimulationMode2D.Script;
                    for (int i = 0; i < 75; i++)
                    {
                        Physics2D.Simulate(0.02f);
                    }

                    float landedY = testPlayer.transform.position.y;
                    float landedRotation = testPlayer.transform.eulerAngles.z;
                    report.physicsSimulation.Add($"Landed after fall: Player Y={landedY:F2}, Z-Rotation={landedRotation:F2} deg");

                    // Verify Player fell and stopped on ground (did not fall through to -Infinity)
                    if (landedY >= spawnY)
                    {
                        report.errors.Add($"Player không rơi xuống dưới tác dụng của Gravity (spawnY={spawnY}, landedY={landedY}).");
                    }
                    if (landedY < groundSurfaceY - 0.2f)
                    {
                        report.errors.Add($"Player rơi xuyên qua Collider mặt đất! (groundSurfaceY={groundSurfaceY}, landedY={landedY}).");
                    }
                    if (Mathf.Abs(landedY - groundSurfaceY) > 0.08f)
                    {
                        report.errors.Add($"Bàn chân Player không khớp với mặt đất! (landedY={landedY:F2}, groundSurfaceY={groundSurfaceY:F2}). Chân bị chìm hoặc bay.");
                    }
                    if (Mathf.Abs(landedRotation) > 0.05f)
                    {
                        report.errors.Add($"Player bị xoay/lăn khi rơi chạm đất (Z-rot={landedRotation}). FreezeRotation chưa hoạt động!");
                    }

                    // Test Movement Right (input = 1.0f)
                    float startX = testPlayer.transform.position.x;
                    movement.SetMoveInput(1.0f);

                    for (int i = 0; i < 25; i++)
                    {
                        // Simulate FixedUpdate physics step
                        var vel = rb.linearVelocity;
                        vel.x = 1.0f * movement.MoveSpeed;
                        rb.linearVelocity = vel;
                        Physics2D.Simulate(0.02f);
                    }

                    float moveRightX = testPlayer.transform.position.x;
                    bool faceRight = !sr.flipX;
                    report.physicsSimulation.Add($"Move Right: Start X={startX:F2}, End X={moveRightX:F2}, flipX={sr.flipX} (expected false)");

                    if (moveRightX <= startX + 0.5f)
                    {
                        report.errors.Add($"Player không di chuyển sang phải khi có input phải (startX={startX}, endX={moveRightX}).");
                    }
                    if (!faceRight)
                    {
                        report.errors.Add("Player đi sang phải nhưng flipX = true (hướng nhìn sai).");
                    }

                    // Test Movement Left (input = -1.0f)
                    movement.SetMoveInput(-1.0f);
                    for (int i = 0; i < 50; i++)
                    {
                        var vel = rb.linearVelocity;
                        vel.x = -1.0f * movement.MoveSpeed;
                        rb.linearVelocity = vel;
                        Physics2D.Simulate(0.02f);
                    }

                    float moveLeftX = testPlayer.transform.position.x;
                    bool faceLeft = sr.flipX;
                    report.physicsSimulation.Add($"Move Left: End X={moveLeftX:F2}, flipX={sr.flipX} (expected true)");

                    if (moveLeftX >= moveRightX - 0.5f)
                    {
                        report.errors.Add($"Player không di chuyển sang trái khi có input trái (startX={moveRightX}, endX={moveLeftX}).");
                    }
                    if (!faceLeft)
                    {
                        report.errors.Add("Player đi sang trái nhưng flipX = false (hướng nhìn sai).");
                    }

                    // Test Gravity & Rotation after horizontal movement
                    float currentYAfterMove = testPlayer.transform.position.y;
                    float currentRotAfterMove = testPlayer.transform.eulerAngles.z;
                    report.physicsSimulation.Add($"Post-Movement Physics: Player Y={currentYAfterMove:F2}, Z-Rotation={currentRotAfterMove:F2} deg");

                    if (currentYAfterMove < groundSurfaceY - 0.5f)
                    {
                        report.errors.Add("Player bị tụt xuyên mặt đất trong quá trình di chuyển ngang.");
                    }
                    if (Mathf.Abs(currentRotAfterMove) > 0.05f)
                    {
                        report.errors.Add("Player bị xoay/nghiêng khi di chuyển trên mặt đất.");
                    }

                    // Reset simulation mode to FixedUpdate
                    Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                    movement.SetUseInputManager(true);
                }

                // 4. Kiểm tra Infinite Background & Road/Ground (Chạy xa cả trái lẫn phải)
                var camGo = GameObject.FindWithTag("MainCamera");
                var camFollow = camGo != null ? camGo.GetComponent<PhoNhoCameraFollow>() : null;
                var groundColObj = GameObject.Find("Ground_Platform");
                var infiniteGround = groundColObj != null ? groundColObj.GetComponent<PhoNhoInfiniteGroundCollider>() : null;
                var infiniteLayers = UnityEngine.Object.FindObjectsByType<PhoNhoInfiniteLayer>(FindObjectsSortMode.None);

                if (camFollow != null)
                {
                    report.infiniteWorldChecks.Add($"CameraFollow ClampX={camFollow.ClampX} (phải là false để di chuyển vô tận).");
                    if (camFollow.ClampX)
                    {
                        report.errors.Add("CameraFollow vẫn còn ClampX=true. Camera sẽ bị nghẽn biên khi chạy xa!");
                    }
                }
                else
                {
                    report.errors.Add("Không tìm thấy PhoNhoCameraFollow trên Main Camera.");
                }

                // Kiểm tra nguyên tắc: TUYỆT ĐỐI KHÔNG recycle/nhân bản nhà và prop
                string[] uniqueEntities = new[]
                {
                    "Ingredient_Shop", "Boba_Shop", "Player_House", "Breakfast_Shop", "Shade_Tree",
                    "Street_Lamp", "Bench_Left", "Bench_Right"
                };

                foreach (string entityName in uniqueEntities)
                {
                    var found = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                        .Where(go => go.name == entityName)
                        .ToList();
                    report.infiniteWorldChecks.Add($"Entity {entityName}: count={found.Count} (yêu cầu đúng 1, không được nhân bản)");
                    if (found.Count != 1)
                    {
                        report.errors.Add($"Entity {entityName} có số lượng = {found.Count} (vi phạm quy tắc cấm nhân bản nhà/prop)!");
                    }
                }

                // Mô phỏng chạy xa sang PHẢI (đến X >= +45f)
                if (testPlayer != null && groundColObj != null && camGo != null)
                {
                    var rb = testPlayer.GetComponent<Rigidbody2D>();
                    var movement = testPlayer.GetComponent<PhoNhoPlayerMovement>();
                    movement.SetUseInputManager(false);
                    movement.SetMoveInput(1.0f);

                    Physics2D.simulationMode = SimulationMode2D.Script;

                    // Di chuyển sang phải qua 150 bước physics mô phỏng
                    for (int step = 0; step < 150; step++)
                    {
                        var vel = rb.linearVelocity;
                        vel.x = 15.0f; // Vận tốc nhanh để đi xa trong bài test
                        rb.linearVelocity = vel;

                        Physics2D.Simulate(0.02f);

                        // Cập nhật vị trí collider và camera
                        if (infiniteGround != null)
                        {
                            infiniteGround.ForceRecenter(testPlayer.transform.position.x);
                        }

                        Vector3 camPos = camGo.transform.position;
                        camPos.x = testPlayer.transform.position.x;
                        camGo.transform.position = camPos;

                        foreach (var layer in infiniteLayers)
                        {
                            layer.UpdateTilesPositions(camPos.x);
                        }
                    }

                    float rightX = testPlayer.transform.position.x;
                    float rightY = testPlayer.transform.position.y;
                    float camRightX = camGo.transform.position.x;
                    var colBoundsRight = groundColObj.GetComponent<Collider2D>().bounds;

                    report.infiniteWorldChecks.Add($"Run Far Right: Player reached X={rightX:F1}, Y={rightY:F2}, Camera X={camRightX:F1}, Ground Bounds=[{colBoundsRight.min.x:F1}..{colBoundsRight.max.x:F1}]");

                    if (rightX < 40f)
                    {
                        report.errors.Add($"Player không chạy được xa sang phải (X={rightX:F1} < 40).");
                    }
                    if (rightY < -1.95f || rightY > -1.70f)
                    {
                        report.errors.Add($"Player bị tụt hoặc bay khỏi mặt đất ở xa bên phải (Y={rightY:F2}).");
                    }
                    if (rightX < colBoundsRight.min.x || rightX > colBoundsRight.max.x)
                    {
                        report.errors.Add("Ground Collider không bao bọc Player ở xa bên phải!");
                    }

                    // Kiểm tra vỉa hè và đường bao phủ tầm nhìn camera ở xa bên phải
                    CheckInfiniteLayerCoverage("Sidewalk_Container", camRightX, report);
                    CheckInfiniteLayerCoverage("Road_Container", camRightX, report);

                    // Mô phỏng chạy xa sang TRÁI (từ +45f qua 0 đến X <= -45f)
                    movement.SetMoveInput(-1.0f);
                    for (int step = 0; step < 300; step++)
                    {
                        var vel = rb.linearVelocity;
                        vel.x = -15.0f;
                        rb.linearVelocity = vel;

                        Physics2D.Simulate(0.02f);

                        if (infiniteGround != null)
                        {
                            infiniteGround.ForceRecenter(testPlayer.transform.position.x);
                        }

                        Vector3 camPos = camGo.transform.position;
                        camPos.x = testPlayer.transform.position.x;
                        camGo.transform.position = camPos;

                        foreach (var layer in infiniteLayers)
                        {
                            layer.UpdateTilesPositions(camPos.x);
                        }
                    }

                    float leftX = testPlayer.transform.position.x;
                    float leftY = testPlayer.transform.position.y;
                    float camLeftX = camGo.transform.position.x;
                    var colBoundsLeft = groundColObj.GetComponent<Collider2D>().bounds;

                    report.infiniteWorldChecks.Add($"Run Far Left: Player reached X={leftX:F1}, Y={leftY:F2}, Camera X={camLeftX:F1}, Ground Bounds=[{colBoundsLeft.min.x:F1}..{colBoundsLeft.max.x:F1}]");

                    if (leftX > -40f)
                    {
                        report.errors.Add($"Player không chạy được xa sang trái (X={leftX:F1} > -40).");
                    }
                    if (leftY < -1.95f || leftY > -1.70f)
                    {
                        report.errors.Add($"Player bị tụt hoặc bay khỏi mặt đất ở xa bên trái (Y={leftY:F2}).");
                    }
                    if (leftX < colBoundsLeft.min.x || leftX > colBoundsLeft.max.x)
                    {
                        report.errors.Add("Ground Collider không bao bọc Player ở xa bên trái!");
                    }

                    // Kiểm tra vỉa hè và đường bao phủ tầm nhìn camera ở xa bên trái
                    CheckInfiniteLayerCoverage("Sidewalk_Container", camLeftX, report);
                    CheckInfiniteLayerCoverage("Road_Container", camLeftX, report);

                    // Trả Player và Camera về lại khu phố trung tâm (X = 0)
                    testPlayer.transform.position = new Vector3(0f, -1.0f, 0f);
                    rb.linearVelocity = Vector2.zero;
                    camGo.transform.position = new Vector3(0f, 0f, -10f);
                    if (infiniteGround != null) infiniteGround.ForceRecenter(0f);
                    foreach (var layer in infiniteLayers) layer.UpdateTilesPositions(0f);

                    Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                    movement.SetUseInputManager(true);
                }

                // 4. Kiểm tra Walk Animation của cả Nam và Nữ
                string[] walkClips = new[]
                {
                    "Assets/PhoNho/Art/Animations/Male_A_Walk.anim",
                    "Assets/PhoNho/Art/Animations/Female_A_Walk.anim"
                };

                foreach (string clipPath in walkClips)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (clip == null)
                    {
                        report.errors.Add($"Thiếu AnimationClip: {clipPath}");
                    }
                    else
                    {
                        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                        if (bindings.Length == 0)
                        {
                            report.errors.Add($"Clip {clip.name} không có Sprite curves.");
                        }
                        else
                        {
                            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, bindings[0]);
                            if (keyframes.Length != 4)
                            {
                                report.errors.Add($"Clip {clip.name} kỳ vọng 4 frames nhưng có {keyframes.Length}");
                            }

                            var settings = AnimationUtility.GetAnimationClipSettings(clip);
                            if (!settings.loopTime)
                            {
                                report.errors.Add($"Clip {clip.name} chưa bật LoopTime.");
                            }

                            report.sceneChecks.Add($"Clip {clip.name}: frames={keyframes.Length}, sampleRate={clip.frameRate}, loopTime={settings.loopTime}");
                        }
                    }
                }

                // 5. Kiểm tra Regression: các scene hiện hữu không bị phá vỡ
                string[] existingScenes = new[]
                {
                    "Assets/PhoNho/Scenes/Character_Preview.unity",
                    "Assets/PhoNho/Scenes/CityOverworld_ArtLayout.unity"
                };

                foreach (string sc in existingScenes)
                {
                    bool exists = File.Exists(sc);
                    report.regressionChecks.Add($"Scene {Path.GetFileName(sc)}: exists={exists}");
                    if (!exists) report.errors.Add($"Scene hiện hữu bị mất: {sc}");
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
                Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                string json = JsonUtility.ToJson(report, true);
                File.WriteAllText(ResultPath, json);
                Debug.Log($"[PlayerMovementValidator] Hoàn tất validation: success={report.success}, errors={report.errors.Count}. Đã lưu tại {ResultPath}");
            }

            if (!report.success)
            {
                throw new InvalidOperationException($"Player movement validation failed: {string.Join("; ", report.errors)}");
            }
        }

        public static void ValidateFromCommandLine()
        {
            ValidateAll();
        }

        private static void CheckInfiniteLayerCoverage(string containerName, float camX, ValidationReport report)
        {
            var container = GameObject.Find(containerName);
            if (container == null)
            {
                report.errors.Add($"Không tìm thấy {containerName} để kiểm tra infinite coverage.");
                return;
            }

            var infiniteLayer = container.GetComponent<PhoNhoInfiniteLayer>();
            if (infiniteLayer == null)
            {
                report.errors.Add($"{containerName} thiếu PhoNhoInfiniteLayer component.");
                return;
            }

            // Camera view spans camX - 9.6f to camX + 9.6f
            float viewLeft = camX - 9.6f;
            float viewRight = camX + 9.6f;

            var tiles = infiniteLayer.Tiles;
            if (tiles == null || tiles.Length == 0)
            {
                report.errors.Add($"{containerName} không có tiles nào được quản lý.");
                return;
            }

            float minTileX = float.MaxValue;
            float maxTileX = float.MinValue;
            foreach (var tile in tiles)
            {
                if (tile == null) continue;
                float tx = tile.position.x;
                if (tx < minTileX) minTileX = tx;
                if (tx > maxTileX) maxTileX = tx;
            }

            report.infiniteWorldChecks.Add($"{containerName} coverage at camX={camX:F1}: minTileX={minTileX:F1}, maxTileX={maxTileX:F1} (view: [{viewLeft:F1}..{viewRight:F1}])");

            if (minTileX > viewLeft)
            {
                report.errors.Add($"{containerName} bị khuyết bên trái tầm nhìn camera (minTileX={minTileX:F1} > viewLeft={viewLeft:F1})!");
            }
            if (maxTileX < viewRight)
            {
                report.errors.Add($"{containerName} bị khuyết bên phải tầm nhìn camera (maxTileX={maxTileX:F1} < viewRight={viewRight:F1})!");
            }
        }
    }
}
