using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
// Import the WebUtility class for URL decoding
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace StorefrontRazor.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;

    public ResetPasswordModel(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public bool ResetSuccessful { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email обязателен.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль.")]
        [StringLength(100, ErrorMessage = "{0} должен быть не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и подтверждение пароля не совпадают.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }

    public IActionResult OnGet([FromQuery] string email, [FromQuery] string? token = null)
    {
        if (token == null || email == null)
        {
            return BadRequest("A token and email must be supplied for password reset.");
        }

        Input = new InputModel
        {
            // We pass the raw token to the form's hidden field.
            // It will be decoded when the form is submitted.
            Token = token,
            Email = email
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist.
            ResetSuccessful = true;
            return Page();
        }

        // --- THIS IS THE FIX ---
        // The token from the URL is Base64UrlEncoded. We must decode it back to the original string.
        try
        {
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(Input.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);
            if (result.Succeeded)
            {
                ResetSuccessful = true;
                return Page();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (FormatException)
        {
            // This will catch invalid Base64 strings, which also means an invalid token.
            ModelState.AddModelError(string.Empty, "Недействительный токен.");
        }

        return Page();
    }
}