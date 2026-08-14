using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosentidade
{
    public int IdProyectoEntidad { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? Entidad { get; set; }

    public string? TipoEntidad { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
