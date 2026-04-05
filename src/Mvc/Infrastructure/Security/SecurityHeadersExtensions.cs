using System.Security.Cryptography;

namespace Mvc.Infrastructure.Security;

public static class SecurityHeadersExtensions
{
    private const string CspNonceItemKey = "CSP_NONCE";

    /// <summary>
    /// 掛上安全標頭（CSP + 反點擊劫持）。每請求產生 nonce 並存到 HttpContext.Items。
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        return app.Use((context, next) =>
        {
            // 產生 Nonce 並置入 Items 供 View 使用
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            context.Items[CspNonceItemKey] = nonce;

            // 在「送出前」最後一刻補上（確保例外/重導之後也會帶到）
            context.Response.OnStarting(() =>
            {
                var n = context.Items[CspNonceItemKey] as string ?? nonce;

                var scriptSrc = string.Join(' ', new[]
                {
                    "'self'",
                    $"'nonce-{n}'",
                    "https://challenges.cloudflare.com",
                    "https://cdn.jsdelivr.net",
                    "https://cdnjs.cloudflare.com",
                    "https://www.gstatic.com",
                    "https://www.google.com"
                });

                var styleSrc = string.Join(' ', new[]
                {
                    "'self'",
                    $"'nonce-{n}'",
                    "https://cdn.jsdelivr.net",
                    "https://cdnjs.cloudflare.com"
                });

                var csp =
                    "default-src 'self'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'; " +
                    "object-src 'none'; " +
                    "frame-ancestors 'none'; " +
                    "frame-src 'self' https://challenges.cloudflare.com; " +
                    $"script-src {scriptSrc}; " +
                    $"style-src {styleSrc}; " +
                    "img-src 'self' data:; " +
                    "font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                    "connect-src 'self'; " +
                    "upgrade-insecure-requests;";

                var header = context.Response.Headers;
                header["Content-Security-Policy"] = csp;
                header["X-Frame-Options"] = "DENY";
                header["X-Content-Type-Options"] = "nosniff";
                //（選配）h["Referrer-Policy"] = "strict-origin-when-cross-origin";
                //（選配）h["Permissions-Policy"] = "geolocation=()"; // 依實際需要收斂

                return Task.CompletedTask;
            });

            return next();
        });
    }

    /// <summary> 取得當前請求的 CSP Nonce（供 Razor 使用）。</summary>
    public static string? GetCspNonce(this HttpContext httpContext)
        => httpContext.Items.TryGetValue(CspNonceItemKey, out var v) ? v as string : null;
}
