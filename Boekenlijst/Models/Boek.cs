using System;
using System.Collections.Generic;

namespace Boekenlijst.Models;

public partial class Boek
{
    public int Id { get; set; }

    public string Titel { get; set; } = null!;

    public int AuteurId { get; set; }

    public int? Jaaruitgave { get; set; }

    public string? Reeks { get; set; }

    public int? ReeksVolgorde { get; set; }

    public int StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Auteur Auteur { get; set; } = null!;

    public virtual Rating? Rating { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
