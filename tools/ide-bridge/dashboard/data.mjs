import fs from 'node:fs/promises';
import path from 'node:path';
import { createHash, randomBytes } from 'node:crypto';

export function hashPin(pin, salt = 'phonho-local-salt') {
  return createHash('sha256').update(pin + salt).digest('hex');
}

export function maskEmail(email) {
  if (!email || typeof email !== 'string') return '***@***.com';
  const parts = email.split('@');
  if (parts.length !== 2) return '***@***';
  const name = parts[0];
  const domain = parts[1];
  const maskedName = name.length <= 2 ? name[0] + '***' : name[0] + '***' + name[name.length - 1];
  return `${maskedName}@${domain}`;
}

export async function parseProgressMd(root) {
  const filePath = path.join(root, 'PROGRESS.md');
  try {
    const raw = await fs.readFile(filePath, 'utf8');
    const lines = raw.split(/\r?\n/);

    let currentMilestone = 'M0';
    let currentSection = '';
    const nextItems = [];
    const sessions = [];
    let currentSession = null;
    let whatWorks = [];
    let knownDebt = [];

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i];
      if (line.startsWith('## TRẠNG THÁI HIỆN TẠI')) {
        currentSection = 'status';
        continue;
      } else if (line.startsWith('## VIỆC TIẾP THEO')) {
        currentSection = 'next';
        continue;
      } else if (line.startsWith('## NHẬT KÝ PHIÊN')) {
        currentSection = 'sessions';
        continue;
      } else if (line.startsWith('## DECISIONS')) {
        currentSection = 'decisions';
        continue;
      }

      if (currentSection === 'status') {
        const mMatch = line.match(/\*\*Mốc hiện tại\*\*:\s*([^\n\r]+)/);
        if (mMatch) currentMilestone = mMatch[1].trim();
        if (line.includes('**Chạy được**:')) {
          whatWorks.push(line.replace(/.*?\*\*Chạy được\*\*:\s*/, '').trim());
        }
        if (line.includes('**Nợ kỹ thuật') || line.includes('**Lỗi đã biết**:')) {
          knownDebt.push(line.replace(/.*?\*\*(?:Nợ kỹ thuật \/ lỗi đã biết|Nợ kỹ thuật)\*\*:\s*/, '').trim());
        }
      } else if (currentSection === 'next') {
        const nMatch = line.match(/^\d+\.\s+(.+)/);
        if (nMatch) nextItems.push(nMatch[1].trim());
      } else if (currentSection === 'sessions') {
        const sMatch = line.match(/^###\s+Phiên\s+(\d+)\s+—\s+([^\(]+)\s+\((.+)\)/);
        if (sMatch) {
          if (currentSession) sessions.push(currentSession);
          currentSession = {
            sessionNumber: Number(sMatch[1]),
            date: sMatch[2].trim(),
            title: sMatch[3].trim(),
            milestone: '',
            doneItems: [],
            files: [],
            verification: ''
          };
          continue;
        }
        if (currentSession) {
          if (line.includes('- **Mốc**:')) {
            currentSession.milestone = line.replace(/.*?-\s*\*\*Mốc\*\*:\s*/, '').trim();
          } else if (line.includes('- **File chính**:')) {
            currentSession.files = line.replace(/.*?-\s*\*\*File chính\*\*:\s*/, '').split(',').map(s => s.trim().replace(/`/g, ''));
          } else if (line.includes('- **Kiểm tra**:')) {
            currentSession.verification = line.replace(/.*?-\s*\*\*Kiểm tra\*\*:\s*/, '').trim();
          } else if (line.trim().startsWith('- ') && !line.includes('**')) {
            currentSession.doneItems.push(line.replace(/^-\s*/, '').trim());
          }
        }
      }
    }
    if (currentSession) sessions.push(currentSession);

    return {
      currentMilestone,
      whatWorks,
      knownDebt,
      nextItems,
      sessions,
      lastUpdated: new Date().toISOString()
    };
  } catch (e) {
    return {
      currentMilestone: 'Unknown',
      whatWorks: [],
      knownDebt: [],
      nextItems: [],
      sessions: [],
      error: e.message
    };
  }
}

export async function readRuntimeTasks(root) {
  const runtimeDir = path.join(root, '.tasks', 'runtime');
  try {
    const files = await fs.readdir(runtimeDir);
    const tasks = [];
    for (const file of files) {
      if (!file.endsWith('.json') || file.startsWith('.')) continue;
      try {
        const content = await fs.readFile(path.join(runtimeDir, file), 'utf8');
        const task = JSON.parse(content);
        if (task && task.id) tasks.push(task);
      } catch {}
    }
    return tasks;
  } catch {
    return [];
  }
}

export async function fetchCloudTasks(env) {
  const url = env.BRIDGE_PUBLIC_URL || 'https://game.zcloudviet.xyz';
  const token = env.BRIDGE_TASK_TOKEN;
  if (!token) return [];
  try {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 4000);
    const res = await fetch(`${url}/v1/tasks`, {
      headers: { Authorization: `Bearer ${token}` },
      signal: controller.signal
    });
    clearTimeout(timeout);
    if (!res.ok) return [];
    const data = await res.json();
    return data.tasks || [];
  } catch {
    return [];
  }
}

export function parseProfileIdFromWorker(worker, profiles = []) {
  if (!worker || typeof worker !== 'string') return null;
  // Match explicitly against known profile IDs
  for (const p of profiles) {
    if (worker.includes(p.id)) return p.id;
  }
  // Try regex match for pattern like antigravity-worker-<profileId>-<session>
  const match = worker.match(/worker-([a-zA-Z0-9_-]+?)-(?:session|\d|$)/);
  if (match && match[1]) {
    const found = profiles.find(p => p.id === match[1] || p.id.includes(match[1]));
    if (found) return found.id;
  }
  return null;
}

export function countTurns5h(turnHistory = []) {
  if (!Array.isArray(turnHistory)) return 0;
  const cutoff = Date.now() - 5 * 3600 * 1000;
  return turnHistory.filter(item => {
    const t = new Date(item.timestamp || item).getTime();
    return !isNaN(t) && t >= cutoff;
  }).length;
}

export function isQuotaStale(lastUpdatedAt) {
  if (!lastUpdatedAt) return true;
  const t = new Date(lastUpdatedAt).getTime();
  if (isNaN(t)) return true;
  return (Date.now() - t) > (30 * 60 * 1000);
}

export async function loadProfiles(root) {
  const filePath = path.join(root, '.ide-bridge', 'profiles.json');
  try {
    const raw = await fs.readFile(filePath, 'utf8');
    const data = JSON.parse(raw);
    
    // Ensure all profiles conform to the updated schema
    const now = new Date().toISOString();
    for (const p of (data.profiles || [])) {
      if (p.quotaPercent === undefined && p.quotaRemaining !== undefined) {
        p.quotaPercent = p.quotaRemaining;
      }
      if (!p.quotaSource || p.quotaSource === 'Đọc từ IDE') {
        p.quotaSource = 'chưa rõ';
      }
      if (!p.lastQuotaUpdate) {
        p.lastQuotaUpdate = p.lastUsedAt || now;
      }
      if (!Array.isArray(p.turnHistory)) {
        p.turnHistory = [];
      }
      p.turns5h = countTurns5h(p.turnHistory);
      p.isStale = isQuotaStale(p.lastQuotaUpdate);
    }
    return data;
  } catch {
    const defaultData = {
      pinHash: hashPin('1234'),
      settings: {
        quotaThresholdPercent: 10,
        autoSwitch: false
      },
      activeProfileId: 'profile-main',
      profiles: [
        {
          id: 'profile-main',
          name: 'Tài khoản chính (Tan FL)',
          emailMasked: 'tan***@gmail.com',
          status: 'active',
          quotaPercent: null, // Chưa rõ nguồn, không bịa số
          quotaTotal: 100,
          quotaUnit: '%',
          quotaSource: 'chưa rõ',
          lastQuotaUpdate: null,
          turns5h: 0,
          turnHistory: [],
          isStale: true,
          expectedReset: 'Hằng ngày',
          lastUsedAt: new Date().toISOString(),
          priority: 1
        },
        {
          id: 'profile-backup-1',
          name: 'Tài khoản dự phòng 1',
          emailMasked: 'pho***@zcloudviet.xyz',
          status: 'ready',
          quotaPercent: null,
          quotaTotal: 100,
          quotaUnit: '%',
          quotaSource: 'chưa rõ',
          lastQuotaUpdate: null,
          turns5h: 0,
          turnHistory: [],
          isStale: true,
          expectedReset: 'Đầy đủ',
          lastUsedAt: null,
          priority: 2
        }
      ],
      switchHistory: [
        {
          timestamp: new Date().toISOString(),
          fromId: 'profile-backup-1',
          toId: 'profile-main',
          reason: 'Khởi tạo profiles với 3 nguồn quota minh bạch'
        }
      ]
    };
    await fs.mkdir(path.dirname(filePath), { recursive: true }).catch(() => {});
    await fs.writeFile(filePath, JSON.stringify(defaultData, null, 2), { mode: 0o600 });
    return defaultData;
  }
}

export async function saveProfiles(root, data) {
  const filePath = path.join(root, '.ide-bridge', 'profiles.json');
  // Refresh turns5h and isStale before saving/returning
  for (const p of (data.profiles || [])) {
    p.turns5h = countTurns5h(p.turnHistory);
    p.isStale = isQuotaStale(p.lastQuotaUpdate);
  }
  await fs.mkdir(path.dirname(filePath), { recursive: true }).catch(() => {});
  await fs.writeFile(filePath, JSON.stringify(data, null, 2), { mode: 0o600 });
}

export async function recordWorkerTurn(root, worker, type = 'claim', taskId = '') {
  if (!worker) return null;
  const data = await loadProfiles(root);
  const profileId = parseProfileIdFromWorker(worker, data.profiles) || data.activeProfileId;
  const profile = data.profiles.find(p => p.id === profileId);
  if (profile) {
    if (!Array.isArray(profile.turnHistory)) profile.turnHistory = [];
    const event = { timestamp: new Date().toISOString(), type, taskId };
    profile.turnHistory.push(event);
    // Keep max 200 events in history
    if (profile.turnHistory.length > 200) {
      profile.turnHistory = profile.turnHistory.slice(-200);
    }
    profile.turns5h = countTurns5h(profile.turnHistory);
    profile.lastUsedAt = new Date().toISOString();
    await saveProfiles(root, data);
    return { profileId, turns5h: profile.turns5h };
  }
  return null;
}

export async function handleQuotaErrorEvent(root, worker, reason = 'ResourceExhausted') {
  const data = await loadProfiles(root);
  const profileId = parseProfileIdFromWorker(worker, data.profiles) || data.activeProfileId;
  const profile = data.profiles.find(p => p.id === profileId);
  if (!profile) return null;

  profile.status = 'hết token';
  profile.quotaPercent = 0;
  profile.quotaSource = 'lỗi quota';
  profile.lastQuotaUpdate = new Date().toISOString();
  profile.isStale = false;

  // Find next ready profile to suggest
  const nextProfile = data.profiles.find(p => p.id !== profile.id && p.status !== 'hết token');

  data.switchHistory.unshift({
    timestamp: new Date().toISOString(),
    fromId: profile.id,
    toId: nextProfile ? nextProfile.id : null,
    reason: `[Tự động bridge] Phát hiện lỗi quota/429/ResourceExhausted từ worker ${worker}: ${reason}`
  });

  data.suggestedSwitch = nextProfile ? {
    targetId: nextProfile.id,
    targetName: nextProfile.name,
    reason: `Tài khoản ${profile.name} đã hết token (${reason}). Hãy chuyển sang ${nextProfile.name}.`
  } : null;

  await saveProfiles(root, data);
  return { profile, suggestedSwitch: data.suggestedSwitch };
}
