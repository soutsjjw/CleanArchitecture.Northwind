using System.Security.Cryptography;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerDetail;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Extensions;
using CleanArchitecture.Northwind.Web.ViewModels.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize(Policy = Policies.Customers_Read)]
public sealed class CustomersController(
    ISender sender,
    IDataProtectionProvider dataProtectionProvider) : Controller
{
    private readonly IDataProtector _detailsIdProtector =
        dataProtectionProvider.CreateProtector("Customers.Details.CustomerId.v1");

    [HttpGet]
    public Task<IActionResult> Index(
        string? keyword = null,
        string? country = null,
        string? city = null,
        CustomerSortField? sortBy = null,
        bool sortDescending = false,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
        => GetIndexAsync(CreateQuery(keyword, country, city, sortBy, sortDescending, pageNumber, pageSize), cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Index))]
    public Task<IActionResult> Search(
        string? keyword = null,
        string? country = null,
        string? city = null,
        CustomerSortField? sortBy = null,
        bool sortDescending = false,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
        => GetIndexAsync(CreateQuery(keyword, country, city, sortBy, sortDescending, pageNumber, pageSize), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> Details(
        string id,
        string? keyword = null,
        string? country = null,
        string? city = null,
        CustomerSortField? sortBy = null,
        bool sortDescending = false,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!TryUnprotectId(id, out var customerId))
        {
            return RedirectToAction(nameof(Index), new { keyword, country, city, sortBy, sortDescending, pageNumber, pageSize })
                .WithError(this, "客戶識別碼無效。");
        }

        var result = await sender.Send(new GetCustomerDetailQuery(customerId), cancellationToken);
        if (!result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { keyword, country, city, sortBy, sortDescending, pageNumber, pageSize })
                .WithError(this, result.Errors.ToList());
        }

        var customer = result.Data;
        return View(new CustomerDetailViewModel
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            ContactName = customer.ContactName,
            ContactTitle = customer.ContactTitle,
            Address = customer.Address,
            City = customer.City,
            Region = customer.Region,
            PostalCode = customer.PostalCode,
            Country = customer.Country,
            Phone = customer.Phone,
            Fax = customer.Fax,
            Keyword = keyword,
            CountryFilter = country,
            CityFilter = city,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    private async Task<IActionResult> GetIndexAsync(GetCustomersQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        if (!result.Succeeded)
        {
            return RedirectToAction("Index", "Home").WithError(this, result.Errors.ToList());
        }

        var customers = result.Data;
        return View(new CustomerIndexViewModel
        {
            Keyword = customers.Keyword,
            Country = customers.Country,
            City = customers.City,
            SortBy = customers.SortBy,
            SortDescending = customers.SortDescending,
            Pagination = customers.Customers,
            TotalCount = customers.Customers.TotalCount,
            FirstItemIndex = customers.Customers.FirstItemIndex,
            LastItemIndex = customers.Customers.LastItemIndex,
            Countries = customers.Countries,
            Cities = customers.Cities,
            Items = customers.Customers.Items.Select(customer => new CustomerListItemViewModel
            {
                DetailsProtectedId = _detailsIdProtector.Protect(customer.Id),
                CompanyName = customer.CompanyName,
                ContactName = customer.ContactName,
                Country = customer.Country,
                City = customer.City,
                Phone = customer.Phone
            }).ToList()
        });
    }

    private static GetCustomersQuery CreateQuery(
        string? keyword,
        string? country,
        string? city,
        CustomerSortField? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize)
        => new()
        {
            Keyword = keyword,
            Country = country,
            City = city,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

    private bool TryUnprotectId(string? protectedId, out string customerId)
    {
        customerId = string.Empty;
        if (string.IsNullOrWhiteSpace(protectedId))
        {
            return false;
        }

        try
        {
            customerId = _detailsIdProtector.Unprotect(protectedId);
            return !string.IsNullOrWhiteSpace(customerId);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
