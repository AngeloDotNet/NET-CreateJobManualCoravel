using System.Text.Json;
using Example4.Data;
using Example4.Models;
using Example4.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Example4.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeadLetterController(AppDbContext db, IEmailQueue queue) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await db.DeadLetters.OrderByDescending(d => d.CreatedAt).Take(100).ToListAsync();
        return Ok(items);
    }

    [HttpPost("{id}/requeue")]
    public async Task<IActionResult> Requeue(int id)
    {
        var item = await db.DeadLetters.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        try
        {
            var req = JsonSerializer.Deserialize<EmailRequest>(item.Payload);

            if (req == null)
            {
                return BadRequest("Payload invalid");
            }

            await queue.EnqueueAsync(req);

            // optionally remove the dead-letter record
            db.DeadLetters.Remove(item);
            await db.SaveChangesAsync();

            return Ok(new { status = "requeued" });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.ToString());
        }
    }
}