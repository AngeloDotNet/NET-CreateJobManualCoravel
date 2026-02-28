using Example1.Models;
using Example1.Services;
using MailKit.Net.Smtp;
using MimeKit;

namespace Example1.HostedServices;

public class EmailSenderHostedService(EmailQueue queue, IServiceProvider provider, ILogger<EmailSenderHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email sender started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            EmailRequest req;

            try
            {
                req = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                // use a scope to resolve scoped services like DbContext if needed
                using var scope = provider.CreateScope();

                // Example: you can resolve AppDbContext here and log the send to DB
                // var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // db.EmailLogs.Add(new EmailLog {...});
                // await db.SaveChangesAsync();

                // Send email using MailKit
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse("no-reply@example.com"));
                message.To.Add(MailboxAddress.Parse(req.To));
                message.Subject = req.Subject;
                message.Body = new TextPart("html") { Text = req.Body };

                using var client = new SmtpClient();

                await client.ConnectAsync("smtp.example.com", 587, MailKit.Security.SecureSocketOptions.StartTls, stoppingToken);
                await client.AuthenticateAsync("smtp-user", "smtp-password", stoppingToken);

                await client.SendAsync(message, stoppingToken);
                await client.DisconnectAsync(true, stoppingToken);

                logger.LogInformation("Email sent to {To}", req.To);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing email to {To}", req.To);
                // implement retry/backoff or dead-letter as needed
            }
        }

        logger.LogInformation("Email sender stopped.");
    }
}