using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;

namespace Mvc.Controllers;

[Authorize(Roles = "SystemAdmin,Administrator")]
public class UserController : BaseController<UserController>
{
    private readonly IApplicationDbContext _context;

    public UserController(IApplicationDbContext context,
        ILogger<UserController> logger)
        : base(logger)
    {
        _context = context;
    }

    public sealed class UserRowVm
    {
        public string UserName { get; init; } = default!;
        public string? Email { get; init; }
        public string? FullName { get; init; }
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        if (result.Succeeded)
        {
            return View(result.Data);
        }

        return RedirectToAction("Index", "Home").WithError(this, "取得使用者列表失敗");
    }
}
