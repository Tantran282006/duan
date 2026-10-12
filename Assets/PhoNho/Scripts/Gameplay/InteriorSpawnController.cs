using UnityEngine;
using PhoNho.Character;
using PhoNho.Map;
using PhoNho.UI;

namespace PhoNho.Gameplay
{
    /// <summary>
    /// Checks for a pending return spawn position when returning from the Cooking scene,
    /// placing the player in front of the cooking counter and snapping camera.
    /// Also announces completed dishes if returning from a cooking session.
    /// </summary>
    [DisallowMultipleComponent]
    public class InteriorSpawnController : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private PhoNhoCameraFollow _cameraFollow;

        public void Configure(Transform playerTransform, PhoNhoCameraFollow cameraFollow = null)
        {
            _playerTransform = playerTransform;
            _cameraFollow = cameraFollow;
        }

        private void Start()
        {
            CheckReturnSpawn();
        }

        public void CheckReturnSpawn()
        {
            if (_playerTransform == null)
            {
                if (CompareTag("Player"))
                {
                    _playerTransform = transform;
                }
                else
                {
                    var player = GameObject.FindWithTag("Player");
                    if (player != null) _playerTransform = player.transform;
                    else _playerTransform = transform;
                }
            }

            if (_cameraFollow == null)
            {
                _cameraFollow = FindFirstObjectByType<PhoNhoCameraFollow>();
            }

            // Return from Cooking Scene
            if (SceneTransitionManager.LastInteriorReturnPosition.HasValue)
            {
                Vector3 returnPos = SceneTransitionManager.LastInteriorReturnPosition.Value;
                SceneTransitionManager.LastInteriorReturnPosition = null; // Consume

                if (_playerTransform != null)
                {
                    _playerTransform.position = new Vector3(returnPos.x, returnPos.y, 0f);
                    var rb = _playerTransform.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
#if UNITY_6000_0_OR_NEWER
                        rb.linearVelocity = Vector2.zero;
#else
                        rb.velocity = Vector2.zero;
#endif
                    }

                    if (_cameraFollow != null)
                    {
                        _cameraFollow.SnapToTarget();
                    }

                    Debug.Log($"[InteriorSpawnController] Đặt nhân vật thành công tại vị trí trước quầy bếp: {returnPos}");
                }
            }

            // Check if returned with a completed dish
            if (!string.IsNullOrEmpty(SceneTransitionManager.LastCompletedDishName))
            {
                string dish = SceneTransitionManager.LastCompletedDishName;
                SceneTransitionManager.LastCompletedDishName = null; // Consume

                var uiManager = FindFirstObjectByType<ShopUIManager>();
                if (uiManager != null)
                {
                    uiManager.ShowPrompt($"★ Đã hoàn thành món '{dish}'! Thu nhập +35 Scoin ★");
                }
                Debug.Log($"[InteriorSpawnController] Hoàn thành chế biến món ăn: {dish}");
            }
        }
    }
}
