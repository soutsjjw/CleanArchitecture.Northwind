using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Domain.Enums;

[Flags]
public enum DataScope
{
    None = 0,

    /// <summary>
    /// 自己
    /// </summary>
    [Display(Name = "自己")]
    Self = 1,

    /// <summary>
    /// 單位
    /// </summary>
    [Display(Name = "單位")]
    Office = 2,

    /// <summary>
    /// 部門
    /// </summary>
    [Display(Name = "部門")]
    Department = 4,

    /// <summary>
    /// 全部
    /// </summary>
    [Display(Name = "全部")]
    All = 8,

    /// <summary>
    /// 系統
    /// </summary>
    [Display(Name = "系統")]
    System = 16
}
