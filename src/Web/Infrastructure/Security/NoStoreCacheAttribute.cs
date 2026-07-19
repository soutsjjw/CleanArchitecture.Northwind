using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace CleanArchitecture.Northwind.Web.Infrastructure.Security;

/// <summary>
/// 對頁面回應加上 no-store/no-cache（適用敏感頁：登入、個資、帳戶設定…）
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoStoreCacheAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var headers = context.HttpContext.Response.Headers;

        headers[HeaderNames.CacheControl] = "no-store, no-cache, max-age=0, must-revalidate";
        headers[HeaderNames.Pragma] = "no-cache";
        headers[HeaderNames.Expires] = "0";

        base.OnResultExecuting(context);
    }
}
