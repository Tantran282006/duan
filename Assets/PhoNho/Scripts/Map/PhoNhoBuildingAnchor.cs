using UnityEngine;

namespace PhoNho.Map
{
    /// <summary>
    /// Gắn trên GameObject nhà để đảm bảo luôn giữ cố định tọa độ và tỉ lệ mặc định của riêng từng nhà.
    /// Cho phép đặt lại tọa độ nhanh chóng qua menu ngữ cảnh Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class PhoNhoBuildingAnchor : MonoBehaviour
    {
        [SerializeField] private string _buildingId;
        [SerializeField] private bool _lockPosition = true;

        public string BuildingId => _buildingId;

        private void Reset()
        {
            _buildingId = gameObject.name;
            ApplyDefaultCoordinates();
        }

        private void Awake()
        {
            if (string.IsNullOrEmpty(_buildingId))
            {
                _buildingId = gameObject.name;
            }
            if (_lockPosition)
            {
                ApplyDefaultCoordinates();
            }
        }

        [ContextMenu("Khôi phục tọa độ mặc định")]
        public void ApplyDefaultCoordinates()
        {
            if (string.IsNullOrEmpty(_buildingId)) _buildingId = gameObject.name;
            BuildingDefaultCoordinates.ApplyPlacement(transform, _buildingId);
        }
    }
}
