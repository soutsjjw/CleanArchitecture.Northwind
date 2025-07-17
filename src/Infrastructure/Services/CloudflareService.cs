using System.Net.Http.Json;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

/// <summary>
/// 提供與 Cloudflare 服務互動的方法
/// </summary>
public class CloudflareService : ICloudflareService
{
    private readonly CloudflareOptions _options;

    public CloudflareService(IOptions<CloudflareOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 異步驗證令牌的有效性
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> VerifyTurnstileAsync(string token)
    {
        var secret = _options.SecretKey;
        var http = new HttpClient();

        var values = new Dictionary<string, string>
        {
            { "secret", secret },
            { "response", token }
        };

        var response = await http.PostAsync(_options.SiteVerify, new FormUrlEncodedContent(values));
        var result = await response.Content.ReadFromJsonAsync<CloudflareVerificationResponse>();
        
        return result?.success ?? false;
    }
}
