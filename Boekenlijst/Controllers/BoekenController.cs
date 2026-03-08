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
    public async Task<ActionResult<IEnumerable<BoekListItemDto>>> GetBoeken()
    {
        return await _context.Boeks
            .Include(b => b.Auteur)
            .Include(b => b.Status)
            .Include(b => b.Rating)
            .Include(b => b.Genres)
            .Select(b => new BoekListItemDto(
                b.Id,
                b.Titel,
                b.AuteurId,
                b.Auteur.Voornaam,
                b.Auteur.Naam,
                b.Jaaruitgave,
                b.Reeks,
                b.ReeksVolgorde,
                b.StatusId,
                b.Status.Naam,
                b.Rating != null ? b.Rating.Waarde : null,
                b.Genres.Select(g => g.Naam).ToList()
            ))
            .ToListAsync();
    }

    // GET: api/Boeken/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BoekListItemDto>> GetBoek(int id)
    {
        var boek = await _context.Boeks
            .Include(b => b.Auteur)
            .Include(b => b.Status)
            .Include(b => b.Rating)
            .Include(b => b.Genres)
            .Where(b => b.Id == id)
            .Select(b => new BoekListItemDto(
                b.Id,
                b.Titel,
                b.AuteurId,
                b.Auteur.Voornaam,
                b.Auteur.Naam,
                b.Jaaruitgave,
                b.Reeks,
                b.ReeksVolgorde,
                b.StatusId,
                b.Status.Naam,
                b.Rating != null ? b.Rating.Waarde : null,
                b.Genres.Select(g => g.Naam).ToList()
            ))
            .FirstOrDefaultAsync();

        if (boek == null)
        {
            return NotFound();
        }

        return boek;
    }

    // PUT: api/Boeken/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBoek(int id, UpdateBoekRequest request)
    {
        var boek = await _context.Boeks.FindAsync(id);
        if (boek == null)
        {
            return NotFound();
        }

        boek.Titel = request.Titel;
        boek.AuteurId = request.AuteurId;
        boek.Jaaruitgave = request.Jaaruitgave;
        boek.Reeks = request.Reeks;
        boek.ReeksVolgorde = request.ReeksVolgorde;
        boek.StatusId = request.StatusId;
        boek.UpdatedAt = DateTime.UtcNow;

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

    // PUT: api/Boeken/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> PutBoekStatus(int id, UpdateBoekStatusRequest request)
    {
        var boek = await _context.Boeks.FindAsync(id);
        if (boek == null)
        {
            return NotFound();
        }

        var statusExists = await _context.Statuses.AnyAsync(s => s.Id == request.StatusId);
        if (!statusExists)
        {
            return BadRequest("Invalid status id.");
        }

        boek.StatusId = request.StatusId;
        boek.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Boeken
    [HttpPost]
    public async Task<ActionResult<BoekListItemDto>> PostBoek(CreateBoekRequest request)
    {
        var boek = new Boek
        {
            Titel = request.Titel,
            AuteurId = request.AuteurId,
            Jaaruitgave = request.Jaaruitgave,
            Reeks = request.Reeks,
            ReeksVolgorde = request.ReeksVolgorde,
            StatusId = request.StatusId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boeks.Add(boek);
        await _context.SaveChangesAsync();

        var createdBook = await _context.Boeks
            .Include(b => b.Auteur)
            .Include(b => b.Status)
            .Include(b => b.Rating)
            .Include(b => b.Genres)
            .Where(b => b.Id == boek.Id)
            .Select(b => new BoekListItemDto(
                b.Id,
                b.Titel,
                b.AuteurId,
                b.Auteur.Voornaam,
                b.Auteur.Naam,
                b.Jaaruitgave,
                b.Reeks,
                b.ReeksVolgorde,
                b.StatusId,
                b.Status.Naam,
                b.Rating != null ? b.Rating.Waarde : null,
                b.Genres.Select(g => g.Naam).ToList()
            ))
            .FirstAsync();

        return CreatedAtAction(nameof(GetBoek), new { id = boek.Id }, createdBook);
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

    public record BoekListItemDto(
        int Id,
        string Titel,
        int AuteurId,
        string AuteurVoornaam,
        string AuteurNaam,
        int? Jaaruitgave,
        string? Reeks,
        int? ReeksVolgorde,
        int StatusId,
        string StatusNaam,
        int? Rating,
        List<string> Genres
    );

    public record CreateBoekRequest(
        string Titel,
        int AuteurId,
        int? Jaaruitgave,
        string? Reeks,
        int? ReeksVolgorde,
        int StatusId
    );

    public record UpdateBoekRequest(
        string Titel,
        int AuteurId,
        int? Jaaruitgave,
        string? Reeks,
        int? ReeksVolgorde,
        int StatusId
    );

    public record UpdateBoekStatusRequest(int StatusId);
}
