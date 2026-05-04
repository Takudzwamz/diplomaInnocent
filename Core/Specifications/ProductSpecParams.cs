namespace Core.Specifications;

public class ProductSpecParams : PagingParams
{
    public int? BrandId { get; set; } // CHANGED from List<string> Brands
    public int? TypeId { get; set; } // CHANGED from List<string> Types
    public int? CategoryId { get; set; }
     public int? MinRating { get; set; } 
    public string? Sort { get; set; }
    
    private string? _search;
    public string Search
    {
        get => _search ?? "";
        set => _search = value?.ToLower();
    }
}