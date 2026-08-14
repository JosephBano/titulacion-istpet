using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosInscripcionesIngle
{
    public string IdAlumno { get; set; } = null!;

    public string IdPeriodo { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public string? UserInscripcion { get; set; }

    public decimal? Puntaje { get; set; }

    public int? IdAsignatura { get; set; }

    public int? IdMalla { get; set; }

    public string? Observacion { get; set; }
}
