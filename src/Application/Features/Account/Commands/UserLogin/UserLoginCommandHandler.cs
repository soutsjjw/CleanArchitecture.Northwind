using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, Result<UserLoginDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IIdentitySettings _identitySettings;

    public UserLoginCommandHandler(IIdentityService identityService,
        IIdentitySettings identitySettings)
    {
        _identityService = identityService;
        _identitySettings = identitySettings;
    }

    public async Task<Result<UserLoginDto>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.UserLogin(request.UserName, request.Password, true);

        if (user == null || result == null || !result.Succeeded)
        {
            return await Result<UserLoginDto>.FailureAsync(LoggingEvents.Account.InvalidLoginAttempt);
        }

        var isPasswordExpiration = user.LastPasswordChangedDate.HasValue &&
                                   user.LastPasswordChangedDate.Value.AddDays(_identitySettings.PasswordExpirationDays) < DateTime.Now;

        if (isPasswordExpiration)
        {
            await _identityService.SignOutAsync();
        }

        var model = new UserLoginDto
        {
            UserName = user.UserName ?? "",
            FullName = user.Profile?.FullName ?? "",
            IDNo = user.Profile?.IDNo ?? "",
            Gender = user.Profile?.Gender.ToString() ?? nameof(Domain.Enums.Gender.Unknow),
            Title = user.Profile?.Title ?? "",
            Status = user.Profile?.Status.ToString() ?? nameof(Domain.Enums.Status.Disable),
            IsPasswordExpiration = isPasswordExpiration,
            User = user,
            SignInResult = result,
        };

        return await Result<UserLoginDto>.SuccessAsync(model);
    }
}
