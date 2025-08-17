using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;

public class UserDetailQueryHandler : IRequestHandler<UserDetailQuery, Result<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDetailQueryHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<UserDetailDto>> Handle(UserDetailQuery request, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByIdAsync(request.UserId) == null)
        {
            return await Result<UserDetailDto>.FailureAsync("使用者不存在");
        }

        var userData = await (
            from user in _context.Users
            join profile in _context.UserProfiles on user.Id equals profile.UserId

            join departments in _context.Departments on profile.DepartmentId equals departments.DepartmentId

            join offices in _context.Offices on new { profile.DepartmentId, profile.OfficeId } equals new { offices.DepartmentId, offices.OfficeId }

            where user.Id.Equals(request.UserId)
            select new UserDetailDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = profile.FullName ?? "",
                IDNo = profile.IDNo ?? "",
                Title = profile.Title ?? "",
                DepartmentId = profile.DepartmentId,
                DepartmentName = departments.DeptName,
                OfficeId = profile.OfficeId,
                OfficeName = offices.OfficeName,
                Status = profile.Status,
                LockoutEnd = user.LockoutEnd,
                EmailConfirmed = user.EmailConfirmed,
            }
        ).SingleOrDefaultAsync(cancellationToken);

        if (userData == null)
            return await Result<UserDetailDto>.FailureAsync("使用者不存在");

        return await Result<UserDetailDto>.SuccessAsync(userData);
    }
}
