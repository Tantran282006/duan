import http from 'node:http';
import fsSync from 'node:fs';
import fs from 'node:fs/promises';
import path from 'node:path';
import { createHash, timingSafeEqual } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { TaskStore, BridgeError } from './store.mjs';
import { argumentsFor, rootFor, readEnvironment, publicURL } from './config.mjs';
import { parseProgressMd, loadProfiles, saveProfiles, recordWorkerTurn, handleQuotaErrorEvent } from './dashboard/data.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const DASHBOARD_DIR = path.join(__dirname, 'dashboard', 'public');
const MIME_TYPES = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'application/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png'
};

function log(msg) {
  try { fsSync.writeSync(1, `[${new Date().toISOString()}] ${msg}\n`); } catch {}
}
function logErr(msg) {
  try { fsSync.writeSync(2, `[${new Date().toISOString()}] ERROR: ${msg}\n`); } catch {}
}

process.on('uncaughtException', err => {
  logErr(`Uncaught exception: ${err?.stack || err}`);
  process.exit(1);
});
process.on('unhandledRejection', reason => {
  logErr(`Unhandled rejection: ${reason?.stack || reason}`);
  process.exit(1);
});

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
export function createBridgeServer({ store, taskToken, workerToken, allowedHosts = ['localhost', '127.0.0.1'], root }) {
  if (typeof taskToken !== 'string' || typeof workerToken !== 'string' || taskToken.length < 32 || workerToken.length < 32 || taskToken === workerToken)
    throw new Error('Run setup first; distinct task/worker tokens of at least 32 characters are required');
  const buckets = new Map();
  const server = http.createServer(async (req, res) => {
    const send = (code, data, contentType = 'application/json; charset=utf-8') => {
      if (!res.destroyed) {
        res.writeHead(code, {
          'Content-Type': contentType,
          'Cache-Control': 'no-store',
          'X-Content-Type-Options': 'nosniff'
        });
        if (Buffer.isBuffer(data) || typeof data === 'string') {
          res.end(data);
        } else {
          res.end(JSON.stringify(data));
        }
      }
    };
    try {
      const url = new URL(req.url, 'http://localhost');

      // Health check endpoint (for Custom GPT and monitoring)
      if (req.method === 'GET' && url.pathname === '/health' && process.env.PUBLIC_HEALTH === 'true') {
        return send(200, { ok: true, service: 'Pho Nho IDE Task Bridge', version: '0.1.0' });
      }

      // Static Dashboard Web UI Serving
      if (req.method === 'GET' && (url.pathname === '/' || url.pathname === '/index.html' || url.pathname === '/styles.css' || url.pathname === '/app.js')) {
        const safeFile = url.pathname === '/' ? '/index.html' : url.pathname;
        const targetPath = path.resolve(DASHBOARD_DIR, '.' + safeFile);
        if (targetPath.startsWith(DASHBOARD_DIR)) {
          try {
            const content = await fs.readFile(targetPath);
            const ext = path.extname(targetPath).toLowerCase();
            return send(200, content, MIME_TYPES[ext] || 'application/octet-stream');
          } catch {}
        }
      }

      // Dashboard APIs
      if (req.method === 'GET' && url.pathname === '/api/overview') {
        const progress = await parseProgressMd(root || process.cwd());
        const taskList = await store.list();
        const tasks = taskList.tasks || [];
        const metrics = {
          total: tasks.length,
          inProgress: tasks.filter(t => t.status === 'in_progress').length,
          pending: tasks.filter(t => t.status === 'pending').length,
          blocked: tasks.filter(t => t.status === 'blocked').length,
          done: tasks.filter(t => t.status === 'done').length
        };
        const completionPercent = Math.min(100, Math.round(20 + (progress.sessions.length * 6) + (metrics.done * 10)));
        return send(200, {
          currentMilestone: progress.currentMilestone,
          completionPercent,
          metrics,
          nextItems: progress.nextItems,
          whatWorks: progress.whatWorks,
          knownDebt: progress.knownDebt,
          recentSessions: progress.sessions.slice(0, 5),
          bridgeUrl: 'https://game.zcloudviet.xyz',
          sources: ['PROGRESS.md', 'Cloud Bridge TaskStore'],
          lastUpdated: new Date().toISOString()
        });
      }

      if (req.method === 'GET' && url.pathname === '/api/tasks') {
        const taskList = await store.list();
        const fullTasks = [];
        for (const t of (taskList.tasks || [])) {
          try { fullTasks.push(await store.read(t.id)); } catch { fullTasks.push(t); }
        }
        return send(200, { tasks: fullTasks });
      }

      if (req.method === 'GET' && url.pathname === '/api/done-log') {
        const progress = await parseProgressMd(root || process.cwd());
        const taskList = await store.list('done');
        return send(200, { sessions: progress.sessions, doneTasks: taskList.tasks || [] });
      }

      if (req.method === 'GET' && url.pathname === '/api/profiles') {
        const profileData = await loadProfiles(root || process.cwd());
        return send(200, profileData);
      }

      const quotaPathMatch = /^\/api\/profiles\/([a-zA-Z0-9_-]+)\/quota$/.exec(url.pathname);
      if (req.method === 'POST' && quotaPathMatch) {
        const profileId = quotaPathMatch[1];
        const bodyData = await body(req);
        const profileData = await loadProfiles(root || process.cwd());
        const profile = profileData.profiles.find(p => p.id === profileId);
        if (!profile) return send(404, { error: 'Không tìm thấy profile' });

        let newPercent = profile.quotaPercent;
        if (bodyData.percent !== undefined) {
          newPercent = Number(bodyData.percent);
        } else if (bodyData.delta !== undefined) {
          const current = (profile.quotaPercent === null || isNaN(profile.quotaPercent)) ? 100 : profile.quotaPercent;
          newPercent = current + Number(bodyData.delta);
        }

        if (isNaN(newPercent)) {
          return send(400, { error: 'Giá trị quota % không hợp lệ' });
        }

        newPercent = Math.max(0, Math.min(100, Math.round(newPercent)));
        profile.quotaPercent = newPercent;
        profile.quotaSource = 'nhập tay';
        profile.lastQuotaUpdate = new Date().toISOString();
        profile.isStale = false;

        if (newPercent > 0 && profile.status === 'hết token') {
          profile.status = (profile.id === profileData.activeProfileId) ? 'active' : 'ready';
        } else if (newPercent === 0) {
          profile.status = 'hết token';
        }

        await saveProfiles(root || process.cwd(), profileData);
        return send(200, { ok: true, profile });
      }

      if (req.method === 'POST' && url.pathname === '/api/profiles/dismiss-suggestion') {
        const profileData = await loadProfiles(root || process.cwd());
        profileData.suggestedSwitch = null;
        await saveProfiles(root || process.cwd(), profileData);
        return send(200, { ok: true });
      }

      if (req.method === 'POST' && url.pathname === '/api/profiles/switch') {
        const bodyData = await body(req);
        const profileData = await loadProfiles(root || process.cwd());
        const target = profileData.profiles.find(p => p.id === bodyData.targetId);
        if (!target) return send(404, { error: 'Profile not found' });
        profileData.switchHistory.unshift({
          timestamp: new Date().toISOString(),
          fromId: profileData.activeProfileId,
          toId: target.id,
          reason: bodyData.reason || 'Chuyển đổi từ Dashboard'
        });
        profileData.activeProfileId = target.id;
        profileData.suggestedSwitch = null;
        await saveProfiles(root || process.cwd(), profileData);
        return send(200, { ok: true, activeProfileId: target.id });
      }

      // REST Security Checks
      if (req.headers.origin && (url.pathname.startsWith('/v1/') || url.pathname === '/health')) {
        throw new BridgeError(403, 'Browser origins are not supported');
      }
      const host = new URL('http://' + (req.headers.host ?? 'localhost')).hostname;
      const isPrivateIp = /^(?:127\.|10\.|172\.(?:1[6-9]|2[0-9]|3[01])\.|192\.168\.)/.test(host);
      if (!allowedHosts.includes('*') && !allowedHosts.includes(host) && !host.endsWith('.zcloudviet.xyz') && !isPrivateIp) {
        throw new BridgeError(403, 'Unrecognized host');
      }

      const isTask = authenticated(req.headers.authorization, taskToken);
      const isWorker = authenticated(req.headers.authorization, workerToken);
      if (!isTask && !isWorker) throw new BridgeError(401, 'Unauthorized');

      const role = isTask ? 'task' : 'worker', now = Date.now();
      let bucket = buckets.get(role);
      if (!bucket || now - bucket.start > 60000) { bucket = { start: now, count: 0 }; buckets.set(role, bucket); }
      if (++bucket.count > 120) throw new BridgeError(429, 'Rate limit exceeded');

      if (req.method === 'GET' && url.pathname === '/health') {
        return send(200, { ok: true, version: '0.1.0' });
      }
      if (req.method === 'GET' && url.pathname === '/v1/tasks') {
        return send(200, await store.list(url.searchParams.get('status') || undefined, Number(url.searchParams.get('offset') ?? 0)));
      }
      if (req.method === 'POST' && url.pathname === '/v1/tasks') {
        if (!isTask) throw new BridgeError(403, 'Task token required');
        const result = await store.submit(await body(req));
        return send(result.created ? 201 : 200, result);
      }
      if (req.method === 'POST' && url.pathname === '/v1/claim') {
        if (!isWorker) throw new BridgeError(403, 'Worker token required');
        const claimBody = await body(req);
        const claimRes = await store.claim(claimBody);
        if (claimRes.task) {
          try { await recordWorkerTurn(root || process.cwd(), claimBody.worker, 'claim', claimRes.task.id); } catch {}
        }
        return send(200, claimRes);
      }
      const match = /^\/v1\/tasks\/([a-zA-Z0-9][a-zA-Z0-9_-]{0,79})(\/result)?$/.exec(url.pathname);
      if (match && !match[2] && req.method === 'GET') {
        return send(200, { task: await store.read(match[1]) });
      }
      if (match?.[2] && req.method === 'POST') {
        if (!isWorker) throw new BridgeError(403, 'Worker token required');
        const reportBody = await body(req);
        const reportRes = await store.report(match[1], reportBody);
        try {
          await recordWorkerTurn(root || process.cwd(), reportBody.worker, 'report', match[1]);
          const textToCheck = `${reportBody.status} ${reportBody.summary || ''} ${(reportBody.validation || []).join(' ')}`.toLowerCase();
          if (textToCheck.includes('quota') || textToCheck.includes('429') || textToCheck.includes('resourceexhausted') || textToCheck.includes('hết token')) {
            await handleQuotaErrorEvent(root || process.cwd(), reportBody.worker, reportBody.summary || 'Lỗi hạn ngạch Quota/ResourceExhausted');
          }
        } catch {}
        return send(200, reportRes);
      }
      throw new BridgeError(404, 'Endpoint not found');
    } catch (e) {
      send(e instanceof BridgeError ? e.status : 500, { error: e instanceof BridgeError ? e.message : 'Internal bridge error' });
    }
  });
  server.requestTimeout = 10000;
  server.headersTimeout = 10000;
  server.maxHeadersCount = 32;
  return server;
}

let isStarting = false;
export async function start() {
  if (isStarting) return;
  isStarting = true;
  log('Pho Nho IDE Task Bridge starting...');
  try {
    const root = await rootFor(argumentsFor());
    log(`Using project root: ${root}`);
    const env = await readEnvironment(root);

    const taskToken = env.BRIDGE_TASK_TOKEN || process.env.BRIDGE_TASK_TOKEN || '13319f0bb241c64b5d0694aa4e52b0958838f042b8a4645d4cb5de127d16e76d';
    const workerToken = env.BRIDGE_WORKER_TOKEN || process.env.BRIDGE_WORKER_TOKEN || '57db009d86e37f0abd0c3f03757852ed6289e18228ece147594516a4001d5636';

    const allowedHosts = ['*'];
    const port = Number(process.env.PORT ?? env.BRIDGE_PORT ?? 3000);
    const bindHost = '0.0.0.0';

    const store = await new TaskStore(root).init();
    const server = createBridgeServer({ store, taskToken, workerToken, allowedHosts, root });
    server.on('error', e => {
      logErr(`Bridge server failed: ${e.code ?? e.message}`);
      process.exit(1);
    });
    server.listen(port, bindHost, () => {
      log(`IDE Bridge ready on ${bindHost}:${port}; public URL: ${env.BRIDGE_PUBLIC_URL ?? 'https://game.zcloudviet.xyz'}`);
    });
    for (const signal of ['SIGINT', 'SIGTERM']) {
      process.on(signal, () => server.close(() => process.exit(0)));
    }
    return server;
  } catch (e) {
    logErr(`Bridge startup error: ${e?.stack || e}`);
    process.exit(1);
  }
}

const isMain = !process.argv[1] || (
  process.argv[1] === fileURLToPath(import.meta.url) ||
  process.argv[1].endsWith('server.mjs') ||
  process.argv[1].endsWith('server.js') ||
  process.argv[1].endsWith('index.js')
);
if (isMain || process.env.NODE_ENV === 'production' || process.env.PORT) {
  start();
}

