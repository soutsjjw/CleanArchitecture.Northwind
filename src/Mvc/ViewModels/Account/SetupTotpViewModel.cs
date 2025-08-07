using CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;

namespace Mvc.ViewModels.Account;

/// <summary>
/// 表示用於設定基於時間的一次性密碼 (TOTP) 的視圖模型。
/// </summary>
/// <remarks>此視圖模型用於透過提供必要的資料（例如產生的程式碼）來簡化 TOTP 驗證的設定過程。</remarks>
public class SetupTotpViewModel : GenerateTotpVm
{
    /// <summary>
    /// 驗證碼
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
