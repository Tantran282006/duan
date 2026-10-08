using UnityEngine;

namespace PhoNho.Map
{
    /// <summary>
    /// Keeps the 2D ground platform collider horizontally centered around the player or camera
    /// so the player can walk endlessly in either direction without falling through the world.
    /// Recenter is only triggered when exceeding the threshold to minimize physics re-indexing.
    /// </summary>
    [DisallowMultipleComponent]
    public class PhoNhoInfiniteGroundCollider : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _recenterThreshold = 20f;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float RecenterThreshold
        {
            get => _recenterThreshold;
            set => _recenterThreshold = value;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void ForceRecenter(float targetX)
        {
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;
        }

        private void OnEnable()
        {
            if (_target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _target = player.transform;
                }
                else if (Camera.main != null)
                {
                    _target = Camera.main.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            float diffX = _target.position.x - transform.position.x;
            if (Mathf.Abs(diffX) >= _recenterThreshold)
            {
                Vector3 pos = transform.position;
                pos.x = _target.position.x;
                transform.position = pos;
            }
        }
    }
}
