# [AUTO-WORKER ASSIGNED] P0 audit Git: kiểm tra >100 file chưa commit, không sửa/xóa/commit

Task ID: `git-uncommitted-audit-008`
Status: `in_progress`
Worker: `antigravity-auto-worker-msi`
Dispatched: `2026-10-08T14:17:56.201Z`

## Mục tiêu
Chỉ kiểm tra trạng thái Git hiện tại của toàn bộ project Phố Nhỏ để xác định chính xác vì sao có hơn 100 file chưa commit. Tuyệt đối không stage, commit, reset, checkout, clean, xóa hay sửa nội dung file trong task này. Phân loại file nào là thay đổi source/asset hợp lệ từ các task, file generated/runtime/cache đáng lẽ nên ignore, file Unity .meta cần giữ, và file nghi ngờ ngoài ý định. Đối chiếu với các task đã done/in_progress để phát hiện task đã báo done nhưng thay đổi vẫn chưa commit.

## Files liên quan
- `.gitignore`
- `AGENTS.md`
- `PROGRESS.md`
- `PROJECT_STATE.md`
- `Assets`
- `tools/ide-bridge`
- `.tasks`
- `tasks`
- `.vscode`

## Các bước thực hiện
1. Chạy git status --short và git status --porcelain=v1 để lấy danh sách chính xác; ghi tổng số file modified, added, deleted, renamed, untracked, staged và unstaged.
2. Chạy git diff --stat và git diff --cached --stat; không thay đổi index.
3. Phân loại từng path theo nhóm: Unity source/assets/scenes/prefabs, Unity .meta, IDE bridge/dashboard, docs/config, tests, generated validation output, runtime/task logs, cache/temp/build artifacts, và unknown/suspicious.
4. Đối chiếu danh sách changed_files của các task đã done với Git status hiện tại; chỉ ra task nào đã done nhưng file vẫn chưa commit. Đặc biệt kiểm tra các task có commit=null.
5. Kiểm tra commit ad62bae2e61e7a65fe36da077428b643c1239935 có chứa đúng các file của ide-accounts-backend-001 hay không; không sửa lịch sử Git.
6. Kiểm tra .gitignore để phát hiện Library, Temp, Logs, obj, Builds, runtime logs, worker heartbeat/locks, validation screenshots/results hoặc file sinh tự động nào đang lọt vào Git status ngoài ý muốn.
7. Với Unity, không đánh dấu .meta là rác chỉ vì là .meta; xác định .meta tương ứng asset mới cần track hay orphan/sai.
8. Tạo bảng báo cáo top-level: tổng file, số file cần giữ/commit, số file nên ignore, số file cần người dùng duyệt, số file đáng nghi. Liệt kê cụ thể path của mọi file đáng nghi hoặc có nguy cơ mất dữ liệu.
9. Không được sửa .gitignore trong task audit này. Chỉ đề xuất patch riêng sau khi có kết luận.
10. Không được stage/commit/reset/clean/delete/checkout/revert bất kỳ file nào. Không được thay đổi working tree.
11. Report_result phải chứa command/evidence đã dùng, thống kê chính xác, phân loại và mapping task->uncommitted files.

## Acceptance Criteria
- Working tree và index không bị thay đổi bởi task audit.
- Có số lượng chính xác file Git status theo từng loại modified/added/deleted/renamed/untracked và staged/unstaged.
- Có danh sách file/thư mục generated hoặc runtime đáng lẽ nên ignore, nếu tồn tại.
- Có mapping các file chưa commit với các task đã done/in_progress, đặc biệt các task commit=null.
- Có kết luận rõ file nào an toàn để commit, file nào cần ignore, file nào cần người dùng duyệt và file nào đáng nghi; không tự xóa hay commit.
