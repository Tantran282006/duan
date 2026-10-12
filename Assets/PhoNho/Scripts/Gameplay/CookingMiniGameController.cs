using System;
using PhoNho.Domain;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace PhoNho.Gameplay
{
    /// <summary>
    /// Interactive cooking station mini-game for preparing signature dishes (e.g. Bánh Mì Ốp La Giòn Rụm).
    /// Supports both interactive UI clicks and direct keyboard inputs [1], [2], [3], [4], [Space], [Esc].
    /// Automatically awards Scoin to wallet via ledger and adds dish to inventory.
    /// </summary>
    [DisallowMultipleComponent]
    public class CookingMiniGameController : MonoBehaviour
    {
        public const string RecipeName = "Bánh Mì Ốp La Giòn Rụm";
        public const int RewardScoin = 35;

        [Header("Cooking Step States (0: Ready, 1: Butter melted, 2: Eggs fried, 3: Bread toasted, 4: Plated)")]
        [SerializeField] private int _currentStep = 0;
        [SerializeField] private bool _isCompleted = false;

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer _panRenderer;
        [SerializeField] private SpriteRenderer _finishedDishRenderer;
        [SerializeField] private SpriteRenderer _stoveFlameRenderer;

        [Header("Sprites for Pan States")]
        [SerializeField] private Sprite _panEmptySprite;
        [SerializeField] private Sprite _panButterSprite;
        [SerializeField] private Sprite _panEggsSprite;

        [Header("UI Text Displays")]
        [SerializeField] private Text _stepInstructionText;
        [SerializeField] private Text _statusNoticeText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Text _actionButtonText;
        [SerializeField] private Button _returnButton;

        [Header("Domain Dependencies")]
        [SerializeField] private PlayerWallet _wallet;
        [SerializeField] private PlayerInventory _inventory;

        public int CurrentStep => _currentStep;
        public bool IsCompleted => _isCompleted;

        public void Configure(
            SpriteRenderer panRenderer,
            SpriteRenderer finishedDishRenderer,
            SpriteRenderer stoveFlameRenderer,
            Text stepInstructionText,
            Text statusNoticeText,
            Button actionButton,
            Text actionButtonText,
            Button returnButton,
            PlayerWallet wallet,
            PlayerInventory inventory)
        {
            _panRenderer = panRenderer;
            _finishedDishRenderer = finishedDishRenderer;
            _stoveFlameRenderer = stoveFlameRenderer;
            _stepInstructionText = stepInstructionText;
            _statusNoticeText = statusNoticeText;
            _actionButton = actionButton;
            _actionButtonText = actionButtonText;
            _returnButton = returnButton;
            _wallet = wallet;
            _inventory = inventory;
        }

        private void Start()
        {
            if (_wallet == null) _wallet = FindFirstObjectByType<PlayerWallet>();
            if (_inventory == null) _inventory = FindFirstObjectByType<PlayerInventory>();

            if (_actionButton != null)
            {
                _actionButton.onClick.AddListener(HandleActionButtonClick);
            }
            if (_returnButton != null)
            {
                _returnButton.onClick.AddListener(CancelAndReturn);
            }

            UpdateStepUI();
        }

        private void Update()
        {
            // Keyboard Controls
            if (Input.GetKeyDown(KeyCode.Alpha1) && _currentStep == 0)
            {
                ExecuteStep1_HeatPanAndButter();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && _currentStep == 1)
            {
                ExecuteStep2_CrackEggs();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && _currentStep == 2)
            {
                ExecuteStep3_ToastBreadAndPate();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) && _currentStep == 3)
            {
                ExecuteStep4_PlateAndFinish();
            }
            else if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && _isCompleted)
            {
                CompleteAndReturn();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelAndReturn();
            }
        }

        public void HandleActionButtonClick()
        {
            switch (_currentStep)
            {
                case 0:
                    ExecuteStep1_HeatPanAndButter();
                    break;
                case 1:
                    ExecuteStep2_CrackEggs();
                    break;
                case 2:
                    ExecuteStep3_ToastBreadAndPate();
                    break;
                case 3:
                    ExecuteStep4_PlateAndFinish();
                    break;
                case 4:
                    CompleteAndReturn();
                    break;
            }
        }

        public void ExecuteStep1_HeatPanAndButter()
        {
            _currentStep = 1;
            if (_stoveFlameRenderer != null) _stoveFlameRenderer.gameObject.SetActive(true);
            if (_finishedDishRenderer != null) _finishedDishRenderer.gameObject.SetActive(false);
            UpdateStepUI();
            Debug.Log("[CookingMiniGame] Bước 1 hoàn thành: Làm nóng chảo và thả bơ tan chảy thơm lừng!");
        }

        public void ExecuteStep2_CrackEggs()
        {
            _currentStep = 2;
            UpdateStepUI();
            Debug.Log("[CookingMiniGame] Bước 2 hoàn thành: Đập 2 trứng gà tươi vào chảo ốp la xèo xèo!");
        }

        public void ExecuteStep3_ToastBreadAndPate()
        {
            _currentStep = 3;
            UpdateStepUI();
            Debug.Log("[CookingMiniGame] Bước 3 hoàn thành: Nướng bánh mì baguette vàng giòn & kẹp pate béo ngậy!");
        }

        public void ExecuteStep4_PlateAndFinish()
        {
            _currentStep = 4;
            _isCompleted = true;
            if (_finishedDishRenderer != null) _finishedDishRenderer.gameObject.SetActive(true);
            UpdateStepUI();
            Debug.Log("[CookingMiniGame] Bước 4 hoàn thành: Bày ra đĩa sứ truyền thống món Bánh Mì Ốp La Giòn Rụm!");
        }

        public void CompleteAndReturn()
        {
            // Award money and dish
            string key = "cook_reward_" + Guid.NewGuid().ToString("N");

            if (_wallet != null)
            {
                _wallet.ApplyTransaction(CurrencyType.Scoin, RewardScoin, $"Nấu hoàn thành '{RecipeName}'", key, out _);
            }
            else
            {
                PhoNhoGameState.RecordTransaction(CurrencyType.Scoin, RewardScoin, $"Nấu hoàn thành '{RecipeName}'", key);
            }

            if (_inventory != null)
            {
                _inventory.TryConsumeIngredient("egg", 1);
                _inventory.TryConsumeIngredient("flour", 1);
                _inventory.AddProduct("banh_mi_op_la", 1);
            }
            else
            {
                PhoNhoGameState.AddProduct("banh_mi_op_la", 1);
            }

            Debug.Log($"[CookingMiniGame] Đã thưởng +{RewardScoin} Scoin và lưu món '{RecipeName}'. Đang quay về quán...");
            SceneTransitionManager.ExitCooking(SceneTransitionManager.HoneyBreakfastInteriorSceneName, RecipeName);
        }

        public void CancelAndReturn()
        {
            Debug.Log("[CookingMiniGame] Hủy nấu ăn, quay về quán nội thất.");
            SceneTransitionManager.ExitCooking(SceneTransitionManager.HoneyBreakfastInteriorSceneName, null);
        }

        private void UpdateStepUI()
        {
            if (_stepInstructionText != null)
            {
                switch (_currentStep)
                {
                    case 0:
                        _stepInstructionText.text = "BƯỚC 1: Nhấn [1] hoặc nút để Thả Bơ Làm Nóng Chảo";
                        break;
                    case 1:
                        _stepInstructionText.text = "BƯỚC 2: Nhấn [2] hoặc nút để Đập Trứng Gà Vào Chảo";
                        break;
                    case 2:
                        _stepInstructionText.text = "BƯỚC 3: Nhấn [3] hoặc nút để Nướng Bánh Mì & Kẹp Pa-tê";
                        break;
                    case 3:
                        _stepInstructionText.text = "BƯỚC 4: Nhấn [4] hoặc nút để Bày Đĩa & Rắc Tiêu Hành";
                        break;
                    case 4:
                        _stepInstructionText.text = "★ HOÀN THÀNH! Bánh Mì Ốp La Giòn Rụm Sẵn Sàng Phục Vụ ★";
                        break;
                }
            }

            if (_statusNoticeText != null)
            {
                _statusNoticeText.text = _isCompleted
                    ? $"Thành phẩm: +{RewardScoin} Scoin. Nhấn [SPACE] để Mang Ra Quán Phục Vụ Khách!"
                    : $"Tiến độ: {_currentStep}/4 bước chế biến.";
            }

            if (_actionButtonText != null)
            {
                switch (_currentStep)
                {
                    case 0:
                        _actionButtonText.text = "1. Thả Bơ Vào Chảo [1]";
                        break;
                    case 1:
                        _actionButtonText.text = "2. Đập Trứng Vào Chảo [2]";
                        break;
                    case 2:
                        _actionButtonText.text = "3. Nướng Bánh & Phết Pate [3]";
                        break;
                    case 3:
                        _actionButtonText.text = "4. Bày Đĩa Hoàn Tất [4]";
                        break;
                    case 4:
                        _actionButtonText.text = "Hoàn Thành & Về Quán [SPACE]";
                        break;
                }
            }
        }
    }
}
