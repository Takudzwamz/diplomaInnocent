using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Users;

public class EditModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EditModel(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public UserEditInput Input { get; set; } = new();
    
    public class UserEditInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Назначить роль администратора?")]
        public bool IsAdmin { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["Title"] = "Редактировать пользователя";

        var user = await _userManager.FindByIdAsync(Id);
        if (user == null)
        {
            return NotFound();
        }

        Input = new UserEditInput
        {
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(Id);
        if (user == null)
        {
            return NotFound();
        }
        
        // Prevent changing the email of the super admin
        if (user.Email!.Equals("admin@test.com", StringComparison.OrdinalIgnoreCase) && 
            !Input.Email.Equals("admin@test.com", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Нельзя изменить email суперадминистратора по умолчанию.");
            return Page();
        }

        user.Email = Input.Email;
        user.UserName = Input.Email; // Keep username in sync with email
        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // Handle Role update
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        
        if (Input.IsAdmin && !isAdmin)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        else if (!Input.IsAdmin && isAdmin)
        {
            // Safety check: don't let user remove the last admin role
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
            {
                ModelState.AddModelError("Input.IsAdmin", "Нельзя снять последнюю роль администратора.");
                return Page();
            }
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        TempData["SuccessMessage"] = "Пользователь успешно обновлён.";
        return RedirectToPage("./Index");
    }
}