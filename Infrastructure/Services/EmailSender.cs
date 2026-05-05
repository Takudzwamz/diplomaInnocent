using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly ISiteSettingsService _siteSettings; // 1. Add this
    private readonly ILogger<EmailSender> _logger; // 2. Add this
    private readonly string _storeName = string.Empty;
    private readonly string _adminEmail = string.Empty;
    private readonly string _apiKey = string.Empty;

    public EmailSender(ISiteSettingsService siteSettings, ILogger<EmailSender> logger) // 3. Change constructor
    {
        _siteSettings = siteSettings;
        _logger = logger;
        
        var settings = _siteSettings.GetSettingsAsync().Result;
        _apiKey = settings.GetValueOrDefault("SendGrid_ApiKey") ?? string.Empty;
        _storeName = settings.GetValueOrDefault("StoreName", "Devs Store") ?? "Devs Store";
        _adminEmail = settings.GetValueOrDefault("AdminNotificationEmail", "sputnikdevs@sputnikdevs.com") ?? "sputnikdevs@sputnikdevs.com";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("--- EMAIL ERROR: SendGrid_ApiKey is not configured in Site Settings. Email not sent. ---");
            return;
        }

        var client = new SendGridClient(_apiKey);
        
        // 4. Use the dynamic settings
        var from = new EmailAddress(_adminEmail, _storeName); 
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
        
        _logger.LogInformation("--- Sending email to {ToEmail} via SendGrid... ---", toEmail);
        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("--- SUCCESS: Email to {ToEmail} queued for delivery! ---", toEmail);
        }
        else
        {
            _logger.LogError("--- ERROR: SendGrid failed to send email. Status Code: {StatusCode} ---", response.StatusCode);
            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogError("Response Body: {ResponseBody}", responseBody);
        }
    }
}