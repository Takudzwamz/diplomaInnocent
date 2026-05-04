using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Types;

public class IndexModel : PageModel
{
    private readonly StoreContext _context;

    public IndexModel(StoreContext context)
    {
        _context = context;
    }

    public IList<ProductType> Types { get; set; } = new List<ProductType>();
    
    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Types = await _context.ProductTypes.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var type = await _context.ProductTypes.FindAsync(id);

        if (type == null)
        {
            return NotFound();
        }

        // Safety Check: Prevent deletion if the type is in use
        var isTypeInUse = await _context.Products.AnyAsync(p => p.ProductTypeId == id);
        if (isTypeInUse)
        {
            ErrorMessage = $"Нельзя удалить тип '{type.Name}' так как он привязан к одному или нескольким товарам.";
            return RedirectToPage();
        }

        _context.ProductTypes.Remove(type);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}