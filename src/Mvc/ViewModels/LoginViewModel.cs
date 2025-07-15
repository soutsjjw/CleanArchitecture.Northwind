using System.ComponentModel.DataAnnotations;

namespace Mvc.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "請輸入您的電子郵件地址")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    public string Email { get; set; }

    [Required(ErrorMessage = "請輸入您的密碼")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
