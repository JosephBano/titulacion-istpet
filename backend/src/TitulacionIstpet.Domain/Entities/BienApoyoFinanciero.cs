using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienApoyoFinanciero
{
    public int IdApoyoFinanciero { get; set; }

    public int IdResponsable { get; set; }

    public int IdResolucionesTribunales { get; set; }

    public int IdMatricula { get; set; }

    public string? Observacion { get; set; }

    public bool EsAceptada { get; set; }

    public DateOnly? FechaAceptacion { get; set; }

    public bool EsActivo { get; set; }

    public virtual Matricula IdMatriculaNavigation { get; set; } = null!;

    public virtual BienResolucionesTribunale IdResolucionesTribunalesNavigation { get; set; } = null!;

    public virtual Usuario IdResponsableNavigation { get; set; } = null!;
}
