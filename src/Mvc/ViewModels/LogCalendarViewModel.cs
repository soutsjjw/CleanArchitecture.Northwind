namespace CleanArchitecture.Northwind.Mvc.ViewModels;

public sealed class LogCalendarViewModel
{
    public required IReadOnlyList<DateOnly> Dates { get; init; }
}
