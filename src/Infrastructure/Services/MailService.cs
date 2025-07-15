using System.Reflection;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class MailService : IMailService
{
    private readonly IFileService _fileService;
    public MailSettings _mailSettings { get; }
    public ILogger<MailService> _logger { get; }

    public MailService(IFileService fileService, IOptions<MailSettings> mailSettings, ILogger<MailService> logger)
    {
        _fileService = fileService;
        _mailSettings = mailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(MailRequest request)
    {
        var email = BuildEmailMessage(request);

        try
        {
            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.None);
                await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return false;
    }

    private MimeMessage BuildEmailMessage(MailRequest request)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(request.From ?? _mailSettings.From);
        email.To.Add(MailboxAddress.Parse(request.To));
        email.Subject = request.Subject;

        var builder = new BodyBuilder
        {
            HtmlBody = request.Body
        };
        email.Body = builder.ToMessageBody();

        return email;
    }

    public async Task<string> GetMailContentAsync<T>(T model, string templatePath) where T : class
    {
        if (model == null)
        {
            Guard.Against.Null(model);
        }

        templatePath = $"Common/MailTemplate/{templatePath}.html";
        var html = await _fileService.GetContentAsync(templatePath);

        foreach (PropertyInfo prop in model.GetType().GetProperties())
        {
            try
            {
                html = html.Replace("{" + prop.Name + "}", GetPropertyString(prop, model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        return html;
    }

    private static string GetPropertyString(PropertyInfo property, object instance)
    {
        var propertyValue = property.GetValue(instance, null);
        if (propertyValue != null)
        {
            return propertyValue.ToString() ?? "";
        }

        return string.Empty;
    }
}
