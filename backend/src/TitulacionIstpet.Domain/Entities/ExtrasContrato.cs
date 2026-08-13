using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ExtrasContrato
{
    public int IdExtraContratos { get; set; }

    public int IdContratos { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public DateOnly? FechaInicioextra { get; set; }

    public decimal? ValorExtra { get; set; }

    public string? Motivo { get; set; }

    public DateOnly? FechaFinalizacion { get; set; }

    public bool? Esactivo { get; set; }

    public string? UsuarioRegistra { get; set; }

    public virtual Contrato IdContratosNavigation { get; set; } = null!;
}
