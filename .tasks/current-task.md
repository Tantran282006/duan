# [AUTO-WORKER ASSIGNED] P0 KHÔNG NGHIỆM THU: làm lại art interior + cooking thực chất, phải qua cửa kiểm duyệt hình ảnh

Task ID: `pho-nho-p0-art-quality-rejection-20261012-001`
Status: `in_progress`
Worker: `antigravity-auto-worker-msi`
Dispatched: `2026-10-13T09:40:06.444Z`

## Mục tiêu
Người dùng chính thức TỪ CHỐI đầu ra hình ảnh ngày 12/10/2026. Ảnh nội thất: cửa và quầy ghép hình khối, tường trống, kệ vài lọ màu, bố cục chật/chồng lớp, HUD thô; ảnh nấu ăn: ba hình tròn thay chảo và thức ăn, các mảng màu giả nguyên liệu, UI tối màu bé và chữ khó đọc. Đây chỉ là placeholder/prototype, KHÔNG ĐẠT tiêu chuẩn nghệ thuật. Mục tiêu: giữ và kiểm tra logic chuyển scene/gameplay hiện có nếu dùng được, nhưng REWORK phần art/UI từ đầu bằng sprites/tilemap/bộ asset thống nhất hoặc artwork 2D thực sự, đưa game tới chất lượng vertical slice có thể trình diễn, không tiếp tục tô màu hoặc thêm vài hình tròn/chữ nhật. Nhiệm vụ này là một quality gate khẩn cấp, phối hợp với task pending pho-nho-unity-three-scenes-rebuild-20261012-001 (KHÔNG làm trùng lặp), người dùng phải được xem screenshot so sánh trước/sau và phê duyệt mỹ thuật trước khi được báo là hoàn thành. Bridge chỉ xếp hàng; không tự nhận đã sửa code khi chưa có agent thực thi.

## Files liên quan
- `Assets/Scenes/ShopInterior_HoneyBreakfast.unity`
- `Assets/Scenes/Cooking_HoneyBreakfast.unity`
- `Assets/Scenes/CityOverworld_PlayerMovement.unity`
- `Assets/PhoNho`
- `Assets/Sprites`
- `Assets/Art`
- `Assets/Tiles`
- `Assets/Prefabs`
- `Assets/UI`
- `Assets/Scripts`
- `PROJECT_STATE.md`
- `PROGRESS.md`

## Các bước thực hiện
1. BƯỚC 0: ĐỌC và đối chiếu task pending pho-nho-unity-three-scenes-rebuild-20261012-001 cùng 004/005/006/009. Không tạo scene/controller mới nếu đã có. Kiểm tra Unity asset hiện có và Git read-only; không reset hoặc xóa tài sản vô cớ. Thông báo rõ task gốc đang pending và nhiều worker in_progress nếu phát hiện nghẽn queue.
2. BƯỚC 1: ĐÁNH GIÁ screenshot bị từ chối (được mô tả trong goal): interior tường trống/đồ khối phẳng thiếu chi tiết/định tỷ lệ kém; cooking có các chảo hình tròn, nguyên liệu mảng màu, không có món ăn trông như món thật, giao diện chữ nhỏ. Dán nhãn 'placeholder / NOT ACCEPTED'. Chỉ giữ script, collider, transition đã xác minh chạy. Tuyệt đối không báo 'đạt thẩm mỹ' chỉ vì Unity compile sạch.
3. BƯỚC 2: Lập art direction NHẤT QUÁN từ asset tốt có sẵn: 2D cozy Vietnamese breakfast shop, texture/sprite có đường nét, vật liệu, bóng tiếp xúc, depth và bảng màu hài hòa. Ấn định reference sheet gồm 1 ảnh look target cho nội thất và 1 ảnh look target cho kitchen, bảng màu, đơn vị scale, player-to-furniture ratio, UI typography. Không copy art có bản quyền, không dùng sprite vuông/tròn đơn sắc để đại diện đạo cụ cuối cùng.
4. BƯỚC 3: DỰNG TRƯỚC 1 KHUNG HÌNH art-quality PROOF CHO NỘI THẤT: cửa, tường trang trí, bảng menu có chữ đọc được, kệ/bát/ly, quầy gỗ có chi tiết, góc bếp nhìn thấy được, 2-3 bộ bàn ghế, đèn ấm, cây và nhân vật rõ nét; bố cục có negative space để player di chuyển, không để mọi vật đè lên nhau. Dùng sprite/tiles thật; preview Game view độ phân giải 1920x1080 và 1280x720. Bắt buộc snapshot trước/sau, ghi rõ các sprites mới đã import. KHÔNG lao ngay vào đầy đủ gameplay khi khung hình vẫn xấu.
5. BƯỚC 4: DỰNG TRƯỚC 1 KHUNG HÌNH art-quality PROOF CHO COOKING: bếp bàn nhìn thành vật thể có chất liệu (gỗ/gạch/kim loại), chảo có cán/lòng và bóng, trứng/thịt/rau/bánh mì có sprite dễ nhận biết, đĩa và thành phẩm xuất hiện có trạng thái; theo một perspective 2D nhất quán, không trưng 3 vòng tròn rời rạc. Màn hình dành đủ diện tích cho thao tác, thanh tiến trình và recipe card dễ đọc; nút tối thiểu 44px với tiếng Việt có dấu, font đủ lớn; không để UI đè lên vật thể.
6. BƯỚC 5: Gate nghiệm thu nội bộ và báo user: gửi ảnh Game view nội thất & nấu ăn ở đúng tỉ lệ 16:9, danh sách sprite/tile thực đã sử dụng và checklist khác biệt rõ so với ảnh bị bác. Nếu tool tạo art chất lượng không có, trung thực ghi 'blocked: không có art assets/công cụ tạo art', trình bày yêu cầu asset hoặc demo bố cục có nhãn PLACEHOLDER; tuyệt đối không đánh tráo prototype thành bản final.
7. BƯỚC 6: CHỈ SAU khi có bản mỹ thuật đáng duyệt, tích hợp lại logic: Street <-> Interior <-> Cooking bằng load scene thật, spawn đúng, collision/sorting hợp lý, không nhân đôi manager. Minigame có ít nhất một công thức thật với sprite nguyên liệu và trạng thái chế biến (trước/sau), tương tác chính xác và nút/tooltip rõ ràng.
8. BƯỚC 7: Chạy compile và play-mode test Street->Interior->Cooking->Interior->Street 3 vòng; ghi kết quả thực tế, lỗi và paths screenshot. Không thể kiểm thử thì báo rõ unverified. Cập nhật PROJECT_STATE.md và PROGRESS.md, tuyệt đối không tự động git reset/clean/force-push.

## Acceptance Criteria
- Hai ảnh bị từ chối không được dùng làm minh chứng done. Các hình khối trơn, hình tròn ký hiệu chảo/nguyên liệu và UI chữ nhỏ không được coi là final-art.
- Có file hình ảnh từ Game view hiện trạng TRƯỚC và hai Game view MỚI interior, cooking 16:9 để user tự so sánh và phê duyệt; không claim approved khi chưa được user đồng ý.
- Interior rõ không gian quán ăn sáng Việt Nam: tỉ lệ player/furniture, phối cảnh, depth, texture, đạo cụ, typography nhất quán và lối đi không bị chặn bởi chồng vật thể.
- Cooking hiển thị món ăn, dụng cụ và nguyên liệu có hình dáng dễ nhận biết với sprite thực, không chỉ vòng tròn/mảng màu; có đủ các trạng thái khi tương tác và sản phẩm hoàn thành trông rõ là món ăn.
- UI tiếng Việt có dấu đọc dễ ở 1280x720, thao tác không bị nút/hint/hud che khuất. Không còn nút 'vào' bé/chữ tràn/chồng.
- Đã rà soát và phối hợp với task pending rebuild và in_progress cũ, không tạo trùng SceneManager/scene hoặc đè code mà worker khác đang sửa.
- Sau khi art được duyệt, giữ/hoàn thiện chuyển scene 2 chiều bằng scene loading thật; build settings đầy đủ, compile sạch và test runtime có bằng chứng hoặc nêu blocker.
- Nếu không đủ asset/tool để dựng đúng chất lượng, báo BLOCKED và cần art inputs thay vì đóng task done/âm thầm làm placeholder.
- Đưa báo cáo filename/path asset mới/sửa, ảnh Game view có thể mở được và kết quả xác minh thực tế.
