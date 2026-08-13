using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacCierresColectivosExclusione
{
    public int IdExclusion { get; set; }

    public int IdCierre { get; set; }

    public string IdProfesor { get; set; } = null!;

    public virtual VacCierresColectivo IdCierreNavigation { get; set; } = null!;
}
