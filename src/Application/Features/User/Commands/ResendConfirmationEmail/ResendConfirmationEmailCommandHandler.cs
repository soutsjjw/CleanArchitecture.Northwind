using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.User.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger;

    public ResendConfirmationEmailCommandHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IIdentityService identityService,
        ILogger<ResendConfirmationEmailCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return await Result.FailureAsync("找不到使用者。");
        }

        if (user.EmailConfirmed)
        {
            return await Result.FailureAsync("使用者已經確認過電子郵件。");
        }

        var result = await _identityService.SendConfirmationEmailAsync(request.UserId, user.Email);

        if (!result)
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterFailedFormat, user.Email);
        }

        return result ? await Result.SuccessAsync() : await Result.FailureAsync(LoggingEvents.Account.SendConfirmLetterFailed);
    }
}
