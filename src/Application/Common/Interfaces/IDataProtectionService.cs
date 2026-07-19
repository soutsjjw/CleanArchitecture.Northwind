namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IDataProtectionService
{
    string Protect(string input);
    string Unprotect(string protectedData);
}
