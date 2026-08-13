using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacCierresColectivo
{
    public int IdCierre { get; set; }

    public string Nombre { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public decimal DiasDescuento { get; set; }

    public int FinesSemanaIncluidos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int RegistradoPorId { get; set; }

    public virtual ICollection<VacCierresColectivosExclusione> VacCierresColectivosExclusiones { get; set; } = new List<VacCierresColectivosExclusione>();
}
