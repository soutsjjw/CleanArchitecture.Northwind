namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;

/// <summary>
/// 表示用於產生基於時間的一次性密碼 (TOTP) 的視圖模型。
/// </summary>
/// <remarks>此類提供在身份驗證器應用程式中顯示二維碼和手動輸入 TOTP 設定金鑰所需的資訊。</remarks>
public class GenerateTotpVm
{
    /// <summary>
    /// 取得或設定二維碼圖像作為 base64 編碼的字串。
    /// </summary>
    public string QrCodeImage { get; set; }

    /// <summary>
    /// 取得或設定用於手動輸入操作的鍵值。
    /// </summary>
    public string ManualEntryKey { get; set; }

    /// <summary>
    /// 備援驗證碼
    /// </summary>
    public string RecoveryCodes { get; set; }
}
