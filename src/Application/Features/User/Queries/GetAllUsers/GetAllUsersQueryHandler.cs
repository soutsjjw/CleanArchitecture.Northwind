using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<UsersDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UsersDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UserItems
            {
                UserId = u.Id,
                UserName = u.UserName ?? "",
                Email = u.Email,
                FullName = u.Profile.FullName,
                Title = u.Profile.Title,
                DepartmentId = u.Profile.DepartmentId,
                OfficeId = u.Profile.OfficeId,
            });

        if (!string.IsNullOrEmpty(request.Email))
        {
            query = query.Where(u => u.Email!.Contains(request.Email));
        }

        if (!string.IsNullOrEmpty(request.FullName))
        {
            query = query.Where(u => u.FullName!.Contains(request.FullName));
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == request.DepartmentId.Value);

            if (request.OfficeId.HasValue)
            {
                query = query.Where(u => u.OfficeId == request.OfficeId.Value);
            }
        }

        var pagedList = await PaginatedList<UserItems>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        return await Result<UsersDto>.SuccessAsync(new UsersDto
        {
            Email = request.Email,
            FullName = request.FullName,
            DepartmentId = request.DepartmentId,
            OfficeId = request.OfficeId,
            Users = pagedList
        });
    }
}
