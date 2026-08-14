using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectospresupuesto
{
    public int IdProyectoPresupuesto { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? Empresa { get; set; }

    public decimal? Cantidad { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
