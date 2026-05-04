using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly StoreContext _context;

    public IndexModel(StoreContext context)
    {
        _context = context;
    }

    public IList<Category> Categories { get; set; } = new List<Category>();
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        // Safety Check: Prevent deletion if the category is used by any products.
        var isCategoryInUse = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (isCategoryInUse)
        {
            ErrorMessage = $"Нельзя удалить категорию '{category.Name}' так как она привязана к одному или нескольким товарам.";
            return RedirectToPage();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}