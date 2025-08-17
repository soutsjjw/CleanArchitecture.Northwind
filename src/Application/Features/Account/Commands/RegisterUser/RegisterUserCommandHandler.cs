using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        IUserProfileRepository userProfileRepository,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _userProfileRepository = userProfileRepository;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        string userId;
        try
        {
            userId = await _identityService.UserRegisterAsync(request.Email, request.Password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LoggingEvents.Account.AccountRegistrationFailedFormat, request.Email);
            return await Result.FailureAsync(LoggingEvents.Account.AccountRegistrationFailed);
        }

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError(LoggingEvents.Account.AccountRegistrationFailedFormat, request.Email);

            return await Result.FailureAsync(LoggingEvents.Account.AccountRegistrationFailed);
        }

        try
        {
            var systemAdminUser = await _userManager.FindByNameAsync("systemadmin@localhost");

            var profile = new ApplicationUserProfile
            {
                UserId = userId,
                FullName = request.FullName,
                IDNo = request.IDNo,
                Title = request.Title,
                DepartmentId = request.DepartmentId,
                OfficeId = request.OfficeId,
            };

            await _userProfileRepository.AddUserProfileAsync(profile, systemAdminUser?.Id);
        }
        catch (Exception ex)
        {
            await _identityService.DeleteUserAsync(userId);

            _logger.LogError(ex, LoggingEvents.Account.UserProfileCreationFailedFormat, request.Email);
            return await Result.FailureAsync(LoggingEvents.Account.UserProfileCreationFailed);
        }

        var sendEmailResult = await _identityService.SendConfirmationEmailAsync(userId, request.Email);

        if (!sendEmailResult)
        {
            _logger.LogError(LoggingEvents.Account.SendConfirmLetterFailedFormat, request.Email);
        }

        return sendEmailResult ? await Result.SuccessAsync() : await Result.FailureAsync(LoggingEvents.Account.SendConfirmLetterFailed);
    }
}

