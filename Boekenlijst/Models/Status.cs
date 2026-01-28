using System;
using System.Collections.Generic;

namespace Boekenlijst.Models;

public partial class Status
{
    public int Id { get; set; }

    public string Naam { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Boek> Boeks { get; set; } = new List<Boek>();
}
