using AutoMapper;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.User.Commands.UpdateUser;
using CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;
using CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;
using Mvc.ViewModels;

namespace Mvc.Controllers;

[Authorize(Roles = "SystemAdmin,Administrator")]
public class UserController : BaseController<UserController>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IApplicationDbContext context,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ILogger<UserController> logger)
        : base(logger)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Search");
    }

    [HttpGet]
    public async Task<IActionResult> Search(int pageNumber = 1, int pageSize = 10)
    {
        var result = await Mediator.Send(new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        GenerateDepartmentAndOfficeOptions();

        if (result.Succeeded)
        {
            return View(result.Data);
        }

        return RedirectToAction("Index", "Home").WithError(this, "取得使用者列表失敗");
    }

    [HttpPost]
    public async Task<IActionResult> Search(UsersDto viewModel, int pageNumber = 1, int pageSize = 10)
    {
        var result = await Mediator.Send(new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Email = viewModel.Email,
            FullName = viewModel.FullName,
            DepartmentId = viewModel.DepartmentId,
            OfficeId = viewModel.OfficeId
        });

        if (result.Succeeded)
        {
            GenerateDepartmentAndOfficeOptions();

            return View("Search", result.Data);
        }
        return RedirectToAction("Index", "Home").WithError(this, "查詢使用者失敗");
    }

    [HttpGet]
    public async Task<IActionResult> DetailsModal(string id)
    {
        var result = await Mediator.Send(new UserDetailQuery
        {
            UserId = id
        });

        if (result.Succeeded)
        {
            return PartialView("_UserDetail", result.Data);
        }

        return PartialView("_NoDataPartial", "無符合資料");
    }

    [HttpGet]
    public async Task<IActionResult> EditModal(string id)
    {
        var result = await Mediator.Send(new UserDetailQuery
        {
            UserId = id
        });

        if (result.Succeeded)
        {
            GenerateDepartmentOptions();
            return PartialView("_UserEdit", result.Data);
        }

        return PartialView("_NoDataPartial", "無符合資料");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserDetailDto viewModel)
    {
        //if (!ModelState.IsValid)
        //{
        //    // 回傳編輯 Partial（HTML），前端會覆蓋 Modal 內容並顯示驗證錯誤
        //    return PartialView("_UserEdit", viewModel);
        //}

        //var user = await _userManager.FindByIdAsync(viewModel.UserId);
        //var userProfile = _context.UserProfiles.SingleOrDefault(x => x.UserId == user.Id);
        //if (user == null || userProfile == null)
        //{
        //    ModelState.AddModelError(string.Empty, "找不到使用者。");
        //    return PartialView("_UserEdit", viewModel);
        //}

        //// 更新欄位
        //user.EmailConfirmed = viewModel.EmailConfirmed;
        //user.LockoutEnd = viewModel.LockoutEnd;

        //userProfile.FullName = viewModel.FullName;
        //userProfile.IDNo = viewModel.IDNo;
        //userProfile.Title = viewModel.Title;
        //userProfile.DepartmentId = viewModel.DepartmentId;
        //userProfile.OfficeId = viewModel.OfficeId;
        //userProfile.Status = viewModel.Status;

        //var result = await _userManager.UpdateAsync(user);
        //if (!result.Succeeded)
        //{
        //    foreach (var e in result.Errors)
        //        ModelState.AddModelError(string.Empty, e.Description);

        //    return PartialView("_UserEdit", viewModel);
        //}




        try
        {
            var model = _mapper.Map<UpdateUserCommand>(viewModel);

            var result = await Mediator.Send(model);
            //if (result.Succeeded)
            //{

            //}
            //else
            //{
            //    return 
            //}

            //return Json(new { ok = true });

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Edit User failed");
            return Json(Result.Failure("更新使用者資料時發生錯誤"));
        }
    }

    private UserEditViewModel MapToVm(IdentityUser user)
    {
        var profile = _context.UserProfiles.SingleOrDefault(x => x.UserId == user.Id);

        // 這裡如果你用 ApplicationUser（擴充欄位），請對應一起加入
        return new UserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = profile?.FullName ?? string.Empty,
            IDNo = profile?.IDNo ?? string.Empty,
            Title = profile?.Title ?? string.Empty,
            DepartmentId = profile?.DepartmentId ?? 0,
            OfficeId = profile?.OfficeId ?? 0,
            DepartmentName = "XXX",
            OfficeName = "YYY",

            PhoneNumber = user.PhoneNumber,
            LockoutEnabled = user.LockoutEnabled,
            // 下面兩個 Confirmed 屬性需要透過 UserManager 讀取，
            // IdentityUser 本身沒有屬性，改以 UserManager 方法取得：
            // 這裡先填 false，實際在 DetailsModal/EditModal 讀取時再補強。
            EmailConfirmed = false,
            PhoneNumberConfirmed = false
        };
    }
}
