using Example1.Models;
using Example1.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Example1;

[ApiController]
[Route("api/[controller]")]
public class EmailController(IEmailQueue queue) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] EmailRequest req)
    {
        // validate...
        await queue.EnqueueAsync(req); // enqueued, returns immediately
        return Accepted(new { status = "queued" });
    }
}