using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boekenlijst.Models;

namespace Boekenlijst.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BoekenController : ControllerBase
{
    private readonly BoekenLijstContext _context;

    public BoekenController(BoekenLijstContext context)
    {
        _context = context;
    }

    // GET: api/Boeken
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Boek>>> GetBoeken()
    {
        return await _context.Boeks
            .Include(b => b.Auteur)
            .Include(b => b.Status)
            .Include(b => b.Rating)
            .Include(b => b.Genres)
            .ToListAsync();
    }

    // GET: api/Boeken/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Boek>> GetBoek(int id)
    {
        var boek = await _context.Boeks
            .Include(b => b.Auteur)
            .Include(b => b.Status)
            .Include(b => b.Rating)
            .Include(b => b.Genres)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (boek == null)
        {
            return NotFound();
        }

        return boek;
    }

    // PUT: api/Boeken/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBoek(int id, Boek boek)
    {
        if (id != boek.Id)
        {
            return BadRequest();
        }

        boek.UpdatedAt = DateTime.UtcNow;
        _context.Entry(boek).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BoekExists(id))
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

    // POST: api/Boeken
    [HttpPost]
    public async Task<ActionResult<Boek>> PostBoek(Boek boek)
    {
        boek.CreatedAt = DateTime.UtcNow;
        boek.UpdatedAt = DateTime.UtcNow;
        
        _context.Boeks.Add(boek);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBoek), new { id = boek.Id }, boek);
    }

    // DELETE: api/Boeken/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoek(int id)
    {
        var boek = await _context.Boeks.FindAsync(id);
        if (boek == null)
        {
            return NotFound();
        }

        _context.Boeks.Remove(boek);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BoekExists(int id)
    {
        return _context.Boeks.Any(e => e.Id == id);
    }
}
