namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

/// <summary>
/// 提供與 Cloudflare 服務互動的方法
/// </summary>
public interface ICloudflareService
{
    /// <summary>
    /// 異步驗證令牌的有效性
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> VerifyTurnstileAsync(string token);
}
