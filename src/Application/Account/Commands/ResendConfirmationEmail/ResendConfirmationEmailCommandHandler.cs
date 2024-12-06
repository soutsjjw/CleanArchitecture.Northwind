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
        var result = await _identityService.ResendConfirmationEmail(request.Email);

        if (!result)
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterFailedFormat, request.Email);
        }

        return await Result.SuccessAsync();
    }
}
