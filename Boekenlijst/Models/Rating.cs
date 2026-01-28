using System;
using System.Collections.Generic;

namespace Boekenlijst.Models;

public partial class Rating
{
    public int Id { get; set; }

    public int BoekId { get; set; }

    public int Waarde { get; set; }

    public string? Notities { get; set; }

    public DateTime? Datum { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Boek Boek { get; set; } = null!;
}
