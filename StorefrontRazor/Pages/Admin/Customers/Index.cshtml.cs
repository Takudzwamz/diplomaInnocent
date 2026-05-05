using Core.Entities;
using Core.Interfaces;
using Core.Paging; // <-- 1. ADD THIS
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Customers;

public class CustomerDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime DateRegistered { get; set; }
    public int OrderCount { get; set; }
    public bool IsLockedOut { get; set; }
}

public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    // 2. BIND PAGEINDEX
    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    // 3. CHANGE LIST TO PAGINATION
    public Pagination<CustomerDto> CustomerPagination { get; set; } = null!;

    // 4. UPDATE ONGETASYNC FOR PAGINATION
    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Управление клиентами";
        var pageSize = 15; // Set a page size

        // Get all users who are NOT in the "Admin" role
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var adminUserIds = adminUsers.Select(u => u.Id).ToHashSet();
        
        // Create the base query for customers
        var customerQuery = _userManager.Users
            .Where(u => !adminUserIds.Contains(u.Id));

        // Get total count
        var totalItems = await customerQuery.CountAsync();
        
        // Get paginated users
        var customerUsers = await customerQuery
            .OrderByDescending(u => u.DateRegistered)
            .Skip((PageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get all order counts grouped by the buyer's email (this part is efficient)
        var orderCounts = (await _unitOfWork.Repository<Core.Entities.OrderAggregate.Order>().ListAllAsync())
            .GroupBy(o => o.BuyerEmail)
            .ToDictionary(g => g.Key, g => g.Count());

        // Map the paged data to our DTO
        var customerDtos = customerUsers.Select(u => new CustomerDto
        {
            Id = u.Id,
            Name = $"{u.FirstName} {u.LastName}",
            Email = u.Email ?? string.Empty,
            DateRegistered = u.DateRegistered,
            OrderCount = orderCounts.TryGetValue(u.Email ?? string.Empty, out var count) ? count : 0,
            IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
        }).ToList();
        
        // Create the pagination object
        CustomerPagination = new Pagination<CustomerDto>(PageIndex, pageSize, totalItems, customerDtos);
    }

    // 5. UPDATE HANDLERS TO REDIRECT TO THE CORRECT PAGE
    public async Task<IActionResult> OnPostBanUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
    }

    public async Task<IActionResult> OnPostUnbanUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
    }
}