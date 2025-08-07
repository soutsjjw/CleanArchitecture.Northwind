using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public UpdateProfileCommandHandler(IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var applicationUser = await _identityService.GetUserByIdAsync(request.UserId);

        if (applicationUser == null)
        {
            return await Result<ProfileVm>.FailureAsync("未找到使用者");
        }

        applicationUser.Profile.FullName = request.FullName;
        applicationUser.Profile.IDNo = request.IDNo;
        applicationUser.Profile.Gender = request.Gender;
        applicationUser.Profile.Title = request.Title;

        _context.UserProfiles.Update(applicationUser.Profile);

        if (await _context.SaveChangesAsync(cancellationToken) >= 1)
        {
            await _identityService.SignInAsync(applicationUser, true);

            return await Result.SuccessAsync("使用者資料已更新");
        }


        return await Result.FailureAsync("更新使用者資料失敗");
    }
}
