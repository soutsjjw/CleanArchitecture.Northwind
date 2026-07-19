namespace CleanArchitecture.Northwind.Web.ViewModels;

public sealed class LogCalendarViewModel
{
    public required IReadOnlyList<DateOnly> Dates { get; init; }
}
