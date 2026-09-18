namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerOrderHistory;

public sealed class GetCustomerOrderHistoryQueryValidator : AbstractValidator<GetCustomerOrderHistoryQuery>
{
    public GetCustomerOrderHistoryQueryValidator()
    {
        RuleFor(query => query.CustomerId).NotEmpty();
        RuleFor(query => query.PageNumber).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
