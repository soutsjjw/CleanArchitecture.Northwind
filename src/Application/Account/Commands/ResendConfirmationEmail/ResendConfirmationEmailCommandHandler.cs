using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Account.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger;

    public ResendConfirmationEmailCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        ILogger<ResendConfirmationEmailCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdAsync(request.Email);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterUseNonExistentEmailFormat, request.Email);

            // 回傳成功是為了避免被猜出使用者帳號
            return await Result.SuccessAsync();
        }

        var sendEmailResult = await _identityService.SendConfirmationEmailAsync(userId, request.Email, request.ConfirmationLink);

        if (!sendEmailResult)
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterFailedFormat, request.Email);
        }

        return sendEmailResult ? await Result.SuccessAsync() : await Result.FailureAsync(LoggingEvents.Account.SendConfirmLetterFailed);
    }
}
