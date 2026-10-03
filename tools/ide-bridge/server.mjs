import http from 'node:http';
import { createHash, timingSafeEqual } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { TaskStore, BridgeError } from './store.mjs';
import { argumentsFor, rootFor, readEnvironment, publicURL } from './config.mjs';

const LIMIT = 65536;
const hash = s => createHash('sha256').update(s).digest();
function authenticated(value, token) {
  return typeof value === 'string' && value.startsWith('Bearer ') && timingSafeEqual(hash(value.slice(7)), hash(token));
}
async function body(req) {
  if (!/^application\/json(?:;|$)/i.test(req.headers['content-type'] ?? '')) throw new BridgeError(415, 'Use application/json');
  if (Number(req.headers['content-length']) > LIMIT) { req.resume(); throw new BridgeError(413, 'Request too large'); }
  return new Promise((resolve, reject) => {
    let size = 0, chunks = [], failed = false;
    req.on('data', chunk => {
      if (failed) return;
      size += chunk.length;
      if (size > LIMIT) { failed = true; chunks = []; reject(new BridgeError(413, 'Request too large')); return; }
      chunks.push(chunk);
    });
    req.on('end', () => {
      if (failed) return;
      try { resolve(JSON.parse(Buffer.concat(chunks).toString('utf8'))); }
      catch { reject(new BridgeError(400, 'Invalid JSON')); }
    });
    req.on('error', () => reject(new BridgeError(400, 'Request interrupted')));
    req.on('aborted', () => reject(new BridgeError(400, 'Request interrupted')));
  });
}
export function createBridgeServer({ store, taskToken, workerToken, allowedHosts = ['localhost', '127.0.0.1'] }) {
  if (typeof taskToken !== 'string' || typeof workerToken !== 'string' || taskToken.length < 32 || workerToken.length < 32 || taskToken === workerToken)
    throw new Error('Run setup first; distinct task/worker tokens of at least 32 characters are required');
  const buckets = new Map();
  const server = http.createServer(async (req, res) => {
    const send = (code, data) => { if (!res.destroyed) { res.writeHead(code, { 'Content-Type': 'application/json; charset=utf-8', 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff' }); res.end(JSON.stringify(data)); } };
    try {
      // Server-to-server endpoint: browser cross-origin requests are unnecessary.
      if (req.headers.origin) throw new BridgeError(403, 'Browser origins are not supported');
      const host = new URL('http://' + (req.headers.host ?? '')).hostname;
      if (!allowedHosts.includes(host)) throw new BridgeError(403, 'Unrecognized host');
      const isTask = authenticated(req.headers.authorization, taskToken), isWorker = authenticated(req.headers.authorization, workerToken);
      if (!isTask && !isWorker) throw new BridgeError(401, 'Unauthorized');
      const role = isTask ? 'task' : 'worker', now = Date.now();
      let bucket = buckets.get(role);
      if (!bucket || now - bucket.start > 60000) { bucket = { start: now, count: 0 }; buckets.set(role, bucket); }
      if (++bucket.count > 120) throw new BridgeError(429, 'Rate limit exceeded');
      const url = new URL(req.url, 'http://localhost');
      if (req.method === 'GET' && url.pathname === '/health') return send(200, { ok: true, version: '0.1.0' });
      if (req.method === 'GET' && url.pathname === '/v1/tasks') return send(200, await store.list(url.searchParams.get('status') || undefined, Number(url.searchParams.get('offset') ?? 0)));
      if (req.method === 'POST' && url.pathname === '/v1/tasks') {
        if (!isTask) throw new BridgeError(403, 'Task token required');
        const result = await store.submit(await body(req)); return send(result.created ? 201 : 200, result);
      }
      if (req.method === 'POST' && url.pathname === '/v1/claim') {
        if (!isWorker) throw new BridgeError(403, 'Worker token required');
        return send(200, await store.claim(await body(req)));
      }
      const match = /^\/v1\/tasks\/([a-zA-Z0-9][a-zA-Z0-9_-]{0,79})(\/result)?$/.exec(url.pathname);
      if (match && !match[2] && req.method === 'GET') return send(200, { task: await store.read(match[1]) });
      if (match?.[2] && req.method === 'POST') {
        if (!isWorker) throw new BridgeError(403, 'Worker token required');
        return send(200, await store.report(match[1], await body(req)));
      }
      throw new BridgeError(404, 'Endpoint not found');
    } catch (e) { send(e instanceof BridgeError ? e.status : 500, { error: e instanceof BridgeError ? e.message : 'Internal bridge error' }); }
  });
  server.requestTimeout = 10000; server.headersTimeout = 10000; server.maxHeadersCount = 32;
  return server;
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  try {
    const root = await rootFor(argumentsFor()), env = await readEnvironment(root);
    const allowedHosts = ['localhost', '127.0.0.1'];
    if (env.BRIDGE_PUBLIC_URL) allowedHosts.push(new URL(publicURL(env.BRIDGE_PUBLIC_URL)).hostname);
    const port = Number(env.BRIDGE_PORT ?? 5000);
    if (!Number.isInteger(port) || port < 1 || port > 65535) throw new Error('Invalid BRIDGE_PORT');
    const store = await new TaskStore(root).init();
    const server = createBridgeServer({ store, taskToken: env.BRIDGE_TASK_TOKEN, workerToken: env.BRIDGE_WORKER_TOKEN, allowedHosts });
    server.on('error', e => { console.error('Bridge server failed:', e.code ?? e.message); process.exitCode = 1; });
    server.listen(port, '127.0.0.1', () => console.log(`IDE Bridge ready on 127.0.0.1:${port}; use an HTTPS tunnel for Custom GPT Actions.`));
    for (const signal of ['SIGINT', 'SIGTERM']) process.on(signal, () => server.close(() => process.exit()));
  } catch (e) { console.error(e.message); process.exitCode = 1; }
}
