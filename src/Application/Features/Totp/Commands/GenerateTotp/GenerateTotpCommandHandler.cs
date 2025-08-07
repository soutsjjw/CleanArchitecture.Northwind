using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.Extensions.Logging;
using OtpNet;
using QRCoder;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;

/// <summary>
/// 處理為使用者產生基於時間的一次性密碼 (TOTP)。
/// </summary>
/// <remarks>
/// 此處理程序處理 <see cref="GenerateTotpCommand"/>，為命令中指定的使用者產生 TOTP 金鑰和二維碼。身份驗證器應用程式可以掃描二維碼來設定基於 TOTP 的雙重認證。
/// </remarks>
public class GenerateTotpCommandHandler : IRequestHandler<GenerateTotpCommand, Result<GenerateTotpVm>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IAppConfigurationSettings _appConfigurationSettings;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public GenerateTotpCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        IAppConfigurationSettings appConfigurationSettings,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _appConfigurationSettings = appConfigurationSettings;
        _logger = logger;
    }

    /// <summary>
    /// 為指定使用者產生基於時間的一次性密碼 (TOTP)。
    /// </summary>
    /// <remarks>此方法透過使用者 ID 檢索使用者並產生 TOTP 金鑰。然後，它會創建一個
    /// 二維碼用於 TOTP 設置，可供身份驗證器套用掃描。如果使用者不存在，此方法
    /// 會記錄錯誤並傳回失敗結果。</remarks>
    /// <param name="request">包含要為其產生 TOTP 的使用者 ID 的指令。</param>
    /// <param name="cancellationToken">用於監控取消請求的令牌。</param>
    /// <returns>表示非同步操作的任務。任務結果包含一個 <see cref="Result{GenerateTotpVm}"/> 對象，其中包含 TOTP 詳細信息，包括二維碼圖像和手動輸入密鑰。 </returns>
    public async Task<Result<GenerateTotpVm>> Handle(GenerateTotpCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(request.UserId);

        if (user == null)
        {
            _logger.LogError("使用者({UserId})不存在", request.UserId);

            return await Result<GenerateTotpVm>.FailureAsync($"找不到使用者");
        }

        // 產生 TOTP Secret
        var totpSecretKey = await SetTotpSecretAsync(request.UserId, cancellationToken);

        // 產生 QR Code
        var issuer = _appConfigurationSettings.SystemName;
        var totpUri = new OtpUri(OtpType.Totp, totpSecretKey, user.Email, issuer);
        var qrCodeUrl = $"https://chart.googleapis.com/chart?chs=200x200&cht=qr&chl={Uri.EscapeDataString(totpUri.ToString())}";

        var uri = new OtpUri(OtpType.Totp, totpSecretKey, user.Email, issuer).ToString();

        // 使用 QRCoder 產生 QR 圖片
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);

        var base64Qr = Convert.ToBase64String(qrCodeImage);

        var vm = new GenerateTotpVm
        {
            QrCodeImage = $"data:image/png;base64,{base64Qr}",
            ManualEntryKey = totpSecretKey
        };

        return await Result<GenerateTotpVm>.SuccessAsync(vm);
    }

    /// <summary>
    /// 為指定使用者非同步設定新的 TOTP 金鑰。
    /// </summary>
    /// <remarks>此方法為 <paramref name="userId"/> 標識的使用者產生新的 TOTP 金鑰，並更新資料庫中的使用者設定檔。如果使用者設定檔不存在，則建立新的設定檔並啟用 TOTP。 </remarks>
    /// <param name="userId">要為其設定 TOTP 金鑰的使用者的唯一識別碼。</param>
    /// <param name="cancellationToken">用於監控取消請求的令牌。</param>
    /// <returns>表示非同步操作的任務。任務結果包含 base32 格式的新 TOTP 金鑰。</returns>
    private async Task<string> SetTotpSecretAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = _context.UserProfiles
            .FirstOrDefault(up => up.UserId == userId);

        // 產生 TOTP Secret
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        if (profile == null)
        {
            profile = new ApplicationUserProfile
            {
                UserId = userId,
                IsTotpEnabled = true,
                TotpSecretKey = base32Secret,
            };
        }

        profile.TotpSecretKey = base32Secret;

        await _context.SaveChangesAsync(cancellationToken);

        return base32Secret;
    }
}
