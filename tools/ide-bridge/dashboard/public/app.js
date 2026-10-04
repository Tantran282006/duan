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

  // Footer
  footerLastUpdated: document.getElementById('footer-last-updated')
};

// Tab Switching
function switchTab(tabId) {
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

// Fetch Overview Data
async function loadOverview() {
  try {
    const res = await fetch('/api/overview');
    if (!res.ok) throw new Error('Không thể tải tổng quan');
    const data = await res.json();
    state.overview = data;

    // Update Hero
    elements.heroPercentText.textContent = `${data.completionPercent}%`;
    elements.heroProgressRing.parentElement.style.background = `conic-gradient(var(--accent) 0% ${data.completionPercent}%, #202b38 ${data.completionPercent}% 100%)`;
    elements.chipMilestone.textContent = `Mốc: ${data.currentMilestone}`;

    // Update Metrics
    elements.metricInProgress.textContent = data.metrics.inProgress;
    elements.metricPending.textContent = data.metrics.pending;
    elements.metricBlocked.textContent = data.metrics.blocked;
    elements.metricDone.textContent = data.recentSessions.length;

    // Recent Activity
    renderRecentActivity(data.recentSessions);

    elements.footerLastUpdated.textContent = new Date().toLocaleTimeString('vi-VN');
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
        <div class="task-card" data-id="${task.id}">
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
            <button class="btn-primary btn-switch-profile" data-id="${p.id}" data-name="${escapeHtml(p.name)}" style="width:100%;justify-content:center;">
              Chuyển sang Profile này
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
      alert(`Đã đổi profile thành công!\n\nLưu ý: Bạn có ${data.inProgressWarning.count} task đang in_progress. Hãy dùng agent mới claim lại task bằng worker ID mới.`);
    }
  } catch (err) {
    alert('Lỗi: ' + err.message);
  } finally {
    elements.btnConfirmSwitch.disabled = false;
    elements.btnConfirmSwitch.textContent = 'Xác nhận chuyển';
  }
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
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape') {
      closeDrawer();
      closeSwitchModal();
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
      alert('Vui lòng nhập số % từ 0 đến 100');
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
  await Promise.all([loadOverview(), loadTasks(), loadDoneLog(), loadProfiles()]);

  // Auto refresh every 20s
  setInterval(() => {
    loadOverview();
    loadTasks();
  }, 20000);
}

init();
