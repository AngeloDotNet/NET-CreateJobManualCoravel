using Example4.Models;

namespace Example4.Services.Interfaces;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailRequest req);
    ValueTask<QueueItem> DequeueAsync(CancellationToken ct);
    bool TryRequeue(QueueItem item);
}