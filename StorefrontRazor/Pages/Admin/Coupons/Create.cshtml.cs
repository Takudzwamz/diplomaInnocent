using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StorefrontRazor.Pages.Admin.Coupons;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public CouponFormViewModel CouponVM { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["Title"] = "Создать купон";
        // We need to load all products to populate the dropdown on the form
        CouponVM.AllProducts = (await _unitOfWork.Repository<Product>().ListAllAsync())
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            .ToList();
            
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Custom validation from the ViewModel
        var coupon = CouponVM.Coupon;
        if (coupon.AmountOff.HasValue && coupon.PercentOff.HasValue)
        {
            ModelState.AddModelError("", "Купон не может иметь одновременно фиксированную скидку и процентную скидку.");
        }
        if (!coupon.AmountOff.HasValue && !coupon.PercentOff.HasValue)
        {
            ModelState.AddModelError("", "Купон должен иметь либо фиксированную скидку, либо процентную скидку.");
        }
        
        if (!ModelState.IsValid)
        {
            // If validation fails, reload the products for the dropdown
            CouponVM.AllProducts = (await _unitOfWork.Repository<Product>().ListAllAsync())
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
            return Page();
        }

        // Add the selected product links to the new coupon entity
        if (CouponVM.SelectedProductIds.Any())
        {
            coupon.ApplicableProducts = CouponVM.SelectedProductIds
                .Select(productId => new CouponProduct { ProductId = productId })
                .ToList();
        }

        _unitOfWork.Repository<Coupon>().Add(coupon);
        await _unitOfWork.Complete();

        return RedirectToPage("./Index");
    }
}