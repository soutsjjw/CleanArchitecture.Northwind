using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : IRequest<Result<CustomersDto>>
{
    public string? Keyword { get; init; }

    public string? Country { get; init; }

    public string? City { get; init; }

    public CustomerSortField? SortBy { get; init; }

    public bool SortDescending { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed class GetCustomersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomersQuery, Result<CustomersDto>>
{
    public async Task<Result<CustomersDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = context.Customers.AsNoTracking();

        var countries = await customers
            .Where(customer => !string.IsNullOrWhiteSpace(customer.Country))
            .Select(customer => customer.Country!)
            .Distinct()
            .OrderBy(country => country)
            .ToListAsync(cancellationToken);

        var cities = customers
            .Where(customer => !string.IsNullOrWhiteSpace(customer.City));

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            cities = cities.Where(customer => customer.Country == request.Country);
        }

        var cityOptions = await cities
            .Select(customer => customer.City!)
            .Distinct()
            .OrderBy(city => city)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            customers = customers.Where(customer =>
                customer.CompanyName.ToLower().Contains(keyword)
                || (customer.ContactName != null && customer.ContactName.ToLower().Contains(keyword))
                || (customer.Country != null && customer.Country.ToLower().Contains(keyword))
                || (customer.City != null && customer.City.ToLower().Contains(keyword))
                || (customer.Phone != null && customer.Phone.ToLower().Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            customers = customers.Where(customer => customer.Country == request.Country);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            customers = customers.Where(customer => customer.City == request.City);
        }

        customers = (request.SortBy, request.SortDescending) switch
        {
            (CustomerSortField.CompanyName, true) => customers.OrderByDescending(customer => customer.CompanyName).ThenByDescending(customer => customer.Id),
            (CustomerSortField.ContactName, false) => customers.OrderBy(customer => customer.ContactName).ThenBy(customer => customer.Id),
            (CustomerSortField.ContactName, true) => customers.OrderByDescending(customer => customer.ContactName).ThenByDescending(customer => customer.Id),
            (CustomerSortField.Country, false) => customers.OrderBy(customer => customer.Country).ThenBy(customer => customer.Id),
            (CustomerSortField.Country, true) => customers.OrderByDescending(customer => customer.Country).ThenByDescending(customer => customer.Id),
            (CustomerSortField.City, false) => customers.OrderBy(customer => customer.City).ThenBy(customer => customer.Id),
            (CustomerSortField.City, true) => customers.OrderByDescending(customer => customer.City).ThenByDescending(customer => customer.Id),
            (CustomerSortField.Phone, false) => customers.OrderBy(customer => customer.Phone).ThenBy(customer => customer.Id),
            (CustomerSortField.Phone, true) => customers.OrderByDescending(customer => customer.Phone).ThenByDescending(customer => customer.Id),
            _ => customers.OrderBy(customer => customer.CompanyName).ThenBy(customer => customer.Id)
        };

        var query = customers.Select(customer => new CustomerListItemDto(
            customer.Id,
            customer.CompanyName,
            customer.ContactName ?? string.Empty,
            customer.Country ?? string.Empty,
            customer.City ?? string.Empty,
            customer.Phone ?? string.Empty));

        var page = await PaginatedList<CustomerListItemDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return await Result<CustomersDto>.SuccessAsync(new CustomersDto
        {
            Keyword = request.Keyword,
            Country = request.Country,
            City = request.City,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Countries = countries,
            Cities = cityOptions,
            Customers = page
        });
    }
}
