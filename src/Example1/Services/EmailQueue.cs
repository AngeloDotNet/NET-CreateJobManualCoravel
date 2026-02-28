using System.Threading.Channels;
using Example1.Models;
using Example1.Services.Interfaces;

namespace Example1.Services;

public class EmailQueue : IEmailQueue, IDisposable
{
    private readonly Channel<EmailRequest> channel;

    public EmailQueue(int capacity = 1000)
    {
        // BoundedChannel per backpressure
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        channel = Channel.CreateBounded<EmailRequest>(options);
    }

    public async ValueTask EnqueueAsync(EmailRequest request) => await channel.Writer.WriteAsync(request);

    public ValueTask<EmailRequest> DequeueAsync(CancellationToken ct) => channel.Reader.ReadAsync(ct);

    public void Dispose() => channel.Writer.TryComplete();
}