using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EspeciesExtra
{
    public int Idmatricula { get; set; }

    public int Idespecie { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public decimal Valor { get; set; }

    public DateOnly FechaLimitePago { get; set; }

    public string? Observacion { get; set; }

    public bool Obligatoria { get; set; }

    public decimal Pagado { get; set; }

    public bool Extra { get; set; }

    public string Tipo { get; set; } = null!;
}
