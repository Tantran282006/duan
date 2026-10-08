using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhoNho.Character;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using PhoNho.Gameplay.Interaction;
using PhoNho.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoNho.Gameplay.Editor
{
    public static class ShopInteractionCookingValidator
    {
        public const string ResultPath = "Assets/PhoNho/Editor/shop-cooking-validation-results.json";

        [Serializable]
        public class ValidationReport
        {
            public bool success;
            public string timestamp;
            public List<string> setupChecks = new List<string>();
            public List<string> walletChecks = new List<string>();
            public List<string> ingredientShopFlow = new List<string>();
            public List<string> bobaCookingFlow = new List<string>();
            public List<string> breakfastShopChecks = new List<string>();
            public List<string> playerHouseChecks = new List<string>();
            public List<string> movementResumeChecks = new List<string>();
            public List<string> errors = new List<string>();
        }

        [MenuItem("Phố Nhỏ/Gameplay/Kiểm tra tương tác Shop & Cooking Flow")]
        public static void ValidateAll()
        {
            var report = new ValidationReport
            {
                timestamp = DateTime.UtcNow.ToString("o")
            };

            try
            {
                // 1. Đảm bảo Scene Gameplay được build mới nhất
                Scene scene = EditorSceneManager.OpenScene(PhoNhoGameplaySceneBuilder.ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    report.errors.Add($"Không mở được scene {PhoNhoGameplaySceneBuilder.ScenePath}");
                }

                var gameSystemGo = GameObject.Find("PhoNho_GameSystem");
                var playerGo = GameObject.Find("Player_Character");

                if (gameSystemGo == null || playerGo == null)
                {
                    Debug.Log("[ShopInteractionCookingValidator] Thiếu GameSystem hoặc Player. Rebuilding scene...");
                    PhoNhoGameplaySceneBuilder.BuildAll();
                    scene = EditorSceneManager.OpenScene(PhoNhoGameplaySceneBuilder.ScenePath, OpenSceneMode.Single);
                    gameSystemGo = GameObject.Find("PhoNho_GameSystem");
                    playerGo = GameObject.Find("Player_Character");
                }

                if (gameSystemGo == null)
                {
                    report.errors.Add("Thiếu GameObject PhoNho_GameSystem trong Scene!");
                    return;
                }

                var wallet = gameSystemGo.GetComponent<PlayerWallet>();
                var inventory = gameSystemGo.GetComponent<PlayerInventory>();
                var cookingService = gameSystemGo.GetComponent<CookingService>();
                var uiManager = gameSystemGo.GetComponent<ShopUIManager>();
                var playerMovement = playerGo.GetComponent<PhoNhoPlayerMovement>();

                // Kiểm tra setup các components
                if (wallet == null) report.errors.Add("Thiếu PlayerWallet trên PhoNho_GameSystem.");
                if (inventory == null) report.errors.Add("Thiếu PlayerInventory trên PhoNho_GameSystem.");
                if (cookingService == null) report.errors.Add("Thiếu CookingService trên PhoNho_GameSystem.");
                if (uiManager == null) report.errors.Add("Thiếu ShopUIManager trên PhoNho_GameSystem.");
                if (playerMovement == null) report.errors.Add("Thiếu PhoNhoPlayerMovement trên Player_Character.");

                report.setupChecks.Add($"GameSystem Setup: wallet={wallet != null}, inventory={inventory != null}, cooking={cookingService != null}, uiManager={uiManager != null}");

                // Đảm bảo khởi tạo dependencies cho BatchMode / EditMode testing
                if (wallet != null)
                {
                    wallet.EnsureInitialized();
                }
                if (cookingService != null && wallet != null && inventory != null)
                {
                    cookingService.Configure(wallet, inventory);
                }
                if (uiManager != null)
                {
                    uiManager.Configure(wallet, inventory, cookingService, playerMovement);
                    uiManager.EnsureUIBuilt();
                }

                // Kiểm tra 4 Shop Triggers
                var triggers = UnityEngine.Object.FindObjectsByType<ShopInteractionTrigger>(FindObjectsSortMode.None);
                report.setupChecks.Add($"Shop Triggers found in scene: count={triggers.Length} (yêu cầu đúng 4 công trình)");

                if (triggers.Length != 4)
                {
                    report.errors.Add($"Số lượng ShopInteractionTrigger trong scene là {triggers.Length}, kỳ vọng đúng 4!");
                }

                var shopTypes = triggers.Select(t => t.ShopType).ToList();
                bool hasIngShop = shopTypes.Contains(ShopType.IngredientShop);
                bool hasBobaShop = shopTypes.Contains(ShopType.BobaShop);
                bool hasHouse = shopTypes.Contains(ShopType.PlayerHouse);
                bool hasBreakfast = shopTypes.Contains(ShopType.BreakfastShop);

                report.setupChecks.Add($"Triggers types: IngredientShop={hasIngShop}, BobaShop={hasBobaShop}, PlayerHouse={hasHouse}, BreakfastShop={hasBreakfast}");
                if (!hasIngShop || !hasBobaShop || !hasHouse || !hasBreakfast)
                {
                    report.errors.Add("Scene thiếu ít nhất 1 loại ShopInteractionTrigger cần thiết!");
                }

                // 2. Kiểm tra Wallet khởi đầu [CHỐT 200 Scoin theo GAME_PROMPT.md]
                int initialScoin = wallet.Scoin;
                int initialGem = wallet.Gem;
                int initialTcoin = wallet.Tcoin;
                report.walletChecks.Add($"Initial Wallet: Scoin={initialScoin} (kỳ vọng 200), Gem={initialGem} (kỳ vọng 10), Tcoin={initialTcoin} (kỳ vọng 0)");

                if (initialScoin != 200)
                {
                    report.errors.Add($"Vốn khởi đầu Scoin là {initialScoin}, không đúng thiết kế đã chốt là 200 Scoin!");
                }

                if (wallet.Ledger.Count == 0)
                {
                    report.errors.Add("Ví người chơi chưa có giao dịch sổ cái (Ledger rỗng)!");
                }
                else
                {
                    report.walletChecks.Add($"Ledger entries: count={wallet.Ledger.Count}, initialReason={wallet.Ledger[0].reason}");
                }

                // 3. Kiểm tra Flow tại Chợ Nguyên Liệu (Ingredient Shop)
                var ingTrigger = triggers.FirstOrDefault(t => t.ShopType == ShopType.IngredientShop);
                if (ingTrigger != null)
                {
                    // Di chuyển player đến trước cửa hàng
                    Vector3 ingPos = ingTrigger.transform.position;
                    playerGo.transform.position = new Vector3(ingPos.x, playerGo.transform.position.y, 0f);

                    // Force trigger check
                    ingTrigger.ForceSetPlayerInside(true);
                    report.ingredientShopFlow.Add($"Player at Ingredient Shop: X={playerGo.transform.position.x:F2}, IsInside={ingTrigger.IsPlayerInside}");

                    // Mở shop dialog
                    uiManager.OpenShopDialog(ingTrigger.ShopType, ingTrigger.ShopDisplayName);
                    report.ingredientShopFlow.Add($"Dialog opened: isOpen={uiManager.IsDialogOpen}, activeType={uiManager.ActiveShopType}");

                    if (!uiManager.IsDialogOpen || uiManager.ActiveShopType != ShopType.IngredientShop)
                    {
                        report.errors.Add("Không mở được giao diện Chợ Nguyên Liệu!");
                    }

                    // Mua nguyên liệu sỉ: 2 Trà (20 Scoin), 2 Sữa (24 Scoin), 3 Đường (15 Scoin), 1 Trân châu (8 Scoin)
                    // Tổng chi = 67 Scoin -> Số dư còn lại = 200 - 67 = 133 Scoin
                    bool bTea = uiManager.BuyIngredient("tea", 2);
                    bool bMilk = uiManager.BuyIngredient("milk", 2);
                    bool bSugar = uiManager.BuyIngredient("sugar", 3);
                    bool bPearl = uiManager.BuyIngredient("pearl", 1);

                    report.ingredientShopFlow.Add($"Purchases: Tea={bTea}, Milk={bMilk}, Sugar={bSugar}, Pearl={bPearl}");

                    int scoinAfterBuy = wallet.Scoin;
                    int expectedScoin = 200 - (2 * 10 + 2 * 12 + 3 * 5 + 1 * 8); // 133
                    report.ingredientShopFlow.Add($"Wallet after purchases: Scoin={scoinAfterBuy} (kỳ vọng {expectedScoin})");

                    if (scoinAfterBuy != expectedScoin)
                    {
                        report.errors.Add($"Số dư sau mua hàng ({scoinAfterBuy}) không khớp kỳ vọng ({expectedScoin})!");
                    }

                    // Kiểm tra tồn kho
                    int haveTea = inventory.GetIngredientCount("tea");
                    int haveMilk = inventory.GetIngredientCount("milk");
                    int haveSugar = inventory.GetIngredientCount("sugar");
                    int havePearl = inventory.GetIngredientCount("pearl");
                    report.ingredientShopFlow.Add($"Inventory: tea={haveTea}, milk={haveMilk}, sugar={haveSugar}, pearl={havePearl}");

                    if (haveTea != 2 || haveMilk != 2 || haveSugar != 3 || havePearl != 1)
                    {
                        report.errors.Add("Tồn kho nguyên liệu sau khi mua không khớp số lượng!");
                    }

                    // Kiểm tra mua vượt quá số dư (thử mua 100 Thịt bò = 2500 Scoin)
                    bool overBuy = uiManager.BuyIngredient("beef", 100);
                    report.ingredientShopFlow.Add($"Over-budget purchase rejected: success={!overBuy} (kỳ vọng false)");
                    if (overBuy)
                    {
                        report.errors.Add("Hệ thống cho phép mua hàng khi không đủ tiền!");
                    }

                    uiManager.CloseShopDialog();
                }

                // 4. Kiểm tra Cooking Flow tại Tiệm Trà Sữa (Boba Shop)
                var bobaTrigger = triggers.FirstOrDefault(t => t.ShopType == ShopType.BobaShop);
                if (bobaTrigger != null)
                {
                    Vector3 bobaPos = bobaTrigger.transform.position;
                    playerGo.transform.position = new Vector3(bobaPos.x, playerGo.transform.position.y, 0f);
                    bobaTrigger.ForceSetPlayerInside(true);

                    uiManager.OpenShopDialog(bobaTrigger.ShopType, bobaTrigger.ShopDisplayName);
                    report.bobaCookingFlow.Add($"Opened Boba Shop dialog: isOpen={uiManager.IsDialogOpen}");

                    // Nấu món 1: Trà Sữa Truyền Thống (cần 1 Trà + 1 Sữa + 1 Đường, doanh thu +45 Scoin)
                    bool startCook1 = uiManager.StartCookRecipe("recipe_boba_classic");
                    report.bobaCookingFlow.Add($"Start Cook Recipe 1 (Trà Sữa Truyền Thống): started={startCook1}, isCooking={cookingService.IsCooking}");

                    if (!startCook1 || !cookingService.IsCooking)
                    {
                        report.errors.Add("Không thể bắt đầu pha chế Trà Sữa Truyền Thống dù đã có đủ nguyên liệu!");
                    }

                    // Fast forward cooking
                    uiManager.FastForwardCurrentCook();
                    int scoinAfterCook1 = wallet.Scoin;
                    int expectedScoin1 = 133 + 45; // 178
                    int bobaProductCount1 = inventory.GetProductCount("recipe_boba_classic");
                    report.bobaCookingFlow.Add($"Cook 1 Finished: Scoin={scoinAfterCook1} (kỳ vọng {expectedScoin1}), ProductCount={bobaProductCount1}");

                    if (scoinAfterCook1 != expectedScoin1)
                    {
                        report.errors.Add($"Số dư Scoin sau khi nấu món 1 ({scoinAfterCook1}) không khớp kỳ vọng ({expectedScoin1})!");
                    }
                    if (bobaProductCount1 != 1)
                    {
                        report.errors.Add($"Số lượng thành phẩm Trà Sữa Truyền Thống ({bobaProductCount1}) không đúng!");
                    }

                    // Nấu món 2: Trà Sữa Trân Châu Đường Đen (cần 1 Trà + 1 Sữa + 2 Đường + 1 Trân Châu, doanh thu +65 Scoin)
                    bool startCook2 = uiManager.StartCookRecipe("recipe_boba_brown_sugar");
                    report.bobaCookingFlow.Add($"Start Cook Recipe 2 (Trà Sữa Trân Châu Đường Đen): started={startCook2}");

                    if (!startCook2)
                    {
                        report.errors.Add("Không thể bắt đầu pha chế Trà Sữa Trân Châu Đường Đen!");
                    }

                    uiManager.FastForwardCurrentCook();
                    int scoinAfterCook2 = wallet.Scoin;
                    int expectedScoin2 = 178 + 65; // 243
                    report.bobaCookingFlow.Add($"Cook 2 Finished: Scoin={scoinAfterCook2} (kỳ vọng {expectedScoin2}), ProductCount={inventory.GetProductCount("recipe_boba_brown_sugar")}");

                    if (scoinAfterCook2 != expectedScoin2)
                    {
                        report.errors.Add($"Số dư Scoin sau khi nấu món 2 ({scoinAfterCook2}) không khớp kỳ vọng ({expectedScoin2})!");
                    }

                    // Thử nấu tiếp khi đã hết nguyên liệu
                    bool cookWithoutIngredients = uiManager.StartCookRecipe("recipe_boba_classic");
                    report.bobaCookingFlow.Add($"Cooking without ingredients rejected: success={!cookWithoutIngredients} (kỳ vọng false)");

                    if (cookWithoutIngredients)
                    {
                        report.errors.Add("Hệ thống cho phép chế biến khi đã hết nguyên liệu!");
                    }

                    uiManager.CloseShopDialog();
                }

                // 5. Kiểm tra Quán Ăn Sáng (Breakfast Shop)
                var breakfastTrigger = triggers.FirstOrDefault(t => t.ShopType == ShopType.BreakfastShop);
                if (breakfastTrigger != null)
                {
                    Vector3 bfPos = breakfastTrigger.transform.position;
                    playerGo.transform.position = new Vector3(bfPos.x, playerGo.transform.position.y, 0f);
                    breakfastTrigger.ForceSetPlayerInside(true);

                    uiManager.OpenShopDialog(breakfastTrigger.ShopType, breakfastTrigger.ShopDisplayName);
                    report.breakfastShopChecks.Add($"Breakfast Shop opened: activeType={uiManager.ActiveShopType}");

                    if (!uiManager.IsDialogOpen || uiManager.ActiveShopType != ShopType.BreakfastShop)
                    {
                        report.errors.Add("Không mở được giao diện Quán Ăn Sáng!");
                    }

                    uiManager.CloseShopDialog();
                }

                // 6. Kiểm tra Nhà Người Chơi (Player House)
                var houseTrigger = triggers.FirstOrDefault(t => t.ShopType == ShopType.PlayerHouse);
                if (houseTrigger != null)
                {
                    Vector3 hPos = houseTrigger.transform.position;
                    playerGo.transform.position = new Vector3(hPos.x, playerGo.transform.position.y, 0f);
                    houseTrigger.ForceSetPlayerInside(true);

                    uiManager.OpenShopDialog(houseTrigger.ShopType, houseTrigger.ShopDisplayName);
                    report.playerHouseChecks.Add($"Player House opened: activeType={uiManager.ActiveShopType}");

                    if (!uiManager.IsDialogOpen || uiManager.ActiveShopType != ShopType.PlayerHouse)
                    {
                        report.errors.Add("Không mở được giao diện Nhà Phố!");
                    }

                    uiManager.CloseShopDialog();
                }

                // 7. Kiểm tra đóng UI và khôi phục di chuyển bình thường
                report.movementResumeChecks.Add($"Dialog closed: isOpen={uiManager.IsDialogOpen}");
                if (uiManager.IsDialogOpen)
                {
                    report.errors.Add("Dialog vẫn mở sau khi gọi CloseShopDialog!");
                }

                // Reset position of player back to safe center
                playerGo.transform.position = new Vector3(0f, -1.0f, 0f);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, PhoNhoGameplaySceneBuilder.ScenePath);

                report.success = report.errors.Count == 0;
            }
            catch (Exception ex)
            {
                report.errors.Add($"Exception: {ex.Message}\n{ex.StackTrace}");
                report.success = false;
            }
            finally
            {
                string json = JsonUtility.ToJson(report, true);
                File.WriteAllText(ResultPath, json);
                Debug.Log($"[ShopInteractionCookingValidator] Hoàn tất validation: success={report.success}, errors={report.errors.Count}. Đã lưu tại {ResultPath}");
            }

            if (!report.success)
            {
                throw new InvalidOperationException($"Shop interaction & cooking validation failed: {string.Join("; ", report.errors)}");
            }
        }

        public static void ValidateFromCommandLine()
        {
            ValidateAll();
        }
    }
}
