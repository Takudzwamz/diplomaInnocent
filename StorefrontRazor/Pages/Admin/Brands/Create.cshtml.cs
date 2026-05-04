using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Brands;

public class CreateModel : PageModel
{
    private readonly StoreContext _context;

    public CreateModel(StoreContext context)
    {
        _context = context;
    }

    [BindProperty]
    public BrandInputModel Brand { get; set; } = null!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var newBrand = new ProductBrand { Name = Brand.Name };

        _context.ProductBrands.Add(newBrand);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    public class BrandInputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}