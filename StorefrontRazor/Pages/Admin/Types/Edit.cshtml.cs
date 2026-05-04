using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Types;

public class EditModel : PageModel
{
    private readonly StoreContext _context;

    public EditModel(StoreContext context)
    {
        _context = context;
    }

    [BindProperty]
    public TypeInputModel Type { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var typeFromDb = await _context.ProductTypes.FindAsync(id);
        if (typeFromDb == null)
        {
            return NotFound();
        }

        Type = new TypeInputModel { Id = typeFromDb.Id, Name = typeFromDb.Name };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var typeToUpdate = await _context.ProductTypes.FindAsync(Type.Id);
        if (typeToUpdate == null)
        {
            return NotFound();
        }

        typeToUpdate.Name = Type.Name;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.ProductTypes.Any(e => e.Id == Type.Id))
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

    public class TypeInputModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
    }
}