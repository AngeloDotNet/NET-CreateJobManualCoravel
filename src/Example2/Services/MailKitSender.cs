using Example2.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Example2.Services;

public class MailKitSender(IConfiguration config, ILogger<MailKitSender> logger) : IMailKitSender
{
    public async Task SendAsync(EmailRequest req, CancellationToken ct = default)
    {
        var message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(config["Mail:From"] ?? "no-reply@example.com"));
        message.To.Add(MailboxAddress.Parse(req.To));
        message.Subject = req.Subject;
        message.Body = new TextPart("html") { Text = req.Body };

        using var client = new SmtpClient();

        var host = config["Mail:SmtpHost"] ?? "smtp.example.com";
        var port = int.Parse(config["Mail:SmtpPort"] ?? "587");

        var user = config["Mail:SmtpUser"] ?? "";
        var pass = config["Mail:SmtpPass"] ?? "";

        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);

        if (!string.IsNullOrEmpty(user))
        {
            await client.AuthenticateAsync(user, pass, ct);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Sent email to {To}", req.To);
    }
}