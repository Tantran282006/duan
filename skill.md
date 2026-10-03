Hướng dẫn kỹ thuật và quản lý vòng đời phát triển dự án game mobile Phố Nhỏ (Unity 2D URP + Nakama backend). Kích hoạt khi người dùng yêu cầu code, thiết kế hệ thống, review kiến trúc, xử lý logic kinh tế hoặc cập nhật tiến độ dự án Phố Nhỏ.

Instructions
Kỹ Sư Phát Triển Game Phố Nhỏ (Pho Nho Architect)
Skill hướng dẫn các quy chuẩn kiến trúc, quy trình làm việc và nguyên tắc bắt buộc khi lập trình, thiết kế hệ thống cho dự án game "Phố Nhỏ".

Khi nào sử dụng
Triển khai code các mốc tính năng (M0 - M9) cho Client Unity (C#) hoặc Backend Nakama (Go/PostgreSQL).
Thiết kế hệ thống kinh tế (Scoin, Gem, Tcoin), ledger giao dịch, thị trường chợ tự do, tính toán sao đánh giá/Michelin.
Viết unit test cho logic server-authoritative.
Cập nhật nhật ký tiến độ và quyết định kiến trúc (PROGRESS.md).
1. Nguyên Tắc Thiết Kế Bất Biến
Server-authoritative hoàn toàn: Client Unity chỉ gửi ý định (intent/action). Mọi phép tính tiền tệ, xác thực đơn hàng, cập nhật điểm sao, trao danh hiệu Michelin, xếp hạng do server Nakama + PostgreSQL xử lý.
Sổ cái Ledger giao dịch: Mọi biến động số dư (Scoin, Gem, Tcoin) bắt buộc đi qua bảng ledger có idempotency_key và chạy trong database transaction ACID. Cấm cộng/trừ số dư trực tiếp.
Phân định tiền tệ rõ ràng:
Scoin: Tiền tệ vận hành kinh doanh, mua sắm cơ bản.
Gem: Thưởng hiếm từ mốc sao/nhiệm vụ, mua trang phục đặc biệt & pet.
Tcoin: Tiền nạp IAP, chỉ dùng cho thẩm mỹ. Tuyệt đối không cho phép Tcoin mua sao, mua đánh giá, can thiệp doanh thu hoặc mua danh hiệu Michelin.
Quy tắc đánh giá quán:
Đánh giá chỉ hợp lệ khi gắn kèm một mã đơn hàng (order_id) đã hoàn tất.
Mỗi đơn hàng tối đa 1 đánh giá; giới hạn lượt đánh giá/ngày (chống spam/trả thù).
Tự động lọc từ khóa thô tục/xúc phạm.
Cấu trúc mở rộng đa thành phố: Mọi bảng cơ sở dữ liệu và khóa chính/phụ liên quan đến thực thể đều phải có trường city_id.
Data-driven & Localization: Mọi hằng số cân bằng (giá, tốc độ offline, ngưỡng sao) nằm trong file config JSON. Chuỗi hiển thị luôn đi qua localization package (tiếng Việt mặc định).
2. Quy Chuẩn Codebase
Client Unity (C#)
Kiến trúc phân lớp: UI <-> Domain/Logic <-> Network. Lớp UI không gọi trực tiếp API/Network.
Quy tắc đặt tên: PascalCase cho Class/Method/Property; camelCase cho biến cục bộ; _camelCase cho private field.
Namespace: Tuân thủ quy chuẩn PhoNho.<Module> (ví dụ: PhoNho.Economy, PhoNho.Store, PhoNho.Map).
Hiệu năng: Không gọi Find hoặc GetComponent trong hàm Update(). Áp dụng Object Pooling cho NPC, particle, floating text.
Xử lý bất đồng bộ: Sử dụng UniTask / async-await với đầy đủ CancellationToken và quản lý state Loading/Error.
Backend Nakama + PostgreSQL
Viết RPC xử lý có kiểm tra xác thực người dùng, rate limit và validation tham số nghiêm ngặt.
Giao dịch liên quan đến ví, tồn kho và khớp lệnh chợ nguyên liệu bắt buộc bọc trong SQL Transaction.
Có unit test độc lập cho logic ledger, tính điểm sao Bayesian và cơ chế offline income.
3. Quy Trình Làm Việc Mỗi Phiên (Workflow)
Khảo sát: Đọc mục TRẠNG THÁI HIỆN TẠI và VIỆC TIẾP THEO trong PROGRESS.md.
Lập kế hoạch ngắn gọn: Lên kế hoạch 3 - 5 bước cụ thể trước khi can thiệp code.
Thực thi & Test: Triển khai code trong phạm vi module được giao; viết hoặc chạy unit test tương ứng.
Bàn giao & Cập nhật PROGRESS.md:
Cập nhật nhật ký phiên mới lên đầu mục NHẬT KÝ PHIÊN.
Cập nhật trạng thái mốc hiện tại và danh sách VIỆC TIẾP THEO.
Ghi lại các quyết định mới vào DECISIONS hoặc câu hỏi mở vào OPEN QUESTIONS.