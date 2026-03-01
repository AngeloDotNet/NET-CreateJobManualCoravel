using Example2.Models;

namespace Example2.Services;

public interface IMailKitSender
{
    Task SendAsync(EmailRequest req, CancellationToken ct = default);
}