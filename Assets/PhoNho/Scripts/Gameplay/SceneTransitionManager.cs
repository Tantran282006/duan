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
        public const string CookingSceneName = "Cooking_HoneyBreakfast";

        private static Vector3? _lastStreetReturnPosition;
        private static Vector3? _lastInteriorReturnPosition;
        private static string _lastCompletedDishName;
        private static bool _isTransitioning;

        public static Vector3? LastStreetReturnPosition
        {
            get => _lastStreetReturnPosition;
            set => _lastStreetReturnPosition = value;
        }

        public static Vector3? LastInteriorReturnPosition
        {
            get => _lastInteriorReturnPosition;
            set => _lastInteriorReturnPosition = value;
        }

        public static string LastCompletedDishName
        {
            get => _lastCompletedDishName;
            set => _lastCompletedDishName = value;
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

            Debug.Log($"[SceneTransitionManager] Vào quán nội thất: {interiorSceneName}. Điểm quay về phố: {returnStreetPosition}");
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

        public static void EnterCooking(string cookingSceneName = CookingSceneName, Vector3? returnInteriorPosition = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition is already in progress, ignoring duplicate call.");
                return;
            }

            _isTransitioning = true;
            _lastInteriorReturnPosition = returnInteriorPosition ?? new Vector3(3.2f, -1.0f, 0f);

            Debug.Log($"[SceneTransitionManager] Vào Scene Nấu Ăn: {cookingSceneName}. Điểm quay về quán: {_lastInteriorReturnPosition}");
            OnTransitionStarted?.Invoke(cookingSceneName);

            LoadSceneSafe(cookingSceneName);
        }

        public static void ExitCooking(string interiorSceneName = HoneyBreakfastInteriorSceneName, string dishPrepared = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition is already in progress, ignoring duplicate call.");
                return;
            }

            _isTransitioning = true;
            _lastCompletedDishName = dishPrepared;

            Debug.Log($"[SceneTransitionManager] Hoàn thành/rời nấu ăn trở về quán: {interiorSceneName}. Món: {dishPrepared}");
            OnTransitionStarted?.Invoke(interiorSceneName);

            LoadSceneSafe(interiorSceneName);
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
#if UNITY_EDITOR
                string path = sceneName;
                if (!path.EndsWith(".unity"))
                {
                    path = $"Assets/PhoNho/Scenes/{sceneName}.unity";
                }
                if (System.IO.File.Exists(path))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }
                _isTransitioning = false;
                OnTransitionCompleted?.Invoke(sceneName);
#else
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                _isTransitioning = false;
                OnTransitionCompleted?.Invoke(sceneName);
#endif
            }
        }

        public static void ResetState()
        {
            _lastStreetReturnPosition = null;
            _lastInteriorReturnPosition = null;
            _lastCompletedDishName = null;
            _isTransitioning = false;
        }
    }
}
