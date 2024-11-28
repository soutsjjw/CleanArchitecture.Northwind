using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.UserLogin;

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
        try
        {
            var result = await _identityService.UserLoginByAPI(request.UserName, request.Password);

            var model = _mapper.Map<UserLoginVm>(result);

            return await Result<UserLoginVm>.SuccessAsync(model);
        }
        catch (ArgumentException ex)
        {
            return await Result<UserLoginVm>.FailureAsync(ex.Message, 401);
        }
    }
}
