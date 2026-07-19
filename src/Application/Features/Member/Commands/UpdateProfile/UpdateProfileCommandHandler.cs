using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUserProfileRepository _userProfileRepository;

    public UpdateProfileCommandHandler(IApplicationDbContext context,
        IIdentityService identityService,
        IUserProfileRepository userProfileRepository)
    {
        _context = context;
        _identityService = identityService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var applicationUser = await _identityService.GetUserByIdAsync(request.UserId);
        var applicationUserProfile = await _userProfileRepository.GetByIdAsync(request.UserId);

        if (applicationUser == null || applicationUserProfile == null)
        {
            return await Result<ProfileVm>.FailureAsync("未找到使用者");
        }

        applicationUserProfile.FullName = request.FullName;
        applicationUserProfile.IDNo = request.IDNo;
        applicationUserProfile.Gender = request.Gender;
        applicationUserProfile.Title = request.Title;

        if (await _userProfileRepository.UpdateUserProfileAsync(applicationUserProfile, applicationUser.Id) >= 1)
        {
            await _identityService.SignInAsync(applicationUser, true);

            return await Result.SuccessAsync("使用者資料已更新");
        }


        return await Result.FailureAsync("更新使用者資料失敗");
    }
}
