# PROGRESS.md — Nhật ký tiến độ "Phố Nhỏ"

> Agent: đọc **TRẠNG THÁI HIỆN TẠI** và **VIỆC TIẾP THEO** là đủ để làm tiếp. Cuối phiên phải cập nhật file này (quy trình ở `AGENTS.md` §6).

## TRẠNG THÁI HIỆN TẠI

- **Mốc hiện tại**: M0 (Khởi tạo dự án Unity & nạp Asset cơ bản)
- **Đã có**: bộ tài liệu thiết kế, cấu trúc `Assets/PhoNho/`, 13 background assets, scene phố `Assets/unity.unity` đã cân lại theo mặt phẳng XY, script parallax và công cụ dựng lại scene mẫu.
- **Chưa có**: backend dev local (Docker Nakama + PostgreSQL), code logic gameplay đầy đủ.
- **Chạy được**: mở trực tiếp `Assets/unity.unity`; hoặc dùng menu `Phố Nhỏ > Bản đồ > Tạo phố mẫu cân chỉnh` để sinh một scene tham chiếu mới.
- **Nợ kỹ thuật / lỗi đã biết**: cần kiểm tra trực quan scene ở Game view 16:9 trên Unity 6000.4.3f1; một số PNG ghế/đèn/nhà có quầng nền gốc cần xử lý art riêng nếu còn lộ.

- **Character**: Đã chuẩn hóa toàn bộ 4 animation chính (Female Idle, Female Movement, Male Idle, Male Movement) thành các sprite strip 1 hàng ngang duy nhất (`1x4`, kích thước `2048 x 640 px`, mỗi frame 512x640, baseline 576, pivot `(0.5, 0.1)`, PPU 256). Giữ chuẩn trang phục giản dị (nữ áo kem + short hồng; nam áo kem + short olive).
- **IDE bridge LIVE**: Cả `https://game.zcloudviet.xyz` (domain chính của app `web`) và `https://phonho.zcloudviet.xyz` (custom domain) cùng `https://game-api.zcloudviet.xyz` (app `api`) đều đang chạy LIVE và ổn định, phản hồi 200 OK.
- **Developer Dashboard & Quản lý Profile IDE**: Đã hoàn thiện backend cục bộ (`http://127.0.0.1:5050`) và UI Tab "Tài khoản IDE" kết nối dữ liệu thật:
  - Điều tra thực tế Antigravity: không có tool/log/context tra quota; tuyệt đối không dùng MITM hay đọc cookie/token.
  - 3 nguồn quota minh bạch: "nhập tay" (+/- và ô nhập %), "lỗi quota" (tự động nhận diện từ worker ID chứa profile_id khi gặp 429/ResourceExhausted), "ước tính theo lượt" (đếm lượt claim/report trong 5 giờ).
  - Cảnh báo dữ liệu cũ (>30 phút) và hiển thị "chưa rõ", không bịa số.
  - Toàn bộ 8/8 unit test kiểm thử vượt qua 100%.

## VIỆC TIẾP THEO (theo thứ tự)

1. M0: Đưa nhân vật mẫu vào `CityOverworld_ArtLayout.unity` (scene phố đi bộ) để kiểm tra tương quan tỉ lệ và di chuyển thực tế trên con phố.
2. M0: Hoàn thiện URP 2D, Addressables và Localization.
3. M0: Dựng backend Docker Nakama + PostgreSQL, kiểm tra kết nối; sau đó M1 ledger/RPC/unit test.

## NHẬT KÝ PHIÊN

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
