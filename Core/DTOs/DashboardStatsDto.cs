namespace Core.DTOs;

public class DashboardStatsDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public List<ChartDataPointDto> SalesLast7Days { get; set; } = [];
}

public class ChartDataPointDto
{
    public string XValue { get; set; } = string.Empty;
    public decimal YValue { get; set; }
}