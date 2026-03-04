using Example4.Models;

namespace Example4.Services.Interfaces;

public interface IMailKitSender
{
    Task SendAsync(EmailRequest req, CancellationToken ct = default);
}