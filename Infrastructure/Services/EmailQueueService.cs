using System.Threading.Channels;
using Core.DTOs;
using Core.Interfaces;

namespace Infrastructure.Services;

public class EmailQueueService : IEmailQueueService
{
    private readonly Channel<EmailMessage> _queue;

    public EmailQueueService()
    {
        _queue = Channel.CreateUnbounded<EmailMessage>();
    }

    public void QueueEmail(EmailMessage message)
    {
        _queue.Writer.TryWrite(message);
    }

    public async Task<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}