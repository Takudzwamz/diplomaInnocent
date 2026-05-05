using System.Security.Claims;
using Core.DTOs;
using Core.Entities.OrderAggregate;
using Core.Extensions;
using Core.Interfaces;
using Core.Paging;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Orders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Pagination<OrderDto> Orders { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "dateDesc";

    public async Task OnGetAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var orderParams = new OrderSpecParams
        {
            PageIndex = PageIndex,
            PageSize = 10,
            Sort = SortOrder
        };

        var spec = new OrderSpecification(email!, orderParams);
        var countSpec = new BaseSpecification<Order>(o => o.BuyerEmail == email);

        var totalItems = await _unitOfWork.Repository<Order>().CountAsync(countSpec);
        var orders = await _unitOfWork.Repository<Order>().ListAsync(spec);

        var data = orders.Select(o => o.ToDto()).ToList();
        Orders = new Pagination<OrderDto>(PageIndex, orderParams.PageSize, totalItems, data);
    }

    // --- THIS IS THE FIX ---
    // Helper methods are now fully implemented here as well.
    public string GetStatusBadgeClass(string status) => status switch
    {
        "PaymentReceived" => "text-bg-success",
        "Pending" => "text-bg-warning",
        "PaymentFailed" => "text-bg-danger",
        "Refunded" => "text-bg-secondary",
        "PaymentMismatch" => "text-bg-info",
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
    // --- END OF FIX ---
}