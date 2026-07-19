using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.User.Commands.ResendConfirmationEmail;
using CleanArchitecture.Northwind.Application.Features.User.Commands.UpdateUser;
using CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;
using CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;
using CleanArchitecture.Northwind.Web.Extensions;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize(Roles = "SystemAdmin,Administrator")]
public class UserController : BaseController<UserController>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDataProtectionService _dataProtectionService;

    public UserController(IApplicationDbContext context,
        IMapper mapper,
        IDataProtectionService dataProtectionService,
        ILogger<UserController> logger)
        : base(logger)
    {
        _context = context;
        _mapper = mapper;
        _dataProtectionService = dataProtectionService;
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
            foreach (var item in result.Data.Users.Items)
            {
                item.UserId = _dataProtectionService.Protect(item.UserId);
            }

            return View(result.Data);
        }

        return RedirectToAction("Index", "Home").WithError(this, "取得使用者列表失敗");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(UsersDto viewModel, int pageNumber = 1, int pageSize = 10)
    {
        var result = await Mediator.Send(new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Email = viewModel.Email,
            FullName = viewModel.FullName,
            DepartmentId = viewModel.DepartmentId,
            OfficeId = viewModel.OfficeId,
            Status = viewModel.Status,
        });

        if (result.Succeeded)
        {
            GenerateDepartmentAndOfficeOptions();

            foreach (var item in result.Data.Users.Items)
            {
                item.UserId = _dataProtectionService.Protect(item.UserId);
            }

            return View("Search", result.Data);
        }
        return RedirectToAction("Index", "Home").WithError(this, "查詢使用者失敗");
    }

    [HttpGet]
    public async Task<IActionResult> DetailsModal(string id)
    {
        var result = await Mediator.Send(new UserDetailQuery
        {
            UserId = _dataProtectionService.Unprotect(id)
        });

        if (result.Succeeded)
        {
            result.Data.UserId = _dataProtectionService.Protect(result.Data.UserId);
            return PartialView("_UserDetail", result.Data);
        }

        return PartialView("_NoDataPartial", "無符合資料");
    }

    [HttpGet]
    public async Task<IActionResult> EditModal(string id)
    {
        var result = await Mediator.Send(new UserDetailQuery
        {
            UserId = _dataProtectionService.Unprotect(id)
        });

        if (result.Succeeded)
        {
            GenerateDepartmentOptions();

            result.Data.UserId = _dataProtectionService.Protect(result.Data.UserId);

            return PartialView("_UserEdit", result.Data);
        }

        return PartialView("_NoDataPartial", "無符合資料");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserDetailDto viewModel)
    {
        try
        {
            var model = _mapper.Map<UpdateUserCommand>(viewModel);
            model.UserId = _dataProtectionService.Unprotect(model.UserId);

            var result = await Mediator.Send(model);
            if (!result.Succeeded)
            {
                foreach (var field in result.FieldErrors)
                {
                    foreach (var value in field.Value)
                    {
                        ModelState.AddModelError(field.Key, value);
                    }
                }

                GenerateDepartmentOptions();
                return PartialView("_UserEdit", viewModel).WithError(result.Errors.ToList());
            }

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Edit User failed");
            return Json(Result.Failure("更新使用者資料時發生錯誤"));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmationEmail(string userId)
    {
        var result = await Mediator.Send(new ResendConfirmationEmailCommand
        {
            UserId = _dataProtectionService.Unprotect(userId)
        });

        return Json(result);
    }
}
