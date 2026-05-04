using Core.Entities;
using Core.Entities.OrderAggregate;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.DeliveryMethods;

public class IndexModel : PageModel
{
    private readonly StoreContext _context;

    public IndexModel(StoreContext context)
    {
        _context = context;
    }

    public IList<DeliveryMethod> DeliveryMethods { get; set; } = new List<DeliveryMethod>();
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        DeliveryMethods = await _context.DeliveryMethods.OrderBy(d => d.Price).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deliveryMethod = await _context.DeliveryMethods.FindAsync(id);

        if (deliveryMethod == null)
        {
            return NotFound();
        }

        // Safety Check: Prevent deletion if the method is used in any orders.
        var isMethodInUse = await _context.Orders.AnyAsync(o => o.DeliveryMethodId == id);
        if (isMethodInUse)
        {
            ErrorMessage = $"Нельзя удалить '{deliveryMethod.ShortName}' так как он связан с существующими заказами.";
            return RedirectToPage();
        }

        _context.DeliveryMethods.Remove(deliveryMethod);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}