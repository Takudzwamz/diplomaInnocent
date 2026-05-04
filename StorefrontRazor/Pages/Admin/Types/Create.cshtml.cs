using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Types;

public class CreateModel : PageModel
{
    private readonly StoreContext _context;

    public CreateModel(StoreContext context)
    {
        _context = context;
    }

    [BindProperty]
    public TypeInputModel Type { get; set; } = null!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var newType = new ProductType { Name = Type.Name };

        _context.ProductTypes.Add(newType);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    public class TypeInputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}