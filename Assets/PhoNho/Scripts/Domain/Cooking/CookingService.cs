using System;
using System.Collections.Generic;
using PhoNho.Domain.Economy;
using UnityEngine;

namespace PhoNho.Domain.Cooking
{
    /// <summary>
    /// Coordinates recipe crafting, cooking timer progression, and earning Scoin revenue upon completion.
    /// Provides the default catalogs of ingredients and recipes according to game.md specifications.
    /// </summary>
    [DisallowMultipleComponent]
    public class CookingService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWallet _wallet;
        [SerializeField] private PlayerInventory _inventory;

        [Header("State")]
        [SerializeField] private bool _isCooking;
        [SerializeField] private float _cookTimer;
        [SerializeField] private float _cookDuration;
        [SerializeField] private RecipeItem _currentRecipe;

        public event Action<RecipeItem> OnCookingStarted;
        public event Action<float> OnCookingProgress; // 0.0f .. 1.0f
        public event Action<RecipeItem, int> OnCookingCompleted; // recipe, scoinEarned

        public bool IsCooking => _isCooking;
        public float CookingProgress => _cookDuration > 0.001f ? Mathf.Clamp01(_cookTimer / _cookDuration) : 0f;
        public RecipeItem CurrentRecipe => _currentRecipe;

        // In-memory catalog of standard ingredients
        public static readonly IReadOnlyList<IngredientItem> StandardIngredients = new List<IngredientItem>
        {
            new IngredientItem("tea", "Lá Trà Đen", 10, "tea", "Lá trà thơm đậm vị truyền thống"),
            new IngredientItem("milk", "Sữa Tươi Tiệt Trùng", 12, "dairy", "Sữa bò tươi béo ngậy"),
            new IngredientItem("sugar", "Đường Mía", 5, "sweetener", "Đường phèn tự nhiên ngọt thanh"),
            new IngredientItem("pearl", "Trân Châu Đen", 8, "topping", "Hạt trân châu dẻo dai thơm lừng"),
            new IngredientItem("flour", "Bột Mì Đa Dụng", 10, "grain", "Bột mì hảo hạng làm bánh"),
            new IngredientItem("egg", "Trứng Gà Ta", 6, "protein", "Trứng gà tươi lòng đỏ vàng óng"),
            new IngredientItem("beef", "Thịt Bò Tươi", 25, "protein", "Thịt bò nạc mềm chất lượng cao")
        };

        // In-memory catalog of recipes
        public static readonly IReadOnlyList<RecipeItem> StandardRecipes = new List<RecipeItem>
        {
            // Boba Shop Recipes
            new RecipeItem("recipe_boba_classic", "Trà Sữa Truyền Thống", "Boba", 1.5f, 45, "Trà sữa đậm vị thanh mát",
                ("tea", 1), ("milk", 1), ("sugar", 1)),
            new RecipeItem("recipe_boba_brown_sugar", "Trà Sữa Trân Châu Đường Đen", "Boba", 2.0f, 65, "Vị đường đen đậm đà kèm trân châu dai giòn",
                ("tea", 1), ("milk", 1), ("sugar", 2), ("pearl", 1)),
            new RecipeItem("recipe_boba_milktea", "Hồng Trà Sữa Tươi", "Boba", 1.2f, 40, "Hồng trà kết hợp sữa tươi thanh nhẹ",
                ("tea", 2), ("milk", 1)),

            // Breakfast Shop Recipes
            new RecipeItem("recipe_breakfast_bread_egg", "Bánh Mì Ốp La", "Breakfast", 1.5f, 40, "Bánh mì giòn rụm kèm 2 trứng ốp la",
                ("flour", 1), ("egg", 2)),
            new RecipeItem("recipe_breakfast_beef_noodles", "Phở Bò Phố Nhỏ", "Breakfast", 2.5f, 75, "Tô phở bò nóng hổi thơm nức góc phố",
                ("flour", 1), ("beef", 1), ("sugar", 1)),
            new RecipeItem("recipe_breakfast_pancake", "Bánh Trứng Mật Ong", "Breakfast", 1.5f, 50, "Bánh mềm thơm ngọt ngào",
                ("flour", 1), ("egg", 1), ("milk", 1), ("sugar", 1))
        };

        public void Configure(PlayerWallet wallet, PlayerInventory inventory)
        {
            _wallet = wallet;
            _inventory = inventory;
        }

        private void Awake()
        {
            if (_wallet == null) _wallet = GetComponent<PlayerWallet>() ?? FindFirstObjectByType<PlayerWallet>();
            if (_inventory == null) _inventory = GetComponent<PlayerInventory>() ?? FindFirstObjectByType<PlayerInventory>();
        }

        private void Update()
        {
            if (!_isCooking) return;

            _cookTimer += Time.deltaTime;
            OnCookingProgress?.Invoke(CookingProgress);

            if (_cookTimer >= _cookDuration)
            {
                FinishCooking();
            }
        }

        public bool TryStartCooking(RecipeItem recipe, out string error)
        {
            error = null;

            if (_isCooking)
            {
                error = "Bếp đang bận chế biến món khác!";
                return false;
            }

            if (recipe == null)
            {
                error = "Công thức không hợp lệ.";
                return false;
            }

            if (_inventory == null || !_inventory.HasIngredients(recipe))
            {
                error = "Không đủ nguyên liệu để chế biến!";
                return false;
            }

            // Consume ingredients
            if (!_inventory.TryConsumeIngredientsForRecipe(recipe))
            {
                error = "Lỗi khi trừ nguyên liệu trong kho.";
                return false;
            }

            _currentRecipe = recipe;
            _cookDuration = Mathf.Max(0.1f, recipe.cookingDuration);
            _cookTimer = 0f;
            _isCooking = true;

            OnCookingStarted?.Invoke(recipe);
            OnCookingProgress?.Invoke(0f);
            return true;
        }

        public void FastForwardCooking()
        {
            if (_isCooking)
            {
                _cookTimer = _cookDuration;
                FinishCooking();
            }
        }

        private void FinishCooking()
        {
            _isCooking = false;
            var finishedRecipe = _currentRecipe;
            _currentRecipe = null;

            if (finishedRecipe == null) return;

            // Add product to inventory
            if (_inventory != null)
            {
                _inventory.AddProduct(finishedRecipe.id, 1);
            }

            // Award Scoin revenue through Wallet Ledger
            int earnedScoin = finishedRecipe.rewardScoin;
            if (_wallet != null)
            {
                string idempotencyKey = $"cook_{finishedRecipe.id}_{Guid.NewGuid():N}";
                _wallet.ApplyTransaction(CurrencyType.Scoin, earnedScoin, $"Bán thành phẩm {finishedRecipe.name}", idempotencyKey, out _);
            }

            OnCookingProgress?.Invoke(1.0f);
            OnCookingCompleted?.Invoke(finishedRecipe, earnedScoin);
        }
    }
}
