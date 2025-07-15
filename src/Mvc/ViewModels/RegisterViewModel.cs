using System.ComponentModel.DataAnnotations;

namespace Mvc.ViewModels;

public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Email 是必填項目")]
    [EmailAddress(ErrorMessage = "請輸入有效的 Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "密碼是必填項目")]
    [DataType(DataType.Password)]
    [StringLength(16, ErrorMessage = "密碼長度必須在 {2} 到 {1} 個字元之間", MinimumLength = 12)]
    public string Password { get; set; }

    [Required(ErrorMessage = "請再次確認密碼")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "兩次密碼輸入不一致")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "姓名是必填項目")]
    [StringLength(10, ErrorMessage = "姓名長度不能超過 {1} 個字元")]
    public string FullName { get; set; }

    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "身分證號只能包含字母和數字")]
    public string? IDNo { get; set; }

    [Required(ErrorMessage = "請選擇職稱")]
    public string Title { get; set; }

    //[Range(typeof(bool), "true", "true", ErrorMessage = "必須同意條款與政策")]
    [Required(ErrorMessage = "必須同意條款與政策")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "必須同意條款與政策")]
    public bool AgreeToTerms { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Password))
        {
            var uniqueChars = Password.Distinct().Count();
            if (uniqueChars < 4)
            {
                yield return new ValidationResult(
                    "密碼必須包含至少 4 個唯一字符",
                    new[] { nameof(Password) });
            }
        }
    }
}
