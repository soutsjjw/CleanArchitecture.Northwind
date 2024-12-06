using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdAsync(request.Email) ?? "";

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning(LoggingEvents.Account.SendForgotPasswordLetterUseNonExistentEmailFormat, request.Email);

            // 回傳成功是為了避免被猜出使用者帳號
            return await Result.SuccessAsync();
        }

        var sendEmailResult = await _identityService.SendForgotPasswordEmailAsync(userId, request.Email, request.ResetCodeLink);

        if (!sendEmailResult)
        {
            _logger.LogError(LoggingEvents.Account.SendForgotPasswordLetterFailedFormat, request.Email);
        }

        return sendEmailResult ? await Result.SuccessAsync() : await Result.FailureAsync(LoggingEvents.Account.SendForgotPasswordLetterFailed);
    }
}
