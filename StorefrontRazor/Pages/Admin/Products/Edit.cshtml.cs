using System.Text.Json;
using Core.DTOs;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StorefrontRazor.Pages.Admin.Products;

public class EditModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly IIndexNowService _indexNowService;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IAdminAIService _aiService;

    public EditModel(IUnitOfWork unitOfWork, IImageService imageService, IIndexNowService indexNowService, ISiteSettingsService siteSettings, IAdminAIService aiService)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _indexNowService = indexNowService;
        _siteSettings = siteSettings;
        _aiService = aiService;
    }

    public bool IsAIEnabled => _aiService.IsEnabled;

    public ProductDto Product { get; set; } = null!;

    [BindProperty]
    public UpdateProductDto ProductUpdate { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty]
    public IFormFileCollection NewImages { get; set; } = null!;

    public SelectList Brands { get; set; } = null!;
    public SelectList Types { get; set; } = null!;
    public SelectList Categories { get; set; } = null!;

    public IReadOnlyList<ProductOption> AvailableOptions { get; set; } = null!;
    public List<int> SelectedOptionIds { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, int pageIndex = 1)
    {
        PageIndex = pageIndex;
        ViewData["Title"] = "Редактировать товар";
        var spec = new ProductSpecification(id, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);

        if (product == null) return NotFound();

        Product = product.ToDto();
        ProductUpdate = new UpdateProductDto
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            QuantityInStock = product.QuantityInStock,
            ProductBrandId = product.ProductBrandId,
            ProductTypeId = product.ProductTypeId,
            CategoryId = product.CategoryId,
            ProductKind = product.ProductKind,
        };

        SelectedOptionIds = product.Options.Select(o => o.Id).ToList();

        if (product.ProductKind == ProductKind.Simple)
        {
            ProductUpdate.Price = product.Price;
            ProductUpdate.QuantityInStock = product.QuantityInStock;
        }
        else
        {
            // Serialize existing variants to JSON for the form's hidden input
            // It serializes the existing variants into a JSON string for the JavaScript.
            var variantInputs = product.Variants.Select(v => new VariantInputDto
            {
                Id = v.Id,
                ValueIds = v.OptionValues.Select(ov => ov.Id).ToList(),
                Price = v.Price,
                QuantityInStock = v.QuantityInStock,
                Sku = v.Sku,
                ImageId = v.ImageId
            }).ToList();

            // Serialize with camelCase to match JavaScript standards
            ProductUpdate.VariantsJson = JsonSerializer.Serialize(variantInputs, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        await PopulateDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateDetailsAsync(int id)
    {
        if (ProductUpdate.ProductKind == ProductKind.Simple)
        {
            if (ProductUpdate.Price <= 0) ModelState.AddModelError("ProductUpdate.Price", "Цена должна быть больше 0.");
            if (ProductUpdate.QuantityInStock < 0) ModelState.AddModelError("ProductUpdate.QuantityInStock", "Количество на складе должно быть 0 или больше.");
        }

        // We must re-populate dropdowns in case of a validation error
        if (!ModelState.IsValid)
        {
            // Reload product data for the title and other view elements
            var originalProductSpec = new ProductSpecification(id, withImages: true);
            var originalProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpec(originalProductSpec);
            if (originalProduct == null) return NotFound();
            Product = originalProduct.ToDto();

            await PopulateDropdownsAsync();
            return Page();
        }

        var spec = new ProductSpecification(id); // Spec now includes Variants
        var productFromDb = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
        if (productFromDb is null) return NotFound();

        // Update shared properties
        productFromDb.Name = ProductUpdate.Name;
        productFromDb.Description = ProductUpdate.Description;
        productFromDb.ProductBrandId = ProductUpdate.ProductBrandId;
        productFromDb.ProductTypeId = ProductUpdate.ProductTypeId;
        productFromDb.CategoryId = ProductUpdate.CategoryId;

        if (productFromDb.ProductKind == ProductKind.Simple)
        {
            productFromDb.Price = ProductUpdate.Price;
            productFromDb.QuantityInStock = ProductUpdate.QuantityInStock;
        }
        else // Handle Variant Updates
        {
            if (string.IsNullOrEmpty(ProductUpdate.VariantsJson))
            {
                ModelState.AddModelError("ProductUpdate.VariantsJson", "Варианты не могут быть пустыми для товара с вариантами.");
                await PopulateDropdownsAsync();
                return Page();
            }

            var variantInputs = JsonSerializer.Deserialize<List<VariantInputDto>>(ProductUpdate.VariantsJson);
            if (variantInputs == null || !variantInputs.Any())
            {
                ModelState.AddModelError("ProductUpdate.VariantsJson", "Варианты не были настроены.");
                await PopulateDropdownsAsync();
                return Page();
            }

            var variantRepo = _unitOfWork.Repository<ProductVariant>();
            var existingVariantIds = productFromDb.Variants.Select(v => v.Id).ToList();
            var incomingVariantIds = variantInputs.Select(v => v.Id).Where(vId => vId != 0).ToList();

            // 1. Delete variants that are no longer in the submitted form
            var variantsToDelete = productFromDb.Variants.Where(v => !incomingVariantIds.Contains(v.Id)).ToList();
            foreach (var variant in variantsToDelete)
            {
                variantRepo.Remove(variant);
            }

            // 2. Update existing variants and add new ones
            var allValueIds = variantInputs.SelectMany(v => v.ValueIds).Distinct().ToList();
            var allOptionValues = await _unitOfWork.Repository<ProductOptionValue>().ListAsync(new BaseSpecification<ProductOptionValue>(v => allValueIds.Contains(v.Id)));
            var valuesDict = allOptionValues.ToDictionary(v => v.Id);

            foreach (var variantInput in variantInputs)
            {
                if (variantInput.Id != 0 && existingVariantIds.Contains(variantInput.Id))
                {
                    // Update existing variant
                    var variantToUpdate = productFromDb.Variants.First(v => v.Id == variantInput.Id);
                    variantToUpdate.Price = variantInput.Price;
                    variantToUpdate.QuantityInStock = variantInput.QuantityInStock;
                    variantToUpdate.Sku = variantInput.Sku;
                    variantToUpdate.ImageId = variantInput.ImageId;
                }
                else
                {
                    // Add new variant (from regeneration)
                    productFromDb.Variants.Add(new ProductVariant
                    {
                        Price = variantInput.Price,
                        QuantityInStock = variantInput.QuantityInStock,
                        Sku = variantInput.Sku,
                        OptionValues = variantInput.ValueIds.Select(valId => valuesDict[valId]).ToList(),
                        ImageId = variantInput.ImageId
                    });
                }
            }

            productFromDb.Price = variantInputs.Min(v => v.Price);
        }

        _unitOfWork.Repository<Product>().Update(productFromDb);
        await _unitOfWork.Complete();

        var settings = await _siteSettings.GetSettingsAsync();
        var publicUrl = settings.GetValueOrDefault("PublicUrl")?.TrimEnd('/');
        if (!string.IsNullOrEmpty(publicUrl))
        {
            var productUrl = $"{publicUrl}/Products/{productFromDb.Id}";
            await _indexNowService.SubmitUrlsAsync(new List<string> { productUrl });
        }

        return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
    }

    public async Task<IActionResult> OnPostUploadImagesAsync(int id)
    {
        var spec = new ProductSpecification(id, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
        if (product == null) return NotFound();

        foreach (var file in NewImages)
        {
            var imageUrl = await _imageService.AddImageAsync(file);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var image = new ProductImage { Url = imageUrl };
                if (!product.Images.Any(i => i.IsMain)) image.IsMain = true;
                product.Images.Add(image);
            }
        }
        await _unitOfWork.Complete();
        return RedirectToPage(new { id = id, PageIndex = this.PageIndex });
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(int id, int imageId)
    {
        var spec = new ProductSpecification(id, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
        if (product == null) return NotFound();

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null) return NotFound("Image not found");

        await _imageService.DeleteImageAsync(image.Url);
        product.Images.Remove(image);

        if (image.IsMain && product.Images.Any())
        {
            product.Images.First().IsMain = true;
        }

        await _unitOfWork.Complete();
        return RedirectToPage(new { id = id, PageIndex = this.PageIndex });
    }

    public async Task<IActionResult> OnPostSetMainImageAsync(int id, int imageId)
    {
        var spec = new ProductSpecification(id, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
        if (product == null) return NotFound();

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null) return NotFound("Image not found");

        var currentMain = product.Images.FirstOrDefault(i => i.IsMain);
        if (currentMain != null) currentMain.IsMain = false;

        image.IsMain = true;
        await _unitOfWork.Complete();
        return RedirectToPage(new { id = id, PageIndex = this.PageIndex });
    }

    private async Task PopulateDropdownsAsync()
    {
        // --- UPDATED to query the new tables directly ---
        var brands = await _unitOfWork.Repository<ProductBrand>().ListAllAsync();
        var types = await _unitOfWork.Repository<ProductType>().ListAllAsync();
        var categories = await _unitOfWork.Repository<Category>().ListAllAsync();

        var optionsSpec = new BaseSpecification<ProductOption>();
        optionsSpec.AddInclude(o => o.Values);
        AvailableOptions = await _unitOfWork.Repository<ProductOption>().ListAsync(optionsSpec);

        // --- UPDATED to create the SelectList with ID and Name ---
        Brands = new SelectList(brands, "Id", "Name");
        Types = new SelectList(types, "Id", "Name");
        Categories = new SelectList(categories, "Id", "Name");
    }

    // AI Description Generator Handler
    public async Task<IActionResult> OnGetGenerateDescriptionAsync(int id, string tone = "Professional")
    {
        if (!_aiService.IsEnabled)
            return new JsonResult(new { error = "AI service is not enabled" });

        var spec = new ProductSpecification(id, withImages: true);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
        
        if (product == null)
            return new JsonResult(new { error = "Product not found" });

        var request = new ProductDescriptionRequest
        {
            ProductName = product.Name,
            Brand = product.ProductBrand?.Name,
            Category = product.Category?.Name,
            Type = product.ProductType?.Name,
            Price = product.Price,
            ExistingDescription = product.Description,
            Tone = tone
        };

        var result = await _aiService.GenerateProductDescriptionAsync(request);
        
        if (result == null)
            return new JsonResult(new { error = "Failed to generate description" });

        return new JsonResult(result);
    }
}
