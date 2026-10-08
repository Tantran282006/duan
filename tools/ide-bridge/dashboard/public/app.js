// Phố Nhỏ Developer Dashboard Client Logic

const state = {
  currentTab: 'overview',
  overview: null,
  tasks: [],
  profilesData: null,
  doneData: null,
  selectedTask: null,
  pendingSwitchProfile: null,
  viewMode: 'kanban'
};

// DOM Elements
const elements = {
  navItems: document.querySelectorAll('.nav-item, .mob-item'),
  tabPanels: document.querySelectorAll('.tab-panel'),
  pageTitle: document.getElementById('page-title'),
  btnRefresh: document.getElementById('btn-refresh'),
  btnToggleSidebar: document.getElementById('btn-toggle-sidebar'),
  sidebar: document.getElementById('sidebar'),

  // Overview
  heroPercentText: document.getElementById('hero-percent-text'),
  heroProgressRing: document.getElementById('hero-progress-ring'),
  metricInProgress: document.getElementById('metric-inprogress'),
  metricPending: document.getElementById('metric-pending'),
  metricBlocked: document.getElementById('metric-blocked'),
  metricDone: document.getElementById('metric-done'),
  recentActivityFeed: document.getElementById('recent-activity-feed'),
  chipMilestone: document.getElementById('chip-milestone'),

  // Plan
  planSearch: document.getElementById('plan-search'),
  btnViewKanban: document.getElementById('btn-view-kanban'),
  btnViewTable: document.getElementById('btn-view-table'),
  kanbanContainer: document.getElementById('kanban-container'),
  tableContainer: document.getElementById('table-container'),
  tableTasksBody: document.getElementById('table-tasks-body'),
  badgePlanCount: document.getElementById('badge-plan-count'),
  cardsPending: document.getElementById('cards-pending'),
  cardsInProgress: document.getElementById('cards-in_progress'),
  cardsBlocked: document.getElementById('cards-blocked'),
  cardsDone: document.getElementById('cards-done'),
  countPending: document.getElementById('count-kanban-pending'),
  countInProgress: document.getElementById('count-kanban-inprogress'),
  countBlocked: document.getElementById('count-kanban-blocked'),
  countDone: document.getElementById('count-kanban-done'),

  // Done Log
  doneSearch: document.getElementById('done-search'),
  doneLogContainer: document.getElementById('done-log-container'),
  badgeDoneCount: document.getElementById('badge-done-count'),

  // Accounts
  bannerProfileName: document.getElementById('banner-profile-name'),
  bannerProfileEmail: document.getElementById('banner-profile-email'),
  bannerQuotaVal: document.getElementById('banner-quota-val'),
  bannerQuotaFill: document.getElementById('banner-quota-fill'),
  bannerQuotaSource: document.getElementById('banner-quota-source'),
  bannerProfileStatus: document.getElementById('banner-profile-status'),
  bannerQuotaStale: document.getElementById('banner-quota-stale'),
  bannerTurnsCount: document.getElementById('banner-turns-count'),
  bannerQuotaUpdated: document.getElementById('banner-quota-updated'),
  activeQuotaInput: document.getElementById('active-quota-input'),
  btnSaveActiveQuota: document.getElementById('btn-save-active-quota'),
  staleWarningBox: document.getElementById('stale-warning-box'),
  suggestedSwitchBox: document.getElementById('suggested-switch-box'),
  suggestedSwitchReason: document.getElementById('suggested-switch-reason'),
  btnAcceptSuggestedSwitch: document.getElementById('btn-accept-suggested-switch'),
  btnDismissSuggestedSwitch: document.getElementById('btn-dismiss-suggested-switch'),
  quotaWarningBox: document.getElementById('quota-warning-box'),
  profileCardsContainer: document.getElementById('profile-cards-container'),
  auditHistoryBody: document.getElementById('audit-history-body'),
  btnQuickSwitchNext: document.getElementById('btn-quick-switch-next'),
  hdrProfileName: document.getElementById('hdr-profile-name'),
  hdrProfileQuota: document.getElementById('hdr-profile-quota'),

  // Drawer
  drawer: document.getElementById('task-drawer'),
  drawerOverlay: document.getElementById('drawer-overlay'),
  btnCloseDrawer: document.getElementById('btn-close-drawer'),
  drawerTaskId: document.getElementById('drawer-task-id'),
  drawerTaskTitle: document.getElementById('drawer-task-title'),
  drawerStatusPill: document.getElementById('drawer-status-pill'),
  drawerWorkerPill: document.getElementById('drawer-worker-pill'),
  drawerRevPill: document.getElementById('drawer-rev-pill'),
  drawerTaskGoal: document.getElementById('drawer-task-goal'),
  drawerTaskFiles: document.getElementById('drawer-task-files'),
  drawerTaskSteps: document.getElementById('drawer-task-steps'),
  drawerTaskCriteria: document.getElementById('drawer-task-criteria'),
  drawerTaskResult: document.getElementById('drawer-task-result'),
  drawerResultSection: document.getElementById('drawer-result-section'),

  // Switch Modal
  switchModal: document.getElementById('switch-modal'),
  modalTargetProfileName: document.getElementById('modal-target-profile-name'),
  modalTaskWarning: document.getElementById('modal-task-warning'),
  modalSwitchReason: document.getElementById('modal-switch-reason'),
  btnCancelSwitch: document.getElementById('btn-cancel-switch'),
  btnConfirmSwitch: document.getElementById('btn-confirm-switch'),

  // Drawer Actions
  btnDrawerClaim: document.getElementById('btn-drawer-claim'),
  btnDrawerBlock: document.getElementById('btn-drawer-block'),
  btnDrawerUnclaim: document.getElementById('btn-drawer-unclaim'),
  btnDrawerDone: document.getElementById('btn-drawer-done'),
  btnDrawerDelete: document.getElementById('btn-drawer-delete'),

  // Add Profile Modal
  btnOpenAddProfile: document.getElementById('btn-open-add-profile'),
  addProfileModal: document.getElementById('add-profile-modal'),
  newProfileName: document.getElementById('new-profile-name'),
  newProfileEmail: document.getElementById('new-profile-email'),
  newProfileQuota: document.getElementById('new-profile-quota'),
  btnCancelAddProfile: document.getElementById('btn-cancel-add-profile'),
  btnConfirmAddProfile: document.getElementById('btn-confirm-add-profile'),

  // Create Task Modal
  btnOpenCreateTask: document.getElementById('btn-open-create-task'),
  createTaskModal: document.getElementById('create-task-modal'),
  newTaskId: document.getElementById('new-task-id'),
  btnGenTaskId: document.getElementById('btn-gen-task-id'),
  newTaskTitle: document.getElementById('new-task-title'),
  newTaskGoal: document.getElementById('new-task-goal'),
  newTaskFiles: document.getElementById('new-task-files'),
  newTaskSteps: document.getElementById('new-task-steps'),
  newTaskCriteria: document.getElementById('new-task-criteria'),
  btnCancelCreateTask: document.getElementById('btn-cancel-create-task'),
  btnSubmitCreateTask: document.getElementById('btn-submit-create-task'),

  // Settings Modal
  btnOpenSettings: document.getElementById('btn-open-settings'),
  settingsModal: document.getElementById('settings-modal'),
  settingsThreshold: document.getElementById('settings-threshold'),
  settingsAutoSwitch: document.getElementById('settings-autoswitch'),
  btnCancelSettings: document.getElementById('btn-cancel-settings'),
  btnConfirmSettings: document.getElementById('btn-confirm-settings'),

  // Complete Task Modal
  completeTaskModal: document.getElementById('complete-task-modal'),
  completeSummary: document.getElementById('complete-summary'),
  completeValidation: document.getElementById('complete-validation'),
  btnCancelComplete: document.getElementById('btn-cancel-complete'),
  btnConfirmComplete: document.getElementById('btn-confirm-complete'),

  // MAX VFX Elements
  ambientCanvas: document.getElementById('ambient-canvas'),
  btnToggleSound: document.getElementById('btn-toggle-sound'),
  soundIcon: document.getElementById('sound-icon'),
  toastContainer: document.getElementById('toast-container'),

  // Footer
  footerLastUpdated: document.getElementById('footer-last-updated')
};

// ==========================================
// MAX VFX SYSTEM: Web Audio Synthesizer SFX
// ==========================================
const SoundSynth = (() => {
  let ctx = null;
  let isMuted = false;
  try {
    isMuted = localStorage.getItem('phonho_sfx_muted') === 'true';
  } catch {}

  function getAudioCtx() {
    if (!ctx && typeof window !== 'undefined' && (window.AudioContext || window.webkitAudioContext)) {
      const AudioCtx = window.AudioContext || window.webkitAudioContext;
      ctx = new AudioCtx();
    }
    if (ctx && ctx.state === 'suspended') {
      ctx.resume().catch(() => {});
    }
    return ctx;
  }

  function playTone(freq, type = 'sine', duration = 0.05, gainLevel = 0.08, rampTo = null) {
    if (isMuted) return;
    try {
      const audio = getAudioCtx();
      if (!audio) return;
      const osc = audio.createOscillator();
      const gain = audio.createGain();

      osc.type = type;
      osc.frequency.setValueAtTime(freq, audio.currentTime);
      if (rampTo !== null) {
        osc.frequency.exponentialRampToValueAtTime(Math.max(10, rampTo), audio.currentTime + duration);
      }

      gain.gain.setValueAtTime(gainLevel, audio.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.0001, audio.currentTime + duration);

      osc.connect(gain);
      gain.connect(audio.destination);

      osc.start();
      osc.stop(audio.currentTime + duration);
    } catch {}
  }

  return {
    isMuted: () => isMuted,
    toggleMute: () => {
      isMuted = !isMuted;
      try {
        localStorage.setItem('phonho_sfx_muted', isMuted ? 'true' : 'false');
      } catch {}
      return isMuted;
    },
    playClick: () => playTone(680, 'sine', 0.04, 0.04, 380),
    playTab: () => playTone(540, 'triangle', 0.08, 0.06, 720),
    playClaim: () => {
      if (isMuted) return;
      playTone(523.25, 'sine', 0.12, 0.07);
      setTimeout(() => playTone(659.25, 'triangle', 0.15, 0.08), 80);
    },
    playSuccess: () => {
      if (isMuted) return;
      playTone(523.25, 'triangle', 0.1, 0.07);
      setTimeout(() => playTone(659.25, 'triangle', 0.1, 0.07), 80);
      setTimeout(() => playTone(783.99, 'triangle', 0.12, 0.08), 160);
      setTimeout(() => playTone(1046.50, 'sine', 0.25, 0.09), 240);
    },
    playWarning: () => {
      if (isMuted) return;
      playTone(460, 'sawtooth', 0.1, 0.05, 320);
    }
  };
})();

// ==========================================
// MAX VFX SYSTEM: Ambient Lantern Particles
// ==========================================
function initAmbientCanvas() {
  const canvas = elements.ambientCanvas;
  if (!canvas || typeof window === 'undefined') return;
  const ctx = canvas.getContext('2d');
  if (!ctx) return;

  let width = (canvas.width = window.innerWidth);
  let height = (canvas.height = window.innerHeight);

  window.addEventListener('resize', () => {
    width = canvas.width = window.innerWidth;
    height = canvas.height = window.innerHeight;
  });

  const mouse = { x: -1000, y: -1000, radius: 130 };
  window.addEventListener('mousemove', (e) => {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
  });
  window.addEventListener('mouseleave', () => {
    mouse.x = -1000;
    mouse.y = -1000;
  });

  const palette = [
    { r: 255, g: 159, b: 67 },  // Amber neon
    { r: 245, g: 158, b: 11 },  // Warm lantern gold
    { r: 56,  g: 189, b: 248 }, // Cozy cyan
    { r: 168, g: 85,  b: 247 }  // Soft purple
  ];

  const particleCount = 42;
  const particles = [];

  for (let i = 0; i < particleCount; i++) {
    const col = palette[Math.floor(Math.random() * palette.length)];
    particles.push({
      x: Math.random() * width,
      y: Math.random() * height,
      r: Math.random() * 2.8 + 1.2,
      vx: (Math.random() - 0.5) * 0.35,
      vy: -(Math.random() * 0.35 + 0.15),
      alpha: Math.random() * 0.45 + 0.25,
      pulse: Math.random() * Math.PI * 2,
      pulseSpeed: Math.random() * 0.02 + 0.01,
      color: col
    });
  }

  function render() {
    ctx.clearRect(0, 0, width, height);

    for (let i = 0; i < particles.length; i++) {
      const p = particles[i];

      p.x += p.vx;
      p.y += p.vy;
      p.pulse += p.pulseSpeed;

      if (p.x < -20) p.x = width + 20;
      if (p.x > width + 20) p.x = -20;
      if (p.y < -20) p.y = height + 20;
      if (p.y > height + 20) p.y = -20;

      const dx = p.x - mouse.x;
      const dy = p.y - mouse.y;
      const dist = Math.sqrt(dx * dx + dy * dy);
      let extraAlpha = 0;
      if (dist < mouse.radius && dist > 0) {
        const force = (mouse.radius - dist) / mouse.radius;
        p.x += (dx / dist) * force * 1.8;
        p.y += (dy / dist) * force * 1.8;
        extraAlpha = force * 0.4;
      }

      const currentAlpha = Math.min(0.9, Math.max(0.1, p.alpha + Math.sin(p.pulse) * 0.15 + extraAlpha));

      const grad = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 4);
      grad.addColorStop(0, `rgba(${p.color.r}, ${p.color.g}, ${p.color.b}, ${currentAlpha})`);
      grad.addColorStop(1, `rgba(${p.color.r}, ${p.color.g}, ${p.color.b}, 0)`);

      ctx.beginPath();
      ctx.arc(p.x, p.y, p.r * 4, 0, Math.PI * 2);
      ctx.fillStyle = grad;
      ctx.fill();

      ctx.beginPath();
      ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
      ctx.fillStyle = `rgba(255, 255, 255, ${currentAlpha * 0.95})`;
      ctx.fill();
    }

    if (!document.hidden) {
      requestAnimationFrame(render);
    } else {
      setTimeout(() => requestAnimationFrame(render), 250);
    }
  }

  requestAnimationFrame(render);
}

// ==========================================
// MAX VFX SYSTEM: Modern Toast Notification
// ==========================================
function showToast(message, type = 'info', duration = 3000) {
  const container = elements.toastContainer || document.getElementById('toast-container');
  if (!container) return;

  const item = document.createElement('div');
  item.className = `toast-item ${type}`;

  const iconMap = {
    success: '✨',
    warning: '⚠️',
    error: '❌',
    info: '💡'
  };

  item.innerHTML = `
    <div class="toast-content-row">
      <span class="toast-icon">${iconMap[type] || '💡'}</span>
      <span class="toast-msg">${escapeHtml(message)}</span>
      <button class="toast-close-btn" aria-label="Đóng">×</button>
    </div>
    <div class="toast-progress-bar"></div>
  `;

  if (type === 'success') SoundSynth.playSuccess();
  else if (type === 'warning' || type === 'error') SoundSynth.playWarning();
  else SoundSynth.playClick();

  const closeBtn = item.querySelector('.toast-close-btn');
  const removeToast = () => {
    if (item.classList.contains('removing')) return;
    item.classList.add('removing');
    setTimeout(() => item.remove(), 280);
  };

  closeBtn?.addEventListener('click', removeToast);
  const timer = setTimeout(removeToast, duration);
  item.addEventListener('mouseenter', () => clearTimeout(timer));

  container.appendChild(item);
}

// ==========================================
// MAX VFX SYSTEM: Animated Number Counter
// ==========================================
function animateNumber(element, targetVal, duration = 500) {
  if (!element) return;
  const target = Number(targetVal);
  if (isNaN(target)) {
    element.textContent = targetVal;
    return;
  }
  const startVal = Number(element.dataset.val || 0);
  element.dataset.val = target;
  if (startVal === target) {
    element.textContent = target;
    return;
  }
  const startTime = performance.now();
  function update(currentTime) {
    const elapsed = currentTime - startTime;
    const progress = Math.min(elapsed / duration, 1);
    const ease = 1 - Math.pow(1 - progress, 3);
    const current = Math.round(startVal + (target - startVal) * ease);
    element.textContent = current;
    if (progress < 1) {
      requestAnimationFrame(update);
    } else {
      element.textContent = target;
    }
  }
  requestAnimationFrame(update);
}

// ==========================================
// MAX VFX SYSTEM: Ripple Click Feedback
// ==========================================
function setupRippleEffects() {
  document.addEventListener('pointerdown', (e) => {
    const btn = e.target.closest('.btn-primary, .btn-secondary, .btn-icon, .nav-item, .task-card');
    if (!btn) return;

    SoundSynth.playClick();

    const rect = btn.getBoundingClientRect();
    const wave = document.createElement('span');
    wave.className = 'ripple-wave';
    const size = Math.max(rect.width, rect.height);
    wave.style.width = wave.style.height = `${size}px`;
    wave.style.left = `${e.clientX - rect.left - size / 2}px`;
    wave.style.top = `${e.clientY - rect.top - size / 2}px`;

    btn.appendChild(wave);
    setTimeout(() => wave.remove(), 600);
  });
}

// Tab Switching with SFX
function switchTab(tabId) {
  if (state.currentTab !== tabId) {
    SoundSynth.playTab();
  }
  state.currentTab = tabId;
  elements.navItems.forEach(item => {
    item.classList.toggle('active', item.dataset.tab === tabId);
  });
  elements.tabPanels.forEach(panel => {
    panel.classList.toggle('active', panel.id === `tab-${tabId}`);
  });

  const titles = {
    overview: 'Tổng quan dự án',
    plan: 'Kế hoạch công việc',
    done: 'Lịch sử hoàn thành (Done Log)',
    accounts: 'Quản lý Tài khoản Antigravity IDE'
  };
  elements.pageTitle.textContent = titles[tabId] || 'Phố Nhỏ Dev Dashboard';
}

// Fetch Overview Data with Animated Counters
async function loadOverview() {
  try {
    const res = await fetch('/api/overview');
    if (!res.ok) throw new Error('Không thể tải tổng quan');
    const data = await res.json();
    state.overview = data;

    // Update Hero with Progress
    elements.heroPercentText.textContent = `${data.completionPercent}%`;
    elements.heroProgressRing.parentElement.style.background = `conic-gradient(var(--accent) 0% ${data.completionPercent}%, #202b38 ${data.completionPercent}% 100%)`;
    elements.chipMilestone.textContent = `Mốc: ${data.currentMilestone}`;

    // Update Metrics with smooth counter animation
    animateNumber(elements.metricInProgress, data.metrics.inProgress);
    animateNumber(elements.metricPending, data.metrics.pending);
    animateNumber(elements.metricBlocked, data.metrics.blocked);
    animateNumber(elements.metricDone, data.recentSessions.length);

    // Recent Activity
    renderRecentActivity(data.recentSessions);

    if (elements.footerLastUpdated) {
      elements.footerLastUpdated.textContent = new Date().toLocaleTimeString('vi-VN');
    }
  } catch (err) {
    console.error('Lỗi nạp overview:', err);
  }
}

function renderRecentActivity(sessions) {
  if (!sessions || sessions.length === 0) {
    elements.recentActivityFeed.innerHTML = '<div class="empty-state">Chưa có nhật ký gần đây</div>';
    return;
  }
  elements.recentActivityFeed.innerHTML = sessions.map(s => `
    <div class="activity-item">
      <div class="act-title-row">
        <span class="act-title">Phiên ${s.sessionNumber}: ${escapeHtml(s.title)}</span>
        <span class="act-date">${escapeHtml(s.date)}</span>
      </div>
      <div class="act-desc">
        ${s.doneItems.length > 0 ? escapeHtml(s.doneItems[0]) : 'Cập nhật tiến độ dự án'}
      </div>
    </div>
  `).join('');
}

// Fetch Tasks for Plan Tab
async function loadTasks() {
  try {
    const res = await fetch('/api/tasks');
    if (!res.ok) throw new Error('Không thể tải tasks');
    const data = await res.json();
    state.tasks = data.tasks || [];
    elements.badgePlanCount.textContent = state.tasks.length;
    renderTasks();
  } catch (err) {
    console.error('Lỗi nạp tasks:', err);
  }
}

function renderTasks() {
  const query = (elements.planSearch.value || '').toLowerCase().trim();
  const filtered = state.tasks.filter(t => {
    return t.id.toLowerCase().includes(query) ||
           (t.title && t.title.toLowerCase().includes(query)) ||
           (t.worker && t.worker.toLowerCase().includes(query));
  });

  const columns = {
    pending: [],
    in_progress: [],
    blocked: [],
    done: []
  };

  filtered.forEach(task => {
    const st = task.status || 'pending';
    if (columns[st]) columns[st].push(task);
  });

  // Update column counts
  elements.countPending.textContent = columns.pending.length;
  elements.countInProgress.textContent = columns.in_progress.length;
  elements.countBlocked.textContent = columns.blocked.length;
  elements.countDone.textContent = columns.done.length;

  // Render Kanban cards
  ['pending', 'in_progress', 'blocked', 'done'].forEach(st => {
    const container = elements[`cards${st === 'in_progress' ? 'InProgress' : capitalize(st)}`];
    if (!container) return;
    if (columns[st].length === 0) {
      container.innerHTML = '<div class="empty-slot" style="color:var(--text-sub);font-size:12px;text-align:center;padding:16px;">(Trống)</div>';
    } else {
      container.innerHTML = columns[st].map(task => `
        <div class="task-card" data-id="${task.id}" draggable="true">
          <div class="task-id-badge">${escapeHtml(task.id)}</div>
          <div class="task-title">${escapeHtml(task.title || 'Chưa đặt tiêu đề')}</div>
          <div class="task-meta-row">
            <span class="worker-tag">👤 ${task.worker ? escapeHtml(task.worker) : 'Chưa nhận'}</span>
            <span class="rev-tag">Rev ${task.revision || 1}</span>
          </div>
        </div>
      `).join('');
    }
  });

  // Render Table
  elements.tableTasksBody.innerHTML = filtered.map(t => `
    <tr data-id="${t.id}" style="cursor:pointer;">
      <td><code style="color:var(--accent);">${escapeHtml(t.id)}</code></td>
      <td><strong>${escapeHtml(t.title || '')}</strong></td>
      <td><span class="status-pill ${t.status}">${escapeHtml(t.status)}</span></td>
      <td>${t.worker ? escapeHtml(t.worker) : '<span style="color:var(--text-sub);">—</span>'}</td>
      <td>Rev ${t.revision || 1}</td>
      <td><button class="btn-text btn-inspect" data-id="${t.id}">Xem</button></td>
    </tr>
  `).join('');

  // Add click events
  document.querySelectorAll('.task-card, .data-table tr[data-id], .btn-inspect').forEach(el => {
    el.addEventListener('click', (e) => {
      const id = el.dataset.id || el.closest('[data-id]')?.dataset.id;
      if (id) openDrawer(id);
    });
  });

  // Setup Drag and Drop on Cards
  document.querySelectorAll('.task-card').forEach(card => {
    card.addEventListener('dragstart', (e) => {
      e.dataTransfer.setData('text/plain', card.dataset.id);
      card.classList.add('dragging');
    });
    card.addEventListener('dragend', () => {
      card.classList.remove('dragging');
    });
  });

  // Setup Drag and Drop on Kanban Columns
  document.querySelectorAll('.kanban-col').forEach(col => {
    col.ondragover = (e) => {
      e.preventDefault();
      col.classList.add('drag-over');
    };
    col.ondragleave = () => {
      col.classList.remove('drag-over');
    };
    col.ondrop = async (e) => {
      e.preventDefault();
      col.classList.remove('drag-over');
      const taskId = e.dataTransfer.getData('text/plain');
      const targetStatus = col.dataset.status;
      if (taskId && targetStatus) {
        await updateTaskStatus(taskId, targetStatus);
      }
    };
  });
}

// Drawer Logic
function openDrawer(taskId) {
  const task = state.tasks.find(t => t.id === taskId);
  if (!task) return;
  state.selectedTask = task;

  elements.drawerTaskId.textContent = task.id;
  elements.drawerTaskTitle.textContent = task.title || '';
  elements.drawerStatusPill.textContent = task.status;
  elements.drawerStatusPill.className = `status-pill ${task.status}`;
  elements.drawerWorkerPill.textContent = task.worker ? `Worker: ${task.worker}` : 'Worker: (chưa nhận)';
  elements.drawerRevPill.textContent = `Revision: ${task.revision || 1}`;
  elements.drawerTaskGoal.textContent = task.goal || 'Chưa có mô tả mục tiêu.';

  // Update Drawer Action Buttons based on status
  if (elements.btnDrawerClaim && elements.btnDrawerBlock && elements.btnDrawerUnclaim && elements.btnDrawerDone && elements.btnDrawerDelete) {
    if (task.status === 'pending') {
      elements.btnDrawerClaim.style.display = 'inline-flex';
      elements.btnDrawerClaim.textContent = '⚡ Nhận Task (Claim)';
      elements.btnDrawerBlock.style.display = 'none';
      elements.btnDrawerUnclaim.style.display = 'none';
      elements.btnDrawerDone.style.display = 'none';
      elements.btnDrawerDelete.style.display = 'inline-flex';
    } else if (task.status === 'in_progress') {
      elements.btnDrawerClaim.style.display = 'none';
      elements.btnDrawerBlock.style.display = 'inline-flex';
      elements.btnDrawerUnclaim.style.display = 'inline-flex';
      elements.btnDrawerDone.style.display = 'inline-flex';
      elements.btnDrawerDelete.style.display = 'inline-flex';
    } else if (task.status === 'blocked') {
      elements.btnDrawerClaim.style.display = 'inline-flex';
      elements.btnDrawerClaim.textContent = '⚡ Nhận lại Task';
      elements.btnDrawerBlock.style.display = 'none';
      elements.btnDrawerUnclaim.style.display = 'inline-flex';
      elements.btnDrawerDone.style.display = 'inline-flex';
      elements.btnDrawerDelete.style.display = 'inline-flex';
    } else if (task.status === 'done') {
      elements.btnDrawerClaim.style.display = 'none';
      elements.btnDrawerBlock.style.display = 'none';
      elements.btnDrawerUnclaim.style.display = 'none';
      elements.btnDrawerDone.style.display = 'none';
      elements.btnDrawerDelete.style.display = 'inline-flex';
    }
  }

  // Files
  if (task.files && task.files.length > 0) {
    elements.drawerTaskFiles.innerHTML = task.files.map(f => `<li>${escapeHtml(f)}</li>`).join('');
  } else {
    elements.drawerTaskFiles.innerHTML = '<li>(Không có file khai báo)</li>';
  }

  // Steps
  if (task.steps && task.steps.length > 0) {
    elements.drawerTaskSteps.innerHTML = task.steps.map(s => `<li>${escapeHtml(s)}</li>`).join('');
  } else {
    elements.drawerTaskSteps.innerHTML = '<li>(Không có danh sách bước)</li>';
  }

  // Criteria
  if (task.acceptance_criteria && task.acceptance_criteria.length > 0) {
    elements.drawerTaskCriteria.innerHTML = task.acceptance_criteria.map(c => `<li>${escapeHtml(c)}</li>`).join('');
  } else {
    elements.drawerTaskCriteria.innerHTML = '<li>(Không có tiêu chí)</li>';
  }

  // Result
  if (task.result) {
    elements.drawerResultSection.classList.remove('hidden');
    elements.drawerTaskResult.textContent = JSON.stringify(task.result, null, 2);
  } else {
    elements.drawerResultSection.classList.add('hidden');
  }

  elements.drawer.classList.add('active');
  elements.drawerOverlay.classList.add('active');
}

function closeDrawer() {
  elements.drawer.classList.remove('active');
  elements.drawerOverlay.classList.remove('active');
  state.selectedTask = null;
}

// Fetch Done Log
async function loadDoneLog() {
  try {
    const res = await fetch('/api/done-log');
    if (!res.ok) throw new Error('Không thể tải done log');
    const data = await res.json();
    state.doneData = data;
    elements.badgeDoneCount.textContent = (data.sessions || []).length;
    renderDoneLog();
  } catch (err) {
    console.error('Lỗi nạp done log:', err);
  }
}

function renderDoneLog() {
  if (!state.doneData) return;
  const query = (elements.doneSearch.value || '').toLowerCase().trim();
  const sessions = (state.doneData.sessions || []).filter(s => {
    return s.title.toLowerCase().includes(query) ||
           s.date.toLowerCase().includes(query) ||
           (s.verification && s.verification.toLowerCase().includes(query)) ||
           (s.files && s.files.some(f => f.toLowerCase().includes(query)));
  });

  if (sessions.length === 0) {
    elements.doneLogContainer.innerHTML = '<div class="empty-state" style="padding:40px;text-align:center;color:var(--text-sub);">Không tìm thấy kết quả phù hợp</div>';
    return;
  }

  elements.doneLogContainer.innerHTML = sessions.map(s => `
    <div class="card" style="margin-bottom:16px;">
      <div class="card-header" style="margin-bottom:12px;">
        <div>
          <span style="font-size:11px;color:var(--accent);font-weight:700;">PHIÊN ${s.sessionNumber}</span>
          <h3 style="margin-top:2px;">${escapeHtml(s.title)}</h3>
        </div>
        <span class="badge-source" style="font-size:12px;">📅 ${escapeHtml(s.date)}</span>
      </div>

      <div style="font-size:13px;margin-bottom:12px;">
        <strong style="color:var(--text-sub);">Việc đã làm:</strong>
        <ul style="padding-left:18px;margin-top:4px;">
          ${s.doneItems.map(item => `<li>${escapeHtml(item)}</li>`).join('')}
        </ul>
      </div>

      ${s.files && s.files.length > 0 ? `
        <div style="font-size:12px;margin-bottom:10px;">
          <strong style="color:var(--text-sub);">Files chính:</strong>
          <div style="display:flex;flex-wrap:wrap;gap:6px;margin-top:4px;">
            ${s.files.map(f => `<span class="source-tag" style="font-family:'JetBrains Mono';">${escapeHtml(f)}</span>`).join('')}
          </div>
        </div>
      ` : ''}

      <div style="font-size:12px;background:var(--elevated);padding:10px 14px;border-radius:var(--radius-md);border:1px solid var(--border);">
        <strong style="color:var(--done);">Kiểm tra xác thực:</strong>
        <span style="color:var(--text-sub);margin-left:6px;">${escapeHtml(s.verification || 'Chưa có dữ liệu')}</span>
      </div>
    </div>
  `).join('');
}

function formatQuota(val) {
  if (val === null || val === undefined || isNaN(val)) return 'chưa rõ';
  return `${val}%`;
}

function formatTime(iso) {
  if (!iso) return 'chưa rõ';
  try {
    const d = new Date(iso);
    if (isNaN(d.getTime())) return 'chưa rõ';
    const mins = Math.round((Date.now() - d.getTime()) / 60000);
    if (mins < 1) return 'vừa xong';
    if (mins < 60) return `${mins} phút trước`;
    return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' ' + d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
  } catch {
    return 'chưa rõ';
  }
}

async function updateQuota(profileId, payload) {
  try {
    const res = await fetch(`/api/profiles/${encodeURIComponent(profileId)}/quota`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể cập nhật quota');
    }
    await loadProfiles();
  } catch (err) {
    alert('Lỗi cập nhật: ' + err.message);
  }
}

// Fetch Profiles (Antigravity IDE)
async function loadProfiles() {
  try {
    const res = await fetch('/api/profiles');
    if (!res.ok) throw new Error('Không thể tải profile');
    const data = await res.json();
    state.profilesData = data;

    const active = data.profiles.find(p => p.id === data.activeProfileId) || data.profiles[0];
    let anyStale = false;

    if (active) {
      elements.bannerProfileName.textContent = active.name;
      elements.bannerProfileEmail.textContent = active.emailMasked;
      
      const qValStr = formatQuota(active.quotaPercent);
      elements.bannerQuotaVal.textContent = qValStr;
      elements.bannerQuotaFill.style.width = active.quotaPercent != null ? `${active.quotaPercent}%` : '0%';
      elements.bannerQuotaSource.textContent = `Nguồn: ${active.quotaSource || 'chưa rõ'}`;
      
      if (active.status === 'hết token') {
        elements.bannerProfileStatus.textContent = 'HẾT TOKEN';
        elements.bannerProfileStatus.className = 'badge-stale';
      } else {
        elements.bannerProfileStatus.textContent = active.status === 'active' ? 'Đang dùng' : 'Sẵn sàng';
        elements.bannerProfileStatus.className = 'badge-status-ok';
      }

      elements.bannerTurnsCount.textContent = `Ước tính theo lượt: ${active.turns5h || 0} lượt / 5h (ước tính)`;
      elements.bannerQuotaUpdated.textContent = `Cập nhật: ${formatTime(active.lastQuotaUpdate)}`;

      if (active.isStale) {
        elements.bannerQuotaStale.classList.remove('hidden');
        anyStale = true;
      } else {
        elements.bannerQuotaStale.classList.add('hidden');
      }

      elements.hdrProfileName.textContent = active.name.split('(')[0].trim();
      elements.hdrProfileQuota.textContent = `Quota: ${qValStr}`;

      // Check quota threshold
      const threshold = data.settings?.quotaThresholdPercent ?? 10;
      if (active.quotaPercent != null && active.quotaPercent < threshold) {
        elements.quotaWarningBox.classList.remove('hidden');
      } else {
        elements.quotaWarningBox.classList.add('hidden');
      }
    }

    // Check if any profile has stale data
    for (const p of data.profiles) {
      if (p.isStale) anyStale = true;
    }
    if (anyStale) {
      elements.staleWarningBox.classList.remove('hidden');
    } else {
      elements.staleWarningBox.classList.add('hidden');
    }

    // Handle suggested switch from bridge
    if (data.suggestedSwitch) {
      elements.suggestedSwitchBox.classList.remove('hidden');
      elements.suggestedSwitchReason.textContent = data.suggestedSwitch.reason;
      elements.btnAcceptSuggestedSwitch.onclick = () => {
        openSwitchModal(data.suggestedSwitch.targetId, data.suggestedSwitch.targetName);
      };
      elements.btnDismissSuggestedSwitch.onclick = async () => {
        await fetch('/api/profiles/dismiss-suggestion', { method: 'POST' });
        elements.suggestedSwitchBox.classList.add('hidden');
      };
    } else {
      elements.suggestedSwitchBox.classList.add('hidden');
    }

    renderProfileCards(data.profiles, data.activeProfileId);
    renderAuditHistory(data.switchHistory || []);
  } catch (err) {
    console.error('Lỗi nạp profiles:', err);
  }
}

function renderProfileCards(profiles, activeId) {
  elements.profileCardsContainer.innerHTML = profiles.map(p => {
    const isActive = p.id === activeId;
    const isExhausted = p.status === 'hết token';
    const qValStr = formatQuota(p.quotaPercent);
    const sourceLabel = escapeHtml(p.quotaSource || 'chưa rõ');
    const updateTimeStr = formatTime(p.lastQuotaUpdate);

    return `
      <div class="profile-card ${isActive ? 'active' : ''}">
        <div class="profile-card-header">
          <div>
            <div class="profile-card-title">${escapeHtml(p.name)}</div>
            <div class="profile-card-email">${escapeHtml(p.emailMasked)}</div>
          </div>
          ${isExhausted 
            ? '<span class="badge-stale" style="color:#ef4444;border-color:#ef4444;">HẾT TOKEN</span>' 
            : (isActive ? '<span class="badge-active">ĐANG DÙNG</span>' : '<span class="badge-source">SẴN SÀNG</span>')}
        </div>

        <div class="quota-stat" style="margin: 8px 0;">
          <div class="quota-label-row">
            <span>Quota còn lại:</span>
            <strong>${qValStr}</strong>
          </div>
          <div class="progress-bar-bg">
            <div class="progress-bar-fill" style="width: ${p.quotaPercent != null ? p.quotaPercent : 0}%;"></div>
          </div>
          
          <div style="display:flex;justify-content:space-between;align-items:center;font-size:10px;color:var(--text-sub);margin-top:4px;">
            <span>Nguồn: <strong style="color:var(--text);">${sourceLabel}</strong></span>
            <span>${p.isStale ? '<span class="badge-stale" style="font-size:9px;padding:1px 4px;">số liệu cũ</span>' : updateTimeStr}</span>
          </div>
          <div style="font-size:10px;color:var(--text-sub);margin-top:2px;">
            <span>Ước tính theo lượt: <strong>${p.turns5h || 0} lượt</strong> / 5h</span>
          </div>

          <!-- Manual Quota Adjustment Controls -->
          <div class="card-quota-controls">
            <span style="font-size:10px;color:var(--text-sub);">Nhập tay:</span>
            <button class="btn-mini btn-card-delta" data-id="${p.id}" data-delta="-5">-5%</button>
            <input type="number" class="input-card-quota" data-id="${p.id}" min="0" max="100" placeholder="${p.quotaPercent != null ? p.quotaPercent : '%'}" />
            <button class="btn-mini btn-card-delta" data-id="${p.id}" data-delta="5">+5%</button>
            <button class="btn-mini btn-primary btn-save-card-quota" data-id="${p.id}">Lưu</button>
          </div>
        </div>

        <div class="profile-card-footer" style="margin-top:10px;">
          ${isActive ? `
            <button class="btn-secondary" style="width:100%;justify-content:center;" disabled>✓ Đang kết nối</button>
          ` : `
            <button class="btn-primary btn-switch-profile" data-id="${p.id}" data-name="${escapeHtml(p.name)}" style="flex:1;justify-content:center;">
              Chuyển sang Profile này
            </button>
            <button class="btn-delete-profile" data-id="${p.id}" data-name="${escapeHtml(p.name)}" title="Xóa Profile này">
              Xóa
            </button>
          `}
        </div>
      </div>
    `;
  }).join('');

  // Switch button click events
  document.querySelectorAll('.btn-switch-profile').forEach(btn => {
    btn.addEventListener('click', () => {
      openSwitchModal(btn.dataset.id, btn.dataset.name);
    });
  });

  // Delete profile click events
  document.querySelectorAll('.btn-delete-profile').forEach(btn => {
    btn.addEventListener('click', async () => {
      const id = btn.dataset.id;
      const name = btn.dataset.name;
      if (confirm(`Bạn có chắc chắn muốn xóa profile "${name}"?`)) {
        await deleteProfile(id);
      }
    });
  });

  // Manual delta click events for cards
  document.querySelectorAll('.btn-card-delta').forEach(btn => {
    btn.addEventListener('click', async () => {
      const id = btn.dataset.id;
      const delta = Number(btn.dataset.delta);
      await updateQuota(id, { delta });
    });
  });

  // Manual save click events for cards
  document.querySelectorAll('.btn-save-card-quota').forEach(btn => {
    btn.addEventListener('click', async () => {
      const id = btn.dataset.id;
      const input = document.querySelector(`.input-card-quota[data-id="${id}"]`);
      if (input && input.value !== '') {
        const val = Number(input.value);
        if (isNaN(val) || val < 0 || val > 100) {
          alert('Vui lòng nhập số % từ 0 đến 100');
          return;
        }
        await updateQuota(id, { percent: val });
        input.value = '';
      }
    });
  });
}

function renderAuditHistory(history) {
  if (history.length === 0) {
    elements.auditHistoryBody.innerHTML = '<tr><td colspan="4" style="text-align:center;color:var(--text-sub);">Chưa có lịch sử chuyển đổi</td></tr>';
    return;
  }
  elements.auditHistoryBody.innerHTML = history.map(h => `
    <tr>
      <td style="font-size:11px;color:var(--text-sub);">${new Date(h.timestamp).toLocaleString('vi-VN')}</td>
      <td><code>${escapeHtml(h.fromId || 'Khởi tạo')}</code></td>
      <td><strong style="color:var(--accent);">${escapeHtml(h.toId)}</strong></td>
      <td>${escapeHtml(h.reason)}</td>
    </tr>
  `).join('');
}

// Switch Modal Logic
function openSwitchModal(targetId, targetName) {
  state.pendingSwitchProfile = targetId;
  elements.modalTargetProfileName.textContent = targetName;

  // Check if there is an in-progress task
  const inProgressTask = state.tasks.find(t => t.status === 'in_progress');
  if (inProgressTask) {
    elements.modalTaskWarning.classList.remove('hidden');
  } else {
    elements.modalTaskWarning.classList.add('hidden');
  }

  elements.switchModal.classList.remove('hidden');
}

function closeSwitchModal() {
  elements.switchModal.classList.add('hidden');
  state.pendingSwitchProfile = null;
}

async function confirmProfileSwitch() {
  if (!state.pendingSwitchProfile) return;
  const reason = elements.modalSwitchReason.value.trim() || 'Chuyển đổi từ Dashboard';
  try {
    elements.btnConfirmSwitch.disabled = true;
    elements.btnConfirmSwitch.textContent = 'Đang chuyển...';

    const res = await fetch('/api/profiles/switch', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ targetId: state.pendingSwitchProfile, reason })
    });
    if (!res.ok) throw new Error('Không thể đổi profile');
    const data = await res.json();

    closeSwitchModal();
    await loadProfiles();
    await loadTasks();

    if (data.inProgressWarning) {
      showToast(`Đã đổi profile! Lưu ý: Có ${data.inProgressWarning.count} task đang in_progress.`, 'warning', 5000);
    } else {
      showToast('Đã đổi profile Antigravity thành công!', 'success');
    }
  } catch (err) {
    showToast('Lỗi đổi profile: ' + err.message, 'error');
  } finally {
    elements.btnConfirmSwitch.disabled = false;
    elements.btnConfirmSwitch.textContent = 'Xác nhận chuyển';
  }
}

// Task API Operations
async function updateTaskStatus(taskId, status, extra = {}) {
  try {
    const res = await fetch(`/api/tasks/${encodeURIComponent(taskId)}/status`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status, ...extra })
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể cập nhật trạng thái task');
    }
    const data = await res.json();
    await Promise.all([loadTasks(), loadOverview()]);
    if (state.selectedTask && state.selectedTask.id === taskId) {
      openDrawer(taskId);
    }
    if (status === 'done') {
      showToast(`Task ${taskId} đã hoàn thành và lưu vào Done Log!`, 'success');
    } else if (status === 'blocked') {
      showToast(`Đã đánh dấu task ${taskId} bị chặn (Blocked)`, 'warning');
    } else {
      showToast(`Đã cập nhật trạng thái task sang "${status}"`, 'info');
    }
    return data;
  } catch (err) {
    showToast('Lỗi cập nhật task: ' + err.message, 'error');
  }
}

async function claimTask(taskId, worker) {
  try {
    const res = await fetch(`/api/tasks/${encodeURIComponent(taskId)}/claim`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ worker })
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể nhận task');
    }
    showToast(`Đã nhận task ${taskId} (Worker: ${worker})`, 'success');
    await Promise.all([loadTasks(), loadOverview()]);
    if (state.selectedTask && state.selectedTask.id === taskId) {
      openDrawer(taskId);
    }
  } catch (err) {
    showToast('Lỗi nhận task: ' + err.message, 'error');
  }
}

async function deleteTask(taskId) {
  try {
    const res = await fetch(`/api/tasks/${encodeURIComponent(taskId)}`, {
      method: 'DELETE'
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể xóa task');
    }
    showToast(`Đã xóa vĩnh viễn task ${taskId}`, 'info');
    closeDrawer();
    await Promise.all([loadTasks(), loadOverview()]);
  } catch (err) {
    showToast('Lỗi xóa task: ' + err.message, 'error');
  }
}

async function createTask(payload) {
  try {
    const res = await fetch('/api/tasks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể tạo task');
    }
    const data = await res.json();
    showToast(`Tạo task "${payload.id}" thành công!`, 'success');
    closeCreateTaskModal();
    await Promise.all([loadTasks(), loadOverview()]);
    if (data.task) openDrawer(data.task.id);
  } catch (err) {
    showToast('Lỗi tạo task: ' + err.message, 'error');
  }
}

async function deleteProfile(profileId) {
  try {
    const res = await fetch(`/api/profiles/${encodeURIComponent(profileId)}`, {
      method: 'DELETE'
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể xóa profile');
    }
    showToast('Đã xóa profile thành công', 'info');
    await loadProfiles();
  } catch (err) {
    showToast('Lỗi xóa profile: ' + err.message, 'error');
  }
}

async function saveSettings(payload) {
  try {
    const res = await fetch('/api/profiles/settings', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || 'Không thể lưu cài đặt');
    }
    showToast('Đã lưu cài đặt Quota & Cảnh báo thành công!', 'success');
    closeSettingsModal();
    await loadProfiles();
  } catch (err) {
    showToast('Lỗi lưu cài đặt: ' + err.message, 'error');
  }
}

// Modal Controllers
function openCreateTaskModal() {
  if (!elements.createTaskModal) return;
  if (elements.newTaskId) {
    elements.newTaskId.value = 'unity-task-' + Date.now().toString(36);
  }
  if (elements.newTaskTitle) elements.newTaskTitle.value = '';
  if (elements.newTaskGoal) elements.newTaskGoal.value = '';
  if (elements.newTaskFiles) elements.newTaskFiles.value = '';
  if (elements.newTaskSteps) elements.newTaskSteps.value = '';
  if (elements.newTaskCriteria) elements.newTaskCriteria.value = '';
  elements.createTaskModal.classList.remove('hidden');
}

function closeCreateTaskModal() {
  elements.createTaskModal?.classList.add('hidden');
}

function openAddProfileModal() {
  if (!elements.addProfileModal) return;
  if (elements.newProfileName) elements.newProfileName.value = '';
  if (elements.newProfileEmail) elements.newProfileEmail.value = '';
  if (elements.newProfileQuota) elements.newProfileQuota.value = '100';
  elements.addProfileModal.classList.remove('hidden');
}

function closeAddProfileModal() {
  elements.addProfileModal?.classList.add('hidden');
}

function openSettingsModal() {
  if (!elements.settingsModal) return;
  const currentSettings = state.profilesData?.settings || {};
  if (elements.settingsThreshold) {
    elements.settingsThreshold.value = currentSettings.quotaThresholdPercent ?? 10;
  }
  if (elements.settingsAutoSwitch) {
    elements.settingsAutoSwitch.checked = Boolean(currentSettings.autoSwitch);
  }
  elements.settingsModal.classList.remove('hidden');
}

function closeSettingsModal() {
  elements.settingsModal?.classList.add('hidden');
}

function openCompleteModal(task) {
  if (!elements.completeTaskModal) return;
  state.taskToComplete = task;
  if (elements.completeSummary) {
    elements.completeSummary.value = `Hoàn thành task ${task.id}: Đã triển khai và kiểm tra thành công.`;
  }
  if (elements.completeValidation) {
    elements.completeValidation.value = 'Xác nhận kiểm thử giao diện & chức năng pass 100%';
  }
  elements.completeTaskModal.classList.remove('hidden');
}

function closeCompleteModal() {
  elements.completeTaskModal?.classList.add('hidden');
  state.taskToComplete = null;
}

// Quick Switch to Next Profile
function quickSwitchNext() {
  if (!state.profilesData || !state.profilesData.profiles) return;
  const profiles = state.profilesData.profiles;
  const currentIndex = profiles.findIndex(p => p.id === state.profilesData.activeProfileId);
  const nextIndex = (currentIndex + 1) % profiles.length;
  const nextProfile = profiles[nextIndex];
  if (nextProfile) {
    openSwitchModal(nextProfile.id, nextProfile.name);
  }
}

// Helper Utilities
function escapeHtml(str) {
  if (!str) return '';
  return String(str).replace(/[&<>"']/g, m => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
  })[m]);
}

function capitalize(str) {
  return str.charAt(0).toUpperCase() + str.slice(1);
}

// Event Listeners
function setupEvents() {
  // Navigation
  elements.navItems.forEach(item => {
    item.addEventListener('click', () => switchTab(item.dataset.tab));
  });

  // Sidebar toggle
  elements.btnToggleSidebar.addEventListener('click', () => {
    elements.sidebar.classList.toggle('collapsed');
  });

  // Refresh
  elements.btnRefresh.addEventListener('click', async () => {
    elements.btnRefresh.classList.add('rotating');
    await Promise.all([loadOverview(), loadTasks(), loadDoneLog(), loadProfiles()]);
    setTimeout(() => elements.btnRefresh.classList.remove('rotating'), 500);
  });

  // Plan search & view toggle
  elements.planSearch.addEventListener('input', renderTasks);
  elements.btnViewKanban.addEventListener('click', () => {
    state.viewMode = 'kanban';
    elements.btnViewKanban.classList.add('active');
    elements.btnViewTable.classList.remove('active');
    elements.kanbanContainer.classList.remove('hidden');
    elements.tableContainer.classList.add('hidden');
  });
  elements.btnViewTable.addEventListener('click', () => {
    state.viewMode = 'table';
    elements.btnViewTable.classList.add('active');
    elements.btnViewKanban.classList.remove('active');
    elements.kanbanContainer.classList.add('hidden');
    elements.tableContainer.classList.remove('hidden');
  });

  // Done search
  elements.doneSearch.addEventListener('input', renderDoneLog);

  // Drawer events
  elements.btnCloseDrawer.addEventListener('click', closeDrawer);
  elements.drawerOverlay.addEventListener('click', closeDrawer);

  // Drawer Action Buttons
  elements.btnDrawerClaim?.addEventListener('click', async () => {
    if (!state.selectedTask) return;
    const defaultWorker = state.profilesData?.profiles?.find(p => p.id === state.profilesData?.activeProfileId)?.name || 'developer-dashboard-user';
    const worker = prompt('Nhập tên Worker nhận task:', defaultWorker);
    if (worker) {
      await claimTask(state.selectedTask.id, worker.trim());
    }
  });

  elements.btnDrawerBlock?.addEventListener('click', async () => {
    if (!state.selectedTask) return;
    const reason = prompt('Nhập lý do bị chặn (Blocked):', 'Chờ tài nguyên / thiết kế bổ sung');
    if (reason !== null) {
      await updateTaskStatus(state.selectedTask.id, 'blocked', { reason: reason.trim() });
    }
  });

  elements.btnDrawerUnclaim?.addEventListener('click', async () => {
    if (!state.selectedTask) return;
    if (confirm(`Trả task "${state.selectedTask.id}" về hàng chờ (Pending)?`)) {
      await updateTaskStatus(state.selectedTask.id, 'pending');
    }
  });

  elements.btnDrawerDone?.addEventListener('click', () => {
    if (!state.selectedTask) return;
    openCompleteModal(state.selectedTask);
  });

  elements.btnDrawerDelete?.addEventListener('click', async () => {
    if (!state.selectedTask) return;
    if (confirm(`Bạn có chắc muốn xóa vĩnh viễn task "${state.selectedTask.id}"?`)) {
      await deleteTask(state.selectedTask.id);
    }
  });

  // Complete Task Modal Events
  elements.btnCancelComplete?.addEventListener('click', closeCompleteModal);
  elements.btnConfirmComplete?.addEventListener('click', async () => {
    if (!state.taskToComplete) return;
    const summary = elements.completeSummary?.value.trim() || 'Hoàn tất task từ Dashboard';
    const validation = elements.completeValidation?.value.split('\n').map(s => s.trim()).filter(Boolean) || [];
    await updateTaskStatus(state.taskToComplete.id, 'done', { summary, validation });
    closeCompleteModal();
  });

  // Create Task Modal Events
  elements.btnOpenCreateTask?.addEventListener('click', openCreateTaskModal);
  elements.btnGenTaskId?.addEventListener('click', () => {
    if (elements.newTaskId) {
      elements.newTaskId.value = 'unity-task-' + Date.now().toString(36);
    }
  });
  elements.btnCancelCreateTask?.addEventListener('click', closeCreateTaskModal);
  elements.btnSubmitCreateTask?.addEventListener('click', async () => {
    const id = elements.newTaskId?.value.trim();
    const title = elements.newTaskTitle?.value.trim();
    const goal = elements.newTaskGoal?.value.trim();
    if (!id || !title || !goal) {
      showToast('Vui lòng điền đủ Mã Task (ID), Tiêu đề và Mục tiêu', 'warning');
      return;
    }
    const files = elements.newTaskFiles?.value.split('\n').map(s => s.trim()).filter(Boolean) || [];
    const steps = elements.newTaskSteps?.value.split('\n').map(s => s.trim()).filter(Boolean) || ['Thực hiện công việc theo yêu cầu'];
    const criteria = elements.newTaskCriteria?.value.split('\n').map(s => s.trim()).filter(Boolean) || ['Hoàn thành và kiểm thử đạt yêu cầu'];
    await createTask({ id, title, goal, files, steps, acceptance_criteria: criteria });
  });

  // Add Profile Modal Events
  elements.btnOpenAddProfile?.addEventListener('click', openAddProfileModal);
  elements.btnCancelAddProfile?.addEventListener('click', closeAddProfileModal);
  elements.btnConfirmAddProfile?.addEventListener('click', async () => {
    const name = elements.newProfileName?.value.trim();
    const email = elements.newProfileEmail?.value.trim();
    const quota = Number(elements.newProfileQuota?.value || 100);
    if (!name || !email) {
      showToast('Vui lòng nhập Tên hiển thị và Email Google', 'warning');
      return;
    }
    try {
      const res = await fetch('/api/profiles', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, quotaRemaining: quota })
      });
      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.error || 'Không thể tạo profile');
      }
      showToast(`Đã thêm profile "${name}" thành công!`, 'success');
      closeAddProfileModal();
      await loadProfiles();
    } catch (err) {
      showToast('Lỗi tạo profile: ' + err.message, 'error');
    }
  });

  // Settings Modal Events
  elements.btnOpenSettings?.addEventListener('click', openSettingsModal);
  elements.btnCancelSettings?.addEventListener('click', closeSettingsModal);
  elements.btnConfirmSettings?.addEventListener('click', async () => {
    const threshold = Number(elements.settingsThreshold?.value || 10);
    const autoSwitch = elements.settingsAutoSwitch?.checked || false;
    await saveSettings({ quotaThresholdPercent: threshold, autoSwitch });
  });

  // Sound Toggle Button Event
  if (SoundSynth.isMuted()) {
    if (elements.soundIcon) elements.soundIcon.textContent = '🔇';
    elements.btnToggleSound?.classList.add('muted');
  }

  elements.btnToggleSound?.addEventListener('click', () => {
    const isMuted = SoundSynth.toggleMute();
    if (elements.soundIcon) {
      elements.soundIcon.textContent = isMuted ? '🔇' : '🔊';
    }
    elements.btnToggleSound?.classList.toggle('muted', isMuted);
    showToast(isMuted ? 'Đã tắt hiệu ứng âm thanh SFX' : 'Đã bật hiệu ứng âm thanh SFX', 'info', 2000);
  });

  // Keyboard shortcuts
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape') {
      closeDrawer();
      closeSwitchModal();
      closeCreateTaskModal();
      closeAddProfileModal();
      closeSettingsModal();
      closeCompleteModal();
    }
  });

  // Banner quick quota controls
  document.querySelectorAll('.btn-delta[data-id="active"]').forEach(btn => {
    btn.addEventListener('click', async () => {
      const activeId = state.profilesData?.activeProfileId;
      if (!activeId) return;
      const delta = Number(btn.dataset.delta);
      await updateQuota(activeId, { delta });
    });
  });

  elements.btnSaveActiveQuota?.addEventListener('click', async () => {
    const activeId = state.profilesData?.activeProfileId;
    if (!activeId) return;
    const val = Number(elements.activeQuotaInput.value);
    if (isNaN(val) || val < 0 || val > 100) {
      showToast('Vui lòng nhập số % từ 0 đến 100', 'warning');
      return;
    }
    await updateQuota(activeId, { percent: val });
    elements.activeQuotaInput.value = '';
  });

  // Modal events
  elements.btnCancelSwitch.addEventListener('click', closeSwitchModal);
  elements.btnConfirmSwitch.addEventListener('click', confirmProfileSwitch);
  elements.btnQuickSwitchNext.addEventListener('click', quickSwitchNext);
  document.getElementById('btn-view-all-done')?.addEventListener('click', () => switchTab('done'));
}

// Initial Boot
async function init() {
  setupEvents();
  setupRippleEffects();
  initAmbientCanvas();

  await Promise.all([loadOverview(), loadTasks(), loadDoneLog(), loadProfiles()]);

  // Auto refresh every 20s
  setInterval(() => {
    loadOverview();
    loadTasks();
  }, 20000);
}

init();
