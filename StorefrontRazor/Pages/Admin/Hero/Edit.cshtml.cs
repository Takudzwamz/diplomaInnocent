using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Hero;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly Infrastructure.Data.StoreContext _context;

    public EditModel(IUnitOfWork unitOfWork, IImageService imageService, Infrastructure.Data.StoreContext context)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _context = context;
    }

    [BindProperty]
    public HeroSlide Slide { get; set; } = default!;

    [BindProperty]
    public IFormFile? ImageUpload { get; set; } // Image upload is now optional

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Edit Slide";
        var slide = await _unitOfWork.Repository<HeroSlide>().GetByIdAsync(id);
        if (slide == null)
        {
            return NotFound();
        }
        Slide = slide;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var slideToUpdate = await _unitOfWork.Repository<HeroSlide>().GetByIdAsync(Slide.Id);
        if (slideToUpdate == null) return NotFound();

        // Check if a new image was uploaded
        if (ImageUpload != null)
        {
            var oldImageUrl = slideToUpdate.ImageUrl;
            
            // Upload the new image
            var newImageUrl = await _imageService.AddImageAsync(ImageUpload);
            if (newImageUrl != null)
            {
                slideToUpdate.ImageUrl = newImageUrl;
                // Delete the old image from Cloudinary to save space
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await _imageService.DeleteImageAsync(oldImageUrl);
                }
            }
        }
        
        // Update the rest of the properties
        slideToUpdate.Title = Slide.Title;
        slideToUpdate.Subtext = Slide.Subtext;
        slideToUpdate.ButtonLink = Slide.ButtonLink;
        slideToUpdate.DisplayOrder = Slide.DisplayOrder;
        slideToUpdate.IsActive = Slide.IsActive;

        _unitOfWork.Repository<HeroSlide>().Update(slideToUpdate);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index");
    }
}