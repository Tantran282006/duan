import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import { tmpdir } from 'node:os';
import { randomUUID } from 'node:crypto';
import { TaskStore } from '../store.mjs';
import { callTool } from '../mcp.mjs';
import { readProjectMemory, updateProjectMemoryWithTask, getProjectContext, createProgressSnapshot, listProgressSnapshots } from '../memory.mjs';

async function createTempWorkspace() {
  const root = path.join(tmpdir(), 'phonho-test-' + randomUUID());
  await fs.mkdir(root, { recursive: true });
  await fs.writeFile(path.join(root, 'AGENTS.md'), '# Agents\n', 'utf8');
  return root;
}

test('project memory: default generation and reading', async () => {
  const root = await createTempWorkspace();
  try {
    const memory = await readProjectMemory(root);
    assert.match(memory, /PROJECT_STATE\.md/);
    assert.match(memory, /Mục tiêu Dự án/);
    assert.match(memory, /Kiến trúc & Công nghệ Hiện tại/);

    // Second read should read the existing file
    const secondRead = await readProjectMemory(root);
    assert.equal(secondRead, memory);
  } finally {
    await fs.rm(root, { recursive: true, force: true });
  }
});

test('project memory: updating memory with completed task summarizes cleanly without log bloat', async () => {
  const root = await createTempWorkspace();
  try {
    const store = await new TaskStore(root).init();
    await store.submit({
      id: 'test-feat-01',
      title: 'Triển khai tính năng trà sữa',
      goal: 'Mô tả chi tiết mục tiêu rất dài nhằm kiểm tra không bị sao chép vào memory...',
      files: ['Assets/Scripts'],
      steps: ['Bước 1', 'Bước 2', 'Bước 3'],
      acceptance_criteria: ['Tiêu chí 1']
    });

    await store.claim({ id: 'test-feat-01', worker: 'worker-1' });
    await store.report('test-feat-01', {
      worker: 'worker-1',
      expected_revision: 2,
      status: 'done',
      summary: 'Đã hoàn thành xuất sắc mô-đun Trà Sữa.\nChi tiết log 1...\nChi tiết log 2...',
      changed_files: ['Assets/Scripts'],
      validation: ['Pass 100%'],
      commit: null
    });

    const memory = await readProjectMemory(root);
    assert.match(memory, /test-feat-01/);
    assert.match(memory, /Đã hoàn thành xuất sắc mô-đun Trà Sữa/);
    // Multi-line log should NOT be in the memory bullet
    assert.doesNotMatch(memory, /Chi tiết log 1/);
  } finally {
    await fs.rm(root, { recursive: true, force: true });
  }
});

test('task archive: safe archiving of done tasks, lookup fallback and listing', async () => {
  const root = await createTempWorkspace();
  try {
    const store = await new TaskStore(root).init();

    await store.submit({
      id: 'task-archive-demo-1',
      title: 'Task cần archive',
      goal: 'Mục tiêu ngắn',
      files: ['Assets/Scripts'],
      steps: ['Bước 1'],
      acceptance_criteria: ['Tiêu chí 1']
    });

    // Cannot archive pending task
    await assert.rejects(
      async () => await store.archive('task-archive-demo-1'),
      /Only completed tasks/
    );

    // Complete task
    await store.claim({ id: 'task-archive-demo-1', worker: 'worker-1' });
    await store.report('task-archive-demo-1', {
      worker: 'worker-1',
      expected_revision: 2,
      status: 'done',
      summary: 'Hoàn thành task archive demo',
      changed_files: ['Assets/Scripts'],
      validation: ['OK'],
      commit: null
    });

    // Archive task
    const archResult = await store.archive('task-archive-demo-1');
    assert.equal(archResult.archived, true);
    assert.equal(archResult.task.is_archived, true);

    // Active list should NOT include archived task
    const activeList = await store.list();
    assert.equal(activeList.tasks.some(t => t.id === 'task-archive-demo-1'), false);

    // read() should still seamlessly retrieve the archived task
    const readArchived = await store.read('task-archive-demo-1');
    assert.equal(readArchived.id, 'task-archive-demo-1');
    assert.equal(readArchived.is_archived, true);

    // listArchived() returns the archived task
    const archList = await store.listArchived();
    assert.equal(archList.tasks.length, 1);
    assert.equal(archList.tasks[0].id, 'task-archive-demo-1');
    assert.equal(archList.tasks[0].is_archived, true);

    // list('archived') also returns archived tasks
    const archListViaList = await store.list('archived');
    assert.equal(archListViaList.tasks.length, 1);
  } finally {
    await fs.rm(root, { recursive: true, force: true });
  }
});

test('auto-archive: keeps recent done tasks and archives older done tasks', async () => {
  const root = await createTempWorkspace();
  try {
    const store = await new TaskStore(root).init();

    // Create and complete 4 tasks
    for (let i = 1; i <= 4; i++) {
      const id = `task-auto-${i}`;
      await store.submit({
        id,
        title: `Task auto ${i}`,
        goal: `Goal ${i}`,
        files: ['Assets/Scripts'],
        steps: ['S1'],
        acceptance_criteria: ['AC1']
      });
      await store.claim({ id, worker: 'w' });
      await store.report(id, {
        worker: 'w',
        expected_revision: 2,
        status: 'done',
        summary: `Summary ${i}`,
        changed_files: ['Assets/Scripts'],
        validation: ['OK'],
        commit: null
      });
      // Small delay to ensure timestamp progression
      await new Promise(r => setTimeout(r, 10));
    }

    // By default autoArchiveDone keeps 2 recent done tasks
    const activeTasks = await store.list();
    const archivedTasks = await store.listArchived();

    // Should have 2 recent active done tasks and 2 archived tasks
    assert.equal(activeTasks.tasks.length, 2);
    assert.equal(archivedTasks.tasks.length, 2);

    // All 4 tasks must still be accessible via read()
    for (let i = 1; i <= 4; i++) {
      const t = await store.read(`task-auto-${i}`);
      assert.equal(t.id, `task-auto-${i}`);
    }
  } finally {
    await fs.rm(root, { recursive: true, force: true });
  }
});

test('get_project_context: returns condensed memory and active tasks only in 1 call', async () => {
  const root = await createTempWorkspace();
  try {
    const store = await new TaskStore(root).init();

    // Submit pending task
    await store.submit({
      id: 'task-pending-1',
      title: 'Task đang chờ',
      goal: 'Cần làm tính năng ABC',
      files: ['Assets/Scripts'],
      steps: ['B1'],
      acceptance_criteria: ['TC1']
    });

    // Submit and claim in_progress task
    await store.submit({
      id: 'task-in-prog-1',
      title: 'Task đang làm',
      goal: 'Đang triển khai XYZ',
      files: ['Assets/Scripts'],
      steps: ['B1'],
      acceptance_criteria: ['TC1']
    });
    await store.claim({ id: 'task-in-prog-1', worker: 'worker-active' });

    // Submit and complete done task (which gets archived)
    await store.submit({
      id: 'task-done-1',
      title: 'Task đã xong',
      goal: 'Đã hoàn thành',
      files: ['Assets/Scripts'],
      steps: ['B1'],
      acceptance_criteria: ['TC1']
    });
    await store.claim({ id: 'task-done-1', worker: 'worker-done' });
    await store.report('task-done-1', {
      worker: 'worker-done',
      expected_revision: 2,
      status: 'done',
      summary: 'Đã xong task 1',
      changed_files: ['Assets/Scripts'],
      validation: ['PASS'],
      commit: null
    });
    await store.archive('task-done-1');

    // Call MCP tool get_project_context
    const context = await callTool(store, 'get_project_context', {});
    assert.ok(context.project_memory);
    assert.match(context.project_memory, /Mục tiêu Dự án/);

    // Active tasks must contain pending and in_progress ONLY (not done/archived)
    assert.equal(context.active_tasks.length, 2);
    const ids = context.active_tasks.map(t => t.id);
    assert.ok(ids.includes('task-pending-1'));
    assert.ok(ids.includes('task-in-prog-1'));
    assert.ok(!ids.includes('task-done-1'));

    // Stats check
    assert.equal(context.stats.pending_count, 1);
    assert.equal(context.stats.in_progress_count, 1);
    assert.equal(context.stats.archived_count, 1);
  } finally {
    await fs.rm(root, { recursive: true, force: true });
  }
});

test('progress snapshots: creation, append-only history, distinction between tasks and game progress', async () => {
  const root = await createTempWorkspace();
  try {
    const store = await new TaskStore(root).init();

    await store.submit({
      id: 'task-snap-1',
      title: 'Task 1',
      goal: 'Goal 1',
      files: ['Assets/Scripts'],
      steps: ['S1'],
      acceptance_criteria: ['AC1']
    });
    await store.claim({ id: 'task-snap-1', worker: 'worker-snap' });
    await store.report('task-snap-1', {
      worker: 'worker-snap',
      expected_revision: 2,
      status: 'done',
      summary: 'Task 1 done',
      changed_files: ['Assets/Scripts'],
      validation: ['OK'],
      commit: null
    });
    await store.archive('task-snap-1');

    await store.submit({
      id: 'task-snap-2',
      title: 'Task 2',
      goal: 'Goal 2',
      files: ['Assets/Scripts'],
      steps: ['S2'],
      acceptance_criteria: ['AC2']
    });
    await store.claim({ id: 'task-snap-2', worker: 'worker-snap' });

    // Create 1st snapshot
    const snap1 = await createProgressSnapshot(root, store, {
      milestone: 'M0',
      milestone_m0_percent: 70,
      game_overall_percent: 22
    });

    assert.ok(snap1.snapshot_id);
    assert.equal(snap1.data.task_queue_metrics.counts.archived, 1);
    assert.equal(snap1.data.task_queue_metrics.counts.in_progress, 1);
    assert.equal(snap1.data.game_progress.milestone_progress_percent, 70);
    assert.equal(snap1.data.game_progress.game_overall_progress_percent, 22);

    // Verify files exist
    const jsonExists = await fs.access(path.join(root, 'docs', 'snapshots', `${snap1.snapshot_id}.json`)).then(() => true).catch(() => false);
    const mdExists = await fs.access(path.join(root, 'docs', 'snapshots', `${snap1.snapshot_id}.md`)).then(() => true).catch(() => false);
    const histExists = await fs.access(path.join(root, 'docs', 'snapshots', 'history.json')).then(() => true).catch(() => false);
    const mdHistExists = await fs.access(path.join(root, 'docs', 'snapshots', 'SNAPSHOT_HISTORY.md')).then(() => true).catch(() => false);

    assert.equal(jsonExists, true);
    assert.equal(mdExists, true);
    assert.equal(histExists, true);
    assert.equal(mdHistExists, true);

    // Sleep briefly then create 2nd snapshot to test append-only history preservation
    await new Promise(r => setTimeout(r, 1100));
    const snap2 = await createProgressSnapshot(root, store, {
      milestone: 'M0',
      milestone_m0_percent: 72,
      game_overall_percent: 23
    });

    assert.notEqual(snap1.snapshot_id, snap2.snapshot_id);

    // List snapshots
    const history = await listProgressSnapshots(root);
    assert.equal(history.length, 2);
    assert.equal(history[0].snapshot_id, snap1.snapshot_id);
    assert.equal(history[1].snapshot_id, snap2.snapshot_id);

    // getProjectContext includes snapshots
    const context = await getProjectContext(root, store);
    assert.ok(Array.isArray(context.snapshots));
    assert.equal(context.snapshots.length, 2);
    assert.equal(context.snapshots[0].snapshot_id, snap2.snapshot_id); // newest first
  } finally {
    await fs.rm(root, { recursive: true, force: true });
  }
});

