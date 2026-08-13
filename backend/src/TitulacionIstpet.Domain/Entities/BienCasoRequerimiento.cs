using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienCasoRequerimiento
{
    public int IdCasoRequerimiento { get; set; }

    public int IdCaso { get; set; }

    public int IdRequerimientoMotivoApertura { get; set; }

    /// <summary>
    /// snapshot del catalogo al abrir
    /// </summary>
    public string Detalle { get; set; } = null!;

    /// <summary>
    /// snapshot del catalogo al abrir
    /// </summary>
    public bool EsObligatorio { get; set; }

    public bool Cumplido { get; set; }

    public int? IdAdjuntosImagenes { get; set; }

    public int? IdUsuarioCumplio { get; set; }

    public DateTime? FechaCumplimiento { get; set; }

    public string? Observacion { get; set; }

    public virtual AdjuntosImagene? IdAdjuntosImagenesNavigation { get; set; }

    public virtual BienCaso IdCasoNavigation { get; set; } = null!;

    public virtual BienRequerimientosMotivoApertura IdRequerimientoMotivoAperturaNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioCumplioNavigation { get; set; }
}
