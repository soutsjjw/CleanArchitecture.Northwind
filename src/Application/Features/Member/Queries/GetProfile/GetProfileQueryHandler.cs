using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<ProfileVm>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetProfileQueryHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<Result<ProfileVm>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var applicationUser = await _identityService.GetUserByIdAsync(request.UserId);

        if (applicationUser == null)
        {
            return await Result<ProfileVm>.FailureAsync("未找到使用者");
        }

        ProfileVm model = _mapper.Map<ProfileVm>(applicationUser.Profile);

        return await Result<ProfileVm>.SuccessAsync(model);
    }
}
