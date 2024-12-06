using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ILogger<UserRegisterCommandHandler> _logger;

    public UserRegisterCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        ILogger<UserRegisterCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.UserRegisterAsync(request.Email, request.Password);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError(LoggingEvents.Account.AccountRegistrationFailedFormat, request.Email);

            return await Result.FailureAsync(LoggingEvents.Account.AccountRegistrationFailed);
        }

        var sendEmailResult = await _identityService.SendConfirmationEmailAsync(userId, request.Email, request.ConfirmationLink);

        if (!sendEmailResult)
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterFailedFormat, request.Email);
        }

        return sendEmailResult ? await Result.SuccessAsync() : await Result.FailureAsync(LoggingEvents.Account.SendConfirmLetterFailed);
    }
}

