using UnityEngine;
using PhoNho.Character;
using PhoNho.Map;

namespace PhoNho.Gameplay
{
    /// <summary>
    /// Checks for a pending return spawn position when returning from an interior scene,
    /// placing the player directly in front of the shop and snapping the camera follow target.
    /// </summary>
    [DisallowMultipleComponent]
    public class StreetSpawnController : MonoBehaviour
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
            if (!SceneTransitionManager.LastStreetReturnPosition.HasValue) return;

            Vector3 returnPos = SceneTransitionManager.LastStreetReturnPosition.Value;
            SceneTransitionManager.LastStreetReturnPosition = null; // Consume

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

                Debug.Log($"[StreetSpawnController] Đặt nhân vật thành công tại vị trí trước cửa quán: {returnPos}");
            }
        }
    }
}
