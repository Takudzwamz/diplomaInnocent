using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace StorefrontRazor.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<AppUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    // This property will hold the confirmation message after the form is submitted.
    [TempData]
    public string Message { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    /* public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist for security reasons.
                Message = "Если ваш адрес электронной почты зарегистрирован, вы скоро получите ссылку для сброса пароля.";
                return RedirectToPage();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { token = code, email = user.Email }, // Pass token and email in the query string
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Сброс пароля",
                $"Пожалуйста, сбросьте пароль, <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>нажав здесь</a>.");
            
            Message = "Если ваш адрес электронной почты зарегистрирован, вы скоро получите ссылку для сброса пароля.";
            return RedirectToPage();
        }

        return Page();
    } */

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                Message = "Если ваш адрес электронной почты зарегистрирован, вы скоро получите ссылку для сброса пароля.";
                return RedirectToPage();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { token = code, email = user.Email },
                protocol: Request.Scheme);

            // --- THIS IS THE FIX ---
            // Remove the HtmlEncoder from around the callbackUrl
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Сброс пароля",
                $"Пожалуйста, сбросьте пароль, <a href='{callbackUrl}'>нажав здесь</a>.");

            Message = "Если ваш адрес электронной почты зарегистрирован, вы скоро получите ссылку для сброса пароля.";
            return RedirectToPage();
        }

        return Page();
    }
}
