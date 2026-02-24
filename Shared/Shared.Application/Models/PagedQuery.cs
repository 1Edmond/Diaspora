using Shared.Application.Abstractions;

namespace Shared.Application.Models;

public abstract record PagedQuery<T> : IQuery<T> where T : class
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    protected PagedQuery()
    {
    }

    protected PagedQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize is > 0 and <= 100 ? pageSize : 10;
    }
}