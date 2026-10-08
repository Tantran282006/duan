# Unity 2D Development Reference — Phố Nhỏ

> Tài liệu tham chiếu chi tiết về phát triển 2D trên Unity C# trong dự án "Phố Nhỏ".

---

## 1. Thiết lập Physics 2D & Nhân vật (Character Controller)

### Quy chuẩn Rigidbody2D
- **Body Type**: `RigidbodyType2D.Dynamic` (cho nhân vật có trọng lực và vận tốc vật lý).
- **Interpolation**: `RigidbodyInterpolation2D.Interpolate` (chống giật hình ảnh camera khi di chuyển).
- **Collision Detection**: `CollisionDetectionMode2D.Continuous` (ngăn xuyên thủng collider khi rơi nhanh hoặc va chạm góc).
- **Constraints**: Bắt buộc kích hoạt `RigidbodyConstraints2D.FreezeRotation` trên trục Z:
  ```csharp
  _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
  ```

### Quy chuẩn Vận tốc & Trọng lực
Khi cập nhật vận tốc di chuyển ngang, **không ghi đè trục Y** bằng 0:
```csharp
private void FixedUpdate()
{
#if UNITY_6000_0_OR_NEWER
    Vector2 currentVelocity = _rigidbody2D.linearVelocity;
    currentVelocity.x = _horizontalInput * _moveSpeed;
    _rigidbody2D.linearVelocity = currentVelocity;
#else
    Vector2 currentVelocity = _rigidbody2D.velocity;
    currentVelocity.x = _horizontalInput * _moveSpeed;
    _rigidbody2D.velocity = currentVelocity;
#endif
}
```

### Kiểm tra Tiếp đất (Ground Check)
Dùng Raycast hoặc BoxCast từ mép dưới Collider để phát hiện mặt đất:
```csharp
private void CheckGroundStatus()
{
    Bounds bounds = _collider2D.bounds;
    Vector2 rayOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.15f);
    _isGrounded = hit.collider != null && hit.collider != _collider2D;
}
```

---

## 2. Sprite & Animation Standards trong Phố Nhỏ

- **Sprite Strips 1 hàng ngang**: Toàn bộ character dùng sprite strip `1x4` (2048 x 640 px), mỗi frame 512 x 640 px, Pixels Per Unit (PPU) = 256.
- **Pivot Point**: Tọa độ tương đối `(0.5, 0.1)` (chân nhân vật tiếp xúc với mặt đất).
- **Lật hướng nhìn (Facing)**:
  - Đi sang phải ($X > 0$): `spriteRenderer.flipX = false;`
  - Đi sang trái ($X < 0$): `spriteRenderer.flipX = true;`
- **Animator Parameters**:
  - `IsMoving` (bool): `Mathf.Abs(input) > 0.01f`
  - `Speed` (float): `1.0f` khi di chuyển, `0.0f` khi đứng yên

---

## 3. Camera Follow & Parallax 2D

### Camera Follow
Camera theo trục X với damping mượt mà, cố định chiều cao Y và giới hạn biên đường phố:
```csharp
private void LateUpdate()
{
    if (_target == null) return;
    float targetX = Mathf.Clamp(_target.position.x + _offset.x, _minX, _maxX);
    Vector3 targetPosition = new Vector3(targetX, transform.position.y, _offset.z);
    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);
}
```

### Parallax Background
Sử dụng script `PhoNho.Map.ParallaxLayer` để cuộn từng lớp theo tỉ lệ dịch chuyển của camera:
- Lớp 01 Bầu trời: `Vector2(1.0, 1.0)` (khóa cùng camera).
- Lớp 02 Dãy phố xa: `Vector2(0.78, 0.15)` (cuộn chậm hơn).
- Lớp 03 Cửa hàng/Công trình: layer tĩnh `(0, 0)`.
- Lớp 06 Cụm hoa lá tiền cảnh: `Vector2(-0.12, 0)` (cuộn nhanh hơn tạo chiều sâu).

---

## 4. Batchmode Validation Workflow

Khi tạo tính năng mới, tạo kèm file Editor Validator kế thừa mẫu:
```csharp
public static class FeatureValidator
{
    [MenuItem("Phố Nhỏ/Validation/Kiểm tra tính năng")]
    public static void ValidateAll()
    {
        // 1. Kiểm tra Prefab component & properties
        // 2. Mở Scene, kiểm tra GameObject hierarchy
        // 3. Mô phỏng Physics2D.Simulate(0.02f)
        // 4. Ghi JSON kết quả ra Assets/PhoNho/Editor/feature-validation-results.json
    }

    public static void ValidateFromCommandLine() => ValidateAll();
}
```
Lệnh thực thi headless trên terminal:
```powershell
Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe" -ArgumentList "-quit", "-batchmode", "-projectPath", "D:\new", "-executeMethod", "PhoNho.Editor.FeatureValidator.ValidateFromCommandLine", "-logFile", "D:\new\unity_val.log" -Wait
```
