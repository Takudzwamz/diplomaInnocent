using Core.DTOs;

namespace Core.Interfaces;

public interface IEmailQueueService
{
    void QueueEmail(EmailMessage message);
    Task<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken);
}