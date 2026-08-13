using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Plantillasparametro
{
    public int IdParametro { get; set; }

    public int? IdPlantilla { get; set; }

    public string? Parametro { get; set; }

    public decimal? X { get; set; }

    public decimal? Y { get; set; }

    public decimal? FontSize { get; set; }

    public string? TextAlign { get; set; }

    public string? Width { get; set; }

    public string? FontFamily { get; set; }

    public virtual Plantilla? IdPlantillaNavigation { get; set; }
}
