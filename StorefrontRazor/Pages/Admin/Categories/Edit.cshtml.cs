using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Categories;

public class EditModel : PageModel
{
    private readonly StoreContext _context;

    public EditModel(StoreContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await _context.Categories.FindAsync(Input.Id);
        if (category == null)
        {
            return NotFound();
        }

        category.Name = Input.Name;
        category.Description = Input.Description;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}