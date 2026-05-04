using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.DeliveryMethods;

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
        [Display(Name = "Краткое название")]
        public string ShortName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Срок доставки")]
        public string DeliveryTime { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 1000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }

    public void OnGet()
    {
        ViewData["Title"] = "Create New Delivery Method";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var deliveryMethod = new DeliveryMethod
        {
            ShortName = Input.ShortName,
            DeliveryTime = Input.DeliveryTime,
            Description = Input.Description,
            Price = Input.Price
        };

        _context.DeliveryMethods.Add(deliveryMethod);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}