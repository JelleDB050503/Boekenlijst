using Boekenlijst.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Boekenlijst.Services;

public class BoekenLijstFileProcessor
{
    private readonly BoekenLijstContext _context;
    private readonly ILogger<BoekenLijstFileProcessor> _logger;

    public BoekenLijstFileProcessor(BoekenLijstContext context, ILogger<BoekenLijstFileProcessor> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessBoekenLijstFile(string filePath)
    {
        try
        {
            _logger.LogInformation($"Starting to process book list from: {filePath}");

            // Skip if data already exists
            if (_context.Boeks.Any())
            {
                _logger.LogInformation("Database already contains books, skipping seed.");
                return;
            }

            // Check if file exists
            if (!File.Exists(filePath))
            {
                _logger.LogError($"File not found: {filePath}");
                return;
            }

            _logger.LogInformation($"File found, starting to read...");

            // Create or get statuses
            var fysiek = _context.Statuses.FirstOrDefault(s => s.Naam == "Fysiek");
            if (fysiek == null)
            {
                _logger.LogInformation("Creating 'Fysiek' status");
                fysiek = new Status { Naam = "Fysiek" };
                _context.Statuses.Add(fysiek);
            }

            var ebook = _context.Statuses.FirstOrDefault(s => s.Naam == "E-boek");
            if (ebook == null)
            {
                _logger.LogInformation("Creating 'E-boek' status");
                ebook = new Status { Naam = "E-boek" };
                _context.Statuses.Add(ebook);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Statuses created/verified. Fysiek ID: {fysiek.Id}, E-boek ID: {ebook.Id}");

            // Dictionary to store auteurs by name
            var auteurs = new Dictionary<string, Auteur>();

            // Read file
            var lines = await File.ReadAllLinesAsync(filePath);
            _logger.LogInformation($"Read {lines.Length} lines from file");

            var boeken = new List<(string Auteur, string Titel, int Jaar, bool IsFysiek)>();
            bool isFysiekSection = true;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Skip empty lines and header
                if (string.IsNullOrEmpty(trimmed) || trimmed == "BOEKENLIJST")
                    continue;

                // Check for section headers
                if (trimmed == "Fysieke boeken")
                {
                    isFysiekSection = true;
                    _logger.LogInformation("Entering Fysieke boeken section");
                    continue;
                }

                if (trimmed == "E-boeken")
                {
                    isFysiekSection = false;
                    _logger.LogInformation("Entering E-boeken section");
                    continue;
                }

                // Parse book entry: "Auteur – Titel – Jaar" or "Auteur - Titel - Jaar"
                var match = Regex.Match(trimmed, @"^(.+?)\s*[–-]\s*(.+?)\s*[–-]\s*(\d{4})$");

                if (match.Success)
                {
                    var auteur = match.Groups[1].Value.Trim();
                    var titel = match.Groups[2].Value.Trim();
                    var jaar = int.Parse(match.Groups[3].Value);

                    boeken.Add((auteur, titel, jaar, isFysiekSection));
                }
            }

            _logger.LogInformation($"Parsed {boeken.Count} books from file");

            // Add boeken to database
            int bookCount = 0;
            foreach (var (auteurNaam, titel, jaar, isFysiek) in boeken)
            {
                // Get or create auteur
                if (!auteurs.ContainsKey(auteurNaam))
                {
                    var parts = auteurNaam.Split(',');
                    var auteur = new Auteur
                    {
                        Naam = parts[0].Trim(),
                        Voornaam = parts.Length > 1 ? parts[1].Trim() : ""
                    };
                    _context.Auteurs.Add(auteur);
                    await _context.SaveChangesAsync();
                    auteurs[auteurNaam] = auteur;
                }

                var boek = new Boek
                {
                    Titel = titel,
                    AuteurId = auteurs[auteurNaam].Id,
                    Jaaruitgave = jaar,
                    StatusId = isFysiek ? fysiek.Id : ebook.Id
                };

                _context.Boeks.Add(boek);
                bookCount++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully added {bookCount} books to database");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing book list: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
