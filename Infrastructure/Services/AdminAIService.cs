using Azure.AI.OpenAI;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// AI service for admin features including forecasting, insights, and content generation
/// </summary>
public class AdminAIService : IAdminAIService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminAIService> _logger;
    private readonly AzureOpenAIClientService _openAIClientService;

    public AdminAIService(
        IUnitOfWork unitOfWork,
        AzureOpenAIClientService openAIClientService,
        ILogger<AdminAIService> logger)
    {
        _unitOfWork = unitOfWork;
        _openAIClientService = openAIClientService;
        _logger = logger;
    }

    public bool IsEnabled => _openAIClientService.IsEnabled && _openAIClientService.Client != null;

    #region Sales Forecasting

    public async Task<SalesForecastResult?> GenerateSalesForecastAsync(SalesForecastRequest request)
    {
        if (!IsEnabled) return null;

        try
        {
            // Gather historical sales data - include DeliveryMethod for GetTotal()
            var orderSpec = new OrdersWithDeliveryMethodSpecification(OrderStatus.PaymentReceived);
            var paidOrders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);

            if (!paidOrders.Any())
            {
                return new SalesForecastResult
                {
                    Summary = "Insufficient sales data available for forecasting.",
                    Confidence = "Low"
                };
            }

            // Calculate historical metrics
            var last30Days = paidOrders.Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-30)).ToList();
            var last60Days = paidOrders.Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-60)).ToList();
            var last90Days = paidOrders.Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-90)).ToList();

            var salesLast30 = last30Days.Sum(o => o.GetTotal());
            var salesPrev30 = last60Days.Except(last30Days).Sum(o => o.GetTotal());
            var dailyAvgLast30 = salesLast30 / 30;

            // Group by day for trend analysis
            var dailySales = last90Days
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.GetTotal()) })
                .ToList();

            // Group by month for seasonal analysis
            var monthlySales = paidOrders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Total = g.Sum(o => o.GetTotal()),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // Build AI prompt
            var salesDataSummary = $@"
Historical Sales Data:
- Total Orders (All Time): {paidOrders.Count}
- Total Revenue (All Time): R{paidOrders.Sum(o => o.GetTotal()):N2}
- Sales Last 30 Days: R{salesLast30:N2} ({last30Days.Count} orders)
- Sales Previous 30 Days: R{salesPrev30:N2}
- Daily Average (Last 30 Days): R{dailyAvgLast30:N2}
- Growth Rate (30-day): {(salesPrev30 > 0 ? ((salesLast30 - salesPrev30) / salesPrev30 * 100) : 0):N1}%

Monthly Breakdown:
{string.Join("\n", monthlySales.TakeLast(6).Select(m => $"- {m.Year}/{m.Month:D2}: R{m.Total:N2} ({m.OrderCount} orders)"))}

Daily Sales Trend (Last 30 Days):
{string.Join(", ", dailySales.TakeLast(30).Select(d => $"{d.Date:MM/dd}:R{d.Total:N0}"))}
";

            var prompt = $@"You are a business analytics AI. Analyze the following sales data and provide a forecast for the next {request.ForecastDays} days.

{salesDataSummary}

Provide your analysis in the following JSON format:
{{
    ""summary"": ""2-3 sentence executive summary of the forecast"",
    ""predictedRevenue"": 12345.67,
    ""predictedGrowthPercent"": 5.5,
    ""keyInsights"": [""insight 1"", ""insight 2"", ""insight 3""],
    ""recommendations"": [""recommendation 1"", ""recommendation 2""],
    ""seasonalTrends"": {{
        ""peakSeason"": ""December"",
        ""slowSeason"": ""February"",
        ""seasonalPatterns"": [""pattern 1"", ""pattern 2""]
    }},
    ""confidence"": ""High""
}}

Be realistic and base predictions on the actual data patterns. Consider day-of-week patterns, monthly trends, and growth trajectory.";

            var result = await CallAIAsync<SalesForecastResult>(prompt);
            
            if (result != null)
            {
                // Generate forecast data points
                result.ForecastData = GenerateForecastDataPoints(dailyAvgLast30, result.PredictedGrowthPercent, request.ForecastDays);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales forecast");
            return null;
        }
    }

    private List<ForecastDataPoint> GenerateForecastDataPoints(decimal dailyAvg, decimal growthPercent, int days)
    {
        var points = new List<ForecastDataPoint>();
        var dailyGrowthFactor = 1 + (double)(growthPercent / 100 / 30); // Spread growth over month
        var currentValue = (double)dailyAvg;
        var variance = currentValue * 0.15; // 15% variance

        for (int i = 1; i <= days; i++)
        {
            var date = DateTime.UtcNow.Date.AddDays(i);
            var dayOfWeek = date.DayOfWeek;
            
            // Add day-of-week seasonality (weekends typically lower)
            var seasonalFactor = dayOfWeek switch
            {
                DayOfWeek.Saturday => 0.85,
                DayOfWeek.Sunday => 0.75,
                DayOfWeek.Monday => 1.1,
                DayOfWeek.Friday => 1.15,
                _ => 1.0
            };

            var predictedValue = currentValue * seasonalFactor;
            
            points.Add(new ForecastDataPoint
            {
                Date = date,
                PredictedSales = (decimal)predictedValue,
                LowerBound = (decimal)(predictedValue - variance),
                UpperBound = (decimal)(predictedValue + variance)
            });

            currentValue *= dailyGrowthFactor;
        }

        return points;
    }

    #endregion

    #region Inventory Insights

    public async Task<InventoryInsightsResult?> GenerateInventoryInsightsAsync()
    {
        if (!IsEnabled) return null;

        try
        {
            // Get all products with variants
            var spec = new ProductWithVariantsSpecification();
            var products = await _unitOfWork.Repository<Product>().ListAsync(spec);

            // Get recent order items for sales velocity calculation - include OrderItems
            var orderSpec = new OrdersWithItemsAndDeliveryMethodSpecification(OrderStatus.PaymentReceived);
            var paidOrders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);
            var recentOrders = paidOrders
                .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-30))
                .ToList();

            // Calculate sales by product
            var salesByProduct = recentOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ItemOrdered.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity));

            // Build inventory summary
            var inventoryData = products.Select(p =>
            {
                var totalStock = p.ProductKind == ProductKind.Variable
                    ? p.Variants.Sum(v => v.QuantityInStock)
                    : p.QuantityInStock;

                var unitsSold = salesByProduct.GetValueOrDefault(p.Id, 0);
                var salesVelocity = unitsSold / 30.0; // Units per day
                var daysUntilStockout = salesVelocity > 0 ? (int)(totalStock / salesVelocity) : 999;

                return new
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = totalStock,
                    UnitsSold30Days = unitsSold,
                    SalesVelocity = salesVelocity,
                    DaysUntilStockout = daysUntilStockout,
                    Price = p.Price
                };
            }).ToList();

            var inventorySummary = $@"
Inventory Analysis Data:
- Total Products: {products.Count}
- Out of Stock Products: {inventoryData.Count(i => i.CurrentStock == 0)}
- Low Stock Products (<10 units): {inventoryData.Count(i => i.CurrentStock > 0 && i.CurrentStock < 10)}
- Critical Stock (<5 days until stockout): {inventoryData.Count(i => i.DaysUntilStockout < 5 && i.CurrentStock > 0)}

Top 10 Fast Moving Products (by units sold last 30 days):
{string.Join("\n", inventoryData.OrderByDescending(i => i.UnitsSold30Days).Take(10).Select(i => 
    $"- {i.ProductName}: {i.UnitsSold30Days} sold, {i.CurrentStock} in stock, {i.DaysUntilStockout} days until stockout"))}

Slow Moving Products (in stock but <3 units sold in 30 days):
{string.Join("\n", inventoryData.Where(i => i.CurrentStock > 10 && i.UnitsSold30Days < 3).Take(10).Select(i => 
    $"- {i.ProductName}: {i.UnitsSold30Days} sold, {i.CurrentStock} in stock"))}

Out of Stock Products:
{string.Join("\n", inventoryData.Where(i => i.CurrentStock == 0).Take(10).Select(i => 
    $"- {i.ProductName}: {i.UnitsSold30Days} potential demand (sold before stockout)"))}
";

            var prompt = $@"You are an inventory management AI. Analyze the following inventory data and provide actionable insights.

{inventorySummary}

Provide your analysis in the following JSON format:
{{
    ""summary"": ""2-3 sentence executive summary of inventory health"",
    ""recommendations"": [""recommendation 1"", ""recommendation 2"", ""recommendation 3""],
    ""estimatedStockoutRisk"": 0.25
}}

Focus on actionable insights for the store manager. Consider:
1. Which products need immediate restocking
2. Which slow-moving products might need promotions or discontinuation
3. Overall inventory health and efficiency";

            var aiResult = await CallAIAsync<InventoryInsightsPartial>(prompt);

            // Build the full result with actual data
            var result = new InventoryInsightsResult
            {
                Summary = aiResult?.Summary ?? "Unable to generate inventory summary.",
                Recommendations = aiResult?.Recommendations ?? [],
                EstimatedStockoutRisk = aiResult?.EstimatedStockoutRisk ?? 0,

                Alerts = inventoryData
                    .Where(i => i.DaysUntilStockout < 7 && i.CurrentStock > 0)
                    .OrderBy(i => i.DaysUntilStockout)
                    .Take(10)
                    .Select(i => new InventoryAlert
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        AlertType = i.DaysUntilStockout < 3 ? "Critical" : "Warning",
                        Message = $"Only {i.DaysUntilStockout} days of stock remaining at current sales rate",
                        CurrentStock = i.CurrentStock,
                        DaysUntilStockout = i.DaysUntilStockout
                    }).ToList(),

                RestockRecommendations = inventoryData
                    .Where(i => i.DaysUntilStockout < 14 && i.SalesVelocity > 0)
                    .OrderBy(i => i.DaysUntilStockout)
                    .Take(10)
                    .Select(i => new RestockRecommendation
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        CurrentStock = i.CurrentStock,
                        SuggestedReorderQuantity = (int)Math.Ceiling(i.SalesVelocity * 30), // 30 days supply
                        Urgency = i.DaysUntilStockout < 3 ? "Immediate" : i.DaysUntilStockout < 7 ? "Soon" : "Planned",
                        Reason = $"Current velocity: {i.SalesVelocity:N1} units/day"
                    }).ToList(),

                SlowMovingProducts = inventoryData
                    .Where(i => i.CurrentStock > 10 && i.UnitsSold30Days < 3)
                    .OrderBy(i => i.UnitsSold30Days)
                    .Take(10)
                    .Select(i => new SlowMovingProduct
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        CurrentStock = i.CurrentStock,
                        UnitsSoldLast30Days = i.UnitsSold30Days,
                        DaysInInventory = i.CurrentStock > 0 && i.SalesVelocity > 0 
                            ? (int)(i.CurrentStock / i.SalesVelocity) 
                            : 999,
                        Recommendation = "Consider running a promotion or discount"
                    }).ToList(),

                FastMovingProducts = inventoryData
                    .OrderByDescending(i => i.UnitsSold30Days)
                    .Take(10)
                    .Select(i => new FastMovingProduct
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        CurrentStock = i.CurrentStock,
                        UnitsSoldLast30Days = i.UnitsSold30Days,
                        SalesVelocity = (decimal)i.SalesVelocity
                    }).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory insights");
            return null;
        }
    }

    private class InventoryInsightsPartial
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = [];
        public decimal EstimatedStockoutRisk { get; set; }
    }

    #endregion

    #region Pricing Recommendations

    public async Task<PricingRecommendationsResult?> GeneratePricingRecommendationsAsync()
    {
        if (!IsEnabled) return null;

        try
        {
            var spec = new ProductWithVariantsSpecification();
            var products = await _unitOfWork.Repository<Product>().ListAsync(spec);

            // Get sales data - include OrderItems for product analysis
            var orderSpec = new OrdersWithItemsAndDeliveryMethodSpecification(OrderStatus.PaymentReceived);
            var paidOrders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);
            var recentOrders = paidOrders
                .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-60))
                .ToList();

            var salesByProduct = recentOrders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ItemOrdered.ProductId)
                .ToDictionary(
                    g => g.Key, 
                    g => new { 
                        Quantity = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                    });

            // Build pricing data
            var pricingData = products.Select(p =>
            {
                var sales = salesByProduct.GetValueOrDefault(p.Id);
                var totalStock = p.ProductKind == ProductKind.Variable
                    ? p.Variants.Sum(v => v.QuantityInStock)
                    : p.QuantityInStock;

                return new
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentPrice = p.Price,
                    Brand = p.ProductBrand?.Name ?? "Unknown",
                    Type = p.ProductType?.Name ?? "Unknown",
                    Stock = totalStock,
                    UnitsSold60Days = sales?.Quantity ?? 0,
                    Revenue60Days = sales?.Revenue ?? 0
                };
            }).ToList();

            var pricingSummary = $@"
Product Pricing and Sales Data (Last 60 Days):
Total Products: {products.Count}
Total Revenue: R{pricingData.Sum(p => p.Revenue60Days):N2}

High Performers (good sales, consider price increase):
{string.Join("\n", pricingData.Where(p => p.UnitsSold60Days > 10).OrderByDescending(p => p.UnitsSold60Days).Take(10).Select(p =>
    $"- {p.ProductName}: R{p.CurrentPrice:N2}, {p.UnitsSold60Days} sold, {p.Stock} in stock"))}

Low Performers (low sales, consider price decrease or promotion):
{string.Join("\n", pricingData.Where(p => p.Stock > 10 && p.UnitsSold60Days < 3).Take(10).Select(p =>
    $"- {p.ProductName}: R{p.CurrentPrice:N2}, {p.UnitsSold60Days} sold, {p.Stock} in stock"))}

Price Range Analysis by Type:
{string.Join("\n", pricingData.GroupBy(p => p.Type).Select(g =>
    $"- {g.Key}: R{g.Min(p => p.CurrentPrice):N2} - R{g.Max(p => p.CurrentPrice):N2} (avg R{g.Average(p => p.CurrentPrice):N2})"))}
";

            var prompt = $@"You are a pricing strategy AI. Analyze the following product and sales data to provide pricing recommendations.

{pricingSummary}

Provide your analysis in the following JSON format:
{{
    ""summary"": ""2-3 sentence executive summary of pricing opportunities"",
    ""keyInsights"": [""insight 1"", ""insight 2""],
    ""potentialRevenueIncrease"": 5000.00,
    ""recommendations"": [
        {{
            ""productId"": 1,
            ""productName"": ""Product Name"",
            ""currentPrice"": 100.00,
            ""suggestedPrice"": 110.00,
            ""priceChangePercent"": 10.0,
            ""reason"": ""High demand with limited stock suggests price elasticity"",
            ""confidence"": ""High"",
            ""impactLevel"": ""High""
        }}
    ]
}}

Consider:
1. Products with high sales velocity might support higher prices
2. Products with excess inventory might benefit from price reductions
3. Price positioning within category/type
4. Be conservative - suggest max 15% price changes";

            return await CallAIAsync<PricingRecommendationsResult>(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pricing recommendations");
            return null;
        }
    }

    #endregion

    #region Customer Insights

    public async Task<CustomerInsightsResult?> GenerateCustomerInsightsAsync()
    {
        if (!IsEnabled) return null;

        try
        {
            var orderSpec = new OrdersWithItemsAndDeliveryMethodSpecification(OrderStatus.PaymentReceived);
            var paidOrders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);

            _logger.LogInformation("Customer Insights: Found {Count} paid orders", paidOrders.Count);

            // If no paid orders, return a meaningful result without AI
            if (!paidOrders.Any())
            {
                return new CustomerInsightsResult
                {
                    Summary = "No customer data available yet. Customer insights will be generated once orders are placed.",
                    TotalCustomers = 0,
                    ActiveCustomers = 0,
                    AverageOrderValue = 0,
                    CustomerLifetimeValue = 0,
                    Segments = [],
                    KeyInsights = ["No orders have been placed yet"],
                    Recommendations = ["Focus on customer acquisition strategies"],
                    ChurnRiskCustomers = [],
                    HighValueCustomers = []
                };
            }

            // Group by customer email
            var customerData = paidOrders
                .GroupBy(o => o.BuyerEmail)
                .Select(g => new
                {
                    Email = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.GetTotal()),
                    FirstOrder = g.Min(o => o.OrderDate),
                    LastOrder = g.Max(o => o.OrderDate),
                    AverageOrderValue = g.Average(o => o.GetTotal()),
                    DaysSinceLastOrder = (DateTime.UtcNow - g.Max(o => o.OrderDate)).Days
                })
                .ToList();

            _logger.LogInformation("Customer Insights: Found {Count} unique customers", customerData.Count);

            var totalCustomers = customerData.Count;
            var activeCustomers = customerData.Count(c => c.DaysSinceLastOrder <= 90);
            var avgOrderValue = customerData.Any() ? customerData.Average(c => c.AverageOrderValue) : 0;
            var avgLifetimeValue = customerData.Any() ? customerData.Average(c => c.TotalSpent) : 0;

            // Segment customers
            var highValue = customerData.Where(c => c.TotalSpent > avgLifetimeValue * 2).ToList();
            var atRisk = customerData.Where(c => c.DaysSinceLastOrder > 60 && c.TotalSpent > avgLifetimeValue).ToList();
            var newCustomers = customerData.Where(c => c.OrderCount == 1 && c.DaysSinceLastOrder <= 30).ToList();
            var loyalCustomers = customerData.Where(c => c.OrderCount >= 3).ToList();

            var customerSummary = $@"
Customer Analytics Summary:
- Total Customers: {totalCustomers}
- Active Customers (ordered in last 90 days): {activeCustomers}
- Average Order Value: R{avgOrderValue:N2}
- Average Customer Lifetime Value: R{avgLifetimeValue:N2}

Customer Segments:
- High Value (spent > 2x average): {highValue.Count} customers
- Loyal (3+ orders): {loyalCustomers.Count} customers
- At Risk (no order in 60+ days, above avg spend): {atRisk.Count} customers
- New (1 order in last 30 days): {newCustomers.Count} customers

Top 10 Customers by Total Spend:
{string.Join("\n", customerData.OrderByDescending(c => c.TotalSpent).Take(10).Select(c =>
    $"- {c.Email}: R{c.TotalSpent:N2} ({c.OrderCount} orders, last order {c.DaysSinceLastOrder} days ago)"))}

At-Risk High-Value Customers:
{string.Join("\n", atRisk.OrderByDescending(c => c.TotalSpent).Take(10).Select(c =>
    $"- {c.Email}: R{c.TotalSpent:N2}, {c.DaysSinceLastOrder} days since last order"))}
";

            var prompt = $@"You are a customer analytics AI. Analyze the following customer data and provide actionable insights.

{customerSummary}

Provide your analysis in the following JSON format:
{{
    ""summary"": ""2-3 sentence executive summary of customer health"",
    ""keyInsights"": [""insight 1"", ""insight 2"", ""insight 3""],
    ""recommendations"": [""recommendation 1"", ""recommendation 2""],
    ""segments"": [
        {{
            ""name"": ""High Value"",
            ""customerCount"": {highValue.Count},
            ""percentageOfTotal"": {(totalCustomers > 0 ? highValue.Count * 100.0 / totalCustomers : 0).ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},
            ""averageSpend"": {(highValue.Any() ? highValue.Average(c => c.TotalSpent) : 0).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},
            ""description"": ""Top spending customers""
        }}
    ]
}}

Focus on:
1. Customer retention strategies
2. Re-engagement opportunities for at-risk customers
3. Upselling opportunities for loyal customers
4. Converting new customers to repeat buyers";

            _logger.LogInformation("Customer Insights: Calling AI service...");
            var aiResult = await CallAIAsync<CustomerInsightsPartial>(prompt);
            _logger.LogInformation("Customer Insights: AI result is {Status}", aiResult != null ? "not null" : "null");

            // Build default insights if AI fails
            var defaultSummary = $"Your store has {totalCustomers} customers with {activeCustomers} active in the last 90 days. " +
                $"Average order value is R{avgOrderValue:N2} and customer lifetime value is R{avgLifetimeValue:N2}.";

            var defaultInsights = new List<string>
            {
                $"{totalCustomers} total customers identified",
                $"{activeCustomers} customers active in the last 90 days",
                $"{highValue.Count} high-value customers spending above average"
            };

            var defaultRecommendations = new List<string>
            {
                atRisk.Count > 0 ? $"Re-engage {atRisk.Count} at-risk customers with win-back campaigns" : "Continue building customer relationships",
                loyalCustomers.Count > 0 ? $"Reward {loyalCustomers.Count} loyal customers with exclusive offers" : "Create a loyalty program to encourage repeat purchases"
            };

            return new CustomerInsightsResult
            {
                Summary = aiResult?.Summary ?? defaultSummary,
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                AverageOrderValue = avgOrderValue,
                CustomerLifetimeValue = avgLifetimeValue,
                Segments = aiResult?.Segments ?? new List<CustomerSegment>
                {
                    new() { Name = "High Value", CustomerCount = highValue.Count, PercentageOfTotal = totalCustomers > 0 ? highValue.Count * 100m / totalCustomers : 0, AverageSpend = highValue.Any() ? highValue.Average(c => c.TotalSpent) : 0, Description = "Top spending customers" },
                    new() { Name = "Loyal", CustomerCount = loyalCustomers.Count, PercentageOfTotal = totalCustomers > 0 ? loyalCustomers.Count * 100m / totalCustomers : 0, AverageSpend = loyalCustomers.Any() ? loyalCustomers.Average(c => c.TotalSpent) : 0, Description = "Customers with 3+ orders" },
                    new() { Name = "At Risk", CustomerCount = atRisk.Count, PercentageOfTotal = totalCustomers > 0 ? atRisk.Count * 100m / totalCustomers : 0, AverageSpend = atRisk.Any() ? atRisk.Average(c => c.TotalSpent) : 0, Description = "No order in 60+ days" },
                    new() { Name = "New", CustomerCount = newCustomers.Count, PercentageOfTotal = totalCustomers > 0 ? newCustomers.Count * 100m / totalCustomers : 0, AverageSpend = newCustomers.Any() ? newCustomers.Average(c => c.TotalSpent) : 0, Description = "First order in last 30 days" }
                },
                KeyInsights = aiResult?.KeyInsights ?? defaultInsights,
                Recommendations = aiResult?.Recommendations ?? defaultRecommendations,

                ChurnRiskCustomers = atRisk
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(10)
                    .Select(c => new ChurnRiskCustomer
                    {
                        Email = c.Email,
                        CustomerName = c.Email.Split('@')[0],
                        DaysSinceLastOrder = c.DaysSinceLastOrder,
                        TotalSpent = c.TotalSpent,
                        ChurnProbability = Math.Min(0.9m, c.DaysSinceLastOrder / 180.0m),
                        SuggestedAction = c.DaysSinceLastOrder > 90 
                            ? "Send win-back campaign with special offer"
                            : "Send personalized re-engagement email"
                    }).ToList(),

                HighValueCustomers = highValue
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(10)
                    .Select(c => new HighValueCustomer
                    {
                        Email = c.Email,
                        CustomerName = c.Email.Split('@')[0],
                        TotalSpent = c.TotalSpent,
                        OrderCount = c.OrderCount,
                        AverageOrderValue = c.AverageOrderValue
                    }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer insights");
            return null;
        }
    }

    private class CustomerInsightsPartial
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyInsights { get; set; } = [];
        public List<string> Recommendations { get; set; } = [];
        public List<CustomerSegment> Segments { get; set; } = [];
    }

    #endregion

    #region Review Analysis

    public async Task<ReviewAnalysisResult?> AnalyzeAllReviewsAsync()
    {
        if (!IsEnabled) return null;

        try
        {
            var reviews = await _unitOfWork.Repository<ProductReview>().ListAllAsync();
            
            if (!reviews.Any())
            {
                return new ReviewAnalysisResult
                {
                    Summary = "No reviews available for analysis.",
                    TotalReviews = 0
                };
            }

            var products = await _unitOfWork.Repository<Product>().ListAllAsync();
            var productDict = products.ToDictionary(p => p.Id, p => p.Name);

            // Group reviews by product
            var reviewsByProduct = reviews
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = productDict.GetValueOrDefault(g.Key, "Unknown"),
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => r.Rating),
                    RecentAvg = g.Where(r => r.ReviewDate >= DateTime.UtcNow.AddDays(-30)).DefaultIfEmpty().Average(r => r?.Rating ?? 0),
                    Reviews = g.OrderByDescending(r => r.ReviewDate).Take(5).ToList()
                })
                .ToList();

            var totalReviews = reviews.Count;
            var avgRating = reviews.Average(r => r.Rating);
            
            // Get sample reviews for AI analysis
            var sampleReviews = reviews
                .OrderByDescending(r => r.ReviewDate)
                .Take(50)
                .Select(r => $"Rating: {r.Rating}/5 - {r.Comment ?? "No comment"}")
                .ToList();

            var reviewSummary = $@"
Review Analytics Summary:
- Total Reviews: {totalReviews}
- Average Rating: {avgRating:N2}/5
- Products with Reviews: {reviewsByProduct.Count}

Rating Distribution:
- 5 stars: {reviews.Count(r => r.Rating == 5)} ({reviews.Count(r => r.Rating == 5) * 100.0 / totalReviews:N1}%)
- 4 stars: {reviews.Count(r => r.Rating == 4)} ({reviews.Count(r => r.Rating == 4) * 100.0 / totalReviews:N1}%)
- 3 stars: {reviews.Count(r => r.Rating == 3)} ({reviews.Count(r => r.Rating == 3) * 100.0 / totalReviews:N1}%)
- 2 stars: {reviews.Count(r => r.Rating == 2)} ({reviews.Count(r => r.Rating == 2) * 100.0 / totalReviews:N1}%)
- 1 star: {reviews.Count(r => r.Rating == 1)} ({reviews.Count(r => r.Rating == 1) * 100.0 / totalReviews:N1}%)

Products with Lowest Ratings (min 3 reviews):
{string.Join("\n", reviewsByProduct.Where(p => p.ReviewCount >= 3).OrderBy(p => p.AverageRating).Take(5).Select(p =>
    $"- {p.ProductName}: {p.AverageRating:N1}/5 ({p.ReviewCount} reviews)"))}

Sample Recent Reviews:
{string.Join("\n", sampleReviews.Take(20))}
";

            var prompt = $@"You are a customer feedback analyst. Analyze the following review data and provide insights.

{reviewSummary}

Provide your analysis in the following JSON format:
{{
    ""summary"": ""2-3 sentence executive summary of customer sentiment"",
    ""overallSentiment"": ""Positive"",
    ""commonPraises"": [""praise theme 1"", ""praise theme 2""],
    ""commonComplaints"": [""complaint theme 1"", ""complaint theme 2""],
    ""recommendations"": [""recommendation 1"", ""recommendation 2""]
}}

Focus on:
1. Overall customer satisfaction trends
2. Common themes in positive and negative feedback
3. Products or areas needing attention
4. Actionable recommendations for improvement";

            var aiResult = await CallAIAsync<ReviewAnalysisPartial>(prompt);

            return new ReviewAnalysisResult
            {
                Summary = aiResult?.Summary ?? "Unable to generate review analysis.",
                TotalReviews = totalReviews,
                AverageRating = avgRating,
                OverallSentiment = aiResult?.OverallSentiment ?? (avgRating >= 4 ? "Positive" : avgRating >= 3 ? "Mixed" : "Negative"),
                CommonPraises = aiResult?.CommonPraises ?? [],
                CommonComplaints = aiResult?.CommonComplaints ?? [],
                Recommendations = aiResult?.Recommendations ?? [],

                ProductSentiments = reviewsByProduct
                    .OrderByDescending(p => p.ReviewCount)
                    .Take(20)
                    .Select(p => new ProductSentiment
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        ReviewCount = p.ReviewCount,
                        AverageRating = p.AverageRating,
                        Sentiment = p.AverageRating >= 4 ? "Positive" : p.AverageRating >= 3 ? "Mixed" : "Negative",
                        SentimentTrend = p.RecentAvg > p.AverageRating + 0.3 ? "Improving" :
                                        p.RecentAvg < p.AverageRating - 0.3 ? "Declining" : "Stable"
                    }).ToList(),

                ReviewAlerts = reviewsByProduct
                    .Where(p => p.AverageRating < 3 && p.ReviewCount >= 2)
                    .OrderBy(p => p.AverageRating)
                    .Take(5)
                    .Select(p => new ReviewAlert
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        AlertType = p.AverageRating < 2 ? "QualityIssue" : "NeedsAttention",
                        Message = $"Average rating of {p.AverageRating:N1}/5 across {p.ReviewCount} reviews",
                        RecentNegativeCount = p.Reviews.Count(r => r.Rating <= 2)
                    }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing reviews");
            return null;
        }
    }

    private class ReviewAnalysisPartial
    {
        public string Summary { get; set; } = string.Empty;
        public string OverallSentiment { get; set; } = string.Empty;
        public List<string> CommonPraises { get; set; } = [];
        public List<string> CommonComplaints { get; set; } = [];
        public List<string> Recommendations { get; set; } = [];
    }

    #endregion

    #region Product Description Generation

    public async Task<ProductDescriptionResult?> GenerateProductDescriptionAsync(ProductDescriptionRequest request)
    {
        if (!IsEnabled) return null;

        try
        {
            var prompt = $@"You are a professional e-commerce copywriter. Generate compelling product descriptions for the following product:

Product Name: {request.ProductName}
Brand: {request.Brand ?? "Unspecified"}
Category: {request.Category ?? "Unspecified"}
Type: {request.Type ?? "Unspecified"}
Price: R{request.Price:N2}
Features: {(request.Features.Any() ? string.Join(", ", request.Features) : "Not specified")}
Keywords: {(request.Keywords.Any() ? string.Join(", ", request.Keywords) : "Not specified")}
Existing Description: {request.ExistingDescription ?? "None"}
Tone: {request.Tone}

Provide your output in the following JSON format:
{{
    ""shortDescription"": ""A compelling 1-2 sentence teaser (max 150 characters)"",
    ""longDescription"": ""A detailed 2-3 paragraph description highlighting benefits and features"",
    ""suggestedKeywords"": [""keyword1"", ""keyword2"", ""keyword3""],
    ""suggestedTags"": [""tag1"", ""tag2"", ""tag3""],
    ""metaDescription"": ""SEO-optimized meta description (max 160 characters)""
}}

Guidelines:
1. Focus on benefits, not just features
2. Use sensory and emotional language appropriate to the tone
3. Include relevant keywords naturally
4. Make the short description punchy and attention-grabbing
5. The long description should build desire and address potential objections";

            return await CallAIAsync<ProductDescriptionResult>(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product description");
            return null;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<T?> CallAIAsync<T>(string prompt) where T : class
    {
        try
        {
            var chatClient = _openAIClientService.Client!.GetChatClient(_openAIClientService.ChatDeployment);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a business analytics AI assistant. Always respond with valid JSON only, no markdown formatting or extra text."),
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 2000
            };

            var completion = await chatClient.CompleteChatAsync(messages, options);
            var responseText = completion.Value.Content[0].Text;

            _logger.LogDebug("AI Response before cleaning: {Response}", responseText);

            // Clean up JSON response
            responseText = CleanJsonResponse(responseText);

            _logger.LogDebug("AI Response after cleaning: {Response}", responseText);

            return JsonSerializer.Deserialize<T>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            });
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing AI response for type {Type}", typeof(T).Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI service for type {Type}", typeof(T).Name);
            return null;
        }
    }

    private string CleanJsonResponse(string response)
    {
        response = response.Trim();
        
        // Remove markdown code blocks
        if (response.StartsWith("```json"))
            response = response[7..];
        else if (response.StartsWith("```"))
            response = response[3..];
        
        if (response.EndsWith("```"))
            response = response[..^3];
        
        return response.Trim();
    }

    #endregion
}
