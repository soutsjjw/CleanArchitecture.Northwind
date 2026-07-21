namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;

public class GetOrderDetailQueryValidator : AbstractValidator<GetOrderDetailQuery>
{
    public GetOrderDetailQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
