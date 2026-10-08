---
name: game-development
description: >-
  Game development orchestrator and Unity 2D expert skill for Antigravity IDE in Pho Nho project.
  Guides game loop, 2D physics, player movement, input, camera follow, state patterns, pooling,
  batchmode validation, and token-saving Project Memory workflow.
---

# Game Development — Antigravity IDE Native Skill

> **Orchestrator skill** dành riêng cho Antigravity IDE trong dự án game "Phố Nhỏ".
> Tối ưu cho Unity 2D (C#) và tích hợp liền mạch với **Project Memory** (`PROJECT_STATE.md`) & IDE Task Bridge.

---

## 1. Khi nào Kích hoạt Skill này
- Triển khai tính năng gameplay, cơ chế điều khiển nhân vật, map, camera, animation hoặc tương tác trên Unity 2D (C#).
- Thiết kế và áp dụng các pattern cốt lõi (State Machine, Object Pooling, Observer/Events).
- Kiểm tra, sửa lỗi va chạm vật lý, tối ưu hóa hiệu năng render/memory.
- Xây dựng và thực thi bộ kiểm thử tự động (Batchmode Validator / Play Mode Validator).

---

## 2. Quy trình Token-Saving & Tích hợp Project Memory

Để tiết kiệm tối đa context window và tránh tải lại lịch sử dài:

1. **Đọc Project Memory 1-call**:
   - Gọi tool `get_project_context` (hoặc đọc file [PROJECT_STATE.md](file:///d:/new/PROJECT_STATE.md)).
   - Chỉ đọc mục tiêu, kiến trúc cốt lõi và các task đang active (`pending`, `in_progress`). Mặc định **không tải task đã done**.
2. **Progressive Disclosure**:
   - Không đọc toàn bộ tài liệu game development cùng lúc.
   - Chỉ mở đúng file reference tương ứng với yêu cầu hiện tại (xem mục 5 Routing bên dưới).
3. **Thực thi với Tool Antigravity**:
   - Sử dụng các tool chuẩn: `view_file` (đọc có mục tiêu với `StartLine`/`EndLine`), `replace_file_content` (chỉnh sửa block cụ thể), `write_to_file`, `run_command` (chạy batchmode hoặc script kiểm tra).
   - Không quét toàn bộ project bằng shell nếu có thể dùng `grep_search`.
4. **Báo cáo & Tự động Archive**:
   - Khi hoàn thành, gọi `report_result` kèm bằng chứng validation thực tế.
   - Hệ thống bridge sẽ tự động cập nhật tóm tắt ngắn vào `PROJECT_STATE.md` và archive các task done cũ vào `.tasks/runtime/archive/`.

---

## 3. Nguyên tắc Cốt lõi của Game Development

### A. The Game Loop & Fixed Timestep
Mọi tương tác trong game tuân thủ chặt chẽ:
```
INPUT (Update)      → Đọc phím, touch, chuột (Input Manager / New Input System)
PHYSICS (FixedUpdate) → Cập nhật vận tốc, lực, va chạm (Fixed 50Hz, deltaTime cố định)
RENDER (LateUpdate) → Interpolation hình ảnh, camera follow, parallax background
```

- **Quy tắc FixedUpdate**:
  - Mọi thao tác làm thay đổi vận tốc vật lý (`linearVelocity`, `AddForce`, `MovePosition`) **bắt buộc** thực hiện trong `FixedUpdate`.
  - Tuyệt đối không gán `transform.position += ...` cho đối tượng đang điều khiển bằng `Rigidbody2D` Dynamic vì sẽ gây jitter hình ảnh và bỏ qua tương tác collider.
  - Sử dụng `Time.fixedDeltaTime` trong FixedUpdate và `Time.deltaTime` trong Update.

### B. Pattern Selection Matrix
| Pattern | Khi nào nên dùng | Áp dụng trong Phố Nhỏ | Reference |
|---|---|---|---|
| **State Machine** | Quản lý 3–5 trạng thái có logic chuyển tiếp rõ ràng | Trạng thái nhân vật: Idle $\to$ Walk $\to$ Interact | [patterns.md](file:///d:/new/.agents/skills/game-development/references/patterns.md) |
| **Object Pooling** | Vật thể sinh/hủy lặp lại nhiều lần | Khách NPC, coin rơi, icon cảm xúc, VFX bụi chân | [patterns.md](file:///d:/new/.agents/skills/game-development/references/patterns.md) |
| **Observer / Events** | Giao tiếp giữa các module không phụ thuộc nhau | Sự kiện hoàn tất đơn hàng, đổi số dư Scoin, lên sao | [patterns.md](file:///d:/new/.agents/skills/game-development/references/patterns.md) |

---

## 4. Hướng dẫn Trọng tâm: Unity 2D (C#) trong Phố Nhỏ

### A. Cấu trúc Component C# Chuẩn
- Namespace: `PhoNho.<Module>` (ví dụ: `PhoNho.Character`, `PhoNho.Map`, `PhoNho.Economy`).
- Đặt tên: PascalCase cho Class/Method/Property; camelCase cho biến cục bộ; `_camelCase` cho private field.
- Ràng buộc: Dùng `[RequireComponent(...)]`, `[DisallowMultipleComponent]`.
- Tránh gọi `Find()` hoặc `GetComponent()` trong `Update()`/`FixedUpdate()`; luôn cache trong `Awake()` hoặc `Reset()`.

### B. Vật lý 2D & Nhân vật
- **Rigidbody2D**:
  - `bodyType = RigidbodyType2D.Dynamic`.
  - Khóa xoay trục Z: `constraints = RigidbodyConstraints2D.FreezeRotation` (chống nhân vật đổ nghiêng khi đi trên mặt đất).
  - Bật `collisionDetectionMode = CollisionDetectionMode2D.Continuous` để không bao giờ xuyên đất.
  - Bảo toàn trọng lực: khi di chuyển ngang, chỉ gán thành phần $X$ của velocity, giữ nguyên thành phần $Y$:
    ```csharp
    #if UNITY_6000_0_OR_NEWER
    Vector2 vel = _rigidbody2D.linearVelocity;
    vel.x = _horizontalInput * _moveSpeed;
    _rigidbody2D.linearVelocity = vel;
    #else
    Vector2 vel = _rigidbody2D.velocity;
    vel.x = _horizontalInput * _moveSpeed;
    _rigidbody2D.velocity = vel;
    #endif
    ```
- **Collider2D**:
  - Dùng `CapsuleCollider2D` hoặc `BoxCollider2D` kích thước vừa vặn thân nhân vật chibi (width ~0.7, height ~1.8).
  - Mặt đất/platform dùng `BoxCollider2D` với bề mặt top phẳng; `isTrigger = false`.

### C. Input Abstraction
- Dự án đang sử dụng Unity Input Manager chuẩn (`Horizontal` axis: A/D và mũi tên trái/phải).
- Khi viết script điều khiển, cung cấp phương thức `SetMoveInput(float input)` để vừa hỗ trợ bàn phím, vừa cho phép test tự động bằng code không cần người bấm phím.

### D. Prefab & Scene Safety
- Khi tạo mới hoặc cập nhật nhân vật/platform, lưu thành Prefab trong `Assets/PhoNho/Prefabs/` bằng `PrefabUtility.SaveAsPrefabAsset()`.
- Tuyệt đối không xóa hay ghi đè bừa bãi các scene hiện hữu ([Character_Preview.unity](file:///d:/new/Assets/PhoNho/Scenes/Character_Preview.unity), [CityOverworld_ArtLayout.unity](file:///d:/new/Assets/PhoNho/Scenes/CityOverworld_ArtLayout.unity)). Tạo scene mới hoặc builder tái tạo scene theo phiên bản.

### E. Quy trình Validation Tự động
- Mọi tính năng gameplay/character phải đi kèm với một Editor Validator (ví dụ [PlayerMovementValidator.cs](file:///d:/new/Assets/PhoNho/Editor/PlayerMovementValidator.cs)).
- Cung cấp phương thức static `ValidateFromCommandLine()` để có thể chạy headless trong Unity batchmode:
  ```powershell
  Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe" -ArgumentList "-quit", "-batchmode", "-projectPath", "D:\new", "-executeMethod", "PhoNho.Gameplay.Editor.PlayerMovementValidator.ValidateFromCommandLine", "-logFile", "D:\new\unity_val.log" -Wait
  ```
- Kết quả kiểm thử được ghi ra file JSON kết quả (ví dụ `player-movement-validation-results.json`) để agent đọc lại xác thực.

---

## 5. Progressive Disclosure & Routing

Chỉ mở các file tham chiếu sau khi cần chuyên sâu vào một mảng cụ thể:

| Nhu cầu cụ thể | File Tham chiếu | Nội dung chính |
|---|---|---|
| **Unity 2D Physics & Movement** | [references/unity-2d.md](file:///d:/new/.agents/skills/game-development/references/unity-2d.md) | Chi tiết thiết lập Rigidbody2D, Collision matrix, Raycast check đất, Parallax 2D |
| **Design Patterns & Tối ưu** | [references/patterns.md](file:///d:/new/.agents/skills/game-development/references/patterns.md) | Mẫu C# State Machine, Object Pool generic, Observer Event System |
| **Kiến trúc Phố Nhỏ & Server** | [references/architecture-pho-nho.md](file:///d:/new/.agents/skills/game-development/references/architecture-pho-nho.md) | Server-authoritative, Ledger ACID, Tcoin thẩm mỹ, Order-backed rating |
| **Platform & Mở rộng** | [references/routing.md](file:///d:/new/.agents/skills/game-development/references/routing.md) | Hướng dẫn Mobile (Touch/Joystick/60fps), Audio Lo-fi, Sprite art pipeline |

---

## 6. Nguồn Gốc & Ghi Công (Attribution & Source)

- **Nguồn cảm hứng ban đầu**: Derived from [game-development skill](https://github.com/davila7/claude-code-templates/tree/main/cli-tool/components/skills/creative-design/game-development) trong repository `davila7/claude-code-templates` (Tác giả: Daniel Vila Suero).
- **Giấy phép nguồn**: MIT License.
- **Bản chuyển đổi**: Đã được tinh chỉnh, viết lại toàn bộ công cụ sang chuẩn Antigravity IDE, loại bỏ các giả định của Claude Code (các tên tool cũ và cấu hình đường dẫn riêng của Claude), tích hợp kiến trúc Unity 2D C# và hệ thống Project Memory / IDE Task Bridge của dự án Phố Nhỏ.
