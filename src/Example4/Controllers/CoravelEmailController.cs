using Coravel.Queuing.Interfaces;
using Example4.Data;
using Example4.Entities;
using Example4.Models;
using Example4.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Example4.Controllers;

// Coravel (Queue + Scheduler + IInvocable) for ready-to-use APIs and fluent scheduling.
[ApiController]
[Route("api/[controller]")]
public class CoravelEmailController(IQueue queue) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] EmailRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.To))
        {
            return BadRequest("To required");
        }

        queue.QueueAsyncTask(async () =>
        {
            var provider = HttpContext.RequestServices;
            var sender = provider.GetRequiredService<IMailKitSender>();
            var db = provider.GetService<AppDbContext>();

            await sender.SendAsync(req);

            if (db is not null)
            {
                db.EmailLogs.Add(new EmailLog
                {
                    To = req.To,
                    Subject = req.Subject,
                    Body = req.Body,
                    SentAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        });

        return Accepted(new { status = "queued" });
    }
}