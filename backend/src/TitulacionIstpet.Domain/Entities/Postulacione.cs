using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Postulacione
{
    public int IdPostulaciones { get; set; }

    public int IdofertasLaborales { get; set; }

    public string IdAlumno { get; set; } = null!;

    public int IddocumentosAdjuntos { get; set; }

    public DateTime? FechaPostulacion { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual DocumentosAdjunto IddocumentosAdjuntosNavigation { get; set; } = null!;

    public virtual OfertasLaborale IdofertasLaboralesNavigation { get; set; } = null!;
}
