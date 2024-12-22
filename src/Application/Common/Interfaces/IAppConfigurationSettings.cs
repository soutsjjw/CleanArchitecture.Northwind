namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IAppConfigurationSettings
{
    string SystemName { get; set; }

    string SiteUrl { get; set; }
}
