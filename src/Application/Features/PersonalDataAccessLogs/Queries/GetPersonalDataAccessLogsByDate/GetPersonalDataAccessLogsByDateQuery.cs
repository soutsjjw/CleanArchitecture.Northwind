using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.PersonalDataAccessLogs.Queries.GetPersonalDataAccessLogsByDate;

public record GetPersonalDataAccessLogsByDateQuery : IRequest<Result<IList<PersonalDataAccessLogDto>>>
{
    public DateOnly Date { get; init; }
}
