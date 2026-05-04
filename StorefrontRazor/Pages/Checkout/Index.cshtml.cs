using System.Security.Claims;
using Core.DTOs;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StorefrontRazor.Services;
using System.Text.Json;

namespace StorefrontRazor.Pages.Checkout;

[Authorize]
public class IndexModel : PageModel
{
    // Inject all the services we need for the entire checkout process
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IPaymentService _paymentService;
    public CheckoutService Checkout { get; }

    

    // public IEnumerable<string> EnabledGateways { get; set; } = new List<string>();
    // [BindProperty]
    // public string SelectedPaymentGateway { get; set; } = string.Empty;

    public IndexModel(
        ICartService cartService,
        CheckoutService checkoutService,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        IPaymentService paymentService)
    {
        _cartService = cartService;
        Checkout = checkoutService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _paymentService = paymentService;

       
    }

    // --- Properties to hold data for the view ---
    public ShoppingCart Cart { get; set; } = null!;
    public IReadOnlyList<DeliveryMethod> DeliveryMethods { get; set; } = null!;


    [BindProperty]
    public ShippingAddress Address { get; set; } = new()
    {
        Name = string.Empty,
        LastName = string.Empty,
        Line1 = string.Empty,
        City = string.Empty,
        State = string.Empty,
        PostalCode = string.Empty,
        Country = string.Empty,
        PhoneNumber = string.Empty,
        DeliveryNotes = string.Empty
    };

    [BindProperty]
    public bool SaveAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Error { get; set; }

    [BindProperty]
    public int SelectedDeliveryMethodId { get; set; }

    // This property controls which step is shown
    [BindProperty(SupportsGet = true)]
    public int Step { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        Cart = await _cartService.GetCartAsync();
        if (Cart == null || !Cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Ваша корзина пуста. Добавьте товары перед оформлением заказа.";
            return RedirectToPage("/Cart/Index");
        }

        if (Step == 1)
        {
            

            var user = await _userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Email == User.FindFirstValue(ClaimTypes.Email));

            // Pre-fill the form with the user's name and saved address
            Address.Name = user.FirstName;
            Address.LastName = user.LastName;
            if (user?.Address != null)
            {
                Address.Line1 = user.Address.Line1;
                Address.Line2 = user.Address.Line2;
                Address.City = user.Address.City;
                Address.State = user.Address.State;
                Address.PostalCode = user.Address.PostalCode;
                Address.Country = user.Address.Country;
                Address.PhoneNumber = user.Address.PhoneNumber;
            }
            ;
            SaveAddress = Checkout.SaveAddress;
        }

        if (Step == 2)
        {
            DeliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().ListAllAsync();

            // --- ADD THIS CHECK ---
            if (!DeliveryMethods.Any())
            {
                TempData["ErrorMessage"] = "В данный момент мы не можем обработать доставку. Свяжитесь с поддержкой.";
                return RedirectToPage("/Cart/Index");
            }
        }


        if (Step == 3)
        {
            if (!Cart.DeliveryMethodId.HasValue)
            {
                return RedirectToPage(new { step = 2 });
            }
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(Cart.DeliveryMethodId.Value);
            if (deliveryMethod != null) Checkout.SetShippingPrice(deliveryMethod.Price);

            // The 'Error' property will be automatically populated from the URL if it exists
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAddress()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // Save the address to the session-backed service for the next steps
        Checkout.ShippingAddress = Address;
        Checkout.SaveAddress = SaveAddress;

        // If the user checked the box, save this address to their permanent user profile
        if (SaveAddress)
        {
            var user = await _userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Email == User.FindFirstValue(ClaimTypes.Email));

            if (user != null)
            {
                // Convert ShippingAddress to the Address entity
                var addressEntity = new Address
                {
                    Line1 = Address.Line1,
                    Line2 = Address.Line2,
                    City = Address.City,
                    State = Address.State,
                    PostalCode = Address.PostalCode,
                    Country = Address.Country,
                    PhoneNumber = Address.PhoneNumber
                };

                user.Address = addressEntity;
                await _userManager.UpdateAsync(user);
            }
        }

        return RedirectToPage(new { step = 2 });
    }

    // --- Step 2 Handler: Saving the Delivery Method ---
    public async Task<IActionResult> OnPostDelivery()
    {
        Cart = await _cartService.GetCartAsync();
        var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(SelectedDeliveryMethodId);
        if (deliveryMethod != null)
        {
            Checkout.SetShippingPrice(deliveryMethod.Price);
            Cart.DeliveryMethodId = deliveryMethod.Id;
            await _cartService.UpdateCartAsync(Cart);
        }
        return RedirectToPage(new { step = 3 });
    }

    public async Task<IActionResult> OnPostUpdateSummaryJsonAsync(int deliveryMethodId)
    {
        var cart = await _cartService.GetCartAsync();
        var subtotal = cart.Items.Sum(i => i.Price * i.Quantity);

        var discount = 0m;
        if (cart.Coupon != null)
        {
            var eligibleItems = cart.Coupon.ApplicableProductIds.Any()
                ? cart.Items.Where(item => cart.Coupon.ApplicableProductIds.Contains(item.ProductId)).ToList()
                : cart.Items.ToList();

            var eligibleSubtotal = eligibleItems.Sum(i => i.Price * i.Quantity);

            if (cart.Coupon.AmountOff.HasValue)
            {
                discount = Math.Min(cart.Coupon.AmountOff.Value, eligibleSubtotal);
            }
            else if (cart.Coupon.PercentOff.HasValue)
            {
                discount = eligibleSubtotal * (cart.Coupon.PercentOff.Value / 100);
            }
        }

        var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
        var deliveryPrice = deliveryMethod?.Price ?? 0m;

        var total = subtotal - discount + deliveryPrice;

        return new JsonResult(new
        {
            deliveryPrice = deliveryPrice.ToString("C"),
            discount = discount.ToString("C"),
            total = total.ToString("C")
        });
    }

    public async Task<IActionResult> OnPostPlaceOrderAsync()
    {
        try // --- ADD A TRY/CATCH BLOCK for safety ---
        {
            Cart = await _cartService.GetCartAsync();
            var items = new List<OrderItem>();

            // 1. VALIDATE CART AND CREATE ORDER ITEMS (Stock Check)
            var productRepo = _unitOfWork.Repository<Product>();
            var variantRepo = _unitOfWork.Repository<ProductVariant>();

            foreach (var item in Cart.Items)
            {
                ProductItemOrdered itemOrdered;
                decimal price;
                int currentStock = 0;

                if (item.ProductVariantId.HasValue)
                {
                    var variant = await variantRepo.GetByIdAsync(item.ProductVariantId.Value);
                    if (variant == null || variant.QuantityInStock < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Товар '{item.ProductName}' ({item.SelectedOptions}) больше нет в наличии.";
                        return RedirectToPage("/Cart/Index");
                    }
                    price = variant.Price;
                    itemOrdered = new ProductItemOrdered
                    {
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ProductName = item.ProductName,
                        PictureUrl = item.PictureUrl,
                        SelectedOptions = item.SelectedOptions
                    };
                }
                else
                {
                    var productItem = await productRepo.GetByIdAsync(item.ProductId);
                    if (productItem == null || productItem.QuantityInStock < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Товар '{item.ProductName}' больше нет в наличии.";
                        return RedirectToPage("/Cart/Index");
                    }
                    price = productItem.Price;
                    itemOrdered = new ProductItemOrdered
                    {
                        ProductId = productItem.Id,
                        ProductName = productItem.Name,
                        PictureUrl = item.PictureUrl
                    };
                }

                var orderItem = new OrderItem { ItemOrdered = itemOrdered, Price = price, Quantity = item.Quantity };
                items.Add(orderItem);
            }

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(Cart.DeliveryMethodId.Value);
            var subtotal = items.Sum(i => i.Price * i.Quantity);

            // 2. CALCULATE DISCOUNT
            var discount = 0m;
            if (Cart.Coupon != null)
            {
                var eligibleItems = Cart.Coupon.ApplicableProductIds.Any()
                    ? Cart.Items.Where(item => Cart.Coupon.ApplicableProductIds.Contains(item.ProductId)).ToList()
                    : Cart.Items.ToList();
                var eligibleSubtotal = eligibleItems.Sum(i => i.Price * i.Quantity);

                if (Cart.Coupon.AmountOff.HasValue) discount = Math.Min(Cart.Coupon.AmountOff.Value, eligibleSubtotal);
                else if (Cart.Coupon.PercentOff.HasValue) discount = eligibleSubtotal * (Cart.Coupon.PercentOff.Value / 100);
            }

            var activeGatewayName = _paymentService.GetEnabledGateways().FirstOrDefault();
            if (string.IsNullOrEmpty(activeGatewayName))
            {
                TempData["ErrorMessage"] = "Платёжный шлюз не настроен. Свяжитесь с поддержкой.";
                return RedirectToPage(new { step = 3 });
            }

            // 3. CREATE THE ORDER ENTITY
            var order = new Order
            {
                OrderItems = items,
                BuyerEmail = User.FindFirstValue(ClaimTypes.Email),
                ShippingAddress = Checkout.ShippingAddress,
                DeliveryMethod = deliveryMethod,
                Subtotal = subtotal,
                Discount = discount,
                CouponCode = Cart.Coupon?.PromotionCode,
                PaymentGatewayName = activeGatewayName,
                PaymentReference = Guid.NewGuid().ToString(),
                Status = OrderStatus.Pending
                // PaymentGatewayName will be set by the PaymentService
            };

            _unitOfWork.Repository<Order>().Add(order);
            await _unitOfWork.Complete();
            await _cartService.DeleteCartAsync();

            // 4. CREATE PAYMENT TRANSACTION (Service will auto-select the admin's chosen gateway)
            var (authorizationUrl, postData) = await _paymentService.CreatePaymentTransactionAsync(order);

            if (postData != null)
            {
                // This is for PayFast
                var settings = await _unitOfWork.Repository<SiteSetting>().ListAllAsync();
                var siteMode = settings.FirstOrDefault(s => s.Key == "Payment_SiteMode")?.Value ?? "Test";
                var useSandbox = (siteMode == "Test");
                var gatewayUrl = useSandbox ? "https://sandbox.payfast.co.za/eng/process" : "https://www.payfast.co.za/eng/process";

                // --- THIS IS THE FIX ---
                // 1. Store the complex data in TempData
                TempData["PayFast_PostData"] = JsonSerializer.Serialize(postData);
                TempData["PayFast_GatewayUrl"] = gatewayUrl;

                // 2. Redirect with NO route data
                return RedirectToPage("/Checkout/PaymentPost");
                // --- END OF FIX ---
            }

            if (authorizationUrl != null)
            {
                // This is for Paystack
                return Redirect(authorizationUrl);
            }

            TempData["ErrorMessage"] = "Проблема с выбранным платёжным провайдером. Попробуйте ещё раз.";
            return RedirectToPage(new { step = 3 });
        }
        catch (Exception ex)
        {
            // Log the exception (not shown here for brevity)
            TempData["ErrorMessage"] = "Непредвиденная ошибка при оформлении заказа. Попробуйте ещё раз.";
            return RedirectToPage(new { step = 3, error = ex.Message });
        }
    }
}