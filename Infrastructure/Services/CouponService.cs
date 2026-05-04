using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Core.Entities.OrderAggregate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public CouponService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }
    
    public async Task<AppCoupon?> GetCouponFromPromoCode(string code, string userEmail)
    {
        // Specification to find the coupon by its code
        var spec = new CouponWithProductsSpecification(code);
        var coupon = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(spec);

        // --- VALIDATION CHECKS ---
        if (coupon == null) return null; // Not found

        // If coupon is limited to one per customer, check their usage history.
        if (coupon.LimitOnePerCustomer)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return null; // User must exist

            var usageSpec = new BaseSpecification<CouponUsage>(
                cu => cu.AppUserId == user.Id && cu.CouponId == coupon.Id
            );
            var existingUsage = await _unitOfWork.Repository<CouponUsage>().GetEntityWithSpec(usageSpec);
            
            // If we found a record, it means they've used this coupon before.
            if (existingUsage != null)
            {
                return null;
            }
        }

         // If this coupon is for first-time customers, check their order history.
        if (coupon.FirstTimeCustomerOnly)
        {
            var orderSpec = new BaseSpecification<Order>(
                o => o.BuyerEmail == userEmail && o.Status == OrderStatus.PaymentReceived
            );
            var orderCount = await _unitOfWork.Repository<Order>().CountAsync(orderSpec);

            // If they have 1 or more completed orders, the coupon is invalid for them.
            if (orderCount > 0)
            {
                return null;
            }
        }
        if (!coupon.IsActive) return null; // Disabled
        
        var now = DateTime.UtcNow;
        if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now) return null; // Not yet valid
        if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < now) return null; // Expired
        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value) return null; // Limit reached

        // If all checks pass, map the valid coupon entity to the AppCoupon DTO for the cart
        return new AppCoupon
        {
            Name = coupon.Description,
            AmountOff = coupon.AmountOff,
            PercentOff = coupon.PercentOff,
            PromotionCode = coupon.Code,
            CouponId = coupon.Id.ToString(),// Use our own entity's ID
            ApplicableProductIds = coupon.ApplicableProducts.Select(cp => cp.ProductId).ToList()
        };
    }
}