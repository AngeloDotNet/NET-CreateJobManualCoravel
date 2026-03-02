using Example3.Models;

namespace Example3.Services.Interfaces;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailRequest req);
    ValueTask<QueueItem> DequeueAsync(CancellationToken ct);
}