using AutoMapper;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;
using CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.DeactivateTotp;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.EnableTotp;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.VerifyTotp;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;
using Mvc.ViewModels.Account;

namespace Mvc.Controllers;

public class MemberController : BaseController<MemberController>
{
    private readonly IUser _user;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public MemberController(IUser user,
        IMapper mapper,
        IIdentityService identityService,
        ILogger<MemberController> logger)
        : base(logger)
    {
        _user = user;
        _mapper = mapper;
        _identityService = identityService;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var result = await Mediator.Send(new GetProfileQuery { UserId = _user.Id ?? "" });

        if (!result.Succeeded)
        {
            throw new ApplicationException("無法取得使用者資料");
        }

        var profileVm = result.Data;

        GenerateGenderOptions();

        return View(profileVm);
    }

    [HttpGet]
    public async Task<IActionResult> ProfileEdit()
    {
        var result = await Mediator.Send(new GetProfileQuery { UserId = _user.Id ?? "" });

        if (!result.Succeeded)
        {
            throw new ApplicationException("無法取得使用者資料");
        }

        var profileVm = result.Data;

        GenerateGenderOptions();

        return View(profileVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfileEdit(ProfileVm viewModel)
    {
        var model = _mapper.Map<UpdateProfileCommand>(viewModel);
        model.UserId = _user.Id ?? "";

        GenerateGenderOptions();

        var result = await Mediator.Send(model);

        if (!result.Succeeded)
        {
            return View(viewModel).WithError(result.Errors.ToList());
        }

        return View(viewModel).WithSuccess("更新成功");
    }

    [HttpGet]
    public async Task<IActionResult> SetupTotp()
    {
        var userId = _user.Id ?? string.Empty;

        var applicationUser = await _identityService.GetUserByIdAsync(userId);
        if (applicationUser == null)
        {
            return RedirectToAction("Login")
                .WithError(this, "使用者不存在或已被刪除");
        }

        if (applicationUser.Profile.IsTotpEnabled)
        {
            return RedirectToAction("Profile")
                .WithError(this, "TOTP 已經啟用");
        }

        var result = await Mediator.Send(new GenerateTotpCommand { UserId = userId });

        if (!result.Succeeded)
        {
            return View().WithError(result.Errors.ToList(), "產生 TOTP 失敗");
        }

        var viewModel = _mapper.Map<SetupTotpViewModel>(result.Data);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupTotp(SetupTotpViewModel viewModel)
    {
        var userId = _user.Id ?? string.Empty;

        var result = await Mediator.Send(new EnableTotpCommand
        {
            UserId = userId,
            Code = viewModel.Code,
        });

        if (result.Succeeded)
        {
            return RedirectToAction("Profile")
                .WithSuccess(this, "啟用 TOTP 成功");
        }

        return View(viewModel)
            .WithError(result.Errors.ToList(), "啟用 TOTP 失敗");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateTotp(string totpCode)
    {
        var userId = _user.Id ?? string.Empty;

        if (userId == null)
            return Json(new { success = false, error = "未登入" });

        var validResult = await Mediator.Send(new VerifyTotpCommand
        {
            UserId = userId,
            Code = totpCode
        });

        if (!validResult.Succeeded)
        {
            return Json(new { success = false, error = "TOTP 驗證失敗" });
        }

        var result = await Mediator.Send(new DeactivateTotpCommand { UserId = userId });

        if (result.Succeeded)
            return Json(new { success = true });
        else
            return Json(new { success = false, error = "停用 TOTP 失敗" });
    }
}
