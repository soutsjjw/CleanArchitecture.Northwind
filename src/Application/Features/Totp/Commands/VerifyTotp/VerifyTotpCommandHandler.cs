using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using OtpNet;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.VerifyTotp;

/// <summary>
/// 處理使用者基於時間的一次性密碼 (TOTP) 的驗證。
/// </summary>
/// <remarks>
/// 此處理程序處理 <see cref="VerifyTotpCommand"/>，以根據使用者儲存的 TOTP 金鑰驗證提供的 TOTP 代碼。如果驗證成功，則使用者登入。
/// </remarks>
public class VerifyTotpCommandHandler : IRequestHandler<VerifyTotpCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public VerifyTotpCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    /// <summary>
    /// 處理使用者基於時間的一次性密碼 (TOTP) 的驗證。
    /// </summary>
    /// <param name="request">包含使用者 ID 和待驗證 TOTP 程式碼的指令。</param>
    /// <param name="cancellationToken">用於監控取消請求的令牌。</param>
    /// <returns>表示非同步操作的任務。任務結果包含一個 <see cref="Result"/> ，指示操作是否成功。</returns>
    /// <exception cref="InvalidOperationException">如果使用者的 TOTP 金鑰為空，則拋出。</exception>
    public async Task<Result> Handle(VerifyTotpCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(request.UserId);

        if (user?.Profile?.TotpSecretKey == null)
        {
            throw new InvalidOperationException("TotpSecretKey cannot be null.");
        }

        var totp = new OtpNet.Totp(Base32Encoding.ToBytes(user.Profile.TotpSecretKey));
        if (totp.VerifyTotp(request.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
        {
            // 正式登入（簽發 Cookie）
            await _identityService.SignInAsync(user, true);
        }

        return await Result.SuccessAsync();
    }
}
