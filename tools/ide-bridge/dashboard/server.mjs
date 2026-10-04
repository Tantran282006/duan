import http from 'node:http';
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { readEnvironment, rootFor, argumentsFor } from '../config.mjs';
import { parseProgressMd, readRuntimeTasks, fetchCloudTasks, loadProfiles, saveProfiles, hashPin, maskEmail } from './data.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const PUBLIC_DIR = path.join(__dirname, 'public');

const MIME_TYPES = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'application/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png'
};

async function readBody(req) {
  let data = '';
  for await (const chunk of req) {
    data += chunk;
    if (data.length > 65536) throw new Error('Body too large');
  }
  return data ? JSON.parse(data) : {};
}

export function createDashboardServer(root, env) {
  const activeSessions = new Set(['local-dev-token']);

  const server = http.createServer(async (req, res) => {
    const send = (status, data, contentType = 'application/json; charset=utf-8') => {
      res.writeHead(status, {
        'Content-Type': contentType,
        'Cache-Control': 'no-store',
        'X-Content-Type-Options': 'nosniff'
      });
      if (Buffer.isBuffer(data) || typeof data === 'string') {
        res.end(data);
      } else {
        res.end(JSON.stringify(data));
      }
    };

    try {
      const url = new URL(req.url, 'http://127.0.0.1');

      // Security: Only allow 127.0.0.1 / localhost requests
      const host = (req.headers.host || '').split(':')[0];
      if (host !== '127.0.0.1' && host !== 'localhost') {
        return send(403, { error: 'Forbidden: Dashboard binds only to 127.0.0.1' });
      }

      // API Routes
      if (url.pathname.startsWith('/api/')) {
        if (url.pathname === '/api/health') {
          return send(200, { ok: true, service: 'Pho Nho Dashboard', version: '1.0.0' });
        }

        if (url.pathname === '/api/auth/verify' && req.method === 'POST') {
          const body = await readBody(req);
          const profileData = await loadProfiles(root);
          if (hashPin(body.pin || '') === profileData.pinHash) {
            const token = 'session-' + Date.now();
            activeSessions.add(token);
            return send(200, { ok: true, token });
          }
          return send(401, { error: 'Mã PIN không đúng (Mặc định: 1234)' });
        }

        if (url.pathname === '/api/overview') {
          const progress = await parseProgressMd(root);
          const localTasks = await readRuntimeTasks(root);
          const cloudTasks = await fetchCloudTasks(env);

          const taskMap = new Map();
          for (const t of localTasks) taskMap.set(t.id, t);
          for (const t of cloudTasks) {
            if (!taskMap.has(t.id) || (t.revision && t.revision >= (taskMap.get(t.id).revision || 0))) {
              taskMap.set(t.id, { ...taskMap.get(t.id), ...t });
            }
          }
          const allTasks = Array.from(taskMap.values());

          const metrics = {
            total: allTasks.length,
            inProgress: allTasks.filter(t => t.status === 'in_progress').length,
            pending: allTasks.filter(t => t.status === 'pending').length,
            blocked: allTasks.filter(t => t.status === 'blocked').length,
            done: allTasks.filter(t => t.status === 'done').length
          };

          // Completion % estimate: base milestones + tasks
          const sessionCount = progress.sessions.length;
          const completionPercent = Math.min(100, Math.round(20 + (sessionCount * 6) + (metrics.done * 10)));

          return send(200, {
            currentMilestone: progress.currentMilestone,
            completionPercent,
            metrics,
            nextItems: progress.nextItems,
            whatWorks: progress.whatWorks,
            knownDebt: progress.knownDebt,
            recentSessions: progress.sessions.slice(0, 5),
            bridgeUrl: env.BRIDGE_PUBLIC_URL || 'https://game.zcloudviet.xyz',
            sources: ['PROGRESS.md', 'Local Runtime .tasks', 'Cloud Bridge'],
            lastUpdated: new Date().toISOString()
          });
        }

        if (url.pathname === '/api/tasks') {
          const localTasks = await readRuntimeTasks(root);
          const cloudTasks = await fetchCloudTasks(env);

          const taskMap = new Map();
          for (const t of localTasks) taskMap.set(t.id, t);
          for (const t of cloudTasks) {
            if (!taskMap.has(t.id) || (t.revision && t.revision >= (taskMap.get(t.id).revision || 0))) {
              taskMap.set(t.id, { ...taskMap.get(t.id), ...t });
            }
          }

          return send(200, { tasks: Array.from(taskMap.values()) });
        }

        if (url.pathname === '/api/done-log') {
          const progress = await parseProgressMd(root);
          const localTasks = await readRuntimeTasks(root);
          const doneTasks = localTasks.filter(t => t.status === 'done' || t.result);

          return send(200, {
            sessions: progress.sessions,
            doneTasks
          });
        }

        if (url.pathname === '/api/profiles') {
          const profileData = await loadProfiles(root);
          // Return masked profile data with transparent quota sources and turns count
          const sanitized = {
            activeProfileId: profileData.activeProfileId,
            settings: profileData.settings,
            profiles: profileData.profiles,
            switchHistory: profileData.switchHistory,
            suggestedSwitch: profileData.suggestedSwitch || null
          };
          return send(200, sanitized);
        }

        // Endpoint: Nhập tay % hoặc tăng/giảm (+/-) quota cho từng profile
        const quotaMatch = /^\/api\/profiles\/([a-zA-Z0-9_-]+)\/quota$/.exec(url.pathname);
        if (quotaMatch && req.method === 'POST') {
          const profileId = quotaMatch[1];
          const body = await readBody(req);
          const profileData = await loadProfiles(root);
          const profile = profileData.profiles.find(p => p.id === profileId);
          if (!profile) return send(404, { error: 'Không tìm thấy profile' });

          let newPercent = profile.quotaPercent;
          if (body.percent !== undefined) {
            newPercent = Number(body.percent);
          } else if (body.delta !== undefined) {
            const current = (profile.quotaPercent === null || isNaN(profile.quotaPercent)) ? 100 : profile.quotaPercent;
            newPercent = current + Number(body.delta);
          }

          if (isNaN(newPercent)) {
            return send(400, { error: 'Giá trị quota % không hợp lệ' });
          }

          newPercent = Math.max(0, Math.min(100, Math.round(newPercent)));
          profile.quotaPercent = newPercent;
          profile.quotaSource = 'nhập tay';
          profile.lastQuotaUpdate = new Date().toISOString();
          profile.isStale = false;

          // Nếu quota > 0 mà status đang là "hết token", khôi phục trạng thái
          if (newPercent > 0 && profile.status === 'hết token') {
            profile.status = (profile.id === profileData.activeProfileId) ? 'active' : 'ready';
          } else if (newPercent === 0) {
            profile.status = 'hết token';
          }

          await saveProfiles(root, profileData);
          return send(200, { ok: true, profile });
        }

        if (url.pathname === '/api/profiles/dismiss-suggestion' && req.method === 'POST') {
          const profileData = await loadProfiles(root);
          profileData.suggestedSwitch = null;
          await saveProfiles(root, profileData);
          return send(200, { ok: true });
        }

        if (url.pathname === '/api/profiles/switch' && req.method === 'POST') {
          const body = await readBody(req);
          const profileData = await loadProfiles(root);
          const target = profileData.profiles.find(p => p.id === body.targetId);
          if (!target) return send(404, { error: 'Không tìm thấy profile' });

          const localTasks = await readRuntimeTasks(root);
          const inProgress = localTasks.filter(t => t.status === 'in_progress');

          // Log switch history
          profileData.switchHistory.unshift({
            timestamp: new Date().toISOString(),
            fromId: profileData.activeProfileId,
            toId: target.id,
            reason: body.reason || 'Chuyển đổi thủ công từ dashboard'
          });

          profileData.activeProfileId = target.id;
          target.status = 'active';
          target.lastUsedAt = new Date().toISOString();

          // Update others to ready
          for (const p of profileData.profiles) {
            if (p.id !== target.id && p.status === 'active') {
              p.status = 'ready';
            }
          }

          await saveProfiles(root, profileData);

          return send(200, {
            ok: true,
            activeProfileId: target.id,
            inProgressWarning: inProgress.length > 0 ? {
              count: inProgress.length,
              tasks: inProgress.map(t => ({ id: t.id, title: t.title }))
            } : null
          });
        }

        if (url.pathname === '/api/profiles' && req.method === 'POST') {
          const body = await readBody(req);
          if (!body.name) return send(400, { error: 'Tên profile là bắt buộc' });
          const profileData = await loadProfiles(root);
          const newProfile = {
            id: 'profile-' + Date.now(),
            name: body.name.trim(),
            emailMasked: maskEmail(body.email || ''),
            status: 'ready',
            quotaRemaining: Number(body.quotaRemaining ?? 100),
            quotaTotal: 100,
            quotaUnit: '%',
            quotaSource: body.quotaSource || 'Nhập tay/ước tính',
            expectedReset: body.expectedReset || 'Hằng ngày',
            lastUsedAt: null,
            priority: profileData.profiles.length + 1
          };
          profileData.profiles.push(newProfile);
          await saveProfiles(root, profileData);
          return send(201, { ok: true, profile: newProfile });
        }

        if (url.pathname === '/api/profiles/settings' && req.method === 'POST') {
          const body = await readBody(req);
          const profileData = await loadProfiles(root);
          if (body.quotaThresholdPercent != null) {
            profileData.settings.quotaThresholdPercent = Number(body.quotaThresholdPercent);
          }
          if (body.autoSwitch != null) {
            profileData.settings.autoSwitch = Boolean(body.autoSwitch);
          }
          await saveProfiles(root, profileData);
          return send(200, { ok: true, settings: profileData.settings });
        }

        return send(404, { error: 'Endpoint API không tồn tại' });
      }

      // Static File Serving
      let safePath = url.pathname === '/' ? '/index.html' : url.pathname;
      const targetFile = path.resolve(PUBLIC_DIR, '.' + safePath);

      // Path traversal security check
      if (!targetFile.startsWith(PUBLIC_DIR)) {
        return send(403, 'Forbidden', 'text/plain');
      }

      try {
        const stat = await fs.stat(targetFile);
        if (stat.isDirectory()) {
          const indexFile = path.join(targetFile, 'index.html');
          const content = await fs.readFile(indexFile);
          return send(200, content, 'text/html; charset=utf-8');
        }
        const ext = path.extname(targetFile).toLowerCase();
        const mime = MIME_TYPES[ext] || 'application/octet-stream';
        const content = await fs.readFile(targetFile);
        return send(200, content, mime);
      } catch {
        return send(404, 'Not Found', 'text/plain');
      }

    } catch (e) {
      send(500, { error: e.message });
    }
  });

  return server;
}

export async function startDashboard(port = 5050) {
  const root = await rootFor(argumentsFor());
  const env = await readEnvironment(root);
  const server = createDashboardServer(root, env);
  server.listen(port, '127.0.0.1', () => {
    console.log(`[Pho Nho Dashboard] Đang chạy tại http://127.0.0.1:${port}`);
    console.log(`[Bảo mật] Chỉ bind trên 127.0.0.1, mã PIN mặc định: 1234`);
  });
  return server;
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const port = Number(process.env.DASHBOARD_PORT || 5050);
  startDashboard(port).catch(err => {
    console.error('Không thể khởi động Dashboard:', err);
    process.exit(1);
  });
}
