using System;

namespace PhoNho.Domain.Cooking
{
    [Serializable]
    public class IngredientItem
    {
        public string id;
        public string name;
        public int basePrice; // Scoin
        public string category;
        public string description;

        public IngredientItem() { }

        public IngredientItem(string id, string name, int basePrice, string category, string description)
        {
            this.id = id;
            this.name = name;
            this.basePrice = basePrice;
            this.category = category;
            this.description = description;
        }
    }
}
