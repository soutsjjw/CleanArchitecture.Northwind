using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Database;
using CleanArchitecture.Northwind.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers;

[Authorize(Policy = Policies.Orders)]
public class OrdersController : BaseController<OrdersController>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOrdersService _ordersService;

    public OrdersController(IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IOrdersService ordersService,
        ILogger<OrdersController> logger)
        : base(logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _ordersService = ordersService;
    }

    [Authorize(Policy = Policies.Orders_Read)]
    public async Task<IActionResult> Index()
    {
        var list = await _ordersService.GetAllAsync();

        var entity = list.Single(x => x.Id == 10250);
        var ok = await _authorizationService.AuthorizeAsync(User, entity, Policies.Orders_Read);
        if (!ok.Succeeded) return Forbid();

        return View(list);
    }
}
