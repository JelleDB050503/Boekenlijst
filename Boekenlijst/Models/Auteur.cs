using System;
using System.Collections.Generic;

namespace Boekenlijst.Models;

public partial class Auteur
{
    public int Id { get; set; }

    public string Voornaam { get; set; } = null!;

    public string Naam { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Boek> Boeks { get; set; } = new List<Boek>();
}
