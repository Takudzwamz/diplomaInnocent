using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Users;

public class CreateModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<CreateModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    public class CreateUserInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и подтверждение пароля не совпадают.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Назначить роль администратора?")]
        public bool IsAdmin { get; set; }
    }

    public void OnGet()
    {
        ViewData["Title"] = "Создать пользователя";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new AppUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            EmailConfirmed = true // Automatically confirm email for admin-created users
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("New user created by admin: {Email}", user.Email);

            if (Input.IsAdmin)
            {
                // Ensure the "Admin" role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                
                // Add the new user to the "Admin" role
                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation("User {Email} was assigned to Admin role.", user.Email);
            }

            return RedirectToPage("./Index");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}