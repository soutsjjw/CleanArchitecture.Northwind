using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class FileService : IFileService
{
    public async Task<string> GetContentAsync(string path)
    {
        var filePath = ConvertToAbsolutePath(path);

        if (File.Exists(filePath))
        {
            var sr = new StreamReader(filePath);
            var content = await sr.ReadToEndAsync();
            sr.Close();

            return content;
        }

        return string.Empty;
    }

    private string ConvertToAbsolutePath(string relativePath)
    {
        // 獲取應用程式的基底目錄
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        // 結合基底目錄和相對路徑來獲取絕對路徑
        string absolutePath = Path.Combine(basePath, relativePath);

        // 獲取絕對路徑的標準化形式
        return Path.GetFullPath(absolutePath);
    }
}
