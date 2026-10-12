# PROJECT_STATE.md — Trạng thái & Bộ nhớ Dự án "Phố Nhỏ"

> **Dành cho AI Agent mới bắt đầu phiên**: Đọc file này hoặc gọi MCP tool `get_project_context` một lần để nắm toàn bộ bối cảnh dự án, kiến trúc, quyết định cốt lõi và các task đang active. Không cần quét toàn bộ task history cũ hay đọc lại cả file log.

---

## 1. Mục tiêu Dự án
- **Game**: "Phố Nhỏ" — Game mô phỏng kinh doanh & quản lý đường phố phong cách Chibi pastel ấm áp (cozy street life management) trên di động (Android / iOS).
- **Core Loop**: Mở quán (Trà sữa, Đồ ăn sáng, Nguyên vật liệu) trên con phố đi bộ cuộn ngang 2D, phục vụ khách NPC và người chơi thật, giao dịch nguyên liệu tự do, nhận đánh giá sau đơn hàng, đua top Bá Khí & Sao Michelin (5.0★).
- **Tương tác**: Bất đồng bộ qua mạng (asynchronous multiplayer), server-authoritative.

---

## 2. Kiến trúc & Công nghệ Hiện tại
- **Client (Unity)**:
  - Phiên bản: Unity 6000.4.3f1 (màn hình ngang Landscape 16:9).
  - 2D Gameplay: Cuộn cảnh Parallax ngang (`PhoNho.Map.ParallaxLayer`), physics 2D (`Rigidbody2D` Dynamic, `CapsuleCollider2D`, khóa FreezeRotation Z, `PhoNhoPlayerMovement`).
  - Điều khiển: Bàn phím A/D và mũi tên trái/phải (`Input.GetAxisRaw("Horizontal")`), camera bám mượt (`PhoNhoCameraFollow`).
  - Character & Animation: 4 sprite strip 1 hàng ngang duy nhất (`1x4`, `2048 x 640 px`, PPU 256, baseline 576, pivot `(0.5, 0.1)`), Animator Controllers `Male_A_Controller` & `Female_A_Controller`.
- **Backend & Tooling**:
  - Kế hoạch backend: Nakama Open-Source + PostgreSQL (Docker local dev, RPC, ledger ACID).
  - IDE Bridge & Auto-Worker: REST Server + MCP stdio (`tools/ide-bridge/`), Cloud bridge LIVE tại `https://game.zcloudviet.xyz` và `https://phonho.zcloudviet.xyz`. Auto-Worker daemon (`tools/ide-bridge/worker.mjs`) tự khởi động khi mở workspace qua `.vscode/tasks.json`, tự động thăm dò, claim và dispatch task cho Antigravity Agent qua `agentapi`.
  - Dashboard quản lý: Web UI local `http://127.0.0.1:5050` quản lý tiến độ, kế hoạch, done log, và profiles Antigravity IDE.
  - Project Memory & Archive: Handoff cô đọng qua `PROJECT_STATE.md`, tự động archive task done vào `.tasks/runtime/archive/`.

---

## 3. Quyết định Kỹ thuật Cốt lõi [CHỐT]
- **D1/D2**: Mobile 2D cuộn cảnh ngang, tương tác người chơi bất đồng bộ.
- **Server-authoritative**: Mọi tính toán tiền, sao, đánh giá, xếp hạng đều do server kiểm soát.
- **Ledger bất biến**: Scoin, Gem, Tcoin cập nhật qua transaction có `idempotency_key`. Cấm cộng/trừ số dư trực tiếp.
- **Tcoin thẩm mỹ**: Tcoin nạp bằng tiền thật chỉ dùng cho thẩm mỹ, tuyệt đối không mua được sao hay lợi thế kinh tế.
- **Đánh giá chặt chẽ**: Mỗi đánh giá gắn liền với một đơn hàng hoàn tất; tối đa 1 đánh giá/đơn, giới hạn theo ngày.
- **Định danh thành phố**: Mọi bảng dữ liệu có `city_id` sẵn sàng mở rộng nhiều thành phố.
- **Tọa độ cố định từng nhà [CHỐT]**: Mỗi công trình trên phố có tọa độ $(X, Y, Z)$ và scale riêng biệt mặc định được lưu trữ trong `BuildingDefaultCoordinates.cs` và bảo vệ bằng `PhoNhoBuildingAnchor`:
  - `Ingredient_Shop`: Position `(-7.05, 0.8, 0)`, Scale `0.3781139`
  - `Boba_Shop`: Position `(-2.4, 0.7, 0)`, Scale `0.3583474`
  - `Player_House`: Position `(2.15, 1.4, 0)`, Scale `0.3774238`
  - `Breakfast_Shop`: Position `(6.75, 0.8, 0)`, Scale `0.3741197`
  Sau khi build nhà hoặc tái tạo scene bằng `PhoNhoGameplaySceneBuilder`, toàn bộ nhà luôn cố định tuyệt đối theo các tọa độ này.

---

## 4. File & Module Quan trọng
| File / Thư mục | Mục đích |
|---|---|
| `Assets/PhoNho/Scripts/Domain/Economy/PlayerWallet.cs` | Quản lý ví tiền tệ (Scoin, Gem, Tcoin), vốn ban đầu [CHỐT] 200 Scoin, giao dịch qua Ledger bất biến có idempotency key |
| `Assets/PhoNho/Scripts/Domain/Cooking/CookingService.cs` | Dịch vụ chế biến đồ uống & đồ ăn sáng: kiểm tra nguyên liệu, tiến độ nấu, cộng doanh thu vào ví |
| `Assets/PhoNho/Scripts/Domain/Cooking/PlayerInventory.cs` | Quản lý tồn kho nguyên liệu sỉ (trà, sữa, đường, trân châu, bột mì, trứng, thịt bò) và thành phẩm |
| `Assets/PhoNho/Scripts/Gameplay/Interaction/ShopInteractionTrigger.cs` | Trigger proximity tương tác công trình trên phố (Chợ nguyên liệu, Trà sữa, Ăn sáng, Nhà phố), hỗ trợ phím E và chạm |
| `Assets/PhoNho/Scripts/UI/ShopUIManager.cs` | Giao diện quản lý shop procedurally built: TopBar HUD, Prompt box, Dialog modal cho từng shop, tiến độ nấu, tạm dừng/khôi phục di chuyển |
| `Assets/PhoNho/Editor/ShopInteractionCookingValidator.cs` | Validator tự động kiểm tra toàn bộ luồng tương tác shop, mua sỉ, nấu ăn, nhận doanh thu, idempotency |
| `Assets/PhoNho/Scripts/Character/PhoNhoPlayerMovement.cs` | Controller vật lý 2D di chuyển nhân vật trái/phải, bảo toàn gravity Y, tự động lật flipX |
| `Assets/PhoNho/Scripts/Map/PhoNhoCameraFollow.cs` | Camera follow bám theo nhân vật mượt mà, hỗ trợ bật/tắt clamp biên để đi vô tận |
| `Assets/PhoNho/Scripts/Map/PhoNhoInfiniteLayer.cs` | Cuộn lặp vô tận O(1) theo vị trí camera cho background (Sky, Distant Town) và ground (Sidewalk, Road) |
| `Assets/PhoNho/Scripts/Map/PhoNhoInfiniteGroundCollider.cs` | Tự động recenter collider mặt đất bám theo Player, đảm bảo chân không bao giờ tụt đất |
| `Assets/PhoNho/Scripts/Map/ParallaxLayer.cs` | Hiệu ứng thị sai nhiều lớp cho bầu trời, chân trời, tán cây |
| `Assets/PhoNho/Prefabs/Player_Character.prefab` | Prefab nhân vật đầy đủ Rigidbody2D, CapsuleCollider2D, Animator, SpriteRenderer |
| `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity` | Scene phố gameplay có nền vô tận, collider mặt đất bám theo player, không bị chặn biên, 4 shop triggers |
| `Assets/PhoNho/Scenes/ShopInterior_HoneyBreakfast.unity` | Scene nội thất quán ăn sáng Chibi Việt Nam: tường vàng Hội An, sàn gạch bông, bàn ghế gỗ, quầy bếp chuyển sang cooking, cửa thoát ra phố |
| `Assets/PhoNho/Scenes/Cooking_HoneyBreakfast.unity` | Scene nấu ăn cận cảnh: mặt bàn bếp đá hoa cương, bếp ga lửa xanh, chảo chống dính, khay nguyên liệu, đĩa thành phẩm Bánh Mì Ốp La |
| `Assets/PhoNho/Scripts/Gameplay/CookingMiniGameController.cs` | Controller mini-game nấu ăn cận cảnh: 4 bước nấu bằng phím [1]-[4], thưởng +35 Scoin vào ledger, nút Hoàn thành [Space] / [ESC] |
| `Assets/PhoNho/Scripts/Domain/PhoNhoGameState.cs` | Quản lý trạng thái toàn cục bền vững liên Scene (Scoin, Gem, Tcoin, Ledger, Inventory) không dùng singleton rườm rà |
| `Assets/PhoNho/Scripts/Gameplay/SceneTransitionManager.cs` | Quản lý chuyển Scene thật 2 chiều 2 cấp (Phố <-> Quán <-> Nấu ăn), debounce chống lặp phím E, lưu tọa độ return spawn |
| `Assets/PhoNho/Scripts/Gameplay/StreetSpawnController.cs` | Tự động đặt Player đúng vị trí trước cửa quán (X=6.75f, Y=-1.80f) và snap camera khi từ nội thất trở ra phố |
| `Assets/PhoNho/Scripts/Gameplay/InteriorSpawnController.cs` | Tự động đặt Player trước quầy bếp (X=-3.20f, Y=-1.00f) khi từ scene nấu ăn trở lại quán |
| `Assets/PhoNho/Scripts/Gameplay/Interaction/ShopExitTrigger.cs` | Trigger cửa thoát trong nội thất quán, nhấn phím [E] để chuyển về scene phố |
| `Assets/PhoNho/Editor/HoneyBreakfastTextureGenerator.cs` | Bộ sinh 14 texture đồ họa Chibi/pixel art phong cách quán ăn Việt Nam và bếp nấu cận cảnh |
| `Assets/PhoNho/Editor/HoneyBreakfastSceneBuilder.cs` | Bộ dựng tự động Scene nội thất ShopInterior_HoneyBreakfast và đăng ký EditorBuildSettings |
| `Assets/PhoNho/Editor/CookingSceneBuilder.cs` | Bộ dựng tự động Scene Cooking_HoneyBreakfast và đăng ký EditorBuildSettings |
| `Assets/PhoNho/Editor/PhoNhoThreeScenesFlowValidator.cs` | Validator Play Mode kiểm thử tự động 3 chu trình lặp: Phố <-> Quán <-> Nấu ăn <-> Quán <-> Phố, kiểm tra 0 duplicate camera/manager |
| `Assets/PhoNho/Scripts/Map/BuildingDefaultCoordinates.cs` | Bảng cấu hình tọa độ (X, Y, Z) và scale mặc định cố định chuẩn của từng nhà theo thiết kế [CHỐT] |
| `Assets/PhoNho/Scripts/Map/PhoNhoBuildingAnchor.cs` | Component khóa và khôi phục nhanh tọa độ mặc định của từng nhà trong Unity Editor và Runtime |
| `Assets/PhoNho/Scenes/Character_Preview.unity` | Scene preview nhân vật nam/nữ với các sprite strip 1 hàng |
| `tools/ide-bridge/` | Bộ cầu nối ChatGPT Custom GPT ↔ Local Task Store ↔ Antigravity MCP |
| `.agents/skills/game-development/` | Skill native Antigravity IDE chuyên sâu Unity 2D, patterns, Project Memory |
| `docs/snapshots/` | Lưu trữ snapshot tiến độ dự án có timestamp, lịch sử append-only không bị xóa/ghi đè |
| `PROGRESS.md` | Nhật ký tiến độ chi tiết từng phiên |
| `PROJECT_STATE.md` | Bộ nhớ cô đọng của dự án (file này) |

---

## 5. Những Phần Đã Hoàn Thành Gần Đây
- **[pho-nho-unity-three-scenes-rebuild-20261012-001] Xây dựng lại 3 scene Phố Nhỏ: CityOverworld, ShopInterior_HoneyBreakfast và Cooking_HoneyBreakfast**: Đã xây dựng lại 3 Scene Unity hoàn chỉnh (CityOverworld_PlayerMovement, ShopInterior_HoneyBreakfast, Cooking_HoneyBreakfast) với đồ họa 2D pixel art/Chibi ấm cúng Hội An & Đông Dương (14 texture procedural mới); chuyển scene thật 2 chiều 2 cấp bằng SceneManager.LoadSceneAsync; mini-game nấu ăn 4 bước "Bánh Mì Ốp La Giòn Rụm" (+35 Scoin/món vào ledger); chạy validator Play Mode tự động 3 chu trình lặp đạt 100% (3/3 pass, 0 duplicate camera/manager, ví tăng từ 200 lên 305 Scoin); xuất 3 ảnh Game View 1080p.
- **[unity-building-coordinates-lock-20261009] Ghi nhớ & khóa tọa độ mặc định cố định riêng cho từng nhà**: Đã thiết lập bảng tọa độ chuẩn `BuildingDefaultCoordinates.cs` và component `PhoNhoBuildingAnchor.cs`, cập nhật `PhoNhoGameplaySceneBuilder.cs` và scene `CityOverworld_PlayerMovement.unity` để sau khi build hoặc khởi tạo, các nhà luôn đứng cố định tại tọa độ chuẩn: Ingredient_Shop `(-7.05, 0.8, 0)`, Boba_Shop `(-2.4, 0.7, 0)`, Player_House `(2.15, 1.4, 0)`, Breakfast_Shop `(6.75, 0.8, 0)`.
- **[unity-enter-shop-scene-transition-redo-006] Hoàn thiện Scene Nội Thất Quán Ăn Sáng & Chuyển Scene Thật Bằng SceneManager**:
- **[project-memory-progress-snapshot-20261009-001] Lưu snapshot báo cáo tiến độ game Phố Nhỏ vào Project Memory, bảo toàn lịch sử**: Đã lưu snapshot báo cáo tiến độ dự án game Phố Nhỏ vào Project Memory (docs/snapshots/snapshot-20261009-173632.json và .md) với lịch sử append-only bảo toàn ...
- **[web-dashboard-max-vfx-ui-003] Nâng cấp Web Dashboard đẳng cấp MAX VFX & Visual Polish đỉnh cao**: Nâng cấp toàn diện giao diện Web Developer Dashboard (`http://127.0.0.1:5050`) lên chuẩn rich aesthetics & MAX VFX: Canvas Ambient Particles hạt đèn lồng ấm áp trôi lơ lửng phản ứng chuột mượt mà; Web Audio Synthesizer SFX độc lập (zero external asset) với âm thanh tactile click, chime đổi tab, chime nhận task, arpeggio fanfare hoàn thành task và âm cảnh báo; nút bật/tắt âm thanh `localStorage`; Glassmorphism v2 & Specular Highlights; 3D hover lift cho Kanban/Profile cards; Shimmer sweep trên quota bar; Animated number counter nhảy số mượt mà; Toast notifications hiện đại với thanh thời gian progress bar; Ripple effect trên toàn bộ nút bấm; 20/20 unit test suites và E2E smoke test pass 100%.
- **[web-dashboard-functional-completion-002] Hoàn thiện toàn bộ UI Web Developer Dashboard**: Đã hoàn thiện toàn bộ UI Web Developer Dashboard (`http://127.0.0.1:5050`) loại bỏ 100% mock data/nút chết: thêm modal tạo task mới lưu trực tiếp vào TaskStore atomic; modal thêm profile Antigravity thật; nút xóa profile phụ an toàn; modal cài đặt cảnh báo Quota %; thanh action buttons trong task drawer (Claim, Báo Blocked, Trả về Pending, Đánh dấu Hoàn thành, Xóa Task); hỗ trợ HTML5 Drag & Drop giữa các cột Kanban; toàn bộ 20 unit test suites và E2E smoke test pass 100%.
- **[smoke-test-task-muvhf8vt] Task kiểm thử smoke test**: Smoke test completed
- **[unity-shop-interaction-cooking-flow-004] Tương tác Shop và Luồng Chế biến Nấu ăn**: Đã triển khai trigger proximity `ShopInteractionTrigger` trên 4 công trình (`Ingredient_Shop`, `Boba_Shop`, `Breakfast_Shop`, `Player_House`) hỗ trợ phím [E] và UI prompt; Domain Economy với `PlayerWallet` quản lý vốn ban đầu [CHỐT] 200 Scoin, 10 Gem, 0 Tcoin, giao dịch qua `LedgerEntry` bất biến với `idempotency_key`; Domain Cooking với `PlayerInventory` và `CookingService` (kiểm tra nguyên liệu, bắt đầu nấu, cập nhật tiến độ, hoàn tất và cộng doanh thu vào ví); Giao diện UI Canvas `ShopUIManager` (TopBar HUD tiền tệ, Modal Dialog chuyên biệt cho từng shop: Chợ mua sỉ, Menu pha chế Trà Sữa với thanh tiến độ Slider, Menu nấu Ăn Sáng, Hồ sơ gia chủ & Kho đồ Nhà phố); Tích hợp package `com.unity.ugui: 2.0.0` dùng font chuẩn Unity 6 `LegacyRuntime.ttf`; Batchmode validator pass 100% (0 errors), xuất ảnh `shop-dialog-view-1080p.png`.

## 6. Việc Đang Làm & Việc Tiếp Theo
- **Đang làm**: Đã hoàn thành trọn vẹn task `pho-nho-unity-three-scenes-rebuild-20261012-001`.
- **Việc tiếp theo**:
  1. M0: Chuẩn hóa hệ thống Audio/SFX cho nấu ăn, tiếng bước chân và âm thanh môi trường đường phố.
  2. M0: Tích hợp localization tiếng Việt toàn diện cho toàn bộ HUD và thực đơn quán.
  3. M0/M1: Dựng backend Docker Nakama + PostgreSQL, kiểm tra kết nối RPC và đồng bộ Ledger lên server.

---

## 7. Hướng dẫn Agent Tiết kiệm Token (Workflow chuẩn)
1. Khi bắt đầu phiên làm việc: Gọi tool `get_project_context` (hoặc đọc file `PROJECT_STATE.md`).
2. Xem mục **Việc Đang Làm** và danh sách `active_tasks` trả về; claim task cần làm qua `claim_task`.
3. Chỉ đọc `PROGRESS.md` hoặc `GAME_PROMPT.md` khi cần tra cứu chi tiết cụ thể của tính năng đang làm.
4. Chỉ mở các file code liên quan trực tiếp. Không quét hoặc đọc cả cây thư mục dự án.
5. Khi hoàn thành: gọi `report_result` với bằng chứng test cụ thể. Hệ thống sẽ tự động cập nhật Project Memory và lưu trữ an toàn các task done cũ vào archive.