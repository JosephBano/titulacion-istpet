using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HorarioProfesore
{
    public int IdHorario { get; set; }

    public int? IdAsignacion { get; set; }

    public int? IdHora { get; set; }

    public int? IdFecha { get; set; }

    public bool? Asiste { get; set; }

    public bool? Activo { get; set; }
}
