using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Detallemalla
{
    public int IdDetalleMalla { get; set; }

    public int IdMalla { get; set; }

    public int IdAsignatura { get; set; }

    public int IdNivel { get; set; }

    public int IdtipoAsignatura { get; set; }

    public string? Tipo { get; set; }

    public bool? Opcional { get; set; }

    public int? Creditos { get; set; }

    public int? Horas { get; set; }

    public bool? Anulada { get; set; }

    public int? HorasDocente { get; set; }

    public decimal? HorasPracticoExperimental { get; set; }

    public virtual Asignatura IdAsignaturaNavigation { get; set; } = null!;

    public virtual Malla IdMallaNavigation { get; set; } = null!;

    public virtual Curso IdNivelNavigation { get; set; } = null!;

    public virtual TiposAsignatura IdtipoAsignaturaNavigation { get; set; } = null!;

    public virtual ICollection<Prerequisito> Prerequisitos { get; set; } = new List<Prerequisito>();
}
