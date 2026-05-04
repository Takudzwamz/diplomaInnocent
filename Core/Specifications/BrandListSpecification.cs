using System;
using Core.Entities;

namespace Core.Specifications;

public class BrandListSpecification : BaseSpecification<Product, string>
{
    // public BrandListSpecification()
    // {
    //     AddSelect(x => x.Brand);
    //     ApplyDistinct();
    // }
    public BrandListSpecification(int typeId)
        : base(x => x.ProductTypeId == typeId)
    {
        AddSelect(x => x.ProductBrand.Name);
        ApplyDistinct();
    }
}
