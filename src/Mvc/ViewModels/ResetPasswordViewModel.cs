using System.ComponentModel.DataAnnotations;

namespace Mvc.ViewModels;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "請輸入電子郵件地址")]
    [EmailAddress(ErrorMessage = "電子郵件地址格式不正確")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "請輸入驗證碼")]
    public required string ResetCode { get; init; }

    [Required(ErrorMessage = "請輸入新密碼")]
    public required string NewPassword { get; init; }

    [Required(ErrorMessage = "請再次輸入新密碼")]
    [Compare("NewPassword", ErrorMessage = "兩次輸入的密碼不一致")]
    public required string ConfirmPassword { get; init; }
}
