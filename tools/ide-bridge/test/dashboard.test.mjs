import test from 'node:test';
import assert from 'node:assert/strict';
import http from 'node:http';
import path from 'node:path';
import fs from 'node:fs/promises';
import fsSync from 'node:fs';
import os from 'node:os';
import { createDashboardServer } from '../dashboard/server.mjs';
import {
  parseProgressMd,
  hashPin,
  maskEmail,
  loadProfiles,
  saveProfiles,
  parseProfileIdFromWorker,
  countTurns5h,
  isQuotaStale,
  recordWorkerTurn,
  handleQuotaErrorEvent
} from '../dashboard/data.mjs';

async function fixture(t) {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), 'phonho-dash-'));
  await fs.mkdir(path.join(root, '.tasks', 'runtime'), { recursive: true });
  await fs.mkdir(path.join(root, '.ide-bridge'), { recursive: true });
  await fs.writeFile(path.join(root, 'PROGRESS.md'), `# PROGRESS.md\n\n## TRẠNG THÁI HIỆN TẠI\n\n- **Mốc hiện tại**: M0 (Khởi tạo dự án)\n\n## VIỆC TIẾP THEO\n\n1. Bước 1\n2. Bước 2\n\n## NHẬT KÝ PHIÊN\n\n### Phiên 1 — 2026-10-01 (Phiên test)\n- **Mốc**: M0\n- **Đã làm**:\n  - Khởi tạo repo\n- **File chính**: file1.txt\n- **Kiểm tra**: pass 100%\n`);
  t.after(async () => {
    await fs.rm(root, { recursive: true, force: true }).catch(() => {});
  });
  return root;
}

test('data parsing: progress md and masked emails', async (t) => {
  const root = await fixture(t);
  const progress = await parseProgressMd(root);
  assert.equal(progress.currentMilestone, 'M0 (Khởi tạo dự án)');
  assert.equal(progress.nextItems.length, 2);
  assert.equal(progress.sessions.length, 1);
  assert.equal(progress.sessions[0].sessionNumber, 1);
  assert.equal(progress.sessions[0].verification, 'pass 100%');

  assert.equal(maskEmail('tantran@gmail.com'), 't***n@gmail.com');
  assert.equal(maskEmail('a@b.com'), 'a***@b.com');
  assert.equal(hashPin('1234'), hashPin('1234'));
});

test('quota 3 transparent sources, worker parsing, turns and stale detection', async (t) => {
  const root = await fixture(t);

  // 1. Worker ID parsing contains profile_id
  const sampleProfiles = [
    { id: 'profile-main', name: 'Main' },
    { id: 'profile-backup-1', name: 'Backup' }
  ];
  assert.equal(parseProfileIdFromWorker('antigravity-worker-profile-main-session13', sampleProfiles), 'profile-main');
  assert.equal(parseProfileIdFromWorker('worker-profile-backup-1-abc', sampleProfiles), 'profile-backup-1');
  assert.equal(parseProfileIdFromWorker('unknown-worker', sampleProfiles), null);

  // 2. Turns counting in 5-hour window
  const now = Date.now();
  const mockTurns = [
    { timestamp: new Date(now - 1000).toISOString() },
    { timestamp: new Date(now - 2 * 3600 * 1000).toISOString() },
    { timestamp: new Date(now - 6 * 3600 * 1000).toISOString() } // Exceeded 5 hours
  ];
  assert.equal(countTurns5h(mockTurns), 2);

  // 3. Stale detection (> 30 mins)
  assert.equal(isQuotaStale(null), true);
  assert.equal(isQuotaStale(new Date(now - 35 * 60 * 1000).toISOString()), true);
  assert.equal(isQuotaStale(new Date(now - 10 * 60 * 1000).toISOString()), false);

  // 4. Default initialization: no fabricated numbers, quotaPercent is null
  const initialData = await loadProfiles(root);
  const mainP = initialData.profiles.find(p => p.id === 'profile-main');
  assert.equal(mainP.quotaPercent, null);
  assert.equal(mainP.quotaSource, 'chưa rõ');

  // 5. Source: "ước tính theo lượt"
  await recordWorkerTurn(root, 'antigravity-worker-profile-main-session13', 'claim', 'task-1');
  await recordWorkerTurn(root, 'antigravity-worker-profile-main-session13', 'report', 'task-1');
  const dataAfterTurns = await loadProfiles(root);
  const mainAfterTurns = dataAfterTurns.profiles.find(p => p.id === 'profile-main');
  assert.equal(mainAfterTurns.turns5h, 2);

  // 6. Source: "lỗi quota" (ResourceExhausted / 429)
  await handleQuotaErrorEvent(root, 'antigravity-worker-profile-main-session13', 'ResourceExhausted quota exceeded');
  const dataAfterError = await loadProfiles(root);
  const mainAfterError = dataAfterError.profiles.find(p => p.id === 'profile-main');
  assert.equal(mainAfterError.status, 'hết token');
  assert.equal(mainAfterError.quotaPercent, 0);
  assert.equal(mainAfterError.quotaSource, 'lỗi quota');
  assert.ok(dataAfterError.suggestedSwitch);
  assert.equal(dataAfterError.suggestedSwitch.targetId, 'profile-backup-1');
});

test('dashboard server: security, overview, manual quota adjustment and profile management', async (t) => {
  const root = await fixture(t);
  const env = { BRIDGE_PUBLIC_URL: 'https://game.zcloudviet.xyz' };
  const server = createDashboardServer(root, env);
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => server.close(resolve)));

  const port = server.address().port;
  const base = `http://127.0.0.1:${port}`;

  // 1. Health check
  const healthRes = await fetch(`${base}/api/health`);
  assert.equal(healthRes.status, 200);
  const healthData = await healthRes.json();
  assert.equal(healthData.ok, true);

  // 2. Reject foreign host
  const foreignRes = await new Promise((resolve) => {
    const req = http.request({
      hostname: '127.0.0.1',
      port,
      path: '/api/health',
      headers: { host: 'evil.com' }
    }, (res) => resolve(res));
    req.end();
  });
  assert.equal(foreignRes.statusCode, 403);

  // 3. Overview API
  const overviewRes = await fetch(`${base}/api/overview`);
  assert.equal(overviewRes.status, 200);
  const overviewData = await overviewRes.json();
  assert.equal(overviewData.currentMilestone, 'M0 (Khởi tạo dự án)');
  assert.equal(overviewData.recentSessions.length, 1);

  // 4. Profiles API & Manual Quota Adjustment (nhập tay)
  const quotaRes = await fetch(`${base}/api/profiles/profile-main/quota`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ percent: 80 })
  });
  assert.equal(quotaRes.status, 200);
  const quotaData = await quotaRes.json();
  assert.equal(quotaData.profile.quotaPercent, 80);
  assert.equal(quotaData.profile.quotaSource, 'nhập tay');
  assert.equal(quotaData.profile.isStale, false);

  // Delta adjustment (+/-)
  const deltaRes = await fetch(`${base}/api/profiles/profile-main/quota`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ delta: -5 })
  });
  assert.equal(deltaRes.status, 200);
  const deltaData = await deltaRes.json();
  assert.equal(deltaData.profile.quotaPercent, 75);

  // 5. Profile Switch
  const profilesRes = await fetch(`${base}/api/profiles`);
  const profilesData = await profilesRes.json();
  const targetId = profilesData.profiles[1].id;
  const switchRes = await fetch(`${base}/api/profiles/switch`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ targetId, reason: 'Test switch' })
  });
  assert.equal(switchRes.status, 200);
  const switchData = await switchRes.json();
  assert.equal(switchData.activeProfileId, targetId);

  // 6. Static asset serving
  const indexRes = await fetch(`${base}/`);
  assert.equal(indexRes.status, 200);
  assert.ok(indexRes.headers.get('content-type').includes('text/html'));
});

test('dashboard server: task creation, claiming, status transition, delete and profile deletion', async (t) => {
  const root = await fixture(t);
  const env = { BRIDGE_PUBLIC_URL: 'https://game.zcloudviet.xyz' };
  const server = createDashboardServer(root, env);
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => server.close(resolve)));

  const port = server.address().port;
  const base = `http://127.0.0.1:${port}`;

  // 1. Create Task via POST /api/tasks
  const newTaskRes = await fetch(`${base}/api/tasks`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      id: 'dashboard-test-task-01',
      title: 'Task kiểm thử tạo từ web dashboard',
      goal: 'Kiểm tra toàn bộ luồng tạo và thao tác task từ Web Dashboard',
      files: ['tools/ide-bridge/dashboard/server.mjs'],
      steps: ['Bước 1: Tạo task', 'Bước 2: Claim', 'Bước 3: Hoàn thành'],
      acceptance_criteria: ['Nghiệm thu pass 100%']
    })
  });
  const newTaskData = await newTaskRes.json();
  assert.equal(newTaskRes.status, 201);
  assert.equal(newTaskData.created, true);
  assert.equal(newTaskData.task.id, 'dashboard-test-task-01');
  assert.equal(newTaskData.task.status, 'pending');

  // Verify task appears in GET /api/tasks
  const getTasksRes = await fetch(`${base}/api/tasks`);
  const getTasksData = await getTasksRes.json();
  const createdTask = getTasksData.tasks.find(tk => tk.id === 'dashboard-test-task-01');
  assert.ok(createdTask, 'Task vừa tạo phải có trong danh sách');
  assert.equal(createdTask.status, 'pending');

  // 2. Claim Task via POST /api/tasks/:id/claim
  const claimRes = await fetch(`${base}/api/tasks/dashboard-test-task-01/claim`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ worker: 'dashboard-worker-test' })
  });
  assert.equal(claimRes.status, 200);
  const claimData = await claimRes.json();
  assert.equal(claimData.task.status, 'in_progress');
  assert.equal(claimData.task.worker, 'dashboard-worker-test');

  // 3. Status transition via POST /api/tasks/:id/status
  // In Progress -> Blocked
  const blockRes = await fetch(`${base}/api/tasks/dashboard-test-task-01/status`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ status: 'blocked', reason: 'Chờ thiết kế từ UI team' })
  });
  assert.equal(blockRes.status, 200);
  const blockData = await blockRes.json();
  assert.equal(blockData.task.status, 'blocked');

  // Blocked -> Pending (Unclaim)
  const unclaimRes = await fetch(`${base}/api/tasks/dashboard-test-task-01/status`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ status: 'pending' })
  });
  assert.equal(unclaimRes.status, 200);
  const unclaimData = await unclaimRes.json();
  assert.equal(unclaimData.task.status, 'pending');
  assert.equal(unclaimData.task.worker, null);

  // Pending -> Done
  const doneRes = await fetch(`${base}/api/tasks/dashboard-test-task-01/status`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      status: 'done',
      summary: 'Task hoàn thành từ smoke test',
      validation: ['Unit test pass', 'E2E smoke test pass']
    })
  });
  assert.equal(doneRes.status, 200);
  const doneData = await doneRes.json();
  assert.equal(doneData.task.status, 'done');
  assert.ok(doneData.task.result);

  // 4. Delete Task via DELETE /api/tasks/:id
  const deleteRes = await fetch(`${base}/api/tasks/dashboard-test-task-01`, {
    method: 'DELETE'
  });
  assert.equal(deleteRes.status, 200);
  const deleteData = await deleteRes.json();
  assert.equal(deleteData.ok, true);

  // Verify task deleted from runtime
  const getTasksAfterDel = await fetch(`${base}/api/tasks`);
  const getTasksAfterDelData = await getTasksAfterDel.json();
  assert.equal(getTasksAfterDelData.tasks.some(tk => tk.id === 'dashboard-test-task-01'), false);

  // 5. Add and Delete Profile
  const addProfileRes = await fetch(`${base}/api/profiles`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: 'Test Secondary Profile',
      email: 'secondary.test@gmail.com',
      quotaRemaining: 95
    })
  });
  assert.equal(addProfileRes.status, 201);
  const addProfileData = await addProfileRes.json();
  const createdProfileId = addProfileData.profile.id;

  // Cannot delete active profile
  const delActiveRes = await fetch(`${base}/api/profiles/profile-main`, {
    method: 'DELETE'
  });
  assert.equal(delActiveRes.status, 400);

  // Delete non-active profile
  const delProfileRes = await fetch(`${base}/api/profiles/${createdProfileId}`, {
    method: 'DELETE'
  });
  assert.equal(delProfileRes.status, 200);
  const delProfileData = await delProfileRes.json();
  assert.equal(delProfileData.profile.id, createdProfileId);

  // 6. Settings
  const settingsRes = await fetch(`${base}/api/profiles/settings`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      quotaThresholdPercent: 15,
      autoSwitch: true
    })
  });
  assert.equal(settingsRes.status, 200);
  const settingsData = await settingsRes.json();
  assert.equal(settingsData.settings.quotaThresholdPercent, 15);
  assert.equal(settingsData.settings.autoSwitch, true);
});

test('MAX VFX UI: visual elements, ambient canvas, sound synth and toast subsystems', async () => {
  const baseDir = fsSync.existsSync(path.resolve('tools/ide-bridge/dashboard/public/index.html'))
    ? path.resolve('tools/ide-bridge/dashboard/public')
    : (fsSync.existsSync(path.resolve('dashboard/public/index.html')) ? path.resolve('dashboard/public') : path.resolve('../dashboard/public'));
  const htmlPath = path.join(baseDir, 'index.html');
  const cssPath = path.join(baseDir, 'styles.css');
  const jsPath = path.join(baseDir, 'app.js');

  const html = await fs.readFile(htmlPath, 'utf8');
  const css = await fs.readFile(cssPath, 'utf8');
  const js = await fs.readFile(jsPath, 'utf8');

  // 1. HTML visual structure
  assert.ok(html.includes('id="ambient-canvas"'), 'index.html phải có canvas #ambient-canvas');
  assert.ok(html.includes('id="btn-toggle-sound"'), 'index.html phải có nút toggle sound');
  assert.ok(html.includes('id="toast-container"'), 'index.html phải có container #toast-container');

  // 2. CSS MAX VFX tokens and keyframe animations
  assert.ok(css.includes('#ambient-canvas'), 'styles.css phải có style cho #ambient-canvas');
  assert.ok(css.includes('.toast-container') && css.includes('.toast-item'), 'styles.css phải có style cho toasts');
  assert.ok(css.includes('.ripple-wave') && css.includes('rippleEffect'), 'styles.css phải có hiệu ứng ripple');
  assert.ok(css.includes('shimmerSweep'), 'styles.css phải có animation shimmer');
  assert.ok(css.includes('pulseGlowBadge'), 'styles.css phải có animation pulse badge');

  // 3. JavaScript subsystems
  assert.ok(js.includes('SoundSynth ='), 'app.js phải có module SoundSynth');
  assert.ok(js.includes('initAmbientCanvas()'), 'app.js phải có hàm initAmbientCanvas');
  assert.ok(js.includes('showToast('), 'app.js phải có hàm showToast');
  assert.ok(js.includes('animateNumber('), 'app.js phải có hàm animateNumber');
  assert.ok(js.includes('setupRippleEffects()'), 'app.js phải có hàm setupRippleEffects');
});

