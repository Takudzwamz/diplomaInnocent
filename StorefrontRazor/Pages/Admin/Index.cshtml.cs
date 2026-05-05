using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin;

public class CustomerDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime DateRegistered { get; set; }
    public int OrderCount { get; set; }
}
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

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly StoreContext _context;

    public IndexModel(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, StoreContext context)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string Period { get; set; } = "7d"; // Default period

    public List<ChartDataPoint> SalesChartData { get; set; } = [];
    public string ChartTitle { get; set; } = string.Empty;

    public List<string> ChartJsLabels { get; set; } = [];
    public List<decimal> ChartJsData { get; set; } = [];

    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int OutOfStockProducts { get; set; }
    public List<ChartDataPoint> SalesLast7Days { get; set; } = new();
    public decimal MaxSalesValue { get; set; } = 1;

    public List<CustomerDto> RecentCustomers { get; set; } = [];
    public List<TopProductDto> TopSellingProducts { get; set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Панель управления";

        // === THIS IS THE FIX: We now await each database call sequentially ===
        TotalSales = await _context.Orders
            .Where(o => o.Status == OrderStatus.PaymentReceived)
            .SumAsync(o => o.Subtotal - o.Discount + o.DeliveryMethod.Price);

        TotalOrders = await _unitOfWork.Repository<Order>().CountAsync(null!);
        TotalProducts = await _unitOfWork.Repository<Product>().CountAsync(null!);


        // --- FETCH RECENT CUSTOMERS (REVISED LOGIC) ---
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var adminUserIds = adminUsers.Select(u => u.Id).ToHashSet();

        // 1. Get the 10 most recent user entities
        var recentUserEntities = await _userManager.Users
            .Where(u => !adminUserIds.Contains(u.Id))
            .OrderByDescending(u => u.DateRegistered)
            .Take(10)
            .ToListAsync();

        // 2. Get all order counts (this is efficient)
        var orderCounts = (await _unitOfWork.Repository<Core.Entities.OrderAggregate.Order>().ListAllAsync())
            .GroupBy(o => o.BuyerEmail)
            .ToDictionary(g => g.Key, g => g.Count());

        // 3. Map the user entities to the CustomerDto, including the order count
        RecentCustomers = recentUserEntities.Select(u => new CustomerDto
        {
            Id = u.Id,
            Name = $"{u.FirstName} {u.LastName}",
            Email = u.Email ?? string.Empty,
            DateRegistered = u.DateRegistered,
            OrderCount = orderCounts.TryGetValue(u.Email ?? string.Empty, out var count) ? count : 0
        }).ToList();

        // --- FIX THE CUSTOMER COUNT ---
        TotalCustomers = await _userManager.Users.CountAsync(u => !adminUserIds.Contains(u.Id));

        

           TopSellingProducts = await _context.OrderItems
           .Where(oi => oi.Order.Status == OrderStatus.PaymentReceived) // Only count paid orders
           .GroupBy(oi => new { 
               oi.ItemOrdered.ProductId, 
               oi.ItemOrdered.ProductVariantId, // Group by variant ID
               oi.ItemOrdered.ProductName, 
               oi.ItemOrdered.PictureUrl,
               oi.ItemOrdered.SelectedOptions // Also group by the options string
           })
           .Select(g => new TopProductDto
           {
               ProductId = g.Key.ProductId,
               ProductVariantId = g.Key.ProductVariantId,
               ProductName = g.Key.ProductName,
               SelectedOptions = g.Key.SelectedOptions,
               PictureUrl = g.Key.PictureUrl,
               TotalQuantitySold = g.Sum(oi => oi.Quantity),
               TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity) // Calculate revenue
           })
           .OrderByDescending(dto => dto.TotalQuantitySold)
           .Take(10)
           .ToListAsync();

        
        // 1. Fetch ALL products, making sure to include their variants
        var spec = new ProductWithVariantsSpecification();
        var allProducts = await _unitOfWork.Repository<Product>().ListAsync(spec);

        // 2. Apply the correct logic in-memory to count out-of-stock products
        OutOfStockProducts = allProducts.Count(p =>
            (p.ProductKind == ProductKind.Simple && p.QuantityInStock == 0) ||
            (p.ProductKind == ProductKind.Variable && p.Variants.Sum(v => v.QuantityInStock) == 0)
        );

        await GetSalesDataAsync();
    }

    private async Task GetSalesDataAsync()
    {
        DateTime startDate = DateTime.UtcNow.Date;

        switch (Period)
        {
            case "30d":
                startDate = DateTime.UtcNow.AddDays(-29).Date;
                ChartTitle = "Sales (Last 30 Days)";
                break;
            case "3m":
                // Normalize start date to the first day of the month
                var startMonth3m = DateTime.UtcNow.AddMonths(-2);
                startDate = new DateTime(startMonth3m.Year, startMonth3m.Month, 1);
                ChartTitle = "Sales (Last 3 Months)";
                break;
            case "1y":
                // Normalize start date to the first day of the month
                var startMonth1y = DateTime.UtcNow.AddMonths(-11);
                startDate = new DateTime(startMonth1y.Year, startMonth1y.Month, 1);
                ChartTitle = "Sales (Last 12 Months)";
                break;
            case "all":
                var firstOrder = await _context.Orders.OrderBy(o => o.OrderDate).FirstOrDefaultAsync();
                // Normalize start date to the first day of the year
                startDate = firstOrder != null ? new DateTime(firstOrder.OrderDate.Year, 1, 1) : new DateTime(DateTime.UtcNow.Year, 1, 1);
                ChartTitle = "All-Time Sales";
                break;
            default: // "7d"
                startDate = DateTime.UtcNow.AddDays(-6).Date;
                ChartTitle = "Sales (Last 7 Days)";
                break;
        }

        // This query is now simpler and correct for all cases
        var salesData = await _context.Orders
            .Where(o => o.Status == OrderStatus.PaymentReceived && o.OrderDate >= startDate)
            .GroupBy(o => Period == "3m" || Period == "1y" ? new DateTime(o.OrderDate.Year, o.OrderDate.Month, 1) :
                           Period == "all" ? new DateTime(o.OrderDate.Year, 1, 1) :
                           o.OrderDate.Date)
            .Select(g => new
            {
                DateKey = g.Key,
                Total = g.Sum(o => o.Subtotal - o.Discount + o.DeliveryMethod.Price)
            })
            .ToDictionaryAsync(x => x.DateKey, x => x.Total);

        // This loop now correctly fills in the gaps for all periods
        var chartPoints = new Dictionary<string, decimal>();
        var currentDate = startDate;
        while (currentDate <= DateTime.UtcNow.Date)
        {
            string key;
            DateTime nextDate;

            if (Period == "3m" || Period == "1y")
            {
                key = currentDate.ToString("MMM yyyy");
                nextDate = currentDate.AddMonths(1);
            }
            else if (Period == "all")
            {
                key = currentDate.ToString("yyyy");
                nextDate = currentDate.AddYears(1);
            }
            else // Daily grouping
            {
                key = currentDate.ToString("MMM dd");
                nextDate = currentDate.AddDays(1);
            }

            // Aggregate all sales for the current key (handles daily data being grouped by month)
            var total = salesData.Where(kvp => kvp.Key >= currentDate && kvp.Key < nextDate).Sum(kvp => kvp.Value);
            chartPoints[key] = total;
            currentDate = nextDate;
        }

        var chartDataList = chartPoints.Select(kvp => new ChartDataPoint { XValue = kvp.Key, YValue = kvp.Value }).ToList();



        SalesChartData = chartDataList;

        ChartJsLabels = SalesChartData.Select(d => d.XValue).ToList();
        ChartJsData = SalesChartData.Select(d => d.YValue).ToList();

        if (SalesChartData.Any(d => d.YValue > 0))
        {
            MaxSalesValue = SalesChartData.Max(d => d.YValue);
        }
    }

    public async Task<JsonResult> OnGetChartDataJsonAsync(string period)
    {
        Period = period;
        await GetSalesDataAsync();

        return new JsonResult(new
        {
            chartTitle = ChartTitle,
            // Add the new properties to the JSON response
            chartJsLabels = ChartJsLabels,
            chartJsData = ChartJsData
        });
    }
    

}

public class ChartDataPoint
{
    public string XValue { get; set; } = string.Empty;
    public decimal YValue { get; set; }
    public bool ShowLabel { get; set; } = true;
}

