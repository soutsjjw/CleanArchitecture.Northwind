using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CleanArchitecture.Northwind.Web.Infrastructure.Security;

[HtmlTargetElement("script", Attributes = "asp-add-nonce")]
[HtmlTargetElement("style", Attributes = "asp-add-nonce")]
public sealed class NonceTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public NonceTagHelper(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    /// <summary>啟用後會自動注入 CSP nonce。</summary>
    public bool AspAddNonce { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!AspAddNonce) return;

        var http = _httpContextAccessor.HttpContext;
        var nonce = http?.GetCspNonce();
        if (!string.IsNullOrEmpty(nonce))
        {
            output.Attributes.SetAttribute("nonce", nonce);
        }
        // 移除屬性避免輸出多餘字樣
        output.Attributes.RemoveAll("asp-add-nonce");
    }
}
