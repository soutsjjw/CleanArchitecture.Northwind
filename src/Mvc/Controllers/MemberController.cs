using AutoMapper;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;
using CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.EnableTotp;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;
using Mvc.ViewModels.Account;

namespace Mvc.Controllers;

public class MemberController : BaseController<MemberController>
{
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public MemberController(IUser user,
        IMapper mapper,
        ILogger<MemberController> logger)
        : base(logger)
    {
        _user = user;
        _mapper = mapper;
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

        var result = await Mediator.Send(new GenerateTotpCommand { UserId = userId });

        if (!result.Succeeded)
        {
            return View().WithError(result.Errors.ToList(), "產生 TOTP 失敗");
        }

        var viewModel = new SetupTotpViewModel
        {
            QrCodeImage = result.Data.QrCodeImage,
            ManualEntryKey = result.Data.ManualEntryKey,
        };

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
}
