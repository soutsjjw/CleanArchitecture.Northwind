using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAccount;

public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, Result<List<AccountVm>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserProfileRepository _userProfileRepository;

    public GetAccountQueryHandler(IApplicationDbContext context,
        IUserProfileRepository userProfileRepository)
    {
        _context = context;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<List<AccountVm>>> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var list = await _userProfileRepository.GetUserProfilesAsync(new Common.DTOs.AccountConditionDto
        {
            DepartmentId = request.DepartmentId,
            OfficeId = request.OfficeId
        });

        return Result<List<AccountVm>>.Success(list.Select(x => new AccountVm
        {
            UserId = x.UserId,
            DisplayName = x.FullName ?? ""
        }).ToList());
    }
}
