using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly IEmailQueueService _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailSenderBackgroundService> _logger;

    public EmailSenderBackgroundService(IEmailQueueService queue, IServiceScopeFactory scopeFactory, ILogger<EmailSenderBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Sender Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for a new email message in the queue
                var message = await _queue.DequeueEmailAsync(stoppingToken);
                _logger.LogInformation("New email campaign dequeued. Preparing to send to all customers.");

                // Create a new dependency scope to resolve services
                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                // Get all users who are not admins
                var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
                var adminUserIds = adminUsers.Select(u => u.Id).ToHashSet();
                var customers = userManager.Users.Where(u => !adminUserIds.Contains(u.Id)).ToList();

                _logger.LogInformation("Sending campaign to {CustomerCount} customers.", customers.Count);
                foreach (var customer in customers)
                {
                    await emailSender.SendEmailAsync(customer.Email, message.Subject, message.Body);
                    // Add a small delay to avoid overwhelming the email provider's API limits
                    await Task.Delay(200, stoppingToken); 
                }
                _logger.LogInformation("Email campaign sending complete.");
            }
            catch (OperationCanceledException)
            {
                // This is expected when the application is shutting down.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the Email Sender Background Service.");
            }
        }
    }
}