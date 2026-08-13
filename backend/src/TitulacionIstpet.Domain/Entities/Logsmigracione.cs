using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Logsmigracione
{
    public int IdLog { get; set; }

    public string? Status { get; set; }

    public DateTime? Fecha { get; set; }
}
