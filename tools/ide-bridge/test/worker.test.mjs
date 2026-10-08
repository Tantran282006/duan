import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
import { IdeAutoWorker } from '../worker.mjs';
import { TaskStore } from '../store.mjs';

test('IdeAutoWorker lifecycle, lock, heartbeat and queue policy', async () => {
  const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), 'ide-worker-test-'));
  await fs.writeFile(path.join(tempDir, 'AGENTS.md'), '# Test Root\n');
  await fs.mkdir(path.join(tempDir, '.tasks', 'runtime'), { recursive: true });

  const worker = new IdeAutoWorker(tempDir, { workerId: 'test-worker-alpha' });
  assert.equal(worker.workerId, 'test-worker-alpha');

  // 1. Acquire lock
  await worker.acquireLock();
  const lockContent = JSON.parse(await fs.readFile(worker.lockFile, 'utf8'));
  assert.equal(lockContent.workerId, 'test-worker-alpha');
  assert.equal(lockContent.pid, process.pid);

  // 2. Second worker on same workspace should fail to acquire lock
  const worker2 = new IdeAutoWorker(tempDir, { workerId: 'test-worker-beta' });
  await assert.rejects(
    () => worker2.acquireLock(),
    /Another worker is active/
  );

  // 3. Heartbeat update
  await worker.updateHeartbeat();
  const statusContent = JSON.parse(await fs.readFile(worker.statusFile, 'utf8'));
  assert.equal(statusContent.workerOnline, true);
  assert.equal(statusContent.workerId, 'test-worker-alpha');
  assert.equal(statusContent.agentRunner, 'agentapi');

  // 4. Test Task Claim & Dispatch Payload
  const store = new TaskStore(tempDir);
  await store.init();
  await store.submit({
    id: 'test-auto-dispatch-001',
    title: 'Test dispatch title',
    goal: 'Test dispatch goal',
    files: ['test.txt'],
    steps: ['Step 1'],
    acceptance_criteria: ['Criterion 1']
  });

  const claimed = await store.claim({ worker: worker.workerId, id: 'test-auto-dispatch-001' });
  assert.equal(claimed.task.status, 'in_progress');
  assert.equal(claimed.task.worker, 'test-worker-alpha');

  // Verify dispatch payload creation
  await worker.dispatchTaskToAgent(claimed.task);
  const payload = JSON.parse(await fs.readFile(worker.executionPayloadFile, 'utf8'));
  assert.equal(payload.task_id, 'test-auto-dispatch-001');
  assert.equal(payload.title, 'Test dispatch title');
  assert.equal(payload.worker_id, 'test-worker-alpha');

  const currentMd = await fs.readFile(worker.currentTaskFile, 'utf8');
  assert.match(currentMd, /test-auto-dispatch-001/);

  // 5. Test Lease recovery in store
  const staleTime = new Date(Date.now() - 700000).toISOString();
  claimed.task.updated_at = staleTime;
  await store.save(claimed.task);

  const recovered = await store.recoverStaleTasks(600000);
  assert.deepEqual(recovered, ['test-auto-dispatch-001']);

  const afterRecovery = await store.read('test-auto-dispatch-001');
  assert.equal(afterRecovery.status, 'pending');
  assert.equal(afterRecovery.worker, null);

  // 6. Release lock
  await worker.releaseLock();
  await assert.rejects(() => fs.access(worker.lockFile));

  // Cleanup
  await fs.rm(tempDir, { recursive: true, force: true });
});
