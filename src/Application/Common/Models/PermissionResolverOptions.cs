namespace CleanArchitecture.Northwind.Application.Common.Models;

public sealed class PermissionResolverOptions
{
    /// <summary>快取秒數（預設 60）。<=0 表示關閉快取、每次直讀 DB。</summary>
    public int CacheSeconds { get; set; } = 60;
    /// <summary>權限 ClaimType（預設 "permission"）。</summary>
    public string ClaimType { get; set; } = PermissionConstants.ClaimType;
}
