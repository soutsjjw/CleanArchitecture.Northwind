namespace CleanArchitecture.Northwind.Application.Features.PersonalDataAccessLogs.Queries.GetPersonalDataAccessLogsByDate;

public class GetPersonalDataAccessLogsByDateQueryValidator : AbstractValidator<GetPersonalDataAccessLogsByDateQuery>
{
    public GetPersonalDataAccessLogsByDateQueryValidator()
    {
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("日期不可大於今天")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)))
            .WithMessage("日期最早不可超過今天一個月");
    }
}
