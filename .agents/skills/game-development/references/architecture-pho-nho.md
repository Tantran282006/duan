# Kiến Trúc Cốt Lõi Dự Án Phố Nhỏ

> Các nguyên tắc thiết kế bất biến và kiến trúc hệ thống bắt buộc phải tuân theo khi code dự án Phố Nhỏ.

---

## 1. Server-Authoritative Hoàn Toàn

- **Client Unity** chỉ gửi **ý định của người chơi** (intent: đặt món, giao hàng, mua nguyên liệu, chỉnh giá).
- **Server (Nakama + PostgreSQL)** tính toán và quyết định kết quả:
  - Khớp lệnh thị trường nguyên liệu.
  - Trừ tiền ví, cộng tồn kho nguyên liệu.
  - Tính điểm sao và phong tặng sao Michelin.
  - Tính toán xếp hạng doanh thu và bá khí.
- Client không được tự ý ghi đè số dư hay trạng thái kinh tế.

---

## 2. Sổ Cái Ledger & Idempotency Key

- Mọi thay đổi về tiền tệ (`Scoin`, `Gem`, `Tcoin`) **bắt buộc** đi qua bảng ledger có `idempotency_key` và chạy trong database transaction ACID.
- Tuyệt đối cấm viết code cộng/trừ trực tiếp vào cột số dư tài khoản mà không có dòng ghi chép trong ledger.
- Mỗi thao tác nạp, chi tiêu hay thưởng phải có lý do (`reason`), mã tham chiếu (`reference_id`) và timestamp.

---

## 3. Tcoin Chỉ Dùng Cho Thẩm Mỹ

- **Tcoin** nạp bằng tiền thật (IAP) chỉ được phép mua các vật phẩm mang tính trang trí, thẩm mỹ (trang phục nhân vật, màu sơn quán, skin thú cưng).
- **Cấm hoàn toàn**:
  - Không cho phép dùng Tcoin để mua sao, mua điểm đánh giá, mua danh hiệu Michelin.
  - Không cho phép Tcoin tạo lợi thế doanh thu hoặc rút ngắn thời gian sản xuất hơn người chơi khác.

---

## 4. Quy Tắc Đánh Giá Quán (UGC Rating)

- Đánh giá chỉ hợp lệ khi gắn với một mã đơn hàng (`order_id`) đã hoàn tất thực tế.
- Mỗi đơn hàng chỉ được gửi tối đa 1 đánh giá.
- Giới hạn số lượt đánh giá tối đa mỗi ngày của một tài khoản để chống hành vi gian lận hoặc phá hoại (spam review).
- Lọc tự động từ khóa thô tục/xúc phạm và cung cấp nút báo cáo vi phạm cho chủ quán.

---

## 5. Mở Rộng Đa Thành Phố & Data-Driven

- **Trường `city_id`**: Mọi bảng cơ sở dữ liệu và khóa liên quan đến người chơi, cửa hàng, giao dịch đều phải có `city_id` để sẵn sàng mở rộng nhiều thành phố.
- **Data-Driven**: Mọi công thức cân bằng, giá gốc, vật phẩm, ngưỡng Michelin, tỉ lệ thu nhập offline đều nằm trong file cấu hình JSON (`/config/`), không hardcode trong mã nguồn C# hoặc Go.
