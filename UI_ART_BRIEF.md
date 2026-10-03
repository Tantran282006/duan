# TÀI LIỆU ĐẶC TẢ GIAO DIỆN & BỘ ASSET (UI/ART SPECIFICATION BRIEF)
## DỰ ÁN: "PHỐ NHỎ" (COZY TOWN BUSINESS SIM)

> **Mục đích tài liệu**: Đây là bản đặc tả bàn giao cho Designer / Artist / 3D-2D Modeler để triển khai:
> 1. Thiết kế bản mẫu giao diện người dùng (**UI/UX Mockups**).
> 2. Thiết kế và xuất bộ tài nguyên đồ họa (**Asset / Sprites / Model Kit**) sẵn sàng tích hợp vào Unity (2D URP).
> 
> *Tài liệu đồng bộ 100% với tài liệu logic kỹ thuật `game.md` và tiến độ `PROGRESS.md`.*

---

## 1. THÔNG TIN CHUNG & ĐỊNH HƯỚNG NGHỆ THUẬT (ART DIRECTION)

| Hạng mục | Thông số đặc tả | Ghi chú |
|---|---|---|
| **Tên dự án** | Phố Nhỏ (Tên tạm thời) | Game mô phỏng kinh doanh & xã hội ấm cúng |
| **Nền tảng** | Mobile (iOS & Android) | Engine: Unity 2D (URP) |
| **Hướng màn hình** | **Ngang (Landscape)** | Tỉ lệ cơ sở: **16:9** (1920x1080 làm chuẩn thiết kế). An toàn (Safe Area) co giãn cho màn hình dài 20:9 và máy tính bảng iPad 4:3. |
| **Góc nhìn (Camera)** | **Cuộn cảnh ngang (Side-scrolling Parallax 2D)** | Con phố chạy dài từ trái sang phải, nhiều lớp nền (Parallax layers: vỉa hè, cửa hàng, cây cối, bầu trời mây pastel). |
| **Định dạng Animation** | **2D Spine (Skeletal Animation)** | Nhân vật, thú cưng, chi tiết chuyển động dùng khung xương Spine 2D để chuyển động cực kỳ mượt mà, dễ gắn trang bị dạng skin/slot. |
| **Phong cách nghệ thuật** | **Chibi dễ thương, đường nét mềm mại bo tròn** | Nhân vật đầu to thân nhỏ tỉ lệ 1:1.5 đến 1:2, mắt to biểu cảm ấm áp, không góc cạnh thô ráp hay gai góc. |
| **Bảng màu chủ đạo (Palette)** | **Tone Pastel nhẹ nhàng, ấm áp, thư giãn** | Màu kem bơ (#FFF8E7), hồng phấn pastel (#FFD1DC), xanh bạc hà (#C1E7E3), vàng mật ong (#FDF0CD), gỗ ấm (#D7B899), xám khói nhẹ (#F4F4F6). Tránh màu neon chói gắt. |
| **Tâm trạng / Cảm xúc (Vibe)** | Healing, Cozy, Chill, Sung túc nhẹ nhàng | Người chơi cảm nhận được sự nhộn nhịp nhưng thanh bình của một con phố nhỏ đầy sức sống. |

---

## 2. DANH MỤC MOCKUP UI CẦN THIẾT KẾ (UI/UX WIREFRAMES & SCREENS)

Bộ Mockup UI cần được thiết kế trên Figma / Photoshop theo layout màn hình ngang (1920x1080), bao gồm 7 màn hình / luồng chính sau:

```
[Bản đồ thành phố (Overworld HUD)]
   ├── Vào tiệm ────────► [Màn hình Quản lý Quán (Nội thất)]
   ├── Mở chợ ──────────► [Chợ Nguyên Vật Liệu (B2B Marketplace)]
   ├── Bấm vào tiệm khác ─► [Trang Hồ sơ Quán & Đánh giá Sao / Michelin]
   ├── Thanh công cụ HUD ─► [Tủ đồ & Shop Thời trang - Bá Khí]
   ├── Nút Xếp hạng ────► [Bảng Xếp Hạng 4 Tab]
   └── Mở lại game ─────► [Popup Thu Nhập Offline]
```

### 2.1. Màn hình 1: Bản đồ phố & HUD chính (City Overworld Map)
- **Khu vực hiển thị map**: Con phố lát đá mềm, vỉa hè có cây xanh, các ô đất cửa hàng san sát nhau.
- **Hiển thị cửa hàng người chơi khác**:
  - Biển hiệu nổi bật trên nóc hoặc trước quán gồm: **Avatar chủ quán (khung tròn chibi) + Tên quán + Số sao vàng (VD: 4.8★) + Huy hiệu sao Michelin (nếu quán 5.0★)**.
  - Nhân vật của người chơi khác hiển thị dưới dạng bóng mờ dễ thương lướt qua trên đường phố.
- **Top Bar (Thanh tài nguyên trên cùng)**:
  - Widget Scoin (Tiền vàng thường) kèm nút `+`.
  - Widget Gem (Đá quý hồng/tím) kèm nút `+`.
  - Widget Tcoin (Tiền nạp xanh ngọc) kèm nút nạp IAP.
  - Avatar nhân vật chính + Level / Danh hiệu.
- **Bottom / Side Bar (Thanh công cụ điều hướng)**:
  - Nút "Quán của tôi" (vào thẳng tiệm).
  - Nút "Chợ nguyên liệu".
  - Nút "Bảng xếp hạng".
  - Nút "Tủ đồ / Thời trang".
  - Nút "Nhiệm vụ & Sự kiện".
  - Nút "Cài đặt" (Settings).

### 2.2. Màn hình 2: Bên trong Cửa hàng Quản lý (Store Interior Management)
*Áp dụng bản đầu cho: Quán Trà Sữa & Quán Ăn Sáng*
- **Khu vực tương tác**:
  - Quầy pha chế / nấu nướng, kệ chứa nguyên liệu.
  - Bàn ghế dành cho khách ngồi thưởng thức.
  - Khách NPC tí hon đứng xếp hàng gọi món, có bong bóng suy nghĩ (bubble chat) hiện icon món muốn mua.
- **Thanh tiến độ & Doanh thu**:
  - Đồng hồ hiển thị tốc độ kiếm tiền cơ bản: **25 Scoin/phút** (có hiệu ứng cộng tiền nhảy số vui nhộn).
  - Nút "Nâng cấp thiết bị" (hiện danh sách máy móc: Máy làm đá, Máy đóng nắp ly, Bình lắc... kèm chi phí Scoin và hiệu suất tăng).
  - Khay hiển thị tồn kho nguyên liệu (Sữa, Trà, Đường, Trân châu...). Khi sắp hết sẽ có biểu tượng cảnh báo màu đỏ/cam.

### 2.3. Màn hình 3: Chợ Nguyên Vật Liệu (Wholesale B2B Market)
- Dành cho các quán trà sữa và đồ ăn sáng vào mua nguyên liệu sỉ:
- **Danh sách mặt hàng chung**: Sữa tươi, Trà đen, Đường phèn, Bột mì, Trứng gà...
- **Bộ lọc so sánh nhà cung cấp (Người chơi nghề Nguyên vật liệu)**:
  - Cột 1: Tên nhà cung cấp + Avatar + Số sao uy tín.
  - Cột 2: Đơn giá do người bán tự đặt (VD: 12 Scoin/đơn vị).
  - Cột 3: Số lượng tồn kho sẵn có.
  - Cột 4: Nút "Mua sỉ" / "Nhập hàng".
- **Tab nguồn hàng khẩn cấp: "Kho Tổng Thành Phố (NPC)"**:
  - Luôn có sẵn hàng nhưng giá trần đắt hơn (đảm bảo người chơi không bao giờ bị nghẽn game khi thiếu người bán).

### 2.4. Màn hình 4: Hồ sơ Quán & Hệ thống Đánh giá - Sao Michelin
- **Header thông tin quán**:
  - Tên quán, Tên chủ tiệm, Nghề nghiệp.
  - Điểm sao trung bình lớn (VD: **4.9 ★ / 5.0** dựa trên các đánh giá gần nhất).
  - **Huy hiệu Michelin 1 sao lấp lánh** (nếu đạt chuẩn 5.0 sao) hoặc thanh tiến độ phấn đấu đạt Michelin.
- **Tab Đánh giá từ khách hàng thực tế**:
  - Danh sách thẻ nhận xét: Avatar người mua, Tên người mua, Số sao chấm (1-5 sao), Nội dung bình luận, Ngày mua.
  - Nút cờ nhỏ: "Báo cáo đánh giá vi phạm / từ ngữ xúc phạm" (gửi admin xét duyệt).
- **Popup Chấm Sao (dành cho người vừa mua hàng)**:
  - 5 ngôi sao tương tác (chạm để chọn 1 đến 5 sao).
  - Ô nhập nhận xét ngắn (tích hợp ghi chú: "Từ ngữ thô tục sẽ tự động bị loại bỏ").

### 2.5. Màn hình 5: Tủ Đồ Thời Trang & Điểm Bá Khí (Wardrobe & Style)
- **Khu vực Preview nhân vật**: Model chibi đứng trên bục xoay nhẹ, đổi đồ thời gian thực.
- **Hệ thống hiển thị Điểm Bá Khí**:
  - Vòng hào quang nhỏ xinh quanh nhân vật khi điểm Bá Khí cao.
  - Chỉ số tổng: "Bá Khí: 1,450 điểm" (gồm điểm từ Quần áo + Trang sức + Xe đang dùng + Nhà sở hữu + Pet cưng).
- **Phân tab trang bị**:
  - Tab 1: Mũ / Nơ / Kiểu tóc.
  - Tab 2: Quần áo / Váy.
  - Tab 3: Giày / Tất.
  - Tab 4: Trang sức / Phụ kiện cầm tay.
  - Tab 5: Xe cộ (Xe đạp, Vespa, Ô tô bọ).
  - Tab 6: Thú cưng (Pet).
- **Thẻ vật phẩm**: Thể hiện khung màu theo độ hiếm (Thường: Trắng/Xanh lá, Hiếm: Tím, Đặc biệt: Vàng óng). Giá mua hiển thị rõ bằng Scoin, Gem hoặc Tcoin.

### 2.6. Màn hình 6: Bảng Xếp Hạng Thành Phố (Leaderboards)
- Gồm 4 tab rõ ràng:
  1. **Top Bá Khí**: Xếp theo tổng điểm phục trang, xe cộ, nhà cửa, pet.
  2. **Top Michelin & Sao Quán**: Vinh danh các cửa hàng 5 sao và danh hiệu Michelin.
  3. **Top Doanh Thu Thành Phố**: Xếp theo Scoin kiếm được trong tuần/mùa.
  4. **Top Theo Nghề**: Bộ lọc riêng cho Nghề Trà Sữa, Nghề Đồ Ăn Sáng, Nghề Nguyên Vật Liệu.
- Thiết kế bục vinh quang Top 1, Top 2, Top 3 với vương miện và cúp chibi siêu dễ thương. Vị trí của người chơi hiện cố định ở thanh đáy để tiện theo dõi thứ hạng cá nhân.

### 2.7. Màn hình 7: Popup Thu Nhập Offline ("Chào mừng bạn trở lại!")
- Xuất hiện khi người chơi mở lại game sau một khoảng thời gian:
- Hình ảnh minh họa: Nhân vật chibi đang ngủ ngon hoặc quán tự động phục vụ các chú mèo NPC.
- Thống kê:
  - Thời gian đã offline (VD: 04 giờ 30 phút / tối đa 06 giờ tích lũy).
  - Số Scoin kiếm được từ khách NPC (tính theo tỉ lệ 40% của 25 Scoin/phút).
  - Nút bấm lớn hiệu ứng rực rỡ: "Thu nhận Scoin".

---

## 3. DANH MỤC BỘ ASSET & MODEL CẦN THỰC HIỆN (SPRITE & ASSET KIT)

Toàn bộ asset được cắt rời thành các file spritesheet hoặc file PNG riêng biệt, viền mượt không răng cưa.

### 3.1. Nhân vật & Trang phục (Spine 2D Skeletal Animation & Modular Skins)
- **Cấu trúc Khung xương Spine (Skeleton Rig)**:
  - Tỉ lệ: Chiều cao khoảng 128px đến 192px trên màn hình Landscape.
  - Hướng nhìn (Facing): Nhân vật nhìn nghiêng 3/4 mặt trước, hỗ trợ Flip X để đi sang trái / phải trên con phố ngang.
  - Các Spine Animation cơ bản cần thiết kế:
    + `idle`: Nhịp thở nhấp nhô nhẹ nhàng, chớp mắt tự nhiên.
    + `walk`: Đi bộ lon ton dễ thương dọc theo con phố.
    + `serve_drink`: Động tác bưng ly trà sữa / bê khay đồ ăn phục vụ khách.
    + `happy_cheer`: Nhảy cẫng vẫy tay khi nhận tiền thưởng hoặc đạt điểm sao cao.
- **Hệ thống Slot & Skin đính kèm (Spine Slots / Skins)**:
  - Tách thành các attachment slot chuẩn để thay đổi trang bị động mà không làm đứt animation:
    + Slot `slot_hair_front`, `slot_hair_back`: 06 Kiểu tóc (3 nam, 3 nữ, tone pastel).
    + Slot `slot_face`, `slot_eyes`, `slot_mouth`: Biểu cảm vui vẻ, ngạc nhiên, mỉm cười.
    + Slot `slot_outfit`: 08 Bộ trang phục (Tạp dề pha chế, Váy yếm pastel, Đồng phục học sinh, Hoodie rộng, Đồ ngủ thú bông, Đồ dạ tiệc Gem, Set quý tộc Tcoin).
    + Slot `slot_accessory_head`: Mũ beret, Nơ cài, Băng đô tai mèo.
    + Slot `slot_hand_item`: Ly trà sữa trân châu, bánh mì mini, túi xách.
    + Slot `slot_shoes`: Giày sneaker nhỏ, giày búp bê, dép bông.

### 3.2. Công trình & Ngoại thất Kiến trúc (Town Buildings)
- **Mặt tiền cửa hàng (Store Fronts - Kích thước chuẩn khoảng 256x256 hoặc 384x256 px)**:
  - **Quán Trà Sữa (Boba Shop)**: Cửa kính mái hiên sọc hồng pastel, biển hiệu hình cốc trà sữa trân châu, chậu cây nhỏ trước cửa.
  - **Quán Đồ Ăn Sáng (Breakfast Diner)**: Mái hiên vàng mật ong, biển hiệu bánh mì ốp la thơm phức, bàn ghế ngoài trời nhỏ xinh.
  - **Tiệm Nguyên Vật Liệu (Wholesale Store)**: Kiểu tiệm tạp hóa cổ điển, có các bao tải bột, thùng sữa gỗ xinh xắn xếp gọn gàng.
  - **Tiệm Sửa Xe (Garage - Chuẩn bị sẵn cho bản sau)**: Biển hiệu cờ lê vặn ốc, bình xăng mini pastel.
  - **Nhà ở người chơi**: 3 cấp độ (Cấp 1: Nhà nhỏ ấm áp; Cấp 2: Nhà 2 tầng ban công hoa; Cấp 3: Biệt thự mini có sân vườn hồ bơi).
- **Biển hiệu thông tin quán (Overhead Shop Sign Prefab)**:
  - Khung biển treo: Có slot hiển thị ảnh avatar tròn, text tên quán, cụm 5 icon sao nhỏ, và slot gắn huy hiệu Michelin.

### 3.3. Nội thất & Thiết bị Cửa hàng (Interior Props & Machines)
- **Quầy pha chế & nấu ăn**: Mặt đá cẩm thạch trắng viền gỗ ấm.
- **Máy móc cửa hàng**:
  - Máy làm đá viên (mini cube ice maker).
  - Bình shaker lắc trà sữa tự động.
  - Nồi nấu trân châu (nắp kính thấy trân châu đen/hoàng kim).
  - Máy dập nắp ly trà sữa.
  - Chảo rán đồ ăn sáng & lò nướng bánh mì mini.
- **Nội thất trang trí**:
  - Bàn gỗ tròn + 2 ghế bọc nệm pastel.
  - Quầy thu ngân kèm máy POS bấm tít tít.
  - Kệ sách, cây trầu bà, gương toàn thân, đèn thả trần ánh sáng vàng ấm.

### 3.4. Phương tiện Di chuyển (Vehicles)
Mỗi xe cần vẽ 2 trạng thái: **Trạng thái Mới/Bình thường** và **Trạng thái Hỏng hóc** (bốc khói hoạt hình đáng yêu / lốp xẹp).
- Xe đạp dạo phố có giỏ hoa phía trước (Cơ bản).
- Xe máy Vespa màu xanh mint / hồng phấn (Trung cấp).
- Xe hơi con bọ (Beetle Car) cổ điển bo tròn màu vàng kem (Cao cấp).

### 3.5. Thú Cưng (Pets)
- 04 loài pet ban đầu:
  1. Mèo Anh lông ngắn màu xám mũm mĩm.
  2. Cún Shiba vàng tròn xoe hay cười.
  3. Thỏ trắng tai cụp.
  4. Bé vịt vàng mini đội lá sen.
- Animation: Đi lon ton đi theo sau người chơi, vẫy đuôi/ngủ gật.

### 3.6. Hệ thống Icon & UI Kit (Buttons, Badges, Icons)
- **Tiền tệ (Currency Icons)**:
  - `ico_scoin.png`: Đồng xu tròn vàng óng có in hình mầm cây/lá trà nhỏ.
  - `ico_gem.png`: Viên đá quý giác cắt trái tim/kim cương màu hồng tím phát sáng nhẹ.
  - `ico_tcoin.png`: Đồng tiền ngọc bích bạch kim cao cấp có hiệu ứng lấp lánh (Sparkle).
- **Hệ thống Đánh giá**:
  - `ico_star_active.png`: Sao vàng đậm rực rỡ bo góc mềm.
  - `ico_star_inactive.png`: Sao màu xám pastel nhẹ.
  - `ico_michelin_badge.png`: Huy hiệu sao Michelin 5 sao thiết kế cách điệu hoa tuyết vàng kim sang trọng, đẳng cấp.
- **Thành phần UI chung**:
  - Khung Popup Modal (bo tròn góc 24px, đổ bóng mềm, viền kem pastel).
  - Button 4 trạng thái (Normal, Hover, Pressed, Disabled) với các màu: Xanh pastel (Confirm), Đỏ hồng mềm (Cancel/Report), Vàng ấm (Action/Shop).
  - Thanh cuộn (Scrollbar) mảnh bo tròn.

---

## 4. HỆ THỐNG ÂM THANH (AUDIO & SFX SPECIFICATION)

Game hướng đến trải nghiệm ấm áp, thư giãn (Cozy & Healing). Mọi âm thanh đều được tinh chỉnh âm sắc tròn, êm tai, không gắt.

### 4.1. Nhạc nền (Background Music - BGM)
- **Phong cách chủ đạo**: **Acoustic Guitar, Piano êm dịu, Lo-Fi Chill Hop** với nhịp điệu chậm rãi (khoảng 70–85 BPM).
- **Các track BGM cơ bản**:
  - `bgm_town_day`: Nhạc dạo phố ban ngày trong trẻo, vui tươi, tiếng chim hót xa xăm.
  - `bgm_town_night`: Nhạc ban đêm sâu lắng, êm dịu, tiếng dế đêm và đàn piano du dương.
  - `bgm_shop_interior`: Nhạc không gian quán trà sữa/đồ ăn sáng ấm cúng, có tiếng thìa cốc chạm khẽ tạo cảm giác sinh động.

### 4.2. Hiệu ứng âm thanh (Sound Effects - SFX)
- **Âm thanh UI**:
  - `sfx_btn_click`: Tiếng gõ gỗ mềm ("pop" / "tap") dễ chịu.
  - `sfx_coin_collect`: Tiếng leng keng tiền xu vàng nhẹ nhàng khi thu nhận Scoin (`+25 Scoin/phút`).
  - `sfx_gem_reward`: Tiếng chuông gió ngân nga lấp lánh (chime) khi nhận thưởng Đá quý Gem.
  - `sfx_star_pop`: Tiếng nổ bóng nước vui nhộn khi khách chấm sao đánh giá.
  - `sfx_michelin_fanfare`: Đoạn jingle ngắn sang trọng, hân hoan khi quán vinh dự đón nhận sao Michelin 5.0★.
- **Âm thanh Tương tác & Gameplay**:
  - `sfx_tea_shaker`: Tiếng bình lắc shaker trà sữa lắc đều rộn rã.
  - `sfx_cup_sealer`: Tiếng máy ép nắp ly kêu "tách" dứt khoát.
  - `sfx_cooking_sizzle`: Tiếng xèo xèo áp chảo bánh mì ốp la thơm ngon.
  - `sfx_footsteps`: Tiếng bước chân lách cách đáng yêu của nhân vật Chibi trên đường phố.
  - `sfx_car_horn`: Tiếng còi xe píp píp ngộ nghĩnh của xe Vespa và ô tô bọ.
  - `sfx_pet_cute`: Tiếng kêu nũng nịu của mèo/cún khi được vuốt ve.

---

## 5. HIỆU ỨNG HÌNH ẢNH ĐẶC BIỆT & THỜI TIẾT (VFX & ENVIRONMENT)

### 5.1. Hiệu ứng Kỹ xảo (VFX & Particle Systems)
- **Hào quang Bá Khí (Bá Khí Aura)**:
  - Khi nhân vật có điểm Bá Khí cao, xuất hiện dải hạt sáng lấp lánh màu pastel (Sparkles) bay lượn nhẹ quanh người.
- **Hào quang Michelin (Michelin Golden Glow)**:
  - Biển hiệu quán 5.0★ Michelin tỏa ra ánh kim vàng óng vương giả, thỉnh thoảng phát ra tia sao lấp lánh thu hút ánh nhìn người qua đường.
- **Hiệu ứng thu hoạch Scoin**:
  - Các đồng xu vàng bắn tung lên thành chùm nhỏ rồi bay vút về thanh hiển thị tiền ở Top Bar.
- **Bong bóng cảm xúc NPC (Mood Bubbles)**:
  - Biểu tượng trái tim hồng nảy lên khi khách hài lòng chấm 5 sao; biểu tượng giọt mồ hôi / mây xám khi quán hết nguyên liệu.

### 5.2. Môi trường & Hệ thống Thời tiết (Dynamic Weather System)
- **Lớp phủ thời tiết Parallax (Weather Overlay)**:
  - **Trời nắng trong veo**: Những vệt nắng vàng mềm rọi nghiêng qua hàng cây, bóng nắng rung rinh trên mặt đường.
  - **Mưa lất phất lãng mạn**: Hạt mưa rơi nghiêng mỏng manh, tạo những gợn sóng nước nhỏ tí xíu trên mặt đường (kèm hiệu ứng nhân vật Chibi bung ô nhỏ đi dạo).
  - **Lá bay theo gió**: Cánh hoa anh đào hồng hoặc lá phong vàng bay là đà qua màn hình theo hướng gió thổi.
- **Chu kỳ Thời gian Ngày / Đêm**:
  - Buổi chiều hoàng hôn ngả sắc cam hồng pastel ấm áp.
  - Buổi tối con phố lên đèn: Đèn đường, đèn lồng và biển hiệu quán tự động phát sáng lung linh.

---

## 6. QUY CHUẨN KỸ THUẬT BÀN GIAO CHO UNITY (TECH ART SPECS)

1. **Định dạng file giao**:
   - File thiết kế gốc: **Figma link (hoặc file `.fig`) / Adobe Photoshop (`.psd`)** có đặt tên layer và nhóm thư mục khoa học.
   - File xuất asset tĩnh / UI: **PNG trong suốt 32-bit (RGBA)**.
   - File xuất hoạt ảnh nhân vật / Pet: **Bộ xuất Spine 2D chuẩn Unity Spine Runtime** gồm:
     + File dữ liệu khung xương: `.skel` (binary) hoặc `.json`.
     + File texture atlas: `.atlas.txt` (hoặc `.atlas`).
     + File ảnh atlas textures: `.png` (kích thước lũy thừa của 2: 1024x1024 hoặc 2048x2048).
   - File Audio: **WAV hoặc OGG (44.1 kHz, 16-bit)** chuẩn nén game mobile.
2. **Quy chuẩn kích thước & Tỉ lệ (Grid & Resolution)**:
   - UI Canvas Base Resolution: **1920 x 1080 px** (Canvas Scaler: *Scale With Screen Size, Match Width or Height = 0.5*).
   - Thiết kế UI Frame/Panel hỗ trợ **9-Slicing** (cắt 9 góc) trong Unity để co giãn linh hoạt mà không méo viền bo góc.
   - PPU (Pixels Per Unit) cho Sprites môi trường / công trình trong thế giới game: Chuẩn **100 PPU** (hoặc **64 PPU**).
3. **Quy tắc đặt tên file (Naming Convention)**:
   - UI Buttons: `ui_btn_[tên_chức_năng]_[normal/hover/pressed/disabled].png`
   - UI Panels/Frames: `ui_panel_[tên_khung].png`
   - Icon: `ui_ico_[tên_icon].png`
   - Môi trường/Nhà: `spr_building_[loại]_[cấp_độ].png`
   - Spine Nhân vật: `spine_char_[nam/nu]_[skel/atlas/png]`
   - Xe: `spr_veh_[tên_xe]_[normal/broken].png`
   - Spine Pet: `spine_pet_[loài]_[skel/atlas/png]`
   - VFX Sprites: `vfx_[tên_hiệu_ứng]_[frame].png`
   - Audio: `bgm_[tên_track].ogg`, `sfx_[tên_hiệu_ứng].wav`

---

## 7. CÁC THÔNG SỐ ĐÃ CHỐT TOÀN DIỆN (FINAL DECISIONS LOG)

- **Art Style**: Chibi dễ thương, đường nét bo tròn, bảng màu pastel ấm áp (phong cách *Tiệm Trà Nhỏ*).
- **Màn hình**: Ngang (Landscape 16:9, base 1920x1080).
- **Bản đồ thành phố**: Cuộn cảnh ngang (Side-scrolling Parallax 2D) đa tầng lớp.
- **Hoạt ảnh Nhân vật & Pet**: Spine 2D Skeletal Animation (khung xương + modular skin slots).
- **Âm thanh (Audio)**: BGM Acoustic/Lo-fi chill + SFX êm dịu, vui nhộn đặc trưng của quán trà sữa và phố phường.
- **Hiệu ứng đặc biệt (VFX)**: Hạt sáng lấp lánh (Sparkle particles) cho hào quang Bá Khí và vầng sáng sao Michelin 5.0★.
- **Môi trường & Thời tiết**: Hiệu ứng thời tiết nhẹ (nắng, mưa lất phất kèm ô che, lá bay) + chu kỳ ngày/đêm phố lên đèn.
- **Vốn khởi đầu**: 200 Scoin.
- **Tốc độ thu nhập**: 25 Scoin/phút (online cấp khởi đầu), tích lũy offline tối đa 6 giờ bằng 40% tốc độ online.
- **Hệ thống sao & Michelin**: Hiển thị trên biển hiệu quán gồm Avatar + Tên quán + Số sao + Huy hiệu sao Michelin 1 sao khi đạt 5.0★.
