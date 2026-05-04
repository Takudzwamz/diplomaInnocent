using Core.DTOs;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Paging;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Products;

// We can use the same DTO from the Dashboard
public class TopProductDto
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public required string ProductName { get; set; }
    public string? SelectedOptions { get; set; }
    public required string PictureUrl { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopSellingModel : PageModel
{
    private readonly StoreContext _context;

    public TopSellingModel(StoreContext context)
    {
        _context = context;
    }

    public Pagination<TopProductDto> ProductPagination { get; set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;
    private const int PageSize = 15;

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Лидеры продаж";

        var query = _context.OrderItems
           .Where(oi => oi.Order.Status == OrderStatus.PaymentReceived)
           .GroupBy(oi => new { 
               oi.ItemOrdered.ProductId, 
               oi.ItemOrdered.ProductVariantId,
               oi.ItemOrdered.ProductName, 
               oi.ItemOrdered.PictureUrl,
               oi.ItemOrdered.SelectedOptions
           })
           .Select(g => new TopProductDto
           {
               ProductId = g.Key.ProductId,
               ProductVariantId = g.Key.ProductVariantId,
               ProductName = g.Key.ProductName,
               SelectedOptions = g.Key.SelectedOptions,
               PictureUrl = g.Key.PictureUrl,
               TotalQuantitySold = g.Sum(oi => oi.Quantity),
               TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
           })
           .OrderByDescending(dto => dto.TotalQuantitySold);

        var totalItems = await query.CountAsync();
        var products = await query
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
            
        ProductPagination = new Pagination<TopProductDto>(PageIndex, PageSize, totalItems, products);
    }
}