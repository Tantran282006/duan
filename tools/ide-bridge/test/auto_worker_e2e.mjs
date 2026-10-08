import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
import { IdeAutoWorker } from '../worker.mjs';
import { TaskStore } from '../store.mjs';

test('End-to-End: Task submission -> Auto-worker detection -> Atomic Claim (in_progress) -> Dispatch -> Done', async () => {
  const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), 'ide-worker-e2e-'));
  await fs.writeFile(path.join(tempDir, 'AGENTS.md'), '# Test Root\n');
  await fs.mkdir(path.join(tempDir, '.tasks', 'runtime'), { recursive: true });

  const store = new TaskStore(tempDir);
  await store.init();

  let worker = null;
  try {
    const canaryTask = {
      id: 'canary-auto-e2e-001',
      title: 'Canary automated test task',
      goal: 'Test end-to-end automated worker claim and dispatch',
      files: ['AGENTS.md'],
      steps: ['Check worker status', 'Validate completion'],
      acceptance_criteria: ['Worker claims task automatically', 'Task transitions to in_progress']
    };
    await store.submit(canaryTask);

    const initialTask = await store.read('canary-auto-e2e-001');
    assert.equal(initialTask.status, 'pending');
    assert.equal(initialTask.worker, null);

    // 2. Initialize and start worker
    worker = new IdeAutoWorker(tempDir, { workerId: 'test-e2e-worker' });
    await worker.start();

    // Wait for worker to poll and claim
    let claimed = null;
    for (let i = 0; i < 20; i++) {
      await new Promise(r => setTimeout(r, 200));
      claimed = await store.read('canary-auto-e2e-001');
      if (claimed.status === 'in_progress') break;
    }

    // 3. Verify automatic claim
    assert.equal(claimed.status, 'in_progress');
    assert.equal(claimed.worker, 'test-e2e-worker');
    assert.equal(worker.currentTaskId, 'canary-auto-e2e-001');

    // Verify status file
    const statusContent = JSON.parse(await fs.readFile(worker.statusFile, 'utf8'));
    assert.equal(statusContent.workerOnline, true);
    assert.equal(statusContent.currentTaskId, 'canary-auto-e2e-001');

    // Verify execution payload
    const payload = JSON.parse(await fs.readFile(worker.executionPayloadFile, 'utf8'));
    assert.equal(payload.task_id, 'canary-auto-e2e-001');

    // 4. Simulate task completion (report result)
    await store.report('canary-auto-e2e-001', {
      worker: 'test-e2e-worker',
      expected_revision: claimed.revision,
      status: 'done',
      summary: 'Automated canary task completed successfully',
      changed_files: ['AGENTS.md'],
      validation: ['E2E claim verified', 'Payload verified', 'Worker status verified']
    });

    // Wait for worker to detect completion
    for (let i = 0; i < 25; i++) {
      await worker.checkCurrentTaskCompletion();
      if (worker.currentTaskId === null) break;
      await new Promise(r => setTimeout(r, 200));
    }

    assert.equal(worker.currentTaskId, null);
    assert.ok(worker.lastResult);
    assert.equal(worker.lastResult.status, 'done');
  } catch (err) {
    console.error('TEST ERROR:', err);
    throw err;
  } finally {
    if (worker) await worker.stop();
    await fs.rm(tempDir, { recursive: true, force: true });
  }
});
