using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Brands;

public class EditModel : PageModel
{
    private readonly StoreContext _context;

    public EditModel(StoreContext context)
    {
        _context = context;
    }

    [BindProperty]
    public BrandInputModel Brand { get; set; } = new BrandInputModel();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var brandFromDb = await _context.ProductBrands.FindAsync(id);
        if (brandFromDb == null)
        {
            return NotFound();
        }

        Brand = new BrandInputModel { Id = brandFromDb.Id, Name = brandFromDb.Name };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var brandToUpdate = await _context.ProductBrands.FindAsync(Brand.Id);
        if (brandToUpdate == null)
        {
            return NotFound();
        }

        brandToUpdate.Name = Brand.Name;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.ProductBrands.Any(e => e.Id == Brand.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    public class BrandInputModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}