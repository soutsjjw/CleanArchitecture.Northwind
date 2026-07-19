using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IExcelExporter
{
    /// <summary>
    /// 把任意型別集合匯出為 xlsx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rows"></param>
    /// <param name="worksheetName"></param>
    /// <returns></returns>
    byte[] Export<T>(IEnumerable<T> rows, string worksheetName = "Sheet1");

    /// <summary>
    /// 強型別欄位定義（指定標題、欄位、格式）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rows"></param>
    /// <param name="columns"></param>
    /// <param name="worksheetName"></param>
    /// <returns></returns>
    byte[] Export<T>(IEnumerable<T> rows, IEnumerable<ExcelColumn<T>> columns, string worksheetName = "Sheet1");

    /// <summary>
    /// 以「標題→值」的字典列輸出（動態欄位）
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="worksheetName"></param>
    /// <returns></returns>
    byte[] Export(IEnumerable<IDictionary<string, object?>> rows, string worksheetName = "Sheet1");
}
