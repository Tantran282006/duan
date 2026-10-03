# AGENTS.md — Quy tắc bắt buộc cho mọi AI agent làm dự án "Phố Nhỏ"

> File này chuẩn chung cho mọi agent (Claude Code, Cursor, Codex, v.v.). Nếu công cụ cần tên file khác (ví dụ `CLAUDE.md`), tạo file trỏ về file này, không nhân bản nội dung.

## 1. Thứ tự đọc khi bắt đầu phiên (tiết kiệm token)

1. Đọc `AGENTS.md` (file này).
2. Đọc `PROGRESS.md` **phần "TRẠNG THÁI HIỆN TẠI" và "VIỆC TIẾP THEO" trước**. Chỉ đọc nhật ký cũ khi cần.
3. Chỉ đọc `GAME_PROMPT.md` **đúng mục liên quan** đến việc đang làm (dùng mục lục: §4 nghề, §5 kinh tế, §6 đánh giá, §7 bá khí, §10 kỹ thuật). Không đọc cả file mỗi phiên.
4. Chỉ mở file code liên quan. **Không quét toàn bộ repo**, không cat file lớn; dùng tìm kiếm có mục tiêu.

## 2. Nguyên tắc thiết kế không được vi phạm

- **Server-authoritative**: client chỉ gửi ý định; tiền, đơn hàng, sao, Michelin, xếp hạng đều tính ở server.
- **Mọi thay đổi tiền tệ** (Scoin, Gem, Tcoin) đi qua `ledger` có `idempotency_key`. Cấm cộng/trừ số dư trực tiếp.
- **Tcoin chỉ dùng cho thẩm mỹ.** Không được viết code cho phép Tcoin mua sao, đánh giá, Michelin hay lợi thế doanh thu.
- **Đánh giá chỉ hợp lệ khi gắn với một đơn hàng hoàn tất**, mỗi đơn tối đa 1 đánh giá, có giới hạn lượt/ngày.
- **Data-driven**: công thức, giá gốc, vật phẩm, ngưỡng, tỉ lệ offline nằm trong config/dữ liệu, không hard-code.
- **Mọi bảng/khóa có `city_id`** để mở rộng nhiều thành phố.
- **Khung nghề chung**: thêm nghề mới = thêm dữ liệu + ít code, không copy-paste logic.
- **Chuỗi chữ** luôn qua localization (Tiếng Việt trước). Không để chữ cứng trong code UI.
- **Không đổi thiết kế đã [CHỐT]** trong `GAME_PROMPT.md`. Nếu thấy mâu thuẫn hay rủi ro, ghi vào `PROGRESS.md` mục OPEN QUESTIONS và hỏi người dùng, đừng tự quyết.
- Mục đánh dấu [GIẢ ĐỊNH] có thể triển khai tạm nhưng phải ghi rõ trong DECISIONS.

## 3. Quy trình làm việc mỗi phiên

1. Chọn **một mốc/nhiệm vụ nhỏ** trong "VIỆC TIẾP THEO". Không ôm nhiều mốc.
2. Lên kế hoạch ngắn (3–7 dòng) trước khi code.
3. Viết code + test. Chạy build/test; sửa tới khi qua.
4. Chỉ sửa đúng phạm vi cần thiết, không refactor lan man.
5. **Cuối phiên bắt buộc cập nhật `PROGRESS.md`** (xem mục 6). Phiên không cập nhật PROGRESS = chưa xong.

## 4. Quy ước code

- **Unity/C#**: PascalCase cho class/method, camelCase cho biến, `_camelCase` cho field private. Một class mỗi file. Namespace `PhoNho.<Module>`.
- Tách lớp: `UI` ↔ `Domain/Logic` ↔ `Network`. UI không gọi network trực tiếp.
- Không dùng `Find`/`GetComponent` trong `Update`. Dùng object pooling cho NPC, vật phẩm lặp lại.
- Async: dùng `async/await` (UniTask nếu dự án đã chọn), có xử lý timeout/lỗi mạng và trạng thái loading.
- **Server**: mọi RPC kiểm tra quyền, validate đầu vào, rate limit. Giao dịch tiền và tồn kho dùng transaction DB.
- Test: logic tiền, đánh giá/sao/Michelin, công thức tính offline, bá khí phải có unit test.
- Không commit secret, khóa API, receipt thật. Dùng file env ví dụ `*.example`.
- Không thêm thư viện mới nếu chưa ghi lý do vào DECISIONS.

## 5. Cấu trúc thư mục dự kiến

```
/AGENTS.md            quy tắc (file này)
/PROGRESS.md          nhật ký tiến độ (agent đọc đầu tiên)
/GAME_PROMPT.md       thiết kế gốc (đọc theo mục)
/docs/professions/    mỗi nghề một file khi làm
/config/              dữ liệu cân bằng (JSON)
/client/              Unity project
/server/              backend (Nakama modules / SQL migrations)
/tests/
```

## 6. Cách cập nhật PROGRESS.md cuối mỗi phiên (bắt buộc, ngắn gọn)

1. Cập nhật **TRẠNG THÁI HIỆN TẠI**: mốc đang ở, cái gì chạy được, cái gì chưa.
2. Thêm **một mục mới ở đầu NHẬT KÝ PHIÊN** theo mẫu, tối đa ~15 dòng:
   - Ngày, mốc, việc đã làm, file chính đã đổi/tạo, cách chạy/kiểm tra, lỗi/nợ kỹ thuật.
3. Viết lại **VIỆC TIẾP THEO** (3–7 việc, có thứ tự, đủ rõ để phiên sau làm ngay).
4. Ghi quyết định mới vào **DECISIONS**, câu hỏi mới vào **OPEN QUESTIONS**.
5. Khi NHẬT KÝ quá dài (>300 dòng), nén các phiên cũ thành 1–2 dòng mỗi phiên, giữ thông tin cần cho phiên sau.
6. Viết bằng tiếng Việt, ngắn, không dán code dài. Chỉ ghi đường dẫn.

## 7. Định nghĩa "xong" (Definition of Done)

- Build thành công, test liên quan qua.
- Không vi phạm mục 2.
- Có thể chạy/kiểm chứng theo mô tả trong PROGRESS.
- PROGRESS.md đã cập nhật.

## 8. Khi không chắc

Dừng và hỏi người dùng (ghi vào OPEN QUESTIONS). Không đoán những thứ ảnh hưởng đến kinh tế, tiền thật, hay đã [CHỐT]. Với chi tiết nhỏ không ảnh hưởng thiết kế, chọn phương án đơn giản nhất, ghi lại, rồi tiếp tục.