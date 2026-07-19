namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IPaginatedList
{
    int PageNumber { get; }
    int TotalPages { get; }
    int PageSize { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
