using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;

public class UserDetailDto
{
    public string UserId { get; set; }

    [Display(Name = "帳號")]
    public string? UserName { get; set; }

    [Display(Name = "電子郵件")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "姓名")]
    public string FullName { get; set; }

    [Display(Name = "身分證號")]
    public string? IDNo { get; set; }

    [Display(Name = "職稱")]
    public string Title { get; set; }

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public int OfficeId { get; set; }

    public string? OfficeName { get; set; }

    public Status Status { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    [Display(Name = "電子郵件已驗證")]
    public bool EmailConfirmed { get; set; }
}
