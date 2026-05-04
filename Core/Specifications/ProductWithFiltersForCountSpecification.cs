using Core.Entities;

namespace Core.Specifications;

/// <summary>
/// This specification is used only to get a count of products that match the filter criteria.
/// It does not include logic for paging or sorting.
/// </summary>
public class ProductWithFiltersForCountSpecification : BaseSpecification<Product>
{
    public ProductWithFiltersForCountSpecification(ProductSpecParams productParams)
        : base(x =>
            (string.IsNullOrEmpty(productParams.Search)
                || x.Name.ToLower().Contains(productParams.Search)) &&
            (!productParams.BrandId.HasValue || x.ProductBrandId == productParams.BrandId) &&
            (!productParams.TypeId.HasValue || x.ProductTypeId == productParams.TypeId) &&
            (!productParams.CategoryId.HasValue || x.CategoryId == productParams.CategoryId) &&
            (!productParams.MinRating.HasValue || 
                (x.Reviews.Any() && 
                 x.Reviews.Average(r => r.Rating) >= productParams.MinRating.Value && 
                 x.Reviews.Average(r => r.Rating) < productParams.MinRating.Value + 1)))
    {
    }
}
