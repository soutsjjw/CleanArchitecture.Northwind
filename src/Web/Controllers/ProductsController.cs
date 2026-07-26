using System.Globalization;
using System.Security.Cryptography;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;
using CleanArchitecture.Northwind.Application.Features.Inventory.Commands.Stocktake;
using CleanArchitecture.Northwind.Application.Features.Inventory.Queries.GetInventoryTransactions;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.CreateProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.DeleteProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.SetProductDiscontinued;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.UpdateProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductDetail;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductFormOptions;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProducts;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Extensions;
using CleanArchitecture.Northwind.Web.Services;
using CleanArchitecture.Northwind.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize]
public sealed class ProductsController(
    ISender sender,
    IDataProtectionProvider dataProtectionProvider,
    IAuthorizationService authorizationService) : Controller
{
    private readonly IDataProtector _detailsIdProtector =
        dataProtectionProvider.CreateProtector("Products.Details.ItemId.v1");
    private readonly IDataProtector _imageIdProtector =
        dataProtectionProvider.CreateProtector("Products.Image.ItemId.v1");
    private readonly IDataProtector _editIdProtector =
        dataProtectionProvider.CreateProtector("Products.Edit.ItemId.v1");
    private readonly IDataProtector _deleteIdProtector =
        dataProtectionProvider.CreateProtector("Products.Delete.ItemId.v1");
    private readonly IDataProtector _setDiscontinuedIdProtector =
        dataProtectionProvider.CreateProtector(
            "Products.SetDiscontinued.ItemId.v1");
    private readonly IDataProtector _adjustInventoryIdProtector =
        dataProtectionProvider.CreateProtector(
            "Products.AdjustInventory.ItemId.v1");
    private readonly IDataProtector _stocktakeIdProtector =
        dataProtectionProvider.CreateProtector("Products.Stocktake.ItemId.v1");

    [HttpGet]
    [Authorize(Policy = Policies.Products_Read)]
    public async Task<IActionResult> Index(
        string? keyword = null,
        int? categoryId = null,
        int? supplierId = null,
        bool? discontinued = null,
        bool lowStockOnly = false,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!await CanReadSuppliersAsync())
        {
            return Forbid();
        }

        var productsResult = await sender.Send(new GetProductsQuery
        {
            Keyword = keyword,
            CategoryId = categoryId,
            SupplierId = supplierId,
            Discontinued = discontinued,
            LowStockOnly = lowStockOnly,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        if (!productsResult.Succeeded)
        {
            return RedirectToAction("Index", "Home")
                .WithError(this, productsResult.Errors.ToList());
        }

        var optionsResult = await sender.Send(
            new GetProductFormOptionsQuery(),
            cancellationToken);
        var options = optionsResult.Succeeded
            ? optionsResult.Data
            : new ProductFormOptionsDto([], []);

        var page = productsResult.Data;
        return View(new ProductIndexViewModel
        {
            Keyword = keyword,
            CategoryId = categoryId,
            SupplierId = supplierId,
            Discontinued = discontinued,
            LowStockOnly = lowStockOnly,
            Pagination = page,
            Items = page.Items.Select(MapListItem).ToList(),
            Categories = options.Categories.Select(MapOption).ToList(),
            Suppliers = options.Suppliers.Select(MapOption).ToList()
        });
    }

    [HttpGet]
    [Authorize(Policy = Policies.Products_Read)]
    [Authorize(Policy = "Inventory:Read:")]
    public async Task<IActionResult> Details(
        string id,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(_detailsIdProtector, id, out var productId))
        {
            return NotFound();
        }

        var detailResult = await sender.Send(
            new GetProductDetailQuery(productId),
            cancellationToken);
        if (!detailResult.Succeeded)
        {
            return NotFound();
        }

        var inventoryResult = await sender.Send(
            new GetInventoryTransactionsQuery
            {
                ProductId = productId,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);

        if (!inventoryResult.Succeeded)
        {
            return RedirectToAction(nameof(Index))
                .WithError(this, inventoryResult.Errors.ToList());
        }

        var detail = detailResult.Data;
        var inventory = inventoryResult.Data;
        return View(new ProductDetailViewModel
        {
            Id = detail.Id,
            DetailsProtectedId = ProtectId(_detailsIdProtector, detail.Id),
            ImageProtectedId = ProtectId(_imageIdProtector, detail.Id),
            EditProtectedId = ProtectId(_editIdProtector, detail.Id),
            DeleteProtectedId = ProtectId(_deleteIdProtector, detail.Id),
            SetDiscontinuedProtectedId =
                ProtectId(_setDiscontinuedIdProtector, detail.Id),
            AdjustInventoryProtectedId =
                ProtectId(_adjustInventoryIdProtector, detail.Id),
            StocktakeProtectedId =
                ProtectId(_stocktakeIdProtector, detail.Id),
            ProductName = detail.ProductName,
            CategoryName = detail.CategoryName,
            SupplierName = detail.SupplierName,
            QuantityPerUnit = detail.QuantityPerUnit,
            UnitPrice = detail.UnitPrice,
            UnitsInStock = detail.UnitsInStock,
            UnitsOnOrder = detail.UnitsOnOrder,
            ReorderLevel = detail.ReorderLevel,
            Discontinued = detail.Discontinued,
            HasPicture = detail.Picture is not null,
            RowVersion = Convert.ToBase64String(detail.RowVersion),
            InventoryPagination = inventory,
            InventoryTransactions = inventory.Items
                .Select(transaction => new InventoryTransactionItemViewModel
                {
                    TransactionType = transaction.TransactionType,
                    QuantityBefore = transaction.QuantityBefore,
                    QuantityDelta = transaction.QuantityDelta,
                    QuantityAfter = transaction.QuantityAfter,
                    Reason = transaction.Reason,
                    Created = transaction.Created,
                    CreatedBy = transaction.CreatedBy
                })
                .ToList()
        });
    }

    [HttpGet]
    [Authorize(Policy = Policies.Products_Read)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Image(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(_imageIdProtector, id, out var productId))
        {
            return NotFound();
        }

        var result = await sender.Send(
            new GetProductDetailQuery(productId),
            cancellationToken);
        if (!result.Succeeded
            || !ProductImageValidator.TryValidateStored(
                result.Data.Picture,
                result.Data.PictureContentType,
                out var image))
        {
            return NotFound();
        }

        return File(image!.Content, image.ContentType);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Products_Create)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken = default)
    {
        if (!await CanReadSuppliersAsync())
        {
            return Forbid();
        }

        var model = new ProductEditViewModel();
        await LoadProductOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Products_Create)]
    [RequestSizeLimit(ProductImageValidator.MaxFileSize + 64 * 1024)]
    public async Task<IActionResult> Create(
        ProductEditViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!await CanReadSuppliersAsync())
        {
            return Forbid();
        }

        var image = ValidateOptionalImage(model.Picture);
        if (!ModelState.IsValid)
        {
            await LoadProductOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var result = await sender.Send(new CreateProductCommand
        {
            ProductName = model.ProductName,
            CategoryId = model.CategoryId,
            SupplierId = model.SupplierId,
            QuantityPerUnit = model.QuantityPerUnit,
            UnitPrice = model.UnitPrice,
            ReorderLevel = model.ReorderLevel,
            Picture = image?.Content,
            PictureContentType = image?.ContentType
        }, cancellationToken);

        if (!result.Succeeded)
        {
            AddResultErrors(result);
            await LoadProductOptionsAsync(model, cancellationToken);
            return View(model);
        }

        return RedirectToAction(
                nameof(Details),
                new { id = ProtectId(_detailsIdProtector, result.Data) })
            .WithSuccess(this, "商品已新增。");
    }

    [HttpGet]
    [Authorize(Policy = Policies.Products_Update)]
    public async Task<IActionResult> Edit(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!await CanReadSuppliersAsync())
        {
            return Forbid();
        }

        if (!TryUnprotectId(_editIdProtector, id, out var productId))
        {
            return NotFound();
        }

        var result = await sender.Send(
            new GetProductDetailQuery(productId),
            cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound();
        }

        var detail = result.Data;
        var model = new ProductEditViewModel
        {
            ProtectedId = ProtectId(_editIdProtector, detail.Id),
            ProductName = detail.ProductName,
            CategoryId = detail.CategoryId,
            SupplierId = detail.SupplierId,
            QuantityPerUnit = detail.QuantityPerUnit,
            UnitPrice = detail.UnitPrice,
            ReorderLevel = detail.ReorderLevel,
            HasPicture = detail.Picture is not null
        };
        await LoadProductOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Products_Update)]
    [RequestSizeLimit(ProductImageValidator.MaxFileSize + 64 * 1024)]
    public async Task<IActionResult> Edit(
        ProductEditViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!await CanReadSuppliersAsync())
        {
            return Forbid();
        }

        if (!TryUnprotectId(
                _editIdProtector,
                model.ProtectedId,
                out var productId))
        {
            return NotFound();
        }

        if (model.RemovePicture && model.Picture is not null)
        {
            ModelState.AddModelError(
                nameof(model.Picture),
                "移除圖片時不可同時上傳新圖片。");
        }

        var image = ValidateOptionalImage(model.Picture);
        if (!ModelState.IsValid)
        {
            await LoadProductOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var result = await sender.Send(new UpdateProductCommand
        {
            Id = productId,
            ProductName = model.ProductName,
            CategoryId = model.CategoryId,
            SupplierId = model.SupplierId,
            QuantityPerUnit = model.QuantityPerUnit,
            UnitPrice = model.UnitPrice,
            ReorderLevel = model.ReorderLevel,
            Picture = image?.Content,
            PictureContentType = image?.ContentType,
            RemovePicture = model.RemovePicture
        }, cancellationToken);

        if (!result.Succeeded)
        {
            AddResultErrors(result);
            await LoadProductOptionsAsync(model, cancellationToken);
            return View(model);
        }

        return RedirectToAction(
                nameof(Details),
                new { id = ProtectId(_detailsIdProtector, productId) })
            .WithSuccess(this, "商品已更新。");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Products_Delete)]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(_deleteIdProtector, id, out var productId))
        {
            return NotFound();
        }

        var result = await sender.Send(
            new DeleteProductCommand { Id = productId },
            cancellationToken);
        var redirect = RedirectToAction(nameof(Index));
        return result.Succeeded
            ? redirect.WithSuccess(this, "商品已刪除。")
            : redirect.WithError(this, result.Errors.ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Products_Update)]
    public async Task<IActionResult> SetDiscontinued(
        string id,
        bool discontinued,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(
                _setDiscontinuedIdProtector,
                id,
                out var productId))
        {
            return NotFound();
        }

        var result = await sender.Send(new SetProductDiscontinuedCommand
        {
            Id = productId,
            Discontinued = discontinued
        }, cancellationToken);
        var redirect = RedirectToAction(
            nameof(Details),
            new { id = ProtectId(_detailsIdProtector, productId) });
        return result.Succeeded
            ? redirect.WithSuccess(this, discontinued ? "商品已停用。" : "商品已恢復啟用。")
            : redirect.WithError(this, result.Errors.ToList());
    }

    [HttpGet]
    [Authorize(Policy = "Inventory:Create:")]
    public Task<IActionResult> AdjustInventory(
        string id,
        CancellationToken cancellationToken = default)
        => GetInventoryFormAsync(id, false, cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Inventory:Create:")]
    public async Task<IActionResult> AdjustInventory(
        InventoryAdjustmentViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(
                _adjustInventoryIdProtector,
                model.ProtectedId,
                out var productId))
        {
            return NotFound();
        }

        if (!TryDecodeRowVersion(model.RowVersion, out var rowVersion))
        {
            ModelState.AddModelError(nameof(model.RowVersion), "庫存版本資料無效，請重新載入頁面。");
        }

        if (!ModelState.IsValid)
        {
            await ReloadInventoryIdentityAsync(model, productId, cancellationToken);
            return View(model);
        }

        var result = await sender.Send(new AdjustInventoryCommand(
            productId,
            model.QuantityDelta,
            model.Reason,
            rowVersion), cancellationToken);
        if (!result.Succeeded)
        {
            AddResultErrors(result);
            await ReloadInventoryIdentityAsync(model, productId, cancellationToken);
            return View(model);
        }

        return RedirectToAction(
                nameof(Details),
                new { id = ProtectId(_detailsIdProtector, productId) })
            .WithSuccess(this, "庫存已調整。");
    }

    [HttpGet]
    [Authorize(Policy = "Inventory:Create:")]
    public Task<IActionResult> Stocktake(
        string id,
        CancellationToken cancellationToken = default)
        => GetInventoryFormAsync(id, true, cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Inventory:Create:")]
    public async Task<IActionResult> Stocktake(
        StocktakeViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(
                _stocktakeIdProtector,
                model.ProtectedId,
                out var productId))
        {
            return NotFound();
        }

        if (!TryDecodeRowVersion(model.RowVersion, out var rowVersion))
        {
            ModelState.AddModelError(nameof(model.RowVersion), "庫存版本資料無效，請重新載入頁面。");
        }

        if (!ModelState.IsValid)
        {
            await ReloadStocktakeIdentityAsync(model, productId, cancellationToken);
            return View(model);
        }

        var result = await sender.Send(new StocktakeCommand(
            productId,
            model.ActualQuantity,
            model.Reason,
            rowVersion), cancellationToken);
        if (!result.Succeeded)
        {
            AddResultErrors(result);
            await ReloadStocktakeIdentityAsync(model, productId, cancellationToken);
            return View(model);
        }

        return RedirectToAction(
                nameof(Details),
                new { id = ProtectId(_detailsIdProtector, productId) })
            .WithSuccess(this, "盤點已完成。");
    }

    private async Task<IActionResult> GetInventoryFormAsync(
        string id,
        bool stocktake,
        CancellationToken cancellationToken)
    {
        var formProtector = stocktake
            ? _stocktakeIdProtector
            : _adjustInventoryIdProtector;
        if (!TryUnprotectId(formProtector, id, out var productId))
        {
            return NotFound();
        }

        var result = await sender.Send(
            new GetProductDetailQuery(productId),
            cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound();
        }

        var detail = result.Data;
        if (stocktake)
        {
            return View(nameof(Stocktake), new StocktakeViewModel
            {
                ProtectedId = ProtectId(_stocktakeIdProtector, detail.Id),
                DetailsProtectedId =
                    ProtectId(_detailsIdProtector, detail.Id),
                ProductName = detail.ProductName,
                UnitsInStock = detail.UnitsInStock,
                ActualQuantity = detail.UnitsInStock,
                RowVersion = Convert.ToBase64String(detail.RowVersion)
            });
        }

        return View(nameof(AdjustInventory), new InventoryAdjustmentViewModel
        {
            ProtectedId =
                ProtectId(_adjustInventoryIdProtector, detail.Id),
            DetailsProtectedId =
                ProtectId(_detailsIdProtector, detail.Id),
            ProductName = detail.ProductName,
            UnitsInStock = detail.UnitsInStock,
            RowVersion = Convert.ToBase64String(detail.RowVersion)
        });
    }

    private async Task LoadProductOptionsAsync(
        ProductEditViewModel model,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductFormOptionsQuery(),
            cancellationToken);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            model.Categories = [];
            model.Suppliers = [];
            return;
        }

        model.Categories = result.Data.Categories.Select(MapOption).ToList();
        model.Suppliers = result.Data.Suppliers.Select(MapOption).ToList();
    }

    private ValidatedProductImage? ValidateOptionalImage(IFormFile? file)
    {
        if (file is null)
        {
            return null;
        }

        if (ProductImageValidator.TryValidate(file, out var image, out var error))
        {
            return image;
        }

        ModelState.AddModelError(
            nameof(ProductEditViewModel.Picture),
            error ?? "商品圖片無效。");
        return null;
    }

    private async Task ReloadInventoryIdentityAsync(
        InventoryAdjustmentViewModel model,
        int productId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductDetailQuery(productId),
            cancellationToken);
        if (!result.Succeeded)
        {
            return;
        }

        model.ProductName = result.Data.ProductName;
        model.UnitsInStock = result.Data.UnitsInStock;
        model.DetailsProtectedId =
            ProtectId(_detailsIdProtector, productId);
        ModelState.Remove(nameof(model.RowVersion));
        model.RowVersion = Convert.ToBase64String(result.Data.RowVersion);
    }

    private async Task ReloadStocktakeIdentityAsync(
        StocktakeViewModel model,
        int productId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductDetailQuery(productId),
            cancellationToken);
        if (!result.Succeeded)
        {
            return;
        }

        model.ProductName = result.Data.ProductName;
        model.UnitsInStock = result.Data.UnitsInStock;
        model.DetailsProtectedId =
            ProtectId(_detailsIdProtector, productId);
        ModelState.Remove(nameof(model.RowVersion));
        model.RowVersion = Convert.ToBase64String(result.Data.RowVersion);
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

    private async Task<bool> CanReadSuppliersAsync()
        => (await authorizationService.AuthorizeAsync(
            User,
            Policies.Suppliers_Read)).Succeeded;

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

    private static bool TryDecodeRowVersion(
        string? value,
        out byte[] rowVersion)
    {
        rowVersion = [];
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            rowVersion = Convert.FromBase64String(value);
            return rowVersion.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private ProductListItemViewModel MapListItem(ProductListItemDto product)
        => new()
        {
            Id = product.Id,
            DetailsProtectedId =
                ProtectId(_detailsIdProtector, product.Id),
            ImageProtectedId =
                ProtectId(_imageIdProtector, product.Id),
            EditProtectedId =
                ProtectId(_editIdProtector, product.Id),
            DeleteProtectedId =
                ProtectId(_deleteIdProtector, product.Id),
            SetDiscontinuedProtectedId =
                ProtectId(_setDiscontinuedIdProtector, product.Id),
            ProductName = product.ProductName,
            CategoryName = product.CategoryName,
            SupplierName = product.SupplierName,
            UnitPrice = product.UnitPrice,
            UnitsInStock = product.UnitsInStock,
            ReorderLevel = product.ReorderLevel,
            Discontinued = product.Discontinued,
            HasPicture = product.HasPicture
        };

    private static ProductOptionViewModel MapOption(ProductFormOptionDto option)
        => new(option.Id, option.Name);
}
