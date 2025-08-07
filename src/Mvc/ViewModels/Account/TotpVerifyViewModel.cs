namespace Mvc.ViewModels.Account;

/// <summary>
/// 表示用於驗證基於時間的一次性密碼 (TOTP) 的視圖模型。
/// </summary>
/// <remarks>此視圖模型通常用於使用者需要輸入 TOTP 程式碼進行驗證的場景，例如雙重認證工作流程。</remarks>
public class TotpVerifyViewModel
{
    /// <summary>
    /// 驗證碼
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
