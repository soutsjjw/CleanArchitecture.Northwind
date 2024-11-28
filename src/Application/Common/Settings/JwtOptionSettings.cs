namespace CleanArchitecture.Northwind.Application.Common.Settings;

public class JwtOptionSettings
{
    public string Key { get; set; }

    public string RefreshKey { get; set; }

    public string Issuer { get; set; }

    public string Audience { get; set; }

    public int ExpiresInMinutes { get; set; }
}
