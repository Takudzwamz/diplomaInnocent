using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.ProductOptions;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public EditModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public CreateOrUpdateOptionDto OptionToUpdate { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var spec = new BaseSpecification<ProductOption>(o => o.Id == id);
        spec.AddInclude(o => o.Values);

        var option = await _unitOfWork.Repository<ProductOption>().GetEntityWithSpec(spec);

        if (option == null)
        {
            return NotFound();
        }

        OptionToUpdate.Name = option.Name;
        // Format values as "Name|ColorHex" or just "Name"
        OptionToUpdate.Values = string.Join(", ", option.Values.Select(v => 
            string.IsNullOrEmpty(v.ColorHex) ? v.Name : $"{v.Name}|{v.ColorHex}"));

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var spec = new BaseSpecification<ProductOption>(o => o.Id == id);
        spec.AddInclude(o => o.Values);
        var optionFromDb = await _unitOfWork.Repository<ProductOption>().GetEntityWithSpec(spec);

        if (optionFromDb == null)
        {
            return NotFound();
        }

        optionFromDb.Name = OptionToUpdate.Name;
        
        // Parse new values - format can be "Name" or "Name|#HEXCOLOR"
        var newValues = (OptionToUpdate.Values ?? string.Empty)
            .Split(',')
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v => {
                var parts = v.Split('|');
                var name = parts[0].Trim();
                var colorHex = parts.Length > 1 ? parts[1].Trim() : null;
                
                // Validate hex color format if provided
                if (!string.IsNullOrEmpty(colorHex) && !System.Text.RegularExpressions.Regex.IsMatch(colorHex, @"^#[0-9A-Fa-f]{6}$"))
                {
                    colorHex = null;
                }
                
                return new { Name = name, ColorHex = colorHex };
            })
            .ToList();
            
        // Remove values that are no longer in the list (by name)
        var newValueNames = newValues.Select(v => v.Name).ToHashSet();
        var valuesToRemove = optionFromDb.Values.Where(v => !newValueNames.Contains(v.Name)).ToList();
        foreach (var value in valuesToRemove)
        {
            optionFromDb.Values.Remove(value);
        }

        // Update existing values and add new ones
        foreach (var newValue in newValues)
        {
            var existingValue = optionFromDb.Values.FirstOrDefault(v => v.Name == newValue.Name);
            if (existingValue != null)
            {
                // Update color hex for existing value
                existingValue.ColorHex = newValue.ColorHex;
            }
            else
            {
                // Add new value
                optionFromDb.Values.Add(new ProductOptionValue 
                { 
                    Name = newValue.Name, 
                    ColorHex = newValue.ColorHex 
                });
            }
        }

        _unitOfWork.Repository<ProductOption>().Update(optionFromDb);
        await _unitOfWork.Complete();
        
        return RedirectToPage("./Index");
    }
}