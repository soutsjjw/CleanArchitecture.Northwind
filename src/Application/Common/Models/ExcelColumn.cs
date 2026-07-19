namespace CleanArchitecture.Northwind.Application.Common.Models;

/// <summary>
/// 強型別欄位定義：Header=欄位標題；Selector=欄位值；Format=ClosedXML 格式字串（如 "yyyy-mm-dd hh:mm:ss"）
/// </summary>
public sealed record ExcelColumn<T>(string Header, Func<T, object?> Selector, string? Format = null);
