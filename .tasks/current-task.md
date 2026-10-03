# Giao việc hiện tại — kiểm tra Character trong Unity

Yêu cầu đầy đủ ở `.tasks/character-check.json`; quy trình dự án ở `AGENTS.md` và `docs/CHARACTER_CLEANUP.md`.

Khi chưa cấu hình MCP, agent Antigravity có thể đọc các file này trực tiếp, triển khai đúng phạm vi, cập nhật `PROGRESS.md`, commit/push kết quả.

Khi đã cấu hình MCP, gửi task mẫu bằng:

```sh
node tools/ide-bridge/submit.mjs --file .tasks/character-check.json
```

Agent gọi `list_tasks`, `claim_task`, thực hiện công việc, rồi gọi `report_result` với revision và worker nhận được. Hàng đợi runtime nằm trong `.tasks/runtime` và được bỏ qua bởi Git.
