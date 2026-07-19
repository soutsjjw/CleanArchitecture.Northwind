using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.EnableTotp;

/// <summary>
/// 啟用基於時間的一次性密碼 (TOTP)
/// </summary>
/// <remarks>此命令用於為指定使用者啟動啟用 TOTP 的流程。它需要用戶的唯一識別碼和驗證碼。</remarks>
public record EnableTotpCommand : IRequest<Result>
{
    /// <summary>
    /// 使用者唯一識別碼
    /// </summary>
    public string UserId { get; init; } = "";

    /// <summary>
    /// 驗證碼
    /// </summary>
    public string Code { get; init; } = "";
}
