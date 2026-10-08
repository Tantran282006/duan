# Specialty & Platform Routing Reference — Phố Nhỏ

> Hướng dẫn định tuyến chuyên môn mở rộng khi phát triển dự án Phố Nhỏ.

---

## 1. Nền Tảng Di Động (Mobile: Android / iOS)

- **Target Framerate**: Cố định `Application.targetFrameRate = 60` để tiết kiệm pin và tránh quá nhiệt thiết bị.
- **Tỉ lệ Màn hình**: Chuẩn Landscape 16:9 (1920x1080), hỗ trợ co giãn an toàn (Safe Area) cho tai thỏ/nốt ruồi.
- **Input Di động**: Chạm màn hình để di chuyển đến vị trí hoặc chạm tương tác với cửa hàng/vật thể; chuẩn bị sẵn virtual joystick cho phiên bản tương lai.
- **Tối ưu Bộ nhớ**: Giới hạn kích thước texture tối đa 2048px; nén định dạng ASTC (Android / iOS).

---

## 2. Hệ Thống Âm Thanh (Audio)

- **BGM**: Nhạc nền phong cách Acoustic / Lo-fi chill nhẹ nhàng, không gây mệt mỏi khi chơi lâu.
- **SFX**: Âm thanh sinh động, ấm áp và êm dịu (tiếng gõ máy pha chế, tiếng chuông cửa tiệm, tiếng đồng xu leng keng, tiếng chuông chấm sao).
- **Quản lý Audio**: Phân tách rõ các AudioSource qua Audio Mixer Groups (`Master`, `BGM`, `SFX`, `UI`) để người chơi có thể tùy chỉnh âm lượng trong menu Cài đặt.

---

## 3. Quy Chuẩn Đồ Họa (Art Pipeline)

- **Phong cách**: Chibi pastel ấm áp (tương đồng Tiệm Trà Nhỏ).
- **Quy chuẩn Nhân vật**:
  - Sprite strip 1 hàng ngang `1x4`, kích thước `2048 x 640 px`.
  - Mỗi khung hình `512 x 640 px`, đường baseline chân tại $Y = 576$ từ đỉnh xuống.
  - Tỉ lệ hiển thị Pixels Per Unit (PPU) = 256.
  - Điểm neo (Pivot Point) tại `(0.5, 0.1)`.
- **Hiệu ứng VFX**: Hiệu ứng hạt bụi chân khi đi lại, hiệu ứng lấp lánh (Sparkle particles) cho vầng sáng sao Michelin 5.0★ và hào quang Bá Khí.

---

## 4. Mạng & Bất Đồng Bộ (Asynchronous Multiplayer)

- **Kiến trúc mạng**: Tương tác không đồng bộ (khách ghé tiệm người chơi khác qua danh sách hoặc đi bộ trên phố).
- **Đồng bộ trạng thái**: Lưu trữ trạng thái quán và avatar người chơi trên backend Nakama.
- **Bảo mật**: Mọi giao dịch nguyên liệu và đánh giá đều qua REST RPC có xác thực token người dùng.
