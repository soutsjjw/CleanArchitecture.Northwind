using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Application.Common.Models;

public sealed class PaginatedList<T> : IPaginatedList
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int FirstItemIndex { get; }
    public int LastItemIndex { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    private PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
        PageNumber = Math.Clamp(pageNumber, 1, TotalPages);
        Items = items;

        if (count == 0)
        {
            FirstItemIndex = 0;
            LastItemIndex = 0;
        }
        else
        {
            FirstItemIndex = (PageNumber - 1) * PageSize + 1;
            LastItemIndex = FirstItemIndex + items.Count - 1;
        }
    }

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 1000); // 依需求調整上限
        var count = await source.CountAsync(ct);

        // 預先算出合法頁碼，避免 Skip 負數
        var totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
        pageNumber = Math.Clamp(pageNumber, 1, totalPages);

        var items = count == 0
            ? new List<T>()
            : await source.Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync(ct);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
