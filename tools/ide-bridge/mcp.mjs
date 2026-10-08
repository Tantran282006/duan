import { fileURLToPath } from 'node:url';
import { TaskStore, BridgeError, object } from './store.mjs';
import { argumentsFor, rootFor } from './config.mjs';

const str = { type: 'string' };
const tools = [
  { name: 'list_tasks', description: 'List project task summaries. Does not execute tasks.', annotations: { readOnlyHint: true },
    inputSchema: { type: 'object', properties: { status: { type: 'string', enum: ['pending', 'in_progress', 'blocked', 'done'] }, offset: { type: 'integer', minimum: 0 } }, additionalProperties: false } },
  { name: 'get_task', description: 'Read one complete task and its IDE result.', annotations: { readOnlyHint: true },
    inputSchema: { type: 'object', properties: { id: str }, required: ['id'], additionalProperties: false } },
  { name: 'claim_task', description: 'Claim a pending task. Use a unique worker ID for this IDE agent session. No code is run.', annotations: { readOnlyHint: false, destructiveHint: false },
    inputSchema: { type: 'object', properties: { id: str, worker: str }, required: ['worker'], additionalProperties: false } },
  { name: 'report_result', description: 'Report done or blocked with actual validation evidence. Uses the revision from claim_task.', annotations: { readOnlyHint: false, destructiveHint: false },
    inputSchema: { type: 'object', properties: { id: str, worker: str, expected_revision: { type: 'integer' }, status: { type: 'string', enum: ['done', 'blocked'] }, summary: str,
      changed_files: { type: 'array', maxItems: 200, items: { type: 'string', maxLength: 240 } }, validation: { type: 'array', items: str },
      commit: { type: ['string', 'null'] } }, required: ['id', 'worker', 'expected_revision', 'status', 'summary', 'changed_files', 'validation'], additionalProperties: false } }
];
export async function callTool(store, name, input) {
  if (name === 'list_tasks') { object(input, ['status', 'offset']); return store.list(input.status, input.offset ?? 0); }
  if (name === 'get_task') { object(input, ['id']); return { task: await store.read(input.id) }; }
  if (name === 'claim_task') return store.claim(input);
  if (name === 'report_result') { object(input, ['id', 'worker', 'expected_revision', 'status', 'summary', 'changed_files', 'validation', 'commit']); const { id, ...result } = input; return store.report(id, result); }
  throw new BridgeError(400, 'Unknown tool');
}
export function serveMcp(store, input = process.stdin, output = process.stdout) {
  let initialized = false, buffer = '', chain = Promise.resolve();
  const emit = value => output.write(JSON.stringify(value) + '\n');
  const error = (id, code, message) => emit({ jsonrpc: '2.0', id, error: { code, message } });
  async function message(line) {
    let request;
    try { request = JSON.parse(line); } catch { error(null, -32700, 'Parse error'); return; }
    if (!request || request.jsonrpc !== '2.0' || typeof request.method !== 'string') { error(request?.id ?? null, -32600, 'Invalid request'); return; }
    const hasId = Object.hasOwn(request, 'id');
    if (!hasId) return; // Notifications receive no response.
    let result;
    if (request.method === 'initialize') {
      const supported = ['2024-11-05', '2025-03-26', '2025-06-18'];
      const protocolVersion = supported.includes(request.params?.protocolVersion) ? request.params.protocolVersion : '2025-06-18';
      initialized = true;
      result = { protocolVersion, capabilities: { tools: { listChanged: false } }, serverInfo: { name: 'phonho-ide-bridge', version: '0.1.0' } };
    } else if (request.method === 'ping') result = {};
    else if (!initialized) { error(request.id, -32000, 'Initialize first'); return; }
    else if (request.method === 'tools/list') result = { tools };
    else if (request.method === 'tools/call') {
      try { const data = await callTool(store, request.params?.name, request.params?.arguments ?? {}); result = { content: [{ type: 'text', text: JSON.stringify(data) }] }; }
      catch (e) { result = { isError: true, content: [{ type: 'text', text: e instanceof BridgeError ? e.message : 'Internal bridge error' }] }; }
    } else { error(request.id, -32601, 'Method not found'); return; }
    emit({ jsonrpc: '2.0', id: request.id, result });
  }
  input.setEncoding('utf8');
  input.on('data', chunk => {
    buffer += chunk;
    while (buffer.includes('\n')) {
      const index = buffer.indexOf('\n'), line = buffer.slice(0, index).replace(/\r$/, ''); buffer = buffer.slice(index + 1);
      if (Buffer.byteLength(line) > 65536) { error(null, -32600, 'Request too large'); continue; }
      if (line.trim()) chain = chain.then(() => message(line)).catch(() => error(null, -32603, 'Internal error'));
    }
    if (Buffer.byteLength(buffer) > 65536) { buffer = ''; error(null, -32600, 'Request too large'); input.destroy(); }
  });
  return () => chain;
}
if (process.argv[1] === fileURLToPath(import.meta.url)) {
  try { const root = await rootFor(argumentsFor()); serveMcp(await new TaskStore(root).init()); }
  catch (e) { console.error(e.message); process.exitCode = 1; }
}
