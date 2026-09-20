using System.Globalization;
using System.Security.Cryptography;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.CreateSupplier;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.DeleteSupplier;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.SetSupplierActive;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.UpdateSupplier;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSupplierDetail;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Extensions;
using CleanArchitecture.Northwind.Web.ViewModels.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize]
public sealed class SuppliersController(ISender sender, IDataProtectionProvider provider) : Controller
{
    private readonly IDataProtector _edit = provider.CreateProtector("Suppliers.Edit.ItemId.v1");
    private readonly IDataProtector _delete = provider.CreateProtector("Suppliers.Delete.ItemId.v1");
    private readonly IDataProtector _active = provider.CreateProtector("Suppliers.SetActive.ItemId.v1");

    [HttpGet, Authorize(Policy = Policies.Suppliers_Read)]
    public async Task<IActionResult> Index(string? keyword = null, bool? isActive = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetSuppliersQuery { Keyword = keyword, IsActive = isActive, PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        if (!result.Succeeded) return RedirectToAction("Index", "Home").WithError(this, result.Errors.ToList());
        var page = result.Data;
        return View(new SupplierIndexViewModel { Keyword = keyword, IsActive = isActive, Pagination = page, Items = page.Items.Select(x => new SupplierListItemViewModel { CompanyName = x.CompanyName, ContactName = x.ContactName, Phone = x.Phone, Country = x.Country, IsActive = x.IsActive, ProductCount = x.ProductCount, EditProtectedId = Protect(_edit, x.Id), DeleteProtectedId = Protect(_delete, x.Id), SetActiveProtectedId = Protect(_active, x.Id) }).ToList() });
    }

    [HttpGet, Authorize(Policy = Policies.Suppliers_Create)] public IActionResult Create() => View(new SupplierEditViewModel());
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Policies.Suppliers_Create)]
    public async Task<IActionResult> Create(SupplierEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await sender.Send(MapCreate(model), cancellationToken);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        return RedirectToAction(nameof(Index)).WithSuccess(this, "供應商已新增。");
    }

    [HttpGet, Authorize(Policy = Policies.Suppliers_Update)]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
    {
        if (!TryUnprotect(_edit, id, out var supplierId)) return NotFound();
        var result = await sender.Send(new GetSupplierDetailQuery(supplierId), cancellationToken);
        return !result.Succeeded ? NotFound() : View(MapEdit(result.Data, Protect(_edit, supplierId)));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Policies.Suppliers_Update)]
    public async Task<IActionResult> Edit(SupplierEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (!TryUnprotect(_edit, model.ProtectedId, out var supplierId)) return NotFound();
        if (!ModelState.IsValid) return View(model);
        var result = await sender.Send(MapUpdate(model, supplierId), cancellationToken);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        return RedirectToAction(nameof(Index)).WithSuccess(this, "供應商已更新。");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Policies.Suppliers_Delete)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        if (!TryUnprotect(_delete, id, out var supplierId)) return NotFound();
        var result = await sender.Send(new DeleteSupplierCommand { Id = supplierId }, cancellationToken);
        return result.Succeeded ? RedirectToAction(nameof(Index)).WithSuccess(this, "供應商已刪除。") : RedirectToAction(nameof(Index)).WithError(this, result.Errors.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Policies.Suppliers_Update)]
    public async Task<IActionResult> SetActive(string id, bool isActive, CancellationToken cancellationToken = default)
    {
        if (!TryUnprotect(_active, id, out var supplierId)) return NotFound();
        var result = await sender.Send(new SetSupplierActiveCommand(supplierId, isActive), cancellationToken);
        return result.Succeeded ? RedirectToAction(nameof(Index)).WithSuccess(this, isActive ? "供應商已恢復啟用。" : "供應商已停用。") : RedirectToAction(nameof(Index)).WithError(this, result.Errors.ToList());
    }

    private static CreateSupplierCommand MapCreate(SupplierEditViewModel x) => new() { CompanyName = x.CompanyName, ContactName = x.ContactName, ContactTitle = x.ContactTitle, Address = x.Address, City = x.City, Region = x.Region, PostalCode = x.PostalCode, Country = x.Country, Phone = x.Phone, Fax = x.Fax, HomePage = x.HomePage };
    private static UpdateSupplierCommand MapUpdate(SupplierEditViewModel x, int id) => new() { Id = id, CompanyName = x.CompanyName, ContactName = x.ContactName, ContactTitle = x.ContactTitle, Address = x.Address, City = x.City, Region = x.Region, PostalCode = x.PostalCode, Country = x.Country, Phone = x.Phone, Fax = x.Fax, HomePage = x.HomePage };
    private static SupplierEditViewModel MapEdit(SupplierDetailDto x, string id) => new() { ProtectedId = id, CompanyName = x.CompanyName, ContactName = x.ContactName, ContactTitle = x.ContactTitle, Address = x.Address, City = x.City, Region = x.Region, PostalCode = x.PostalCode, Country = x.Country, Phone = x.Phone, Fax = x.Fax, HomePage = x.HomePage, IsActive = x.IsActive, ProductCount = x.ProductCount };
    private void AddErrors(Result result) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error); }
    private static string Protect(IDataProtector protector, int id) => protector.Protect(id.ToString(CultureInfo.InvariantCulture));
    private static bool TryUnprotect(IDataProtector protector, string? value, out int id) { id = 0; try { return !string.IsNullOrWhiteSpace(value) && int.TryParse(protector.Unprotect(value), NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0; } catch (CryptographicException) { return false; } }
}
