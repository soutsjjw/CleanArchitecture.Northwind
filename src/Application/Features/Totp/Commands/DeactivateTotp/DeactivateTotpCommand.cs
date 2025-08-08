using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.DeactivateTotp;

public record DeactivateTotpCommand : IRequest<Result>
{
    /// <summary>
    /// 使用者的唯一識別碼。
    /// </summary>
    public string UserId { get; init; } = "";
}
