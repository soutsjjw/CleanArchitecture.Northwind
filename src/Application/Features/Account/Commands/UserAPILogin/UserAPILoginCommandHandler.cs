using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserAPILogin;

public class UserAPILoginCommandHandler : IRequestHandler<UserAPILoginCommand, Result<UserAPILoginVm>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public UserAPILoginCommandHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<Result<UserAPILoginVm>> Handle(UserAPILoginCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.UserLogin(request.UserName, request.Password, false);

        var profile = await _context.UserProfiles.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
        var token = await _identityService.GenerateTokenResponseAsync(user);

        UserAPILoginVm model = _mapper.Map<UserAPILoginVm>(token);
        model.UserName = user.UserName ?? "";
        model.FullName = profile.FullName ?? "";
        model.Title = profile.Title ?? "";

        return await Result<UserAPILoginVm>.SuccessAsync(model);
    }
}
