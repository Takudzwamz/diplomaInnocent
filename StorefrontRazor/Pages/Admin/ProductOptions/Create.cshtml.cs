using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.ProductOptions;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public CreateOrUpdateOptionDto NewOption { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var option = new ProductOption { Name = NewOption.Name };

        if (!string.IsNullOrWhiteSpace(NewOption.Values))
        {
            // Parse values - format can be "Name" or "Name|#HEXCOLOR"
            var values = NewOption.Values.Split(',')
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .Select(v => {
                    var parts = v.Split('|');
                    var name = parts[0].Trim();
                    var colorHex = parts.Length > 1 ? parts[1].Trim() : null;
                    
                    // Validate hex color format if provided
                    if (!string.IsNullOrEmpty(colorHex) && !System.Text.RegularExpressions.Regex.IsMatch(colorHex, @"^#[0-9A-Fa-f]{6}$"))
                    {
                        colorHex = null; // Invalid format, ignore
                    }
                    
                    return new ProductOptionValue 
                    { 
                        Name = name, 
                        ColorHex = colorHex,
                        ProductOption = option 
                    };
                });
            
            option.Values.AddRange(values);
        }

        _unitOfWork.Repository<ProductOption>().Add(option);
        await _unitOfWork.Complete();

        return RedirectToPage("./Index");
    }
}