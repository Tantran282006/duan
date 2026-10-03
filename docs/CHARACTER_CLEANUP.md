# Character — làm sạch và căn ảnh dùng chung

Đã xử lý 6 sprite sheet, tổng 32 frame: 4 bộ Walk cũ (nam/nữ, 4 và 8 frame) và 2 bộ Idle A mới. Bộ 8 frame được giữ để tham khảo; tư thế lặp trong ảnh gốc không được tự động sửa thành một chu kỳ đi bộ khác.

## Dùng trong Unity / Antigravity

1. Pull nhánh `codex/clean-character-assets` và mở Unity 6000.4.3f1, đợi compile.
2. Chọn **Phố Nhỏ > Character > Làm sạch và căn toàn bộ**. Công cụ dựng PNG từ nguồn gốc rồi cắt sprite bằng 2D Sprite Data Provider có sẵn trong dự án.
3. Kiểm tra Sprite Editor: tên frame theo thứ tự trái → phải, trên → dưới. Xem trên nền sáng và tối trước khi đưa vào Animator.
4. Với ảnh mới: đặt PNG trong `Assets/PhoNho/Art/Characters`, chọn ảnh rồi dùng menu **Căn ảnh chọn - 4 khung (2x2)**, **8 khung (4x2)** hoặc **1 khung**. Thao tác này lưu cấu hình vào `CharacterSheets.json`; những lần sau dùng menu làm sạch toàn bộ.

Ảnh mới được xóa alpha rất thấp khi import, nhưng không tự đoán số frame. Nếu muốn thêm bố cục khác, thêm `file`, `columns`, `rows` vào `Assets/PhoNho/Editor/CharacterSheets.json` rồi chạy lại toàn bộ. Không áp dụng công cụ này cho background, icon, tờ concept nhiều nhân vật, tóc/phụ kiện rời hoặc ảnh chưa có nền trong suốt: thuật toán giữ khối liền lớn nhất của mỗi frame.

## Quy chuẩn đầu ra

- Ô frame 512 × 640 px; chiều cao nhân vật trung vị 512 px; cùng một hệ số scale cho cả sheet, giữ biến thiên tư thế.
- Căn giữa theo dải thân người, không theo tay đang vung. Mặt đất tại Y=576 tính từ đỉnh ô; pivot `(0.5, 0.1)`; 256 Pixels Per Unit.
- Tìm khoảng trống ngang gần ranh giới hàng trước khi tách frame, tránh cắt mất giày khi sheet AI lệch lưới.
- Bỏ pixel có alpha <32/255, bỏ mảnh rời ngoài nhân vật, lấy màu từ pixel phía trong cho mép bán trong suốt. Không xóa màu trắng của áo/giày và không dùng lọc màu hồng/xanh toàn ảnh.
- Bản 4 frame: 1024 × 1280 px. Bản 8 frame: 2048 × 1280 px.
- Tắt mipmaps/compression để thử art; giữ nguyên tên và spriteID khi số frame/tên đã có thể ghép tương ứng. Nếu đổi số frame phải kiểm tra lại các animation tham chiếu.

## Nguồn gốc và chạy lại

`img/character` là nguồn gốc chưa xử lý. Công cụ luôn dựng từ nguồn này để tránh co ảnh/làm mỏng viền qua nhiều lần chạy. Đối với ảnh chưa có nguồn, nó sao lưu lần đầu trước khi sửa. Nếu thay thiết kế nhưng giữ tên file, cập nhật bản trong `img/character` trước; không lấy PNG đã căn làm nguồn mới. Git giữ lịch sử bản cũ.

Công cụ Editor: `CharacterSheetPixels.cs`, `CharacterSheetTools.cs`, `CharacterTexturePostprocessor.cs`; tất cả nằm trong `Assets/PhoNho/Editor`, không thêm thư viện.

Chạy bằng Unity CLI sau khi clone:

```sh
Unity -batchmode -quit -projectPath PATH_TO_PROJECT -executeMethod PhoNho.Art.Editor.CharacterSheetTools.CleanAll -logFile character-cleanup.log
```

## Prompt cho Antigravity

```text
Đọc AGENTS.md, PROGRESS.md và docs/CHARACTER_CLEANUP.md.
Mở module Character hiện có và dùng công cụ dùng chung
PhoNho.Art.Editor.CharacterSheetTools.CleanAll để kiểm tra/căn 6 sheet.
Nguồn gốc nằm trong img/character; đầu ra nằm trong
Assets/PhoNho/Art/Characters. Không xử lý lại từ PNG đầu ra.
Không thay controller di chuyển, scene phố, collider hoặc logic mạng.
Compile Unity; sửa lỗi nếu có. Kiểm tra 32 frame trên nền sáng/tối,
không mất giày, pivot ổn định, không còn quầng và không nhảy kích thước.
Tạo/chỉnh clip Walk và Idle trong module animation hiện có:
Walk 4 frame thử 8 FPS; Idle 4 frame thử 2 FPS; Loop Time bật.
Khi đi trái dùng flipX, khi dừng giữ hướng cuối. Không xem các tư thế
trùng trong sheet cũ là bước chân mới, không gọi đây là Spine rig.
Nếu chưa có Animator/controller hãy báo thiếu và tạo bản preview riêng.
Cập nhật PROGRESS.md với kết quả thực tế và lỗi còn lại.
```

## Kiểm tra trong phiên sửa

Đã kiểm tra PNG bằng decoder có CRC, số frame/canvas, alpha thấp và pixel RGB ngoài vùng alpha, khoảng cách tới mép ô, vị trí bàn chân trên 32 frame, tính ổn định bước làm sạch alpha. Đã xem bản so sánh và tất cả khung hình xuất.

Chưa có Unity Editor trong môi trường sửa này, nên chưa compile hoặc chạy menu/Animator. Kiểm tra C# và thao tác import/slice cần được xác nhận trong IDE. Làm sạch ảnh không sửa được sự thay đổi hình dáng/góc nhìn do ảnh AI gốc; không thay thế quy trình rig Spine đã chốt.
