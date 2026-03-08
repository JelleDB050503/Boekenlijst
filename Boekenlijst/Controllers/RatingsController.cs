using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boekenlijst.Models;

namespace Boekenlijst.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RatingsController : ControllerBase
{
    private readonly BoekenLijstContext _context;

    public RatingsController(BoekenLijstContext context)
    {
        _context = context;
    }

    // GET: api/Ratings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rating>>> GetRatings()
    {
        return await _context.Ratings
            .Include(r => r.Boek)
            .ToListAsync();
    }

    // GET: api/Ratings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Rating>> GetRating(int id)
    {
        var rating = await _context.Ratings
            .Include(r => r.Boek)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rating == null)
        {
            return NotFound();
        }

        return rating;
    }

    // PUT: api/Ratings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutRating(int id, Rating rating)
    {
        if (id != rating.Id)
        {
            return BadRequest();
        }

        rating.UpdatedAt = DateTime.UtcNow;
        _context.Entry(rating).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RatingExists(id))
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

    // POST: api/Ratings
    [HttpPost]
    public async Task<ActionResult<Rating>> PostRating(Rating rating)
    {
        rating.CreatedAt = DateTime.UtcNow;
        rating.UpdatedAt = DateTime.UtcNow;
        rating.Datum = DateTime.UtcNow;
        
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRating), new { id = rating.Id }, rating);
    }

    // DELETE: api/Ratings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRating(int id)
    {
        var rating = await _context.Ratings.FindAsync(id);
        if (rating == null)
        {
            return NotFound();
        }

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RatingExists(int id)
    {
        return _context.Ratings.Any(e => e.Id == id);
    }
}
