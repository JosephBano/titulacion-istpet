using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienMotivoApertura
{
    public int IdMotivoApertura { get; set; }

    public string Nombre { get; set; } = null!;

    public string? DescripcionMotivoApertura { get; set; }

    public string? AccionesPrevias { get; set; }

    /// <summary>
    /// plantilla/formato opcional asociado al motivo
    /// </summary>
    public int? IdAdjuntosImagenes { get; set; }

    public bool EsActivo { get; set; }

    public virtual ICollection<BienCaso> BienCasos { get; set; } = new List<BienCaso>();

    public virtual ICollection<BienRequerimientosMotivoApertura> BienRequerimientosMotivoAperturas { get; set; } = new List<BienRequerimientosMotivoApertura>();

    public virtual AdjuntosImagene? IdAdjuntosImagenesNavigation { get; set; }
}
