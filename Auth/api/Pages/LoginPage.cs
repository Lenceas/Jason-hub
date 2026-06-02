namespace AuthApi.Pages;

/// <summary>登录页面</summary>
public static class LoginPage
{
    public static string GetHtml()
    {
        return """
        <!DOCTYPE html>
        <html lang="zh-CN">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>登录 · Jason-hub 统一鉴权</title>
            <link rel="icon" type="image/png" href="/images/favicon.png">
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body {
                    font-family: -apple-system, "Microsoft YaHei", sans-serif;
                    background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 30%, #ede9fe 60%, #f8fafc 100%);
                    background-size: 300% 300%;
                    animation: bgShift 24s ease-in-out infinite;
                    min-height: 100vh; display: flex; align-items: center; justify-content: center;
                    padding: 1rem; overflow: hidden;
                }
                @keyframes bgShift {
                    0%,100% { background-position: 0% 50%; }
                    50% { background-position: 100% 50%; }
                }
                .glow { position: fixed; border-radius: 50%; pointer-events: none; z-index: 0; }
                .glow-1 {
                    width: 520px; height: 520px;
                    background: radial-gradient(circle, rgba(59,130,246,0.18) 0%, transparent 70%);
                    top: -180px; right: -120px;
                    animation: drift1 14s ease-in-out infinite;
                }
                .glow-2 {
                    width: 380px; height: 380px;
                    background: radial-gradient(circle, rgba(139,92,246,0.15) 0%, transparent 70%);
                    bottom: -100px; left: -80px;
                    animation: drift2 18s ease-in-out infinite;
                }
                .glow-3 {
                    width: 260px; height: 260px;
                    background: radial-gradient(circle, rgba(6,182,212,0.12) 0%, transparent 70%);
                    top: 45%; left: -60px;
                    animation: drift3 16s ease-in-out infinite;
                }
                @keyframes drift1 {
                    0%,100% { transform: translate(0,0) scale(1); }
                    33% { transform: translate(-30px,25px) scale(1.04); }
                    66% { transform: translate(15px,-15px) scale(0.96); }
                }
                @keyframes drift2 {
                    0%,100% { transform: translate(0,0) scale(1); }
                    33% { transform: translate(25px,-18px) scale(1.06); }
                    66% { transform: translate(-18px,12px) scale(0.94); }
                }
                @keyframes drift3 {
                    0%,100% { transform: translate(0,0) scale(1); }
                    50% { transform: translate(20px,28px) scale(1.08); }
                }
                @keyframes fadeUp {
                    from { opacity: 0; transform: translateY(24px); }
                    to   { opacity: 1; transform: translateY(0); }
                }
                .card {
                    background: rgba(255,255,255,0.88);
                    backdrop-filter: blur(16px);
                    padding: 2.5rem; border-radius: 16px;
                    border: 1px solid rgba(255,255,255,0.7);
                    box-shadow: 0 8px 40px rgba(59,130,246,0.1);
                    width: 380px; max-width: 100%;
                    animation: fadeUp 0.5s ease-out;
                    position: relative; z-index: 1;
                }
                h1 { color: #1E293B; font-size: 1.5rem; text-align: center; margin-bottom: 0.5rem; }
                .brand-icon { text-align: center; margin-bottom: 0.75rem; }
                .brand-icon svg { width: 40px; height: 40px; color: #3B82F6; }
                .source-hint {
                    text-align: center; font-size: 0.8rem; color: #94A3B8;
                    margin-bottom: 0.5rem; min-height: 1.2em;
                }
                .form-group { margin-bottom: 1.25rem; }
                .form-group label {
                    display: block; color: #475569; font-size: 0.875rem;
                    margin-bottom: 0.375rem; font-weight: 500;
                }
                label .required { color: #EF4444; margin-left: 2px; }
                .input-wrap { position: relative; display: flex; align-items: center; }
                .input-wrap input {
                    width: 100%; padding: 0.75rem 2.5rem 0.75rem 1rem; border: 1px solid #E2E8F0;
                    border-radius: 8px; font-size: 0.9375rem; transition: border-color 0.2s, box-shadow 0.2s;
                    outline: none; background: #fff;
                }
                input:-webkit-autofill {
                    -webkit-box-shadow: 0 0 0 30px #fff inset;
                    -webkit-text-fill-color: #1E293B;
                }
                #password { padding-right: 4.5rem; }
                .input-wrap input:focus {
                    border-color: #3B82F6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1);
                }
                input[type="password"]::-ms-reveal,
                input[type="password"]::-ms-clear { display: none; }
                input[type="password"]::-webkit-credentials-auto-fill-button,
                input[type="password"]::-webkit-textfield-decoration-container {
                    display: none !important;
                }
                .input-wrap input.error { border-color: #EF4444; box-shadow: 0 0 0 3px rgba(239,68,68,0.1); }
                .pw-toggle {
                    position: absolute; right: 12px; top: 50%; transform: translateY(-50%);
                    background: none; border: none; cursor: pointer; padding: 4px;
                    color: #94A3B8; display: flex; align-items: center; justify-content: center;
                    transition: color 0.2s; width: auto;
                }
                .pw-toggle:hover { color: #475569; background: none; }
                .pw-toggle svg { width: 20px; height: 20px; display: block; }
                .clear-btn {
                    position: absolute; right: 12px; top: 50%; transform: translateY(-50%);
                    background: none; border: none; cursor: pointer; padding: 2px;
                    color: #94A3B8; display: none; align-items: center; justify-content: center;
                    border-radius: 50%; width: 20px; height: 20px;
                    transition: background 0.15s, color 0.15s;
                }
                .clear-btn:hover { background: #E2E8F0; color: #475569; }
                .clear-btn.visible { display: flex; }
                .clear-btn svg { width: 16px; height: 16px; display: block; }
                #password ~ .clear-btn { right: 40px; }
                button[type="submit"] {
                    width: 100%; padding: 0.75rem; background: #3B82F6; color: white;
                    border: none; border-radius: 8px; font-size: 1rem; font-weight: 500;
                    cursor: pointer; transition: background 0.2s, transform 0.15s;
                    display: flex; align-items: center; justify-content: center; gap: 0.5rem;
                    position: relative;
                }
                button[type="submit"]:hover:not(:disabled) { background: #2563EB; }
                button[type="submit"]:active:not(:disabled) { transform: scale(0.98); }
                button[type="submit"]:disabled { background: #93C5FD; cursor: not-allowed; }
                .spinner {
                    width: 18px; height: 18px; border: 2px solid rgba(255,255,255,0.3);
                    border-top-color: #fff; border-radius: 50%; animation: spin 0.6s linear infinite;
                    display: none;
                }
                button[type="submit"].loading .spinner { display: inline-block; }
                button[type="submit"].loading .btn-text { display: none; }
                button[type="submit"].success { background: #10B981; }
                button[type="submit"].success .btn-text { display: inline; }
                @keyframes spin { to { transform: rotate(360deg); } }
                .error-msg {
                    color: #EF4444; font-size: 0.875rem; text-align: center;
                    margin: 0.5rem 0 1rem 0; padding: 0.5rem 0.75rem;
                    background: #FEF2F2; border-radius: 8px; border: 1px solid #FECACA;
                    opacity: 0; visibility: hidden; transition: opacity 0.3s, visibility 0.3s;
                    max-height: 0; overflow: hidden; padding: 0 0.75rem;
                }
                .error-msg.show { opacity: 1; visibility: visible; max-height: 80px; padding: 0.5rem 0.75rem; margin: 0.5rem 0 1rem 0; }
                .error-msg.fade { opacity: 0; visibility: hidden; }
                .lock-countdown {
                    display: none; text-align: center; color: #F59E0B;
                    font-size: 0.875rem; margin-top: 0.75rem; font-weight: 500;
                }
                .back-link {
                    display: block; text-align: center; margin-top: 0.4rem; margin-bottom: 0.5rem;
                    font-size: 0.8rem; color: #6B7280; text-decoration: none;
                    transition: color 0.15s;
                }
                .back-link:hover { color: #3B82F6; }
                @media (max-width: 640px) {
                    .glow-1, .glow-2, .glow-3 { display: none; }
                    .card { padding: 1.5rem 1rem; }
                    h1 { font-size: 1.25rem; }
                    input { padding: 0.65rem 0.875rem; font-size: 0.875rem; }
                    button[type="submit"] { padding: 0.65rem; font-size: 0.9375rem; }
                    .form-group { margin-bottom: 1rem; }
                }
            </style>
        </head>
        <body>
            <div class="glow glow-1"></div>
            <div class="glow glow-2"></div>
            <div class="glow glow-3"></div>
            <div class="card">
                <div class="brand-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                        <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                        <path d="M7 11V7a5 5 0 0110 0v4"/>
                        <circle cx="12" cy="16" r="1.5" fill="currentColor" stroke="none"/>
                        <path d="M12 16v-2" stroke-width="2"/>
                    </svg>
                </div>
                <h1>Jason-hub 统一鉴权</h1>
                <a href="https://lujiesheng.cn" class="back-link">← 返回主页</a>
                <p id="sourceHint" class="source-hint"></p>
                <div id="error" class="error-msg"></div>
                <form id="loginForm" novalidate>
                    <div class="form-group">
                        <label for="username">用户名 <span class="required">*</span></label>
                        <div class="input-wrap">
                            <input type="text" id="username" name="username" autocomplete="username" autofocus required>
                            <button type="button" class="clear-btn" data-target="username" aria-label="清空用户名" tabindex="-1">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                                </svg>
                            </button>
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="password">密码 <span class="required">*</span></label>
                        <div class="input-wrap">
                            <input type="password" id="password" name="password" autocomplete="current-password" required>
                            <button type="button" class="clear-btn" data-target="password" aria-label="清空密码" tabindex="-1">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                                </svg>
                            </button>
                            <button type="button" class="pw-toggle" id="pwToggle" aria-label="切换密码可见性" tabindex="-1">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                                    <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/>
                                    <circle cx="12" cy="12" r="3"/>
                                    <path id="eyeSlash" d="M2 2l20 20" style="display:none"/>
                                </svg>
                            </button>
                        </div>
                    </div>
                    <button type="submit" id="submitBtn">
                        <span class="spinner"></span>
                        <span class="btn-text">登录</span>
                    </button>
                    <div id="lockCountdown" class="lock-countdown"></div>
                </form>
            </div>
            <script>
                (() => {
                    const form = document.getElementById('loginForm');
                    const error = document.getElementById('error');
                    const submitBtn = document.getElementById('submitBtn');
                    const username = document.getElementById('username');
                    const password = document.getElementById('password');
                    const pwToggle = document.getElementById('pwToggle');
                    const lockCountdown = document.getElementById('lockCountdown');
                    const sourceHint = document.getElementById('sourceHint');

                    const params = new URLSearchParams(window.location.search);
                    const rawRedirect = params.get('redirect') || '';
                    let redirectUrl = '';
                    const APP_MAP = {
                        'lujiesheng.cn': 'Portfolio 主站',
                        'monitor.lujiesheng.cn': 'Monitor 监控面板',
                    };
                    let sourceName = '';
                    if (rawRedirect) {
                        try {
                            const u = new URL(rawRedirect);
                            if (u.hostname === 'lujiesheng.cn' || u.hostname.endsWith('.lujiesheng.cn')) {
                                redirectUrl = rawRedirect;
                                sourceName = APP_MAP[u.hostname] || u.hostname;
                            }
                        } catch {}
                    }
                    sourceHint.textContent = sourceName ? `来自 ${sourceName}` : '登录后跳转至 Portfolio 主站';

                    let countdownTimer = null, errorTimer = null, cryptoKey = null;

                    if (redirectUrl) {
                        fetch('/api/v1/auth/me', { credentials: 'include' })
                            .then(r => { if (r.ok) window.location.href = redirectUrl; })
                            .catch(() => {});
                    }

                    const loadPublicKey = async () => {
                        try {
                            const pem = await fetch('/api/v1/auth/public-key').then(r => r.text());
                            const b64 = pem.replace(/-----[^-]+-----/g, '').replace(/\s/g, '');
                            const binary = atob(b64);
                            const bytes = new Uint8Array(binary.length);
                            for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
                            cryptoKey = await crypto.subtle.importKey('spki', bytes.buffer,
                                { name: 'RSA-OAEP', hash: 'SHA-256' }, false, ['encrypt']);
                        } catch (e) {}
                    };
                    loadPublicKey();

                    pwToggle.addEventListener('click', () => {
                        password.type = password.type === 'password' ? 'text' : 'password';
                    });

                    const clearBtns = document.querySelectorAll('.clear-btn');
                    const toggleClearBtn = (input) => {
                        const btn = document.querySelector(`.clear-btn[data-target="${input.id}"]`);
                        if (btn) btn.classList.toggle('visible', input.value.length > 0);
                    };
                    clearBtns.forEach(btn => {
                        const input = document.getElementById(btn.dataset.target);
                        if (!input) return;
                        btn.addEventListener('click', () => { input.value = ''; input.focus(); toggleClearBtn(input); });
                        input.addEventListener('input', () => toggleClearBtn(input));
                    });

                    const showError = (msg, type) => {
                        if (errorTimer) clearTimeout(errorTimer);
                        error.classList.remove('fade');
                        error.textContent = (type || '⚠ ') + msg;
                        error.style.display = ''; error.classList.add('show');
                        errorTimer = setTimeout(() => {
                            error.classList.add('fade');
                            setTimeout(() => { error.classList.remove('show'); error.classList.remove('fade'); }, 300);
                        }, 2000);
                    };

                    const startLockCountdown = (seconds) => {
                        const tick = () => {
                            const mins = Math.floor(seconds / 60);
                            const secs = seconds % 60;
                            lockCountdown.textContent = `⏳ 账户已锁定，${mins}:${String(secs).padStart(2,'0')} 后可重试`;
                            if (seconds <= 0) { stopCountdown(); submitBtn.disabled = false; setInputsDisabled(false); lockCountdown.style.display = 'none'; error.classList.remove('show'); return; }
                            seconds--;
                            countdownTimer = setTimeout(tick, 1000);
                        };
                        stopCountdown();
                        submitBtn.disabled = true;
                        setInputsDisabled(true);
                        lockCountdown.style.display = 'block';
                        tick();
                    };
                    const stopCountdown = () => { if (countdownTimer) { clearTimeout(countdownTimer); countdownTimer = null; } };
                    const setInputsDisabled = (d) => { username.disabled = d; password.disabled = d; if (d) submitBtn.classList.add('loading'); else submitBtn.classList.remove('loading'); };
                    const showFieldError = (f, m) => { f.classList.add('error'); showError(m); };
                    const clearFieldError = (f) => { f.classList.remove('error'); };
                    const clearError = () => { if (errorTimer) { clearTimeout(errorTimer); errorTimer = null; } error.classList.remove('show'); username.classList.remove('error'); password.classList.remove('error'); };

                    const touched = { username: false, password: false };
                    const validateField = (field, checkEmpty = true) => {
                        const val = field.value.trim();
                        const isUser = field.id === 'username';
                        const minLen = isUser ? 5 : 8;
                        const label = isUser ? '用户名' : '密码';
                        if (checkEmpty && !val) return `${label}不能为空`;
                        if (val && val.length < minLen) return `${label}至少 ${minLen} 个字符`;
                        return '';
                    };
                    username.addEventListener('input', () => { touched.username = true; clearFieldError(username); if (!password.classList.contains('error')) error.classList.remove('show'); });
                    password.addEventListener('input', () => { touched.password = true; clearFieldError(password); if (!username.classList.contains('error')) error.classList.remove('show'); });
                    username.addEventListener('blur', () => { if (!touched.username) return; const e = validateField(username, false); if (e) showFieldError(username, e); else clearFieldError(username); });
                    password.addEventListener('blur', () => { if (!touched.password) return; const e = validateField(password, false); if (e) showFieldError(password, e); else clearFieldError(password); });

                    form.addEventListener('submit', async (e) => {
                        e.preventDefault();
                        clearError();
                        const uname = username.value.trim();
                        const pwd = password.value;
                        const uErr = validateField(username);
                        if (uErr) { showFieldError(username, uErr); return; }
                        const pErr = validateField(password);
                        if (pErr) { showFieldError(password, pErr); return; }
                        submitBtn.classList.add('loading');
                        submitBtn.disabled = true;

                        let sendPassword = pwd, encrypted = false;
                        if (cryptoKey) {
                            try {
                                const encoded = new TextEncoder().encode(pwd);
                                const enc = await crypto.subtle.encrypt({ name: 'RSA-OAEP' }, cryptoKey, encoded);
                                sendPassword = btoa(String.fromCharCode(...new Uint8Array(enc)));
                                encrypted = true;
                            } catch (e) {}
                        }

                        try {
                            const res = await fetch('/api/v1/auth/login', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({ username: uname, password: sendPassword, encrypted })
                            });
                            const data = await res.json();

                            if (!res.ok) {
                                if (res.status === 429) { showError('登录过于频繁，请一分钟后再试', '⏱ '); }
                                else if (data.lockedRemainingSeconds > 0) { startLockCountdown(data.lockedRemainingSeconds); showError('账户已锁定，请稍后重试', '🔒 '); }
                                else { showError(data.message || '用户名或密码错误', data.remainingAttempts > 0 ? '' : '✕ '); username.classList.add('error'); password.classList.add('error'); }
                                return;
                            }

                            const dest = redirectUrl || 'https://lujiesheng.cn';
                            sourceHint.textContent = '登录成功，正在跳转…';
                            sourceHint.style.color = '#10B981';
                            submitBtn.classList.remove('loading');
                            submitBtn.classList.add('success');
                            submitBtn.querySelector('.btn-text').textContent = '✓ 登录成功';
                            setTimeout(() => { window.location.href = dest; }, 500);
                        } catch (err) {
                            showError('网络错误，请检查网络连接后重试', '⚠ ');
                        } finally {
                            if (!lockCountdown.style.display || lockCountdown.style.display === 'none') {
                                submitBtn.classList.remove('loading');
                                submitBtn.disabled = false;
                            }
                        }
                    });
                })();
            </script>
        </body>
        </html>
        """;
    }
}
