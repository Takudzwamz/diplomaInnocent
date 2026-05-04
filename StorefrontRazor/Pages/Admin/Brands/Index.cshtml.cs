using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Brands;

public class IndexModel : PageModel
{
    private readonly StoreContext _context;

    public IndexModel(StoreContext context)
    {
        _context = context;
    }

    public IList<ProductBrand> Brands { get; set; } = new List<ProductBrand>();
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Brands = await _context.ProductBrands.OrderBy(b => b.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var brand = await _context.ProductBrands.FindAsync(id);

        if (brand == null)
        {
            return NotFound();
        }

        // Safety Check: Prevent deletion if the brand is in use
        var isBrandInUse = await _context.Products.AnyAsync(p => p.ProductBrandId == id);
        if (isBrandInUse)
        {
            ErrorMessage = $"Нельзя удалить бренд '{brand.Name}' так как он привязан к одному или нескольким товарам.";
            return RedirectToPage();
        }

        _context.ProductBrands.Remove(brand);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}