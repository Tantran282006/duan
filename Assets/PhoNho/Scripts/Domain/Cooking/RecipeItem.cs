using System;
using System.Collections.Generic;

namespace PhoNho.Domain.Cooking
{
    [Serializable]
    public class IngredientRequirement
    {
        public string ingredientId;
        public int quantity;

        public IngredientRequirement() { }

        public IngredientRequirement(string ingredientId, int quantity)
        {
            this.ingredientId = ingredientId;
            this.quantity = quantity;
        }
    }

    [Serializable]
    public class RecipeItem
    {
        public string id;
        public string name;
        public string profession; // "Boba" hoặc "Breakfast"
        public List<IngredientRequirement> requirements = new List<IngredientRequirement>();
        public float cookingDuration; // Thời gian chế biến (giây)
        public int rewardScoin; // Doanh thu khi hoàn tất
        public string description;

        public RecipeItem() { }

        public RecipeItem(string id, string name, string profession, float cookingDuration, int rewardScoin, string description, params (string id, int count)[] reqs)
        {
            this.id = id;
            this.name = name;
            this.profession = profession;
            this.cookingDuration = cookingDuration;
            this.rewardScoin = rewardScoin;
            this.description = description;

            if (reqs != null)
            {
                foreach (var req in reqs)
                {
                    requirements.Add(new IngredientRequirement(req.id, req.count));
                }
            }
        }
    }
}
