(function () {
  'use strict';

  const IDLE_LIMIT_MS = 30 * 60 * 1000; // 30分鐘
  const CHECK_TICK_MS = 1000;           // 每秒更新倒數
  const STORAGE_LAST_ACTIVITY = 'app:last-activity-utc';
  const STORAGE_FORCE_LOGOUT = 'app:force-logout-utc';

  const badge = document.getElementById('idle-timer-badge');
  const text = document.getElementById('idle-timer-text');
  if (!badge || !text) return;

  const csrfMeta = document.querySelector('meta[name="csrf-token"]');
  const csrf = csrfMeta && csrfMeta.content ? csrfMeta.content : '';

  badge.style.display = 'inline-flex';

  // 取得本分頁的最後活動時間
  let lastActivityUtc = parseInt(localStorage.getItem(STORAGE_LAST_ACTIVITY) || Date.now().toString(), 10);
  let loggingOut = false;

  // 活動偵測
  ['click', 'keydown', 'mousemove', 'scroll', 'touchstart', 'wheel', 'focus'].forEach(evt => {
    window.addEventListener(evt, () => {
      lastActivityUtc = Date.now();
      localStorage.setItem(STORAGE_LAST_ACTIVITY, String(lastActivityUtc));
    }, { passive: true });
  });

  // 跨分頁同步活動
  window.addEventListener('storage', (e) => {
    if (e.key === STORAGE_LAST_ACTIVITY && e.newValue) {
      const v = parseInt(e.newValue, 10);
      if (!Number.isNaN(v)) lastActivityUtc = v;
    }
    if (e.key === STORAGE_FORCE_LOGOUT && e.newValue) {
      doLogout();
    }
  });

  function fmt(ms) {
    if (ms < 0) ms = 0;
    const s = Math.floor(ms / 1000);
    const mm = Math.floor(s / 60);
    const ss = s % 60;
    return `${mm.toString().padStart(2, '0')}:${ss.toString().padStart(2, '0')}`;
  }

  function doLogout() {
    if (loggingOut) return;
    loggingOut = true;
    localStorage.setItem(STORAGE_FORCE_LOGOUT, String(Date.now())); // 通知其它分頁
    fetch('/account/logout', {
      method: 'POST',
      headers: { 'X-CSRF-TOKEN': csrf }
    }).catch(() => { }).finally(() => {
      window.location.href = '/Account/Login';
    });
  }

  // 初始化
  localStorage.setItem(STORAGE_LAST_ACTIVITY, String(Date.now()));

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