using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        string userId;
        try
        {
            userId = await _identityService.UserRegisterAsync(request.Email, request.Password);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, LoggingEvents.Account.AccountRegistrationFailedFormat, request.Email);
            return await Result.FailureAsync(LoggingEvents.Account.AccountRegistrationFailed);
        }

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError(LoggingEvents.Account.AccountRegistrationFailedFormat, request.Email);

            return await Result.FailureAsync(LoggingEvents.Account.AccountRegistrationFailed);
        }

        var profile = new ApplicationUserProfile
        {
            UserId = userId,
            FullName = request.FullName,
            IDNo = request.IDNo,
            Title = request.Title,
            Department = request.Department,
            Office = request.Office,
        };

        _context.UserProfiles.Add(profile);

        await _context.SaveChangesAsync(cancellationToken);

        var sendEmailResult = await _identityService.SendConfirmationEmailAsync(userId, request.Email);

        if (!sendEmailResult)
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterFailedFormat, request.Email);
        }

        return sendEmailResult ? await Result.SuccessAsync() : await Result.FailureAsync(LoggingEvents.Account.SendConfirmLetterFailed);
    }
}

