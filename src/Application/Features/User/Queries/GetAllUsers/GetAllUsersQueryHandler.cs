using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<UsersDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<UsersDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UsersDto
            {
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.Profile.FullName,
                Title = u.Profile.Title
            });

        var pagedList = await PaginatedList<UsersDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        return await Result<PaginatedList<UsersDto>>.SuccessAsync(pagedList);
    }
}
