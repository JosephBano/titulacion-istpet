using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienParentezcosAlumno
{
    public int IdParentezcoAlumno { get; set; }

    public int IdParentezco { get; set; }

    public string IdAlumno { get; set; } = null!;

    public int? Idpaises { get; set; }

    public int? IdNivelInstruccion { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal? IngresoMensualPromedio { get; set; }

    public bool EsResponsableEconomico { get; set; }

    public bool ContactoEmergencia { get; set; }

    public string? NumeroContactoEmergencia { get; set; }

    public bool? EsCarga { get; set; }

    public bool? TieneDiscapacidad { get; set; }

    public bool EsActivo { get; set; }

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual BienNivelInstruccion? IdNivelInstruccionNavigation { get; set; }

    public virtual BienParentesco IdParentezcoNavigation { get; set; } = null!;

    public virtual Paise? IdpaisesNavigation { get; set; }
}
