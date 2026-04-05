using Microsoft.AspNetCore.Mvc.Filters;

namespace Mvc.Infrastructure.Security;

/// <summary>
/// 可公開快取
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PublicCacheAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var headers = context.HttpContext.Response.Headers;
        headers["Cache-Control"] = "public, max-age=600"; // 10 分鐘示例
        headers.Remove("Pragma");
        headers.Remove("Expires");
    }
}
