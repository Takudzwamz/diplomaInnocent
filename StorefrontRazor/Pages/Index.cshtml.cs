using Core.DTOs;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StorefrontRazor.Pages;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartService _cartService;
    private readonly IWishlistService _wishlistService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IAIRecommendationService _aiRecommendationService;
    private readonly IAdaptiveRecommendationService _adaptiveRecommendationService;
    private readonly UserManager<AppUser> _userManager;
    public List<HeroSlide> HeroSlides { get; set; } = [];

    public List<ProductDto> NewArrivals { get; set; } = new();
    public List<ProductDto> BestSellers { get; set; } = new();
    public List<ProductDto> PersonalizedRecommendations { get; set; } = new();
    public IReadOnlyList<Category> Categories { get; set; } = new List<Category>();
    public IReadOnlyList<ProductBrand> Brands { get; set; } = new List<ProductBrand>();

    public string? PromoHtmlContent { get; set; }

    public ShoppingCart Cart { get; set; } = null!;

    public bool ShowShopByBrand { get; set; }
    public bool ShowShopByCategory { get; set; }
    public bool IsAIEnabled { get; set; }

    public IndexModel(
        IUnitOfWork unitOfWork,
        ICartService cartService,
        IWishlistService wishlistService,
        ISiteSettingsService siteSettings,
        SignInManager<AppUser> signInManager,
        IAIRecommendationService aiRecommendationService,
        IAdaptiveRecommendationService adaptiveRecommendationService,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _cartService = cartService;
        _wishlistService = wishlistService;
        _siteSettings = siteSettings;
        _signInManager = signInManager;
        _aiRecommendationService = aiRecommendationService;
        _adaptiveRecommendationService = adaptiveRecommendationService;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        var settings = await _siteSettings.GetSettingsAsync();
        var storeName = settings.GetValueOrDefault("StoreName", "Devs Store");
        var logoUrl = settings.GetValueOrDefault("StoreLogoUrl", ""); // Use logo as default image

        ViewData["Title"] = $"{storeName} — Качественные товары и аксессуары онлайн";
        ViewData["Description"] = $"Ваш универсальный магазин качественных товаров. {storeName} предлагает широкий ассортимент товаров и аксессуаров. Откройте нашу коллекцию уже сегодня!";
        ViewData["ImageUrl"] = logoUrl;

        var heroSpec = new BaseSpecification<HeroSlide>(h => h.IsActive, h => h.DisplayOrder);
        HeroSlides = (await _unitOfWork.Repository<HeroSlide>().ListAsync(heroSpec)).ToList();
        // --- 1. Get New Arrivals ---
        var specNewest = new ProductSpecification(new ProductSpecParams { PageSize = 4 });
        specNewest.AddOrderByDescending(p => p.Id);
        var newestProducts = await _unitOfWork.Repository<Product>().ListAsync(specNewest);
        NewArrivals = newestProducts.Select(p => p.ToDto()).ToList();

        // --- 2. Get Best Sellers (UPDATED) ---
        var orderItemsRepo = _unitOfWork.Repository<OrderItem>();
        var bestSellingProductIds = await orderItemsRepo.GetQueryable()
            .GroupBy(oi => oi.ItemOrdered.ProductId)
            .OrderByDescending(g => g.Sum(oi => oi.Quantity))
            .Take(4)
            .Select(g => g.Key)
            .ToListAsync();

        if (bestSellingProductIds.Any())
        {
            // Use the new constructor to create the specification
            var specBestSellers = new ProductSpecification(bestSellingProductIds);
            var bestSellingProducts = await _unitOfWork.Repository<Product>().ListAsync(specBestSellers);

            BestSellers = bestSellingProducts
                .OrderBy(p => bestSellingProductIds.IndexOf(p.Id))
                .Select(p => p.ToDto())
                .ToList();
        }

        // --- 3. Get Categories & Brands for display ---
        Categories = await _unitOfWork.Repository<Category>().ListAllAsync();
        Brands = await _unitOfWork.Repository<ProductBrand>().ListAllAsync();

        // --- 4. Get the Cart for the product cards ---
        Cart = await _cartService.GetCartAsync();

        var promoSpec = new BaseSpecification<ContentBlock>(cb => cb.Key == "home-page-promo-html");
        var promoBlock = await _unitOfWork.Repository<ContentBlock>().GetEntityWithSpec(promoSpec);

        if (promoBlock != null && promoBlock.IsHtml)
        {
            PromoHtmlContent = promoBlock.Content;
        }
        else
        {
            PromoHtmlContent = null;
        }

        var settingsRepo = _unitOfWork.Repository<ContentBlock>();
        var brandBlock = await settingsRepo.GetEntityWithSpec(new BaseSpecification<ContentBlock>(cb => cb.Key == "homepage-show-brands"));
        var categoryBlock = await settingsRepo.GetEntityWithSpec(new BaseSpecification<ContentBlock>(cb => cb.Key == "homepage-show-categories"));

        // Convert the string "true" or "false" to a boolean
        ShowShopByBrand = bool.TryParse(brandBlock?.Content, out var showBrand) && showBrand;
        ShowShopByCategory = bool.TryParse(categoryBlock?.Content, out var showCategory) && showCategory;

        // Get AI-powered personalized recommendations for logged-in users
        var aiClientService = HttpContext.RequestServices
            .GetRequiredService<Infrastructure.Services.AzureOpenAIClientService>();
        IsAIEnabled = aiClientService.IsEnabled;

        if (_signInManager.IsSignedIn(User))
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        // Use the adaptive recommendation system (supports A/B testing)
                        var adaptiveProducts = await _adaptiveRecommendationService
                            .GetAdaptiveRecommendationsAsync(user.Id, count: 8);

                        if (adaptiveProducts.Any())
                        {
                            PersonalizedRecommendations = adaptiveProducts
                                .Select(p => p.ToDto())
                                .ToList();
                        }
                        else if (IsAIEnabled)
                        {
                            // Fallback to original AI recommendations if adaptive has no data
                            var orderSpec = new OrderSpecification(email, new OrderSpecParams 
                            { 
                                PageSize = 3,
                                Sort = "dateDesc" 
                            });
                            var orders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);
                            
                            var purchasedProductIds = orders
                                .SelectMany(o => o.OrderItems)
                                .Select(oi => oi.ItemOrdered.ProductId)
                                .Distinct()
                                .Take(10)
                                .ToList();

                            if (purchasedProductIds.Any())
                            {
                                var personalizedProducts = await _aiRecommendationService
                                    .GetPersonalizedRecommendationsAsync(purchasedProductIds, count: 4);
                                
                                PersonalizedRecommendations = personalizedProducts
                                    .Select(p => p.ToDto())
                                    .ToList();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Personalized recommendations are optional - don't fail the page load
            }
        }
    }

    // --- ADDED: Handler for adding items to the cart ---
    public async Task<IActionResult> OnPostAddToCartJsonAsync(int productId)
    {
        var cart = await _cartService.AddItemToCartAsync(productId, null, 1);
        return new JsonResult(new { itemCount = cart.Items.Sum(i => i.Quantity) });
    }

    // --- ADDED: Handler for updating item quantity in the cart ---
    public async Task<IActionResult> OnPostUpdateCartJsonAsync(int productId, int quantity)
    {
        var cart = await _cartService.SetItemQuantityAsync(productId, null, quantity);
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);

        return new JsonResult(new
        {
            itemCount = cart.Items.Sum(i => i.Quantity),
            newQuantity = cart.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0,
            stock = product.QuantityInStock
        });
    }

    // --- Wishlist handler is already here and correct ---
    public async Task<IActionResult> OnPostToggleWishlistAsync(int productId)
    {
        if (!_signInManager.IsSignedIn(User)) return Unauthorized();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var isInWishlist = await _wishlistService.IsItemInWishlistAsync(userId, productId);

        if (isInWishlist)
        {
            var wishlist = await _wishlistService.GetOrCreateWishlistForUserAsync(userId);
            var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) await _wishlistService.RemoveItemFromWishlistAsync(item.Id);
        }
        else
        {
            await _wishlistService.AddItemToWishlistAsync(userId, productId);
        }

        return new JsonResult(new { isInWishlist = !isInWishlist });
    }
}