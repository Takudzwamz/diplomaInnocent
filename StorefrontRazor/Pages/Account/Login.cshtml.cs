

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces; // Add this
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StorefrontRazor.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager; // Add this
    private readonly IWishlistService _wishlistService; // Add this

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IWishlistService wishlistService)
    {
        _signInManager = signInManager;
        _userManager = userManager; // Add this
        _wishlistService = wishlistService; // Add this
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();
    
    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // --- ADD THIS LOGIC ---
                // After a successful login, ensure the user has a wishlist.
                var user = await _userManager.FindByEmailAsync(Input.Email);
               if (user != null)
                {
                    // --- START: ADDED REDIRECT LOGIC ---
                    // Check if the user is in the "Admin" role.
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        // If they are an admin, go directly to the admin dashboard.
                        return RedirectToPage("/Admin/Index");
                    }
                    // --- END: ADDED REDIRECT LOGIC ---

                    // For regular customers, ensure they have a wishlist.
                    await _wishlistService.GetOrCreateWishlistForUserAsync(user.Id);
                }
                
                return LocalRedirect(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Аккаунт заблокирован из-за слишком многих неудачных попыток. Повторите через 15 минут.");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return Page();
            }
        }

        return Page();
    }
}