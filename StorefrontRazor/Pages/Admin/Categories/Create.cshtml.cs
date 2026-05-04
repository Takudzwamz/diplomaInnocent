using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Categories;

public class CreateModel : PageModel
{
    private readonly StoreContext _context;

    public CreateModel(StoreContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = new Category
        {
            Name = Input.Name,
            Description = Input.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}