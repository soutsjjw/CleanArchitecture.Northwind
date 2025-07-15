using System.ComponentModel.DataAnnotations;

namespace Mvc.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "請輸入您的電子郵件地址")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    public string Email { get; set; }
}
