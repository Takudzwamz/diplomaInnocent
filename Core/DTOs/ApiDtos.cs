namespace Core.DTOs;

/// <summary>
/// Request DTO for AI chat endpoint
/// </summary>
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public ChatContextDto? Context { get; set; }
}

/// <summary>
/// DTO for parsing frontend context (different from core ChatContext)
/// </summary>
public class ChatContextDto
{
    public string? CurrentUrl { get; set; }
    public bool OnProductPage { get; set; }
    public CartContextDto? Cart { get; set; }
    public OrdersContextDto? Orders { get; set; }
}

/// <summary>
/// Cart context for AI chat
/// </summary>
public class CartContextDto
{
    public int ItemCount { get; set; }
    public List<CartItemDto>? Items { get; set; }
    public decimal Total { get; set; }
}

/// <summary>
/// Cart item DTO for AI chat context
/// </summary>
public class CartItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Orders context for AI chat
/// </summary>
public class OrdersContextDto
{
    public bool IsLoggedIn { get; set; }
    public int OrderCount { get; set; }
    public List<ChatOrderDto>? Orders { get; set; }
}

/// <summary>
/// Order DTO for AI chat context
/// </summary>
public class ChatOrderDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<ChatOrderItemDto>? Items { get; set; }
}

/// <summary>
/// Order item DTO for AI chat context
/// </summary>
public class ChatOrderItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Request DTO for adding items to cart
/// </summary>
public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Request DTO for updating cart item quantity
/// </summary>
public class UpdateCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Response DTO for product API
/// </summary>
public class ProductApiResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PictureUrl { get; set; }
    public string? Brand { get; set; }
    public string? Type { get; set; }
    public bool HasVariants { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Response DTO for cart operations
/// </summary>
public class CartApiResponse
{
    public bool Success { get; set; }
    public int CartCount { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Response DTO for cart context
/// </summary>
public class CartContextResponse
{
    public int ItemCount { get; set; }
    public List<CartItemResponse> Items { get; set; } = [];
    public decimal Total { get; set; }
}

/// <summary>
/// Cart item in cart context response
/// </summary>
public class CartItemResponse
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Response DTO for orders context
/// </summary>
public class OrdersContextResponse
{
    public bool IsLoggedIn { get; set; }
    public int OrderCount { get; set; }
    public List<OrderContextItemResponse> Orders { get; set; } = [];
}

/// <summary>
/// Order item in orders context response
/// </summary>
public class OrderContextItemResponse
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
}

/// <summary>
/// Order item details in orders context response
/// </summary>
public class OrderItemResponse
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
