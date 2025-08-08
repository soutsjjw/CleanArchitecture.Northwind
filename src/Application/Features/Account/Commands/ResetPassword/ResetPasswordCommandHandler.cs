using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IIdentityService identityService)
    {
        _context = context;
        _userManager = userManager;
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByEmailAsync(request.Email);
        if (user == null)
            return await Result.FailureAsync("找不到使用者");

        var last3Hashes = user.PasswordHistories
            .OrderByDescending(h => h.ChangedAt)
            .Take(3)
            .Select(h => h.PasswordHash)
            .ToList();

        // 檢查新密碼是否與前三次相同
        foreach (var hash in last3Hashes)
        {
            if (_userManager.PasswordHasher.VerifyHashedPassword(user, hash, request.NewPassword) == PasswordVerificationResult.Success)
            {
                return await Result.FailureAsync("新密碼不可與前三次相同");
            }
        }

        if (await _identityService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword))
        {
            // 新增密碼歷史紀錄
            _context.UserPasswordHistories.Add(new ApplicationUserPasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash!,
                ChangedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);

            return await Result.SuccessAsync();
        }

        return await Result.FailureAsync("重設密碼失敗，請確認您的電子郵件、驗證碼和新密碼是否正確。");
    }
}
