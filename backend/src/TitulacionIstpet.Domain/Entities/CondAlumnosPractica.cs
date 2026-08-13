using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CondAlumnosPractica
{
    public int IdPractica { get; set; }

    public string Idalumno { get; set; } = null!;

    public int Idvehiculo { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string IdPeriodo { get; set; } = null!;

    public string? Dia { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly? HoraSalida { get; set; }

    public TimeOnly? HoraLlegada { get; set; }

    public TimeOnly? Tiempo { get; set; }

    public bool? Ensalida { get; set; }

    public bool? Verificada { get; set; }

    public string? UserAsigna { get; set; }

    public string? UserLlegada { get; set; }

    public bool? Cancelado { get; set; }

    public string? Observaciones { get; set; }
}
