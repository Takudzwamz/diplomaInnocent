using Core.DTOs;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Paging;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Admin.Orders;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Pagination<OrderDto>? OrderPagination { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;
    
    // This property is now for sorting, not filtering
    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "dateDesc";
    
    [BindProperty(SupportsGet = true)]
    public string? CustomerEmail { get; set; }


    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Управление заказами";

        var orderParams = new OrderSpecParams
        {
            PageIndex = PageIndex,
            PageSize = 10,
            Sort = SortOrder, // Pass the sort order to the specification
            CustomerEmail = CustomerEmail
        };

        // Use the admin constructor that accepts spec params for sorting
        var spec = new OrderSpecification(orderParams);

        var countSpec = new OrderSpecification(new OrderSpecParams { Sort = SortOrder }); // Count all orders without filters

        var totalItems = await _unitOfWork.Repository<Order>().CountAsync(countSpec);
        var orders = await _unitOfWork.Repository<Order>().ListAsync(spec);

        var data = orders.Select(o => o.ToDto()).ToList();
        OrderPagination = new Pagination<OrderDto>(PageIndex, orderParams.PageSize, totalItems, data);
    }
    
    public string GetStatusBadgeClass(string status) => status switch
    {
        "PaymentReceived" => "text-bg-success",
        "Pending" => "text-bg-warning",
        "PaymentFailed" => "text-bg-danger",
        "Refunded" => "text-bg-secondary",
        _ => "text-bg-light"
    };

    public string GetDeliveryStatusBadgeClass(string status) => status switch
    {
        "Delivered" => "text-bg-success",
        "Processing" => "text-bg-info",
        "Shipped" => "text-bg-primary",
        "OutForDelivery" => "text-bg-dark",
        _ => "text-bg-light"
    };
}