using System.Globalization;
using System.Security.Cryptography;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.CreateCategory;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.DeleteCategory;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.SetCategoryActive;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.UpdateCategory;
using CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategories;
using CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategoryDetail;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Extensions;
using CleanArchitecture.Northwind.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize]
public sealed class CategoriesController(
    ISender sender,
    IDataProtectionProvider dataProtectionProvider) : Controller
{
    private readonly IDataProtector _editIdProtector =
        dataProtectionProvider.CreateProtector("Categories.Edit.ItemId.v1");
    private readonly IDataProtector _deleteIdProtector =
        dataProtectionProvider.CreateProtector("Categories.Delete.ItemId.v1");
    private readonly IDataProtector _setActiveIdProtector =
        dataProtectionProvider.CreateProtector(
            "Categories.SetActive.ItemId.v1");

    [HttpGet]
    [Authorize(Policy = Policies.Categories_Read)]
    public async Task<IActionResult> Index(
        string? keyword = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCategoriesQuery
        {
            Keyword = keyword,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);
        if (!result.Succeeded)
        {
            return RedirectToAction("Index", "Home")
                .WithError(this, result.Errors.ToList());
        }

        var page = result.Data;
        return View(new CategoryIndexViewModel
        {
            Keyword = keyword,
            IsActive = isActive,
            Pagination = page,
            Items = page.Items.Select(category => new CategoryListItemViewModel
            {
                Id = category.Id,
                EditProtectedId =
                    ProtectId(_editIdProtector, category.Id),
                DeleteProtectedId =
                    ProtectId(_deleteIdProtector, category.Id),
                SetActiveProtectedId =
                    ProtectId(_setActiveIdProtector, category.Id),
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive,
                ProductCount = category.ProductCount
            }).ToList()
        });
    }

    [HttpGet]
    [Authorize(Policy = Policies.Categories_Create)]
    public IActionResult Create()
        => View(new CategoryEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Categories_Create)]
    public async Task<IActionResult> Create(
        CategoryEditViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await sender.Send(new CreateCategoryCommand
        {
            CategoryName = model.CategoryName,
            Description = model.Description
        }, cancellationToken);
        if (!result.Succeeded)
        {
            AddResultErrors(result);
            return View(model);
        }

        return RedirectToAction(nameof(Index))
            .WithSuccess(this, "分類已新增。");
    }

    [HttpGet]
    [Authorize(Policy = Policies.Categories_Update)]
    public async Task<IActionResult> Edit(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(_editIdProtector, id, out var categoryId))
        {
            return NotFound();
        }

        var result = await sender.Send(
            new GetCategoryDetailQuery(categoryId),
            cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound();
        }

        return View(new CategoryEditViewModel
        {
            ProtectedId = ProtectId(_editIdProtector, result.Data.Id),
            CategoryName = result.Data.CategoryName,
            Description = result.Data.Description,
            IsActive = result.Data.IsActive,
            ProductCount = result.Data.ProductCount
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Categories_Update)]
    public async Task<IActionResult> Edit(
        CategoryEditViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(
                _editIdProtector,
                model.ProtectedId,
                out var categoryId))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await sender.Send(new UpdateCategoryCommand
        {
            Id = categoryId,
            CategoryName = model.CategoryName,
            Description = model.Description
        }, cancellationToken);
        if (!result.Succeeded)
        {
            AddResultErrors(result);
            return View(model);
        }

        return RedirectToAction(nameof(Index))
            .WithSuccess(this, "分類已更新。");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Categories_Delete)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(_deleteIdProtector, id, out var categoryId))
        {
            return NotFound();
        }

        var result = await sender.Send(
            new DeleteCategoryCommand { Id = categoryId },
            cancellationToken);
        var redirect = RedirectToAction(nameof(Index));
        return result.Succeeded
            ? redirect.WithSuccess(this, "分類已刪除。")
            : redirect.WithError(this, result.Errors.ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Categories_Update)]
    public async Task<IActionResult> SetActive(
        string id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(
                _setActiveIdProtector,
                id,
                out var categoryId))
        {
            return NotFound();
        }

        var result = await sender.Send(new SetCategoryActiveCommand
        {
            Id = categoryId,
            IsActive = isActive
        }, cancellationToken);
        var redirect = RedirectToAction(nameof(Index));
        return result.Succeeded
            ? redirect.WithSuccess(this, isActive ? "分類已恢復啟用。" : "分類已停用。")
            : redirect.WithError(this, result.Errors.ToList());
    }

    private void AddResultErrors(Result result)
    {
        foreach (var field in result.FieldErrors)
        {
            foreach (var error in field.Value)
            {
                ModelState.AddModelError(field.Key, error);
            }
        }

        if (!result.HasFieldErrors)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
    }

    private static bool TryUnprotectId(
        IDataProtector protector,
        string? protectedId,
        out int id)
    {
        id = default;
        if (string.IsNullOrWhiteSpace(protectedId))
        {
            return false;
        }

        try
        {
            var value = protector.Unprotect(protectedId);
            return int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out id)
                && id > 0;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private static string ProtectId(IDataProtector protector, int id)
        => protector.Protect(
            id.ToString(CultureInfo.InvariantCulture));
}
