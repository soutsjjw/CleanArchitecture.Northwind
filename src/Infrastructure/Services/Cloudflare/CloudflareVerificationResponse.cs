namespace CleanArchitecture.Northwind.Infrastructure.Services.Cloudflare;

/// <summary>
/// Cloudflare 驗證請求回應
/// </summary>
public class CloudflareVerificationResponse
{
    // https://developers.cloudflare.com/turnstile/get-started/server-side-validation/

    public bool success { get; set; }
    public string challenge_ts { get; set; }
    public string hostname { get; set; }
    public List<string> error_codes { get; set; }
}
