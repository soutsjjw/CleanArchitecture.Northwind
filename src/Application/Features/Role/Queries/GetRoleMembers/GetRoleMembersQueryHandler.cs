using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetRoleMembers;

public class GetRoleMembersQueryHandler : IRequestHandler<GetRoleMembersQuery, Result<List<MembersDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<GetRoleMembersQueryHandler> _logger;

    public GetRoleMembersQueryHandler(IApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<GetRoleMembersQueryHandler> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result<List<MembersDto>>> Handle(GetRoleMembersQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        if (role is null)
        {
            _logger.LogError($"找不到角色 ID: {request.RoleId}");
            return await Result<List<MembersDto>>.FailureAsync("找不到角色資料");
        }

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
