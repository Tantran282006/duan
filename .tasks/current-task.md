# [P0 EXECUTION] Xây dựng lại 3 scene Phố Nhỏ: CityOverworld, ShopInterior_HoneyBreakfast và Cooking_HoneyBreakfast

Task ID: `pho-nho-unity-three-scenes-rebuild-20261012-001`
Status: `in_progress`
Worker: `antigravity-auto-worker-msi`
Dispatched: `2026-10-12T19:40:17+07:00`

## Mục tiêu
Xây dựng lại 3 scene Unity thật với đồ họa 2D giàu chi tiết, chuyển scene thật 2 chiều hai cấp (Phố <-> Nội thất <-> Nấu ăn), không dùng UI popup, kiểm thử tối thiểu 3 chu trình lặp hoàn chỉnh.

## Files liên quan
- `Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity`
- `Assets/PhoNho/Scenes/ShopInterior_HoneyBreakfast.unity`
- `Assets/PhoNho/Scenes/Cooking_HoneyBreakfast.unity`
- `Assets/PhoNho/Scripts/Gameplay/SceneTransitionManager.cs`
- `PROJECT_STATE.md`
- `PROGRESS.md`

## Các bước thực hiện
1. Kiểm tra kiến trúc scene và scripts hiện tại, cập nhật SceneTransitionManager hỗ trợ luồng 3 scene 2 chiều.
2. Thiết kế lại toàn diện Scene ShopInterior_HoneyBreakfast giàu chi tiết (quầy bếp, quầy chế biến, kệ nguyên liệu, bàn ghế, thực đơn, ánh sáng, trang trí, khách NPC, trigger nấu ăn).
3. Xây dựng Scene mới Cooking_HoneyBreakfast cận cảnh bàn bếp, dụng cụ, nguyên liệu và các bước chế biến món ăn mẫu hoàn chỉnh.
4. Đăng ký BuildSettings, kiểm tra spawn return, kiểm tra Camera và Manager không bị nhân bản.
5. Viết validator PlayMode tự động kiểm thử 3 chu trình: Phố -> Nội Thất -> Nấu Ăn -> Nội Thất -> Phố.

## Acceptance Criteria
- 3 Scene Unity thật hoạt động đồng bộ 2D chi tiết, không dùng hình chữ nhật đơn giản làm sản phẩm cuối.
- Chuyển scene thật 2 chiều bằng SceneManager.LoadSceneAsync giữa Phố <-> Nội Thất <-> Nấu Ăn.
- Lưu đúng vị trí quay lại và trạng thái món ăn/tiền tệ.
- Kiểm thử chu trình lặp tối thiểu 3 lần không lỗi compile/runtime, không mất nhân vật, không nhân đôi Camera/Manager.
- Cập nhật PROGRESS.md và PROJECT_STATE.md với ảnh snapshot kiểm chứng.
