using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Coupons;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IReadOnlyList<Coupon> Coupons { get; set; } = new List<Coupon>();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Управление купонами";
        Coupons = await _unitOfWork.Repository<Coupon>().ListAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
        if (coupon == null)
        {
            return NotFound();
        }

        _unitOfWork.Repository<Coupon>().Remove(coupon);
        await _unitOfWork.Complete();

        return RedirectToPage();
    }

    // Helper to format the discount value for display
    public string GetDiscountValue(Coupon coupon)
    {
        if (coupon.PercentOff.HasValue)
        {
            return $"{coupon.PercentOff.Value}% off";
        }
        if (coupon.AmountOff.HasValue)
        {
            return coupon.AmountOff.Value.ToString("C");
        }
        return "N/A";
    }
}