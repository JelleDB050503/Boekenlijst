using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boekenlijst.Models;

namespace Boekenlijst.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusesController : ControllerBase
{
    private readonly BoekenLijstContext _context;

    public StatusesController(BoekenLijstContext context)
    {
        _context = context;
    }

    // GET: api/Statuses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
    {
        return await _context.Statuses.ToListAsync();
    }

    // GET: api/Statuses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Status>> GetStatus(int id)
    {
        var status = await _context.Statuses
            .Include(s => s.Boeks)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (status == null)
        {
            return NotFound();
        }

        return status;
    }

    // PUT: api/Statuses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutStatus(int id, Status status)
    {
        if (id != status.Id)
        {
            return BadRequest();
        }

        _context.Entry(status).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StatusExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Statuses
    [HttpPost]
    public async Task<ActionResult<Status>> PostStatus(Status status)
    {
        status.CreatedAt = DateTime.UtcNow;
        
        _context.Statuses.Add(status);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStatus), new { id = status.Id }, status);
    }

    // DELETE: api/Statuses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStatus(int id)
    {
        var status = await _context.Statuses.FindAsync(id);
        if (status == null)
        {
            return NotFound();
        }

        _context.Statuses.Remove(status);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool StatusExists(int id)
    {
        return _context.Statuses.Any(e => e.Id == id);
    }
}
