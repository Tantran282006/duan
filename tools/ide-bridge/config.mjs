import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { projectRoot } from './store.mjs';

export function argumentsFor(argv = process.argv.slice(2)) {
  const result = {};
  for (let i = 0; i < argv.length; i += 2) {
    if (!['--root', '--url', '--file'].includes(argv[i]) || !argv[i + 1]) throw new Error('Use --root PATH, --url HTTPS_URL, or --file JSON_FILE');
    result[argv[i].slice(2)] = argv[i + 1];
  }
  return result;
}
export const defaultRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
export const rootFor = async args => projectRoot(args.root ?? defaultRoot);
export async function readEnvironment(root) {
  let file = '';
  try { file = await fs.readFile(path.join(root, '.ide-bridge', 'secrets.env'), 'utf8'); }
  catch (e) { if (e.code !== 'ENOENT') throw e; }
  const result = {};
  for (const line of file.split(/\r?\n/)) {
    const m = /^([A-Z_]+)=(.*)$/.exec(line); if (m) result[m[1]] = m[2];
  }
  return { ...result, ...process.env };
}
export function publicURL(value) {
  const url = new URL(value);
  if (url.protocol !== 'https:' || url.username || url.password || (url.port && url.port !== '443') || url.search || url.hash || url.pathname !== '/')
    throw new Error('Tunnel URL must be an HTTPS origin on port 443');
  return url.origin;
}
