using Core.Entities;

namespace Core.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string ProductType { get; set; }
    public required string ProductBrand { get; set; }
    public required string ProductCategory { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int QuantityInStock { get; set; }

    public ProductKind ProductKind { get; set; }
    public List<ProductOptionDto> Options { get; set; } = [];
    public List<ProductVariantDto> Variants { get; set; } = [];

    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductReviewDto> Reviews { get; set; } = [];
    public bool CanUserReview { get; set; }
    public string? AvailableCoupon { get; set; }
    public decimal? DiscountedPrice { get; set; }
}