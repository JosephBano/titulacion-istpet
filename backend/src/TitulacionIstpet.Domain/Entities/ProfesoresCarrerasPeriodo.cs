using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ProfesoresCarrerasPeriodo
{
    public int IdProfesoresCarrerasPeriodos { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public string IdProfesor { get; set; } = null!;

    public int? IdCarrera { get; set; }

    public bool? EsActivo { get; set; }

    public bool? SonTodas { get; set; }

    public virtual Carrera? IdCarreraNavigation { get; set; }

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
