# PROGRESS.md — Nhật ký tiến độ "Phố Nhỏ"

> Agent: đọc **TRẠNG THÁI HIỆN TẠI** và **VIỆC TIẾP THEO** là đủ để làm tiếp. Cuối phiên phải cập nhật file này (quy trình ở `AGENTS.md` §6).

## TRẠNG THÁI HIỆN TẠI

- **Ưu tiên 2026-10-08**: sửa lỗi bridge và kiểm tra/commit thay đổi trước khi mở rộng gameplay. Đã sửa báo cáo trên 100 file (nay tối đa 200), kết quả cũ khi nhận lại task blocked và schema MCP cho `commit: null`; 8/8 kiểm thử Node qua trên checkout GitHub. Chưa triển khai các sửa này trên máy IDE.
- **Giới hạn xác minh hiện tại**: truy cập bridge công khai trả Cloudflare 403/1010 lúc 09:29 (Asia/Saigon); chưa đọc được queue mới. 108 thay đổi do người dùng báo nằm trên máy Windows, chưa được đưa vào checkout này và chưa được kiểm tra/commit trong phiên này. Không suy ra đã hoàn tất chỉ từ trạng thái done trước đó.

- **Mốc hiện tại**: M0 (Khởi tạo dự án Unity & nạp Asset cơ bản)
- **Đã có**: bộ tài liệu thiết kế, cấu trúc `Assets/PhoNho/`, 13 background assets, scene phố `Assets/unity.unity` đã cân lại theo mặt phẳng XY, script parallax và công cụ dựng lại scene mẫu.
- **Chưa có**: backend dev local (Docker Nakama + PostgreSQL), code logic gameplay đầy đủ.
- **Chạy được**: mở trực tiếp `Assets/unity.unity`; hoặc dùng menu `Phố Nhỏ > Bản đồ > Tạo phố mẫu cân chỉnh` để sinh một scene tham chiếu mới.
- **Nợ kỹ thuật / lỗi đã biết**: cần kiểm tra trực quan scene ở Game view 16:9 trên Unity 6000.4.3f1; một số PNG ghế/đèn/nhà có quầng nền gốc cần xử lý art riêng nếu còn lộ.

- **Character**: Đã chuẩn hóa toàn bộ 4 animation chính (Female Idle, Female Movement, Male Idle, Male Movement) thành các sprite strip 1 hàng ngang duy nhất (`1x4`, kích thước `2048 x 640 px`, mỗi frame 512x640, baseline 576, pivot `(0.5, 0.1)`, PPU 256). Giữ chuẩn trang phục giản dị (nữ áo kem + short hồng; nam áo kem + short olive). Toàn bộ Animation Clips, Animator Controllers và Scene Preview `Character_Preview.unity` đã được cập nhật trỏ sang các 1-row strips mới và nghiệm thu qua `CharacterPlayModeValidator.ValidateAll` trong Unity 6000.4.3f1 batchmode với 0 lỗi.
- **Lỗi / nợ kỹ thuật còn lại**: Biến thiên nhẹ góc nhìn/chi tiết tóc do ảnh gốc AI; đáp ứng tốt cho giai đoạn prototype trước khi chuyển sang Spine rig theo D25.
- **IDE bridge**: Hoàn thành nhận việc, thực thi và báo cáo kết quả qua MCP `phonho-task-bridge` cho các task `character-cleanup-unity-check-001`, `test-01` và `character-animation-simple-outfit-02`.

## VIỆC TIẾP THEO (theo thứ tự)

1. Khôi phục truy cập bridge qua cấu hình Cloudflare của chủ domain; kiểm tra queue/worker thật trước khi giao thêm việc. Áp dụng bản sửa bridge sau khi kiểm tra xung đột với code chưa commit trên máy Windows.
2. Chạy task hiện có `git-uncommitted-audit-008`; phân loại 108 thay đổi theo task, source/asset kèm `.meta`, file sinh tự động và secret; kiểm tra diff/build/test. Task này chỉ audit, không tự đổi phạm vi thành commit.
3. Theo yêu cầu mới ngày 2026-10-08, commit các nhóm đã kiểm tra trên máy IDE, báo SHA và số file còn lại; giữ nguyên phần chưa đạt kiểm thử và ghi rõ lý do. Kiểm tra task chuyển cảnh `005`/`006` để giữ bản redo `006`, tránh chạy trùng.
4. M0: kiểm thử nhân vật, nền đường và chuyển vào quán trong Unity; ghi bằng chứng/ảnh thật và nghiệm thu trước khi đóng task. Sau đó tiếp tục URP 2D, Addressables, Localization và backend.

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

### Phiên 9 — 2026-10-08 (Sửa lỗi báo cáo bridge, ưu tiên commit)
- **Mốc**: sửa lỗi điều phối/task trước gameplay.
- **Đã làm**: tăng giới hạn báo cáo từ 100 lên 200 đường dẫn, giữ giới hạn 24 KiB; đồng bộ OpenAPI/MCP và chấp nhận `commit: null` trong schema MCP.
- **Đã làm**: nhận lại task blocked xóa kết quả cũ; retry claim đang chạy vẫn giữ revision.
- **File chính**: `tools/ide-bridge/store.mjs`, `openapi.mjs`, `mcp.mjs`, `test/bridge.test.mjs`, `docs/IDE_BRIDGE.md`, `PROGRESS.md`.
- **Kiểm tra**: tái hiện lỗi báo cáo 108 file và kết quả cũ trước sửa; sau sửa `node --test tools/ide-bridge/test/bridge.test.mjs` đạt 8/8, gồm REST → MCP thật → REST với 108 file và `commit: null`.
- **Giới hạn**: không có Unity Editor hoặc 108 thay đổi Windows ở checkout này; bridge vẫn bị Cloudflare chặn. Chưa tuyên bố task trên máy IDE hoàn tất hoặc đã commit 108 file.

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
