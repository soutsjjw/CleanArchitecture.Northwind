using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public UserRegisterCommandHandler(IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.UserRegisterAsync(request.Email, request.Password);

        var result = await _identityService.SendConfirmationEmailAsync(userId, request.Email);

        return result ? await Result.SuccessAsync() : await Result.FailureAsync("郵件寄送失敗");
    }
}

