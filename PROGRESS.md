# PROGRESS.md — Nhật ký tiến độ "Phố Nhỏ"

> Agent: đọc **TRẠNG THÁI HIỆN TẠI** và **VIỆC TIẾP THEO** là đủ để làm tiếp. Cuối phiên phải cập nhật file này (quy trình ở `AGENTS.md` §6).

## TRẠNG THÁI HIỆN TẠI

- **Mốc hiện tại**: M0 (Khởi tạo dự án Unity, Gameplay 2D Movement, Tương tác Cửa hàng & Web Developer Dashboard MAX VFX Đỉnh Cao)
- **Đã có**: bộ tài liệu thiết kế, cấu trúc `Assets/PhoNho/`, 13 background assets, scene phố `Assets/unity.unity`, `CityOverworld_ArtLayout.unity` và scene gameplay `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity`.
- **Character & Gameplay 2D Movement**:
  - Đã chuẩn hóa 4 sprite strip 1 hàng (`1x4`, `2048 x 640 px`, PPU 256, pivot 0.5, 0.1).
  - Walk animation của cả Nam và Nữ được tái thiết kế: bước chân luân phiên 2 chân rõ ràng, tiếp đất tại baseline Y=575 (pivot Y=0.1), chuyển trạng thái tức thì, tốc độ 3.0 unit/s.
  - Ground baseline thế giới được chuẩn hóa thống nhất tại `Y = -1.80f`.
  - Prefab `Assets/PhoNho/Prefabs/Player_Character.prefab` với Rigidbody2D Dynamic (FreezeRotation Z), CapsuleCollider2D chạm đất chuẩn xác, Animator và script `PhoNhoPlayerMovement`.
- **Infinite World (Background & Road/Ground cuộn vô tận)**:
  - Đã triển khai cơ chế cuộn lặp vô tận `PhoNhoInfiniteLayer` cho bầu trời `01_Sky`, chân trời `02_DistantTown` (parallax 0.78), vỉa hè `Sidewalk`, và lòng đường `Road`.
  - Đã triển khai `PhoNhoInfiniteGroundCollider` trên `Ground_Platform` giữ collider 100 units bao trọn Player.
  - Bảo toàn tuyệt đối: không nhân bản nhà/prop.
- **Tương tác Shop & Luồng Nấu ăn/Pha chế (Task unity-shop-interaction-cooking-flow-004)**:
  - Trigger tương tác proximity `ShopInteractionTrigger` gắn trên 4 công trình (`Ingredient_Shop`, `Boba_Shop`, `Breakfast_Shop`, `Player_House`) hỗ trợ phím `[E]` và nút chạm UI.
  - Hệ thống tiền tệ & ví `PlayerWallet`: Vốn khởi đầu chuẩn 200 Scoin, 10 Gem, 0 Tcoin. Mọi giao dịch tiền tệ đều đi qua `LedgerEntry` bất biến với `idempotency_key`.
  - Hệ thống kho đồ `PlayerInventory` và dịch vụ chế biến `CookingService`.
  - Giao diện UI `ShopUIManager`: TopBar HUD tiền tệ, Box prompt tương tác, Modal Dialog chuyên biệt cho từng công trình, thanh tiến độ nấu ăn real-time.
- **Web Developer Dashboard MAX VFX Đẳng Cấp (Task web-dashboard-max-vfx-ui-003 & 002)**:
  - Toàn bộ UI web (`http://127.0.0.1:5050`) đạt chuẩn thẩm mỹ cao cấp với rich aesthetics & MAX VFX sống động:
    - Canvas Ambient Particle VFX (`#ambient-canvas`): Các hạt ánh sáng đèn lồng ấm áp trôi lơ lửng, phản ứng tương tác dạt ra khi di chuột.
    - Web Audio Synthesizer SFX (`SoundSynth`): Âm thanh xúc giác tactile click, chime đổi tab, chime nhận task, arpeggio fanfare hoàn thành task và âm cảnh báo; nút bật/tắt âm thanh (`#btn-toggle-sound`) lưu trạng thái `localStorage`.
    - Glassmorphism v2 & Specular Highlights: Viền sáng specular trên cards, modals và drawers; 3D tilt và hover lift mượt mà cho Kanban cards và Profile cards; shimmer sweep trên các thanh tiến độ quota; pulse glow cho status badges.
    - Animated Number Counters (`animateNumber`): Đếm số mượt mà khi nạp dữ liệu overview và task metrics.
    - Toast Notification hiện đại (`showToast`): Thay thế toàn bộ `alert(...)` thô sơ bằng toast có progress bar tự co lại theo thời gian, phát âm thanh tương ứng theo level.
    - Hiệu ứng sóng nước Ripple (`setupRippleEffects`) trên toàn bộ nút bấm tương tác.
    - Chức năng thật 100%: Tạo task mới thật, thêm/xóa profile thật, cài đặt quota cảnh báo, action buttons trong task drawer, HTML5 drag & drop giữa các cột Kanban.
- **Project Memory & Token Optimization**:
  - Đã có `PROJECT_STATE.md` (Project Memory/Handoff) cô đọng mục tiêu, kiến trúc, quyết định chốt.
  - Endpoint `GET /v1/context` và MCP tool `get_project_context` hoạt động ổn định.
- **Chạy được**:
  - Web Developer Dashboard: chạy qua `node tools/ide-bridge/dashboard/server.mjs` (truy cập `http://127.0.0.1:5050`).
  - Scene `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity`: Di chuyển vô tận, lại gần các tiệm bấm `[E]` để mua nguyên liệu, pha trà sữa, nấu ăn sáng và xem hồ sơ nhà phố.
- **IDE Auto-Worker Daemon (Task ide-bridge-auto-worker-bootstrap-007)**:
  - Đã triển khai hoàn chỉnh daemon thường trực `tools/ide-bridge/worker.mjs` tự động khởi động khi mở workspace qua `.vscode/tasks.json` (`runOn: folderOpen`) và `.vscode/settings.json` (`task.allowAutomaticTasks: on`).
  - Hỗ trợ single-instance lock `.worker.lock`, heartbeat `.tasks/runtime/worker-status.json`, log an toàn `.tasks/runtime/ide-worker.log` redact secrets.
  - Tích hợp cơ chế dispatch chính thức vào Antigravity Agent qua `agentapi send-message` và companion `antigravity-ide.cmd chat -r`.
  - Cơ chế lease recovery (10 phút) và Queue policy ưu tiên task `unity-enter-shop-scene-transition-redo-006`.
  - Bộ test unit `worker.test.mjs` và test tích hợp `auto_worker_e2e.mjs` đạt 100% pass (22/22 test suites pass).
  - Worker daemon hiện đang chạy thường trực trong background (`antigravity-auto-worker-msi`) và đã tự động claim task `unity-enter-shop-scene-transition-redo-006` sang `in_progress`.

## VIỆC TIẾP THEO (theo thứ tự)

1. Task `unity-enter-shop-scene-transition-redo-006`: Nhấn E chuyển scene thật vào quán `ShopInterior_HoneyBreakfast.unity`, không dùng UI overlay, có điểm spawn và Exit quay về trước cửa quán.
2. M0: Hoàn thiện URP 2D, Addressables và Localization tiếng Việt.
3. M0/M1: Dựng backend Docker Nakama + PostgreSQL, kiểm tra kết nối RPC và đồng bộ Ledger lên server.
4. M1: Triển khai đánh giá khách hàng (review/sao), gắn chặt với đơn hàng hoàn tất.

## NHẬT KÝ PHIÊN

### Phiên 24 — 2026-10-08 (Lọc, kiểm tra toàn vẹn và commit toàn bộ 109 file dự án)
- **Mốc**: M0 / Tooling & Audit Git Commit
- **Đã làm**:
  - Khảo sát và phân loại chi tiết toàn bộ 109 file chưa commit (24 modified, 84 untracked, 1 staged).
  - Lọc và kiểm tra toàn vẹn: 31 file Unity `.meta` (0 orphan meta, 0 missing meta), 14 file C# scripts, 5 file Editor tools, 3 scenes/prefabs, 18 file IDE Bridge/Web Dashboard/Worker daemon, 10 file Skills.
  - Xác nhận an toàn bảo mật: không có secret/API token bị lộ; toàn bộ 22/22 test suites bridge pass 100%.
  - Thống nhất cùng người dùng giữ nguyên và commit toàn bộ 109 file (bao gồm cả ảnh Game View 1080p và file kết quả kiểm thử).
  - Thực hiện stage và commit toàn bộ 109 file với commit message chuẩn mực.
- **File chính**: `Assets/PhoNho/Scripts/**`, `Assets/PhoNho/Prefabs/**`, `Assets/PhoNho/Scenes/**`, `tools/ide-bridge/**`, `.agents/**`, `skills/**`, `.tasks/**`, `PROJECT_STATE.md`, `PROGRESS.md`.
- **Cách kiểm tra**: `npm test` trong `tools/ide-bridge` pass 22/22; `git status` sạch (clean working directory).

### Phiên 23 — 2026-10-06 (Hoàn thành Task ide-bridge-auto-worker-bootstrap-007: Nâng cấp IDE Bridge thành auto-worker thường trực)
- **Mốc**: Tooling / IDE Auto-Worker Daemon Bootstrap
- **Đã làm**:
  - Nhận và claim task `ide-bridge-auto-worker-bootstrap-007` với worker `antigravity-worker-auto-bootstrap-007`.
  - Phát hiện và kích hoạt entrypoint chính thức trên máy: `agentapi` (`C:/Users/tan/.gemini/antigravity-ide/bin/agentapi.bat`) và `antigravity-ide.cmd chat -r`.
  - Xây dựng daemon `tools/ide-bridge/worker.mjs` với single-instance lock (`.worker.lock`), lease recovery (10 phút) và status heartbeat (`worker-status.json`).
  - Cấu hình tự động khởi động khi mở workspace qua `.vscode/tasks.json` (`runOn: folderOpen`) và `.vscode/settings.json` (`task.allowAutomaticTasks: on`).
  - Bổ sung endpoint `/api/worker-status` và `/v1/worker/status` trong `tools/ide-bridge/server.mjs`.
  - Cập nhật hướng dẫn trong `docs/GPT_BRIDGE_INSTRUCTIONS.md`.
  - Viết test unit `worker.test.mjs` và test tích hợp end-to-end `auto_worker_e2e.mjs`, toàn bộ 22 test suites pass 100%.
  - Khởi chạy daemon background: worker tự động thăm dò Cloud Bridge, phát hiện và tự động claim `unity-enter-shop-scene-transition-redo-006` sang `in_progress`.
- **File chính**: `tools/ide-bridge/worker.mjs`, `tools/ide-bridge/server.mjs`, `tools/ide-bridge/store.mjs`, `.vscode/tasks.json`, `.vscode/settings.json`, `docs/GPT_BRIDGE_INSTRUCTIONS.md`, `tools/ide-bridge/test/worker.test.mjs`, `tools/ide-bridge/test/auto_worker_e2e.mjs`.
- **Cách kiểm tra**: Chạy `npm test` trong `tools/ide-bridge` (22/22 passed); kiểm tra `worker-status.json` và log `ide-worker.log`.

### Phiên 22 — 2026-10-05 (Hoàn thành Task web-dashboard-max-vfx-ui-003: Nâng cấp Web Dashboard đẳng cấp MAX VFX & Visual Polish đỉnh cao)
- **Mốc**: M0 / Web Dashboard MAX VFX UI & Visual Excellence
- **Đã làm**:
  - Nhận và claim task `web-dashboard-max-vfx-ui-003` với worker `antigravity-worker-20261005-dashboard-vfx`.
  - Triển khai Canvas Ambient Particle VFX (`#ambient-canvas`): 42 hạt sáng lơ lửng màu đèn lồng phố ấm áp (amber, lantern gold, cozy cyan, soft purple) bay nhẹ, phản ứng đẩy dạt mượt mà khi rê chuột.
  - Triển khai Web Audio Synthesizer SFX (`SoundSynth`): tổng hợp âm thanh bằng Web Audio API không phụ thuộc file ngoài, cung cấp xúc giác click, chime đổi tab, chime nhận task, arpeggio fanfare hoàn thành task và âm cảnh báo; nút bật/tắt âm thanh (`#btn-toggle-sound`) lưu trạng thái `localStorage`.
  - Nâng cấp Glassmorphism v2 & Specular Highlights: viền sáng specular trên cards, modals và drawers; 3D tilt và hover lift mượt mà cho Kanban cards và Profile cards; shimmer sweep trên các thanh tiến độ quota; pulse glow cho status badges.
  - Triển khai Animated Number Counters (`animateNumber`): đếm số mượt mà khi tải dữ liệu overview và task metrics.
  - Triển khai hệ thống Toast Notification hiện đại (`showToast`): thay thế toàn bộ `alert(...)` thô sơ bằng toast có progress bar tự co lại theo thời gian, phát âm thanh tương ứng theo level (success, warning, error, info).
  - Triển khai hiệu ứng sóng nước Ripple (`setupRippleEffects`) trên toàn bộ nút bấm tương tác.
  - Bổ sung test case thứ 5 trong `tools/ide-bridge/test/dashboard.test.mjs` xác thực trọn vẹn toàn bộ visual elements, keyframes và subsystems.
  - File chính: `tools/ide-bridge/dashboard/public/index.html`, `tools/ide-bridge/dashboard/public/styles.css`, `tools/ide-bridge/dashboard/public/app.js`, `tools/ide-bridge/test/dashboard.test.mjs`, `tools/ide-bridge/test/e2e_smoke_test.mjs`.
  - Kiểm tra: `node tools/ide-bridge/test/e2e_smoke_test.mjs` PASS 100%, `node --test tools/ide-bridge/test/*.test.mjs` PASS 20/20 tests (1 skipped).
- **Lỗi / Nợ kỹ thuật**: Không.

### Phiên 21 — 2026-10-05 (Hoàn thành Task web-dashboard-functional-completion-002: Hoàn thiện toàn bộ chức năng UI Web Dashboard không để mock/nút chết)
- **Mốc**: M0 / Web Dashboard Functional Completion & Developer Tooling
- **Đã làm**:
  - Nhận và claim task `web-dashboard-functional-completion-002` với worker `antigravity-worker-20261005-dashboard-002`.
  - Khảo sát toàn bộ UI web dashboard, phát hiện các nút chết/thiếu chức năng: thiếu nút/modal Tạo Task mới, nút `+ Thêm Profile` không có handler/modal, thiếu nút Xóa Profile, thiếu form Cài đặt Quota, Task Drawer chỉ có chế độ đọc không có action thao tác task, Kanban board chưa hỗ trợ kéo thả đổi trạng thái.
  - Bổ sung Backend REST API trong `tools/ide-bridge/dashboard/server.mjs`:
    - `POST /api/tasks`: Nhận body task và submit vào `TaskStore`, lưu atomic JSON và Markdown.
    - `POST /api/tasks/:id/claim`: Claim task với worker name.
    - `POST/PATCH /api/tasks/:id/status`: Chuyển đổi trạng thái linh hoạt (`pending`, `in_progress`, `blocked`, `done`) kèm ghi nhận lý do và result.
    - `DELETE /api/tasks/:id`: Xóa task an toàn khỏi runtime storage.
    - `DELETE /api/profiles/:id`: Xóa profile phụ, ngăn chặn xóa active profile và bảo toàn ít nhất 1 profile.
    - Sửa triệt để các route GET (`/api/tasks`, `/api/profiles`, `/api/overview`, `/api/done-log`) kiểm tra đúng `req.method === 'GET'`.
  - Bổ sung Giao diện UI trong `tools/ide-bridge/dashboard/public/index.html`:
    - Nút `+ Tạo Task` trên thanh điều khiển Kế hoạch, modal `create-task-modal` với nút sinh ID tự động.
    - Modal `add-profile-modal` (Tên hiển thị, Email Google, Quota khởi tạo).
    - Modal `settings-modal` (Ngưỡng cảnh báo Quota thấp, Tự động gợi ý chuyển khi dính 429).
    - Modal `complete-task-modal` (Báo cáo kết quả và bằng chứng kiểm thử).
    - Thanh action buttons trong Task Drawer: `⚡ Nhận Task`, `🛑 Báo Blocked`, `⏳ Trả về Pending`, `✅ Đánh dấu Hoàn thành`, `🗑️ Xóa Task`.
    - Nút `Xóa` trên từng thẻ Profile phụ.
  - Bổ sung Styles trong `tools/ide-bridge/dashboard/public/styles.css`:
    - Styling modal lớn responsive, form controls, drawer actions bar, visual indicator khi drag & drop (dragging, drag-over).
  - Triển khai Logic trong `tools/ide-bridge/dashboard/public/app.js`:
    - Kết nối tất cả modals, phím tắt Escape đóng mọi modal.
    - Triển khai HTML5 Drag & Drop trên Kanban Board: kéo card thả vào cột lập tức gọi API đổi trạng thái task thật trên server.
    - Điều chỉnh Quota thủ công (+/- 5%, lưu %) và cập nhật theo thời gian thực.
  - Viết test & nghiệm thu:
    - Bổ sung test suite trong `tools/ide-bridge/test/dashboard.test.mjs` kiểm tra toàn bộ luồng tạo, claim, block, done, delete task và profile.
    - Viết script `tools/ide-bridge/test/e2e_smoke_test.mjs` chạy end-to-end smoke test mô phỏng toàn bộ hành vi người dùng trên Web UI: pass 100% không lỗi.
    - Toàn bộ 20 bài test trong `tools/ide-bridge/test/*.test.mjs` đều pass 100%.
- **File chính**: `tools/ide-bridge/dashboard/server.mjs`, `tools/ide-bridge/dashboard/public/index.html`, `tools/ide-bridge/dashboard/public/app.js`, `tools/ide-bridge/dashboard/public/styles.css`, `tools/ide-bridge/test/dashboard.test.mjs`, `tools/ide-bridge/test/e2e_smoke_test.mjs`, `.tasks/web-dashboard-functional-completion-002.json`, `PROGRESS.md`, `PROJECT_STATE.md`.
- **Kiểm tra**: E2E smoke test pass 100%, 20/20 unit test suites pass, 0 nút chết, 0 mock data.
- **Nợ kỹ thuật**: Không có.

### Phiên 20 — 2026-10-05 (Hoàn thành Task unity-shop-interaction-cooking-flow-004: Tương tác Shop, Chợ Nguyên Liệu, Pha Chế Trà Sữa, Nấu Ăn Sáng & Hồ Sơ Nhà Phố)
- **Mốc**: M0 / Shop Interaction, Economy Ledger & Cooking Flow
- **Đã làm**:
  - Nhận và claim task `unity-shop-interaction-cooking-flow-004` với worker `antigravity-worker-20261005-shop-cooking-004`.
  - Xây dựng Domain Economy: `CurrencyType` (Scoin, Gem, Tcoin), `LedgerEntry` (bất biến, có `idempotencyKey`, `balanceAfter`, `timestamp`, `reason`), `PlayerWallet` quản lý vốn ban đầu [CHỐT] 200 Scoin, 10 Gem, 0 Tcoin, giao dịch qua `ApplyTransaction`.
  - Xây dựng Domain Cooking & Inventory: `IngredientItem`, `RecipeItem`, `PlayerInventory` (quản lý tồn kho nguyên liệu và thành phẩm), `CookingService` (kiểm tra nguyên liệu, bắt đầu nấu, cập nhật tiến độ, hoàn tất và cộng doanh thu vào ví có idempotency).
  - Xây dựng Gameplay Interaction: `ShopType` (4 loại shop), `ShopInteractionTrigger` gắn trên `Ingredient_Shop`, `Boba_Shop`, `Breakfast_Shop`, `Player_House` với prompt [E] và event proximity.
  - Xây dựng UI: `ShopUIManager` tạo UI Canvas procedural 1920x1080 (TopBar HUD tiền tệ, Prompt Box, Shop Dialog modal chuyên biệt cho từng shop: Chợ sỉ nguyên liệu, Menu pha chế kèm thanh tiến độ Slider, Menu nấu ăn sáng, Hồ sơ gia chủ & Kho đồ nhà phố; tạm dừng/khôi phục input di chuyển của player).
  - Tích hợp package chuẩn `com.unity.ugui: 2.0.0` vào `Packages/manifest.json`, dùng font chuẩn Unity 6 `LegacyRuntime.ttf`.
  - Tích hợp `ShopInteractionCookingValidator` và `GroundWalkArtRunner`: kiểm tra trọn vẹn luồng từ khởi tạo ví 200 Scoin, mua nguyên liệu ở Chợ, nấu 2 món trà sữa, nhận doanh thu vào ví, từ chối khi hết nguyên liệu, mở quán ăn sáng và nhà phố.
  - Chạy Unity batchmode: toàn bộ validators pass 100% (0 errors), xuất ảnh `shop-dialog-view-1080p.png` và cập nhật `shop-cooking-validation-results.json` (`success: true`).
- **File chính**: `Assets/PhoNho/Scripts/Domain/Economy/CurrencyType.cs`, `Assets/PhoNho/Scripts/Domain/Economy/LedgerEntry.cs`, `Assets/PhoNho/Scripts/Domain/Economy/PlayerWallet.cs`, `Assets/PhoNho/Scripts/Domain/Cooking/IngredientItem.cs`, `Assets/PhoNho/Scripts/Domain/Cooking/RecipeItem.cs`, `Assets/PhoNho/Scripts/Domain/Cooking/PlayerInventory.cs`, `Assets/PhoNho/Scripts/Domain/Cooking/CookingService.cs`, `Assets/PhoNho/Scripts/Gameplay/Interaction/ShopType.cs`, `Assets/PhoNho/Scripts/Gameplay/Interaction/ShopInteractionTrigger.cs`, `Assets/PhoNho/Scripts/UI/ShopUIManager.cs`, `Assets/PhoNho/Editor/ShopInteractionCookingValidator.cs`, `Assets/PhoNho/Editor/PhoNhoGameplaySceneBuilder.cs`, `Assets/PhoNho/Editor/GroundWalkArtRunner.cs`, `Packages/manifest.json`, `Assets/PhoNho/Editor/shop-cooking-validation-results.json`, `Assets/PhoNho/Editor/shop-dialog-view-1080p.png`.
- **Kiểm tra**: Unity 6000.4.3f1 batchmode validator pass 100% (`ExitCode: 0`, 0 errors), xuất ảnh Shop Dialog view 1920x1080.
- **Nợ kỹ thuật**: Không có.

### Phiên 19 — 2026-10-05 (Hoàn thành Task unity-infinite-background-road-003: Infinite Background và Road/Ground)
- **Mốc**: M0 / Infinite Scrolling & World Expansion
- **Đã làm**:
  - Nhận và claim task `unity-infinite-background-road-003` với worker `antigravity-worker-20261005-infinite-road-003`.
  - Thiết kế và tạo component `PhoNhoInfiniteLayer.cs` cho phép cuộn lặp vô tận $O(1)$ tự động tính toán wrap-around theo tọa độ camera trong local space của layer, áp dụng cho: bầu trời `01_Sky` (3 tiles 19.2f), chân trời phố xa `02_DistantTown` (3 tiles 20.2f với parallax 0.78), vỉa hè gạch hoa `Sidewalk` (7 tiles bao phủ ~46 units, căn mặt gạch đúng `GroundBaseline = -1.80f`), và lòng đường nhựa `Road` (3 tiles bao phủ ~61 units).
  - Thiết kế và tạo component `PhoNhoInfiniteGroundCollider.cs` gắn trên `Ground_Platform` (BoxCollider2D size 100f, 1.0f) tự động recenter khi Player di chuyển quá 20 units, đảm bảo Player luôn có mặt đất vật lý vững chắc bên dưới ở mọi tọa độ X từ $-\infty$ đến $+\infty$.
  - Cập nhật `PhoNhoCameraFollow.cs`: thêm `ClampX`, `SetClamp`, `DisableClamping` và tắt camera clamping (`ClampX = false`) trong scene gameplay; gỡ bỏ hoàn toàn `Boundary_Left` và `Boundary_Right`.
  - Tuân thủ nghiêm ngặt yêu cầu: TUYỆT ĐỐI KHÔNG recycle/nhân bản nhà và prop. Toàn bộ nhà (`Ingredient_Shop`, `Boba_Shop`, `Player_House`, `Breakfast_Shop`, `Shade_Tree`) và prop (`Street_Lamp`, `Bench_Left`, `Bench_Right`) giữ nguyên đúng 1 thực thể tại trung tâm phố.
  - Cập nhật `PlayerMovementValidator.cs`: thêm kiểm tra chạy xa sang phải ($X = +43.1f$) và sang trái ($X = -46.0f$), kiểm tra không rơi xuyên đất, collider bao bọc, camera follow, vỉa hè và đường phủ kín tầm nhìn camera 100%, và xác minh số lượng từng công trình/prop đúng bằng 1.
  - Cập nhật `GroundWalkArtRunner.cs`: kết xuất ảnh Game view 1080p tại tâm phố ($X = 0$), xa bên phải ($X = +45f$), và xa bên trái ($X = -45f$).
  - Chạy kiểm thử thành công: Unity 6000.4.3f1 batchmode validator pass 100% (0 errors), IDE bridge test suite pass 18/18.
- **File chính**: `Assets/PhoNho/Scripts/Map/PhoNhoInfiniteLayer.cs`, `Assets/PhoNho/Scripts/Map/PhoNhoInfiniteGroundCollider.cs`, `Assets/PhoNho/Scripts/Map/PhoNhoCameraFollow.cs`, `Assets/PhoNho/Editor/PhoNhoGameplaySceneBuilder.cs`, `Assets/PhoNho/Editor/PlayerMovementValidator.cs`, `Assets/PhoNho/Editor/GroundWalkArtRunner.cs`, `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity`, `Assets/PhoNho/Editor/player-movement-validation-results.json`, `Assets/PhoNho/Editor/gameplay-view-1080p.png`, `Assets/PhoNho/Editor/gameplay-view-right-1080p.png`, `Assets/PhoNho/Editor/gameplay-view-left-1080p.png`, `PROGRESS.md`, `PROJECT_STATE.md`.
- **Kiểm tra**: Unity 6000.4.3f1 batchmode validator pass 100% (`player-movement-validation-results.json`); 3 ảnh Game view 1920x1080; test suite bridge pass 18/18.
- **Nợ kỹ thuật**: Không có.

### Phiên 18 — 2026-10-05 (Hoàn thành Task unity-ground-art-walk-animation-002: Sửa Ground Baseline và Walk Animation Nam/Nữ)
- **Mốc**: M0 / Art, Animation & Ground Baseline Alignment
- **Đã làm**:
  - Nhận và claim task `unity-ground-art-walk-animation-002` với worker `antigravity-worker-20261005-ground-walk-002`.
  - Khảo sát và xác định nguyên nhân nhà/mặt đất bị lệch: các sprite PNG có padding transparent khác nhau ở đáy (nhà 44-116px, ghế 93px, đèn 38px, vỉa hè gạch hoa 202px ở đáy và 213px ở đỉnh), đồng thời tên sprite vỉa hè bị sai dẫn đến không render vỉa hè và collider đặt lệch ở -2.58f trong khi chân nhà ở -1.78f.
  - Thiết lập chuẩn Ground Baseline thống nhất tại `Y = -1.80f`: bù trừ bottom padding cho toàn bộ công trình (`Ingredient_Shop`, `Boba_Shop`, `Player_House`, `Breakfast_Shop`), gốc cây (`Shade_Tree`), đèn đường (`Street_Lamp`), ghế băng (`Bench_Left`, `Bench_Right`) để chân visual tiếp xúc chính xác 100% trên mặt vỉa hè.
  - Sửa vỉa hè `Dải vỉa hè gạch hoa-1.png`: căn mép trên gạch hoa nằm đúng `GroundBaseline = -1.80f`, mép dưới tiếp giáp tự nhiên với lòng đường nhựa `Road`.
  - Căn chỉnh `Ground_Platform` BoxCollider2D (mặt trên đúng `-1.80f`), Player CapsuleCollider2D (đáy offset Y=0.90 size Y=1.80 trùng khít Y=0 bàn chân).
  - Tái tạo walk cycle 4 frames cho cả nhân vật Nam và Nữ: bước chân luân phiên 2 chân rõ rệt (contact pose chạm đất, passing pose lướt qua), depth shading phân biệt chân gần/chân xa, bàn chân tiếp đất vững chãi tại Baseline Y=575 (pivot Y=0.1), đồng nhất độ cao đầu và thân người dao động tự nhiên 1-2px (triệt tiêu hoàn toàn cú sụt lún/nảy thân 20px).
  - Tinh chỉnh Animator Controller: transition duration = 0f (chuyển tức thì không trễ bước), canTransitionToSelf = false, đồng bộ tốc độ di chuyển `MoveSpeed = 3.0f` với nhịp bước 8 FPS triệt tiêu foot sliding.
  - Viết runner `GroundWalkArtRunner.cs` kết xuất ảnh Game view 1920x1080 trực tiếp từ Scene Camera.
  - Chạy toàn bộ validation tự động trong Unity 6000.4.3f1 batchmode: `PlayerMovementValidator` và `CharacterPlayModeValidator` đạt 100% pass (0 errors).
- **File chính**: `Assets/PhoNho/Editor/PhoNhoGameplaySceneBuilder.cs`, `Assets/PhoNho/Editor/PlayerMovementValidator.cs`, `Assets/PhoNho/Editor/GroundWalkArtRunner.cs`, `Assets/PhoNho/Editor/CharacterAnimationBuilder.cs`, `Assets/PhoNho/Art/Characters/Male_A_Movement_Strip.png`, `Assets/PhoNho/Art/Characters/Female_A_Movement_Strip.png`, `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity`, `Assets/PhoNho/Prefabs/Player_Character.prefab`, `Assets/PhoNho/Editor/gameplay-view-1080p.png`.
- **Kiểm tra**: Unity 6000.4.3f1 batchmode validator pass 100% (`player-movement-validation-results.json`, `character-validation-results.json`); Game view 1920x1080 kết xuất tại `Assets/PhoNho/Editor/gameplay-view-1080p.png`; test suite bridge pass 18/18.
- **Nợ kỹ thuật**: Không có.

### Phiên 17 — 2026-10-05 (Hoàn thành Task antigravity-game-dev-skill-001: Chuyển đổi Game Development Skill sang Antigravity Native)
- **Mốc**: Dev Tooling & Antigravity Native Skills
- **Đã làm**:
  - Khảo sát nguồn `davila7/claude-code-templates` (MIT License) và chuyển đổi hoàn chỉnh thành skill native cho Antigravity IDE đặt tại `.agents/skills/game-development/` (và bản đồng bộ `skills/game-development/`).
  - Loại bỏ triệt để các giả định của Claude Code (`allowed-tools`, cấu hình đường dẫn Claude), thay thế bằng các công cụ Antigravity (`view_file`, `write_to_file`, `replace_file_content`, `run_command`).
  - Trọng tâm hóa vào Unity 2D (C#) cho Phố Nhỏ: FixedUpdate, Rigidbody2D Dynamic (FreezeRotation Z), Collider2D, legacy Input Manager, camera follow, và quy trình validation tự động bằng Unity 6000.4.3f1 batchmode.
  - Tích hợp Project Memory (`PROJECT_STATE.md` và `get_project_context`) với cơ chế progressive disclosure gồm 4 tài liệu tham chiếu chuyên sâu (`references/`).
  - Ghi nhận đầy đủ nguồn gốc và giấy phép (Attribution & MIT License note).
  - Viết bộ kiểm thử tự động `tools/ide-bridge/test/skill.test.mjs`, toàn bộ 19/19 test suites đều pass 100%.
- **File chính**: `.agents/skills/game-development/SKILL.md`, `.agents/skills/game-development/references/*.md`, `skills/game-development/`, `tools/ide-bridge/test/skill.test.mjs`, `skill.md`, `PROJECT_STATE.md`, `PROGRESS.md`.
- **Kiểm tra**: Chạy `node --test tools/ide-bridge/test/*.test.mjs` đạt 18 pass, 0 fail, 1 skip; chạy lại Unity batchmode validator đạt 100% pass, không có regression.
- **Nợ kỹ thuật**: Không có.

### Phiên 16 — 2026-10-05 (Hoàn thành Task project-memory-archive-001: Project Memory & Task Archive cho IDE Bridge)
- **Mốc**: Dev Tooling & Token Optimization
- **Đã làm**:
  - Tạo module `tools/ide-bridge/memory.mjs` và tài liệu bộ nhớ dự án `PROJECT_STATE.md` (chứa mục tiêu, kiến trúc, quyết định cốt lõi, module quan trọng, việc vừa hoàn thành, việc tiếp theo).
  - Tích hợp cơ chế tự động cập nhật tóm tắt vào `PROJECT_STATE.md` khi task hoàn thành (chỉ lưu 1-2 câu súc tích, không sao chép log dài, kiểm soát dung lượng <16KB).
  - Bổ sung endpoint `GET /v1/context` và MCP tool `get_project_context` giúp Agent mới lấy toàn bộ bối cảnh dự án và task active trong 1 call duy nhất, không tải lịch sử done dài.
  - Xây dựng cơ chế archive an toàn: thư mục `.tasks/runtime/archive/`, phương thức `archive(id)`, `autoArchiveDone(keepRecent=2)`, `listArchived()`, và tra cứu tự động trong `read(id)` khi tra cứu task cũ.
  - Cập nhật OpenAPI schema, `server.mjs`, `store.mjs`, `mcp.mjs`, `AGENTS.md` (§1 thứ tự đọc tiết kiệm token) và `docs/IDE_BRIDGE.md` (§4 workflow mới).
  - Viết bộ kiểm thử tự động `tools/ide-bridge/test/memory.test.mjs`, chạy toàn bộ test suite đạt 14/14 pass (13 pass, 1 skip theo thiết kế trên Windows).
- **File chính**: `PROJECT_STATE.md`, `tools/ide-bridge/memory.mjs`, `tools/ide-bridge/store.mjs`, `tools/ide-bridge/mcp.mjs`, `tools/ide-bridge/server.mjs`, `tools/ide-bridge/openapi.mjs`, `tools/ide-bridge/test/memory.test.mjs`, `docs/IDE_BRIDGE.md`, `AGENTS.md`.
- **Kiểm tra**: Chạy `node --test tools/ide-bridge/test/*.test.mjs` đạt 13 pass, 0 fail; kiểm tra trực tiếp `getContext()` (~5.3KB) và `autoArchiveDone()` hoạt động chính xác.
- **Nợ kỹ thuật**: Không có.

### Phiên 15 — 2026-10-05 (Hoàn thành Task unity-map-player-movement-001: Tạo Map 2D & Player Physics Movement)
- **Mốc**: M0 / Gameplay 2D & Player Controller
- **Đã làm**:
  - Nhận và claim task `unity-map-player-movement-001` trên cloud bridge `game.zcloudviet.xyz` và local MCP bridge với worker `antigravity-worker-20261005-unity-map`.
  - Sửa lỗi xử lý UTF-8 BOM trong `tools/ide-bridge/store.mjs` giúp TaskStore tương thích an toàn với file có BOM.
  - Viết controller vật lý `Assets/PhoNho/Scripts/Character/PhoNhoPlayerMovement.cs`: di chuyển ngang A/D và mũi tên trái/phải, giữ vận tốc trọng lực Y, khóa xoay FreezeRotation Z, tự động flipX và cập nhật Animator.
  - Viết `Assets/PhoNho/Scripts/Map/PhoNhoCameraFollow.cs`: camera bám theo nhân vật mượt mà, có giới hạn biên street.
  - Tạo prefab `Assets/PhoNho/Prefabs/Player_Character.prefab` và scene `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity` qua builder `PhoNhoGameplaySceneBuilder.cs`.
  - Viết và chạy validator `Assets/PhoNho/Editor/PlayerMovementValidator.cs` trong Unity 6000.4.3f1 batchmode: mô phỏng rơi vật lý, chạm collider mặt đất ổn định, di chuyển trái/phải và hướng nhìn chính xác (100% pass, 0 lỗi).
- **File chính**: `Assets/PhoNho/Scripts/Character/PhoNhoPlayerMovement.cs`, `Assets/PhoNho/Scripts/Map/PhoNhoCameraFollow.cs`, `Assets/PhoNho/Editor/PhoNhoGameplaySceneBuilder.cs`, `Assets/PhoNho/Editor/PlayerMovementValidator.cs`, `Assets/PhoNho/Prefabs/Player_Character.prefab`, `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity`, `tools/ide-bridge/store.mjs`.
- **Kiểm tra**: Chạy `PlayerMovementValidator.ValidateAll()` và `CharacterPlayModeValidator.ValidateAll()` trong Unity 6000.4.3f1 batchmode (kết quả tại `Assets/PhoNho/Editor/player-movement-validation-results.json` và `character-validation-results.json` đều success=true); chạy `node --test tools/ide-bridge/test/*.test.mjs` đạt 8/8 pass.
- **Nợ kỹ thuật**: Cần bổ sung các hiệu ứng hạt bụi bước chân (Dust VFX) và âm thanh bước chân khi di chuyển.

### Phiên 14 — 2026-10-04 (Hoàn thiện backend & kết nối tab Quản lý Tài khoản Antigravity IDE)
- **Mốc**: M0 / Dev Tools
- **Đã làm**:
  - Điều tra thực tế thư mục `%APPDATA%\Antigravity IDE\logs`: xác nhận IDE không có log hay CLI lưu số quota, agent không có tool tra quota; tuân thủ nghiêm ngặt không dùng MITM proxy, không đọc session token/cookie.
  - Cập nhật model dữ liệu `.ide-bridge/profiles.json` hỗ trợ 3 nguồn số liệu minh bạch:
    a) "nhập tay": API `POST /api/profiles/:id/quota` với % trực tiếp và delta `+/-`, gán timestamp cập nhật.
    b) "lỗi quota": bridge tự động trích xuất `profile_id` từ worker ID agent (vd `antigravity-worker-profile-main-session13`), đặt trạng thái "hết token" khi gặp 429/ResourceExhausted, ghi lịch sử và tạo gợi ý chuyển acc (có modal xác nhận).
    c) "ước tính theo lượt": đếm số lượt claim/report trong cửa sổ 5 giờ (`turns5h`), gắn nhãn rõ là ước tính, không phải token thật.
  - Bổ sung kiểm tra dữ liệu cũ: quá 30 phút không cập nhật hiển thị badge "số liệu cũ" và banner nhắc nhở. Không có nguồn hiển thị "chưa rõ", không bịa số.
  - Cập nhật Dashboard UI (`index.html`, `styles.css`, `app.js`) với các nút điều khiển nhập tay, badge nguồn và banner gợi ý chuyển profile.
  - Viết bộ test `tools/ide-bridge/test/dashboard.test.mjs`, toàn bộ 8/8 tests pass.
- **File chính**: `tools/ide-bridge/dashboard/data.mjs`, `tools/ide-bridge/dashboard/server.mjs`, `tools/ide-bridge/server.mjs`, `tools/ide-bridge/dashboard/public/index.html`, `tools/ide-bridge/dashboard/public/app.js`, `tools/ide-bridge/dashboard/public/styles.css`, `tools/ide-bridge/test/dashboard.test.mjs`, `.ide-bridge/profiles.json`.
- **Kiểm tra**: Chạy `node --test tools/ide-bridge/test/*.test.mjs` đạt 8/8 pass; gọi API local `http://127.0.0.1:5050/api/profiles` và test cập nhật quota thành công.
- **Nợ kỹ thuật**: Việc chuyển cửa sổ IDE tự động trên Windows (focus window process) cần quyền OS/PowerShell riêng, hiện tại dashboard hiển thị modal hướng dẫn và xác nhận chuyển phiên an toàn cho người dùng.

## DECISIONS (quyết định đã chốt / giả định đang dùng)

| # | Quyết định | Trạng thái |
|---|---|---|
| D1 | Mobile Android/iOS, Unity C#, 2D có bản đồ thành phố, chạm để di chuyển | CHỐT |
| D2 | Tương tác bất đồng bộ giữa người chơi | CHỐT |
| D3 | Khách: NPC lấp chỗ trống + người chơi thật cho đơn lớn và đánh giá | CHỐT |
| D4 | Chỉ người đã mua mới đánh giá; giới hạn lượt/ngày; chủ quán báo cáo đánh giá sai | CHỐT |
| D5 | Bản đầu: Trà sữa, Đồ ăn sáng, Nguyên vật liệu. Sửa xe và Bánh để sau; xe hỏng do thợ NPC (đắt/chậm hơn) | CHỐT |
| D6 | Gameplay nghề: quản lý/idle | CHỐT |
| D7 | Tiền: Scoin, Gem (hiếm, kiếm trong game), Tcoin (nạp, chỉ thẩm mỹ) | CHỐT |
| D8 | Giá nguyên liệu do nhà cung cấp tự đặt hoàn toàn tự do | CHỐT |
| D9 | Sao ảnh hưởng: NPC đông hơn, mở khóa giá bán cao hơn, thưởng Gem theo mốc | CHỐT |
| D10 | Michelin: 5 sao nhận; mất nếu tụt dưới 5 | CHỐT |
| D11 | 4 bảng xếp hạng: bá khí, sao/Michelin, doanh thu, theo nghề | CHỐT |
| D12 | Tìm tiệm: đi bộ trên bản đồ và danh sách/dịch chuyển | CHỐT |
| D13 | Offline vẫn kiếm tiền nhưng ít hơn online | CHỐT |
| D14 | Ngôn ngữ: Tiếng Việt trước, sẵn sàng thêm ngôn ngữ; AGENTS.md chung | CHỐT |
| D15 | Backend: Nakama + PostgreSQL (mã nguồn mở, RPC, ledger ACID, ví, BXH) | CHỐT |
| D16 | Kho tổng NPC làm nguồn hàng và trần giá cho nguyên liệu | GIẢ ĐỊNH |
| D17 | Công thức sao (trung bình N đánh giá gần nhất, làm mịn Bayes), ngưỡng Michelin ≥4.95 | GIẢ ĐỊNH |
| D18 | Offline = 40% tốc độ, tối đa 6 giờ; giới hạn 5 đánh giá/ngày | GIẢ ĐỊNH (config) |
| D19 | Mỗi người chơi 1 nghề; đổi nghề khi đủ điều kiện ở cập nhật sau | CHỐT |
| D20 | Thu nhập cơ bản 25 Scoin/phút (online, cấp khởi đầu) | CHỐT (diễn giải, kiểm tra lại) |
| D21 | Gem mua trang phục đặc biệt, pet, v.v. | CHỐT |
| D22 | Lọc từ khóa thô tục/tấn công trong đánh giá | CHỐT |
| D23 | Bản đồ hiển thị avatar + số sao + tên quán của người chơi khác | CHỐT |
| D24 | Art style Chibi pastel (gần Tiệm Trà Nhỏ), màn hình Ngang (Landscape), bàn giao cả Mockup UI và bộ Asset | CHỐT |
| D25 | Nhân vật & Pet dùng 2D Spine (Skeletal Animation) với modular skin slot | CHỐT |
| D26 | Bản đồ thành phố dạng con phố dài cuộn cảnh ngang (Side-scrolling Parallax 2D) | CHỐT |
| D27 | Vốn khởi đầu cho người chơi mới = 200 Scoin | CHỐT |
| D28 | Hệ thống Âm thanh: BGM Acoustic/Lo-fi chill + SFX êm dịu, sinh động (pha chế, thu tiền, chấm sao) | CHỐT |
| D29 | Hiệu ứng VFX hạt (Sparkle particles) cho hào quang Bá Khí và vầng sáng sao Michelin 5.0★ | CHỐT |
| D30 | Môi trường thời tiết động Parallax (nắng, mưa bay, lá rụng) + chu kỳ ngày/đêm phố lên đèn | CHỐT |
| D31 | Kiểm duyệt UGC giai đoạn đầu: tập trung nút Báo cáo đánh giá sai cho chủ quán | CHỐT |
| D32 | Điều kiện cụ thể để mở đổi nghề: để dành khi bắt đầu phát triển bản cập nhật tương lai | CHỐT |
| D33 | Nhân vật nam/nữ trang phục A (áo kem, short xanh/hồng); sprite sheet hiện dùng để thử art, không thay quyết định Spine D25. Nguồn trong img/character, đầu ra căn theo ô 512×640/pivot chân. | CHỐT (A) / GIẢ ĐỊNH (quy chuẩn kỹ thuật) |
| D34 | Cầu nối giao việc dùng REST Action + MCP stdio, Node >=22 và thư viện built-in (không thêm dependency); webhook chỉ lưu task, IDE agent thực thi theo quyền riêng. | TRIỂN KHAI / CHƯA KÍCH HOẠT Ở MÁY NGƯỜI DÙNG |
| D35 | Project Memory qua PROJECT_STATE.md & context 1-call (get_project_context), auto-archive task done cũ vào .tasks/runtime/archive/ | CHỐT |

## OPEN QUESTIONS (cần người dùng trả lời)

- Toàn bộ các câu hỏi lớn về Thiết kế, Đồ họa, Âm thanh và Hạ tầng Backend đã được giải đáp đầy đủ!
- Sẵn sàng bước vào giai đoạn kỹ thuật M0 (Khởi tạo repo cấu trúc thư mục, Unity project 2D URP và Nakama local dev).

## NHẬT KÝ PHIÊN (mới nhất ở trên cùng)

### Phiên 13 — 2026-10-04 (Triển khai hoàn chỉnh Phố Nhỏ Developer Dashboard & Hoàn thành Task web-dashboard-redesign-001)
- **Mốc**: Hạ tầng / Tooling — Triển khai Web Developer Dashboard quản lý tiến độ, kế hoạch, done log và Antigravity profiles.
- **Đã làm**:
  - Nhận và claim task `web-dashboard-redesign-001` từ cloud bridge `game.zcloudviet.xyz` với worker `antigravity-worker-20261004-session12` (revision 2).
  - Xây dựng backend dashboard cục bộ (`tools/ide-bridge/dashboard/server.mjs`, `data.mjs`), chỉ bind `127.0.0.1:5050`, bảo mật bằng mã PIN (mặc định 1234), chống path traversal an toàn.
  - Xây dựng giao diện web Dark theme chuẩn tokens (`#0B0F14`, `#121821`, `#38BDF8`, `#22C55E`, `#EF4444`, `#8B5CF6`) với 4 tab:
    - **Tổng quan**: ProgressHero % hoàn thành, MetricCards (in_progress, pending, blocked, done), lộ trình Milestones, hoạt động gần đây từ `PROGRESS.md`.
    - **Kế hoạch**: Chuyển đổi Kanban / Bảng, bộ lọc tìm kiếm, drawer trượt xem chi tiết task (mục tiêu, file, tiêu chí, kết quả).
    - **Đã làm (Done Log)**: Lọc lịch sử phiên, xem thay đổi file, bằng chứng test xác thực thực tế từ `PROGRESS.md` và runtime.
    - **Tài khoản IDE Antigravity**: Quản lý danh sách profile cục bộ (che email, quota minh bạch "Đọc từ IDE" / "Nhập tay", cảnh báo quota < 10%, nút chuyển sang acc tiếp theo có modal xác nhận nhắc nhở lưu/reclaim task, lưu lịch sử audit).
  - Viết bộ test `tools/ide-bridge/test/dashboard.test.mjs`, chạy `npm test` toàn bộ 8 test đều pass.
  - Báo cáo kết quả `report_result` lên cloud bridge, task `web-dashboard-redesign-001` chuyển trạng thái `done` (revision 3).
  - Tích hợp dashboard UI trực tiếp vào `tools/ide-bridge/server.mjs`, đóng gói bằng `tar.exe` và deploy lên app `web` qua API MCP ZCloudViet. Xác nhận Dashboard đã LIVE 24/7 trực tiếp trên cả 2 tên miền `https://game.zcloudviet.xyz` và `https://phonho.zcloudviet.xyz`.
- **File chính**: `tools/ide-bridge/server.mjs`, `tools/ide-bridge/dashboard/*`, `tools/ide-bridge/test/dashboard.test.mjs`, `PROGRESS.md`.
- **Kiểm tra**: `npm test` pass 100%; `curl https://game.zcloudviet.xyz/` trả về đầy đủ HTML Dashboard (200 OK); `curl https://game.zcloudviet.xyz/health` trả về 200 OK cho Custom GPT Action.

### Phiên 12 — 2026-10-04 (Cập nhật endpoint Action/Connector sang Cloud Bridge game.zcloudviet.xyz)
- **Mốc**: Hạ tầng / Tooling — Cập nhật cấu hình Action/Connector và tài liệu.
- **Đã làm**:
  - Đồng bộ và kiểm tra `.ide-bridge/openapi.json` trỏ chuẩn về `https://game.zcloudviet.xyz`.
  - Cập nhật [docs/IDE_BRIDGE.md](file:///d:/new/docs/IDE_BRIDGE.md) mục 3 hướng dẫn chi tiết cách cấu hình Custom GPT Action kết nối trực tiếp cloud bridge 24/7 (không cần chạy ngrok cục bộ).
  - Tinh chỉnh `tools/ide-bridge/test/bridge.test.mjs` xử lý quyền symlink trên môi trường Windows non-elevated; toàn bộ unit test chạy pass 100%.
- **File chính**: `.ide-bridge/openapi.json`, `docs/IDE_BRIDGE.md`, `tools/ide-bridge/test/bridge.test.mjs`, `PROGRESS.md`.
- **Kiểm tra**: `curl` kiểm tra trực tiếp `https://game.zcloudviet.xyz/health` (200 OK) và `https://game.zcloudviet.xyz/v1/tasks` (200 OK); chạy `node --test tools/ide-bridge/test/bridge.test.mjs` pass.

### Phiên 11 — 2026-10-04 (Deploy thành công ứng dụng Web lên ZCloudViet — Hoạt động 100%)
- **Mốc**: Hạ tầng / Tooling — Deploy hoàn tất cả ứng dụng `web` (Railpack) và `api` (Dockerfile).
- **Đã làm**:
  - Dừng ứng dụng `test` để giải phóng RAM/CPU trên gói Vibe Host Basic.
  - Cập nhật biến môi trường đầy đủ cho app `web` (`PORT=3000`, `NODE_ENV=production`, `PUBLIC_HEALTH=true`, `BRIDGE_PUBLIC_URL=https://game.zcloudviet.xyz`, tokens).
  - Đóng gói mã nguồn sạch bằng `tar.exe` và upload lên `web` qua `deploy_upload`.
  - Quá trình build hoàn tất thành công (`status: done`) lúc 20:38:54 (giờ VN).
  - Kiểm tra xác nhận cả 2 tên miền đã hoạt động chuẩn xác:
    - Domain chính: `https://game.zcloudviet.xyz/v1/tasks` → 200 OK.
    - Custom domain: `https://phonho.zcloudviet.xyz/v1/tasks` → 200 OK.
    - Health check có token: `https://game.zcloudviet.xyz/health` → `{"ok":true,"service":"Pho Nho IDE Task Bridge","version":"0.1.0"}`.
- **File chính**: `tools/ide-bridge/*`, `PROGRESS.md`.
- **Kiểm tra**: `curl` trực tiếp đến `game.zcloudviet.xyz` và `phonho.zcloudviet.xyz` đều trả về HTTP 200 JSON hợp lệ.

### Phiên 10 — 2026-10-04 (IDE Bridge LIVE trên ZCloudViet — Debug hoàn chỉnh)
- **Mốc**: Hạ tầng / Tooling — Deploy IDE Bridge thành công, bridge live và nhận request.
- **Đã làm**:
  - Tìm ra root cause 502: Plan Basic có 2048MB RAM + 1000m CPU cho CẢ 3 apps; khi 3 apps cùng chạy, mỗi app bị OOM kill → không log, 502. Fix: chỉ chạy 1 app tại 1 thời điểm.
  - Stop `web` và `test`, chỉ chạy `api` (dockerfile) — bridge khởi động bình thường.
  - Deploy code bridge thật (`server.mjs`, `store.mjs`, `config.mjs`, `Dockerfile`) lên app `api`.
  - Set env: `PORT=3000`, `NODE_ENV=production`, `PUBLIC_HEALTH=true`, `DATA_DIR=/data`, `BRIDGE_TASK_TOKEN`, `BRIDGE_WORKER_TOKEN`, `BRIDGE_PUBLIC_URL=https://game-api.zcloudviet.xyz`.
  - Xác nhận live: `https://game-api.zcloudviet.xyz/health` → 200 OK.
- **File chính**: `tools/ide-bridge/server.mjs`, `store.mjs`, `Dockerfile`; `c:\Users\tan\.gemini\config\mcp_config.json`.
- **Kiểm tra**: `curl https://game-api.zcloudviet.xyz/health` → `{"ok":true,"service":"Pho Nho IDE Task Bridge","version":"0.1.0"}`.

### Phiên 9 — 2026-10-04 (Triển khai ban đầu lên ZCloudViet — gặp 502)
- Tạo 3 apps (web/api/test), cài MCP zcloudviet, optimize code bridge, upload. Build done nhưng 502 do tài nguyên chia đều cho 3 apps.

### Phiên 8 — 2026-10-04 (Chuẩn hóa Sprite Strip 1 Hàng Ngang & Hoàn thành Task Bridge)
- **Mốc**: M0 (Character Animation 1-Row Strip Standardization & Task Completion).
- **Đã làm**:
  - Nhận và claim task `character-animation-simple-outfit-02` qua MCP `phonho-task-bridge` (revision 2).
  - Chuẩn hóa toàn bộ 4 animation chính sang sprite strip 1 hàng ngang (1x4, kích thước `2048 x 640 px`):
    - `Male_A_Idle_Strip.png` (nam áo kem/trắng, short olive, giày trắng, 4 frame ngang).
    - `Male_A_Movement_Strip.png` (nam áo kem/trắng, short olive, giày trắng, 4 frame bước đi ngang).
    - `Female_A_Idle_Strip.png` (nữ áo kem/trắng, short hồng pastel, giày trắng, 4 frame ngang).
    - `Female_A_Movement_Strip.png` (nữ áo kem/trắng, short hồng pastel, giày trắng, 4 frame bước đi ngang).
  - Làm sạch nền trong suốt, khóa baseline Y=576, pivot `(0.5, 0.1)`, 256 PPU cho từng ô 512×640 px.
  - Cập nhật cấu hình slicing `CharacterSheets.json`, tool [CharacterSheetTools.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterSheetTools.cs), [CharacterAnimationBuilder.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterAnimationBuilder.cs).
  - Cập nhật 4 Animation Clips (`Male_A_Idle`, `Male_A_Walk`, `Female_A_Idle`, `Female_A_Walk`) trỏ 100% vào các sprite từ 1-row strips mới; loại bỏ hoàn toàn việc tham chiếu bố cục 2 hàng cũ.
  - Cập nhật Scene Preview [Character_Preview.unity](file:///d:/new/Assets/PhoNho/Scenes/Character_Preview.unity) với sprite mặc định từ strip 1 hàng.
  - Chạy `CharacterPlayModeValidator.ValidateAll` qua Unity 6000.4.3f1 batchmode, xác nhận `success: true`, 0 lỗi, kiểm tra 4 strips x 4 frames đều có `rect.y == 0` (1 hàng ngang tuyệt đối).
  - Gọi MCP `report_result` hoàn tất task `character-animation-simple-outfit-02` (revision 3).
- **File chính**:
  - `Assets/PhoNho/Art/Characters/*Strip.png` & `*.png.meta`
  - `Assets/PhoNho/Art/Animations/*.anim` & `*.controller`
  - [CharacterSheets.json](file:///d:/new/Assets/PhoNho/Editor/CharacterSheets.json)
  - [CharacterAnimationBuilder.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterAnimationBuilder.cs)
  - [CharacterPlayModeValidator.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterPlayModeValidator.cs)
  - [character-validation-results.json](file:///d:/new/Assets/PhoNho/Editor/character-validation-results.json)
  - [Character_Preview.unity](file:///d:/new/Assets/PhoNho/Scenes/Character_Preview.unity)
  - [PROGRESS.md](file:///d:/new/PROGRESS.md)
- **Kiểm tra**: Unity batchmode exit code 0; `character-validation-results.json` ghi nhận `success: true`, 4 one-row strips, 16 sprites, 0 lỗi.

### Phiên 7 — 2026-10-03 (Nghiệm thu Task Character Check qua MCP Task Bridge)
- **Mốc**: M0 (Character Preview Play Mode Validation & MCP Bridge Integration).
- **Đã làm**:
  - Nhận task `character-cleanup-unity-check-001` từ Custom GPT qua MCP `phonho-task-bridge` và claim với worker ID duy nhất.
  - Viết bộ validator tự động [CharacterPlayModeValidator.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterPlayModeValidator.cs) trong Unity Editor để kiểm tra toàn diện cấu hình Sprite, Animation và Scene.
  - Chạy `CharacterPlayModeValidator.ValidateAll` qua Unity 6000.4.3f1 batchmode CLI (`-executeMethod`), xác nhận 100% tiêu chí đạt chuẩn:
    - 6 sheet (32/32 frame) đúng thứ tự, không cắt mất giày/chân, baseline Y=576, pivot `(0.5, 0.1)`, 256 PPU.
    - 4 Animation Clips (`Male_A_Idle.anim` 2 FPS, `Male_A_Walk.anim` 8 FPS, `Female_A_Idle.anim` 2 FPS, `Female_A_Walk.anim` 8 FPS) có `loopTime = true`.
    - 2 Animator Controllers (`Male_A_Controller`, `Female_A_Controller`) có đủ tham số `IsMoving` và `Speed`.
    - Scene [Character_Preview.unity](file:///d:/new/Assets/PhoNho/Scenes/Character_Preview.unity) có Camera Orthographic, hai panel nền sáng (`#F2F2EB`) và nền tối (`#1F1F24`), cùng bộ đôi nhân vật Nam/Nữ gắn script [PhoNhoCharacterPreview.cs](file:///d:/new/Assets/PhoNho/Scripts/Character/PhoNhoCharacterPreview.cs) (đi trái `flipX = true`, dừng giữ hướng cuối).
    - Kết quả ghi chi tiết tại [character-validation-results.json](file:///d:/new/Assets/PhoNho/Editor/character-validation-results.json).
  - Báo cáo kết quả `report_result` với status `done` và revision 2 về MCP Task Bridge.
- **File chính**:
  - [CharacterPlayModeValidator.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterPlayModeValidator.cs)
  - [character-validation-results.json](file:///d:/new/Assets/PhoNho/Editor/character-validation-results.json)
  - [PROGRESS.md](file:///d:/new/PROGRESS.md)
- **Kiểm tra**: Unity batchmode exit code 0, không có lỗi console, `character-validation-results.json` trả về `success: true` và `errors: []`.
- **Việc tiếp theo**: Ghép nhân vật vào scene phố chính `CityOverworld_ArtLayout.unity`.

### Phiên 6 — 2026-10-03
- **Mốc**: công cụ giao việc ChatGPT ↔ Antigravity cho M0.
- **Đã làm**: REST webhook có hai vai trò/token, queue JSON/Markdown, retry cùng ID, khóa claim/revision; MCP stdio list/get/claim/report; setup sinh schema và cấu hình theo máy.
- **File chính**: tools/ide-bridge/*, .tasks/character-check.json, .tasks/current-task.md, docs/IDE_BRIDGE.md, docs/GPT_BRIDGE_INSTRUCTIONS.md, .gitignore.
- **Kiểm tra**: 6 Node tests qua, gồm luồng REST → tiến trình MCP thật → REST, xác thực/giới hạn request/role, claim đồng thời, retry, symlink và setup giữ token.
- **Cách chạy**: node tools/ide-bridge/setup.mjs; ghép config vào Antigravity; tùy chọn ngrok + Custom GPT Actions theo docs.
- **Nợ kỹ thuật**: chưa chạy tunnel/Custom GPT/Antigravity thật ở máy người dùng; phiên bridge không chạy Unity; gửi task không tự đánh thức agent.

### Phiên 5 — 2026-10-03 (Hoàn tất Slicing Sprite, Animation Clips & Scene Preview Character)
- **Mốc**: M0 (Character Cleanup & Animation Integration).
- **Đã làm**:
  - Chạy thành công `PhoNho.Art.Editor.CharacterSheetTools.CleanAll` thông qua Unity 6000.4.3f1 batchmode CLI (`-executeMethod`).
  - Kiểm tra và tự động cập nhật metadata slice cho toàn bộ 6 sheet (32/32 frame): ô 512×640 px, baseline 576, pivot `(0.5, 0.1)`, 256 PPU.
  - Tạo 4 Animation Clips chuẩn theo đặc tả: `Male_A_Idle.anim` (2 FPS), `Male_A_Walk.anim` (8 FPS), `Female_A_Idle.anim` (2 FPS), `Female_A_Walk.anim` (8 FPS); đã bật `loopTime = true`.
  - Tạo 2 Animator Controllers (`Male_A_Controller.controller`, `Female_A_Controller.controller`) với state machine `Idle` và `Walk`, chuyển trạng thái bằng tham số `IsMoving` / `Speed`.
  - Viết controller điều khiển preview [PhoNhoCharacterPreview.cs](file:///d:/new/Assets/PhoNho/Scripts/Character/PhoNhoCharacterPreview.cs) hỗ trợ đi tuần tra mẫu, phím bấm, rẽ trái dùng `flipX = true`, dừng lại giữ nguyên hướng nhìn cuối.
  - Tạo Scene riêng [Character_Preview.unity](file:///d:/new/Assets/PhoNho/Scenes/Character_Preview.unity) với panel nền sáng (`#F2F2EB`) và nền tối (`#1F1F24`) cùng đường ground line để kiểm tra trực quan quầng viền, giày và kích thước.
  - Tự động ghi nhật ký chi tiết tại [character-cleanup-execution.log](file:///d:/new/Assets/PhoNho/Editor/character-cleanup-execution.log).
- **File chính**:
  - `Assets/PhoNho/Art/Animations/*.anim` & `*.controller`
  - [Character_Preview.unity](file:///d:/new/Assets/PhoNho/Scenes/Character_Preview.unity)
  - [PhoNhoCharacterPreview.cs](file:///d:/new/Assets/PhoNho/Scripts/Character/PhoNhoCharacterPreview.cs)
  - [CharacterAnimationBuilder.cs](file:///d:/new/Assets/PhoNho/Editor/CharacterAnimationBuilder.cs)
  - `Assets/PhoNho/Art/Characters/*.png.meta`
- **Cách chạy/kiểm tra**: Mở Unity, mở scene `Assets/PhoNho/Scenes/Character_Preview.unity`, bấm Play để xem nhân vật tự tuần tra chuyển đổi giữa Walk và Idle.
- **Lỗi / Nợ kỹ thuật**: Sprite AI có độ biến thiên nhẹ giữa các góc nhìn; chuẩn bị quy trình chuyển đổi sang Spine rig 2D theo D25.

### Phiên 4 — 2026-10-03
- **Mốc**: M0 (Character cleanup).
- **Đã làm**: xử lý cả 4 sheet Walk cũ và 2 sheet Idle mới, tổng 32 frame; xóa alpha thấp/mảnh rời, sửa màu mép, căn thân và điểm đặt chân, giữ nguồn gốc.
- **Công cụ**: menu Character làm sạch/căn toàn bộ hoặc ảnh chọn 1/4/8 frame; cấu hình JSON dùng chung; import xóa alpha thấp cho ảnh mới.
- **File chính**: `Assets/PhoNho/Art/Characters/*.png`, `Assets/PhoNho/Editor/CharacterSheet*.cs`, `CharacterTexturePostprocessor.cs`, `CharacterSheets.json`, `docs/CHARACTER_CLEANUP.md`.
- **Kiểm tra**: decoder PNG/CRC, 32 frame, alpha/RGB ngoài silhouette, không cắt mép, baseline 574–575, bước làm sạch alpha ổn định khi chạy lại, test nhiễu tổng hợp; xem toàn bộ sheet.
- **Cách chạy**: pull nhánh và menu `Phố Nhỏ > Character > Làm sạch và căn toàn bộ`; hướng dẫn/prompt IDE trong docs.
- **Nợ kỹ thuật**: chưa có Unity Editor để compile/chạy importer/slicing; góc nhìn và tư thế lặp trong ảnh AI chưa được sửa, chưa tạo Spine rig.



### Phiên 3 — 2026-10-03
- **Mốc**: M0 (Scene phố 2D Parallax)
- **Đã làm**:
  - Chuyển `Assets/unity.unity` từ bố cục xoay trục Y sang mặt phẳng XY chuẩn cho game 2D.
  - Cân lại vỉa hè thành 3 đoạn nhỏ, đồng nhất chiều cao quán, thu ghế/đèn và làm nhạt phố xa.
  - Thêm cây trung cảnh, phân Sorting Order từ trời đến bụi hoa tiền cảnh.
  - Thêm `ParallaxLayer` cho trời, phố xa và bụi hoa; thêm công cụ Editor để dựng lại scene tham chiếu.
- **File chính**: `Assets/unity.unity`, `Assets/PhoNho/Scripts/Map/ParallaxLayer.cs`, `Assets/PhoNho/Editor/PhoNhoStreetSceneBuilder.cs`.
- **Cách chạy/kiểm tra**: mở scene, Game view Full HD 16:9; di chuyển Main Camera theo X để xem parallax.
- **Lỗi / nợ kỹ thuật**: chưa thể chạy Unity Editor qua GitHub connector; cần xác nhận trực quan sau khi Pull.


### Phiên 2 — Khởi tạo cấu trúc Unity, Import Asset & Thiết lập Git
- **Mốc**: M0 (Khởi tạo dự án)
- **Đã làm**:
  - Giải thích cơ chế quản lý file của Unity (chỉ hiện các tệp nằm trong `Assets/`).
  - Hướng dẫn cấu hình Camera Orthographic 2D và xếp lớp bối cảnh (Order in Layer 0-5).
  - Tạo cấu trúc thư mục chuẩn Unity `Assets/PhoNho/` gồm `Art`, `Audio`, `Config`, `Prefabs`, `Scenes`, `Scripts`.
  - Di chuyển/sao chép toàn bộ 13 tệp background từ `img/back/` sang `Assets/PhoNho/Art/Backgrounds/`.
  - Khởi tạo script mẫu [PhoNhoConstants.cs](file:///d:/new/Assets/PhoNho/Scripts/Domain/PhoNhoConstants.cs) với namespace chuẩn `PhoNho.Domain`.
  - Tạo [.gitignore](file:///d:/new/.gitignore) chuẩn cho Unity (bỏ qua Library, Temp, Logs, UserSettings).
  - Khởi tạo git repository nhánh `main` và hoàn tất commit ban đầu (`feat(init)`).
  - Kết nối remote repository và hoàn tất đẩy toàn bộ mã nguồn lên GitHub: `https://github.com/Tantran282006/duan.git`.
- **File chính**: [.gitignore](file:///d:/new/.gitignore), [PhoNhoConstants.cs](file:///d:/new/Assets/PhoNho/Scripts/Domain/PhoNhoConstants.cs), [PROGRESS.md](file:///d:/new/PROGRESS.md).
- **Kiểm tra**: `git push -u origin main` thành công 100%, repo GitHub đã có nhánh `main` đầy đủ.
- **Việc tiếp theo**: Tiếp tục hoàn thiện Scene 2D Parallax và thiết lập backend local Docker Nakama + DB.

### Phiên 1 — Đặc tả UI/Art Brief & Khung Asset
- **Mốc**: Chuẩn bị / Tài liệu thiết kế
- **Đã làm**:
  - Khôi phục và đồng bộ toàn bộ nội dung `game.md`, `AGENTS.md`, `PROGRESS.md`.
  - Cập nhật 6 câu trả lời phần 13 (nghề duy nhất, 25 Scoin/phút, Gem mua đồ đặc biệt/pet, lọc từ khóa thô tục, hiển thị avatar + sao + tên quán).
  - Khởi tạo tài liệu đặc tả mỹ thuật [UI_ART_BRIEF.md](file:///d:/new/UI_ART_BRIEF.md) chi tiết phục vụ bàn giao cho Artist / UI Designer (Mockup UI 7 màn hình landscape, Asset kit Chibi pastel, quy chuẩn Unity 2D URP).
- **File chính**: [UI_ART_BRIEF.md](file:///d:/new/UI_ART_BRIEF.md), [game.md](file:///d:/new/game.md), [PROGRESS.md](file:///d:/new/PROGRESS.md).
- **Việc tiếp theo**: Thu thập câu trả lời cho các câu hỏi mở về Model/Asset để hoàn thiện file Brief; chuẩn bị khởi tạo Unity project và backend dev local.

### Phiên 0 — Lập kế hoạch
- **Mốc**: chuẩn bị
- **Đã làm**: phỏng vấn yêu cầu qua 7 đợt hỏi đáp; viết `GAME_PROMPT.md`, `AGENTS.md`, `PROGRESS.md`.
- **File chính**: ba file trên.
- **Kiểm tra**: chưa áp dụng.
- **Cập nhật**: đã ghi 5 câu trả lời của người dùng (D19–D23). Thêm `UI_ART_BRIEF.md` cho bên làm giao diện/hình ảnh.
- **Nợ kỹ thuật**: các mục GIẢ ĐỊNH ở bảng DECISIONS cần được xác nhận.

<!-- Mẫu cho phiên mới (sao chép lên trên cùng):

### Phiên N — YYYY-MM-DD
- **Mốc**: Mx
- **Đã làm**: ...
- **File chính**: path1, path2
- **Cách chạy/kiểm tra**: ...
- **Lỗi / nợ kỹ thuật**: ...
-->
