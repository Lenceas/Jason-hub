using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using AuthApi.Models;
using AuthApi.Services;
using AuthShared;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// ======== 服务注册 ========

// OpenAPI / Scalar
builder.Services.AddOpenApi();

// SqlSugar
builder.Services.AddSingleton<ISqlSugarClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("Default")
        ?? Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION")
        ?? throw new InvalidOperationException("缺少数据库连接配置：请设置 AUTH_DB_CONNECTION 环境变量或 ConnectionStrings:Default");
    return new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connStr,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true
    });
});

// JWT
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton(sp => new JwtValidator(sp.GetRequiredService<JwtService>().PublicKeyPem));

// Auth Handler (Shared middleware)
builder.Services.AddSingleton<AuthHandler>();

// Auth Service
builder.Services.AddScoped<AuthService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://lujiesheng.cn",
                "https://www.lujiesheng.cn",
                "https://monitor.lujiesheng.cn"
            )
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Rate Limiting — 使用 X-Real-IP 作为分区键（经 Nginx 反代后获取真实 IP）
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("LoginPolicy", context =>
    {
        var ip = context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// Cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

var app = builder.Build();

// ======== 中间件 ========

// Forwarded Headers — 信任 Nginx 反代头，获取真实客户端 IP
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Docker 桥接网络 + 本地回环均为可信代理
    KnownProxies = { System.Net.IPAddress.Parse("172.16.0.1"), System.Net.IPAddress.Parse("127.0.0.1") }
});

// 安全响应头
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["X-XSS-Protection"] = "1; mode=block";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    if (!app.Environment.IsDevelopment())
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    await next();
});

app.UseStaticFiles();
app.UseCors();
app.UseRateLimiter();
app.UseCookiePolicy();
app.Use(AuthHandlerMiddleware);

// ======== 端点 ========

// Scalar API 文档（需 JWT）
app.MapGet("/api/v1/docs", () => Results.Redirect("/scalar/v1")).RequireScope("auth:read");

// OpenAPI JSON
app.MapOpenApi();

// Scalar UI
app.MapScalarApiReference("scalar/v1", options =>
{
    options.WithTitle("Jason-hub Auth API")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

// 登录页 HTML
app.MapGet("/login", () =>
{
    var html = """
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
            /* 背景渐变装饰圆 */
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
                box-shadow: 0 8px 40px rgba(59,130,246,0.1);
                width: 380px; max-width: 100%;
                animation: fadeUp 0.5s ease-out;
                position: relative; z-index: 1;
            }
            h1 { color: #1E293B; font-size: 1.5rem; text-align: center; margin-bottom: 0.5rem; }
            .source-hint {
                text-align: center; font-size: 0.8rem; color: #94A3B8;
                margin-bottom: 1.5rem; min-height: 1.2em;
            }
            .form-group { margin-bottom: 1.25rem; }
            .form-group label {
                display: block; color: #475569; font-size: 0.875rem;
                margin-bottom: 0.375rem; font-weight: 500;
            }
            label .required { color: #EF4444; margin-left: 2px; }
            .input-wrap {
                position: relative; display: flex; align-items: center;
            }
            .input-wrap input {
                width: 100%; padding: 0.75rem 2.5rem 0.75rem 1rem; border: 1px solid #E2E8F0;
                border-radius: 8px; font-size: 0.9375rem; transition: border-color 0.2s, box-shadow 0.2s;
                outline: none; background: #fff;
            }
            #password { padding-right: 4.5rem; }
            .input-wrap input:focus {
                border-color: #3B82F6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1);
            }
            /* 隐藏浏览器原生密码显隐按钮 */
            input[type="password"]::-ms-reveal,
            input[type="password"]::-ms-clear { display: none; }
            input[type="password"]::-webkit-credentials-auto-fill-button,
            input[type="password"]::-webkit-textfield-decoration-container {
                display: none !important;
            }
            .input-wrap input.error {
                border-color: #EF4444; box-shadow: 0 0 0 3px rgba(239,68,68,0.1);
            }
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
            @keyframes spin { to { transform: rotate(360deg); } }
            .error-msg {
                color: #EF4444; font-size: 0.875rem; text-align: center;
                margin: 0.5rem 0 1rem 0; display: none; padding: 0.5rem 0.75rem;
                background: #FEF2F2; border-radius: 8px; border: 1px solid #FECACA;
                transition: opacity 0.3s;
            }
            .error-msg.fade { opacity: 0; }
            .lock-countdown {
                display: none; text-align: center; color: #F59E0B;
                font-size: 0.875rem; margin-top: 0.75rem; font-weight: 500;
            }
            .back-link {
                display: block; text-align: center; margin-top: 0.4rem; margin-bottom: 0.9rem;
                font-size: 0.8rem; color: #6B7280; text-decoration: none;
                transition: color 0.15s;
            }
            .back-link:hover { color: #3B82F6; }
            @media (max-width: 480px) {
                .card { padding: 1.75rem 1.25rem; }
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
            <h1>Jason-hub 统一鉴权</h1>
            <a href="https://lujiesheng.cn" class="back-link">← 返回主页</a>
            <p id="sourceHint" class="source-hint"></p>
            <div id="error" class="error-msg"></div>
            <form id="loginForm" novalidate>
                <div class="form-group">
                    <label for="username">用户名 <span class="required">*</span></label>
                    <div class="input-wrap">
                        <input type="text" id="username" name="username"
                               autocomplete="username"
                               autofocus required>
                        <button type="button" class="clear-btn" data-target="username"
                                aria-label="清空用户名" tabindex="-1">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                 stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <line x1="18" y1="6" x2="6" y2="18"/>
                                <line x1="6" y1="6" x2="18" y2="18"/>
                            </svg>
                        </button>
                    </div>
                </div>
                <div class="form-group">
                    <label for="password">密码 <span class="required">*</span></label>
                    <div class="input-wrap">
                        <input type="password" id="password" name="password"
                               autocomplete="current-password" required>
                        <button type="button" class="clear-btn" data-target="password"
                                aria-label="清空密码" tabindex="-1">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                 stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <line x1="18" y1="6" x2="6" y2="18"/>
                                <line x1="6" y1="6" x2="18" y2="18"/>
                            </svg>
                        </button>
                        <button type="button" class="pw-toggle" id="pwToggle"
                                aria-label="切换密码可见性" tabindex="-1">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                 stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
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

                // 识别来源子项目 + 安全校验 redirect URL
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
                if (sourceName) {
                    sourceHint.textContent = `来自 ${sourceName}`;
                } else {
                    sourceHint.textContent = '登录后跳转至 Portfolio 主站';
                }

                let countdownTimer = null;
                let errorTimer = null;
                let cryptoKey = null; // 缓存的 RSA 公钥

                // 已登录自动跳转（Cookie 有效则直接放行）
                if (redirectUrl) {
                    fetch('/api/v1/auth/me', { credentials: 'include' })
                        .then(r => { if (r.ok) window.location.href = redirectUrl; })
                        .catch(() => {}); // 未登录忽略
                }

                // 预加载 RSA 公钥（页面加载时异步获取）
                const loadPublicKey = async () => {
                    try {
                        const pem = await fetch('/api/v1/auth/public-key').then(r => r.text());
                        const b64 = pem.replace(/-----[^-]+-----/g, '').replace(/\s/g, '');
                        const binary = atob(b64);
                        const bytes = new Uint8Array(binary.length);
                        for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
                        cryptoKey = await crypto.subtle.importKey('spki', bytes.buffer,
                            { name: 'RSA-OAEP', hash: 'SHA-256' }, false, ['encrypt']);
                    } catch (e) { /* 本地开发无 RSA 密钥时降级为明文 */ }
                };
                loadPublicKey();

                // 密码显隐切换
                pwToggle.addEventListener('click', () => {
                    const isPw = password.type === 'password';
                    password.type = isPw ? 'text' : 'password';
                });

                // 清空按钮功能
                const clearBtns = document.querySelectorAll('.clear-btn');
                const toggleClearBtn = (input) => {
                    const btn = document.querySelector(`.clear-btn[data-target="${input.id}"]`);
                    if (!btn) return;
                    btn.classList.toggle('visible', input.value.length > 0);
                };
                clearBtns.forEach(btn => {
                    const input = document.getElementById(btn.dataset.target);
                    if (!input) return;
                    btn.addEventListener('click', () => {
                        input.value = '';
                        input.focus();
                        toggleClearBtn(input);
                    });
                    input.addEventListener('input', () => toggleClearBtn(input));
                });

                // 显示错误（含自动消失）
                const showError = (msg, type) => {
                    if (errorTimer) clearTimeout(errorTimer);
                    error.classList.remove('fade');
                    error.textContent = (type || '⚠ ') + msg;
                    error.style.display = 'block';
                    errorTimer = setTimeout(() => {
                        error.classList.add('fade');
                        setTimeout(() => { error.style.display = 'none'; error.classList.remove('fade'); }, 300);
                    }, 2000);
                };

                // 锁定倒计时
                const startLockCountdown = (seconds) => {
                    stopCountdown();
                    submitBtn.disabled = true;
                    setInputsDisabled(true);
                    lockCountdown.style.display = 'block';

                    const tick = () => {
                        const mins = Math.floor(seconds / 60);
                        const secs = seconds % 60;
                        lockCountdown.textContent = `⏳ 账户已锁定，${mins}:${String(secs).padStart(2,'0')} 后可重试`;
                        if (seconds <= 0) {
                            stopCountdown();
                            submitBtn.disabled = false;
                            setInputsDisabled(false);
                            lockCountdown.style.display = 'none';
                            error.style.display = 'none';
                            return;
                        }
                        seconds--;
                        countdownTimer = setTimeout(tick, 1000);
                    };
                    tick();
                };

                const stopCountdown = () => {
                    if (countdownTimer) { clearTimeout(countdownTimer); countdownTimer = null; }
                };

                const setInputsDisabled = (disabled) => {
                    username.disabled = disabled;
                    password.disabled = disabled;
                    if (disabled) { submitBtn.classList.add('loading'); }
                    else { submitBtn.classList.remove('loading'); }
                };

                const showFieldError = (field, msg) => {
                    field.classList.add('error');
                    showError(msg);
                };
                const clearFieldError = (field) => {
                    field.classList.remove('error');
                };
                const clearError = () => {
                    if (errorTimer) { clearTimeout(errorTimer); errorTimer = null; }
                    error.style.display = 'none';
                    username.classList.remove('error');
                    password.classList.remove('error');
                };

                // 失焦校验 — 只提示"不够长"，空的不打扰。提交时兜底空值
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
                username.addEventListener('input', () => {
                    touched.username = true;
                    clearFieldError(username);
                    if (!password.classList.contains('error')) error.style.display = 'none';
                });
                password.addEventListener('input', () => {
                    touched.password = true;
                    clearFieldError(password);
                    if (!username.classList.contains('error')) error.style.display = 'none';
                });
                username.addEventListener('blur', () => {
                    if (!touched.username) return;
                    const err = validateField(username, false); // 不校验空
                    if (err) showFieldError(username, err); else clearFieldError(username);
                });
                password.addEventListener('blur', () => {
                    if (!touched.password) return;
                    const err = validateField(password, false); // 不校验空
                    if (err) showFieldError(password, err); else clearFieldError(password);
                });

                // 登录提交
                form.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    clearError();
                    const uname = username.value.trim();
                    const pwd = password.value;
                    // 按顺序校验：先用户名，再密码，遇到第一个错误就停
                    const uErr = validateField(username);
                    if (uErr) { showFieldError(username, uErr); return; }
                    const pErr = validateField(password);
                    if (pErr) { showFieldError(password, pErr); return; }
                    submitBtn.classList.add('loading');
                    submitBtn.disabled = true;

                    // RSA 加密密码（公钥已就绪时）
                    let sendPassword = pwd;
                    let encrypted = false;
                    if (cryptoKey) {
                        try {
                            const encoded = new TextEncoder().encode(pwd);
                            const enc = await crypto.subtle.encrypt({ name: 'RSA-OAEP' }, cryptoKey, encoded);
                            sendPassword = btoa(String.fromCharCode(...new Uint8Array(enc)));
                            encrypted = true;
                        } catch (e) { /* 加密失败降级明文 */ }
                    }

                    try {
                        const res = await fetch('/api/v1/auth/login', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({
                                username: uname,
                                password: sendPassword,
                                encrypted: encrypted
                            })
                        });
                        const data = await res.json();

                        if (!res.ok) {
                            if (res.status === 429) {
                                showError('登录过于频繁，请一分钟后再试', '⏱ ');
                            } else if (data.lockedRemainingSeconds > 0) {
                                startLockCountdown(data.lockedRemainingSeconds);
                                showError('账户已锁定，请稍后重试', '🔒 ');
                            } else {
                                const prefix = data.remainingAttempts > 0 ? '' : '✕ ';
                                showError(data.message || '用户名或密码错误', prefix);
                                username.classList.add('error');
                                password.classList.add('error');
                            }
                            return;
                        }

                        // 登录成功 — 跳回来源子项目
                        const dest = redirectUrl || 'https://lujiesheng.cn';
                        // Token 已由服务端写入 HttpOnly Cookie，直接跳转无需 URL 传参
                        // 显示成功提示 500ms 再跳转
                        sourceHint.textContent = '登录成功，正在跳转…';
                        sourceHint.style.color = '#10B981';
                        setTimeout(() => {
                            window.location.href = dest;
                        }, 500);
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
    return Results.Content(html, "text/html; charset=utf-8");
});

// 公开健康检查
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "jason-auth", time = DateTime.UtcNow }));

// 深度健康检查（需 JWT）
app.MapGet("/api/v1/auth/health", (ISqlSugarClient db) =>
{
    var dbOk = false;
    try { db.Ado.GetScalar("SELECT 1"); dbOk = true; } catch { }
    return Results.Ok(new {
        status = "healthy",
        service = "jason-auth",
        database = dbOk,
        time = DateTime.UtcNow
    });
}).RequireScope("auth:read");

// 用户登录
app.MapPost("/api/v1/auth/login", async (LoginRequest request, AuthService auth, JwtService jwt, HttpContext context) =>
{
    // 获取真实客户端 IP（经 Nginx X-Real-IP 头）
    var remoteIp = context.Request.Headers["X-Real-IP"].FirstOrDefault()
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";
    var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";

    // 前端 RSA 加密密码 → 解密还原明文
    var password = request.Password;
    if (request.Encrypted && !string.IsNullOrEmpty(password))
    {
        try
        {
            var cipherBytes = Convert.FromBase64String(password);
            password = jwt.Decrypt(cipherBytes);
        }
        catch { /* 解密失败，用原密码继续（可能是非加密请求） */ }
    }

    // 服务端输入长度校验（防直接调 API 绕过前端校验）
    if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Trim().Length < 5)
        return Results.Json(new { message = "用户名或密码错误" }, statusCode: 401);
    if (string.IsNullOrEmpty(password) || password.Length < 8)
        return Results.Json(new { message = "用户名或密码错误" }, statusCode: 401);

    var result = await auth.Login(new LoginRequest(request.Username, password), remoteIp, userAgent);

    if (result == null || string.IsNullOrEmpty(result.AccessToken))
    {
        var response = new Dictionary<string, object> { ["message"] = "用户名或密码错误" };
        if (result != null)
        {
            if (result.LockedRemainingSeconds > 0)
                response["message"] = $"账户已锁定，请 {result.LockedRemainingSeconds / 60} 分钟后重试";
            else if (result.RemainingAttempts > 0 && result.RemainingAttempts < 10)
                response["message"] = $"用户名或密码错误，还剩 {result.RemainingAttempts} 次尝试";
            response["remainingAttempts"] = result.RemainingAttempts;
            response["lockedRemainingSeconds"] = result.LockedRemainingSeconds;
        }
        return Results.Json(response, statusCode: 401);
    }

    // 写 HttpOnly Cookie（跨子域共享）
    context.Response.Cookies.Append("jwt", result.AccessToken, new CookieOptions
    {
        Domain = ".lujiesheng.cn",
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        MaxAge = TimeSpan.FromSeconds(result.ExpiresIn),
        Path = "/"
    });

    return Results.Ok(result);
}).RequireRateLimiting("LoginPolicy");

// 服务间调用认证
app.MapPost("/api/v1/auth/token", async (TokenRequest request, AuthService auth) =>
{
    var result = await auth.AuthenticateService(request);
    return result == null
        ? Results.Json(new { message = "Client ID 或 Secret 错误" }, statusCode: 401)
        : Results.Ok(result);
});

// 刷新 Token
app.MapPost("/api/v1/auth/refresh", async (RefreshRequest request, AuthService auth) =>
{
    var result = await auth.RefreshToken(request.RefreshToken);
    return result == null
        ? Results.Json(new { message = "刷新令牌无效或已过期" }, statusCode: 401)
        : Results.Ok(result);
});

// 公钥端点
app.MapGet("/api/v1/auth/public-key", (JwtService jwt) =>
    Results.Content(jwt.PublicKeyPem, "text/plain; charset=utf-8"));

// 获取当前登录用户信息
app.MapGet("/api/v1/auth/me", (HttpContext context) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated != true)
        return Results.Json(new { message = "未登录" }, statusCode: 401);

    return Results.Json(new
    {
        userId = user.FindFirstValue("sub") ?? "",
        username = user.FindFirstValue("username") ?? "",
        role = user.FindFirstValue("role") ?? "",
        scopes = user.FindFirstValue("scopes") ?? "",
        type = user.FindFirstValue("type") ?? "user",
        loginTime = DateTime.UtcNow
    });
});

// 退出登录 — 清除 Cookie，可选重定向（限 *.lujiesheng.cn）
app.MapGet("/logout", (HttpContext context) =>
{
    context.Response.Cookies.Append("jwt", "", new CookieOptions
    {
        Domain = ".lujiesheng.cn",
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UnixEpoch,
        Path = "/"
    });

    var redirect = context.Request.Query["redirect"].FirstOrDefault();
    if (!string.IsNullOrEmpty(redirect))
    {
        // 只允许跳转到 *.lujiesheng.cn，防 Open Redirect
        try
        {
            var uri = new Uri(redirect, UriKind.Absolute);
            var host = uri.Host;
            if (host == "lujiesheng.cn" || host.EndsWith(".lujiesheng.cn"))
                return Results.Redirect(redirect);
        }
        catch { }
        return Results.Redirect("https://lujiesheng.cn");
    }

    return Results.Json(new { message = "已退出登录" });
});

app.Run();

// ======== 中间件 ========
static async Task AuthHandlerMiddleware(HttpContext context, RequestDelegate next)
{
    var path = context.Request.Path.Value ?? "";
    if (path is "/healthz" or "/login" or "/logout" or "/api/v1/auth/public-key" or "/api/v1/auth/login" or "/api/v1/auth/token")
    {
        await next(context);
        return;
    }

    var handler = context.RequestServices.GetRequiredService<AuthHandler>();
    await handler.InvokeAsync(context, next);
}
