using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosSuceso
{
    public int IdSuceso { get; set; }

    public string? IdAlumno { get; set; }

    public int? IdMatricula { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string Observacion { get; set; } = null!;

    public string? Usuario { get; set; }
}
