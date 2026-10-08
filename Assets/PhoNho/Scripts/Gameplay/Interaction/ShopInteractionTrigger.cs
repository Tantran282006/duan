using System;
using UnityEngine;

namespace PhoNho.Gameplay.Interaction
{
    /// <summary>
    /// Placed on street buildings (shops, player house) to detect player proximity
    /// and trigger the corresponding interaction popup when the player presses [E] or taps.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopInteractionTrigger : MonoBehaviour
    {
        [Header("Shop Identity")]
        [SerializeField] private ShopType _shopType;
        [SerializeField] private string _shopDisplayName = "Cửa Hàng";
        [SerializeField] private string _promptText = "Nhấn [E] để vào quán";

        [Header("Detection")]
        [Tooltip("Maximum horizontal and Euclidean distance from player to activate prompt.")]
        [SerializeField] private float _interactionRadius = 2.4f;

        [Header("Runtime State (Read Only)")]
        [SerializeField] private bool _isPlayerInside;

        private Transform _playerTransform;

        public static event Action<ShopInteractionTrigger, bool> OnShopProximityChanged; // trigger, isInside
        public static event Action<ShopInteractionTrigger> OnShopInteracted;             // trigger

        public ShopType ShopType => _shopType;
        public string ShopDisplayName => _shopDisplayName;
        public string PromptText => _promptText;
        public float InteractionRadius => _interactionRadius;
        public bool IsPlayerInside => _isPlayerInside;

        public void Configure(ShopType type, string displayName, string prompt, float radius = 2.4f)
        {
            _shopType = type;
            _shopDisplayName = displayName;
            _promptText = prompt;
            _interactionRadius = radius;
        }

        private void OnEnable()
        {
            CachePlayer();
        }

        private void CachePlayer()
        {
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
            }
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                CachePlayer();
                if (_playerTransform == null) return;
            }

            float dist = Mathf.Abs(_playerTransform.position.x - transform.position.x);
            bool shouldBeInside = dist <= _interactionRadius;

            if (shouldBeInside != _isPlayerInside)
            {
                _isPlayerInside = shouldBeInside;
                OnShopProximityChanged?.Invoke(this, _isPlayerInside);
            }

            // Keyboard shortcut [E] or [KeypadEnter] or [Return]
            if (_isPlayerInside && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
            {
                TriggerInteraction();
            }
        }

        public void TriggerInteraction()
        {
            OnShopInteracted?.Invoke(this);
        }

        public void ForceSetPlayerInside(bool inside)
        {
            if (_isPlayerInside != inside)
            {
                _isPlayerInside = inside;
                OnShopProximityChanged?.Invoke(this, _isPlayerInside);
            }
        }

        private void OnDisable()
        {
            if (_isPlayerInside)
            {
                _isPlayerInside = false;
                OnShopProximityChanged?.Invoke(this, false);
            }
        }
    }
}
