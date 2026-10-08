using System;
using System.Linq;
using UnityEngine;

namespace PhoNho.Map
{
    /// <summary>
    /// Repositions an array of repeating background or ground tiles so they continuously
    /// cover the camera view in an infinite loop as the camera moves horizontally.
    /// Works with any coordinate range from negative to positive infinity.
    /// </summary>
    [DisallowMultipleComponent]
    public class PhoNhoInfiniteLayer : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _segmentWidth = 20f;
        [SerializeField] private Transform[] _tiles;

        public float SegmentWidth
        {
            get => _segmentWidth;
            set => _segmentWidth = value;
        }

        public Transform[] Tiles => _tiles;

        public void Configure(Transform cameraTransform, float segmentWidth, Transform[] tiles)
        {
            _cameraTransform = cameraTransform;
            _segmentWidth = segmentWidth;
            _tiles = tiles;
        }

        public void SetTiles(Transform[] tiles)
        {
            _tiles = tiles;
        }

        private void OnEnable()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_tiles == null || _tiles.Length == 0)
            {
                _tiles = transform.Cast<Transform>().ToArray();
            }
        }

        private void LateUpdate()
        {
            UpdateTilesPositions();
        }

        /// <summary>
        /// Updates the horizontal position of all tiles based on camera position (or an override reference X).
        /// </summary>
        public void UpdateTilesPositions(float overrideRefX = float.NaN)
        {
            if (_tiles == null || _tiles.Length <= 1 || _segmentWidth <= 0.001f)
            {
                return;
            }

            float refX;
            if (!float.IsNaN(overrideRefX))
            {
                refX = overrideRefX;
            }
            else
            {
                if (_cameraTransform == null)
                {
                    if (Camera.main != null)
                    {
                        _cameraTransform = Camera.main.transform;
                    }
                    else
                    {
                        return;
                    }
                }
                refX = transform.InverseTransformPoint(_cameraTransform.position).x;
            }

            float totalWidth = _segmentWidth * _tiles.Length;
            float halfTotalWidth = totalWidth * 0.5f;

            for (int i = 0; i < _tiles.Length; i++)
            {
                Transform tile = _tiles[i];
                if (tile == null) continue;

                Vector3 localPos = tile.localPosition;
                float diff = refX - localPos.x;

                if (Mathf.Abs(diff) > halfTotalWidth)
                {
                    float shifts = Mathf.Floor((diff + halfTotalWidth) / totalWidth);
                    localPos.x += shifts * totalWidth;
                    tile.localPosition = localPos;
                }
            }
        }
    }
}
