import fs from 'node:fs/promises';
import fsSync from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { execFile, spawn } from 'node:child_process';
import { promisify } from 'node:util';
import { fileURLToPath } from 'node:url';
import { TaskStore, BridgeError } from './store.mjs';
import { rootFor, readEnvironment, argumentsFor } from './config.mjs';

const execFileAsync = promisify(execFile);
const __dirname = path.dirname(fileURLToPath(import.meta.url));

// Configuration & Constants
const POLL_INTERVAL_MS = 2000;
const HEARTBEAT_INTERVAL_MS = 3000;
const LEASE_TIMEOUT_MS = 600000; // 10 minutes
const STALE_LOCK_TIMEOUT_MS = 15000; // 15 seconds
const PRIORITY_TASK_ID = 'unity-enter-shop-scene-transition-redo-006';

// Redact secrets from text
function redact(text) {
  if (typeof text !== 'string') return text;
  return text.replace(/[a-f0-9]{32,64}/gi, '***REDACTED***');
}

export class IdeAutoWorker {
  constructor(root, options = {}) {
    this.root = root;
    this.options = options;
    this.runtimeDir = path.join(this.root, '.tasks', 'runtime');
    this.lockFile = path.join(this.runtimeDir, '.worker.lock');
    this.statusFile = path.join(this.runtimeDir, 'worker-status.json');
    this.logFile = path.join(this.runtimeDir, 'ide-worker.log');
    this.currentTaskFile = path.join(this.root, '.tasks', 'current-task.md');
    this.executionPayloadFile = path.join(this.runtimeDir, 'current-task-execution.json');

    const hostClean = os.hostname().toLowerCase().replace(/[^a-z0-9]/g, '');
    this.workerId = options.workerId || `antigravity-auto-worker-${hostClean}`;
    this.store = new TaskStore(root);

    this.isRunning = false;
    this.pollTimer = null;
    this.heartbeatTimer = null;
    this.currentTaskId = null;
    this.lastResult = null;
    this.startTime = Date.now();
  }

  log(message, level = 'INFO') {
    const time = new Date().toISOString();
    const cleanMsg = redact(message);
    const line = `[${time}] [${level}] [${this.workerId}] ${cleanMsg}\n`;
    try {
      process.stdout.write(line);
      fsSync.appendFileSync(this.logFile, line);
    } catch {}
  }

  async acquireLock() {
    await fs.mkdir(this.runtimeDir, { recursive: true });
    try {
      const existing = await fs.readFile(this.lockFile, 'utf8');
      const lockData = JSON.parse(existing);
      const isDead = await this.isProcessDead(lockData.pid);
      const isStale = (Date.now() - lockData.heartbeat) > STALE_LOCK_TIMEOUT_MS;

      if (!isDead && !isStale) {
        throw new Error(`Another worker is active (PID ${lockData.pid}, heartbeat ${new Date(lockData.heartbeat).toISOString()})`);
      }
      this.log(`Taking over stale lock from PID ${lockData.pid}`, 'WARN');
    } catch (err) {
      if (err.code !== 'ENOENT' && !err.message.includes('Taking over stale lock')) {
        throw err;
      }
    }

    const payload = {
      pid: process.pid,
      workerId: this.workerId,
      workspace: this.root,
      startedAt: new Date().toISOString(),
      heartbeat: Date.now()
    };
    await fs.writeFile(this.lockFile, JSON.stringify(payload, null, 2), { flag: 'w' });
    this.log(`Acquired single-instance lock for PID ${process.pid}`);
  }

  async isProcessDead(pid) {
    if (!pid || typeof pid !== 'number') return true;
    try {
      process.kill(pid, 0);
      return false;
    } catch {
      return true;
    }
  }

  async releaseLock() {
    try {
      const existing = await fs.readFile(this.lockFile, 'utf8');
      const lockData = JSON.parse(existing);
      if (lockData.pid === process.pid) {
        await fs.unlink(this.lockFile);
        this.log('Released single-instance lock');
      }
    } catch {}
  }

  async updateHeartbeat() {
    try {
      const lockData = {
        pid: process.pid,
        workerId: this.workerId,
        workspace: this.root,
        startedAt: new Date(this.startTime).toISOString(),
        heartbeat: Date.now()
      };
      await fs.writeFile(this.lockFile, JSON.stringify(lockData, null, 2));

      const status = {
        workerOnline: true,
        workerId: this.workerId,
        currentTaskId: this.currentTaskId,
        lastHeartbeat: new Date().toISOString(),
        agentRunner: 'agentapi',
        lastResult: this.lastResult,
        uptimeSeconds: Math.round((Date.now() - this.startTime) / 1000)
      };
      await fs.writeFile(this.statusFile, JSON.stringify(status, null, 2));
    } catch (err) {
      this.log(`Heartbeat update error: ${err.message}`, 'WARN');
    }
  }

  async findActiveConversation() {
    const brainDir = path.join(os.homedir(), '.gemini', 'antigravity-ide', 'brain');
    try {
      const entries = await fs.readdir(brainDir, { withFileTypes: true });
      const dirs = entries.filter(e => e.isDirectory() && e.name !== 'tempmediaStorage');
      const stats = await Promise.all(dirs.map(async d => {
        const s = await fs.stat(path.join(brainDir, d.name));
        return { name: d.name, mtime: s.mtimeMs };
      }));
      stats.sort((a, b) => b.mtime - a.mtime);

      const targetRoot = this.root.replace(/\\/g, '/').toLowerCase();
      for (const item of stats.slice(0, 6)) {
        try {
          const { stdout } = await execFileAsync('agentapi.bat', ['get-conversation-metadata', item.name], { shell: true });
          const meta = JSON.parse(stdout);
          const workspaces = meta?.response?.conversationMetadata?.metadata?.workspaces || [];
          const match = workspaces.some(w => {
            const uri = (w.workspaceFolderAbsoluteUri || '').toLowerCase();
            return uri.includes(targetRoot) || uri.includes(targetRoot.replace(':', '%3a'));
          });
          if (match) {
            return item.name;
          }
        } catch {}
      }
      return stats[0]?.name || null;
    } catch (err) {
      this.log(`Conversation lookup warning: ${err.message}`, 'WARN');
      return null;
    }
  }

  async dispatchTaskToAgent(task) {
    this.log(`Dispatching task ${task.id} ("${task.title}") to Agent runner...`);

    // 1. Create and save complete execution payload
    const executionPayload = {
      task_id: task.id,
      title: task.title,
      goal: task.goal,
      files: task.files || [],
      steps: task.steps || [],
      acceptance_criteria: task.acceptance_criteria || [],
      project_root: this.root,
      worker_id: this.workerId,
      dispatched_at: new Date().toISOString()
    };
    await fs.writeFile(this.executionPayloadFile, JSON.stringify(executionPayload, null, 2));

    // 2. Update current-task.md
    const currentTaskMd = `# [AUTO-WORKER ASSIGNED] ${task.title}\n\nTask ID: \`${task.id}\`\nStatus: \`${task.status}\`\nWorker: \`${this.workerId}\`\nDispatched: \`${new Date().toISOString()}\`\n\n## Mục tiêu\n${task.goal}\n\n## Files liên quan\n${(task.files || []).map(f => `- \`${f}\``).join('\n')}\n\n## Các bước thực hiện\n${(task.steps || []).map((s, i) => `${i + 1}. ${s}`).join('\n')}\n\n## Acceptance Criteria\n${(task.acceptance_criteria || []).map(a => `- ${a}`).join('\n')}\n`;
    await fs.writeFile(this.currentTaskFile, currentTaskMd);

    // 3. Dispatch to Antigravity via agentapi
    const activeConversationId = await this.findActiveConversation();
    const promptMessage = `[AUTO-WORKER DISPATCH] Đã tự động claim task mới: ${task.title} (ID: ${task.id}). Mục tiêu: ${task.goal}. Acceptance Criteria: ${(task.acceptance_criteria || []).join('; ')}. Vui lòng thực thi và báo kết quả!`;

    let dispatched = false;
    if (activeConversationId) {
      try {
        this.log(`Sending message via agentapi to conversation ${activeConversationId}`);
        await execFileAsync('agentapi.bat', [
          'send-message',
          `"--title=[Auto-Worker] Task: ${task.id}"`,
          activeConversationId,
          `"${promptMessage.replace(/[\r\n"']/g, ' ')}"`
        ], { shell: true });
        this.log(`Successfully dispatched task ${task.id} to conversation ${activeConversationId}`);
        dispatched = true;
      } catch (err) {
        this.log(`agentapi send-message error: ${err.message}`, 'WARN');
      }
    }

    // 4. Companion trigger via antigravity-ide.cmd chat
    const ideCmd = 'C:\\Users\\tan\\AppData\\Local\\Programs\\Antigravity IDE\\bin\\antigravity-ide.cmd';
    try {
      if (fsSync.existsSync(ideCmd)) {
        await execFileAsync(`"${ideCmd}"`, ['chat', '-r', `--mode=agent`, `"[Auto-Worker] Thực hiện task ${task.id}: ${task.title}"`], { shell: true });
        this.log(`Companion chat trigger sent via antigravity-ide.cmd`);
        dispatched = true;
      }
    } catch (err) {
      this.log(`antigravity-ide chat trigger warning: ${err.message}`, 'DEBUG');
    }

    return dispatched;
  }

  async pollAndProcess() {
    if (this.currentTaskId) {
      // Currently processing a task, check if completed
      await this.checkCurrentTaskCompletion();
      return;
    }

    const env = await readEnvironment(this.root);
    const bridgeUrl = env.BRIDGE_PUBLIC_URL || 'https://game.zcloudviet.xyz';
    const workerToken = env.BRIDGE_WORKER_TOKEN;

    let pendingTasks = [];

    // Try cloud bridge first if token and URL are available
    if (bridgeUrl && workerToken) {
      try {
        const res = await fetch(`${bridgeUrl}/v1/tasks?status=pending`, {
          headers: { 'Authorization': `Bearer ${workerToken}` }
        });
        if (res.ok) {
          const data = await res.json();
          pendingTasks = data.tasks || [];
        }
      } catch (err) {
        this.log(`Cloud bridge fetch error: ${err.message}`, 'DEBUG');
      }
    }

    // Also check local store if no cloud tasks found
    if (pendingTasks.length === 0) {
      try {
        const localList = await this.store.list('pending');
        pendingTasks = localList.tasks || [];
      } catch (err) {
        this.log(`Local store list error: ${err.message}`, 'DEBUG');
      }
    }

    if (pendingTasks.length === 0) {
      return;
    }

    // Apply Queue Policy:
    // 1. Priority to PRIORITY_TASK_ID ('unity-enter-shop-scene-transition-redo-006')
    // 2. FIFO order by created_at
    pendingTasks.sort((a, b) => {
      if (a.id === PRIORITY_TASK_ID) return -1;
      if (b.id === PRIORITY_TASK_ID) return 1;
      return (a.created_at || '').localeCompare(b.created_at || '');
    });

    const candidate = pendingTasks[0];
    this.log(`Found candidate task: ${candidate.id} ("${candidate.title}")`);

    // Atomic claim
    let claimedTask = null;
    if (bridgeUrl && workerToken) {
      try {
        const claimRes = await fetch(`${bridgeUrl}/v1/claim`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${workerToken}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ worker: this.workerId, id: candidate.id })
        });
        if (claimRes.ok) {
          const claimData = await claimRes.json();
          claimedTask = claimData.task;
        } else {
          this.log(`Cloud claim returned ${claimRes.status}`, 'WARN');
        }
      } catch (err) {
        this.log(`Cloud claim exception: ${err.message}`, 'WARN');
      }
    }

    if (!claimedTask) {
      try {
        const localClaim = await this.store.claim({ worker: this.workerId, id: candidate.id });
        claimedTask = localClaim.task;
      } catch (err) {
        this.log(`Local claim exception: ${err.message}`, 'WARN');
      }
    }

    if (!claimedTask) {
      this.log(`Failed to claim task ${candidate.id} (may have been claimed by another worker)`);
      return;
    }

    this.log(`Successfully claimed task ${claimedTask.id} (revision: ${claimedTask.revision})!`);
    this.currentTaskId = claimedTask.id;
    await this.updateHeartbeat();

    // Fetch full task details if needed
    let fullTask = claimedTask;
    if (bridgeUrl && workerToken) {
      try {
        const fullRes = await fetch(`${bridgeUrl}/v1/tasks/${claimedTask.id}`, {
          headers: { 'Authorization': `Bearer ${workerToken}` }
        });
        if (fullRes.ok) {
          const fullData = await fullRes.json();
          if (fullData.task) fullTask = fullData.task;
        }
      } catch {}
    }

    // Sync to local store
    try {
      await this.store.save(fullTask);
    } catch {}

    // Dispatch to Agent
    await this.dispatchTaskToAgent(fullTask);
  }

  async checkCurrentTaskCompletion() {
    if (!this.currentTaskId) return;

    const env = await readEnvironment(this.root);
    const bridgeUrl = env.BRIDGE_PUBLIC_URL || 'https://game.zcloudviet.xyz';
    const workerToken = env.BRIDGE_WORKER_TOKEN;

    let task = null;
    if (bridgeUrl && workerToken) {
      try {
        const res = await fetch(`${bridgeUrl}/v1/tasks/${this.currentTaskId}`, {
          headers: { 'Authorization': `Bearer ${workerToken}` }
        });
        if (res.ok) {
          const data = await res.json();
          task = data.task;
        }
      } catch {}
    }

    if (!task) {
      try {
        task = await this.store.read(this.currentTaskId);
      } catch {}
    }

    if (task && (task.status === 'done' || task.status === 'blocked')) {
      this.log(`Task ${this.currentTaskId} finished with status: ${task.status}!`);
      this.lastResult = {
        taskId: this.currentTaskId,
        status: task.status,
        summary: task.result?.summary || 'Completed',
        completedAt: new Date().toISOString()
      };
      this.currentTaskId = null;
    }
  }

  async start() {
    this.log(`Starting IDE Auto Worker on ${this.root}...`);
    await this.store.init();
    await this.acquireLock();
    this.isRunning = true;

    // Start heartbeat
    await this.updateHeartbeat();
    this.heartbeatTimer = setInterval(() => this.updateHeartbeat(), HEARTBEAT_INTERVAL_MS);

    // Start polling loop
    const pollLoop = async () => {
      if (!this.isRunning) return;
      try {
        await this.pollAndProcess();
      } catch (err) {
        this.log(`Polling loop error: ${err.message}`, 'ERROR');
      }
      if (this.isRunning) {
        this.pollTimer = setTimeout(pollLoop, POLL_INTERVAL_MS);
      }
    };
    this.pollTimer = setTimeout(pollLoop, 500);

    // Register exit handlers
    const shutdown = async () => {
      await this.stop();
      process.exit(0);
    };
    process.on('SIGINT', shutdown);
    process.on('SIGTERM', shutdown);

    this.log(`IDE Auto Worker online and watching for tasks!`);
  }

  async stop() {
    this.log(`Stopping IDE Auto Worker...`);
    this.isRunning = false;
    if (this.pollTimer) clearTimeout(this.pollTimer);
    if (this.heartbeatTimer) clearInterval(this.heartbeatTimer);
    await this.releaseLock();
    try {
      const status = {
        workerOnline: false,
        workerId: this.workerId,
        currentTaskId: null,
        lastHeartbeat: new Date().toISOString(),
        agentRunner: 'agentapi',
        lastResult: this.lastResult,
        uptimeSeconds: Math.round((Date.now() - this.startTime) / 1000)
      };
      await fs.writeFile(this.statusFile, JSON.stringify(status, null, 2));
    } catch {}
  }
}

// CLI entrypoint
if (process.argv[1] === fileURLToPath(import.meta.url)) {
  (async () => {
    try {
      const args = argumentsFor(process.argv.slice(2));
      const root = await rootFor(args);
      const worker = new IdeAutoWorker(root, args);
      await worker.start();
    } catch (err) {
      console.error(`Worker start failed: ${err.message}`);
      process.exit(1);
    }
  })();
}
