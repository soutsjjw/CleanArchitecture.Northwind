using System.ComponentModel.DataAnnotations;

namespace Mvc.ViewModels;

public class UserEditViewModel
{
    public string Id { get; set; }

    [Display(Name = "帳號")]
    public string UserName { get; set; }

    [Display(Name = "電子郵件")]
    [EmailAddress]
    public string Email { get; set; }

    [Display(Name = "姓名")]
    public string FullName { get; set; }

    [Display(Name = "身分證號")]
    public string IDNo { get; set; }

    [Display(Name = "職稱")]
    public string Title { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; }

    public int OfficeId { get; set; }

    public string OfficeName { get; set; }

    [Display(Name = "手機號碼")]
    [Phone]
    public string? PhoneNumber { get; set; }

    [Display(Name = "是否啟用鎖定機制")]
    public bool LockoutEnabled { get; set; }

    [Display(Name = "電子郵件已驗證")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "手機已驗證")]
    public bool PhoneNumberConfirmed { get; set; }
}
