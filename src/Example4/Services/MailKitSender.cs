using Example4.Models;
using Example4.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Example4.Services;

public class MailKitSender : IMailKitSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailKitSender> _logger;

    public MailKitSender(IConfiguration config, ILogger<MailKitSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(EmailRequest req, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config["Mail:From"] ?? "no-reply@example.com"));
        message.To.Add(MailboxAddress.Parse(req.To));
        message.Subject = req.Subject;
        message.Body = new TextPart("html") { Text = req.Body };

        using var client = new SmtpClient();
        var host = _config["Mail:SmtpHost"] ?? "smtp.example.com";
        var port = int.Parse(_config["Mail:SmtpPort"] ?? "587");
        var user = _config["Mail:SmtpUser"] ?? "";
        var pass = _config["Mail:SmtpPass"] ?? "";

        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls, ct);
        if (!string.IsNullOrEmpty(user))
        {
            await client.AuthenticateAsync(user, pass, ct);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Sent email to {To}", req.To);
    }
}