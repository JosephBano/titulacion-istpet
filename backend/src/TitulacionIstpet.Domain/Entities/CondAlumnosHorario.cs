using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CondAlumnosHorario
{
    public int IdAsignacionHorario { get; set; }

    public int IdAsignacion { get; set; }

    public int IdFecha { get; set; }

    public int IdHora { get; set; }

    public bool? Asiste { get; set; }

    public bool? Activo { get; set; }

    public string? Observacion { get; set; }
}
