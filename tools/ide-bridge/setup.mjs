import fs from 'node:fs/promises';
import path from 'node:path';
import { randomBytes } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { actionSchema } from './openapi.mjs';
import { argumentsFor, rootFor, publicURL, readEnvironment } from './config.mjs';

export async function setup(root, url) {
  const directory = path.join(root, '.ide-bridge');
  await fs.mkdir(directory, { recursive: true, mode: 0o700 });
  if ((await fs.lstat(directory)).isSymbolicLink() || path.resolve(await fs.realpath(directory)) !== path.resolve(directory)) throw new Error('Bridge config cannot be a symlink');
  const secretFile = path.join(directory, 'secrets.env');
  try {
    await fs.writeFile(secretFile, `BRIDGE_TASK_TOKEN=${randomBytes(32).toString('hex')}\nBRIDGE_WORKER_TOKEN=${randomBytes(32).toString('hex')}\nBRIDGE_PORT=5000\n`, { flag: 'wx', mode: 0o600 });
  } catch (e) { if (e.code !== 'EEXIST') throw e; }
  if ((await fs.lstat(secretFile)).isSymbolicLink()) throw new Error('Secrets file cannot be a symlink');
  const env = await readEnvironment(root);
  const origin = url ? publicURL(url) : env.BRIDGE_PUBLIC_URL ? publicURL(env.BRIDGE_PUBLIC_URL) : 'https://replace-with-your-tunnel.invalid';
  if (url) {
    const lines = (await fs.readFile(secretFile, 'utf8')).split(/\r?\n/).filter(l => l && !l.startsWith('BRIDGE_PUBLIC_URL='));
    await fs.writeFile(secretFile, lines.join('\n') + `\nBRIDGE_PUBLIC_URL=${origin}\n`, { mode: 0o600 });
  }
  const config = { mcpServers: { 'phonho-task-bridge': { command: process.execPath,
    args: [path.join(root, 'tools', 'ide-bridge', 'mcp.mjs'), '--root', root] } } };
  for (const file of ['mcp_config.json', 'openapi.json']) {
    try { if ((await fs.lstat(path.join(directory, file))).isSymbolicLink()) throw new Error('Generated config cannot be a symlink'); }
    catch (e) { if (e.code !== 'ENOENT') throw e; }
  }
  await fs.writeFile(path.join(directory, 'mcp_config.json'), JSON.stringify(config, null, 2) + '\n');
  await fs.writeFile(path.join(directory, 'openapi.json'), JSON.stringify(actionSchema(origin), null, 2) + '\n');
  return { directory, configuredHTTPS: !origin.endsWith('.invalid') };
}
if (process.argv[1] === fileURLToPath(import.meta.url)) {
  try { const args = argumentsFor(); const result = await setup(await rootFor(args), args.url);
    console.log('Setup ready: .ide-bridge/mcp_config.json and .ide-bridge/openapi.json.');
    console.log(result.configuredHTTPS ? 'HTTPS origin configured; restart the REST server after URL changes.' : 'REST Action still needs your HTTPS tunnel URL: rerun setup with --url HTTPS_URL.');
    console.log('Tokens are stored only in .ide-bridge/secrets.env; no tokens were printed.');
  } catch (e) { console.error(e.message); process.exitCode = 1; }
}
