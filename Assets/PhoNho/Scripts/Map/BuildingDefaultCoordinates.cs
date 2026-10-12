using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoNho.Map
{
    [Serializable]
    public struct BuildingPlacement
    {
        public string buildingName;
        public Vector3 defaultPosition;
        public Vector3 defaultScale;
        public int sortingOrder;

        public BuildingPlacement(string name, Vector3 pos, Vector3 scale, int order = 0)
        {
            buildingName = name;
            defaultPosition = pos;
            defaultScale = scale;
            sortingOrder = order;
        }
    }

    /// <summary>
    /// Lưu trữ bảng tọa độ và tỉ lệ mặc định cố định của từng công trình trên phố.
    /// Đảm bảo sau khi build hoặc khởi tạo lại scene, các nhà luôn đứng cố định tại tọa độ này.
    /// </summary>
    public static class BuildingDefaultCoordinates
    {
        // 1. Chợ Nguyên Liệu Việt (X: -7.05, Y: 0.8, Z: 0 | Scale: 0.3781139)
        public static readonly BuildingPlacement IngredientShop = new BuildingPlacement(
            "Ingredient_Shop",
            new Vector3(-7.05f, 0.8f, 0f),
            new Vector3(0.3781139f, 0.3781139f, 0.3781139f),
            0
        );

        // 2. Tiệm Trà Sữa Hồng Đáng Yêu (X: -2.4, Y: 0.7, Z: 0 | Scale: 0.3583474)
        public static readonly BuildingPlacement BobaShop = new BuildingPlacement(
            "Boba_Shop",
            new Vector3(-2.4f, 0.7f, 0f),
            new Vector3(0.3583474f, 0.3583474f, 0.3583474f),
            0
        );

        // 3. Nhà Phố Hai Tầng Hoa Giấy (X: 2.15, Y: 1.4, Z: 0 | Scale: 0.3774238)
        public static readonly BuildingPlacement PlayerHouse = new BuildingPlacement(
            "Player_House",
            new Vector3(2.15f, 1.4f, 0f),
            new Vector3(0.3774238f, 0.3774238f, 0.3774238f),
            1
        );

        // 4. Quán Ăn Sáng Việt Màu Mật Ong (X: 6.75, Y: 0.8, Z: 0 | Scale: 0.3741197)
        public static readonly BuildingPlacement BreakfastShop = new BuildingPlacement(
            "Breakfast_Shop",
            new Vector3(6.75f, 0.8f, 0f),
            new Vector3(0.3741197f, 0.3741197f, 0.3741197f),
            0
        );

        // 5. Cây Nhiệt Đới Rợp Bóng (X: 0.1, Y: 0.86183506, Z: 0 | Scale: 0.44198895)
        public static readonly BuildingPlacement ShadeTree = new BuildingPlacement(
            "Shade_Tree",
            new Vector3(0.1f, 0.86183506f, 0f),
            new Vector3(0.44198895f, 0.44198895f, 0.44198895f),
            -10
        );

        public static readonly IReadOnlyDictionary<string, BuildingPlacement> AllPlacements =
            new Dictionary<string, BuildingPlacement>(StringComparer.OrdinalIgnoreCase)
            {
                { IngredientShop.buildingName, IngredientShop },
                { BobaShop.buildingName, BobaShop },
                { PlayerHouse.buildingName, PlayerHouse },
                { BreakfastShop.buildingName, BreakfastShop },
                { ShadeTree.buildingName, ShadeTree }
            };

        public static bool TryGetPlacement(string buildingName, out BuildingPlacement placement)
        {
            return AllPlacements.TryGetValue(buildingName, out placement);
        }

        public static void ApplyPlacement(Transform targetTransform, string buildingName)
        {
            if (targetTransform == null) return;
            if (TryGetPlacement(buildingName, out var placement))
            {
                targetTransform.position = placement.defaultPosition;
                targetTransform.localScale = placement.defaultScale;
            }
        }
    }
}
