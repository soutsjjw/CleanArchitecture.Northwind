using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Models.Letter;
using CleanArchitecture.Northwind.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMailService _mailService;
    private readonly IOptions<AppConfigurationSettings> _appConfig;

    public UserRegisterCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        IMailService mailService,
        IOptions<AppConfigurationSettings> appConfig)
    {
        _context = context;
        _identityService = identityService;
        _mailService = mailService;
        _appConfig = appConfig;
    }

    public async Task<Result> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.UserRegisterAsync(request.Email, request.Password);

        var result = await SendConfirmationEmailAsync(userId, request);

        return result ? await Result.SuccessAsync() : await Result.FailureAsync("郵件寄送失敗");
    }

    private async Task<bool> SendConfirmationEmailAsync(string userId, UserRegisterCommand request)
    {
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);
        var confirmationLink = $"{_appConfig.Value.SiteUrl}/Account/ConfirmEmail?token={token}&email={request.Email}";

        // 信件內容
        var letterModel = new ConfirmationEmailLetterModel()
        {
            SystemName = _appConfig.Value.SystemName,
            SiteUrl = _appConfig.Value.SiteUrl,
            UserName = request.Email,
            ConfirmationLink = confirmationLink
        };

        // 取得範本
        var html = await _mailService.GetMailContentAsync(letterModel, "ConfirmationEmailLetter");

        return await _mailService.SendAsync(new MailRequest
        {
            To = request.Email,
            Subject = $"歡迎加入 {_appConfig.Value.SystemName}！請驗證你的電子郵件地址",
            Body = html,
        });
    }
}

