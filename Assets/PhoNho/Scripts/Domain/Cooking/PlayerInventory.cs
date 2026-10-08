using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoNho.Domain.Cooking
{
    /// <summary>
    /// Stores the player's stock of raw ingredients and finished cooked products.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInventory : MonoBehaviour
    {
        private readonly Dictionary<string, int> _ingredients = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _products = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public event Action<string, int> OnIngredientChanged; // ingredientId, newTotal
        public event Action<string, int> OnProductChanged;    // recipeId, newTotal

        public IReadOnlyDictionary<string, int> Ingredients => _ingredients;
        public IReadOnlyDictionary<string, int> Products => _products;

        public int GetIngredientCount(string ingredientId)
        {
            if (string.IsNullOrEmpty(ingredientId)) return 0;
            return _ingredients.TryGetValue(ingredientId, out int count) ? count : 0;
        }

        public int GetProductCount(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return 0;
            return _products.TryGetValue(recipeId, out int count) ? count : 0;
        }

        public void AddIngredient(string ingredientId, int amount)
        {
            if (string.IsNullOrEmpty(ingredientId) || amount <= 0) return;
            int current = GetIngredientCount(ingredientId);
            int next = current + amount;
            _ingredients[ingredientId] = next;
            OnIngredientChanged?.Invoke(ingredientId, next);
        }

        public bool TryConsumeIngredient(string ingredientId, int amount)
        {
            if (string.IsNullOrEmpty(ingredientId) || amount <= 0) return false;
            int current = GetIngredientCount(ingredientId);
            if (current < amount) return false;

            int next = current - amount;
            if (next == 0)
            {
                _ingredients.Remove(ingredientId);
            }
            else
            {
                _ingredients[ingredientId] = next;
            }

            OnIngredientChanged?.Invoke(ingredientId, next);
            return true;
        }

        public bool HasIngredients(RecipeItem recipe)
        {
            if (recipe == null || recipe.requirements == null) return false;

            foreach (var req in recipe.requirements)
            {
                if (GetIngredientCount(req.ingredientId) < req.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryConsumeIngredientsForRecipe(RecipeItem recipe)
        {
            if (!HasIngredients(recipe)) return false;

            foreach (var req in recipe.requirements)
            {
                TryConsumeIngredient(req.ingredientId, req.quantity);
            }

            return true;
        }

        public void AddProduct(string recipeId, int amount = 1)
        {
            if (string.IsNullOrEmpty(recipeId) || amount <= 0) return;
            int current = GetProductCount(recipeId);
            int next = current + amount;
            _products[recipeId] = next;
            OnProductChanged?.Invoke(recipeId, next);
        }
    }
}
