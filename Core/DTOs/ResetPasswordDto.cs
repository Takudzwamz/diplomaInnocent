using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public class ResetPasswordDto
{
    [Required]
    public string? Token { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? NewPassword { get; set; }
}