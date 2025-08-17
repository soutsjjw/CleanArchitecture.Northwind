namespace CleanArchitecture.Northwind.Application.Common.Settings;

public class DataProtectionSettings
{
    public string Purpose { get; set; }

    private string _sqlPassPhrase;

    public string SQLPassPhrase
    {
        get
        {
            // 取得環境變數中的 SQLPassPhrase，如果不存在則使用預設值
            var envValue = Environment.GetEnvironmentVariable(_sqlPassPhrase);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
            return _sqlPassPhrase;
        }
        set
        {
            _sqlPassPhrase = value;
        }
    }
}
