using System.Text.Json;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Core.Paging;

namespace StorefrontRazor.Pages.Admin.Products;

public class CreateModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
     private readonly IIndexNowService _indexNowService; 
    private readonly ISiteSettingsService _siteSettings; 

    public CreateModel(IUnitOfWork unitOfWork, IImageService imageService, IIndexNowService indexNowService, ISiteSettingsService siteSettings)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _indexNowService = indexNowService;
        _siteSettings = siteSettings;
    }

    [BindProperty]
    public CreateProductDto NewProduct { get; set; } = new();

    [BindProperty]
    public IFormFileCollection? UploadedImages { get; set; }

    public SelectList? Brands { get; set; }
    public SelectList? Types { get; set; }
    public SelectList? Categories { get; set; }

    public IReadOnlyList<ProductOption> AvailableOptions { get; set; } = null!;

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Создать товар";
        await PopulateDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewProduct.ProductKind == ProductKind.Simple)
        {
            if (NewProduct.Price <= 0) ModelState.AddModelError("NewProduct.Price", "Цена должна быть больше 0.");
            if (NewProduct.QuantityInStock <= 0) ModelState.AddModelError("NewProduct.QuantityInStock", "Количество на складе должно быть не менее 1.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        // --- FIX: Only initialize properties common to BOTH product kinds ---
        var product = new Product
        {
            Name = NewProduct.Name,
            Description = NewProduct.Description,
            ProductBrandId = NewProduct.ProductBrandId,
            ProductTypeId = NewProduct.ProductTypeId,
            CategoryId = NewProduct.CategoryId,
            ProductKind = NewProduct.ProductKind
        };

        if (NewProduct.ProductKind == ProductKind.Simple)
        {
            // Now, correctly set the price and stock only for simple products
            product.Price = NewProduct.Price;
            product.QuantityInStock = NewProduct.QuantityInStock;
        }
        else // Variable Product Logic
        {
            // FIX: Use case-insensitive deserialization for robustness
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var variantInputs = JsonSerializer.Deserialize<List<VariantInputDto>>(NewProduct.VariantsJson, options);

            if (variantInputs == null || !variantInputs.Any())
            {
                ModelState.AddModelError("NewProduct.VariantsJson", "Варианты не были настроены.");
                await PopulateDropdownsAsync();
                return Page();
            }

            product.Price = variantInputs.Min(v => v.Price);
            // Parent product's stock is the sum of its variants, not set directly.
            // product.QuantityInStock is intentionally left as 0.

            var allValueIds = variantInputs.SelectMany(v => v.ValueIds).Distinct().ToList();
            var valueRepo = _unitOfWork.Repository<ProductOptionValue>();
            var valuesSpec = new BaseSpecification<ProductOptionValue>(v => allValueIds.Contains(v.Id));
            var allOptionValues = await valueRepo.ListAsync(valuesSpec);
            var valuesDict = allOptionValues.ToDictionary(v => v.Id);

            var parentOptionIds = allOptionValues.Select(v => v.ProductOptionId).Distinct().ToList();
            var optionsRepo = _unitOfWork.Repository<ProductOption>();
            var parentOptionsSpec = new BaseSpecification<ProductOption>(o => parentOptionIds.Contains(o.Id));
            var parentOptions = await optionsRepo.ListAsync(parentOptionsSpec);
            
            product.Options = parentOptions.ToList();

            foreach (var variantInput in variantInputs)
            {
                product.Variants.Add(new ProductVariant
                {
                    Price = variantInput.Price,
                    QuantityInStock = variantInput.QuantityInStock,
                    Sku = variantInput.Sku,
                    OptionValues = variantInput.ValueIds.Select(id => valuesDict[id]).ToList()
                });
            }
        }

        // Handle image uploads
        if (UploadedImages != null && UploadedImages.Any())
        {
            foreach (var file in UploadedImages)
            {
                var imageUrl = await _imageService.AddImageAsync(file);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    product.Images.Add(new ProductImage { Url = imageUrl });
                }
            }
            // Set the first uploaded image as the main one
            if (product.Images.Any())
            {
                product.Images.First().IsMain = true;
            }
        }

        _unitOfWork.Repository<Product>().Add(product);
        await _unitOfWork.Complete();

        var settings = await _siteSettings.GetSettingsAsync();
        var publicUrl = settings.GetValueOrDefault("PublicUrl")?.TrimEnd('/');
        if (!string.IsNullOrEmpty(publicUrl))
        {
            var productUrl = $"{publicUrl}/Products/{product.Id}";
            await _indexNowService.SubmitUrlsAsync(new List<string> { productUrl });
        }

        // 1. Get the total count of *all* products now
        var countSpec = new ProductWithFiltersForCountSpecification(new ProductSpecParams());
        var totalItems = await _unitOfWork.Repository<Product>().CountAsync(countSpec);
        
        // 2. Define the page size (must match your Index page's PageSize)
        var pageSize = 10; 
        
        // 3. Calculate the last page
        // This tells us which page the *newest* item will be on if
        // your products are sorted by ID or Name in ascending order.
        var lastPageIndex = (int)Math.Ceiling(totalItems / (double)pageSize);

        // 4. Redirect to the Index page, passing the last page index
        return RedirectToPage("./Index", new { PageIndex = lastPageIndex });

        // return RedirectToPage("./Index");
    }

    private async Task PopulateDropdownsAsync()
    {
        // --- UPDATED to query the new tables directly ---
        var brands = await _unitOfWork.Repository<ProductBrand>().ListAllAsync();
        var types = await _unitOfWork.Repository<ProductType>().ListAllAsync();
        var categories = await _unitOfWork.Repository<Category>().ListAllAsync();
        var optionsSpec = new BaseSpecification<ProductOption>();
        // 2. Then, call the AddInclude method
        optionsSpec.AddInclude(o => o.Values);
        AvailableOptions = await _unitOfWork.Repository<ProductOption>().ListAsync(optionsSpec);
        // --- UPDATED to create the SelectList with ID and Name ---
        Brands = new SelectList(brands, "Id", "Name");
        Types = new SelectList(types, "Id", "Name");
        Categories = new SelectList(categories, "Id", "Name");
    }
}


