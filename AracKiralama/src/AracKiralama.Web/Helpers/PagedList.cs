using Microsoft.EntityFrameworkCore;

namespace AracKiralama.Web.Helpers;

/// <summary>Sayfalanmış liste. Views/Shared/_Pagination.cshtml ile birlikte kullanılır.</summary>
public class PagedList<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}
