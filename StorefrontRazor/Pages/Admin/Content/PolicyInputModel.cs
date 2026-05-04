using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Content;

public class PolicyInputModel
{
    [Required, Display(Name = "Return Window (in days)")]
    public string ReturnWindowDays { get; set; } = "30";

    [Required, Display(Name = "Refund Processing Time")]
    public string RefundTimeframe { get; set; } = "5-10 business days";

    [Required, EmailAddress, Display(Name = "Support Email for Returns")]
    public string ContactEmail { get; set; } = "support@example.com";
    
    [Required, DataType(DataType.MultilineText), Display(Name = "List of Non-Returnable Items (one item per line)")]
    public string NonReturnableItems { get; set; } = "Gift cards\nDownloadable software";
}