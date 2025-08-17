using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Domain.Enums;

public enum Status
{
    /// <summary>
    /// 註冊
    /// </summary>
    [Display(Name = "註冊")]
    Register,
    /// <summary>
    /// 啟用
    /// </summary>
    [Display(Name = "啟用")]
    Enabled,
    /// <summary>
    /// 有限
    /// </summary>
    [Display(Name = "有限")]
    Limit,
    /// <summary>
    /// 停用
    /// </summary>
    [Display(Name = "停用")]
    Disable,
}
