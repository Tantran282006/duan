import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { spawn } from 'node:child_process';
import http from 'node:http';
import { createInterface } from 'node:readline';
import { fileURLToPath } from 'node:url';
import { TaskStore, validateTask } from '../store.mjs';
import { createBridgeServer } from '../server.mjs';
import { setup } from '../setup.mjs';
import { actionSchema } from '../openapi.mjs';

const task = (id = 'character-test-001') => ({ id, title: 'Character check', goal: 'Compile and inspect the Character sheets.', files: ['PROGRESS.md', 'Assets/PhoNho/Editor/CharacterSheetTools.cs'],
  steps: ['Compile Unity', 'Inspect the pivot'], acceptance_criteria: ['No compiler errors', 'Feet are aligned'] });
async function fixture(t) {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), 'phonho-bridge-'));
  await fs.writeFile(path.join(root, 'AGENTS.md'), '# Test project\n');
  t.after(() => fs.rm(root, { recursive: true, force: true }));
  return { root, store: await new TaskStore(root).init() };
}
test('task storage: idempotence, conflicting retries, retained Markdown, strict path/schema', async t => {
  const { root, store } = await fixture(t);
  const first = await store.submit(task()); assert.equal(first.created, true);
  const retry = await store.submit(task()); assert.equal(retry.created, false); assert.equal(retry.task.revision, 1);
  await assert.rejects(store.submit({ ...task(), goal: 'Different work' }), e => e.status === 409);
  assert.match(await fs.readFile(path.join(root, '.tasks/runtime/character-test-001.md'), 'utf8'), /Character check/);
  assert.throws(() => validateTask({ ...task(), shell: 'anything' }));
  for (const file of ['../outside', '/tmp/outside', 'D:\\project\\outside', '.git/config']) assert.throws(() => validateTask({ ...task(), files: [file] }));
  assert.throws(() => validateTask({ ...task(), id: '../outside' }));
  assert.throws(() => validateTask({ ...task(), steps: Array(30).fill('x'.repeat(1500)) }), /32 KiB/);
  // A missing Markdown view can be repaired without duplicating the task.
  await fs.unlink(path.join(root, '.tasks/runtime/character-test-001.md'));
  await store.submit(task()); await fs.access(path.join(root, '.tasks/runtime/character-test-001.md'));
});
test('separate store instances cannot claim the same task; result ownership/revision and retries', async t => {
  const { root, store } = await fixture(t); await store.submit(task());
  const other = await new TaskStore(root).init();
  const claims = await Promise.allSettled([store.claim({ id: task().id, worker: 'worker-a' }), other.claim({ id: task().id, worker: 'worker-b' })]);
  assert.equal(claims.filter(c => c.status === 'fulfilled').length, 1);
  const claimed = claims.find(c => c.status === 'fulfilled').value.task;
  assert.equal(claimed.revision, 2);
  const result = { worker: claimed.worker, expected_revision: claimed.revision, status: 'done', summary: 'Unity test passed', changed_files: ['PROGRESS.md'], validation: ['Compile passed'] };
  await assert.rejects(store.report(claimed.id, { ...result, worker: 'wrong-worker' }), e => e.status === 409);
  await assert.rejects(store.report(claimed.id, { ...result, expected_revision: 1 }), e => e.status === 409);
  assert.equal((await store.report(claimed.id, result)).task.status, 'done');
  assert.equal((await store.report(claimed.id, result)).task.revision, 3);
  assert.equal((await store.list('done')).tasks.length, 1);
  assert.equal((await store.claim({ worker: 'next-session' })).task, null);
});
test('queue symlinks cannot write outside project', async t => {
  const { root, store } = await fixture(t);
  const outside = path.join(root, 'outside.json'); await fs.writeFile(outside, 'untouched');
  await fs.symlink(outside, path.join(root, '.tasks/runtime/evil.json'));
  await assert.rejects(store.submit(task('evil')), e => e.status === 409);
  assert.equal(await fs.readFile(outside, 'utf8'), 'untouched');
});
test('REST auth, roles, body limits, cross-origin/host, task round trip and rate limit', async t => {
  const { store } = await fixture(t);
  const taskToken = 'a'.repeat(64), workerToken = 'b'.repeat(64);
  assert.throws(() => createBridgeServer({ store, taskToken: 'weak', workerToken }));
  const server = createBridgeServer({ store, taskToken, workerToken });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }));
  const url = 'http://127.0.0.1:' + server.address().port;
  const request = (route, { token = taskToken, data, method = data ? 'POST' : 'GET', headers = {} } = {}) => fetch(url + route, {
    method, headers: { Authorization: 'Bearer ' + token, ...(data ? { 'Content-Type': 'application/json' } : {}), ...headers }, body: data ? JSON.stringify(data) : undefined });
  assert.equal((await fetch(url + '/health')).status, 401);
  assert.equal((await request('/health', { token: 'wrong' })).status, 401);
  assert.equal((await request('/health', { headers: { Origin: 'https://evil.example' } })).status, 403);
  // Node fetch can replace Host; send an actual HTTP Host header for this check.
  const badHost = await new Promise((resolve, reject) => {
    const req = http.get(url + '/health', { headers: { Host: 'evil.example', Authorization: 'Bearer ' + taskToken } }, res => { res.resume(); resolve(res.statusCode); });
    req.on('error', reject);
  });
  assert.equal(badHost, 403);
  assert.equal((await request('/v1/claim', { data: { worker: 'ide' } })).status, 403);
  assert.equal((await request('/v1/tasks', { token: workerToken, data: task() })).status, 403);
  assert.equal((await request('/v1/tasks', { data: { ...task(), command: 'anything' } })).status, 400);
  assert.equal((await request('/v1/tasks', { data: task(), headers: { 'Content-Type': 'text/plain' } })).status, 415);
  assert.equal((await request('/v1/tasks', { data: { ...task(), goal: 'x'.repeat(70000) } })).status, 413);
  assert.equal((await request('/v1/tasks', { data: task() })).status, 201);
  assert.equal((await request('/v1/tasks', { data: task() })).status, 200);
  const claimed = await (await request('/v1/claim', { token: workerToken, data: { worker: 'ide-session' } })).json();
  const result = { worker: claimed.task.worker, expected_revision: claimed.task.revision, status: 'done', summary: 'Pass', changed_files: [], validation: ['Actual check'] };
  assert.equal((await request('/v1/tasks/' + task().id + '/result', { data: result })).status, 403);
  assert.equal((await request('/v1/tasks/' + task().id + '/result', { token: workerToken, data: result })).status, 200);
  const read = await (await request('/v1/tasks/' + task().id)).json(); assert.equal(read.task.result.summary, 'Pass');
  let last; for (let i = 0; i < 120; i++) last = await request('/health');
  assert.equal(last.status, 429);
});
test('setup keeps secrets on rerun, generates absolute MCP config and HTTPS Action schema', async t => {
  const { root } = await fixture(t);
  await setup(root, 'https://demo.example');
  const secrets = await fs.readFile(path.join(root, '.ide-bridge/secrets.env'), 'utf8');
  assert.match(secrets, /BRIDGE_TASK_TOKEN=[a-f0-9]{64}/); assert.match(secrets, /BRIDGE_WORKER_TOKEN=[a-f0-9]{64}/);
  await setup(root, 'https://next.example');
  const later = await fs.readFile(path.join(root, '.ide-bridge/secrets.env'), 'utf8');
  assert.equal(secrets.split('\n')[0], later.split('\n')[0]);
  const config = JSON.parse(await fs.readFile(path.join(root, '.ide-bridge/mcp_config.json')));
  assert.ok(path.isAbsolute(config.mcpServers['phonho-task-bridge'].command));
  assert.equal(config.mcpServers['phonho-task-bridge'].args.at(-1), root);
  const schema = JSON.parse(await fs.readFile(path.join(root, '.ide-bridge/openapi.json')));
  assert.equal(schema.servers[0].url, 'https://next.example');
  await assert.rejects(setup(root, 'http://localhost:5000'));
  assert.equal(actionSchema('https://demo.example').paths['/v1/tasks'].post['x-openai-isConsequential'], false);
});
test('end to end: REST submission, MCP stdio claim/result, REST retrieval', async t => {
  const { root, store } = await fixture(t);
  const taskToken = 'a'.repeat(64), workerToken = 'b'.repeat(64);
  const server = createBridgeServer({ store, taskToken, workerToken });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }));
  const base = 'http://127.0.0.1:' + server.address().port;
  const submitted = await fetch(base + '/v1/tasks', { method: 'POST', headers: { Authorization: 'Bearer ' + taskToken, 'Content-Type': 'application/json' }, body: JSON.stringify(task()) });
  assert.equal(submitted.status, 201);
  const mcp = fileURLToPath(new URL('../mcp.mjs', import.meta.url));
  const child = spawn(process.execPath, [mcp, '--root', root], { stdio: ['pipe', 'pipe', 'pipe'] });
  t.after(() => { child.stdin.end(); child.kill(); });
  let stderr = '', id = 0; child.stderr.on('data', data => stderr += data);
  const pending = new Map();
  createInterface({ input: child.stdout }).on('line', line => { const value = JSON.parse(line); const resolve = pending.get(value.id); if (resolve) { pending.delete(value.id); resolve(value); } });
  async function rpc(method, params) {
    const requestId = ++id;
    const promise = new Promise((resolve, reject) => { pending.set(requestId, resolve); setTimeout(() => { if (pending.delete(requestId)) reject(new Error('MCP timeout: ' + stderr)); }, 3000).unref(); });
    child.stdin.write(JSON.stringify({ jsonrpc: '2.0', id: requestId, method, params }) + '\n'); return promise;
  }
  const init = await rpc('initialize', { protocolVersion: '2025-06-18', capabilities: {}, clientInfo: { name: 'test', version: '1' } });
  assert.equal(init.result.protocolVersion, '2025-06-18');
  child.stdin.write(JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' }) + '\n');
  assert.equal((await rpc('tools/list', {})).result.tools.length, 4);
  const response = await rpc('tools/call', { name: 'claim_task', arguments: { worker: 'mcp-test-session' } });
  const claimed = JSON.parse(response.result.content[0].text).task;
  const report = await rpc('tools/call', { name: 'report_result', arguments: { id: claimed.id, worker: claimed.worker,
    expected_revision: claimed.revision, status: 'blocked', summary: 'Unity unavailable', changed_files: [], validation: ['Unity not executed'] } });
  assert.equal(report.result.isError, undefined); assert.equal((await store.read(task().id)).status, 'blocked');
  const retrieved = await (await fetch(base + '/v1/tasks/' + task().id, { headers: { Authorization: 'Bearer ' + taskToken } })).json();
  assert.equal(retrieved.task.result.summary, 'Unity unavailable');
  assert.equal((await rpc('tools/call', { name: 'not_a_tool', arguments: {} })).result.isError, true);
  assert.equal(stderr, '');
});
