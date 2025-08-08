using AutoMapper;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;

namespace Mvc.ViewModels.Account;

/// <summary>
/// 表示用於設定基於時間的一次性密碼 (TOTP) 的視圖模型。
/// </summary>
/// <remarks>此視圖模型用於透過提供必要的資料（例如產生的程式碼）來簡化 TOTP 驗證的設定過程。</remarks>
public class SetupTotpViewModel
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

    /// <summary>
    /// 驗證碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GenerateTotpVm, SetupTotpViewModel>();
        }
    }
}
