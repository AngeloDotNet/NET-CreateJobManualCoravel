using Example3.Models;

namespace Example3.Services.Interfaces;

public interface IMailKitSender
{
    Task SendAsync(EmailRequest req, CancellationToken ct = default);
}