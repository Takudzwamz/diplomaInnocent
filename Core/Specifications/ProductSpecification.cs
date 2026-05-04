using System;
using Core.Entities;
using Microsoft.EntityFrameworkCore;


namespace Core.Specifications;

public class ProductSpecification : BaseSpecification<Product>
{
    public ProductSpecification(ProductSpecParams productParams)
        : base(x =>
            (string.IsNullOrEmpty(productParams.Search)
                || x.Name.ToLower().Contains(productParams.Search)) &&
            (!productParams.BrandId.HasValue || x.ProductBrandId == productParams.BrandId) &&
            (!productParams.TypeId.HasValue || x.ProductTypeId == productParams.TypeId) &&
            (!productParams.CategoryId.HasValue || x.CategoryId == productParams.CategoryId) &&
        (!productParams.MinRating.HasValue ||
                (x.Reviews.Any() &&
                 x.Reviews.Average(r => r.Rating) >= productParams.MinRating.Value &&
                 x.Reviews.Average(r => r.Rating) < productParams.MinRating.Value + 1))
    )
    {
        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
        AddInclude(x => x.Category);
        AddInclude(x => x.Images);
        AddInclude(p => p.Variants);
        AddInclude(x => x.Reviews);
        AddInclude("Coupons.Coupon");
        ApplyPaging(productParams.PageSize * (productParams.PageIndex - 1), productParams.PageSize);

        switch (productParams.Sort)
        {
            case "priceAsc":
                AddOrderBy(x => x.Price);
                break;
            case "priceDesc":
                AddOrderByDescending(x => x.Price);
                break;
            case "newest":
                AddOrderByDescending(x => x.Id);
                break;
            default:
                AddOrderBy(x => x.Name);
                break;
        }
    }

    public ProductSpecification(IEnumerable<int> productIds) : base(p => productIds.Contains(p.Id))
    {
        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
        AddInclude(x => x.Category);
        AddInclude(x => x.Images);
        AddInclude(x => x.Reviews);
    }


    // --- ADD THIS NEW CONSTRUCTOR ---
    public ProductSpecification(int id, bool withImages = false)
        : base(x => x.Id == id)
    {
        if (withImages)
        {
            AddInclude(x => x.Images);
        }
        AddInclude(x => x.Reviews);
        AddInclude("Reviews.AppUser");

        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
        AddInclude(x => x.Category);
        AddInclude("Coupons.Coupon");

        AddInclude(p => p.Options);
        AddInclude("Options.Values"); // String-based for nested include
        AddInclude(p => p.Variants);
        AddInclude("Variants.OptionValues");
    }
}