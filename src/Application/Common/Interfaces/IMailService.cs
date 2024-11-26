using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IMailService
{
    Task<bool> SendAsync(MailRequest request);

    Task<string> GetMailContentAsync<T>(T model, string templatePath) where T : class;
}
