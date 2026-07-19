using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;

/// <summary>
/// 表示為指定使用者產生基於時間的一次性密碼 (TOTP) 的命令。
/// </summary>
/// <remarks>此命令用於請求產生用於身份驗證的 TOTP。執行此指令的結果封裝在 <see cref="Result{T}"/> 物件中，該物件包含一個 <see cref="GenerateTotpVm"/>。
/// </remarks>
public record GenerateTotpCommand : IRequest<Result<GenerateTotpVm>>
{
    /// <summary>
    /// 使用者唯一識別碼
    /// </summary>
    public string UserId { get; init; }
}
