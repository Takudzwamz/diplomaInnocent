using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    
    public decimal Price { get; set; }

    // [Required]
    // public string PictureUrl { get; set; } = string.Empty;
    // ADD the new collection of images
    public List<ProductImage> Images { get; set; } = [];

    public List<ProductReview> Reviews { get; set; } = [];

    // ADD this helper property. It finds the main image for display on list pages.
    // The [NotMapped] attribute tells EF Core not to create a column for this in the database.
    // This allows your existing product listing UI to work without any changes!
    [NotMapped]
    public string PictureUrl => Images.FirstOrDefault(i => i.IsMain)?.Url ?? "images/placeholder.png";

    // [Required]
    // public string Type { get; set; } = string.Empty;

    // [Required]
    // public string Brand { get; set; } = string.Empty;


    public int ProductTypeId { get; set; }
    public ProductType ProductType { get; set; }
    public int ProductBrandId { get; set; }
    public ProductBrand ProductBrand { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    
    public int QuantityInStock { get; set; }

    public List<CouponProduct> Coupons { get; set; } = [];

    public ProductKind ProductKind { get; set; } = ProductKind.Simple;

    public List<ProductOption> Options { get; set; } = [];
    public List<ProductVariant> Variants { get; set; } = [];
    
    // AI-powered recommendations: Store pre-computed embedding as JSON array
    // This eliminates the need to call Azure OpenAI API on every recommendation request
    public string? Embedding { get; set; }
}