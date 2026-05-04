using Core.DTOs;
using Core.Entities;

namespace Core.Extensions;

public static class DtoExtensions
{


    public static ProductDto ToDto(this Product product, bool canReview = false)
    {
        string? couponDisplay = null;
        decimal? discountedPrice = null;
        Coupon? firstValidCoupon = null;

        if (product.Coupons.Any())
        {
            var now = DateTime.UtcNow;
            // Find the first active, valid coupon associated with this product
            firstValidCoupon = product.Coupons
                .Select(cp => cp.Coupon)
                .FirstOrDefault(c => c.IsActive &&
                               (!c.ValidFrom.HasValue || c.ValidFrom.Value <= now) &&
                               (!c.ValidUntil.HasValue || c.ValidUntil.Value >= now) &&
                               (!c.UsageLimit.HasValue || c.UsageCount < c.UsageLimit.Value));

            if (firstValidCoupon != null)
            {
                if (firstValidCoupon.AmountOff.HasValue)
                {
                    couponDisplay = $"Save {firstValidCoupon.AmountOff.Value:C0} with code: {firstValidCoupon.Code}";
                }
                else if (firstValidCoupon.PercentOff.HasValue)
                {
                    couponDisplay = $"{firstValidCoupon.PercentOff.Value}% off with code: {firstValidCoupon.Code}";
                }

                // --- ADD THIS CALCULATION ---
                if (firstValidCoupon.AmountOff.HasValue)
                {
                    discountedPrice = product.Price - firstValidCoupon.AmountOff.Value;
                }
                else if (firstValidCoupon.PercentOff.HasValue)
                {
                    discountedPrice = product.Price * (1 - (firstValidCoupon.PercentOff.Value / 100));
                }
                // Ensure the price doesn't go below zero
                if (discountedPrice < 0) discountedPrice = 0;
            }
        }
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price, // This is now the base/display price
            PictureUrl = product.PictureUrl, // This uses your smart helper property
                                             // Type = product.Type,
                                             // Brand = product.Brand,
            ProductType = product.ProductType.Name,
            ProductBrand = product.ProductBrand.Name,
            ProductCategory = product.Category.Name,
            AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = product.Reviews.Count,
            AvailableCoupon = couponDisplay,
            DiscountedPrice = discountedPrice,


            QuantityInStock = product.ProductKind == ProductKind.Simple
            ? product.QuantityInStock
            : product.Variants.Sum(v => v.QuantityInStock),

            //  Map variant data to DTOs
            ProductKind = product.ProductKind,
            Options = product.Options?.Select(o => new ProductOptionDto
            {
                Id = o.Id,
                Name = o.Name,
                Values = o.Values?.Select(ov => new ProductOptionValueDto
                {
                    Id = ov.Id,
                    Name = ov.Name,
                    ColorHex = ov.ColorHex
                }).ToList() ?? []
            }).ToList() ?? [],
            Variants = product.Variants?.Select(v =>
            {
                decimal? variantDiscountedPrice = null;
                if (firstValidCoupon != null) // Check if a valid coupon exists
                {
                    if (firstValidCoupon.AmountOff.HasValue)
                    {
                        variantDiscountedPrice = v.Price - firstValidCoupon.AmountOff.Value;
                    }
                    else if (firstValidCoupon.PercentOff.HasValue)
                    {
                        variantDiscountedPrice = v.Price * (1 - (firstValidCoupon.PercentOff.Value / 100));
                    }
                    if (variantDiscountedPrice < 0) variantDiscountedPrice = 0;
                }

                return new ProductVariantDto
                {
                    Id = v.Id,
                    Price = v.Price,
                    QuantityInStock = v.QuantityInStock,
                    ImageId = v.ImageId,
                    OptionValueIds = v.OptionValues?.Select(ov => ov.Id).ToList() ?? [],
                    DiscountedPrice = variantDiscountedPrice // Assign the calculated discount price
                };
            }).ToList() ?? [],

            Images = product.Images.Select(img => new ProductImageDto
            {
                Id = img.Id,
                Url = img.Url,
                IsMain = img.IsMain
            }).ToList(),
            Reviews = product.Reviews?.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerName = r.ReviewerName,
                ReviewDate = r.ReviewDate
            }).OrderByDescending(r => r.ReviewDate).ToList() ?? [],
            CanUserReview = canReview
        };
    }
}