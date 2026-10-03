import fs from 'node:fs/promises';
import path from 'node:path';
import { randomUUID, createHash } from 'node:crypto';

export class BridgeError extends Error {
  constructor(status, message) { super(message); this.status = status; }
}
const fail = (message) => { throw new BridgeError(400, message); };
export function object(value, keys) {
  if (!value || typeof value !== 'object' || Array.isArray(value)) fail('Expected an object');
  if (Object.keys(value).some(k => !keys.includes(k))) fail('Unknown field');
}
function text(value, name, limit, multiline = true) {
  if (typeof value !== 'string' || !value.trim() || value.length > limit || /\0/.test(value) || (!multiline && /[\r\n]/.test(value))) fail(`Invalid ${name}`);
  return value.trim();
}
export function taskId(value) {
  if (typeof value !== 'string' || !/^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$/.test(value)) fail('Invalid task id');
  return value;
}
export function projectFile(value) {
  const p = text(value, 'project file', 240, false);
  if (p.includes('\\') || p.includes(':') || p.startsWith('/') || p.split('/').some(s => !s || s === '.' || s === '..' || s === '.git')) fail('Project files must be relative paths');
  return p;
}
function strings(value, name, maxCount, limit, mapper) {
  if (!Array.isArray(value) || value.length > maxCount) fail(`Invalid ${name}`);
  return value.map(v => mapper ? mapper(v) : text(v, name, limit));
}
export function validateTask(input) {
  object(input, ['id', 'title', 'goal', 'files', 'steps', 'acceptance_criteria']);
  const task = { id: taskId(input.id), title: text(input.title, 'title', 160, false),
    goal: text(input.goal, 'goal', 10000), files: strings(input.files, 'files', 50, 240, projectFile),
    steps: strings(input.steps, 'steps', 40, 1500),
    acceptance_criteria: strings(input.acceptance_criteria, 'acceptance criteria', 40, 1000) };
  if (Buffer.byteLength(JSON.stringify(task)) > 32768) fail('Task exceeds 32 KiB; split it into smaller tasks');
  return task;
}
function workerName(value) { return text(value, 'worker', 100, false); }
function markdown(t) {
  const list = a => a.length ? a.map(v => `- ${v}`).join('\n') : '- (none)';
  return `# ${t.title}\n\nTask: ${t.id}\nStatus: ${t.status}\nRevision: ${t.revision}\nWorker: ${t.worker ?? '(unclaimed)'}\n\n## Mục tiêu\n\n${t.goal}\n\n## File liên quan\n\n${list(t.files)}\n\n## Các bước\n\n${list(t.steps)}\n\n## Tiêu chí hoàn thành\n\n${list(t.acceptance_criteria)}\n\n## Kết quả IDE\n\n${t.result ? JSON.stringify(t.result, null, 2) : '(Chưa có kết quả)'}\n`;
}

export async function projectRoot(value) {
  const root = await fs.realpath(value);
  await fs.access(path.join(root, 'AGENTS.md'));
  return root;
}
async function safeDirectory(dir) {
  await fs.mkdir(dir, { recursive: true, mode: 0o700 });
  const stat = await fs.lstat(dir);
  if (!stat.isDirectory() || stat.isSymbolicLink() || path.resolve(await fs.realpath(dir)) !== path.resolve(dir))
    throw new BridgeError(409, 'Queue directory cannot be a symlink');
}
async function safeFile(file) {
  try { if ((await fs.lstat(file)).isSymbolicLink()) throw new BridgeError(409, 'Queue file cannot be a symlink'); }
  catch (e) { if (e.code !== 'ENOENT') throw e; }
}
async function atomic(file, value) {
  await safeFile(file);
  const temp = file + '.' + randomUUID() + '.tmp';
  try { await fs.writeFile(temp, value, { flag: 'wx', mode: 0o600 }); await fs.rename(temp, file); }
  finally { await fs.rm(temp, { force: true }); }
}

export class TaskStore {
  constructor(root) { this.root = root; this.dir = path.join(root, '.tasks', 'runtime'); }
  async init() {
    await safeDirectory(path.join(this.root, '.tasks'));
    await safeDirectory(this.dir);
    return this;
  }
  async locked(fn) {
    const lock = path.join(this.dir, '.lock');
    let acquired = false;
    for (let i = 0; i < 100; i++) {
      try { await fs.mkdir(lock, { mode: 0o700 }); acquired = true; break; }
      catch (e) { if (e.code !== 'EEXIST') throw e; await new Promise(r => setTimeout(r, 20)); }
    }
    if (!acquired) throw new BridgeError(409, 'Queue busy or stale lock; see troubleshooting');
    try { return await fn(); } finally { await fs.rmdir(lock); }
  }
  async read(id) {
    const file = path.join(this.dir, taskId(id) + '.json');
    await safeFile(file);
    try { return JSON.parse(await fs.readFile(file, 'utf8')); }
    catch (e) { if (e.code === 'ENOENT') throw new BridgeError(404, 'Task not found'); throw e; }
  }
  async all() {
    const names = (await fs.readdir(this.dir)).filter(n => /^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}\.json$/.test(n));
    const tasks = await Promise.all(names.map(n => this.read(n.slice(0, -5))));
    return tasks.sort((a, b) => a.created_at.localeCompare(b.created_at) || a.id.localeCompare(b.id));
  }
  async list(status, offset = 0) {
    if (status && !['pending', 'in_progress', 'blocked', 'done'].includes(status)) fail('Invalid status');
    if (!Number.isSafeInteger(offset) || offset < 0) fail('Invalid offset');
    const tasks = (await this.all()).filter(t => !status || t.status === status);
    return { tasks: tasks.slice(offset, offset + 50).map(t => ({ id: t.id, title: t.title, status: t.status,
      revision: t.revision, worker: t.worker, created_at: t.created_at, result_summary: t.result?.summary.slice(0, 300) ?? null })),
      next_offset: offset + 50 < tasks.length ? offset + 50 : null };
  }
  async save(t) {
    // JSON is authoritative; Markdown is a view, never an instruction executor.
    await atomic(path.join(this.dir, t.id + '.json'), JSON.stringify(t, null, 2) + '\n');
    await atomic(path.join(this.dir, t.id + '.md'), markdown(t));
  }
  async submit(input) {
    const data = validateTask(input);
    const digest = createHash('sha256').update(JSON.stringify(data)).digest('hex');
    return this.locked(async () => {
      try {
        const existing = await this.read(data.id);
        if (existing.input_digest !== digest) throw new BridgeError(409, 'Task id already exists with different content');
        // Repair a missing Markdown view after a crash between the two writes.
        await atomic(path.join(this.dir, existing.id + '.md'), markdown(existing));
        return { created: false, task: existing };
      } catch (e) { if (e.status !== 404) throw e; }
      if ((await this.all()).length >= 1000) throw new BridgeError(429, 'Task queue full; archive completed tasks locally');
      const now = new Date().toISOString();
      const t = { ...data, status: 'pending', worker: null, revision: 1, created_at: now,
        updated_at: now, result: null, input_digest: digest };
      await this.save(t);
      return { created: true, task: t };
    });
  }
  async claim(input) {
    object(input, ['id', 'worker']);
    const worker = workerName(input.worker); if (input.id !== undefined) taskId(input.id);
    return this.locked(async () => {
      let t = input.id ? await this.read(input.id) : (await this.all()).find(t => t.status === 'pending');
      if (!t) return { task: null };
      if (t.status === 'in_progress' && t.worker === worker) return { task: t };
      if (!['pending', 'blocked'].includes(t.status)) throw new BridgeError(409, 'Task already claimed or completed');
      t.status = 'in_progress'; t.worker = worker; t.revision++; t.updated_at = new Date().toISOString();
      await this.save(t); return { task: t };
    });
  }
  async report(id, input) {
    taskId(id); object(input, ['worker', 'expected_revision', 'status', 'summary', 'changed_files', 'validation', 'commit']);
    const result = { summary: text(input.summary, 'summary', 5000),
      changed_files: strings(input.changed_files, 'changed files', 100, 240, projectFile),
      validation: strings(input.validation, 'validation', 40, 1000), commit: input.commit ?? null };
    if (Buffer.byteLength(JSON.stringify(result)) > 24576) fail('Result exceeds 24 KiB; summarize validation');
    if (result.commit !== null && !/^[a-f0-9]{40}([a-f0-9]{24})?$/.test(result.commit)) fail('Invalid commit SHA');
    const worker = workerName(input.worker);
    if (!['done', 'blocked'].includes(input.status) || !Number.isSafeInteger(input.expected_revision)) fail('Invalid result status/revision');
    return this.locked(async () => {
      const t = await this.read(id);
      if (t.worker !== worker) throw new BridgeError(409, 'Worker does not own this task');
      if (t.status === input.status && t.revision === input.expected_revision + 1 && JSON.stringify(t.result) === JSON.stringify(result)) return { task: t };
      if (t.status !== 'in_progress' || t.revision !== input.expected_revision) throw new BridgeError(409, 'Stale revision or task not in progress');
      t.status = input.status; t.result = result; t.revision++; t.updated_at = new Date().toISOString();
      await this.save(t); return { task: t };
    });
  }
}
