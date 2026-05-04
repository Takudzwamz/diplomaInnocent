

namespace Core.Specifications;

public class OrderSpecParams : PagingParams
{
    public string? Status { get; set; }
    public string? Sort { get; set; }
    public string? CustomerEmail { get; set; }
}
