using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace StorefrontRazor.Pages.Admin.Hero;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    public CreateModel(IUnitOfWork unitOfWork, IImageService imageService)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    [BindProperty]
    public HeroSlide Slide { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageUpload { get; set; } // <-- THE FIX: Make this property nullable

    public void OnGet() { ViewData["Title"] = "Add New Slide"; }

    public async Task<IActionResult> OnPostAsync()
    {
        // Our primary validation check is now here.
        if (ImageUpload == null)
        {
            ModelState.AddModelError("ImageUpload", "Пожалуйста, выберите фоновое изображение.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // We know ImageUpload is not null here, so we can use the ! operator.
        var imageUrl = await _imageService.AddImageAsync(ImageUpload!);
        if (imageUrl != null)
        {
            Slide.ImageUrl = imageUrl;
        }
        else
        {
            // This handles a failure from the image service itself.
            ModelState.AddModelError("", "Произошла ошибка при загрузке изображения.");
            return Page();
        }

        _unitOfWork.Repository<HeroSlide>().Add(Slide);
        await _unitOfWork.Complete();
        return RedirectToPage("./Index");
    }
}