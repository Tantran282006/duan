# PROGRESS.md — Nhật ký tiến độ "Phố Nhỏ"

> Agent: đọc **TRẠNG THÁI HIỆN TẠI** và **VIỆC TIẾP THEO** là đủ để làm tiếp. Cuối phiên phải cập nhật file này (quy trình ở `AGENTS.md` §6).

## TRẠNG THÁI HIỆN TẠI

- **Mốc hiện tại**: M0 (Khởi tạo dự án Unity & nạp Asset cơ bản)
- **Đã có**: bộ tài liệu thiết kế, cấu trúc thư mục chuẩn `Assets/PhoNho/`, 13 background assets, script parallax và công cụ dựng tự động scene phố mẫu cân chỉnh.
- **Chưa có**: backend dev local (Docker Nakama + PostgreSQL), code logic gameplay đầy đủ.
- **Chạy được**: trong Unity dùng menu `Phố Nhỏ > Bản đồ > Tạo phố mẫu cân chỉnh` để tạo `Assets/PhoNho/Scenes/CityOverworld_ArtLayout.unity`.
- **Nợ kỹ thuật / lỗi đã biết**: cần kiểm tra trực quan scene sinh ra ở Game view 16:9 trên máy có Unity 6000.4.3f1; một số PNG ghế/đèn/nhà có quầng nền gốc cần xử lý art riêng nếu còn lộ.

## VIỆC TIẾP THEO (theo thứ tự)

1. M0: chạy công cụ dựng phố mẫu, kiểm tra Game view 16:9 và tinh chỉnh tỉ lệ nếu cần.
2. M0: commit scene `CityOverworld_ArtLayout.unity` được Unity sinh ra sau khi đã duyệt trực quan.
3. M0: hoàn thiện URP 2D, Addressables và Localization.
4. M0: dựng backend dev local bằng Docker (Nakama + PostgreSQL), kiểm tra Unity kết nối và đăng nhập thử.
5. M1: bảng `players`, `wallets`, `ledger` + RPC có `idempotency_key` + unit test.

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

## OPEN QUESTIONS (cần người dùng trả lời)

- Toàn bộ các câu hỏi lớn về Thiết kế, Đồ họa, Âm thanh và Hạ tầng Backend đã được giải đáp đầy đủ!
- Sẵn sàng bước vào giai đoạn kỹ thuật M0 (Khởi tạo repo cấu trúc thư mục, Unity project 2D URP và Nakama local dev).

## NHẬT KÝ PHIÊN (mới nhất ở trên cùng)


### Phiên 3 — 2026-10-03
- **Mốc**: M0 (Scene phố 2D Parallax)
- **Đã làm**:
  - Thêm `ParallaxLayer` với hệ số riêng cho trời, phố xa và tiền cảnh.
  - Thêm công cụ Editor dựng scene mới từ các asset hiện có, không ghi đè scene đang mở.
  - Cân lại bố cục: vỉa hè thấp hơn, shop đồng đều, ghế/đèn nhỏ hơn, phố xa nhạt và thêm cây trung cảnh.
  - Scene đầu ra dùng Camera Orthographic 16:9 và lưu tại `Assets/PhoNho/Scenes/CityOverworld_ArtLayout.unity`.
- **File chính**: `Assets/PhoNho/Scripts/Map/ParallaxLayer.cs`, `Assets/PhoNho/Editor/PhoNhoStreetSceneBuilder.cs`.
- **Cách chạy/kiểm tra**: mở Unity, chọn `Phố Nhỏ > Bản đồ > Tạo phố mẫu cân chỉnh`, xem Game view Full HD 16:9.
- **Lỗi / nợ kỹ thuật**: chưa thể chạy Unity Editor trong môi trường GitHub connector; cần kiểm tra trực quan sau khi Pull.


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