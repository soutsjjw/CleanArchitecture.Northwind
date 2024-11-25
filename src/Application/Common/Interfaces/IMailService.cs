using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IMailService
{
    Task SendAsync(MailRequest request);
}
