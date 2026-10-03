# GAME_PROMPT.md — Prompt gốc & thiết kế game "Phố Nhỏ" (tên tạm)

> Đây là nguồn sự thật duy nhất (single source of truth) về ý tưởng game.
> AI agent: KHÔNG tự ý lược bỏ, đổi nghĩa hay thu hẹp bất kỳ mục nào ở đây.
> Mọi thay đổi thiết kế phải ghi vào `PROGRESS.md` mục DECISIONS và được người dùng đồng ý.
> Ký hiệu: **[CHỐT]** = người dùng đã quyết. **[GIẢ ĐỊNH]** = agent đề xuất, người dùng chưa xác nhận, cần xác nhận trước khi hoàn thiện.

---

## 0. Vai trò của bạn

Bạn là đội kỹ sư game + backend làm game mobile **online** bằng **Unity (C#)**, có server quyền uy (server-authoritative). Hãy xây dựng game theo đặc tả dưới đây, theo từng mốc nhỏ (xem mục 12), và luôn đọc `AGENTS.md` và `PROGRESS.md` trước khi làm.

## 1. Tầm nhìn

Game mô phỏng **một thành phố** nơi mỗi người chơi chọn một nghề (sửa xe, bán trà sữa, bán bánh, bán đồ ăn sáng, v.v.), kinh doanh cửa hàng, dùng tiền kiếm được để mua **nhà, xe, trang phục, trang sức, pet**. Người chơi mua bán, đánh giá và phụ thuộc lẫn nhau trong một **nền kinh tế liên kết giữa người chơi thật**.

- Phong cách gameplay lấy cảm hứng từ game *Tiệm Trà Nhỏ* (quản lý cửa hàng dễ thương, nâng cấp, ra đơn), **nhưng gameplay khác**: có thành phố, nhiều nghề, kinh tế liên kết, đánh giá giữa người chơi, xếp hạng.
- Bản đầu: **1 thành phố**. Thiết kế dữ liệu phải sẵn sàng **nhiều thành phố** về sau (mọi bảng/khóa đều có `city_id`).
- Một thành phố chứa **nhiều cửa hàng** của nhiều người chơi khác nhau.

### Trụ cột thiết kế
1. **Nghề nghiệp đa dạng**: mỗi nghề một cách chơi riêng (về lâu dài).
2. **Người chơi phụ thuộc nhau**: chuỗi cung ứng thật giữa người chơi.
3. **Danh tiếng**: sao đánh giá của người chơi khác quyết định thành bại của quán, đỉnh cao là sao Michelin.
4. **Khoe phong cách**: điểm **bá khí** từ trang phục/trang sức/nhà/xe/pet để lên bảng xếp hạng.

## 2. Nền tảng & phong cách hiển thị [CHỐT]

- Mobile: Android + iOS. Engine: **Unity (C#)**, 2D URP.
- Hiển thị: **2D cuộn cảnh ngang (Side-scrolling)** dọc theo con phố dài, hỗ trợ Parallax background; nhân vật tự di chuyển (chạm để đi); vào cửa hàng/nhà thì chuyển sang màn hình quản lý cửa hàng (store management view).
- Đồ họa & Animation: **Chibi dễ thương màu pastel (phong cách Tiệm Trà Nhỏ)**; nhân vật và pet dùng **2D Spine (Skeletal Animation)** để animation mượt mà và dễ thay skin/trang phục modular [CHỐT].
- Âm thanh & VFX [CHỐT]: Nhạc nền **Lo-Fi / Acoustic Chill**, hiệu ứng âm thanh ấm cúng vui tươi; hiệu ứng hạt lấp lánh (**Sparkle VFX**) cho hào quang Bá Khí và vầng sáng sao Michelin 5.0★; môi trường thời tiết động Parallax (nắng, mưa bay, lá rụng) và chu kỳ ngày/đêm phố lên đèn [CHỐT].
- Tương tác online **bất đồng bộ** [CHỐT]: người chơi không cần online cùng lúc. Vào cửa hàng người khác mua bán, để lại đánh giá, mọi thứ được server xử lý. Không cần đồng bộ vị trí realtime giữa người chơi (có thể chỉ hiện "nhân vật bóng" của người chơi cùng khu, cập nhật chậm).
- Ngôn ngữ: **Tiếng Việt trước**, mọi chuỗi chữ qua hệ thống localization để thêm ngôn ngữ sau [CHỐT].

## 3. Vòng lặp chơi

1. Tạo nhân vật → **chọn nghề** (bản đầu: Bán trà sữa, Bán đồ ăn sáng, Nguyên vật liệu).
2. Mở cửa hàng với vốn khởi đầu: **200 Scoin** [CHỐT: khởi đầu thử thách tiết kiệm].
3. Cửa hàng ra đơn theo công thức/nâng cấp (**quản lý/idle** [CHỐT]): chọn công thức, nâng cấp thiết bị, chờ thời gian ra đơn, thu tiền.
4. Khách là **NPC** (lấp chỗ trống) + **người chơi thật** (đơn lớn và để lại đánh giá) [CHỐT].
5. Kiếm **Scoin** → mua nhà, xe, trang phục, trang trí, pet → tăng **bá khí**.
6. Khách hàng đánh giá → sao quán thay đổi → NPC đông/ít, mở khóa giá cao, thưởng Gem → có thể đạt **Michelin**.
7. Xếp hạng toàn thành phố.

## 4. Nghề nghiệp

### Danh sách tổng (từ ý tưởng gốc)
Sửa xe · Bán trà sữa · Bán bánh · Bán đồ ăn sáng · Nguyên vật liệu · "vv" (mở rộng thêm nghề khác về sau).

### Phạm vi bản đầu [CHỐT]
| Nghề | Bản đầu | Ghi chú |
|---|---|---|
| Bán trà sữa | Có | Mua nguyên liệu từ nghề Nguyên vật liệu |
| Bán đồ ăn sáng | Có | Mua nguyên liệu từ nghề Nguyên vật liệu |
| Nguyên vật liệu | Có | Nghề đặc thù, xem mục 4.2 |
| Sửa xe | Bản sau | Bản đầu xe hỏng do **thợ NPC** sửa; thiết kế sẵn để nghề này cắm vào |
| Bán bánh | Bản sau | Cùng khung với trà sữa/đồ ăn sáng |

**Quy tắc nghề [CHỐT]**: mỗi người chơi chỉ có **1 nghề** (1 cửa hàng). **Đổi nghề / mở thêm nghề khi đủ điều kiện** nằm ở **bản cập nhật sau**, không làm ở bản đầu, nhưng thiết kế dữ liệu không được chặn việc này.

Mỗi nghề phải được cài đặt qua **một khung nghề chung** (interface/data-driven: công thức, nguyên liệu cần, thời gian, giá, thiết bị nâng cấp) để thêm nghề mới chỉ cần thêm dữ liệu và ít code.

### 4.1 Cách chơi mỗi nghề [CHỐT: quản lý/idle]
- Chọn công thức/menu, nâng cấp thiết bị và quán, chờ thời gian ra đơn, nhận tiền.
- Cách chơi chuyên biệt cho từng nghề khác nhau về sau (tài liệu hóa mỗi nghề trong `docs/professions/<nghề>.md` khi làm).

### 4.2 Nghề Nguyên vật liệu (đặc thù) [CHỐT]
- Tạm thời **chỉ có 1 shop riêng** để nhập hàng: một màn hình "Chợ nguyên liệu". Các mặt hàng là **hàng chung** (cùng một loại như sữa, trà, đường, trứng...), nhưng **giá nhập/giá bán khác nhau tùy từng người chơi bán**.
- Người chơi nghề Nguyên vật liệu **tự đặt giá hoàn toàn tự do** [CHỐT].
- Quán trà sữa/đồ ăn sáng mở chợ, thấy cùng một mặt hàng từ nhiều nhà cung cấp với giá, tồn kho, sao đánh giá khác nhau, rồi chọn mua.
- Nhà cung cấp cũng được **đánh giá sao** như các nghề khác (xem mục 6).
- [GIẢ ĐỊNH] Nguồn hàng của nhà cung cấp: nhập từ **kho tổng NPC** với giá gốc cố định và hạn mức mỗi ngày. Kho tổng NPC cũng bán thẳng cho quán với giá cao hơn nhiều, đóng vai trò **trần giá** tự nhiên của thị trường và đảm bảo quán không bị tê liệt khi không có nhà cung cấp.
- [GIẢ ĐỊNH] Đơn nhập khi nhà cung cấp offline vẫn được xử lý tự động từ tồn kho của họ (bất đồng bộ).

### 4.3 Liên kết giữa các nghề [CHỐT, từ ý tưởng gốc]
- Nguyên vật liệu cung cấp cho quán trà sữa, quán ăn sáng (và bánh về sau).
- Chủ quán có **xe**, chạy một thời gian thì **xe hư**, phải đến **tiệm sửa xe của người chơi khác** để sửa (bản sau).
- Bản đầu: không có người chơi sửa xe → **thợ NPC** sửa, **giá đắt hơn và chậm hơn**, tiệm người chơi được ưu tiên khi có [CHỐT].
- Cơ chế hao mòn xe: độ bền giảm theo quãng đường/thời gian dùng, về 0 thì hỏng. [GIẢ ĐỊNH: thông số trong config]

## 5. Kinh tế & tiền tệ [CHỐT]

| Tiền | Kiếm | Dùng | Ghi chú |
|---|---|---|---|
| **Scoin** | Bán hàng, nhiệm vụ | Mua nguyên liệu, nâng cấp, nhà, xe, trang phục, pet | Tiền thường |
| **Gem** (đá quý) | Hiếm: nhiệm vụ, mốc sao, sự kiện | **Mua trang phục đặc biệt, pet, v.v.** [CHỐT]; công dụng khác thêm sau | Không nạp trực tiếp, kiếm chậm |
| **Tcoin** | **Chỉ nạp tiền thật** | **Chỉ thẩm mỹ**: trang phục, nhà, xe, pet đặc biệt | **Không** mua được sao, Michelin, đánh giá, lợi thế kinh doanh |

Quy tắc bất biến:
- Tcoin KHÔNG được ảnh hưởng gameplay cạnh tranh (đơn hàng, sao, doanh thu, Michelin).
- Mọi thay đổi số dư đi qua **sổ cái giao dịch (ledger)** ở server: có `idempotency_key`, ghi nguồn/đích/lý do. Client không bao giờ tự cộng/trừ tiền.
- Nạp Tcoin qua **In-App Purchase** (Apple/Google) với **xác thực receipt ở server**.
- Item mua bằng Tcoin có thể đi kèm điểm bá khí, nhưng bá khí chỉ là danh tiếng và bảng xếp hạng, không ảnh hưởng doanh thu. [GIẢ ĐỊNH, phù hợp "chỉ thẩm mỹ"]

## 6. Đánh giá, sao & Michelin [CHỐT]

### 6.1 Đánh giá
- Người chơi mua hàng ở quán rồi có thể **chấm sao và comment trực tiếp** trên trang đánh giá của quán. Hài lòng thì cộng sao; không hài lòng thì kéo sao xuống (ví dụ khách mua trà sữa không hài lòng).
- Áp dụng cho **mọi nghề** (nhà cung cấp nguyên liệu, quán ăn sáng, trà sữa, sửa xe về sau...).
- **Chỉ người đã thật sự mua** mới được đánh giá: mỗi đánh giá gắn với **một đơn hàng hoàn tất** (mỗi đơn tối đa 1 đánh giá).
- **Giới hạn số lượt đánh giá mỗi ngày** cho mỗi người chơi để chống spam (mặc định `[GIẢ ĐỊNH]` 5/ngày, trong config).
- Chủ quán có thể **báo cáo đánh giá sai/spam** để admin xét; admin có thể ẩn/xóa và đánh giá đó bị loại khỏi điểm sao.
- Comment có phản hồi của chủ quán: chưa chọn, không làm ở bản đầu.

### 6.2 Tính sao `[GIẢ ĐỊNH: công thức chỉnh được trong config]`
- Điểm sao hiển thị = trung bình có trọng số của N đánh giá gần nhất (mặc định N=100), làm mịn Bayes khi quán còn ít đánh giá, để quán mới không lên 5 sao chỉ nhờ 1 đánh giá.
- Chỉ tính đánh giá còn hiệu lực (không bị admin ẩn).

### 6.3 Sao ảnh hưởng gameplay [CHỐT]
- Sao cao → **NPC đến đông hơn**.
- Sao cao → **mở khóa mức giá bán tối đa cao hơn**.
- **Thưởng Gem theo mốc sao** (mỗi mốc thưởng 1 lần cho mỗi cửa hàng, để không farm bằng cách lên xuống).

### 6.4 Michelin [CHỐT]
- Khi quán đạt **5 sao** → nhận **1 sao Michelin** (huy hiệu hiển thị trên quán, bảng xếp hạng).
- **Mất Michelin nếu điểm sao tụt dưới 5**.
- Điều kiện kỹ thuật `[GIẢ ĐỊNH]`: "5 sao" = điểm hiển thị ≥ 4.95 (làm tròn 1 chữ số thập phân = 5.0), đủ tối thiểu số đánh giá và số người mua khác nhau (config).
- Áp dụng tương tự cho các nghề khác.

### 6.5 Chống lạm dụng
- Đã chốt: báo cáo đánh giá sai/spam + giới hạn lượt/ngày + chỉ người đã mua.
- [CHỐT] **Bộ lọc từ khóa thô tục/xúc phạm tự động**: đánh giá/comment chứa từ thô tục, cố ý tấn công hoặc đánh giá xấu bằng lời lẽ xúc phạm sẽ bị lọc (chặn không đăng hoặc ẩn). Danh sách từ khóa nằm trong config/server, cập nhật không cần phát hành lại app.
- **Khuyến nghị bổ sung (chưa chốt)**: chặn/báo cáo người dùng bởi bất kỳ ai, cách liên hệ hỗ trợ. Apple/Google thường yêu cầu app có nội dung do người dùng tạo (UGC) phải có lọc nội dung, báo cáo và chặn.
- Phát hiện gian lận: đánh giá/mua bán qua lại giữa tài khoản liên quan (cùng thiết bị/IP), tài khoản quá mới, "bán giá cực cao cho tài khoản phụ để chuyển tiền".

## 7. Bá khí & bảng xếp hạng [CHỐT]

- **Bá khí** = điểm từ trang phục và trang sức **đang mặc trên người**. Mỗi món có điểm theo độ hiếm/đặc biệt. Nhà, xe, pet cũng đóng góp điểm theo quy tắc ở config `[GIẢ ĐỊNH: mức đóng góp]`.
- Bảng xếp hạng (4 loại):
  1. **Bá khí** (trang phục, trang sức, nhà, xe, pet)
  2. **Sao và Michelin** của quán
  3. **Doanh thu/lợi nhuận**
  4. **Theo từng nghề** (mỗi nghề một bảng riêng)
- Cửa sổ thời gian `[GIẢ ĐỊNH]`: tuần + mùa + mọi thời đại. Xếp hạng tính ở server, chống gian lận.

## 8. Mua sắm & vật phẩm

- **Nhà**: nhiều bậc, mua bằng Scoin hoặc Tcoin (bản đặc biệt), đóng góp bá khí, có thể tùy biến.
- **Xe**: có độ bền, hỏng cần sửa (mục 4.3), đóng góp bá khí, có thể tăng tốc di chuyển trên bản đồ.
- **Trang phục, trang sức**: nhiều slot (đầu, thân, chân, phụ kiện...), đóng góp bá khí.
- **Pet**: trang phục cho pet, đóng góp bá khí.
- Catalog vật phẩm lưu trong **dữ liệu (JSON/ScriptableObject + bảng server)**, không hard-code.

## 9. Thành phố & bản đồ [CHỐT]

- Một thành phố, hiển thị dạng **con phố cuộn cảnh ngang (Side-scrolling Parallax 2D)**, chia thành **các khu**; mỗi khu chứa tối đa K cửa hàng/nhà của người chơi (K trong config) để mở rộng ngang được.
- Người chơi tìm và vào tiệm người khác bằng **cả hai cách**: (a) đi bộ trên bản đồ con phố, thấy nhà/tiệm người chơi cùng khu rồi bấm vào; (b) tìm qua danh sách/bảng xếp hạng/tìm kiếm rồi bấm để dịch chuyển tới.
- [CHỐT] Hiển thị người chơi khác trên bản đồ: **avatar người chơi, số sao, tên quán**. Cập nhật chậm, không cần realtime.
- Thu nhập cơ bản `[CHỐT]`: **25 Scoin/phút** ở cấp quán khởi đầu khi online; nâng cấp làm tăng con số này (config).
- Thu nhập khi offline: quán **vẫn kiếm tiền từ khách NPC nhưng ít hơn khi online**, tích lũy tối đa vài giờ rồi nhận khi mở app (mặc định `[GIẢ ĐỊNH]` 40% tốc độ online, tối đa 6 giờ, config).

## 10. Kiến trúc kỹ thuật

### Client (Unity)
- Unity LTS, C#, URP 2D, Addressables, Localization package, kiến trúc tách lớp (UI ↔ logic ↔ network), data-driven.
- Client **không tin cậy**: chỉ hiển thị và gửi ý định (intent); server tính toán.

### Backend [CHỐT]
- **Nakama (mã nguồn mở, Go) + PostgreSQL**. Lý do: có sẵn tài khoản, bảng xếp hạng, storage, RPC, lịch sự kiện, SDK Unity; PostgreSQL cho giao dịch ACID cần thiết với ledger và thị trường tự do giá.
- Server-authoritative: giá, công thức, ra đơn, đánh giá, ledger, xếp hạng đều tính ở server.
- Xử lý bất đồng bộ: đơn hàng, thu nhập offline, tính sao, tính Michelin chạy bằng job nền/lịch.
- Bảo mật: xác thực người dùng, rate limit, xác thực receipt IAP, log giao dịch kiểm toán, chống sửa memory/replay.
- Kiểm duyệt UGC (giai đoạn đầu): tập trung vào nút "Báo cáo đánh giá sai / ngôn từ xúc phạm" cho chủ quán [CHỐT].

### Mô hình dữ liệu lõi (tối thiểu, mọi bảng có `city_id`)
`players`, `shops`, `professions`, `recipes`, `items`/`catalog`, `inventory`, `orders`, `reviews`, `review_reports`, `shop_rating_state`, `ledger`, `wallets`, `leaderboard_snapshots`, `districts`, `vehicles`, `houses`, `pets`, `equipment`, `market_listings` (nhà cung cấp nguyên liệu).

### Cân bằng & cấu hình
- Mọi con số (giá gốc, tỉ lệ offline, giới hạn đánh giá, ngưỡng Michelin, hao mòn xe...) nằm trong **file config**, không hard-code.

## 11. Rủi ro cần xử lý
1. Rửa tiền / chuyển tiền giữa tài khoản qua giá tự do → giới hạn, phát hiện bất thường, log.
2. Đánh giá trả thù / chéo nhau → ràng buộc đơn hàng, báo cáo, phát hiện cụm.
3. Lạm phát Scoin → có "bể tiền" (sink): nâng cấp, thuế, phí sửa xe NPC, vật phẩm.
4. Thị trường nguyên liệu bị độc quyền → trần giá bằng kho tổng NPC.
5. Chính sách store (IAP, UGC) → tuân thủ từ đầu.
6. Mở rộng nhiều thành phố → `city_id` ở mọi nơi.

## 12. Lộ trình (mốc nhỏ để agent làm từng bước)

| Mốc | Nội dung | Kết quả kiểm chứng |
|---|---|---|
| M0 | Khởi tạo repo, Unity project, backend dev local, CI cơ bản, cấu trúc thư mục | Build chạy, đăng nhập thử |
| M1 | Tài khoản, nhân vật, ví, ledger, config loader | Cộng/trừ tiền an toàn, có test |
| M2 | Bản đồ thành phố 2D, di chuyển, khu, vào/ra cửa hàng | Đi và vào quán mẫu |
| M3 | Khung nghề + nghề Trà sữa (idle: công thức, nâng cấp, ra đơn, NPC) | Chơi trọn một vòng kiếm tiền |
| M4 | Nghề Nguyên vật liệu + Chợ nguyên liệu + kho tổng NPC; nghề Đồ ăn sáng | Quán mua nguyên liệu từ người chơi khác |
| M5 | Mua bán giữa người chơi (đơn lớn), đánh giá, sao, Michelin, báo cáo | Sao đổi theo đánh giá, Michelin bật/tắt đúng |
| M6 | Nhà, xe (hao mòn, thợ NPC), trang phục, pet, bá khí | Mặc đồ thấy bá khí tăng |
| M7 | Bảng xếp hạng 4 loại, thu nhập offline, nhiệm vụ/Gem | Bảng đúng, offline tính đúng |
| M8 | Tcoin + IAP, cửa hàng cosmetic, chống gian lận, kiểm duyệt | IAP sandbox pass |
| M9 | Cân bằng, tối ưu, closed beta | Chạy ổn với nhiều người chơi thử |
| Sau | Nghề Sửa xe, Bán bánh, thành phố mới | — |

## 13. Câu hỏi còn mở & Trạng thái hoàn thành thiết kế
- **Đã chốt toàn diện**:
  1. Nghề nghiệp: 1 người chơi/1 nghề; đổi nghề mở ở bản cập nhật tương lai.
  2. Kinh tế: 25 Scoin/phút (online); offline tích lũy tối đa 6 giờ (40% tốc độ); vốn khởi đầu 200 Scoin.
  3. Tiền tệ: Scoin (thường), Gem (đá quý mua trang phục đặc biệt & pet), Tcoin (chỉ thẩm mỹ).
  4. Đánh giá: Ràng buộc theo đơn hàng, lọc từ khóa thô tục tự động, nút báo cáo cho chủ quán.
  5. Đồ họa: Chibi pastel (phong cách *Tiệm Trà Nhỏ*), hoạt ảnh Spine 2D, bản đồ cuộn cảnh ngang Side-scrolling.
  6. Âm thanh & VFX: Lo-Fi/Acoustic chill, hiệu ứng hạt Sparkle cho Bá Khí & sao Michelin 5.0★, thời tiết động Parallax & chu kỳ ngày/đêm.
  7. Kỹ thuật: Unity 2D URP + Backend Nakama + PostgreSQL.

> Toàn bộ đặc tả bàn giao cho Designer / Artist / Spine Modeler: xem [UI_ART_BRIEF.md](file:///d:/new/UI_ART_BRIEF.md).