// src/Mvc/wwwroot/js/idle-timeout.js
(function () {
  'use strict';

  // ====== 可調參數 ======
  const IDLE_LIMIT_MS = 30 * 60 * 1000;   // 30 分鐘
  const CHECK_TICK_MS = 1000;             // 每秒更新倒數
  const ACTIVE_WINDOW_MS = 60 * 1000;     // 近 1 分鐘內有操作才視為需要 keep-alive

  // ====== Key（跨分頁同步）======
  const STORAGE_LAST_ACTIVITY = 'app:last-activity-utc';
  const STORAGE_FORCE_LOGOUT  = 'app:force-logout-utc';

  // ====== DOM ======
  const badge = document.getElementById('idle-timer-badge');
  const text  = document.getElementById('idle-timer-text');
  if (!badge || !text) return; // 沒載入 Partial（未登入）則不啟動

  const csrfMeta = document.querySelector('meta[name="csrf-token"]');
  const csrf = csrfMeta && csrfMeta.content ? csrfMeta.content : '';

  // 僅在登入頁面載入：此 Partial 只在已登入才被 _Layout 引用
  badge.style.display = 'inline-flex';

  // ====== 狀態 ======
  let lastActivityUtc = parseInt(localStorage.getItem(STORAGE_LAST_ACTIVITY) || Date.now().toString(), 10);
  let lastPingUtc = 0;
  let loggingOut = false;

  // ====== 工具 ======
  const pad2 = n => (n < 10 ? '0' + n : '' + n);
  function fmt(ms) {
    if (ms < 0) ms = 0;
    const s = Math.floor(ms / 1000);
    const mm = Math.floor(s / 60);
    const ss = s % 60;
    return `${pad2(mm)}:${pad2(ss)}`;
  }

  function markActivity() {
    lastActivityUtc = Date.now();
    localStorage.setItem(STORAGE_LAST_ACTIVITY, String(lastActivityUtc));
  }

  function doLogout() {
    if (loggingOut) return;
    loggingOut = true;
    localStorage.setItem(STORAGE_FORCE_LOGOUT, String(Date.now())); // 通知其它分頁
    fetch('/account/logout', {
      method: 'POST',
      headers: { 'X-CSRF-TOKEN': csrf }
    }).catch(() => {}).finally(() => {
      window.location.href = '/Account/Login';
    });
  }

  // ====== 綁定事件：任何操作都算活動 ======
  ['click','keydown','mousemove','scroll','touchstart','wheel','focus'].forEach(evt => {
    window.addEventListener(evt, markActivity, { passive: true });
  });

  // 跨分頁同步（另一分頁有操作或登出時同步）
  window.addEventListener('storage', (e) => {
    if (e.key === STORAGE_LAST_ACTIVITY && e.newValue) {
      const v = parseInt(e.newValue, 10);
      if (!Number.isNaN(v)) lastActivityUtc = v;
    }
    if (e.key === STORAGE_FORCE_LOGOUT && e.newValue) {
      doLogout();
    }
  });

  // 初始化為「現在有活動」
  markActivity();

  // ====== 主迴圈：每秒更新倒數，定期 keep-alive ======
  setInterval(() => {
    const now = Date.now();
    const remaining = IDLE_LIMIT_MS - (now - lastActivityUtc);

    if (remaining <= 0) {
      text.textContent = '00:00';
      doLogout();
      return;
    }

    text.textContent = fmt(remaining);
  }, CHECK_TICK_MS);
})();
