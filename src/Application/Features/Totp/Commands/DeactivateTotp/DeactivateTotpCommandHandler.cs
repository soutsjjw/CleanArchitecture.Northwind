using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.DeactivateTotp;

public class DeactivateTotpCommandHandler : IRequestHandler<DeactivateTotpCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public DeactivateTotpCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(DeactivateTotpCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(request.UserId);

        if (user?.Profile == null)
        {
            return await Result.FailureAsync("使用者不存在或資料不完整");
        }

        // 清除 TOTP 相關資料
        user.Profile.IsTotpEnabled = false;
        user.Profile.TotpSecretKey = null;
        user.Profile.TotpRecoveryCodes = null;

        await _context.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync();
    }
}
