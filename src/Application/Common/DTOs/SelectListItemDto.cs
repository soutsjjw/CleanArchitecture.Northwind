namespace CleanArchitecture.Northwind.Application.Common.DTOs;

public class SelectListItemDto
{
    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 名稱
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// 是否選取
    /// </summary>
    public bool? Selected { get; set; }
}
