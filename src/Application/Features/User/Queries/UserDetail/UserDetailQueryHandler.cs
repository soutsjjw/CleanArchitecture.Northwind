using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;

public class UserDetailQueryHandler : IRequestHandler<UserDetailQuery, Result<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ICurrentUserService _currentUserService;

    public UserDetailQueryHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserProfileRepository userProfileRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserDetailDto>> Handle(UserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return await Result<UserDetailDto>.FailureAsync("使用者不存在");

        var userProfile = await _userProfileRepository.GetByIdAsync(request.UserId);
        if (userProfile == null)
            return await Result<UserDetailDto>.FailureAsync("使用者個人資料不存在");

        var department = await _context.Departments
            .Where(d => d.DepartmentId == userProfile.DepartmentId)
            .SingleOrDefaultAsync(cancellationToken);

        var office = await _context.Offices
            .Where(o => o.DepartmentId == userProfile.DepartmentId && o.OfficeId == userProfile.OfficeId)
            .SingleOrDefaultAsync(cancellationToken);

        if (request.UserId != _currentUserService.UserId)
        {
            // 新增個資瀏覽紀錄
            var log = new PersonalDataAccessLog
            {
                ViewerUserId = _currentUserService.UserId,
                TargetUserId = request.UserId,
                Action = nameof(ActionType.View),
                Accessed = DateTime.UtcNow,
                Description = "瀏覽使用者個資"
            };
            _context.PersonalDataAccessLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var userData = new UserDetailDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            FullName = userProfile.FullName ?? "",
            IDNo = userProfile.IDNo ?? "",
            Title = userProfile.Title ?? "",
            DepartmentId = userProfile.DepartmentId,
            DepartmentName = department?.DeptName,
            OfficeId = userProfile.OfficeId,
            OfficeName = office?.OfficeName,
            Status = userProfile.Status,
            LockoutEnd = user.LockoutEnd,
            EmailConfirmed = user.EmailConfirmed,
        };

        return await Result<UserDetailDto>.SuccessAsync(userData);
    }
}
