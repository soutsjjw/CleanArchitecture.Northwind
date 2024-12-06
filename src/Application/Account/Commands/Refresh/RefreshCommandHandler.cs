using CleanArchitecture.Northwind.Application.Common.Exceptions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshVm>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public RefreshCommandHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<Result<RefreshVm>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new UnauthorizedException("無效的客戶端令牌");
        }

        var result = await _identityService.RefreshByAPI(request.RefreshToken);

        var model = _mapper.Map<RefreshVm>(result);

        return await Result<RefreshVm>.SuccessAsync(model);
    }
}
