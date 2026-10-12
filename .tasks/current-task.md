# [AUTO-WORKER ASSIGNED] P0: Hoàn thiện nội thất cửa hàng Unity theo ảnh phản hồi, xác minh Git 108 file

Task ID: `unity-shop-interior-screenshot-fix-20261009-009`
Status: `in_progress`
Worker: `antigravity-auto-worker-msi`
Dispatched: `2026-10-12T12:27:24.168Z`

## Mục tiêu
Người dùng báo phần bên trong cửa hàng game Phố Nhỏ vẫn chưa hoàn thiện. Ảnh chụp Unity gửi ngày 09/10/2026 cho thấy khung CHỢ NGUYÊN LIỆU VIỆT là UI overlay trắng trên cảnh đường phố, có danh sách nguyên liệu và nút X; chưa thấy Scene nội thất thật. Cần kiểm tra và hoàn thiện gameplay vào cửa hàng, chuyển Scene thật, không chỉ bật overlay. Ảnh gốc nằm trong hội thoại ChatGPT; IDE Agent có thể không truy cập được ảnh nên dùng mô tả này làm tham chiếu và yêu cầu ảnh gốc nếu cần. Người dùng khẳng định đã commit toàn bộ 108 file; phải kiểm tra read-only Git để xác minh, không mặc định còn 108 file uncommitted.

## Files liên quan
- `Assets`
- `PROJECT_STATE.md`
- `PROGRESS.md`

## Các bước thực hiện
1. Kiểm tra scene và code Unity hiện tại, log compile, các task unity-shop-interaction-cooking-flow-004, unity-shop-real-scene-transition-005, unity-enter-shop-scene-transition-redo-006; tránh trùng lặp hoặc phá thay đổi đang làm.
2. Tái hiện lỗi theo ảnh người dùng: UI 'CHỢ NGUYÊN LIỆU VIỆT' đang phủ trên đường phố. Xác định đây là shop nguyên liệu hay lối vào quán; không nhầm hai tương tác. Đảm bảo nhấn E tại cửa quán chuyển Scene nội thất thực, có camera, nhân vật, nền, collider, cửa ra, quầy nấu và tương tác cơ bản; cửa hàng nguyên liệu nếu là chức năng riêng phải được giữ đúng chức năng.
3. Tạo hoặc hoàn thiện Scene nội thất thực và kết nối SceneManager/Build Settings, điểm spawn/return, trạng thái nhân vật; kiểm tra không chỉ thay overlay bằng overlay khác.
4. Chạy kiểm tra compile, scene references và test tương tác nếu môi trường cho phép; lưu bằng chứng screenshot Unity nội thất sau khi vào quán và báo đường dẫn.
5. Chạy chỉ các lệnh Git read-only: git status --short, git log -1 --oneline, git show --stat --oneline HEAD; xác minh lời người dùng rằng 108 file đã commit, nêu số liệu thực tế. Không commit/reset/clean/stage hay sửa lịch sử Git trong bước kiểm tra.
6. Cập nhật kết quả và những giới hạn kiểm chứng cho người dùng, không tuyên bố hoàn thành nếu chưa test được.

## Acceptance Criteria
- Có chuyển Scene thật vào nội thất khi tương tác đúng cửa quán, không chỉ UI overlay.
- Phân biệt rõ UI chợ nguyên liệu và Scene nội thất quán, không làm mất chức năng chợ.
- Không có lỗi compile do thay đổi, các scene reference hợp lệ và có bằng chứng test hoặc nêu rõ chưa test.
- Báo cáo trạng thái Git thực tế và commit mới nhất; không tự ý thao tác thay đổi Git.
- Không xóa hoặc ghi đè dữ liệu dự án không liên quan.
