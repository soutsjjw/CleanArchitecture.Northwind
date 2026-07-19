using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Domain.Enums;

public enum ActionType
{
    /// <summary>
    /// 檢視
    /// </summary>
    [Display(Name = "檢視")]
    View,

    /// <summary>
    /// 更新
    /// </summary>
    [Display(Name = "更新")]
    Update,
}
