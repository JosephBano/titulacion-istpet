using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CondAlumnosVehiculo
{
    public int IdAsignacion { get; set; }

    public string IdAlumno { get; set; } = null!;

    public int IdVehiculo { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public string? IdProfesor { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool? Activa { get; set; }

    public string? Observacion { get; set; }
}
