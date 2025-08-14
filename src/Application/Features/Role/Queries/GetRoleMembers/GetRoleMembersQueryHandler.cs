using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetRoleMembers;

public class GetRoleMembersQueryHandler : IRequestHandler<GetRoleMembersQuery, Result<List<MembersDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRoleMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MembersDto>>> Handle(GetRoleMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await (
            from user in _context.Users
            join profile in _context.UserProfiles on user.Id equals profile.UserId

            join departments in _context.Departments on profile.DepartmentId equals departments.DepartmentId

            join offices in _context.Offices on new { profile.DepartmentId, profile.OfficeId } equals new { offices.DepartmentId, offices.OfficeId }

            where user.UserRoles.Any(ur => ur.RoleId == request.RoleId)
            select new MembersDto
            {
                UserId = user.Id,
                DisplayName = profile.FullName ?? "",
                DepartmentName = departments.DeptName,
                OfficeName = offices.OfficeName
            }
        ).ToListAsync(cancellationToken);

        return Result<List<MembersDto>>.Success(members ?? new List<MembersDto>());
    }
}
