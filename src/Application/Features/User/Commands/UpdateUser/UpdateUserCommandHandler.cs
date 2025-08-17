using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Features.User.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserProfileRepository userProfileRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        var userProfile = await _userProfileRepository.GetByIdAsync(request.UserId);
        if (user == null || userProfile == null)
        {
            return await Result.FailureAsync("找不到使用者。");
        }

        // 更新欄位
        user.EmailConfirmed = request.EmailConfirmed;
        user.LockoutEnd = request.LockoutEnd;

        userProfile.FullName = request.FullName;
        userProfile.IDNo = request.IDNo;
        userProfile.Title = request.Title;
        userProfile.DepartmentId = request.DepartmentId;
        userProfile.OfficeId = request.OfficeId;
        userProfile.Status = request.Status;

        await _userProfileRepository.UpdateUserProfileAsync(userProfile, _currentUserService.UserId);

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return await Result.SuccessAsync("使用者更新成功");
        }
        else
        {
            var errors = result.Errors.Select(e => e.Description);
            return await Result.FailureAsync(errors, 500);
        }
    }
}
