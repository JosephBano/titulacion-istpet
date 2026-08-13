using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectoscarrerasdetalle
{
    public int IdProyectoCarrera { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdTipoPoblacion { get; set; }

    public string? Poblacion { get; set; }

    public string? Descripcion { get; set; }

    public int? Orden { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }

    public virtual Vinculaciontipospoblacione? IdTipoPoblacionNavigation { get; set; }
}
