import fs from 'node:fs/promises';
import { argumentsFor, rootFor } from './config.mjs';
import { TaskStore } from './store.mjs';

try {
  const args = argumentsFor(); if (!args.file) throw new Error('Use --file TASK_JSON');
  const raw = await fs.readFile(args.file); if (raw.length > 65536) throw new Error('Task JSON exceeds 64 KiB');
  const store = await new TaskStore(await rootFor(args)).init();
  const result = await store.submit(JSON.parse(raw.toString('utf8')));
  console.log(JSON.stringify({ id: result.task.id, status: result.task.status, created: result.created }));
} catch (e) { console.error(e.message); process.exitCode = 1; }
