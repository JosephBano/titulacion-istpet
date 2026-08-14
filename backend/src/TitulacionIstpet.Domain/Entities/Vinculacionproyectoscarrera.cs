using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectoscarrera
{
    public int IdProyectoCarrera { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdCarrera { get; set; }

    public bool? EsPrincipal { get; set; }

    public bool? Activo { get; set; }

    public virtual Carrera? IdCarreraNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
