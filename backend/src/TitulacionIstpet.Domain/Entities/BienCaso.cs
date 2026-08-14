using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienCaso
{
    public int IdCaso { get; set; }

    public string CodigoCaso { get; set; } = null!;

    public int IdMotivoApertura { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public int IdUsuarioResponsableCaso { get; set; }

    public string? AccionesRealizadas { get; set; }

    public string? ConclusionesCierre { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<BienCasoDesarrollo> BienCasoDesarrollos { get; set; } = [];

    public virtual ICollection<BienCasoRequerimiento> BienCasoRequerimientos { get; set; } = [];

    public virtual ICollection<BienUsuarioCaso> BienUsuarioCasos { get; set; } = [];

    public virtual BienMotivoApertura IdMotivoAperturaNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioResponsableCasoNavigation { get; set; } = null!;
}
