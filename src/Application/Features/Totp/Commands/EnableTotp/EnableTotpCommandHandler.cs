using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using OtpNet;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.EnableTotp;

/// <summary>
/// 處理為使用者啟用基於時間的一次性密碼 (TOTP)。
/// </summary>
/// <remarks>此處理程序處理 <see cref="EnableTotpCommand"/>，透過驗證提供的 TOTP 代碼為使用者啟用 TOTP。如果驗證成功，
/// 則為使用者的個人資料啟用 TOTP。</remarks>
public class EnableTotpCommandHandler : IRequestHandler<EnableTotpCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public EnableTotpCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(EnableTotpCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(request.UserId);

        if (user?.Profile?.TotpSecretKey == null)
        {
            throw new InvalidOperationException("TotpSecretKey cannot be null.");
        }

        var totp = new OtpNet.Totp(Base32Encoding.ToBytes(user.Profile.TotpSecretKey));

        if (totp.VerifyTotp(request.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
        {
            await EnableTotpAsync(user.Profile, cancellationToken);

            return await Result.SuccessAsync("TOTP 已啟用。");
        }

        return await Result.FailureAsync("無效的 TOTP 驗證碼。請檢查您的輸入並重試。");
    }

    /// <summary>
    /// 為指定的使用者設定檔啟用基於時間的一次性密碼 (TOTP) 身份驗證。
    /// </summary>
    /// <remarks>此方法將 <see cref="ApplicationUserProfile.IsTotpEnabled"/> 屬性設為 <see langword="true"/>，並將變更儲存到資料庫。在呼叫此方法之前，請確保 <paramref name="profile"/> 不為 <see langword="null"/>。</remarks>
    /// <param name="profile">若要啟用 TOTP 身份驗證的使用者設定檔。</param>
    /// <param name="cancellationToken">用於監控取消請求的令牌。</param>
    /// <returns></returns>
    private async Task EnableTotpAsync(ApplicationUserProfile profile, CancellationToken cancellationToken)
    {
        profile.IsTotpEnabled = true;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
