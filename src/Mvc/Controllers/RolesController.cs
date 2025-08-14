using AutoMapper;
using CleanArchitecture.Northwind.Application.Common.DTOs;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.AddMemberToRole;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.CreateRole;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.EditRole;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;
using CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;
using CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAccount;
using CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAllRoles;
using CleanArchitecture.Northwind.Application.Features.Role.Queries.GetRoleMembers;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;

namespace Mvc.Controllers;

[Authorize(Roles = "SystemAdmin,Administrator")] // 只有管理員能調整角色矩陣
public class RolesController : BaseController<RolesController>
{
    private readonly IMapper _mapper;
    private readonly RoleManager<ApplicationRole> _roles;
    private readonly ICommonService _commonService;

    public RolesController(IMapper mapper,
        RoleManager<ApplicationRole> roles,
        ICommonService commonService,
        ILogger<RolesController> logger)
        : base(logger)
    {
        _mapper = mapper;
        _roles = roles;
        _commonService = commonService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await Mediator.Send(new GetAllRolesQuery());

        ViewBag.Departments = _commonService.GetDepartmentOptions();
        ViewBag.Offices = _commonService.GetOfficeOptions();

        return View(result.Data);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var result = await Mediator.Send(new EditRolePrepareQuery { RoleId = id });

        if (result.Succeeded)
        {
            return View(result.Data);
        }

        return RedirectToAction(nameof(Index))
            .WithError(this, string.Join("、", result.Errors));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditRolePrepareDto viewModel)
    {
        try
        {
            var model = _mapper.Map<EditRoleCommand>(viewModel);

            var result = await Mediator.Send(model);

            if (result.Succeeded)
                return View(viewModel).WithSuccess("角色權限已成功更新");
            else
                return View(viewModel).WithError(result.Errors.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "編輯角色權限失敗");

            return View(viewModel).WithError("編輯角色權限失敗");
        }
    }

    public IActionResult Create() => View(new CreateRoleDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleDto viewModel)
    {
        try
        {
            var model = _mapper.Map<CreateRoleCommand>(viewModel);

            var result = await Mediator.Send(model);

            if (result.Succeeded)
                return RedirectToAction(nameof(Index));
            else
                return View(viewModel).WithError(result.Errors.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RemoveMemberFromRole failed");
            return Json(Result.Failure("建立角色時發生錯誤"));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetAccounts([FromBody] AccountConditionDto query)
    {
        try
        {
            var accounts = await Mediator.Send(new GetAccountQuery { DepartmentId = query.DepartmentId, OfficeId = query.OfficeId });

            return Json(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            return Json(await Result.FailureAsync("取得人員名單時發生錯誤"));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMemberToRole([FromBody] AddMemberToRoleDto dto)
    {
        try
        {
            var result = await Mediator.Send(new AddMemberToRoleCommand
            {
                RoleId = dto.RoleId,
                UserId = dto.UserId
            });

            if (result.Succeeded)
                return Json(Result.Success());
            else
                return Json(Result.Failure(result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddMemberToRole failed");
            return Json(Result.Failure("新增成員時發生錯誤"));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetRoleMembers([FromBody] string roleId)
    {
        try
        {
            var result = await Mediator.Send(new GetRoleMembersQuery { RoleId = roleId });

            if (result.Succeeded)
                return Json(result.Data);
            else
                return Json(Result.Failure(result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRoleMembers failed");
            return Json(Result.Failure("取得角色成員時發生錯誤"));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMemberFromRole([FromBody] RemoveMemberFromRoleDto viewModel)
    {
        try
        {
            var model = _mapper.Map<RemoveMemberFromRoleCommand>(viewModel);

            var result = await Mediator.Send(model);

            if (result.Succeeded)
                return Json(Result.Success());
            else
                return Json(Result.Failure(result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RemoveMemberFromRole failed");
            return Json(Result.Failure("移除成員時發生錯誤"));
        }
    }
}
