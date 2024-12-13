using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, Result<UserLoginVm>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public UserLoginCommandHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<Result<UserLoginVm>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.UserLogin(request.UserName, request.Password, true);

        UserLoginVm model = new UserLoginVm
        {
            UserName = user.UserName ?? "",
            FullName = user.Profile?.FullName ?? "",
            IDNo = user.Profile?.IDNo ?? "",
            Gender = user.Profile?.Gender.ToString() ?? nameof(Domain.Enums.Gender.Unknow),
            Title = user.Profile?.Title ?? "",
            Status = user.Profile?.Status.ToString() ?? nameof(Domain.Enums.Status.Disable),
            SignInResult = result,
        };

        return await Result<UserLoginVm>.SuccessAsync(model);
    }
}
