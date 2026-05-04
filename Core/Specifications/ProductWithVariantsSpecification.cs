using Core.Entities;

namespace Core.Specifications;

public class ProductWithVariantsSpecification : BaseSpecification<Product>
{
    public ProductWithVariantsSpecification()
    {
        AddInclude(p => p.Variants);
    }
}