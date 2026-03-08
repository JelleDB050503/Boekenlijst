using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boekenlijst.Models;

namespace Boekenlijst.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GenresController : ControllerBase
{
    private readonly BoekenLijstContext _context;

    public GenresController(BoekenLijstContext context)
    {
        _context = context;
    }

    // GET: api/Genres
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
    {
        return await _context.Genres.ToListAsync();
    }

    // GET: api/Genres/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Genre>> GetGenre(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.Boeks)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        return genre;
    }

    // PUT: api/Genres/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutGenre(int id, Genre genre)
    {
        if (id != genre.Id)
        {
            return BadRequest();
        }

        _context.Entry(genre).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GenreExists(id))
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

    // POST: api/Genres
    [HttpPost]
    public async Task<ActionResult<Genre>> PostGenre(Genre genre)
    {
        genre.CreatedAt = DateTime.UtcNow;
        
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
    }

    // DELETE: api/Genres/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null)
        {
            return NotFound();
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool GenreExists(int id)
    {
        return _context.Genres.Any(e => e.Id == id);
    }
}
