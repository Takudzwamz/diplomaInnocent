using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // <-- Add this
using Microsoft.EntityFrameworkCore; // <-- Add this

namespace StorefrontRazor.Pages.Admin.Coupons;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Infrastructure.Data.StoreContext _context;


    public EditModel(IUnitOfWork unitOfWork, Infrastructure.Data.StoreContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [BindProperty]
    public CouponFormViewModel CouponVM { get; set; } = new();

     public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Редактировать купон";
        
        var coupon = await _context.Coupons
            .Include(c => c.ApplicableProducts)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (coupon == null)
        {
            return NotFound();
        }

        var allProducts = (await _unitOfWork.Repository<Product>().ListAllAsync())
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            .ToList();
            
        // Populate the ViewModel with all the data for the form
        CouponVM = new CouponFormViewModel
        {
            Coupon = coupon,
            SelectedProductIds = coupon.ApplicableProducts.Select(cp => cp.ProductId).ToList(),
            AllProducts = allProducts
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // If validation fails, we need to reload the products for the dropdown
            CouponVM.AllProducts = (await _unitOfWork.Repository<Product>().ListAllAsync())
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
            return Page();
        }

        var couponToUpdate = await _context.Coupons
            .Include(c => c.ApplicableProducts)
            .FirstOrDefaultAsync(c => c.Id == CouponVM.Coupon.Id);

        if (couponToUpdate == null) return NotFound();

        // Update the main coupon properties
        _context.Entry(couponToUpdate).CurrentValues.SetValues(CouponVM.Coupon);
        
        // Clear the existing product links
        couponToUpdate.ApplicableProducts.Clear();
        
        // Add the new product links based on the form submission
        if (CouponVM.SelectedProductIds.Any())
        {
            foreach (var productId in CouponVM.SelectedProductIds)
            {
                couponToUpdate.ApplicableProducts.Add(new CouponProduct { ProductId = productId });
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}