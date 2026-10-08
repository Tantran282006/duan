import assert from 'node:assert/strict';
import { createDashboardServer } from '../dashboard/server.mjs';
import { rootFor, argumentsFor, readEnvironment } from '../config.mjs';

async function runE2ESmokeTest() {
  console.log('--- BẮT ĐẦU END-TO-END SMOKE TEST DASHBOARD WEB UI ---');
  const root = await rootFor(argumentsFor());
  const env = await readEnvironment(root);
  const server = createDashboardServer(root, env);
  
  await new Promise(resolve => server.listen(5051, '127.0.0.1', resolve));
  const base = 'http://127.0.0.1:5051';
  console.log(`[1] Dashboard test server chạy tại: ${base}`);

  try {
    // 1. Kiểm tra static assets
    console.log('[2] Kiểm tra nạp UI assets...');
    const htmlRes = await fetch(`${base}/`);
    assert.equal(htmlRes.status, 200);
    const htmlText = await htmlRes.text();
    assert.ok(htmlText.includes('Phố Nhỏ — Developer Dashboard'));
    assert.ok(htmlText.includes('id="btn-open-create-task"'));
    assert.ok(htmlText.includes('id="add-profile-modal"'));
    assert.ok(htmlText.includes('id="create-task-modal"'));
    assert.ok(htmlText.includes('id="settings-modal"'));
    assert.ok(htmlText.includes('id="complete-task-modal"'));
    assert.ok(htmlText.includes('id="btn-drawer-claim"'));
    console.log(' -> HTML đầy đủ components, modals và action buttons.');

    const jsRes = await fetch(`${base}/app.js`);
    assert.equal(jsRes.status, 200);
    const jsText = await jsRes.text();
    assert.ok(jsText.includes('updateTaskStatus'));
    assert.ok(jsText.includes('openCreateTaskModal'));
    assert.ok(jsText.includes('openAddProfileModal'));
    console.log(' -> app.js nạp thành công, không thiếu method.');

    const cssRes = await fetch(`${base}/styles.css`);
    assert.equal(cssRes.status, 200);
    console.log(' -> styles.css nạp thành công.');

    // 2. Overview
    console.log('[3] Kiểm tra /api/overview...');
    const ovRes = await fetch(`${base}/api/overview`);
    assert.equal(ovRes.status, 200);
    const ovData = await ovRes.json();
    assert.ok(ovData.currentMilestone);
    assert.ok(ovData.metrics);
    console.log(` -> Milestone: ${ovData.currentMilestone}, Task metrics: ${JSON.stringify(ovData.metrics)}`);

    // 3. Profiles
    console.log('[4] Kiểm tra Quản lý Profiles...');
    const profRes = await fetch(`${base}/api/profiles`);
    assert.equal(profRes.status, 200);
    const profData = await profRes.json();
    assert.ok(profData.activeProfileId);
    assert.ok(profData.profiles.length >= 1);

    // Thêm profile mới
    const addProfRes = await fetch(`${base}/api/profiles`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: 'Smoke Test Profile',
        email: 'smoke.tester@gmail.com',
        quotaRemaining: 90
      })
    });
    assert.equal(addProfRes.status, 201);
    const addProfData = await addProfRes.json();
    const newProfId = addProfData.profile.id;
    console.log(` -> Thêm profile thành công: ${newProfId}`);

    // Nhập tay quota (+/- 5%)
    const quotaRes = await fetch(`${base}/api/profiles/${newProfId}/quota`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ percent: 85 })
    });
    assert.equal(quotaRes.status, 200);
    const qData = await quotaRes.json();
    assert.equal(qData.profile.quotaPercent, 85);
    console.log(' -> Điều chỉnh quota thủ công 85% thành công.');

    // Xóa profile vừa thêm
    const delProfRes = await fetch(`${base}/api/profiles/${newProfId}`, {
      method: 'DELETE'
    });
    assert.equal(delProfRes.status, 200);
    console.log(' -> Xóa profile vừa tạo thành công.');

    // Chặn xóa active profile
    const delActiveRes = await fetch(`${base}/api/profiles/${profData.activeProfileId}`, {
      method: 'DELETE'
    });
    assert.equal(delActiveRes.status, 400);
    console.log(' -> Bảo vệ active profile: chặn xóa an toàn (400).');

    // 4. Task Management Flow
    console.log('[5] Kiểm tra Luồng Tạo, Claim, Đổi trạng thái, Xóa Task...');
    const testTaskId = 'smoke-test-task-' + Date.now().toString(36);
    const createTaskRes = await fetch(`${base}/api/tasks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: testTaskId,
        title: 'Task kiểm thử smoke test',
        goal: 'Kiểm tra luồng CRUD task từ Web UI',
        files: ['tools/ide-bridge/dashboard/public/app.js'],
        steps: ['Tạo task', 'Nhận task', 'Hoàn tất'],
        acceptance_criteria: ['Pass 100% không lỗi']
      })
    });
    assert.equal(createTaskRes.status, 201);
    console.log(` -> Tạo task thành công: ${testTaskId}`);

    // Claim task
    const claimRes = await fetch(`${base}/api/tasks/${testTaskId}/claim`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ worker: 'smoke-worker-1' })
    });
    assert.equal(claimRes.status, 200);
    const claimData = await claimRes.json();
    assert.equal(claimData.task.status, 'in_progress');
    console.log(' -> Claim task thành công (in_progress).');

    // Block task
    const blockRes = await fetch(`${base}/api/tasks/${testTaskId}/status`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status: 'blocked', reason: 'Smoke test block' })
    });
    assert.equal(blockRes.status, 200);
    const blockData = await blockRes.json();
    assert.equal(blockData.task.status, 'blocked');
    console.log(' -> Báo blocked thành công.');

    // Done task
    const doneRes = await fetch(`${base}/api/tasks/${testTaskId}/status`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        status: 'done',
        summary: 'Smoke test completed',
        validation: ['Smoke test verified']
      })
    });
    assert.equal(doneRes.status, 200);
    const doneData = await doneRes.json();
    assert.equal(doneData.task.status, 'done');
    console.log(' -> Đánh dấu hoàn tất (done) thành công.');

    // Delete task
    const delTaskRes = await fetch(`${base}/api/tasks/${testTaskId}`, {
      method: 'DELETE'
    });
    assert.equal(delTaskRes.status, 200);
    console.log(' -> Xóa task thành công.');

    // 5. Cài đặt Quota & Cảnh báo
    console.log('[6] Kiểm tra Cài đặt Quota...');
    const setRes = await fetch(`${base}/api/profiles/settings`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ quotaThresholdPercent: 12, autoSwitch: true })
    });
    assert.equal(setRes.status, 200);
    const setData = await setRes.json();
    assert.equal(setData.settings.quotaThresholdPercent, 12);
    console.log(' -> Cập nhật cài đặt thành công.');

    console.log('\n======================================================');
    console.log('✅ TOÀN BỘ SMOKE TEST END-TO-END PASS 100% KHÔNG LỖI!');
    console.log('======================================================\n');
  } finally {
    await new Promise(resolve => server.close(resolve));
  }
}

runE2ESmokeTest().catch(err => {
  console.error('❌ SMOKE TEST THẤT BẠI:', err);
  process.exit(1);
});
