using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.DeliveryMethods;

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var deliveryMethod = await _context.DeliveryMethods.FindAsync(id);
        if (deliveryMethod == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = deliveryMethod.Id,
            ShortName = deliveryMethod.ShortName,
            DeliveryTime = deliveryMethod.DeliveryTime,
            Description = deliveryMethod.Description,
            Price = deliveryMethod.Price
        };
        
        ViewData["Title"] = $"Edit: {deliveryMethod.ShortName}";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var deliveryMethod = await _context.DeliveryMethods.FindAsync(Input.Id);
        if (deliveryMethod == null)
        {
            return NotFound();
        }

        deliveryMethod.ShortName = Input.ShortName;
        deliveryMethod.DeliveryTime = Input.DeliveryTime;
        deliveryMethod.Description = Input.Description;
        deliveryMethod.Price = Input.Price;

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}