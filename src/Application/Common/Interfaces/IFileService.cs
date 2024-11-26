namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IFileService
{
    Task<string> GetContentAsync(string path);
}
