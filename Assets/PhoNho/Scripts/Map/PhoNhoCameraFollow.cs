using UnityEngine;

namespace PhoNho.Map
{
    /// <summary>
    /// Smoothly follows a target Transform horizontally with boundary clamping.
    /// Keeps camera height and depth fixed for a side-scrolling 2D street view.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class PhoNhoCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        [Header("Smooth Settings")]
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private bool _clampX = true;
        [SerializeField] private float _minX = -5.0f;
        [SerializeField] private float _maxX = 5.0f;

        private Vector3 _currentVelocity = Vector3.zero;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public bool ClampX
        {
            get => _clampX;
            set => _clampX = value;
        }

        public void SetClamp(bool clamp)
        {
            _clampX = clamp;
        }

        public void DisableClamping()
        {
            _clampX = false;
        }

        public void SetBounds(float minX, float maxX)
        {
            _minX = minX;
            _maxX = maxX;
            _clampX = true;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            float targetX = _target.position.x + _offset.x;
            if (_clampX)
            {
                targetX = Mathf.Clamp(targetX, _minX, _maxX);
            }

            Vector3 targetPosition = new Vector3(targetX, transform.position.y, _offset.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);
        }
    }
}
