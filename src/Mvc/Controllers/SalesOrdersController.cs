using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers;

[Authorize(Policy = "SalesOrders::")]
public class SalesOrdersController : BaseController<SalesOrdersController>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOrdersService _ordersService;

    public SalesOrdersController(IApplicationDbContext context,
        IAuthorizationService authorizationService,
        IOrdersService ordersService,
        ILogger<SalesOrdersController> logger)
        : base(logger)
    {
        _context = context;
        _authorizationService = authorizationService;
        _ordersService = ordersService;
    }

    //[Authorize(Policy = "PERM:SalesOrders:Read")]
    //[Authorize(Policy = "SalesOrders:Read:")]
    public async Task<IActionResult> Index()
    {
        var list = await _ordersService.GetAllAsync();


        //var customer = await _context.Customers.FindAsync(id); // customer : IOwnedResource
        var entity = list.Single(x => x.Id == 10250);
        var ok = await _authorizationService.AuthorizeAsync(User, entity, "SalesOrders:Read:Self");
        if (!ok.Succeeded) return Forbid();


        return View(list);
    }

    //[Authorize(Policy = Policies.SalesOrders_Create)]
    //public async Task<IActionResult> Create(CreateOrderDto dto) { /* ... */ }

    //[HttpPost]
    //public async Task<IActionResult> UpdateOwn(int id, UpdateOrderDto dto)
    //{
    //    // 1) 先檢查 role policy（可加在 action 屬性上）
    //    var auth = await _authorization.AuthorizeAsync(User, id, Policies.SalesOrders_Update_Own);
    //    if (!auth.Succeeded) return Forbid();

    //    // 2) 執行更新（僅限擁有者）
    //    // ...
    //    return Ok();
    //}

    //[HttpPost]
    //[Authorize(Policy = Policies.SalesOrders_UpdateShipmentFields)]
    //public async Task<IActionResult> UpdateShipment(int id, UpdateShipmentDto dto)
    //{
    //    var order = await _orders.GetByIdAsync(id);
    //    if (order is null) return NotFound();

    //    var auth = await _authorization.AuthorizeAsync(User, (order, dto), Policies.SalesOrders_UpdateShipmentFields);
    //    if (!auth.Succeeded) return Forbid();

    //    // 更新允許欄位...
    //    return Ok();
    //}

    //[HttpPost]
    //[Authorize(Policy = Policies.SalesOrders_Delete_Soft)]
    //public async Task<IActionResult> Delete(int id)
    //{
    //    // 軟刪
    //    await _orders.SoftDeleteAsync(id);
    //    return Ok();
    //}
}
