using Example3.Models;
using Example3.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Example3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelEmailController(IEmailQueue queue) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] EmailRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.To))
        {
            return BadRequest("To is required");
        }

        await queue.EnqueueAsync(req);
        return Accepted(new { status = "queued" });
    }
}