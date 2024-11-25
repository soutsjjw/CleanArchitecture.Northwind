using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class SMTPMailService : IMailService
{
    public MailSettings _mailSettings { get; }
    public ILogger<SMTPMailService> _logger { get; }

    public SMTPMailService(IOptions<MailSettings> mailSettings, ILogger<SMTPMailService> logger)
    {
        _mailSettings = mailSettings.Value;
        _logger = logger;
    }

    public async Task SendAsync(MailRequest request)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(request.From ?? _mailSettings.From);
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = request.Body;
            email.Body = builder.ToMessageBody();
            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, false);
                smtp.Authenticate(_mailSettings.UserName, _mailSettings.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}
