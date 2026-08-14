using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienPostulacionRequisitosBeca
{
    public int IdPostulacionBecaDocumentos { get; set; }

    public int IdParametroRequisitoBeca { get; set; }

    public string EstadoDocumento { get; set; } = null!;

    public string? ObservacionBienestar { get; set; }

    public DateTime? FechaValidacionBienestar { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool EsActivo { get; set; }

    public int IdPostulacionesBecas { get; set; }

    public int? IdUsuarioBienestar { get; set; }

    public bool? RequisitoBool { get; set; }

    public int? RequisitoAdjunto { get; set; }

    public virtual BienParametroRequisitoBeca IdParametroRequisitoBecaNavigation { get; set; } = null!;

    public virtual BienPostulacionesBeca IdPostulacionesBecasNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioBienestarNavigation { get; set; }

    public virtual AdjuntosImagene? RequisitoAdjuntoNavigation { get; set; }
}
