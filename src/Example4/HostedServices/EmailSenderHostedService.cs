using System.Text.Json;
using Example4.Data;
using Example4.Entities;
using Example4.Models;
using Example4.Services;
using Example4.Services.Interfaces;

namespace Example4.HostedServices;

public class EmailSenderHostedService(EmailQueue queue, IServiceProvider provider, ILogger<EmailSenderHostedService> logger) : BackgroundService
{
    private readonly int maxRetries = 5;
    private readonly TimeSpan baseDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailSenderHostedService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            QueueItem item;

            try
            {
                item = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = provider.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IMailKitSender>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Send
                await sender.SendAsync(item.Request, stoppingToken);

                // Log success
                db.EmailLogs.Add(new EmailLog
                {
                    To = item.Request.To,
                    Subject = item.Request.Subject,
                    Body = item.Request.Body,
                    SentAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                item.Attempts++;
                logger.LogWarning(ex, "Failed sending email to {To} (attempt {Attempt})", item.Request.To, item.Attempts);

                if (item.Attempts >= maxRetries)
                {
                    // dead-letter persist
                    try
                    {
                        using var scope = provider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        db.DeadLetters.Add(new DeadLetter
                        {
                            Payload = JsonSerializer.Serialize(item.Request),
                            Attempts = item.Attempts,
                            LastError = ex.ToString(),
                            CreatedAt = DateTime.UtcNow
                        });
                        await db.SaveChangesAsync(stoppingToken);
                        logger.LogError("Moved to dead-letter: {To}", item.Request.To);
                    }
                    catch (Exception dbEx)
                    {
                        logger.LogError(dbEx, "Failed to persist dead-letter for {To}", item.Request.To);
                        // as a last resort si potrebbe scrivere su file
                    }
                }
                else
                {
                    // exponential backoff before requeueing
                    var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, item.Attempts - 1));
                    logger.LogInformation("Delaying {Delay} before retry {Attempt} for {To}", delay, item.Attempts, item.Request.To);

                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                        var requeued = queue.TryRequeue(item);

                        if (!requeued)
                        {
                            logger.LogWarning("Requeue failed (channel full), persisting as dead-letter temporarily.");

                            using var scope = provider.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                            db.DeadLetters.Add(new DeadLetter
                            {
                                Payload = JsonSerializer.Serialize(item.Request),
                                Attempts = item.Attempts,
                                LastError = ex.ToString(),
                                CreatedAt = DateTime.UtcNow
                            });

                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                    catch (OperationCanceledException) { /* shutting down */ }
                }
            }
        }

        logger.LogInformation("EmailSenderHostedService stopping.");
    }
}