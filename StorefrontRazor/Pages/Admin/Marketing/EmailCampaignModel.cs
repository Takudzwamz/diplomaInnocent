using System.ComponentModel.DataAnnotations;

namespace StorefrontRazor.Pages.Admin.Marketing;

public class EmailCampaignModel
{
    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}