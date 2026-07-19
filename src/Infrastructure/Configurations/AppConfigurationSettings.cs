using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Infrastructure.Configurations;

public class AppConfigurationSettings : IAppConfigurationSettings
{
    public string SystemName { get; set; }

    public string SiteUrl { get; set; }
}
