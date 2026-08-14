using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FechasPagosCuota
{
    public int IdFecha { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public int IdModalidad { get; set; }

    public int IdCarrera { get; set; }

    public string? CodigoReferencia { get; set; }

    public DateOnly? Fecha { get; set; }

    public decimal? ValorCuota { get; set; }

    public bool? GeneraFecha { get; set; }

    public bool? Activo { get; set; }

    public virtual Carrera IdCarreraNavigation { get; set; } = null!;

    public virtual Modalidade IdModalidadNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
}
