using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Domain.Enums;

public enum Gender
{
    /// <summary>
    /// 未知
    /// </summary>
    [Display(Name = "未知")]
    Unknow,
    /// <summary>
    /// 男性
    /// </summary>
    [Display(Name = "男性")]
    Male,
    /// <summary>
    /// 女性
    /// </summary>
    [Display(Name = "女性")]
    Female,
}
