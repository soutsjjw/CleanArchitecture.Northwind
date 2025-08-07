using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.VerifyTotp;

/// <summary>
/// 表示用於驗證使用者基於時間的一次性密碼 (TOTP) 的命令。
/// </summary>
/// <remarks>此指令用於驗證使用者提供的 TOTP 程式碼，通常作為雙重認證過程的一部分。</remarks>
public record VerifyTotpCommand : IRequest<Result>
{
    /// <summary>
    /// 使用者的唯一識別碼。
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// 驗證碼，基於時間的一次性密碼 (TOTP)。
    /// </summary>
    public string Code { get; set; }
}
