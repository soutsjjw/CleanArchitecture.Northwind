using System.Reflection;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using ClosedXML.Excel;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public sealed class ExcelExporter : IExcelExporter
{
    public byte[] Export<T>(IEnumerable<T> rows, string worksheetName = "Sheet1")
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(worksheetName);

        var list = rows?.ToList() ?? new List<T>();
        if (list.Count == 0)
        {
            ws.Cell(1, 1).Value = "No data";
            using var msEmpty = new MemoryStream();
            wb.SaveAs(msEmpty);
            return msEmpty.ToArray();
        }

        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // 標題
        for (int i = 0; i < props.Length; i++)
            ws.Cell(1, i + 1).Value = props[i].Name;

        // 內容
        for (int r = 0; r < list.Count; r++)
        {
            var item = list[r];
            for (int c = 0; c < props.Length; c++)
            {
                var val = props[c].GetValue(item);
                ws.Cell(r + 2, c + 1).Value = ClosedXML.Excel.XLCellValue.FromObject(val);
            }
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] Export<T>(IEnumerable<T> rows, IEnumerable<ExcelColumn<T>> columns, string worksheetName = "Sheet1")
    {
        var list = rows?.ToList() ?? new List<T>();
        var cols = columns?.ToList() ?? new List<ExcelColumn<T>>();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(worksheetName);

        if (cols.Count == 0)
        {
            ws.Cell(1, 1).Value = "No columns";
            FinalizeSheet(ws, 1, 1);
            return SaveToBytes(wb);
        }

        // Header
        for (int c = 0; c < cols.Count; c++)
            ws.Cell(1, c + 1).Value = cols[c].Header;

        // Rows
        for (int r = 0; r < list.Count; r++)
        {
            var item = list[r];
            for (int c = 0; c < cols.Count; c++)
            {
                var col = cols[c];
                var val = Normalize(col.Selector(item));
                var cell = ws.Cell(r + 2, c + 1);
                cell.Value = ClosedXML.Excel.XLCellValue.FromObject(val);

                if (!string.IsNullOrWhiteSpace(col.Format))
                    cell.Style.DateFormat.Format = col.Format;
            }
        }

        FinalizeSheet(ws, 1, cols.Count);
        return SaveToBytes(wb);
    }

    public byte[] Export(IEnumerable<IDictionary<string, object?>> rows, string worksheetName = "Sheet1")
    {
        var list = rows?.ToList() ?? new List<IDictionary<string, object?>>();
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(worksheetName);

        if (list.Count == 0)
        {
            ws.Cell(1, 1).Value = "No data";
            FinalizeSheet(ws, 1, 1);
            return SaveToBytes(wb);
        }

        // 以第一列的 Key 決定欄順序；後續列若有不存在的 Key 會追加在尾端
        var headers = new List<string>(list[0].Keys);
        foreach (var row in list)
            foreach (var k in row.Keys)
                if (!headers.Contains(k)) headers.Add(k);

        // Header
        for (int c = 0; c < headers.Count; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        // Rows
        for (int r = 0; r < list.Count; r++)
        {
            var row = list[r];
            for (int c = 0; c < headers.Count; c++)
            {
                row.TryGetValue(headers[c], out var val);
                ws.Cell(r + 2, c + 1).Value = ClosedXML.Excel.XLCellValue.FromObject(Normalize(val) ?? string.Empty);
            }
        }

        FinalizeSheet(ws, 1, headers.Count);
        return SaveToBytes(wb);
    }

    // ---- helpers ----
    private static object? Normalize(object? v)
    {
        if (v is null) return null;

        // ClosedXML 對 DateOnly/TimeOnly 支援有限；轉成 DateTime/TimeSpan
        if (v is DateOnly d) return d.ToDateTime(TimeOnly.MinValue);
        if (v is TimeOnly t) return new DateTime(1, 1, 1).Add(t.ToTimeSpan());

        // Enum 直接輸出字串（避免 Excel 變數值）
        if (v is Enum) return v.ToString();

        return v;
    }

    private static void FinalizeSheet(IXLWorksheet ws, int headerRow, int colCount)
    {
        var header = ws.Range(headerRow, 1, headerRow, colCount);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.6);
        ws.SheetView.FreezeRows(headerRow);
        ws.RangeUsed().SetAutoFilter();
        ws.Columns(1, colCount).AdjustToContents();
    }

    private static byte[] SaveToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
