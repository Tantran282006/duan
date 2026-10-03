using UnityEngine;

namespace PhoNho.Map
{
    /// <summary>
    /// Keeps a visual layer moving at a controlled fraction of the camera movement.
    /// A multiplier of 1 locks the layer to the camera, 0 keeps it in world space,
    /// and a small negative value makes foreground elements move slightly faster.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Vector2 _movementMultiplier = Vector2.zero;

        private Vector3 _lastCameraPosition;

        public void Configure(Transform cameraTransform, Vector2 movementMultiplier)
        {
            _cameraTransform = cameraTransform;
            _movementMultiplier = movementMultiplier;
            CacheCameraPosition();
        }

        private void OnEnable()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            CacheCameraPosition();
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            Vector3 cameraDelta = _cameraTransform.position - _lastCameraPosition;
            transform.position += new Vector3(
                cameraDelta.x * _movementMultiplier.x,
                cameraDelta.y * _movementMultiplier.y,
                0f);

            _lastCameraPosition = _cameraTransform.position;
        }

        private void CacheCameraPosition()
        {
            if (_cameraTransform != null)
            {
                _lastCameraPosition = _cameraTransform.position;
            }
        }
    }
}
