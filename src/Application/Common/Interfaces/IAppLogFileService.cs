namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IAppLogFileService
{
    /// <summary>
    /// 依日期開啟日誌檔，若不存在回傳 null。
    /// </summary>
    /// <param name="date"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Stream?> OpenAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// 用於下載的建議檔名。
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    string GetFileName(DateOnly date);

    /// <summary>
    /// 內容型別（純文字或你實際的日誌格式）。
    /// </summary>
    string ContentType { get; }
}
