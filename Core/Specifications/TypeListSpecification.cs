using System;
using Core.Entities;

namespace Core.Specifications;

public class TypeListSpecification : BaseSpecification<Product, string>
{
    // public TypeListSpecification()
    // {
    //     AddSelect(x => x.Type);
    //     ApplyDistinct();
    // }
    public TypeListSpecification(int brandId)
        : base(x => x.ProductBrandId == brandId)
    {
        AddSelect(x => x.ProductType.Name);
        ApplyDistinct();
    }
}