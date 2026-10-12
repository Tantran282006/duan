using System;
using UnityEngine;

namespace PhoNho.Gameplay.Interaction
{
    /// <summary>
    /// Placed on the shop interior exit door. Detects proximity and triggers return to street upon [E] press.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopExitTrigger : MonoBehaviour
    {
        [Header("Exit Identity")]
        [SerializeField] private string _promptText = "Nhấn [E] Ra ngoài phố";
        [SerializeField] private float _interactionRadius = 2.4f;
        [SerializeField] private string _streetSceneName = "CityOverworld_PlayerMovement";

        [Header("Runtime State (Read Only)")]
        [SerializeField] private bool _isPlayerInside;

        private Transform _playerTransform;

        public string PromptText => _promptText;
        public float InteractionRadius => _interactionRadius;
        public bool IsPlayerInside => _isPlayerInside;
        public string StreetSceneName => _streetSceneName;

        public static event Action<ShopExitTrigger, bool> OnExitProximityChanged;
        public static event Action<ShopExitTrigger> OnExitInteracted;

        public void Configure(string prompt, float radius = 2.4f, string streetScene = "CityOverworld_PlayerMovement")
        {
            _promptText = prompt;
            _interactionRadius = radius;
            _streetSceneName = streetScene;
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
                OnExitProximityChanged?.Invoke(this, _isPlayerInside);
            }

            if (_isPlayerInside && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
            {
                TriggerExit();
            }
        }

        public void TriggerExit()
        {
            OnExitInteracted?.Invoke(this);
            SceneTransitionManager.ExitShop(_streetSceneName);
        }

        public void ForceSetPlayerInside(bool inside)
        {
            if (_isPlayerInside != inside)
            {
                _isPlayerInside = inside;
                OnExitProximityChanged?.Invoke(this, _isPlayerInside);
            }
        }

        private void OnDisable()
        {
            if (_isPlayerInside)
            {
                _isPlayerInside = false;
                OnExitProximityChanged?.Invoke(this, false);
            }
        }
    }
}
