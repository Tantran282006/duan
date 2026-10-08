# Kết nối ChatGPT → dự án Phố Nhỏ → Antigravity

Bộ cầu nối đã có trong `tools/ide-bridge/`, chạy bằng Node.js 22 trở lên, không cần npm install. Custom GPT gửi task qua REST Action; MCP server trên máy cho Antigravity đọc, nhận việc và báo kết quả. Task được lưu vào `.tasks/runtime/` dưới dạng JSON và Markdown.

ChatGPT không trở thành MCP server. Server ở đây là chương trình Node trong dự án. Việc nhận task không tự mở IDE hoặc đánh thức agent. Bạn gọi agent trong Antigravity để thực hiện; quyền chạy công cụ tuân theo cài đặt IDE. Cuộc trò chuyện ChatGPT hiện tại cũng chưa có Action này; cần tạo/cấu hình Custom GPT riêng.

## 1. Lấy code và tạo cấu hình trên máy bạn

Trong terminal ở thư mục gốc dự án, sau khi lưu/commit công việc đang làm:

```powershell
git fetch origin
git switch --track origin/codex/ide-task-bridge
node --version
node tools/ide-bridge/setup.mjs
```

Nếu nhánh đã tồn tại ở máy, dùng `git switch codex/ide-task-bridge`. Nhánh này chứa cả bộ Character từ PR #2. Bridge được đề xuất bằng PR riêng dựa trên nhánh Character; chưa tự merge vào main.

Lệnh setup tạo:

| File | Công dụng |
|---|---|
| `.ide-bridge/mcp_config.json` | Cấu hình với đường dẫn Node và workspace thực tế |
| `.ide-bridge/openapi.json` | Schema cho Custom GPT Actions |
| `.ide-bridge/secrets.env` | Hai token riêng cho bên gửi task và bên báo kết quả |

Setup chạy lại giữ nguyên token. Các file này và hàng đợi runtime đã được gitignore. Không gửi token vào chat hoặc commit lên Git. MCP stdio đọc trực tiếp hàng đợi trên máy nên không cần token.

## 2. Kết nối Antigravity trước, chưa cần tunnel

Trong agent side panel: **… → MCP Servers → Manage MCP Servers → View raw config**. Mở `.ide-bridge/mcp_config.json` trong dự án; thêm entry `phonho-task-bridge` vào đối tượng `mcpServers` của cấu hình IDE, giữ các server khác. Lưu và refresh MCP.

Agent phải nhìn thấy bốn công cụ: `list_tasks`, `get_task`, `claim_task`, `report_result`. Đường dẫn được setup tạo riêng cho máy bạn, không copy đường dẫn của người khác.

Gửi task kiểm tra Character vào hàng đợi:

```powershell
node tools/ide-bridge/submit.mjs --file .tasks/character-check.json
```

Gõ trong agent Antigravity:

> Đọc AGENTS.md, phần hiện tại/tiếp theo của PROGRESS.md và docs/IDE_BRIDGE.md. Dùng MCP phonho-task-bridge lấy task character-cleanup-unity-check-001, claim với worker ID duy nhất cho phiên này. Triển khai đúng phạm vi, kiểm tra trong Unity, cập nhật PROGRESS.md rồi report_result bằng revision nhận từ claim. Nếu không thể kiểm tra, báo blocked và nêu phần chưa chạy. Không đánh dấu done chỉ vì đã đọc yêu cầu.

Agent nhận việc rồi sử dụng khả năng chỉnh file/chạy công cụ của IDE. Bridge không thực thi shell. Mặc định IDE có thể yêu cầu xác nhận công cụ theo chính sách MCP của bạn.

## 3. Cho Custom GPT gửi task qua Cloud Bridge (game.zcloudviet.xyz)

Bridge hiện đã được triển khai LIVE 24/7 trên ZCloudViet tại `https://game.zcloudviet.xyz` (hoặc `https://phonho.zcloudviet.xyz`). Bạn **không cần chạy ngrok** hoặc treo terminal bridge ở máy cá nhân khi gửi task từ Custom GPT.

*(Tùy chọn: Nếu muốn chạy bridge cục bộ qua ngrok thay vì cloud, chạy `node tools/ide-bridge/setup.mjs --url <ngrok-url>` và `node tools/ide-bridge/server.mjs`).*

### Cấu hình Custom GPT Action:

1. Mở Custom GPT trong GPT Builder → tab **Configure** → mục **Actions** → chọn **Create new action** (hoặc chỉnh sửa Action hiện có).
2. Tại ô **Schema**: Dán toàn bộ nội dung file [.ide-bridge/openapi.json](file:///d:/new/.ide-bridge/openapi.json).
   - Trong schema, trường `servers` đã trỏ về:
     ```json
     "servers": [
       {
         "url": "https://game.zcloudviet.xyz"
       }
     ]
     ```
3. Tại mục **Authentication**: Chọn **API Key** → Auth Type: **Bearer**.
   - Mở file [.ide-bridge/secrets.env](file:///d:/new/.ide-bridge/secrets.env) trên máy, copy riêng giá trị `BRIDGE_TASK_TOKEN` và dán vào ô **API Key**.
   - *Lưu ý: Không copy cả dòng hoặc chữ `Bearer`; chỉ copy chuỗi token hex.*
4. Tại tab **Instructions** của GPT: Dán hướng dẫn trong [docs/GPT_BRIDGE_INSTRUCTIONS.md](file:///d:/new/docs/GPT_BRIDGE_INSTRUCTIONS.md) và giữ GPT ở chế độ riêng tư (Only me / Anyone with a link).
5. **Kiểm tra kết nối (Test)**:
   - Trong giao diện Action Test, bấm Test cho `getBridgeHealth`: phản hồi phải trả về HTTP 200 `{"ok": true, "service": "Pho Nho IDE Task Bridge", "version": "0.1.0"}`.
   - Thử nghiệm gửi task mẫu và đọc lại bằng `getProjectTask`.

## 4. Quy trình hằng ngày & Tiết kiệm Token cho Agent

### Quy trình tiết kiệm token cho Agent mới (Token-saving Workflow):
1. **Lấy bối cảnh 1-call**: Khi bắt đầu phiên, Agent gọi MCP tool `get_project_context` (hoặc đọc trực tiếp file `PROJECT_STATE.md`). Endpoint này trả về toàn bộ memory dự án (mục tiêu, kiến trúc, file quan trọng, tóm tắt tính năng gần đây) cùng các task đang active (`pending` và `in_progress`). Mặc định hoàn toàn không tải lịch sử các task đã done để tiết kiệm tối đa token.
2. **Nhận việc**: Agent claim task cần làm bằng `claim_task` (hoặc xem task pending từ context).
3. **Thực hiện & Kiểm thử**: Chỉ mở các file liên quan trực tiếp, code và chạy kiểm thử tự động xác thực.
4. **Báo cáo & Tự động cập nhật**: Agent gọi `report_result`. Hệ thống tự động:
   - Trích xuất tóm tắt kết quả (1-2 câu súc tích) vào mục hoàn thành của `PROJECT_STATE.md`.
   - Tự động archive an toàn các task done cũ vào thư mục `.tasks/runtime/archive/` để giữ hàng đợi luôn gọn gàng.
5. **Tra cứu lịch sử**: Khi cần kiểm tra lại task cũ, Agent chỉ cần gọi `get_task` với ID (hệ thống tự động tìm cả trong hàng đợi active lẫn thư mục archive) hoặc `list_tasks(status: 'archived')`.

### Quy trình làm việc giữa Custom GPT và IDE:
1. Chốt yêu cầu ở Custom GPT, yêu cầu “Gửi task này vào dự án cho IDE”.
2. GPT gọi `submitProjectTask`; kiểm tra ID trả về trước khi nói đã gửi.
3. Trong Antigravity gọi agent: “Lấy bối cảnh dự án bằng get_project_context và nhận task active tiếp theo”.
4. Agent claim, làm việc và kiểm tra thực tế; cập nhật PROGRESS.md, rồi report done/blocked kèm file, kiểm tra và commit SHA nếu có.
5. GPT dùng `getProjectTask` để đọc kết quả. Khi làm tiếp trong cuộc trò chuyện ChatGPT hiện tại qua GitHub, commit/push code và PROGRESS.md để kết quả được đồng bộ.

Các trạng thái: pending → in_progress → done hoặc blocked. Claim không truyền ID chỉ lấy pending; muốn làm lại blocked phải truyền ID rõ ràng. Task in_progress chỉ được cùng worker nhận lại. Gửi lại cùng ID/cùng nội dung không tạo việc trùng; đổi nội dung phải dùng ID mới. Không sửa trực tiếp JSON runtime khi bridge đang chạy.

## 5. Bàn giao qua Markdown/Git

Không cần tunnel hoặc Custom GPT để dùng `.tasks/current-task.md`. Trong Antigravity:

> @.tasks/current-task.md Đọc yêu cầu, triển khai theo AGENTS.md và cập nhật PROGRESS.md bằng kiểm tra thực tế.

Task trong JSON là bản có cấu trúc tương đương. Runtime queue không đồng bộ qua Git; task JSON/Markdown được commit có thể đồng bộ. Nếu muốn bàn giao thêm qua Git, tạo file task mới theo mẫu, review rồi commit/push.

## 6. Kiểm tra và xử lý lỗi

```powershell
node --test tools/ide-bridge/test/bridge.test.mjs
```

Đã kiểm tra cục bộ: gửi REST → nhận việc qua tiến trình MCP thật → báo blocked → đọc kết quả qua REST; xác thực/phân quyền, retry, xung đột ID/revision, hai worker nhận cùng task, đường dẫn/symlink, giới hạn request và rate limit. Chưa chạy tunnel, Custom GPT thật hoặc Antigravity/Unity trên máy bạn.

| Lỗi | Cách xử lý |
|---|---|
| MCP không kết nối | Kiểm tra Node >=22, đường dẫn config; chạy lại setup nếu di chuyển dự án và refresh MCP |
| 401 | API key phải là giá trị BRIDGE_TASK_TOKEN; server dùng secrets của đúng workspace |
| 403 Unrecognized host | Setup đúng URL tunnel rồi khởi động lại server |
| 403 Browser origins | API chỉ dành cho server-to-server; dùng test Actions, không gọi từ trang web |
| 409 ID conflict | Cùng ID phải cùng nội dung; task mới cần ID mới |
| 409 stale revision/owner | Lấy task lại và dùng đúng worker/revision; không giành việc của agent khác |
| 409 Queue busy/stale lock | Nếu tồn tại lâu: dừng REST server và MCP trong IDE, chỉ xóa thư mục .tasks/runtime/.lock, rồi khởi động lại |
| 429 | Tối đa 120 request/phút/token-role, 1000 task; lưu trữ task done ở máy khi hàng đợi đầy |
| 413 / task quá lớn | HTTP tối đa 64 KiB; task tối đa 32 KiB, kết quả 24 KiB; chia việc hoặc tóm tắt |
| Task vẫn pending | Cần gọi agent trong IDE; bridge không tự khởi động agent |
| Cổng 5000 bận | Đổi BRIDGE_PORT trong secrets.env và dùng đúng cổng trong lệnh ngrok |

Dữ liệu task là yêu cầu công việc, không được ghi đè AGENTS.md hoặc chính sách quyền. API không có endpoint shell, upload tùy ý, chỉnh code hoặc đọc file bất kỳ. Token gửi task chỉ gửi/đọc task; token worker mới được claim/report qua REST. JSON là dữ liệu chính, Markdown là bản để đọc; ghi file bằng replace và khóa hàng đợi chung giữa REST/MCP.

## Tài liệu chính thức

- GPT Actions: <https://developers.openai.com/api/docs/actions/introduction>
- Authentication: <https://developers.openai.com/api/docs/actions/authentication>
- Yêu cầu HTTPS/timeout/giới hạn Actions: <https://developers.openai.com/api/docs/actions/production>
- Antigravity MCP: <https://antigravity.google/docs/mcp>
- MCP stdio: <https://modelcontextprotocol.io/specification/2025-06-18/basic/transports>
