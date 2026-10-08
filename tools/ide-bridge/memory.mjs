import fs from 'node:fs/promises';
import path from 'node:path';
import { randomUUID } from 'node:crypto';

const MAX_MEMORY_BYTES = 16384;
const MAX_RECENT_COMPLETED = 8;

async function atomicWrite(filePath, content) {
  const tempPath = filePath + '.' + randomUUID() + '.tmp';
  try {
    await fs.writeFile(tempPath, content, { encoding: 'utf8', flag: 'w', mode: 0o644 });
    await fs.rename(tempPath, filePath);
  } finally {
    await fs.rm(tempPath, { force: true }).catch(() => {});
  }
}

export function defaultProjectMemory() {
  return `# PROJECT_STATE.md — Trạng thái & Bộ nhớ Dự án "Phố Nhỏ"

> **Dành cho AI Agent mới bắt đầu phiên**: Đọc file này hoặc gọi MCP tool \`get_project_context\` một lần để nắm toàn bộ bối cảnh dự án, kiến trúc, quyết định cốt lõi và các task đang active. Không cần quét toàn bộ task history cũ hay đọc lại cả file log.

---

## 1. Mục tiêu Dự án
- **Game**: "Phố Nhỏ" — Game mô phỏng kinh doanh & quản lý đường phố phong cách Chibi pastel ấm áp (cozy street life management) trên di động (Android / iOS).
- **Core Loop**: Mở quán (Trà sữa, Đồ ăn sáng, Nguyên vật liệu) trên con phố đi bộ cuộn ngang 2D, phục vụ khách NPC và người chơi thật, giao dịch nguyên liệu tự do, nhận đánh giá sau đơn hàng, đua top Bá Khí & Sao Michelin (5.0★).
- **Tương tác**: Bất đồng bộ qua mạng (asynchronous multiplayer), server-authoritative.

---

## 2. Kiến trúc & Công nghệ Hiện tại
- **Client (Unity)**:
  - Phiên bản: Unity 6000.4.3f1 (màn hình ngang Landscape 16:9).
  - 2D Gameplay: Cuộn cảnh Parallax ngang (\`PhoNho.Map.ParallaxLayer\`), physics 2D (\`Rigidbody2D\` Dynamic, \`CapsuleCollider2D\`, khóa FreezeRotation Z, \`PhoNhoPlayerMovement\`).
  - Điều khiển: Bàn phím A/D và mũi tên trái/phải (\`Input.GetAxisRaw("Horizontal")\`), camera bám mượt (\`PhoNhoCameraFollow\`).
  - Character & Animation: 4 sprite strip 1 hàng ngang duy nhất (\`1x4\`, \`2048 x 640 px\`, PPU 256, baseline 576, pivot \`(0.5, 0.1)\`), Animator Controllers \`Male_A_Controller\` & \`Female_A_Controller\`.
- **Backend & Tooling**:
  - Kế hoạch backend: Nakama Open-Source + PostgreSQL (Docker local dev, RPC, ledger ACID).
  - IDE Bridge: REST Server + MCP stdio (\`tools/ide-bridge/\`), Node.js 22 native, Cloud bridge LIVE tại \`https://game.zcloudviet.xyz\` và \`https://phonho.zcloudviet.xyz\`.
  - Dashboard quản lý: Web UI local \`http://127.0.0.1:5050\` quản lý tiến độ, kế hoạch, done log, và profiles Antigravity IDE.
  - Project Memory & Archive: Handoff cô đọng qua \`PROJECT_STATE.md\`, tự động archive task done vào \`.tasks/runtime/archive/\`.

---

## 3. Quyết định Kỹ thuật Cốt lõi [CHỐT]
- **D1/D2**: Mobile 2D cuộn cảnh ngang, tương tác người chơi bất đồng bộ.
- **Server-authoritative**: Mọi tính toán tiền, sao, đánh giá, xếp hạng đều do server kiểm soát.
- **Ledger bất biến**: Scoin, Gem, Tcoin cập nhật qua transaction có \`idempotency_key\`. Cấm cộng/trừ số dư trực tiếp.
- **Tcoin thẩm mỹ**: Tcoin nạp bằng tiền thật chỉ dùng cho thẩm mỹ, tuyệt đối không mua được sao hay lợi thế kinh tế.
- **Đánh giá chặt chẽ**: Mỗi đánh giá gắn liền với một đơn hàng hoàn tất; tối đa 1 đánh giá/đơn, giới hạn theo ngày.
- **Định danh thành phố**: Mọi bảng dữ liệu có \`city_id\` sẵn sàng mở rộng nhiều thành phố.

---

## 4. File & Module Quan trọng
| File / Thư mục | Mục đích |
|---|---|
| \`Assets/PhoNho/Scripts/Character/PhoNhoPlayerMovement.cs\` | Controller vật lý 2D di chuyển nhân vật trái/phải, bảo toàn gravity Y, tự động lật flipX |
| \`Assets/PhoNho/Scripts/Map/PhoNhoCameraFollow.cs\` | Camera follow bám theo nhân vật mượt mà, có clamp biên |
| \`Assets/PhoNho/Scripts/Map/ParallaxLayer.cs\` | Hiệu ứng thị sai nhiều lớp cho bầu trời, chân trời, tán cây |
| \`Assets/PhoNho/Prefabs/Player_Character.prefab\` | Prefab nhân vật đầy đủ Rigidbody2D, CapsuleCollider2D, Animator, SpriteRenderer |
| \`Assets/PhoNho/Scenes/CityOverworld_PlayerMovement.unity\` | Scene phố gameplay có nền, collider mặt đất, tường chắn và Player |
| \`Assets/PhoNho/Scenes/Character_Preview.unity\` | Scene preview nhân vật nam/nữ với các sprite strip 1 hàng |
| \`tools/ide-bridge/\` | Bộ cầu nối ChatGPT Custom GPT ↔ Local Task Store ↔ Antigravity MCP |
| \`PROGRESS.md\` | Nhật ký tiến độ chi tiết từng phiên |
| \`PROJECT_STATE.md\` | Bộ nhớ cô đọng của dự án (file này) |

---

## 5. Những Phần Đã Hoàn Thành Gần Đây
- **[unity-map-player-movement-001] Tạo map Unity 2D và nhân vật di chuyển trái/phải**: Controller vật lý PhoNhoPlayerMovement (Rigidbody2D, CapsuleCollider2D, FreezeRotation), scene gameplay CityOverworld_PlayerMovement có collider mặt đất và tường chắn, camera follow (100% pass trên Unity batchmode).
- **[web-dashboard-redesign-001] Thiết kế lại dashboard Phố Nhỏ**: Giao diện dark theme đáp ứng 4 tab Tổng quan, Kế hoạch, Done log và Profiles Antigravity.
- **[character-animation-simple-outfit-02] Tích hợp character trang phục giản dị**: Chuẩn hóa 4 animation chính thành sprite strip 1 hàng ngang 1x4 2048x640 PPU 256.

---

## 6. Việc Đang Làm & Việc Tiếp Theo
- **Đang làm**: Task \`project-memory-archive-001\` (Tối ưu Memory & Task Archive cho IDE Bridge).
- **Việc tiếp theo**:
  1. M0: Hoàn thiện URP 2D, Addressables và Localization tiếng Việt.
  2. M0: Thêm tính năng tương tác (nút E / chạm) với các cửa hàng (Boba, Breakfast, Ingredient) trên phố.
  3. M0: Dựng backend Docker Nakama + PostgreSQL, kiểm tra kết nối; sau đó triển khai M1 ledger/RPC/unit test.

---

## 7. Hướng dẫn Agent Tiết kiệm Token (Workflow chuẩn)
1. Khi bắt đầu phiên làm việc: Gọi tool \`get_project_context\` (hoặc đọc file \`PROJECT_STATE.md\`).
2. Xem mục **Việc Đang Làm** và danh sách \`active_tasks\` trả về; claim task cần làm qua \`claim_task\`.
3. Chỉ đọc \`PROGRESS.md\` hoặc \`GAME_PROMPT.md\` khi cần tra cứu chi tiết cụ thể của tính năng đang làm.
4. Chỉ mở các file code liên quan trực tiếp. Không quét hoặc đọc cả cây thư mục dự án.
5. Khi hoàn thành: gọi \`report_result\` với bằng chứng test cụ thể. Hệ thống sẽ tự động cập nhật Project Memory và lưu trữ an toàn các task done cũ vào archive.
`;
}

export async function readProjectMemory(root) {
  const filePath = path.join(root, 'PROJECT_STATE.md');
  try {
    const content = await fs.readFile(filePath, 'utf8');
    return content.replace(/^\uFEFF/, '').trim();
  } catch (err) {
    if (err.code === 'ENOENT') {
      const initial = defaultProjectMemory();
      await atomicWrite(filePath, initial);
      return initial.trim();
    }
    throw err;
  }
}

export async function updateProjectMemoryWithTask(root, task) {
  if (!task || task.status !== 'done' || !task.result) {
    return;
  }

  const filePath = path.join(root, 'PROJECT_STATE.md');
  let content = await readProjectMemory(root);

  // Extract a clean 1-2 sentence essence (no multi-line logs, max 160 chars)
  const rawSummary = (task.result.summary || '').split('\n')[0].trim();
  const shortSummary = rawSummary.length > 160 ? rawSummary.slice(0, 157) + '...' : rawSummary;
  const newBullet = `- **[${task.id}] ${task.title}**: ${shortSummary}`;

  // Section header for completed items
  const sectionHeader = '## 5. Những Phần Đã Hoàn Thành Gần Đây';
  const nextSectionHeader = '## 6. Việc Đang Làm & Việc Tiếp Theo';

  const startIndex = content.indexOf(sectionHeader);
  const endIndex = content.indexOf(nextSectionHeader);

  if (startIndex !== -1 && endIndex !== -1 && endIndex > startIndex) {
    const before = content.slice(0, startIndex + sectionHeader.length);
    const middle = content.slice(startIndex + sectionHeader.length, endIndex).trim();
    const after = content.slice(endIndex);

    // Parse existing bullets
    const existingBullets = middle
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.startsWith('-'));

    // Check if task is already summarized
    const alreadyPresent = existingBullets.some(line => line.includes(`[${task.id}]`));
    if (!alreadyPresent) {
      existingBullets.unshift(newBullet);
    }

    // Keep at most MAX_RECENT_COMPLETED items
    const trimmedBullets = existingBullets.slice(0, MAX_RECENT_COMPLETED);
    const updatedMiddle = '\n' + trimmedBullets.join('\n') + '\n\n';

    content = before + updatedMiddle + after;
  } else {
    // Fallback: append bullet if section not found
    content += `\n\n- **[${task.id}] ${task.title}**: ${shortSummary}\n`;
  }

  // Enforce memory bounds
  if (Buffer.byteLength(content, 'utf8') > MAX_MEMORY_BYTES) {
    content = content.slice(0, MAX_MEMORY_BYTES);
  }

  await atomicWrite(filePath, content);
}

export async function getProjectContext(root, store, options = {}) {
  const memory = await readProjectMemory(root);
  const allActive = await store.all();

  const includePending = options.include_pending ?? true;
  const includeInProgress = options.include_in_progress ?? true;

  const activeTasks = allActive
    .filter(t => (includePending && t.status === 'pending') || (includeInProgress && t.status === 'in_progress'))
    .map(t => ({
      id: t.id,
      title: t.title,
      status: t.status,
      worker: t.worker,
      goal_summary: t.goal ? (t.goal.length > 250 ? t.goal.slice(0, 247) + '...' : t.goal) : null,
      revision: t.revision,
      created_at: t.created_at,
      updated_at: t.updated_at
    }));

  const archivedList = await store.listArchived(0);

  return {
    project_memory: memory,
    active_tasks: activeTasks,
    stats: {
      pending_count: allActive.filter(t => t.status === 'pending').length,
      in_progress_count: allActive.filter(t => t.status === 'in_progress').length,
      archived_count: archivedList.tasks.length,
      total_active: allActive.length
    }
  };
}
