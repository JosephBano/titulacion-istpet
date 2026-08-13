using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienResolucionesTribunale
{
    public int IdResolucionesTribunales { get; set; }

    public int IdPostulacionesBecas { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public string? Resolucion { get; set; }

    public decimal PorcentajeFinal { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<BienApoyoFinanciero> BienApoyoFinancieros { get; set; } = new List<BienApoyoFinanciero>();

    public virtual BienVotosTribunale? BienVotosTribunale { get; set; }

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual BienPostulacionesBeca IdPostulacionesBecasNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioRegistraNavigation { get; set; } = null!;
}
