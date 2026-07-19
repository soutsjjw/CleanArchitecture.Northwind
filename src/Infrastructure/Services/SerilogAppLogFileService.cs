using System.Globalization;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public sealed class SerilogAppLogFileService : IAppLogFileService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public SerilogAppLogFileService(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public string ContentType => "text/plain";

    public async Task<Stream?> OpenAsync(DateOnly date, CancellationToken ct = default)
    {
        var fullPath = GetConcreteFilePath(date);
        if (fullPath is null || !File.Exists(fullPath)) return null;

        // 檔案可能被 Serilog 開啟占用；用 FileShare.ReadWrite 以便讀取
        var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return await Task.FromResult<Stream?>(fs);
    }

    public string GetFileName(DateOnly date)
    {
        var fullPath = GetConcreteFilePath(date);
        return fullPath is null ? $"log-{date:yyyyMMdd}.log" : Path.GetFileName(fullPath);
    }

    // ---- helpers ----

    private string? GetConcreteFilePath(DateOnly date)
    {
        // 1) 找出 Serilog 的 File sink
        var sink = FindFileSink(_config.GetSection("Serilog:WriteTo"));
        if (sink is null) return null;

        // 2) 擴充環境變數、解析相對路徑
        var template = Environment.ExpandEnvironmentVariables(sink.Value.Path);
        var absTemplate = ToAbsolute(template);

        // 3) 根據 rollingInterval 產出日期字串
        var dateSuffix = GetDateSuffix(sink.Value.RollingInterval, date);

        // 4) 依 Serilog 慣例把 "app-.log" 的 '-' 位置替換為日期
        //    也支援非標準的 "{Date}" 標記
        var concrete = ReplaceDateToken(absTemplate, dateSuffix, sink.Value.RollingInterval);

        return concrete;
    }

    private static string ReplaceDateToken(string path, string dateSuffix, string rollingInterval)
    {
        if (string.Equals(rollingInterval, "Infinite", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(dateSuffix))
        {
            // 無輪替 -> 檔名固定
            return path;
        }

        // 標準模式：檔名中有 "-."（例如 "app-.log"），插入日期成 "app-20250817.log"
        var fileName = Path.GetFileName(path);
        var dir = Path.GetDirectoryName(path) ?? "";

        if (fileName.Contains("-."))
        {
            var replaced = fileName.Replace("-.", "-" + dateSuffix + ".");
            return Path.Combine(dir, replaced);
        }

        // 兼容：自訂 {Date} 標記
        if (fileName.Contains("{Date}", StringComparison.OrdinalIgnoreCase))
        {
            var replaced = fileName.Replace("{Date}", dateSuffix, StringComparison.OrdinalIgnoreCase);
            return Path.Combine(dir, replaced);
        }

        // 沒有任何標記：最後手段 -> 在副檔名前插入日期
        var idx = fileName.LastIndexOf(".", StringComparison.Ordinal);
        if (idx > 0)
        {
            var replaced = fileName.Insert(idx, "-" + dateSuffix);
            return Path.Combine(dir, replaced);
        }

        // 沒有副檔名
        return Path.Combine(dir, fileName + "-" + dateSuffix);
    }

    private string ToAbsolute(string path)
    {
        if (Path.IsPathRooted(path)) return path;

        // 相對於 ContentRoot
        return Path.GetFullPath(Path.Combine(_env.ContentRootPath, path));
    }

    private static string GetDateSuffix(string rollingInterval, DateOnly date)
    {
        // 參考 Serilog.Sinks.File RollingInterval
        // Infinite / Year / Month / Day / Hour / Minute
        return rollingInterval?.ToLowerInvariant() switch
        {
            "year" => date.ToString("yyyy", CultureInfo.InvariantCulture),
            "month" => date.ToString("yyyyMM", CultureInfo.InvariantCulture),
            "day" => date.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            "hour" => date.ToDateTime(TimeOnly.MinValue).ToString("yyyyMMddHH", CultureInfo.InvariantCulture),
            "minute" => date.ToDateTime(TimeOnly.MinValue).ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture),
            _ => "" // Infinite 或未知 -> 不附加日期
        };
    }

    private static (string Path, string RollingInterval)? FindFileSink(IConfigurationSection writeTo)
    {
        foreach (var sink in writeTo.GetChildren())
        {
            var name = sink["Name"];

            if (string.Equals(name, "File", StringComparison.OrdinalIgnoreCase))
            {
                var args = sink.GetSection("Args");
                var path = args["path"];
                if (string.IsNullOrWhiteSpace(path)) continue;

                var interval = args["rollingInterval"] ?? "Day";
                return (path, interval);
            }

            // 支援 Async 包裝： { Name:"Async", Args:{ }, "configure":[ { Name:"File", Args:{...} } ] }
            if (string.Equals(name, "Async", StringComparison.OrdinalIgnoreCase))
            {
                var configure = sink.GetSection("configure");
                var nested = FindFileSink(configure);
                if (nested is not null) return nested;
            }
        }

        return null;
    }
}
