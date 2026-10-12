using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay
{
    /// <summary>
    /// Coordinates seamless scene transitions between CityOverworld and shop interiors.
    /// Preserves return spawn coordinates outside the building entrance and prevents duplicate transitions.
    /// </summary>
    public static class SceneTransitionManager
    {
        public const string StreetSceneName = "CityOverworld_PlayerMovement";
        public const string HoneyBreakfastInteriorSceneName = "ShopInterior_HoneyBreakfast";

        private static Vector3? _lastStreetReturnPosition;
        private static bool _isTransitioning;

        public static Vector3? LastStreetReturnPosition
        {
            get => _lastStreetReturnPosition;
            set => _lastStreetReturnPosition = value;
        }

        public static bool IsTransitioning
        {
            get => _isTransitioning;
            set => _isTransitioning = value;
        }

        public static event Action<string> OnTransitionStarted;
        public static event Action<string> OnTransitionCompleted;

        public static void EnterShop(string interiorSceneName, Vector3 returnStreetPosition)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition is already in progress, ignoring duplicate call.");
                return;
            }

            _isTransitioning = true;
            _lastStreetReturnPosition = returnStreetPosition;

            Debug.Log($"[SceneTransitionManager] Vào quán nội thất: {interiorSceneName}. Điểm quay về: {returnStreetPosition}");
            OnTransitionStarted?.Invoke(interiorSceneName);

            LoadSceneSafe(interiorSceneName);
        }

        public static void ExitShop(string streetSceneName = StreetSceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition is already in progress, ignoring duplicate call.");
                return;
            }

            _isTransitioning = true;

            Debug.Log($"[SceneTransitionManager] Rời quán trở về phố: {streetSceneName}. Vị trí spawn trước cửa: {_lastStreetReturnPosition}");
            OnTransitionStarted?.Invoke(streetSceneName);

            LoadSceneSafe(streetSceneName);
        }

        private static void LoadSceneSafe(string sceneName)
        {
            if (Application.isPlaying)
            {
                var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                if (asyncOp != null)
                {
                    asyncOp.completed += _ =>
                    {
                        _isTransitioning = false;
                        OnTransitionCompleted?.Invoke(sceneName);
                    };
                }
                else
                {
                    _isTransitioning = false;
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                _isTransitioning = false;
                OnTransitionCompleted?.Invoke(sceneName);
            }
        }

        public static void ResetState()
        {
            _lastStreetReturnPosition = null;
            _isTransitioning = false;
        }
    }
}
