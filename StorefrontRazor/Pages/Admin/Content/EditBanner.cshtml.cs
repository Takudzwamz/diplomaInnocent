using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Content;

public class EditBannerModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public EditBannerModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public BannerInputModel BannerInput { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Edit Promotional Banner";
        // We can pre-populate the form by parsing the existing HTML,
        // but for simplicity, we'll present a fresh form.
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Find the content block for the home page banner
        var spec = new BaseSpecification<ContentBlock>(cb => cb.Key == "home-page-promo-html");
        var bannerBlock = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(spec);

        if (bannerBlock == null)
        {
            // This shouldn't happen if the data was seeded, but it's a good safeguard.
            return NotFound("The 'home-page-promo-html' content block was not found.");
        }

        // --- Build the final HTML from the user's simple inputs ---
        bannerBlock.Content = $@"<div class='alert text-center border-0 shadow-sm rounded-3 py-3 mb-4' role='alert' style='background: var(--bs-info-bg-subtle); color: var(--bs-info-text-emphasis); font-size: 1.1rem;'>
            🎉 Use code <strong class='text-primary'>{BannerInput.CouponCode}</strong> and get <span class='fw-bold text-success'>{BannerInput.DiscountValue}</span> {BannerInput.MainText}
        </div>";

        _unitOfWork.Repository<ContentBlock>().Update(bannerBlock);
        await _unitOfWork.Complete();

        return RedirectToPage("./Index");
    }
}