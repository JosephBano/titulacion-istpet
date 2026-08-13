using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosobjetivosoportunidade
{
    public int IdProyectObjetivoOportunidad { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdObjetivoOportunidad { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionobjetivosoportunidade? IdObjetivoOportunidadNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
