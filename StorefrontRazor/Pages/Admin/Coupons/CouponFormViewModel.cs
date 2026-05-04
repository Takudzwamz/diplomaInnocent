using Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StorefrontRazor.Pages.Admin.Coupons;

public class CouponFormViewModel
{
    public Coupon Coupon { get; set; } = new();
    public List<SelectListItem> AllProducts { get; set; } = [];
    public List<int> SelectedProductIds { get; set; } = [];
}