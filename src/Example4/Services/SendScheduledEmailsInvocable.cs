using System.Text.Json;
using Coravel.Invocable;
using Coravel.Queuing.Interfaces;
using Example4.Data;
using Example4.Entities;
using Example4.Models;
using Example4.Services.Interfaces;

namespace Example4.Services;

public class SendScheduledEmailsInvocable(AppDbContext db, IQueue queue, ILogger<SendScheduledEmailsInvocable> logger, IServiceProvider serviceProvider) : IInvocable
{
    // Coravel calls Invoke() when scheduled
    public async Task Invoke()
    {
        var now = DateTime.UtcNow;
        var toRun = db.ScheduledEmails.Where(s => s.Enabled && s.NextRunUtc <= now).ToList();

        if (toRun.Count == 0)
        {
            logger.LogDebug("No scheduled emails to run at {Now}", now);
            return;
        }

        foreach (var scheduled in toRun)
        {
            var req = new EmailRequest(scheduled.To, scheduled.Subject, scheduled.Body);

            queue.QueueAsyncTask(() =>
            {
                return SendEmailAsync(serviceProvider, req);
            });

            scheduled.NextRunUtc = scheduled.NextRunUtc.Add(scheduled.RepeatInterval);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Scheduled emails enqueued: {Count}", toRun.Count);
    }

    private static async Task SendEmailAsync(IServiceProvider serviceProvider, EmailRequest req)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var db = scopedProvider.GetRequiredService<AppDbContext>();
        var sender = scopedProvider.GetRequiredService<IMailKitSender>();

        try
        {
            await sender.SendAsync(req);

            db.EmailLogs.Add(new EmailLog
            {
                To = req.To,
                Subject = req.Subject,
                Body = req.Body,
                SentAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            db.DeadLetters.Add(new DeadLetter
            {
                Payload = JsonSerializer.Serialize(req),
                Attempts = 1,
                LastError = ex.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}