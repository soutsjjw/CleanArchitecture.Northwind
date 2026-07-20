namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x => !x.OrderedFrom.HasValue || !x.OrderedTo.HasValue || x.OrderedFrom.Value.Date <= x.OrderedTo.Value.Date)
            .WithMessage("訂單起始日期不可晚於結束日期");
    }
}
