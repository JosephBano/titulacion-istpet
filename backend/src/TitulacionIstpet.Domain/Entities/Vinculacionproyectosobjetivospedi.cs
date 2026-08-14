using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosobjetivospedi
{
    public int IdProyectoObjetivoPedi { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdObjetivoPedi { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacionobjetivospedi? IdObjetivoPediNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
