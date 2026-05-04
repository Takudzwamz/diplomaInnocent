using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace StorefrontRazor.Extensions;

/// <summary>
/// Extension methods to register API endpoints for the chat widget and future mobile app
/// </summary>
public static class ApiEndpointsExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Maps all API endpoints for AI chat, products, cart, and orders
    /// </summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapAIChatEndpoints();
        app.MapProductEndpoints();
        app.MapCartEndpoints();
        app.MapOrderEndpoints();
        
        return app;
    }

    /// <summary>
    /// Maps AI chat related endpoints
    /// </summary>
    private static void MapAIChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/ai/chat", async (HttpContext context, IAIService aiService) =>
        {
            try
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var request = JsonSerializer.Deserialize<ChatRequest>(body, JsonOptions);

                if (request == null || string.IsNullOrEmpty(request.Message))
                {
                    return Results.BadRequest(new { success = false, message = "Message is required" });
                }

                // Enforce input length limit to prevent prompt injection and cost abuse
                if (request.Message.Length > 500)
                {
                    return Results.BadRequest(new { success = false, message = "Message is too long. Please keep it under 500 characters." });
                }

                // Convert DTO to core ChatContext
                var chatContext = MapToChatContext(request.Context);
                var response = await aiService.ChatAsync(request.Message, chatContext);
                
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Ok(new ChatResponse 
                { 
                    Success = false, 
                    Message = "Sorry, I encountered an error. Please try again.",
                    Error = ex.Message
                });
            }
        }).RequireRateLimiting("AiChat");
    }
    /// <summary>
    /// Maps product related endpoints
    /// </summary>
    private static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/api/products/{id:int}", async (int id, StoreContext db) =>
        {
            var product = await db.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductType)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
                return Results.NotFound();
            
            var hasVariants = product.ProductKind == ProductKind.Variable && product.Variants.Any();
            var totalStock = hasVariants 
                ? product.Variants.Sum(v => v.QuantityInStock) 
                : product.QuantityInStock;
            
            return Results.Ok(new ProductApiResponse
            { 
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                PictureUrl = product.PictureUrl,
                Brand = product.ProductBrand?.Name,
                Type = product.ProductType?.Name,
                HasVariants = hasVariants,
                Stock = totalStock
            });
        });
    }

    /// <summary>
    /// Maps cart related endpoints
    /// </summary>
    private static void MapCartEndpoints(this WebApplication app)
    {
        // Add item to cart
        app.MapPost("/api/cart/add", async (HttpContext context, ICartService cartService, ILogger<Program> logger) =>
        {
            try
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                logger.LogInformation("Cart add request body: {Body}", body);
                
                var request = JsonSerializer.Deserialize<AddToCartRequest>(body, JsonOptions);
                
                if (request == null) 
                {
                    logger.LogWarning("Failed to deserialize cart add request");
                    return Results.BadRequest(new CartApiResponse { Success = false, Error = "Invalid request body" });
                }
                
                logger.LogInformation("Adding product {ProductId} to cart with quantity {Quantity}", 
                    request.ProductId, request.Quantity);
                
                var cart = await cartService.AddItemToCartAsync(request.ProductId, null, request.Quantity);
                var cartCount = cart.Items.Sum(i => i.Quantity);
                
                logger.LogInformation("Cart updated successfully. New count: {CartCount}", cartCount);
                
                return Results.Ok(new CartApiResponse { Success = true, CartCount = cartCount });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding to cart");
                return Results.BadRequest(new CartApiResponse { Success = false, Error = ex.Message });
            }
        });

        // Get cart count
        app.MapGet("/api/cart/count", async (ICartService cartService) =>
        {
            var cart = await cartService.GetCartAsync();
            var count = cart.Items.Sum(i => i.Quantity);
            return Results.Ok(new { count });
        });

        // Update cart item quantity
        app.MapPost("/api/cart/update", async (HttpContext context, ICartService cartService) =>
        {
            try
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var request = JsonSerializer.Deserialize<UpdateCartRequest>(body, JsonOptions);
                
                if (request == null) 
                    return Results.BadRequest(new CartApiResponse { Success = false, Error = "Invalid request" });
                
                var cart = await cartService.SetItemQuantityAsync(request.ProductId, null, request.Quantity);
                
                return Results.Ok(new 
                { 
                    success = true, 
                    quantity = request.Quantity,
                    cartCount = cart.Items.Sum(i => i.Quantity)
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new CartApiResponse { Success = false, Error = ex.Message });
            }
        });

        // Get cart context
        app.MapGet("/api/cart/context", async (ICartService cartService) =>
        {
            var cart = await cartService.GetCartAsync();
            return Results.Ok(new CartContextResponse
            { 
                ItemCount = cart.Items.Sum(i => i.Quantity),
                Items = cart.Items.Select(i => new CartItemResponse
                { 
                    ProductId = i.ProductId,
                    Name = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList(),
                Total = cart.Items.Sum(i => i.Price * i.Quantity)
            });
        });
    }

    /// <summary>
    /// Maps order related endpoints
    /// </summary>
    private static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapGet("/api/orders/context", async (HttpContext context, IUnitOfWork unitOfWork) =>
        {
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(email))
            {
                return Results.Ok(new OrdersContextResponse { IsLoggedIn = false });
            }
            
            var spec = new OrderSpecification(email, new OrderSpecParams { PageSize = 20 });
            var orders = await unitOfWork.Repository<Core.Entities.OrderAggregate.Order>().ListAsync(spec);
            
            return Results.Ok(new OrdersContextResponse
            {
                IsLoggedIn = true,
                OrderCount = orders.Count,
                Orders = orders.Take(10).Select(o => new OrderContextItemResponse
                {
                    Id = o.Id,
                    Date = o.OrderDate,
                    Status = o.Status.ToString(),
                    Total = o.GetTotal(),
                    Items = o.OrderItems.Select(i => new OrderItemResponse
                    {
                        ProductId = i.ItemOrdered.ProductId,
                        Name = i.ItemOrdered.ProductName,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                }).ToList()
            });
        });
    }

    /// <summary>
    /// Maps ChatContextDto to core ChatContext
    /// </summary>
    private static ChatContext? MapToChatContext(ChatContextDto? dto)
    {
        if (dto == null) return null;

        var chatContext = new ChatContext();
        
        // Map cart items
        if (dto.Cart?.Items != null)
        {
            chatContext.CartItems = dto.Cart.Items.Select(i => new CartContextItem
            {
                ProductId = i.ProductId,
                Name = i.Name,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            chatContext.CartTotal = dto.Cart.Total;
        }
        
        // Map order history
        if (dto.Orders != null)
        {
            chatContext.IsLoggedIn = dto.Orders.IsLoggedIn;
            if (dto.Orders.Orders != null)
            {
                chatContext.OrderHistory = dto.Orders.Orders.Select(o => new OrderContextItem
                {
                    OrderId = o.Id,
                    OrderDate = o.Date,
                    Status = o.Status,
                    Total = o.Total,
                    Items = o.Items?.Select(i => new OrderItemContext
                    {
                        ProductId = i.ProductId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList() ?? []
                }).ToList();
            }
        }

        return chatContext;
    }
}
