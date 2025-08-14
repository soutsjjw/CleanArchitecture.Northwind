using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Mvc.Filters;

public sealed class AntiforgeryRedirectFilter : IAsyncAuthorizationFilter, IOrderedFilter
{
    private readonly IAntiforgery _antiforgery;
    private readonly ITempDataDictionaryFactory _tempDataFactory;
    private readonly ILogger<AntiforgeryRedirectFilter> _logger;
    public int Order => int.MinValue + 100; // 儘量早一點執行

    public AntiforgeryRedirectFilter(
        IAntiforgery antiforgery,
        ITempDataDictionaryFactory tempDataFactory,
        ILogger<AntiforgeryRedirectFilter> logger)
    {
        _antiforgery = antiforgery;
        _tempDataFactory = tempDataFactory;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var req = context.HttpContext.Request;

        // 只驗 POST/非安全方法；GET/HEAD/OPTIONS/TRACE 略過
        if (HttpMethods.IsGet(req.Method) || HttpMethods.IsHead(req.Method) ||
            HttpMethods.IsOptions(req.Method) || HttpMethods.IsTrace(req.Method))
            return;

        try
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException ex)
        {
            _logger.LogWarning(ex, "CSRF 失敗，改以重新載入本頁方式處理。Path={Path}", req.Path);

            // 寫入一次性訊息並立即保存
            var temp = _tempDataFactory.GetTempData(context.HttpContext);
            temp["AntiforgeryExpired"] = "頁面已過期，已重新載入以取得最新安全權杖，請再送出一次。";
            temp.Save(); // 確保本次回應把 TempData cookie 寫出去

            // 303 轉同一路徑（清掉查詢字串也可以保留，依你需求）
            var redirectUrl = UriHelper.BuildRelative(req.PathBase, req.Path, QueryString.Empty);
            context.HttpContext.Response.StatusCode = StatusCodes.Status303SeeOther;
            context.Result = new LocalRedirectResult(redirectUrl, false);
        }
    }
}
