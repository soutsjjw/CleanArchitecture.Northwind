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
        var query = from user in _context.Users.AsNoTracking()
                    join profile in _context.UserProfiles on user.Id equals profile.UserId

                    join departments in _context.Departments on profile.DepartmentId equals departments.DepartmentId
                    into departments_jointable
                    from departments in departments_jointable.DefaultIfEmpty()

                    join offices in _context.Offices on new { profile.DepartmentId, profile.OfficeId } equals new { offices.DepartmentId, offices.OfficeId }
                    into offices_jointable
                    from offices in offices_jointable.DefaultIfEmpty()

                    select new UserItems
                    {
                        UserId = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FullName = profile.FullName ?? "",
                        Title = profile.Title ?? "",
                        DepartmentId = profile.DepartmentId,
                        DepartmentName = departments.DeptName,
                        OfficeId = profile.OfficeId,
                        OfficeName = offices.OfficeName,
                        Status = profile.Status,
                    };

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

        if (request.Status.HasValue)
        {
            query = query.Where(u => u.Status == request.Status.Value);
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
