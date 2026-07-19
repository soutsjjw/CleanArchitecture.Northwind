using CleanArchitecture.Northwind.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;

    public DataProtectionService(IDataProtectionProvider provider, string purpose)
    {
        _protector = provider.CreateProtector(purpose);
    }

    public string Protect(string input)
    {
        return _protector.Protect(input);
    }

    public string Unprotect(string protectedData)
    {
        try
        {
            return _protector.Unprotect(protectedData);
        }
        catch
        {
            return null;
        }
    }
}
