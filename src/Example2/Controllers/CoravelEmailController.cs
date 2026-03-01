using Coravel.Queuing.Interfaces;
using Example2.Data;
using Example2.Entities;
using Example2.Models;
using Example2.Services;
using Microsoft.AspNetCore.Mvc;

namespace Example2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoravelEmailController(IQueue queue) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] EmailRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.To))
        {
            return BadRequest("To is required");
        }

        // QueueAsyncTask returns Guid, not Task, so don't await it
        queue.QueueAsyncTask(async () =>
        {
            var provider = HttpContext.RequestServices;
            var sender = provider.GetRequiredService<IMailKitSender>();
            var db = provider.GetService<AppDbContext>(); // optional

            await sender.SendAsync(req);

            if (db is not null)
            {
                db.Messages.Add(new Message
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