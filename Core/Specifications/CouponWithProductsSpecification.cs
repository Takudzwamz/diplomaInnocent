using Core.Entities;

namespace Core.Specifications;

public class CouponWithProductsSpecification : BaseSpecification<Coupon>
{
    public CouponWithProductsSpecification(string couponCode)
        : base(c => c.Code.ToUpper() == couponCode.ToUpper())
    {
        // This is the crucial line that tells EF Core to load the related products.
        AddInclude(c => c.ApplicableProducts);
    }
}