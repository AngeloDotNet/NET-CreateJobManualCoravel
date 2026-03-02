using System.Threading.Channels;
using Example3.Models;
using Example3.Services.Interfaces;

namespace Example3.Services;

public class EmailQueue : IEmailQueue, IDisposable
{
    private readonly Channel<QueueItem> channel;

    public EmailQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        channel = Channel.CreateBounded<QueueItem>(options);
    }

    public ValueTask EnqueueAsync(EmailRequest req) => channel.Writer.WriteAsync(new QueueItem { Request = req, Attempts = 0 });

    public ValueTask<QueueItem> DequeueAsync(CancellationToken ct) => channel.Reader.ReadAsync(ct);

    public void Dispose() => channel.Writer.TryComplete();

    // helper for requeue
    public bool TryRequeue(QueueItem item) => channel.Writer.TryWrite(item);
}