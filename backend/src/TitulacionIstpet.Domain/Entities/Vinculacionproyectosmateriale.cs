using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosmateriale
{
    public int IdProyectosMateriales { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? Material { get; set; }

    public int? Cantidad { get; set; }

    public decimal? Valor { get; set; }

    public decimal? Total { get; set; }

    public int Instituto { get; set; }

    public int Autogestion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
