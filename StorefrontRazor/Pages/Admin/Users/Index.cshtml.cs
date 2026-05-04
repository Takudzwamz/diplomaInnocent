using Core.Entities;
using Core.Paging; // <-- 1. ADD THIS
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace StorefrontRazor.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    // 2. BIND PAGEINDEX
    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    // 3. CHANGE LIST TO PAGINATION
    public Pagination<UserViewModel> UserPagination { get; set; } = null!;

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    // 4. UPDATE ONGETASYNC FOR PAGINATION
    public async Task OnGetAsync()
    {
        ViewData["Title"] = "User Management";
        var pageSize = 15; // Set a page size
        
        // Get the total count first
        var totalItems = await _userManager.Users.CountAsync();
        
        // Get just the users for the current page
        var users = await _userManager.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((PageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        
        // Create the pagination object
        UserPagination = new Pagination<UserViewModel>(PageIndex, pageSize, totalItems, userViewModels);
    }

    // 5. UPDATE ONPOSTDELETEASYNC TO REDIRECT TO THE CORRECT PAGE
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден.";
            return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
        }

        if (user.Email.Equals("admin@production.com", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Аккаунт суперадминистратора по умолчанию не может быть удалён.";
            return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
            {
                TempData["ErrorMessage"] = "Нельзя удалить последний аккаунт администратора.";
                return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
            }
        }

        await _userManager.DeleteAsync(user);
        TempData["SuccessMessage"] = "Пользователь успешно удалён.";
        return RedirectToPage("./Index", new { PageIndex = this.PageIndex });
    }
}