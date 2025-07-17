namespace CleanArchitecture.Northwind.Infrastructure.Configurations;

/// <summary>
/// Cloudflare 服務互動的設定選項
/// </summary>
public class CloudflareOptions
{
    public string SiteKey { get; set; }
    public string SecretKey { get; set; }
    public string SiteVerify { get; set; }
}
