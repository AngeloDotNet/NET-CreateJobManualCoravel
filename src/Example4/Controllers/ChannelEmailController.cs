using Example4.Models;
using Example4.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Example4.Controllers;

// Channel + BackgroundService for full control over retry/backoff/dead-letter.
[ApiController]
[Route("api/[controller]")]
public class ChannelEmailController(IEmailQueue queue) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] EmailRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.To))
        {
            return BadRequest("To required");
        }

        await queue.EnqueueAsync(req);
        return Accepted(new { status = "queued" });
    }
}