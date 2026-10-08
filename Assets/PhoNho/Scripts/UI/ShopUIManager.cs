using System;
using System.Collections.Generic;
using System.Text;
using PhoNho.Character;
using PhoNho.Domain.Cooking;
using PhoNho.Domain.Economy;
using PhoNho.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace PhoNho.UI
{
    /// <summary>
    /// Manages the in-game HUD (currency bar, interaction prompt) and shop dialogs
    /// for Boba Shop, Breakfast Shop, Ingredient Market, and Player House.
    /// Handles procedural UI creation, responsive updates, and user interactions.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopUIManager : MonoBehaviour
    {
        [Header("Domain Dependencies")]
        [SerializeField] private PlayerWallet _wallet;
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private CookingService _cookingService;
        [SerializeField] private PhoNhoPlayerMovement _playerMovement;

        [Header("UI State (Read Only)")]
        [SerializeField] private bool _isDialogOpen;
        [SerializeField] private ShopType _activeShopType;

        // UI references
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;

        private Text _scoinText;
        private Text _gemText;
        private Text _tcoinText;

        private GameObject _promptPanel;
        private Text _promptLabel;
        private Button _promptButton;

        private GameObject _dialogModal;
        private Text _dialogTitleText;
        private Text _dialogSubtitleText;
        private Text _dialogBodyText;
        private Text _dialogStatusText;
        private Button _closeButton;

        private GameObject _ingredientContentPanel;
        private GameObject _recipeContentPanel;
        private GameObject _houseContentPanel;

        private Slider _cookingProgressBar;
        private Text _cookingProgressLabel;

        private ShopInteractionTrigger _currentProximityShop;

        public bool IsDialogOpen => _isDialogOpen;
        public ShopType ActiveShopType => _activeShopType;

        public void Configure(PlayerWallet wallet, PlayerInventory inventory, CookingService cooking, PhoNhoPlayerMovement movement)
        {
            _wallet = wallet;
            _inventory = inventory;
            _cookingService = cooking;
            _playerMovement = movement;

            if (_wallet != null) _wallet.OnBalanceChanged += HandleBalanceChanged;
            if (_cookingService != null)
            {
                _cookingService.OnCookingStarted += HandleCookingStarted;
                _cookingService.OnCookingProgress += HandleCookingProgress;
                _cookingService.OnCookingCompleted += HandleCookingCompleted;
            }

            EnsureUIBuilt();
            UpdateCurrencyDisplay();
        }

        public void EnsureUIBuilt()
        {
            FindDependencies();
            if (_canvas == null || _dialogModal == null)
            {
                var existingCanvas = transform.Find("PhoNho_UI_Canvas");
                if (existingCanvas != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(existingCanvas.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(existingCanvas.gameObject);
                    }
                }
                BuildUIHierarchy();
            }
        }

        private void Awake()
        {
            EnsureUIBuilt();
        }

        private void OnEnable()
        {
            ShopInteractionTrigger.OnShopProximityChanged += HandleShopProximityChanged;
            ShopInteractionTrigger.OnShopInteracted += HandleShopInteracted;

            if (_wallet != null) _wallet.OnBalanceChanged += HandleBalanceChanged;
            if (_cookingService != null)
            {
                _cookingService.OnCookingStarted += HandleCookingStarted;
                _cookingService.OnCookingProgress += HandleCookingProgress;
                _cookingService.OnCookingCompleted += HandleCookingCompleted;
            }

            UpdateCurrencyDisplay();
        }

        private void OnDisable()
        {
            ShopInteractionTrigger.OnShopProximityChanged -= HandleShopProximityChanged;
            ShopInteractionTrigger.OnShopInteracted -= HandleShopInteracted;

            if (_wallet != null) _wallet.OnBalanceChanged -= HandleBalanceChanged;
            if (_cookingService != null)
            {
                _cookingService.OnCookingStarted -= HandleCookingStarted;
                _cookingService.OnCookingProgress -= HandleCookingProgress;
                _cookingService.OnCookingCompleted -= HandleCookingCompleted;
            }
        }

        private void FindDependencies()
        {
            if (_wallet == null) _wallet = FindFirstObjectByType<PlayerWallet>();
            if (_inventory == null) _inventory = FindFirstObjectByType<PlayerInventory>();
            if (_cookingService == null) _cookingService = FindFirstObjectByType<CookingService>();
            if (_playerMovement == null) _playerMovement = FindFirstObjectByType<PhoNhoPlayerMovement>();
        }

        private void Update()
        {
            // Close dialog with Escape key
            if (_isDialogOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShopDialog();
            }
        }

        #region Proximity & Interaction Handlers

        private void HandleShopProximityChanged(ShopInteractionTrigger trigger, bool isInside)
        {
            if (isInside)
            {
                _currentProximityShop = trigger;
                ShowPrompt(trigger.PromptText);
            }
            else
            {
                if (_currentProximityShop == trigger)
                {
                    _currentProximityShop = null;
                    HidePrompt();
                }
            }
        }

        private void HandleShopInteracted(ShopInteractionTrigger trigger)
        {
            OpenShopDialog(trigger.ShopType, trigger.ShopDisplayName);
        }

        public void ShowPrompt(string message)
        {
            EnsureUIBuilt();
            if (_promptPanel != null)
            {
                _promptPanel.SetActive(true);
                if (_promptLabel != null) _promptLabel.text = message;
            }
        }

        public void HidePrompt()
        {
            if (_promptPanel != null)
            {
                _promptPanel.SetActive(false);
            }
        }

        #endregion

        #region Shop Dialog Control

        public void OpenShopDialog(ShopType shopType, string shopName)
        {
            EnsureUIBuilt();
            _activeShopType = shopType;
            _isDialogOpen = true;

            // Pause player movement
            if (_playerMovement != null)
            {
                _playerMovement.SetUseInputManager(false);
                _playerMovement.SetMoveInput(0f);
            }

            if (_dialogModal != null)
            {
                _dialogModal.SetActive(true);
            }

            if (_dialogTitleText != null)
            {
                _dialogTitleText.text = shopName.ToUpper();
            }

            SetStatusMessage($"Chào mừng bạn đến với {shopName}!");
            RefreshDialogContent();
        }

        public void CloseShopDialog()
        {
            _isDialogOpen = false;

            if (_dialogModal != null)
            {
                _dialogModal.SetActive(false);
            }

            // Resume player movement
            if (_playerMovement != null)
            {
                _playerMovement.SetUseInputManager(true);
            }
        }

        public void RefreshDialogContent()
        {
            if (!_isDialogOpen) return;

            UpdateCurrencyDisplay();

            switch (_activeShopType)
            {
                case ShopType.IngredientShop:
                    ShowIngredientMarket();
                    break;
                case ShopType.BobaShop:
                    ShowRecipeShop("Boba", "TIỆM TRÀ SỮA HỒNG — PHA CHẾ TRÀ SỮA");
                    break;
                case ShopType.BreakfastShop:
                    ShowRecipeShop("Breakfast", "QUÁN ĂN SÁNG MẬT ONG — NẤU ĐỒ ĂN SÁNG");
                    break;
                case ShopType.PlayerHouse:
                    ShowPlayerHouse();
                    break;
            }
        }

        private void ShowIngredientMarket()
        {
            if (_dialogSubtitleText != null)
            {
                _dialogSubtitleText.text = "Nhập sỉ nguyên liệu phục vụ kinh doanh quán trà sữa & đồ ăn sáng";
            }

            var sb = new StringBuilder();
            sb.AppendLine("DANH SÁCH NGUYÊN LIỆU SỈ:");
            sb.AppendLine("------------------------------------------------------------");

            foreach (var ing in CookingService.StandardIngredients)
            {
                int inBag = _inventory != null ? _inventory.GetIngredientCount(ing.id) : 0;
                sb.AppendLine($"• [{ing.name}] — Giá sỉ: {ing.basePrice} Scoin | Có trong túi: {inBag} cái");
            }

            if (_dialogBodyText != null)
            {
                _dialogBodyText.text = sb.ToString();
            }
        }

        private void ShowRecipeShop(string profession, string subtitle)
        {
            if (_dialogSubtitleText != null)
            {
                _dialogSubtitleText.text = subtitle;
            }

            var sb = new StringBuilder();
            sb.AppendLine("THỰC ĐƠN & CÔNG THỨC CHẾ BIẾN:");
            sb.AppendLine("------------------------------------------------------------");

            foreach (var recipe in CookingService.StandardRecipes)
            {
                if (recipe.profession != profession) continue;

                bool canCraft = _inventory != null && _inventory.HasIngredients(recipe);
                string status = canCraft ? "<color=#2E7D32>[ĐỦ NGUYÊN LIỆU]</color>" : "<color=#D32F2F>[THIẾU NGUYÊN LIỆU]</color>";

                sb.AppendLine($"• {recipe.name} (Doanh thu: +{recipe.rewardScoin} Scoin, Nấu: {recipe.cookingDuration:F1}s) — {status}");

                sb.Append("   Yêu cầu: ");
                for (int i = 0; i < recipe.requirements.Count; i++)
                {
                    var req = recipe.requirements[i];
                    var ing = GetIngredientById(req.ingredientId);
                    string ingName = ing != null ? ing.name : req.ingredientId;
                    int have = _inventory != null ? _inventory.GetIngredientCount(req.ingredientId) : 0;
                    sb.Append($"{ingName} ({have}/{req.quantity})");
                    if (i < recipe.requirements.Count - 1) sb.Append(", ");
                }
                sb.AppendLine();
            }

            if (_dialogBodyText != null)
            {
                _dialogBodyText.text = sb.ToString();
            }
        }

        private void ShowPlayerHouse()
        {
            if (_dialogSubtitleText != null)
            {
                _dialogSubtitleText.text = "Không gian nghỉ ngơi ấm áp, xem tủ đồ và kho nguyên liệu";
            }

            var sb = new StringBuilder();
            sb.AppendLine("HỒ SƠ GIA CHỦ & TỦ ĐỒ PHỐ NHỎ:");
            sb.AppendLine("------------------------------------------------------------");
            sb.AppendLine($"• Chủ nhà: Cư Dân Phố Nhỏ");
            sb.AppendLine($"• Vốn Scoin: {_wallet?.Scoin ?? 0} Scoin");
            sb.AppendLine($"• Đá quý Gem: {_wallet?.Gem ?? 0} Gem");
            sb.AppendLine($"• Điểm Bá Khí: 100 điểm (Trang phục Chibi cơ bản)");
            sb.AppendLine();
            sb.AppendLine("KHO NGUYÊN LIỆU:");
            if (_inventory != null && _inventory.Ingredients.Count > 0)
            {
                foreach (var kvp in _inventory.Ingredients)
                {
                    var ing = GetIngredientById(kvp.Key);
                    string name = ing != null ? ing.name : kvp.Key;
                    sb.AppendLine($"  - {name}: {kvp.Value} đơn vị");
                }
            }
            else
            {
                sb.AppendLine("  (Chưa có nguyên liệu nào. Hãy ghé Chợ Nguyên Liệu để mua sỉ!)");
            }

            sb.AppendLine();
            sb.AppendLine("MÓN ĐÃ CHẾ BIẾN HOÀN TẤT:");
            if (_inventory != null && _inventory.Products.Count > 0)
            {
                foreach (var kvp in _inventory.Products)
                {
                    var rec = GetRecipeById(kvp.Key);
                    string name = rec != null ? rec.name : kvp.Key;
                    sb.AppendLine($"  - {name}: {kvp.Value} phần");
                }
            }
            else
            {
                sb.AppendLine("  (Chưa có món nào. Hãy ghé Tiệm Trà Sữa hoặc Quán Ăn Sáng để nấu!)");
            }

            if (_dialogBodyText != null)
            {
                _dialogBodyText.text = sb.ToString();
            }
        }

        #endregion

        #region Business Logic Actions (Purchasing & Cooking)

        public bool BuyIngredient(string ingredientId, int quantity = 1)
        {
            var ing = GetIngredientById(ingredientId);
            if (ing == null)
            {
                SetStatusMessage("Nguyên liệu không tồn tại!");
                return false;
            }

            int totalCost = ing.basePrice * quantity;
            if (_wallet == null || !_wallet.CanAfford(CurrencyType.Scoin, totalCost))
            {
                SetStatusMessage($"Không đủ Scoin để mua {quantity} {ing.name}! (Cần: {totalCost} Scoin)");
                return false;
            }

            string idempotencyKey = $"buy_{ingredientId}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid():N}";
            if (_wallet.ApplyTransaction(CurrencyType.Scoin, -totalCost, $"Mua sỉ {quantity}x {ing.name}", idempotencyKey, out string error))
            {
                if (_inventory != null)
                {
                    _inventory.AddIngredient(ingredientId, quantity);
                }
                SetStatusMessage($"Đã mua thành công {quantity}x {ing.name}! (-{totalCost} Scoin)");
                RefreshDialogContent();
                return true;
            }
            else
            {
                SetStatusMessage($"Giao dịch thất bại: {error}");
                return false;
            }
        }

        public bool StartCookRecipe(string recipeId)
        {
            var recipe = GetRecipeById(recipeId);
            if (recipe == null)
            {
                SetStatusMessage("Công thức không hợp lệ!");
                return false;
            }

            if (_cookingService == null)
            {
                SetStatusMessage("Hệ thống bếp chưa sẵn sàng!");
                return false;
            }

            if (_cookingService.TryStartCooking(recipe, out string error))
            {
                SetStatusMessage($"Bắt đầu chế biến {recipe.name}...");
                RefreshDialogContent();
                return true;
            }
            else
            {
                SetStatusMessage(error);
                return false;
            }
        }

        public void FastForwardCurrentCook()
        {
            if (_cookingService != null && _cookingService.IsCooking)
            {
                _cookingService.FastForwardCooking();
            }
        }

        #endregion

        #region Event Callbacks

        private void HandleBalanceChanged(CurrencyType currency, int newBalance, int delta)
        {
            UpdateCurrencyDisplay();
        }

        private void HandleCookingStarted(RecipeItem recipe)
        {
            SetStatusMessage($"Đang chế biến {recipe.name}...");
            if (_cookingProgressBar != null) _cookingProgressBar.gameObject.SetActive(true);
            RefreshDialogContent();
        }

        private void HandleCookingProgress(float progress)
        {
            if (_cookingProgressBar != null)
            {
                _cookingProgressBar.value = progress;
            }
            if (_cookingProgressLabel != null)
            {
                _cookingProgressLabel.text = $"Tiến độ: {Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        private void HandleCookingCompleted(RecipeItem recipe, int scoinEarned)
        {
            SetStatusMessage($"Chế biến hoàn tất {recipe.name}! Thu về +{scoinEarned} Scoin!");
            if (_cookingProgressBar != null) _cookingProgressBar.gameObject.SetActive(false);
            RefreshDialogContent();
        }

        private void SetStatusMessage(string message)
        {
            if (_dialogStatusText != null)
            {
                _dialogStatusText.text = message;
            }
        }

        private void UpdateCurrencyDisplay()
        {
            if (_wallet != null)
            {
                if (_scoinText != null) _scoinText.text = $"{_wallet.Scoin:N0} Scoin";
                if (_gemText != null) _gemText.text = $"{_wallet.Gem:N0} Gem";
                if (_tcoinText != null) _tcoinText.text = $"{_wallet.Tcoin:N0} Tcoin";
            }
        }

        private IngredientItem GetIngredientById(string id)
        {
            foreach (var ing in CookingService.StandardIngredients)
            {
                if (string.Equals(ing.id, id, StringComparison.OrdinalIgnoreCase)) return ing;
            }
            return null;
        }

        private RecipeItem GetRecipeById(string id)
        {
            foreach (var r in CookingService.StandardRecipes)
            {
                if (string.Equals(r.id, id, StringComparison.OrdinalIgnoreCase)) return r;
            }
            return null;
        }

        #endregion

        #region Procedural UI Building

        public void BuildUIHierarchy()
        {
            // Create Canvas
            var canvasGo = new GameObject("PhoNho_UI_Canvas");
            canvasGo.transform.SetParent(transform, false);

            _canvas = canvasGo.AddComponent<Canvas>();
            Camera mainCam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (mainCam != null)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = mainCam;
                _canvas.planeDistance = 5f;
            }
            else
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            _canvas.sortingOrder = 500;

            _scaler = canvasGo.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.matchWidthOrHeight = 0.5f;

            Font defaultFont = null;
            try
            {
                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                // Fallback for older Unity versions if needed
                try { defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }

            // 1. Top HUD Bar
            BuildTopHud(canvasGo.transform, defaultFont);

            // 2. Interaction Prompt Box
            BuildPromptBox(canvasGo.transform, defaultFont);

            // 3. Shop Modal Dialog
            BuildShopDialog(canvasGo.transform, defaultFont);
        }

        private void BuildTopHud(Transform parent, Font font)
        {
            var hudGo = new GameObject("TopHUD_Bar");
            hudGo.transform.SetParent(parent, false);

            var rt = hudGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 70f);
            rt.anchoredPosition = Vector2.zero;

            var bg = hudGo.AddComponent<Image>();
            bg.color = new Color(0.98f, 0.96f, 0.93f, 0.92f); // Pastel cream

            // Title
            var titleGo = new GameObject("Town_Title");
            titleGo.transform.SetParent(hudGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0f);
            titleRt.anchorMax = new Vector2(0.3f, 1f);
            titleRt.anchoredPosition = new Vector2(25f, 0f);
            var titleText = titleGo.AddComponent<Text>();
            titleText.font = font;
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(0.35f, 0.25f, 0.2f, 1f); // Warm coffee brown
            titleText.text = "PHỐ NHỎ — V1.0";
            titleText.alignment = TextAnchor.MiddleLeft;

            // Scoin Box
            _scoinText = CreateCurrencyBadge(hudGo.transform, font, new Vector2(0.60f, 0.5f), "200 Scoin", new Color(0.95f, 0.75f, 0.1f, 1f));
            // Gem Box
            _gemText = CreateCurrencyBadge(hudGo.transform, font, new Vector2(0.75f, 0.5f), "10 Gem", new Color(0.85f, 0.4f, 0.8f, 1f));
            // Tcoin Box
            _tcoinText = CreateCurrencyBadge(hudGo.transform, font, new Vector2(0.90f, 0.5f), "0 Tcoin", new Color(0.2f, 0.75f, 0.7f, 1f));
        }

        private Text CreateCurrencyBadge(Transform parent, Font font, Vector2 anchor, string initialText, Color iconColor)
        {
            var badgeGo = new GameObject("Badge_" + initialText);
            badgeGo.transform.SetParent(parent, false);

            var rt = badgeGo.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(170f, 44f);
            rt.anchoredPosition = Vector2.zero;

            var img = badgeGo.AddComponent<Image>();
            img.color = new Color(iconColor.r, iconColor.g, iconColor.b, 0.22f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(badgeGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var txt = textGo.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(0.25f, 0.2f, 0.15f, 1f);
            txt.text = initialText;
            txt.alignment = TextAnchor.MiddleCenter;

            return txt;
        }

        private void BuildPromptBox(Transform parent, Font font)
        {
            _promptPanel = new GameObject("Interaction_Prompt_Box");
            _promptPanel.transform.SetParent(parent, false);

            var rt = _promptPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.22f);
            rt.anchorMax = new Vector2(0.5f, 0.22f);
            rt.sizeDelta = new Vector2(460f, 75f);
            rt.anchoredPosition = Vector2.zero;

            var img = _promptPanel.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.22f, 0.88f); // Soft dark slate

            var labelGo = new GameObject("Prompt_Label");
            labelGo.transform.SetParent(_promptPanel.transform, false);
            var lRt = labelGo.AddComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.05f, 0f);
            lRt.anchorMax = new Vector2(0.75f, 1f);
            lRt.sizeDelta = Vector2.zero;

            _promptLabel = labelGo.AddComponent<Text>();
            _promptLabel.font = font;
            _promptLabel.fontSize = 22;
            _promptLabel.color = Color.white;
            _promptLabel.alignment = TextAnchor.MiddleLeft;
            _promptLabel.text = "Nhấn [E] để vào quán";

            var btnGo = new GameObject("Prompt_Button");
            btnGo.transform.SetParent(_promptPanel.transform, false);
            var bRt = btnGo.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.76f, 0.12f);
            bRt.anchorMax = new Vector2(0.96f, 0.88f);
            bRt.sizeDelta = Vector2.zero;

            var bImg = btnGo.AddComponent<Image>();
            bImg.color = new Color(0.98f, 0.72f, 0.45f, 1f); // Warm peach

            _promptButton = btnGo.AddComponent<Button>();
            _promptButton.onClick.AddListener(() =>
            {
                if (_currentProximityShop != null)
                {
                    _currentProximityShop.TriggerInteraction();
                }
            });

            var btnTxtGo = new GameObject("BtnText");
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btRt = btnTxtGo.AddComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.sizeDelta = Vector2.zero;

            var bt = btnTxtGo.AddComponent<Text>();
            bt.font = font;
            bt.fontSize = 18;
            bt.fontStyle = FontStyle.Bold;
            bt.color = Color.white;
            bt.alignment = TextAnchor.MiddleCenter;
            bt.text = "VÀO";

            _promptPanel.SetActive(false);
        }

        private void BuildShopDialog(Transform parent, Font font)
        {
            _dialogModal = new GameObject("Shop_Dialog_Modal");
            _dialogModal.transform.SetParent(parent, false);

            var modalRt = _dialogModal.AddComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.sizeDelta = Vector2.zero;

            // Semi-transparent backdrop
            var backdrop = _dialogModal.AddComponent<Image>();
            backdrop.color = new Color(0.1f, 0.1f, 0.15f, 0.55f);

            // Dialog Panel
            var panelGo = new GameObject("Dialog_Panel");
            panelGo.transform.SetParent(_dialogModal.transform, false);
            var panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(960f, 620f);
            panelRt.anchoredPosition = Vector2.zero;

            var pBg = panelGo.AddComponent<Image>();
            pBg.color = new Color(0.99f, 0.98f, 0.96f, 1f); // Cream pastel

            // Header Banner
            var headerGo = new GameObject("Header_Banner");
            headerGo.transform.SetParent(panelGo.transform, false);
            var hRt = headerGo.AddComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(0f, 90f);
            hRt.anchoredPosition = Vector2.zero;

            var hBg = headerGo.AddComponent<Image>();
            hBg.color = new Color(0.96f, 0.88f, 0.82f, 1f); // Pastel peach

            // Title
            var titleGo = new GameObject("Shop_Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var tRt = titleGo.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.04f, 0.45f);
            tRt.anchorMax = new Vector2(0.85f, 0.95f);
            tRt.sizeDelta = Vector2.zero;

            _dialogTitleText = titleGo.AddComponent<Text>();
            _dialogTitleText.font = font;
            _dialogTitleText.fontSize = 28;
            _dialogTitleText.fontStyle = FontStyle.Bold;
            _dialogTitleText.color = new Color(0.35f, 0.22f, 0.15f, 1f);
            _dialogTitleText.text = "CỬA HÀNG PHỐ NHỎ";
            _dialogTitleText.alignment = TextAnchor.MiddleLeft;

            // Subtitle
            var subGo = new GameObject("Shop_Subtitle");
            subGo.transform.SetParent(headerGo.transform, false);
            var sRt = subGo.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.04f, 0.05f);
            sRt.anchorMax = new Vector2(0.85f, 0.45f);
            sRt.sizeDelta = Vector2.zero;

            _dialogSubtitleText = subGo.AddComponent<Text>();
            _dialogSubtitleText.font = font;
            _dialogSubtitleText.fontSize = 17;
            _dialogSubtitleText.color = new Color(0.5f, 0.4f, 0.35f, 1f);
            _dialogSubtitleText.text = "Quản lý kinh doanh và chế biến";
            _dialogSubtitleText.alignment = TextAnchor.MiddleLeft;

            // Close Button
            var closeGo = new GameObject("Close_Button");
            closeGo.transform.SetParent(headerGo.transform, false);
            var cRt = closeGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.91f, 0.2f);
            cRt.anchorMax = new Vector2(0.97f, 0.8f);
            cRt.sizeDelta = Vector2.zero;

            var cImg = closeGo.AddComponent<Image>();
            cImg.color = new Color(0.85f, 0.35f, 0.35f, 1f); // Soft red

            _closeButton = closeGo.AddComponent<Button>();
            _closeButton.onClick.AddListener(CloseShopDialog);

            var cTxtGo = new GameObject("X");
            cTxtGo.transform.SetParent(closeGo.transform, false);
            var cxRt = cTxtGo.AddComponent<RectTransform>();
            cxRt.anchorMin = Vector2.zero;
            cxRt.anchorMax = Vector2.one;
            cxRt.sizeDelta = Vector2.zero;
            var cxTxt = cTxtGo.AddComponent<Text>();
            cxTxt.font = font;
            cxTxt.fontSize = 24;
            cxTxt.fontStyle = FontStyle.Bold;
            cxTxt.color = Color.white;
            cxTxt.alignment = TextAnchor.MiddleCenter;
            cxTxt.text = "X";

            // Body Area
            var bodyGo = new GameObject("Body_Area");
            bodyGo.transform.SetParent(panelGo.transform, false);
            var bRt = bodyGo.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.04f, 0.16f);
            bRt.anchorMax = new Vector2(0.96f, 0.83f);
            bRt.sizeDelta = Vector2.zero;

            _dialogBodyText = bodyGo.AddComponent<Text>();
            _dialogBodyText.font = font;
            _dialogBodyText.fontSize = 19;
            _dialogBodyText.lineSpacing = 1.3f;
            _dialogBodyText.color = new Color(0.22f, 0.2f, 0.18f, 1f);
            _dialogBodyText.alignment = TextAnchor.UpperLeft;
            _dialogBodyText.text = "Nội dung cửa hàng...";

            // Cooking Progress Bar
            var barGo = new GameObject("Cooking_Progress_Bar");
            barGo.transform.SetParent(panelGo.transform, false);
            var barRt = barGo.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.04f, 0.10f);
            barRt.anchorMax = new Vector2(0.96f, 0.15f);
            barRt.sizeDelta = Vector2.zero;

            var barBg = barGo.AddComponent<Image>();
            barBg.color = new Color(0.88f, 0.86f, 0.82f, 1f);

            _cookingProgressBar = barGo.AddComponent<Slider>();
            _cookingProgressBar.minValue = 0f;
            _cookingProgressBar.maxValue = 1f;
            _cookingProgressBar.value = 0f;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.75f, 0.45f, 1f); // Fresh green
            _cookingProgressBar.fillRect = fillRt;

            barGo.SetActive(false);

            // Footer Status Bar
            var statusGo = new GameObject("Footer_Status_Bar");
            statusGo.transform.SetParent(panelGo.transform, false);
            var stRt = statusGo.AddComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.04f, 0.02f);
            stRt.anchorMax = new Vector2(0.96f, 0.09f);
            stRt.sizeDelta = Vector2.zero;

            _dialogStatusText = statusGo.AddComponent<Text>();
            _dialogStatusText.font = font;
            _dialogStatusText.fontSize = 17;
            _dialogStatusText.fontStyle = FontStyle.Italic;
            _dialogStatusText.color = new Color(0.45f, 0.35f, 0.25f, 1f);
            _dialogStatusText.alignment = TextAnchor.MiddleLeft;
            _dialogStatusText.text = "Sẵn sàng.";

            _dialogModal.SetActive(false);
        }

        #endregion
    }
}
