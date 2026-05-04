using System;

namespace Core.Specifications;

public class PagingParams
{
    private const int MaxPageSize = 1000;
    public int PageIndex { get; set; } = 1;

    private int _pageSize = 12;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}
