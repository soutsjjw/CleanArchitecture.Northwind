using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Member.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(_currentUserService.UserId);
        if (user == null)
            return await Result.FailureAsync("找不到使用者");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!isPasswordValid)
        {
            _logger.LogError("使用者 {UserId} 嘗試使用無效的當前密碼。", user.Id);

            return await Result.InvalidAsync(
                new Dictionary<string, string[]>
                {
                    { nameof(request.CurrentPassword), new[] { "當前密碼不正確" } }
                },
                statusCode: 401);
        }

        if (_identityService.IsPasswordSameAsLastThree(user, request.NewPassword))
        {
            return await Result.FailureAsync("新密碼不可與前三次相同");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
        {
            user.LastPasswordChangedDate = DateTime.Now;

            // 新增密碼歷史紀錄
            _context.UserPasswordHistories.Add(new ApplicationUserPasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash!,
                ChangedAt = DateTime.Now
            });
            await _context.SaveChangesAsync(cancellationToken);

            await _identityService.RefreshSignInAsync(user, true);

            return await Result.SuccessAsync();
        }

        foreach (var err in result.Errors)
        {
            _logger.LogError($"{err.Code}: {err.Description}");
        }

        return await Result.FailureAsync("重設密碼失敗，請確認您的電子郵件、驗證碼和新密碼是否正確。");
    }
}
